using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Text;
using BaseApp;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.Utils;
using Sbor.CommonControls;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Tablepart;
using System.Linq;
using Sbor.Logic;
using ValueType = Sbor.DbEnums.ValueType;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;
using Sbor.Interfaces;

// ReSharper disable CheckNamespace
namespace Sbor.Document
// ReSharper restore CheckNamespace
{
    public partial class ActivityOfSBP : ISubDocSGE, IColumnFactoryForDenormalizedTablepart, IClarificationDoc, IDocStatusTerminate, IAddNeed, IPpoVerDoc
    {
        private SystemGoalElement _mainSystemGoalElement;

        private List<ActivityOfSBP_Activity> tpActivity;
        private List<ActivityOfSBP_Activity_Value> tpActivity_Value;
        private List<ActivityOfSBP_ActivityResourceMaintenance> tpActivityResourceMaintenance;
        private List<ActivityOfSBP_ActivityResourceMaintenance_Value> tpActivityResourceMaintenance_Value;
        private List<ActivityOfSBP_GoalIndicator> tpGoalIndicator;
        private List<ActivityOfSBP_GoalIndicator_Value> tpGoalIndicator_Value;
        private List<ActivityOfSBP_IndicatorQualityActivity> tpIndicatorQualityActivity;
        private List<ActivityOfSBP_IndicatorQualityActivity_Value> tpIndicatorQualityActivity_Value;
        private List<ActivityOfSBP_ResourceMaintenance> tpResourceMaintenance;
        private List<ActivityOfSBP_ResourceMaintenance_Value> tpResourceMaintenance_Value;
        private List<ActivityOfSBP_SystemGoalElement> tpSystemGoalElement;
        private List<ActivityOfSBP_ActivityDemandAndCapacity> tpActivityDemandAndCapacity;
        private List<ActivityOfSBP_ActivityDemandAndCapacity_Value> tpActivityDemandAndCapacity_Value;

        private int[] _arrIdParent;
        private int[] _arrRegisters;
        
        public override string ToString()
        {
            return String.Format("{0} № {1} от {2}", DocType.Caption, Number, Date.ToString("dd.MM.yyyy"));
        }

        #region Функции для сервисов

        /// <summary>   
        /// Получение массива идентификаторов документов всех версий этого документа, включая его самого
        /// </summary>         
        public int[] GetIdAllVersionDoc(DataContext context, bool isClearCache = false)
        {
            if (isClearCache || _ids == null)
            {
                var curdoc = this;
                List<int> tmp = new List<int>();
                while (curdoc != null)
                {
                    tmp.Add(curdoc.Id);
                    curdoc = GetPrevVersionDoc(context, curdoc);
                }
                _ids = tmp.ToArray();
            }

            return _ids;
        }

        /// <summary>   
        /// Получение предыдущей версии документа
        /// </summary>         
        public ActivityOfSBP GetPrevVersionDoc(DataContext context, ActivityOfSBP curdoc)
        {
            if (curdoc.IdParent.HasValue)
            {
                return
                    context.ActivityOfSBP.Where(w => w.Id == curdoc.IdParent).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void FillData_ItemsSystemGoals(DataContext context)
        {
            var qSg0 = context.SystemGoal.Where(w =>
                w.IdPublicLegalFormation == IdPublicLegalFormation
                && w.IdDocType_CommitDoc == IdDocType
                && w.DateStart >= DateStart && w.DateEnd <= DateEnd
                && w.IdRefStatus == (byte)RefStats.Work
            );

            // получаем актуальные записи из справочника "Система целеполагания"
            var qSg = qSg0.Select(a => new { sg = a, isOtherDocSG = false }).Concat(
                qSg0.Where(w => w.IdParent.HasValue && !qSg0.Any(a => a.Id == w.IdParent)).Select(s => new { sg = s.Parent, isOtherDocSG = true }).Distinct()
            ).ToList();

            // получаем записи из тч, с признаком из этого документа
            var qTp = context.ActivityOfSBP_SystemGoalElement.Where(w => w.IdOwner == Id).ToList();

            // чтобы удалять было не напряжно, иерархию изначально удаляем
            foreach (var item in qTp)
            {
                item.IdParent = null;
            }

            // обновляем где нужно признак из другого документа
            var qUpdateItems = qTp.Join(
                qSg, a => a.IdSystemGoal, b => b.sg.Id,
                (a, b) => new { Tp = a, b.isOtherDocSG }
            ).Where(w => w.Tp.FromAnotherDocumentSE != w.isOtherDocSG);
            foreach (var item in qUpdateItems)
            {
                item.Tp.FromAnotherDocumentSE = item.isOtherDocSG;
            }

            // создаем новые записи
            var qNewItems = qSg.Where(w => qTp.All(a => a.IdSystemGoal != w.sg.Id));
            foreach (var item in qNewItems)
            {
                context.ActivityOfSBP_SystemGoalElement.Add(new ActivityOfSBP_SystemGoalElement
                    {
                    IdOwner = Id,
                    IdSystemGoal = item.sg.Id,
                    FromAnotherDocumentSE = item.isOtherDocSG
                });
            }

            // удаляем устаревшие
            var qDelItems = qTp.Where(w => qSg.All(a => a.sg.Id != w.IdSystemGoal));
            foreach (var item in qDelItems)
            {
                context.ActivityOfSBP_SystemGoalElement.Remove(item);
            }

            context.SaveChanges();

            // теперь удаляем лишние данные для СЦ из другого документа
            var qGi1 = context.ActivityOfSBP_GoalIndicator.Where(w => w.IdOwner == Id && w.Master.FromAnotherDocumentSE);
            foreach (var item in qGi1)
            {
                context.ActivityOfSBP_GoalIndicator.Remove(item);
            }

            context.SaveChanges();

            // для записей из нашего документа обновляем вычислемые хранимые поля (в т.ч. восстанавливаем иерархию), показатели, значения показателей
            int[] items = context.ActivityOfSBP_SystemGoalElement.Where(w => w.IdOwner == Id).Select(s => s.Id).ToArray();
            RefreshData_SystemGoalElement(context, items);
            FillData_GoalIndicator_Value(context, items);
        }

        public void RefreshData_SystemGoalElement(DataContext context, int[] items, bool flag = false)
        {
            //вызвать контроль и передать данные
            if (flag)
                  ExecuteControl(e => e.Control_0342(context, items));
            
            var list = context.ActivityOfSBP_SystemGoalElement.Where(w => w.IdOwner == Id).Select(s => new { str = s, sg = s.SystemGoal }).ToList();
            foreach (var item in list.Where(w => items.Contains(w.str.Id)))
            {
                var obj = item.sg;
                item.str.IdElementTypeSystemGoal = obj.IdElementTypeSystemGoal;
                item.str.IdSBP = obj.IdSBP;
                item.str.Code = obj.Code;
                item.str.DateStart = obj.DateStart;
                item.str.DateEnd = obj.DateEnd;
                item.str.IdParent = item.str.FromAnotherDocumentSE ? null : list.Where(s => s.str.IdSystemGoal == item.sg.IdParent).Select(a => (int?)a.str.Id).SingleOrDefault();
            }
            context.SaveChanges();
        }

        public void FillData_GoalIndicator_Value(DataContext context, int[] items)
        {
            // записи "Элементы СЦ" для которых будем обновлять показатели и значения показателей
            var qTpSg = context.ActivityOfSBP_SystemGoalElement.Where(w => items.Contains(w.Id) && !w.FromAnotherDocumentSE);

            // подходящие показатели для обновления
            var qGi = context.SystemGoal_GoalIndicator.Where(w => w.IdVersion == IdVersion)
                             .Join(qTpSg, a => a.IdOwner, b => b.IdSystemGoal, (a, b) => a).ToList();

            // существующие показатели в тч
            var qTpGi = context.ActivityOfSBP_GoalIndicator.Where(w => w.IdOwner == Id)
                               .Join(qTpSg, a => a.IdMaster, b => b.Id, (a, b) => a).ToList();

            // вставляем новые показатели
            var qNewItems = qGi.Where(w =>
                !qTpGi.Any(a =>
                    a.IdGoalIndicator == w.IdGoalIndicator
                    && a.Master.IdSystemGoal == w.IdOwner
                )
            ).Join(
                qTpSg, a => a.IdOwner, b => b.IdSystemGoal, (a, b) => new { IdGoalIndicator = a.IdGoalIndicator, IdMaster = b.Id }
            );
            foreach (var i in qNewItems)
            {
                context.ActivityOfSBP_GoalIndicator.Add(new ActivityOfSBP_GoalIndicator()
                {
                    IdOwner = Id,
                    IdMaster = i.IdMaster,
                    IdGoalIndicator = i.IdGoalIndicator
                });
            }

            // удаляем устаревшие показатели
            var qDelItems = qTpGi.Where(w => !qGi.Any(a => a.IdGoalIndicator == w.IdGoalIndicator && a.IdOwner == w.Master.IdSystemGoal));
            foreach (var i in qDelItems)
            {
                context.ActivityOfSBP_GoalIndicator.Remove(i);
            }

            context.SaveChanges();

            // теперь обновляем значения показателей
            int[] itms = context.ActivityOfSBP_GoalIndicator.Where(w => w.IdOwner == Id)
                                .Join(qTpSg, a => a.IdMaster, b => b.Id, (a, b) => a)
                                .Select(s => s.Id).ToArray();
            RefreshData_GoalIndicator_Value(context, itms);
        }

        public void FillData_ActivityResourceMaintenance_Value(DataContext context, int[] items)
        {

            // удаляем перечитываемые строки
            var qDelItems = context.ActivityOfSBP_ActivityResourceMaintenance.Where(w => w.IdOwner == this.Id && w.IsDocument == true);
            context.ActivityOfSBP_ActivityResourceMaintenance.RemoveAll(qDelItems);


            var Activitys = context.ActivityOfSBP_Activity.Where(w => w.IdOwner == Id).ToList();

            var IdBudget = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id;

            var blank = context.ActivityOfSBP_SBPBlankActual.FirstOrDefault(b => b.SBP_BlankHistory.IdBudget == IdBudget).SBP_BlankHistory;

            if (blank == null)
            {
                return;
            }

            foreach (var Activity in Activitys)
            {
                
                var sbp = context.SBP.SingleOrDefault(s => s.Id == Activity.IdSBP);

                byte req = (byte) BlankValueType.Mandatory;

                var regLVA_Value =
                    (from LVA in context.LimitVolumeAppropriations.Where(w =>
                                                                         w.IdPublicLegalFormation ==
                                                                         this.IdPublicLegalFormation
                                                                         && w.IdBudget == IdBudget
                                                                         && w.IdVersion == this.IdVersion
                                                                         && w.IdValueType == (byte) ValueType.Justified)
                     join EL in context.EstimatedLine on LVA.IdEstimatedLine equals EL.Id
                     join CSBP in
                         context.SBP.Where(
                             w => w.IdParent == Activity.IdSBP && w.IdSBPType == (byte) SBPType.TreasuryEstablishment)
                         on EL.IdSBP equals CSBP.Id
                     join TC in context.TaskCollection.Where(
                         tc => tc.IdContingent == Activity.IdContingent && tc.IdActivity == Activity.IdActivity) on
                         LVA.IdTaskCollection equals TC.Id
                     select new {LVA, EL})
                        .GroupBy(g => new
                            {
                                ExpenseObligationType = (blank.IdBlankValueType_ExpenseObligationType != req || g.EL.IdExpenseObligationType == null ? null : g.EL.IdExpenseObligationType),
                                FinanceSource = (blank.IdBlankValueType_FinanceSource != req || g.EL.IdFinanceSource == null ? null : g.EL.FinanceSource),
                                KFO = (blank.IdBlankValueType_KFO != req || g.EL.IdKFO == null ? null : g.EL.KFO),
                                KVSR = (blank.IdBlankValueType_KVSR != req || g.EL.IdKVSR == null ? null : g.EL.KVSR),
                                RZPR = (blank.IdBlankValueType_RZPR != req || g.EL.IdRZPR == null ? null : g.EL.RZPR),
                                KCSR = (blank.IdBlankValueType_KCSR != req || g.EL.IdKCSR == null ? null : g.EL.KCSR),
                                KVR = (blank.IdBlankValueType_KVR != req || g.EL.IdKVR == null ? null : g.EL.KVR),
                                KOSGU = (blank.IdBlankValueType_KOSGU != req || g.EL.IdKOSGU == null ? null : g.EL.KOSGU),
                                DFK = (blank.IdBlankValueType_DFK != req || g.EL.IdDFK == null ? null : g.EL.DFK),
                                DKR = (blank.IdBlankValueType_DKR != req || g.EL.IdDKR == null ? null : g.EL.DKR),
                                DEK = (blank.IdBlankValueType_DEK != req || g.EL.IdDEK == null ? null : g.EL.DEK),
                                CodeSubsidy = (blank.IdBlankValueType_CodeSubsidy != req || g.EL.IdCodeSubsidy == null ? null : g.EL.CodeSubsidy),
                                BranchCode = (blank.IdBlankValueType_BranchCode != req || g.EL.IdBranchCode == null ? null : g.EL.BranchCode),
                                g.LVA.IdOKATO,
                                g.LVA.IdAuthorityOfExpenseObligation,
                                g.LVA.HierarchyPeriod,
                                g.LVA.HasAdditionalNeed
                            })
                        .Select(s => new
                            {
                                Key = s.Key,
                                Value = s.Sum(c => (decimal?) c.LVA.Value)
                            })
                        .Where(r => r.Value > 0)
                        .ToList();

                var regLVA = regLVA_Value.GroupBy(g => new
                {
                    ExpenseObligationType = g.Key.ExpenseObligationType,
                    FinanceSource = g.Key.FinanceSource,
                    KFO =  g.Key.KFO,
                    KVSR =g.Key.KVSR,
                    RZPR =g.Key.RZPR,
                    KCSR =g.Key.KCSR,
                    KVR = g.Key.KVR,
                    KOSGU = g.Key.KOSGU,
                    DFK = g.Key.DFK,
                    DKR = g.Key.DKR,
                    DEK = g.Key.DEK,
                    CodeSubsidy = g.Key.CodeSubsidy,
                    BranchCode = g.Key.BranchCode,
                    g.Key.IdOKATO,
                    g.Key.IdAuthorityOfExpenseObligation
                }).Select(s => new
                {
                    Key = s.Key
                }).ToList();
                
                foreach (var LVA in regLVA)
                {
                    var newActivityResourceMaintenance = new ActivityOfSBP_ActivityResourceMaintenance()
                    {
                        Owner = this,
                        Master = Activity,
                        IdBudget = IdBudget,
                        IdExpenseObligationType = LVA.Key.ExpenseObligationType,
                        FinanceSource = LVA.Key.FinanceSource,
                        KFO = LVA.Key.KFO,
                        KVSR = LVA.Key.KVSR,
                        RZPR = LVA.Key.RZPR,
                        KCSR = LVA.Key.KCSR,
                        KVR = LVA.Key.KVR,
                        KOSGU = LVA.Key.KOSGU,
                        DFK = LVA.Key.DFK,
                        DKR = LVA.Key.DKR,
                        DEK = LVA.Key.DEK,
                        CodeSubsidy = LVA.Key.CodeSubsidy,
                        BranchCode = LVA.Key.BranchCode,
                        IdOKATO = LVA.Key.IdOKATO,
                        IdAuthorityOfExpenseObligation = LVA.Key.IdAuthorityOfExpenseObligation,
                        IsDocument = true
                    };

                    context.ActivityOfSBP_ActivityResourceMaintenance.Add(newActivityResourceMaintenance);

                    // Находим Значения по строке КБК

                    var LVA_Value = regLVA_Value.Where(w =>
                        w.Key.ExpenseObligationType == LVA.Key.ExpenseObligationType &&
                        w.Key.FinanceSource == LVA.Key.FinanceSource &&
                        w.Key.KFO == LVA.Key.KFO &&
                        w.Key.KVSR == LVA.Key.KVSR &&
                        w.Key.RZPR == LVA.Key.RZPR &&
                        w.Key.KCSR == LVA.Key.KCSR &&
                        w.Key.KVR == LVA.Key.KVR &&
                        w.Key.KOSGU == LVA.Key.KOSGU &&
                        w.Key.DFK == LVA.Key.DFK &&
                        w.Key.DKR == LVA.Key.DKR &&
                        w.Key.DEK == LVA.Key.DEK &&
                        w.Key.CodeSubsidy == LVA.Key.CodeSubsidy &&
                        w.Key.BranchCode == LVA.Key.BranchCode &&
                        w.Key.IdOKATO == LVA.Key.IdOKATO &&
                        w.Key.IdAuthorityOfExpenseObligation == LVA.Key.IdAuthorityOfExpenseObligation 
                        ).ToList();

                    foreach (var Value in LVA_Value)
                    {
                        var newActivityResourceMaintenance_Value = new ActivityOfSBP_ActivityResourceMaintenance_Value()
                        {
                            Owner = this,
                            Master = newActivityResourceMaintenance,
                            HierarchyPeriod = Value.Key.HierarchyPeriod,
                            Value = Value.Key.HasAdditionalNeed == true ? 0 : Value.Value,
                            AdditionalValue = Value.Key.HasAdditionalNeed == true ? Value.Value : 0,

                        };
                        context.ActivityOfSBP_ActivityResourceMaintenance_Value.Add(newActivityResourceMaintenance_Value);
                    }
 
                    
                    
                
                }
                context.SaveChanges();
            }



        }

        public void RefreshData_GoalIndicator_Value(DataContext context, int[] items)
        {
            // показатели для которых обновляем значения
            var qTpGi = context.ActivityOfSBP_GoalIndicator.Where(w => items.Contains(w.Id));

            // уже существующие значения показателей
            var qTpGiv = context.ActivityOfSBP_GoalIndicator_Value.Where(w => w.IdOwner == Id && items.Contains(w.IdMaster)).ToList();

            // данные по значениям показателей для обновления
            var qGiv = context.SystemGoal_GoalIndicatorValue.Where(r => r.Master.IdVersion == IdVersion).Join(
                qTpGi,
                a => new { IdSytemGoal = a.IdOwner, IdGoalIndicator = a.Master.IdGoalIndicator },
                b => new { IdSytemGoal = b.Master.IdSystemGoal, IdGoalIndicator = b.IdGoalIndicator },
                (a, b) => new { Giv = a, TpGi = b }
            ).ToList();

            // создаем новые значения показателей
            var qNewItems = qGiv.Where(w =>
                !qTpGiv.Any(a =>
                    a.Master.IdGoalIndicator == w.TpGi.IdGoalIndicator
                    && a.Master.Master.IdSystemGoal == w.Giv.IdOwner
                    && a.IdHierarchyPeriod == w.Giv.IdHierarchyPeriod
                    && a.Value == w.Giv.Value
                )
            ).Select(s => new
            {
                IdMaster = s.TpGi.Id,
                IdHierarchyPeriod = s.Giv.IdHierarchyPeriod,
                Value = s.Giv.Value
            });
            foreach (var v in qNewItems)
            {
                context.ActivityOfSBP_GoalIndicator_Value.Add(new ActivityOfSBP_GoalIndicator_Value()
                {
                    IdOwner = Id,
                    IdMaster = v.IdMaster,
                    IdHierarchyPeriod = v.IdHierarchyPeriod,
                    Value = v.Value
                });
            }

            // удаляем лишние показатели
            var qDelItems = qTpGiv.Where(w =>
                !qGiv.Any(a =>
                    a.TpGi.IdGoalIndicator == w.Master.IdGoalIndicator
                    && a.Giv.IdOwner == w.Master.Master.IdSystemGoal
                    && a.Giv.IdHierarchyPeriod == w.IdHierarchyPeriod
                    && a.Giv.Value == w.Value
                )
            );

            //var qDelOf = context.ActivityOfSBP_GoalIndicator_Value.Where(r => r.IdOwner == this.Id && !qTpGi.Any(w => w.Id == r.IdMaster));

            foreach (var v in qDelItems)//.Union(qDelOf))
            {
                context.ActivityOfSBP_GoalIndicator_Value.Remove(v);
            }

            context.SaveChanges();
        }


        #endregion


        #region Функции для работы с версиями

        /// <summary>   
        /// Кэш массива идентификаторов документов всех версий этого документа
        /// </summary>         
        private int[] _ids;

        /// <summary>   
        /// Получение массива идентификаторов документов всех версий этого документа, включая его самого
        /// </summary>         
        public int[] AllVersionDocIds
        {
            get
            {
                if (_ids == null)
                {
                    var context = IoC.Resolve<DbContext>().Cast<DataContext>();
                    _ids = this.GetIdAllVersion(context).ToArray();
                }
                return _ids;
            }
        }
       
        #endregion

        #region Методы операций

        private List<TaskCollection> taskCollection;
        Dictionary<int, int> dirActivity;

        /// <summary>   
        /// Операция «Обработать»   
        /// </summary>  
        public void Process(DataContext context)
        {

            InitScopeDoc(context);
            GetDataDocTables(context);

            if (!IgnorControlsOnProcess)
            {
                ExecProcessControls(context);
            }

            ReasonClarification = null;
            IsRequireClarification = false;

            IEnumerable<ActivityOfSBP_SystemGoalElement> tpSystemGoalElementsLoc =
                tpSystemGoalElement.Where(r =>
                                          (!r.FromAnotherDocumentSE &&
                                           r.SystemGoal.IdDocType_CommitDoc == IdDocType &&
                                           r.SystemGoal.IdDocType_ImplementDoc != IdDocType));

            byte bNewProgram;
            var newProgram = CreateProgram(context, out bNewProgram);

            if (bNewProgram == 2)
            {
                InitScopeDoc(context, false);
                RegisterMethods.SetTerminatorById(context, _arrIdParent, this.EntityId, this.DateTerminate ?? DateTime.Now, this.Id, this.EntityId, _arrRegisters.Where(r => r != LimitVolumeAppropriations.EntityIdStatic).ToArray());
            }

            context.SaveChanges();

            IEnumerable<CPair> listId_sges
                = tpSystemGoalElementsLoc
                    .Select(s =>
                            new CPair()
                            {
                                Id = s.Id,
                                IdParent = s.IdParent
                            })
                    .ToList();

            IEnumerable<CPair> savSge
                = listId_sges.Where(r1 => !listId_sges.Select(r2 => r2.Id).Contains(r1.IdParent ?? 0));

            Dictionary<int, SystemGoalElement> dirSystemGoalElement = new Dictionary<int, SystemGoalElement>();

            IQueryable<SystemGoalElement> sgeOfParents;

            if (this.IdParent.HasValue)
            {
                sgeOfParents = DocSGEMethod.GetRegDataOfParentDocs(context, _arrIdParent, this.EntityId);
            }
            else
            {
                sgeOfParents = context.SystemGoalElement.Where(r => r.IdRegistrator == this.Id && r.IdRegistratorEntity == EntityId);
            }

            CreateMainMoves(context, listId_sges, savSge, newProgram, tpSystemGoalElementsLoc, dirSystemGoalElement, sgeOfParents);

            foreach (var lsge in tpSystemGoalElement)
            {
                if (!dirSystemGoalElement.Any(k => k.Key == lsge.Id))
                {
                    var rs =
                        context.SystemGoalElement.Where(
                            g => g.IdSystemGoal == lsge.IdSystemGoal && g.IdPublicLegalFormation == this.IdPublicLegalFormation);
                    if (rs.Any())
                    {
                        dirSystemGoalElement.Add(lsge.Id, rs.FirstOrDefault());
                    }
                }
            }

            List<TaskCollection> items = new List<TaskCollection>();
            foreach (var lineActivity in tpActivity)
            {
                bool newTC;
                TaskCollection newTaskCollection = RegisterMethods.FindTaskCollection(context,
                                                                                   this.IdPublicLegalFormation,
                                                                                   lineActivity.IdActivity,
                                                                                   lineActivity.IdContingent,
                                                                                   out newTC,
                                                                                   false);
                if (newTC)
                {
                    items.Add(newTaskCollection);
                }

            }
            context.TaskCollection.InsertAsTableValue(items, context);
            context.SaveChanges();

            var idsActivity = tpActivity.Select(s => s.IdActivity).Distinct().ToList();
            taskCollection = context.TaskCollection.Where(r => r.IdPublicLegalFormation == IdPublicLegalFormation && idsActivity.Contains(r.IdActivity)).ToList();
            var taskCollectionQ = taskCollection.AsQueryable();

            dirActivity = new Dictionary<int, int>();

            foreach (var lineActivity in tpActivity)
            {
                dirActivity.Add(lineActivity.Id, RegisterMethods.FindTaskCollection(taskCollectionQ,
                                                                                    lineActivity.IdActivity,
                                                                                    lineActivity.IdContingent).Id);
            }

            var iTaskVolume = new List<TaskVolume>();
            CreateTaskVolumeMoves(context, newProgram, dirSystemGoalElement, iTaskVolume);
            CreateTaskDemandCapacityMoves(context, newProgram, dirSystemGoalElement, iTaskVolume);
            context.TaskVolume.InsertAsTableValue(iTaskVolume, context);

            var iTaskIndicatorQuality = new List<TaskIndicatorQuality>();
            CreateTaskIndicatorQualityMoves(context, newProgram, iTaskIndicatorQuality);
            context.TaskIndicatorQuality.InsertAsTableValue(iTaskIndicatorQuality, context);

            var iProgram_ResourceMaintenance = new List<Program_ResourceMaintenance>();

            CreateResourceMaintenance(context, newProgram, iProgram_ResourceMaintenance);

            CreateActivityResourceMaintenance(context, newProgram, iProgram_ResourceMaintenance);

            context.Program_ResourceMaintenance.InsertAsTableValue(iProgram_ResourceMaintenance, context);

            CreateAttrProgram(context, newProgram);

            var iLimitVolumeAppropriations = new List<LimitVolumeAppropriations>();

            CreateLimitVolumeAppropriations(context, newProgram, iLimitVolumeAppropriations);

            context.LimitVolumeAppropriations.InsertAsTableValue(iLimitVolumeAppropriations, context);

            context.SaveChanges();

            #region часть: Требует уточнения

            IEnumerable<RegCommLink> docregs0308;
            IEnumerable<RegCommLink> docregs0311;
            string errstr0308;
            string errstr0311;

            LogicControl0308_0311(context,
                                  out docregs0308,
                                  out docregs0311,
                                  out errstr0308,
                                  out errstr0311);

            if (errstr0308 != string.Empty)
            {
                ExecuteControl(e => e.Control_0308(context, errstr0308));
            }
            if (errstr0311 != string.Empty)
            {
                ExecuteControl(e => e.Control_0311(context, errstr0311));
            }

            if (docregs0308.Any())
            {
                // Установление признака  «Требует уточнения» при несоответствии сроков реализации с нижестоящими элементами
                DocSGEMethod.SetRequireClarification(context, docregs0308, this.Header,
                    "{date}. У элементов из текущего документа имеются вышестоящие элементы из документа {this}, с которыми нарушается соответствие настроенной Модели СЦ."
                );
            }

            if (docregs0311.Any())
            {
                // Установление признака «Требует уточнения» при нарушении Модели СЦ 
                DocSGEMethod.SetRequireClarification(context, docregs0311, this.Header,
                                                     "{date}. Сроки реализации элементов из текущего документа не соответствуют срокам реализации их вышестоящих элементов из документа {this}."
                    );
            }

            var prevDoc = (ActivityOfSBP)CommonMethods.GetPrevVersionDoc(context, this, EntityId);
            if (prevDoc != null)
            {
                var tskVol = context.TaskVolume.Where(r =>
                                                      r.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                                      r.IdVersion == this.IdVersion &&
                                                      r.IdValueType == (int) ValueType.Plan &&
                                                      !r.IdTerminator.HasValue &&
                                                      r.SBP.IdSBPType != (int) DbEnums.SBPType.GeneralManager &&
                                                      r.SBP.IdSBPType != (int) DbEnums.SBPType.Manager &&
                                                      idsActivity.Contains(r.TaskCollection.IdActivity)
                    ).ToList();
                var lsbp = context.SBP.Where(r => r.IdPublicLegalFormation == this.IdPublicLegalFormation).ToList();

                var docsall = context.Set<IClarificationDoc>(PlanActivity.EntityIdStatic).ToList();

                // Установление признака «Требует уточнения» в документах «План деятельности» при изменении перечня мероприятий в деятельности ведомства
                var prevTpdActivity = context.ActivityOfSBP_Activity.Where(r => r.IdOwner == prevDoc.Id).ToList();
                var divActivity = prevTpdActivity.Where(o =>
                                                        !tpActivity.Any(n => o.IdActivity == n.IdActivity &&
                                                                             n.IdContingent == o.IdContingent
                                                             )).
                                                  ToList().
                                                  Select(
                                                      s =>
                                                      new
                                                          {
                                                              tc = dirActivity[s.Id],
                                                              sbp = s.SBP
                                                          }
                    );

                var recTV = (from da in divActivity
                             join tsk in tskVol.Where(r => r.HierarchyPeriod.HasEntrance(this.DateStart, this.DateEnd))
                                 on
                                 new
                                     {
                                         sbp = da.sbp,
                                         tc = da.tc
                                     }
                                 equals
                                 new
                                     {
                                         sbp = GetFirstParentRBS(tsk.SBP, lsbp),
                                         tc = tsk.IdTaskCollection
                                     }
                             select tsk).ToList();

                if (recTV.Any())
                {

                    var cladocs = recTV.
                        Select(tsk => new {tsk.IdRegistrator, tsk.RegistratorEntity}).
                        Distinct().
                        Select(s => new RegCommLink()
                            {
                                IdRegistrator = s.IdRegistrator,
                                RegistratorEntity = s.RegistratorEntity
                            });

                    DocSGEMethod.SetRequireClarificationSimple(context, cladocs, this.Header,
                                                               "{date}. В документе {this} был изменен перечень мероприятий на вкладке «Мероприятия»",
                                                               docsall
                        );
                }

                // Установление признака «Требует уточнения» в документах «План деятельности» при изменении объемов мероприятий в деятельности ведомства
                var existActivity = prevTpdActivity.Where(o =>
                                                          tpActivity.Any(n => o.IdActivity == n.IdActivity &&
                                                                              n.IdContingent == o.IdContingent
                                                              )).
                                                    ToList().
                                                    Select(
                                                        s =>
                                                        new
                                                            {
                                                                line = s.Id,
                                                                tc = RegisterMethods.FindTaskCollection(taskCollection,
                                                                                                        s.IdActivity,
                                                                                                        s.IdContingent),
                                                                sbp = s.SBP
                                                            });
                var tpActivityV = (from a in tpActivity
                                   join v in tpActivity_Value.Where(r => r.Value > 0) on a.Id equals v.IdMaster
                                   select new
                                       {
                                           a,
                                           v
                                       }).ToList();

                var prevTpdActivityValue = (from ea in existActivity
                                            join pr in
                                                context.ActivityOfSBP_Activity_Value.Where(r => r.IdOwner == prevDoc.Id)
                                                       .ToList() on
                                                new
                                                    {
                                                        idl = ea.line,
                                                        pd = prevDoc.Id
                                                    }
                                                equals new
                                                    {
                                                        idl = pr.IdMaster,
                                                        pd = pr.IdOwner
                                                    }
                                            select new CTmp0()
                                                {
                                                    tc = ea.tc,
                                                    sbp = ea.sbp,
                                                    hp = pr.HierarchyPeriod,
                                                    Value = pr.Value ?? 0
                                                }).ToList();

                var divTpdActivityValue1 = prevTpdActivityValue.Where(o =>
                                                                      !tpActivityV.Any(n =>
                                                                                       n.a.IdActivity == o.tc.IdActivity &&
                                                                                       (!n.a.IdContingent.HasValue &&
                                                                                        !o.tc.IdContingent.HasValue ||
                                                                                        n.a.IdContingent ==
                                                                                        o.tc.IdContingent) &&
                                                                                       n.v.IdHierarchyPeriod == o.hp.Id &&
                                                                                       n.v.Value == o.Value
                                                                           )).
                                                                ToList();

                var divTpdActivityValue2 = tpActivityV.Where(o =>
                                                             !prevTpdActivityValue.Any(n =>
                                                                                       n.tc.IdActivity == o.a.IdActivity &&
                                                                                       (!n.tc.IdContingent.HasValue &&
                                                                                        !o.a.IdContingent.HasValue ||
                                                                                        n.tc.IdContingent ==
                                                                                        o.a.IdContingent) &&
                                                                                       o.v.IdHierarchyPeriod == n.hp.Id &&
                                                                                       o.v.Value == n.Value
                                                                  )).
                                                       ToList().
                                                       Select(s =>
                                                              new CTmp0()
                                                                  {
                                                                      tc =
                                                                          RegisterMethods.FindTaskCollection(context,
                                                                                                             IdPublicLegalFormation,
                                                                                                             s.a
                                                                                                              .IdActivity,
                                                                                                             s.a
                                                                                                              .IdContingent),
                                                                      sbp = s.a.SBP,
                                                                      hp = s.v.HierarchyPeriod,
                                                                      Value = s.v.Value ?? 0
                                                                  });

                var divTpdActivityValue = divTpdActivityValue1.Union(divTpdActivityValue2).
                                                               Select(s =>
                                                                      new
                                                                          {
                                                                              s.Value,
                                                                              s.hp,
                                                                              s.sbp,
                                                                              s.tc
                                                                          }).
                                                               Distinct().
                                                               ToList();

                var recTV2 = (from da in divTpdActivityValue
                              join tsk in tskVol
                                  on
                                  new
                                      {
                                          sbp = da.sbp,
                                          tc = da.tc.Id,
                                          hp = da.hp.Id
                                      }
                                  equals
                                  new
                                      {
                                          sbp = GetFirstParentRBS(tsk.SBP, lsbp),
                                          tc = tsk.IdTaskCollection,
                                          hp = tsk.IdHierarchyPeriod
                                      }
                              select tsk).ToList();

                if (recTV2.Any())
                {
                    var cladocs2 = recTV2.Select(s =>
                                                 new
                                                     {
                                                         IdRegistrator = s.IdRegistrator,
                                                         RegistratorEntity = s.RegistratorEntity,
                                                         tsk = s
                                                     }

                        );

                    var docs = cladocs2.Select(s => new {s.IdRegistrator, s.RegistratorEntity}).Distinct();

                    foreach (var rcl in docs)
                    {
                        var oacts0 =
                            cladocs2.Where(
                                r =>
                                r.IdRegistrator == rcl.IdRegistrator && r.RegistratorEntity == rcl.RegistratorEntity).
                                     Select(s =>
                                            string.Format("{0}   {1}    {2}",
                                                          s.tsk.TaskCollection.Activity.Caption,
                                                          (s.tsk.TaskCollection.IdContingent.HasValue
                                                               ? s.tsk.TaskCollection.Contingent.Caption
                                                               : ""),
                                                          s.tsk.HierarchyPeriod.Caption)).
                                     Distinct();

                        var oacts = oacts0.Aggregate((a, b) => a + "\n" + b);

                        DocSGEMethod.SetRequireClarificInDoc(context,
                                                             new RegCommLink()
                                                                 {
                                                                     IdRegistrator = rcl.IdRegistrator,
                                                                     RegistratorEntity = rcl.RegistratorEntity
                                                                 }, this.Header,
                                                             "{date}. В документе {this} были изменены объемы в следующих мероприятиях: \n" +
                                                             oacts, docsall);
                    }
                    context.SaveChanges();
                }

                #endregion часть: Требует уточнения

                if (!IgnorControlsOnProcess)
                {
                    // Над документом, указанным в поле «Предыдущая редакция», выполнить операцию «В архив (скрытая)»
                    prevDoc.ExecuteOperation(e => e.Archive(context));
                }
            }

        }

        /// <summary>
        /// первое вышестоящее СБП с типом «ГРБС» или «РБС» 
        /// </summary>
        /// <param name="sbp">СБП, от которого начинаем искать</param>
        /// <param name="lsbp"></param>
        /// <returns></returns>
        public SBP GetFirstParentRBS(SBP sbp, List<SBP> lsbp)
        {
            SBP ret = sbp;
            do
            {
                if (!ret.IdParent.HasValue)
                {
                    ret = null;
                    break;
                }
                ret = lsbp.FirstOrDefault(r => r.Id == ret.IdParent);
                if (ret.IdSBPType == (int)DbEnums.SBPType.GeneralManager ||
                    ret.IdSBPType == (int)DbEnums.SBPType.Manager)
                {
                    break;
                }
            } while (true);

            return ret;
        }

        private class CTmp0
        {
            public TaskCollection tc;
            public SBP sbp;
            public HierarchyPeriod hp;
            public decimal Value;
        }

        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            SetBlankActual(context);
        }

        /// <summary>
        /// Операция "Редактировать"
        /// </summary>
        /// <param name="context"></param>
        public void BeforeEdit(DataContext context)
        {

            ExecuteControl(e => e.Control_0345(context));

            ExecuteControl(e => e.Control_0351(context));
            if (SetBlankActual(context))
            {
                TrimKbkByNewActualBlank(context);
                context.SaveChanges();
            }
        }
        public void Edit(DataContext context)
        {
            //ExecuteControl(e => e.Control_0345(context)); 
            DateLastEdit = DateTime.Now;
        }

        /// <summary>
        /// Операция «Отменить обработку»
        /// </summary>  
        public void UndoProcess(DataContext context)
        {
            ExecuteControl<CommonControl_7004>();

            InitScopeDoc(context);

            ReasonClarification = null;
            IsRequireClarification = false;

            RegisterMethods.RemoveFromRegistersByRegistrator(context, Id, EntityId, _arrRegisters);
            using (new ControlScope())
            {
                context.SaveChanges();
            }

            RegisterMethods.ClearTerminatorByIdDoc(context, Id, EntityId, _arrRegisters.Where(r => r != LimitVolumeAppropriations.EntityIdStatic).ToArray());

            var prevDoc = (ActivityOfSBP)CommonMethods.GetPrevVersionDoc(context, this, EntityId);
            if (prevDoc != null)
            {
                prevDoc.ExecuteOperation(e => e.UndoArchive(context));
            }
        }

        /// <summary>   
        /// Операция «Утвердить»   
        /// </summary>  
        public void Confirm(DataContext context)
        {
            InitScopeDoc(context);
            GetDataDocTables(context);

            ExecuteControl<CommonControlAddNeed_0242>();
            ExecuteControl(e => e.Control_0313(context));
            ExecuteControl(e => e.Control_0327(context));
            ExecuteControl(e => e.Control_0349(context));

            if (!HasAdditionalNeed)
            {
                DateCommit = DateTime.Now;

                RegisterMethods.SetRegsApproved(context, Id, Date, EntityId, AllVersionDocIds, _arrRegisters.Where(r => r != LimitVolumeAppropriations.EntityIdStatic).ToArray());

                var table = context.LimitVolumeAppropriations;
                var qq = table.Where(w => w.IdRegistratorEntity == EntityId && (AllVersionDocIds.Contains(w.IdRegistrator) || w.IdRegistrator == this.Id)
                    );
                foreach (var rec in qq)
                {
                    rec.DateCommit = Date;
                    rec.IdApprovedEntity = EntityId;
                    rec.IdApproved = Id;
                }
            }
            else
            {
                // 2.	Создать новую редакцию документа 
                var newDoc = CloneActivityOfSbp(context, false, DateTime.Now.Date, DocStatus.Approved);

                newDoc.ReasonTerminate = null;
                newDoc.ReasonCancel = null;
                newDoc.HasAdditionalNeed = false;

                // очистить поля «Доп. потребность» 
                DocSGEMethod.DeclineAddValueInTp(context, ActivityOfSBP_ResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
                DocSGEMethod.DeclineAddValueInTp(context, ActivityOfSBP_Activity_Value.EntityIdStatic, newDoc.Id);
                DocSGEMethod.DeclineAddValueInTp(context, ActivityOfSBP_ActivityResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
                DocSGEMethod.DeclineAddValueInTp(context, ActivityOfSBP_IndicatorQualityActivity_Value.EntityIdStatic, newDoc.Id);

                // 3.	Найти в регистрах .... Во всех найденных записях установить Дата утверждения = ШапкаДокумента.Дата, Утверждающий документ = текущий документ
                var lRegisters = new List<int>();
                lRegisters.Add(AttributeOfSystemGoalElement.EntityIdStatic);
                lRegisters.Add(ValuesGoalTarget.EntityIdStatic);
                lRegisters.Add(GoalTarget.EntityIdStatic);
                lRegisters.Add(Sbor.Registry.SystemGoalElement.EntityIdStatic);
                lRegisters.Add(AttributeOfProgram.EntityIdStatic);
                lRegisters.Add(Program.EntityIdStatic);
                RegisterMethods.SetRegsApproved(context, newDoc.Id, newDoc.Date, EntityId, AllVersionDocIds, lRegisters.ToArray());

                //4.	Найти в регистрах «Ресурсное обеспечение программ», «Объемы задач», «Показатели качества задач» все записи, у которых
                //•	Регистратор – текущий документ или документ-предок (по цепочке документов) и Аннулятор = Ложь и Дата утверждения = пусто.
                //Во всех найденных записях установить:
                //- если Регистр. «Доп.потребность» = Ложь, то  «Дата утверждения» = ШапкаДокумента.Дата, «Утверждающий документ» = текущий документ.
                //- если Регистр. «Доп. потребность» = Истина, то «Дата аннулирования» = Документ.Дата документа, «Аннулятор» = текущий документ.
                RegisterMethods.SetApproveOrTerminateByAddValue(context, _arrIdParent, this.Id, newDoc.Id, this.EntityId, Program_ResourceMaintenance.EntityIdStatic, newDoc.Date);
                RegisterMethods.SetApproveOrTerminateByAddValue(context, _arrIdParent, this.Id, newDoc.Id, this.EntityId, TaskIndicatorQuality.EntityIdStatic, newDoc.Date);
                RegisterMethods.SetApproveOrTerminateByAddValue(context, _arrIdParent, this.Id, newDoc.Id, this.EntityId, TaskVolume.EntityIdStatic, newDoc.Date);
                RegisterMethods.SetApproveOrTerminateByAddValueForLVA(context, _arrIdParent, this.Id, newDoc.Id, this.EntityId, newDoc.Date);
            }

            IdDocStatus = !HasAdditionalNeed ? DocStatus.Approved : DocStatus.Archive;
            context.SaveChanges();
        }

        /// <summary>
        /// Временный переключатель - для игнорирования всех контролей на операции "Обработать"
        /// </summary>
        public bool IgnorControlsOnProcess = false;

        /// <summary>   
        /// Операция «Утвердить с доп. потребностями»   
        /// </summary>  
        public void ConfirmWithAddNeed(DataContext context)
        {
            InitScopeDoc(context);
            GetDataDocTables(context);

            ExecuteControl(e => e.Control_0313(context));
            ExecuteControl(e => e.Control_0327(context));
            ExecuteControl(e => e.Control_0349(context));

            ExecuteControl<CommonControlAddNeed_0243>();
            ExecuteControl<CommonControlAddNeed_0244>();

            var newDoc = CloneActivityOfSbp(context, false, DateTime.Now.Date, DocStatus.Approved);

            newDoc.ReasonTerminate = null;
            newDoc.ReasonCancel = null;
            newDoc.HasAdditionalNeed = false;

            DocSGEMethod.AcceptAddValueInTp(context, ActivityOfSBP_ResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
            DocSGEMethod.AcceptAddValueInTp(context, ActivityOfSBP_Activity_Value.EntityIdStatic, newDoc.Id);
            DocSGEMethod.AcceptAddValueInTp(context, ActivityOfSBP_ActivityResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
            DocSGEMethod.AcceptAddValueInTp(context, ActivityOfSBP_IndicatorQualityActivity_Value.EntityIdStatic, newDoc.Id);

            this.IdDocStatus = DocStatus.Archive;
            context.SaveChanges();

            newDoc.IgnorControlsOnProcess = true;
            newDoc.Process(context);
            newDoc.IgnorControlsOnProcess = false;

            newDoc.DateCommit = DateTime.Now;

            RegisterMethods.SetRegsApproved(context, newDoc.Id, newDoc.Date, EntityId, AllVersionDocIds, _arrRegisters.Where(r => r != LimitVolumeAppropriations.EntityIdStatic).ToArray());

            var table = context.LimitVolumeAppropriations;
            var qq = table.Where(w => w.IdRegistratorEntity == EntityId && (AllVersionDocIds.Contains(w.IdRegistrator) || w.IdRegistrator == this.Id)
                );
            foreach (var rec in qq)
            {
                rec.DateCommit = Date;
                rec.IdApprovedEntity = EntityId;
                rec.IdApproved = Id;
            }

        }

        /// <summary>   
        /// Операция «Отменить утверждение»   
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            DateCommit = null;

            InitScopeDoc(context);
            RegisterMethods.ClearRegsApproved(context, Id, EntityId, _arrRegisters.Where(r => r != LimitVolumeAppropriations.EntityIdStatic).ToArray());

            var table = context.LimitVolumeAppropriations;
            foreach (var rec in table.Where(w => w.IdApprovedEntity == EntityIdStatic && w.IdApproved == this.Id))
            {
                rec.DateCommit = null;
                rec.IdApprovedEntity = null;
                rec.IdApproved = null;
            }
        }

        /// <summary>   
        /// Операция «Прекратить»
        /// </summary>  
        public void Terminate(DataContext context)
        {
            ExecuteControl<Control_7001>();

            InitScopeDoc(context);
            GetDataDocTables(context);

            ExecuteControl(e => e.Control_0339(context));
            ExecuteControl(e => e.Control_0340(context));

            //2. Аннулировать проводки (с учетом ППО, Версии):

            RegisterMethods.SetTerminatorById(context, _arrIdParent, this.EntityId, this.DateTerminate ?? DateTime.Now, this.Id, this.EntityId, _arrRegisters.Where(r => r != LimitVolumeAppropriations.EntityIdStatic).ToArray());
        }

        /// <summary>   
        /// Операция «Отменить прекращение»   
        /// </summary>  
        public void UndoTerminate(DataContext context)
        {
            var dataContext = context;
            InitScopeDoc(dataContext);
            GetDataDocTables(dataContext);

            ExecuteControl(e => e.Control_0341(context));

            DateTerminate = null;
            ReasonTerminate = null;

            //3. В регистрах отменить аннулирование произведенное при переводе на статус «Прекращен», очистить поле «Аннулятор» и «Дата аннулирования»

            var exoper = DocSGEMethod.GetLastExecutedOperation(dataContext, this.Id, this.EntityId, "Terminate");

            RegisterMethods.ClearTerminatorByIdDoc(dataContext, this.Id, EntityId, _arrRegisters.Where(r => r != LimitVolumeAppropriations.EntityIdStatic).ToArray(), exoper.Any() ? exoper.First() : 0);
        }

        /// <summary>   
        /// Операция «Изменить»   
        /// </summary>  
        public void Change(DataContext context)
        {

            var newDoc = CloneActivityOfSbp(context, this.HasAdditionalNeed, null, DocStatus.Draft);
            newDoc.Date = DateTime.Now.Date;
        }

        /// <summary>   
        /// Операция «Отменить изменение»   
        /// </summary>
        public void UndoChange(DataContext context)
        {
            var q = context.ActivityOfSBP.Where(w => w.IdParent == Id);
            foreach (var doc in q)
            {
                context.ActivityOfSBP.Remove(doc);
            }

            IdDocStatus = DateCommit.HasValue ? DocStatus.Approved : ((ReasonCancel ?? string.Empty) != string.Empty ? DocStatus.Denied : DocStatus.Project);

            context.SaveChanges();
        }

        /// <summary>
        /// Операция «Отказать»
        /// </summary>  
        public void Deny(DataContext context)
        {
            if (string.IsNullOrEmpty(ReasonCancel))
            {
                Controls.Throw("Не заполнено поле 'Причина отказа'");
            }
        }

        /// <summary>   
        /// Операция «Отменить отказ»   
        /// </summary>  
        public void ReturnToProject(DataContext context)
        {
            //Очистить поле «Причина отказа»
            ReasonCancel = null;
        }

        /// <summary>   
        /// Операция «Вернуть на черновик»   
        /// </summary>  
        public void BackToDraft(DataContext context)
        {
            UndoProcess(context);
        }

        /// <summary>   
        /// Операция «В архив»   
        /// </summary>  
        public void Archive(DataContext context)
        {

        }

        /// <summary>   
        /// Операция «Вернуть на изменен»   
        /// </summary>  
        public void UndoArchive(DataContext context)
        {

        }

        /// <summary>   
        /// Операция «Согласование МРГ»   
        /// </summary>  
        public void CheckMRG(DataContext context)
        {
            ExecuteControl(e => e.Control_0353(context));
        }


        #endregion

        #region Вспомогательные методы для операций и контролей

        public bool SetBlankActual(DataContext context)
        {
            var budgets = context.Budget.Where(r => r.IdRefStatus == (byte)RefStatus.Work && r.IdPublicLegalFormation == this.IdPublicLegalFormation);
            var IdCurBudget = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id;

            var oldCurBlanksActual = context.ActivityOfSBP_SBPBlankActual.Where(r => r.IdOwner == this.Id && r.SBP_BlankHistory.IdBudget == IdCurBudget).ToList();

            SBP_BlankHistory oldBlankActual = null;

            foreach (var bgt in budgets)
            {
                var newBlanks =
                    context.SBP_BlankHistory.Where(r =>
                                                   (((this.SBP.SBPType == DbEnums.SBPType.GeneralManager) && r.IdOwner == this.SBP.Id) ||
                                                    ((this.SBP.SBPType == DbEnums.SBPType.Manager) && r.IdOwner == this.SBP.IdParent)) &&
                                                   r.IdBlankType == (byte) DbEnums.BlankType.FormationGRBS &&
                                                   r.IdBudget == bgt.Id)
                           .OrderByDescending(o => o.DateCreate);

                if (newBlanks.Any())
                {
                    var newBlankActual = newBlanks.FirstOrDefault();
                    ActivityOfSBP_SBPBlankActual newSBP_BlankActual = new ActivityOfSBP_SBPBlankActual()
                        {
                            IdOwner = this.Id,
                            IdSBP_BlankHistory = newBlankActual.Id
                        };

                    var oldBlanksActual = context.ActivityOfSBP_SBPBlankActual.Where(r => r.IdOwner == this.Id && r.SBP_BlankHistory.IdBudget == bgt.Id);

                    if (oldBlanksActual.Any())
                    {
                        oldBlankActual = oldBlanksActual.FirstOrDefault().SBP_BlankHistory;

                        var chBlankActual = oldBlanksActual.FirstOrDefault();

                        if (chBlankActual.IdSBP_BlankHistory != newBlankActual.Id)
                        {
                            chBlankActual.IdSBP_BlankHistory = newBlankActual.Id;
                        }
                    }
                    else
                    {
                        context.ActivityOfSBP_SBPBlankActual.Add(newSBP_BlankActual);
                    }
                }
            }
            context.SaveChanges();

            var newCurBlanksActual = context.ActivityOfSBP_SBPBlankActual.Where(r => r.IdOwner == this.Id && r.SBP_BlankHistory.IdBudget == IdCurBudget);

            if (newCurBlanksActual.Any() && oldBlankActual != null)
            {
                return !SBP_BlankHelper.IsEqualBlank(newCurBlanksActual.FirstOrDefault().SBP_BlankHistory, oldBlankActual);
            }
            else
            {
                return false;
            }

        }

        private void TrimKbkByNewActualBlank(DataContext context)
        {

            var IdCurBudget = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id;

            var oldBlanks = context.ActivityOfSBP_SBPBlankActual.Where(r => r.IdOwner == this.Id && r.SBP_BlankHistory.IdBudget == IdCurBudget);

            var SBP_BlankActual = oldBlanks.FirstOrDefault().SBP_BlankHistory;

            var gkbktrim =
                (from kbk in context.ActivityOfSBP_ActivityResourceMaintenance.
                                     Where(r => r.IdOwner == this.Id && r.IdBudget.HasValue && r.IdBudget == IdCurBudget).
                                     ToList().
                                     Select(s =>
                                            new
                                                {
                                                    s.IdOwner,
                                                    s.IdMaster,
                                                    IdBranchCode = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_BranchCode) ? null : s.IdBranchCode,
                                                    IdCodeSubsidy = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_CodeSubsidy) ? null : s.IdCodeSubsidy,
                                                    IdDEK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DEK) ? null : s.IdDEK,
                                                    IdDFK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DFK) ? null : s.IdDFK,
                                                    IdDKR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DKR) ? null : s.IdDKR,
                                                    IdExpenseObligationType = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_ExpenseObligationType) ? null : s.IdExpenseObligationType,
                                                    IdFinanceSource = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_FinanceSource) ? null : s.IdFinanceSource,
                                                    IdKCSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KCSR) ? null : s.IdKCSR,
                                                    IdKOSGU = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KOSGU) ? null : s.IdKOSGU,
                                                    IdKFO = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KFO) ? null : s.IdKFO,
                                                    IdKVR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVR) ? null : s.IdKVR,
                                                    IdKVSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVSR) ? null : s.IdKVSR,
                                                    IdRZPR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_RZPR) ? null : s.IdRZPR,
                                                    s.IdOKATO,
                                                    s.IdAuthorityOfExpenseObligation,
                                                    s.Id,
                                                    s.IdBudget
                                                })
                 join val in context.ActivityOfSBP_ActivityResourceMaintenance_Value.Where(r => r.IdOwner == this.Id) on
                     kbk.Id equals val.IdMaster
                 select new {kbk, val}).
                    GroupBy(g =>
                            new
                                {
                                    g.kbk.IdOwner,
                                    g.kbk.IdMaster,
                                    g.kbk.IdBranchCode,
                                    g.kbk.IdCodeSubsidy,
                                    g.kbk.IdDEK,
                                    g.kbk.IdDFK,
                                    g.kbk.IdDKR,
                                    g.kbk.IdExpenseObligationType,
                                    g.kbk.IdFinanceSource,
                                    g.kbk.IdKCSR,
                                    g.kbk.IdKOSGU,
                                    g.kbk.IdKFO,
                                    g.kbk.IdKVR,
                                    g.kbk.IdKVSR,
                                    g.kbk.IdRZPR,
                                    g.kbk.IdOKATO,
                                    g.kbk.IdAuthorityOfExpenseObligation,
                                    g.kbk.IdBudget,
                                    g.val.IdHierarchyPeriod
                                }).
                    Select(s =>
                           new
                               {
                                   kbk = s.Key,
                                   Value = s.Sum(ss => ss.val.Value),
                                   AdditionalValue = s.Sum(ss => ss.val.AdditionalValue)
                               }).ToList();

            context.ActivityOfSBP_ActivityResourceMaintenance.RemoveAll(context.ActivityOfSBP_ActivityResourceMaintenance.Where(r => r.IdOwner == this.Id && r.IdBudget.HasValue && r.IdBudget == IdCurBudget));

            context.ActivityOfSBP_ActivityResourceMaintenance_Value.RemoveAll(context.ActivityOfSBP_ActivityResourceMaintenance_Value.Where(r => r.IdOwner == this.Id && r.Master.IdBudget.HasValue && r.Master.IdBudget == IdCurBudget));

            var kbks = gkbktrim.Select(g =>
                                       new
                                       {
                                           g.kbk.IdOwner,
                                           g.kbk.IdMaster,
                                           g.kbk.IdBranchCode,
                                           g.kbk.IdCodeSubsidy,
                                           g.kbk.IdDEK,
                                           g.kbk.IdDFK,
                                           g.kbk.IdDKR,
                                           g.kbk.IdExpenseObligationType,
                                           g.kbk.IdFinanceSource,
                                           g.kbk.IdKCSR,
                                           g.kbk.IdKOSGU,
                                           g.kbk.IdKFO,
                                           g.kbk.IdKVR,
                                           g.kbk.IdKVSR,
                                           g.kbk.IdOKATO,
                                           g.kbk.IdAuthorityOfExpenseObligation,
                                           g.kbk.IdRZPR,
                                           g.kbk.IdBudget
                                       });

            foreach (var kbk in kbks.Distinct())
            {
                var newKbk = new ActivityOfSBP_ActivityResourceMaintenance()
                {
                    IdOwner = kbk.IdOwner,
                    IdMaster = kbk.IdMaster,
                    IdBranchCode = kbk.IdBranchCode,
                    IdCodeSubsidy = kbk.IdCodeSubsidy,
                    IdDEK = kbk.IdDEK,
                    IdDFK = kbk.IdDFK,
                    IdDKR = kbk.IdDKR,
                    IdExpenseObligationType = kbk.IdExpenseObligationType,
                    IdFinanceSource = kbk.IdFinanceSource,
                    IdKCSR = kbk.IdKCSR,
                    IdKOSGU = kbk.IdKOSGU,
                    IdKFO = kbk.IdKFO,
                    IdKVR = kbk.IdKVR,
                    IdKVSR = kbk.IdKVSR,
                    IdRZPR = kbk.IdRZPR,
                    IdOKATO = kbk.IdOKATO,
                    IdAuthorityOfExpenseObligation = kbk.IdAuthorityOfExpenseObligation
                };

                context.ActivityOfSBP_ActivityResourceMaintenance.Add(newKbk);

                var values = gkbktrim.Where(g =>
                                            g.kbk.IdOwner == kbk.IdOwner &&
                                            g.kbk.IdMaster == kbk.IdMaster &&
                                            g.kbk.IdBranchCode == kbk.IdBranchCode &&
                                            g.kbk.IdCodeSubsidy == kbk.IdCodeSubsidy &&
                                            g.kbk.IdDEK == kbk.IdDEK &&
                                            g.kbk.IdDFK == kbk.IdDFK &&
                                            g.kbk.IdDKR == kbk.IdDKR &&
                                            g.kbk.IdExpenseObligationType == kbk.IdExpenseObligationType &&
                                            g.kbk.IdFinanceSource == kbk.IdFinanceSource &&
                                            g.kbk.IdKCSR == kbk.IdKCSR &&
                                            g.kbk.IdKOSGU == kbk.IdKOSGU &&
                                            g.kbk.IdKFO == kbk.IdKFO &&
                                            g.kbk.IdKVR == kbk.IdKVR &&
                                            g.kbk.IdKVSR == kbk.IdKVSR &&
                                            g.kbk.IdRZPR == kbk.IdRZPR &&
                                            g.kbk.IdOKATO == kbk.IdOKATO &&
                                            g.kbk.IdAuthorityOfExpenseObligation == kbk.IdAuthorityOfExpenseObligation &&
                                            g.kbk.IdRZPR == kbk.IdRZPR &&
                                            g.kbk.IdBudget == kbk.IdBudget);

                foreach (var value in values)
                {
                    var newValue = new ActivityOfSBP_ActivityResourceMaintenance_Value()
                    {
                        IdOwner = value.kbk.IdOwner,
                        Master = newKbk,
                        IdHierarchyPeriod = value.kbk.IdHierarchyPeriod,
                        Value = value.Value,
                        AdditionalValue = value.AdditionalValue
                    };
                    context.ActivityOfSBP_ActivityResourceMaintenance_Value.Add(newValue);
                }
            }
        }

        private ActivityOfSBP CloneActivityOfSbp(DataContext context, bool hasAdditionalNeed, DateTime? dateCommit,
                                                 int idDocStatus)
        {
            Clone cloner = new Clone(this);
            ActivityOfSBP newDoc = (ActivityOfSBP)cloner.GetResult();
            newDoc.Date = new DateTime(Date.Year, Date.Month, Date.Day);
            newDoc.IdParent = Id;
            newDoc.DateCommit = null;
            newDoc.DateTerminate = null;
            newDoc.DocType = this.DocType;
            newDoc.HasAdditionalNeed = hasAdditionalNeed;
            newDoc.DateCommit = dateCommit;
            newDoc.IdDocStatus = idDocStatus;

            var ids = AllVersionDocIds;
            var rootNum = context.ActivityOfSBP.Single(w => !w.IdParent.HasValue && ids.Contains(w.Id)).Number;
            newDoc.Number = rootNum + "." + ids.Count().ToString(CultureInfo.InvariantCulture);

            newDoc.Header = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();
            return newDoc;
        }


        private void InitScopeDoc(DataContext context, bool withRegProgram = true)
        {
            _arrIdParent = AllVersionDocIds;

            var lRegisters = new List<int>();
            lRegisters.Add(TaskVolume.EntityIdStatic);
            lRegisters.Add(TaskIndicatorQuality.EntityIdStatic);
            lRegisters.Add(Program_ResourceMaintenance.EntityIdStatic);
            lRegisters.Add(AttributeOfSystemGoalElement.EntityIdStatic);
            lRegisters.Add(ValuesGoalTarget.EntityIdStatic);
            lRegisters.Add(GoalTarget.EntityIdStatic);
            lRegisters.Add(Sbor.Registry.SystemGoalElement.EntityIdStatic);
            lRegisters.Add(AttributeOfProgram.EntityIdStatic);
            lRegisters.Add(LimitVolumeAppropriations.EntityIdStatic);
            if (withRegProgram)
            {
                lRegisters.Add(Program.EntityIdStatic);
            }
            _arrRegisters = lRegisters.ToArray();
        }

        private void GetDataDocTables(DataContext context)
        {
            tpActivity = context.ActivityOfSBP_Activity.Where(r => r.IdOwner == this.Id).ToList();

            tpActivity_Value = context.ActivityOfSBP_Activity_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpActivityResourceMaintenance = context.ActivityOfSBP_ActivityResourceMaintenance.Where(r => r.IdOwner == this.Id).ToList();

            tpActivityResourceMaintenance_Value = context.ActivityOfSBP_ActivityResourceMaintenance_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpGoalIndicator = context.ActivityOfSBP_GoalIndicator.Where(r => r.IdOwner == this.Id).ToList();

            tpGoalIndicator_Value = context.ActivityOfSBP_GoalIndicator_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpIndicatorQualityActivity = context.ActivityOfSBP_IndicatorQualityActivity.Where(r => r.IdOwner == this.Id).ToList();

            tpIndicatorQualityActivity_Value = context.ActivityOfSBP_IndicatorQualityActivity_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpResourceMaintenance = context.ActivityOfSBP_ResourceMaintenance.Where(r => r.IdOwner == this.Id).ToList();

            tpResourceMaintenance_Value = context.ActivityOfSBP_ResourceMaintenance_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpSystemGoalElement = context.ActivityOfSBP_SystemGoalElement.Where(r => r.IdOwner == this.Id).ToList();

            tpActivityDemandAndCapacity = context.ActivityOfSBP_ActivityDemandAndCapacity.Where(r => r.IdOwner == this.Id).ToList();

            tpActivityDemandAndCapacity_Value = context.ActivityOfSBP_ActivityDemandAndCapacity_Value.Where(r => r.IdOwner == this.Id).ToList();
        }

        private void CreateMainMoves(DataContext context, IEnumerable<CPair> listId_sges, IEnumerable<CPair> savSge,
                                     Program newProgram,
                                     IEnumerable<ActivityOfSBP_SystemGoalElement> tpSystemGoalElements,
                                     Dictionary<int, SystemGoalElement> dirSystemGoalElement,
                                     IQueryable<SystemGoalElement> sgeOfParents)
        {

            // аннулируем записи регистра с отсутствующими в ТЧ элементами системы полагания
            var list = tpSystemGoalElements.Select(s => s.IdSystemGoal);

            var sgeOfParentsOnDelete = sgeOfParents.Where(r =>
                                                          !list
                                                               .Contains(r.IdSystemGoal) &&
                                                          !r.IdTerminator.HasValue);
            
            foreach (var delSge in sgeOfParentsOnDelete.ToList())
            {
                delSge.Terminate(context, this.Id, EntityId, this.Date);
            }

            // перебираем строки ТЧ ЭлементыСЦ 
            foreach (var sge in savSge)
            {
                var lineTpSge = tpSystemGoalElements.FirstOrDefault(r => r.Id == sge.Id);

                var oldSystemGoalElements = sgeOfParents.Where(r => r.IdSystemGoal == lineTpSge.IdSystemGoal);

                SystemGoalElement newSystemGoalElement;

                // проверяем существование записи по элементу в регистре
                if (oldSystemGoalElements.Any())
                {
                    newSystemGoalElement = oldSystemGoalElements.FirstOrDefault();
                }
                else
                {
                    // если нет - создаем
                    newSystemGoalElement = new SystemGoalElement()
                    {
                        IdRegistrator = this.Id,
                        IdRegistratorEntity = EntityId,
                        IdVersion = this.IdVersion,
                        IdPublicLegalFormation = this.IdPublicLegalFormation,
                        IdSystemGoal = lineTpSge.IdSystemGoal,
                        IdProgram = newProgram.Id,
                        DateCreate = DateTime.Now
                    };
                    context.SystemGoalElement.Add(newSystemGoalElement);
                }

                // сопоставляем ID строки ТЧ с элементов в регистре
                dirSystemGoalElement.Add(sge.Id, newSystemGoalElement);

                context.SaveChanges();

                // создаем атрибуты
                CreateAttributeSystemGoalElement(context, newSystemGoalElement, lineTpSge, sge, dirSystemGoalElement);

                context.SaveChanges();

                // создаем показатели
                CreateGoalTargets(context, sge.Id, newSystemGoalElement, lineTpSge);

                var childSge = listId_sges.Where(r => r.IdParent == sge.Id);
                if (childSge.Any())
                {
                    // если имеются нижестоящие строки, то проделываем всё тоже самое для них
                    CreateMainMoves(context, listId_sges, childSge, newProgram, tpSystemGoalElements, dirSystemGoalElement, sgeOfParents);
                }
            }
        }



        private void CreateTaskDemandCapacityMoves(DataContext context, Program newProgram, Dictionary<int, SystemGoalElement> dirSystemGoalElement, List<TaskVolume> iTaskVolume)
        {
            foreach (var dsge in dirSystemGoalElement)
            {
                var newSystemGoalElement = dsge.Value;

                // обрабатываем данные в поле Спрос
                var oldTaskVolume =
                    context.TaskVolume.Where(
                        r =>
                        _arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId &&
                        r.IdSystemGoalElement == newSystemGoalElement.Id && !r.IdTerminator.HasValue &&
                        r.IdValueType == (byte)ValueType.Demand).ToList();

                var tpActivityV = (from a in tpActivity.Where(t => t.IdMaster == dsge.Key)
                                   join d in tpActivityDemandAndCapacity on a.Id equals d.IdActivity
                                   join v in tpActivityDemandAndCapacity_Value.Where(r => r.Demand.HasValue && r.Demand > 0) on d.Id equals v.IdMaster
                                   select new
                                   {
                                       a,
                                       v
                                   }).ToList();

                var delTaskVolume = oldTaskVolume.ToList().Where(old =>
                                                                 !tpActivityV
                                                                      .Any(line =>
                                                                           Equals(newSystemGoalElement,
                                                                                  old.SystemGoalElement) &&
                                                                           Equals(
                                                                               RegisterMethods.FindTaskCollection(taskCollection,
                                                                                                               line.a
                                                                                                                   .IdActivity,
                                                                                                               line.a
                                                                                                                   .IdContingent),
                                                                               old.TaskCollection) &&
                                                                           Equals(line.a.IndicatorActivity_Volume,
                                                                                  old.IndicatorActivity_Volume) &&
                                                                           Equals(line.v.HierarchyPeriod,
                                                                                  old.HierarchyPeriod) &&
                                                                           line.v.Demand.Value == old.Value));
                // аннулируем лишние записи в регистре
                foreach (var recL in delTaskVolume)
                {
                    var rec = oldTaskVolume.Where(r => r.Id == recL.Id).FirstOrDefault();

                    rec.IdTerminator = this.Id;
                    rec.IdTerminatorEntity = EntityId;
                    rec.DateTerminate = this.Date;
                }

                var newTaskVolumes = tpActivityV.Where(line =>
                                                       !oldTaskVolume
                                                            .Any(old =>
                                                                 Equals(newSystemGoalElement, old.SystemGoalElement) &&
                                                                 Equals(RegisterMethods.FindTaskCollection(taskCollection,
                                                                                                        line.a
                                                                                                            .IdActivity,
                                                                                                        line.a
                                                                                                            .IdContingent),
                                                                        old.TaskCollection) &&
                                                                 Equals(line.a.IndicatorActivity_Volume,
                                                                        old.IndicatorActivity_Volume) &&
                                                                 Equals(line.v.HierarchyPeriod,
                                                                        old.HierarchyPeriod) &&
                                                                 line.v.Demand.Value == old.Value));

                foreach (var lineActivity in newTaskVolumes)
                {
                    TaskCollection curTaskCollection = RegisterMethods.FindTaskCollection(taskCollection,
                                                                                       lineActivity.a.IdActivity,
                                                                                       lineActivity.a.IdContingent);

                    var newTaskVolume = new TaskVolume()
                    {
                        IdRegistrator = this.Id,
                        IdRegistratorEntity = EntityId,
                        DateCreate = DateTime.Now,
                        IdPublicLegalFormation = this.IdPublicLegalFormation,
                        IdVersion = this.IdVersion,
                        IdSBP = lineActivity.a.IdSBP,
                        IdProgram = newProgram.Id,
                        IdSystemGoalElement = newSystemGoalElement.Id,
                        SystemGoalElement = newSystemGoalElement,
                        IdIndicatorActivity_Volume = lineActivity.a.IdIndicatorActivity_Volume,
                        IdHierarchyPeriod = lineActivity.v.IdHierarchyPeriod,
                        Value = lineActivity.v.Demand.Value,
                        ValueType = DbEnums.ValueType.Demand,
                        IdTaskCollection = curTaskCollection.Id
                    };
                    iTaskVolume.Add(newTaskVolume);
                }

                // обрабатываем данные в поле Мощность
                oldTaskVolume =
                    context.TaskVolume.Where(
                        r =>
                        _arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId &&
                        r.IdSystemGoalElement == newSystemGoalElement.Id && !r.IdTerminator.HasValue &&
                        r.IdValueType == (byte)ValueType.Capacity).ToList();

                tpActivityV = (from a in tpActivity.Where(t => t.IdMaster == dsge.Key)
                               join d in tpActivityDemandAndCapacity on a.Id equals d.IdActivity
                               join v in tpActivityDemandAndCapacity_Value.Where(r => r.Capacity.HasValue && r.Capacity > 0) on d.Id equals v.IdMaster
                               select new
                               {
                                   a,
                                   v
                               }).ToList();

                delTaskVolume = oldTaskVolume.ToList().Where(old =>
                                                                 !tpActivityV
                                                                      .Any(line =>
                                                                           Equals(newSystemGoalElement,
                                                                                  old.SystemGoalElement) &&
                                                                           Equals(
                                                                               RegisterMethods.FindTaskCollection(taskCollection,
                                                                                                               line.a
                                                                                                                   .IdActivity,
                                                                                                               line.a
                                                                                                                   .IdContingent),
                                                                               old.TaskCollection) &&
                                                                           Equals(line.a.IndicatorActivity_Volume,
                                                                                  old.IndicatorActivity_Volume) &&
                                                                           Equals(line.v.HierarchyPeriod,
                                                                                  old.HierarchyPeriod) &&
                                                                           line.v.Capacity.Value == old.Value));
                // аннулируем лишние записи в регистре
                foreach (var recL in delTaskVolume)
                {
                    var rec = oldTaskVolume.Where(r => r.Id == recL.Id).FirstOrDefault();

                    rec.IdTerminator = this.Id;
                    rec.IdTerminatorEntity = EntityId;
                    rec.DateTerminate = this.Date;
                }

                newTaskVolumes = tpActivityV.Where(line =>
                                                       !oldTaskVolume
                                                            .Any(old =>
                                                                 Equals(newSystemGoalElement, old.SystemGoalElement) &&
                                                                 Equals(RegisterMethods.FindTaskCollection(taskCollection,
                                                                                                        line.a
                                                                                                            .IdActivity,
                                                                                                        line.a
                                                                                                            .IdContingent),
                                                                        old.TaskCollection) &&
                                                                 Equals(line.a.IndicatorActivity_Volume,
                                                                        old.IndicatorActivity_Volume) &&
                                                                 Equals(line.v.HierarchyPeriod,
                                                                        old.HierarchyPeriod) &&
                                                                 line.v.Capacity.Value == old.Value));

                foreach (var lineActivity in newTaskVolumes)
                {
                    TaskCollection curTaskCollection = RegisterMethods.FindTaskCollection(taskCollection,
                                                                                       lineActivity.a.IdActivity,
                                                                                       lineActivity.a.IdContingent);

                    var newTaskVolume = new TaskVolume()
                    {
                        IdRegistrator = this.Id,
                        IdRegistratorEntity = EntityId,
                        DateCreate = DateTime.Now,
                        IdPublicLegalFormation = this.IdPublicLegalFormation,
                        IdVersion = this.IdVersion,
                        IdSBP = lineActivity.a.IdSBP,
                        IdProgram = newProgram.Id,
                        IdSystemGoalElement = newSystemGoalElement.Id,
                        SystemGoalElement = newSystemGoalElement,
                        IdIndicatorActivity_Volume = lineActivity.a.IdIndicatorActivity_Volume,
                        IdHierarchyPeriod = lineActivity.v.IdHierarchyPeriod,
                        Value = lineActivity.v.Capacity.Value,
                        ValueType = DbEnums.ValueType.Capacity,
                        IdTaskCollection = curTaskCollection.Id
                    };
                    iTaskVolume.Add(newTaskVolume);
                }
            }
        }

        /// <summary>
        /// формируем записи в регистр "Объемы задач"
        /// </summary>
        private void CreateTaskVolumeMoves(DataContext context, Program newProgram, Dictionary<int, SystemGoalElement> dirSystemGoalElement, List<TaskVolume> iTaskVolume)
        {
            // необходимо удалить все записи в регистре которые ссылаются на не существующие в данном документе строки Элементы СЦ
            // на случай если изменился Элемент СЦ - SBORIII-1028

            var oldTaskVolume0 =
                context.TaskVolume.Where(
                    r =>
                    _arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId &&
                    !r.IdTerminator.HasValue &&
                    r.IdValueType == (byte)ValueType.Plan).ToList();

            var delTaskVolume0 = oldTaskVolume0.ToList().Where(old => !old.IdSystemGoalElement.HasValue || !dirSystemGoalElement.Select(s => s.Value.Id).Contains(old.IdSystemGoalElement ?? 0));
            // аннулируем лишние записи в регистре
            foreach (var recL in delTaskVolume0)
            {
                var rec = oldTaskVolume0.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            foreach (var dsge in dirSystemGoalElement)
            {
                var newSystemGoalElement = dsge.Value;

                // обрабатываем данные в поле Значение
                var oldTaskVolume =
                    context.TaskVolume.Where(
                        r =>
                        _arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId &&
                        r.IdSystemGoalElement == newSystemGoalElement.Id && !r.IdTerminator.HasValue &&
                        !r.IsAdditionalNeed && r.IdValueType == (byte)ValueType.Plan).ToList();

                var tpActivityV = (from a in tpActivity.Where(t => t.IdMaster == dsge.Key)
                                   join v in tpActivity_Value.Where(r => r.Value > 0) on a.Id equals v.IdMaster
                                   select new
                                   {
                                       a,
                                       v
                                   }).ToList();

                var delTaskVolume = oldTaskVolume.ToList().Where(old =>
                                                                 !tpActivityV
                                                                      .Any(line =>
                                                                           newSystemGoalElement.Id == old.IdSystemGoalElement &&
                                                                           dirActivity[line.a.Id] == old.IdTaskCollection &&
                                                                           line.a.IdIndicatorActivity_Volume == old.IdIndicatorActivity_Volume &&
                                                                           line.v.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                           line.v.Value == old.Value &&
                                                                           line.a.IdSBP == old.IdSBP
                                                                      ));
                // аннулируем лишние записи в регистре
                foreach (var recL in delTaskVolume)
                {
                    var rec = oldTaskVolume.Where(r => r.Id == recL.Id).FirstOrDefault();

                    rec.IdTerminator = this.Id;
                    rec.IdTerminatorEntity = EntityId;
                    rec.DateTerminate = this.Date;
                }

                var newTaskVolumes = tpActivityV.Where(line =>
                                                       !oldTaskVolume
                                                            .Any(old =>
                                                                 newSystemGoalElement.Id == old.IdSystemGoalElement &&
                                                                 dirActivity[line.a.Id] == old.IdTaskCollection &&
                                                                 line.a.IdIndicatorActivity_Volume ==
                                                                 old.IdIndicatorActivity_Volume &&
                                                                 line.v.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                 line.v.Value == old.Value &&
                                                                 line.a.IdSBP == old.IdSBP
                                                            ));

                foreach (var lineActivity in newTaskVolumes)
                {
                    var newTaskVolume = new TaskVolume()
                    {
                        IdRegistrator = this.Id,
                        IdRegistratorEntity = EntityId,
                        DateCreate = DateTime.Now,
                        IdPublicLegalFormation = this.IdPublicLegalFormation,
                        IdVersion = this.IdVersion,
                        IdSBP = lineActivity.a.IdSBP,
                        IdProgram = newProgram.Id,
                        IdSystemGoalElement = newSystemGoalElement.Id,
                        SystemGoalElement = newSystemGoalElement,
                        IdIndicatorActivity_Volume = lineActivity.a.IdIndicatorActivity_Volume,
                        IdHierarchyPeriod = lineActivity.v.IdHierarchyPeriod,
                        Value = lineActivity.v.Value ?? 0,
                        ValueType = DbEnums.ValueType.Plan,
                        IdTaskCollection = dirActivity[lineActivity.a.Id]
                    };
                    iTaskVolume.Add(newTaskVolume);
                }

                // обрабатываем данные в поле "Доп. потребности"
                oldTaskVolume =
                    context.TaskVolume.Where(
                        r =>
                        _arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId &&
                        r.IdSystemGoalElement == newSystemGoalElement.Id && !r.IdTerminator.HasValue &&
                        r.IsAdditionalNeed && r.IdValueType == (byte)ValueType.Plan).ToList();

                tpActivityV = (from a in tpActivity.Where(t => t.IdMaster == dsge.Key)
                               join v in tpActivity_Value.Where(r => r.AdditionalValue.HasValue) on a.Id equals v.IdMaster
                               select new
                               {
                                   a,
                                   v
                               }).ToList();

                delTaskVolume = oldTaskVolume.ToList().Where(old =>
                                                                 !tpActivityV
                                                                      .Any(line =>
                                                                           newSystemGoalElement.Id == old.IdSystemGoalElement &&
                                                                           dirActivity[line.a.Id] == old.IdTaskCollection &&
                                                                           line.a.IdIndicatorActivity_Volume == old.IdIndicatorActivity_Volume &&
                                                                           line.v.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                           line.v.AdditionalValue.Value == old.Value &&
                                                                           line.a.IdSBP == old.IdSBP));
                // аннулируем лишние записи в регистре
                foreach (var recL in delTaskVolume)
                {
                    var rec = oldTaskVolume.Where(r => r.Id == recL.Id).FirstOrDefault();

                    rec.IdTerminator = this.Id;
                    rec.IdTerminatorEntity = EntityId;
                    rec.DateTerminate = this.Date;
                }

                newTaskVolumes = tpActivityV.Where(line =>
                                                   !oldTaskVolume
                                                        .Any(old =>
                                                             newSystemGoalElement.Id == old.IdSystemGoalElement &&
                                                             dirActivity[line.a.Id] == old.IdTaskCollection &&
                                                             line.a.IdIndicatorActivity_Volume ==
                                                             old.IdIndicatorActivity_Volume &&
                                                             line.v.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                             line.v.AdditionalValue.Value == old.Value &&
                                                             line.a.IdSBP == old.IdSBP));

                foreach (var lineActivity in newTaskVolumes)
                {
                    var newTaskVolume = new TaskVolume()
                    {
                        IdRegistrator = this.Id,
                        IdRegistratorEntity = EntityId,
                        DateCreate = DateTime.Now,
                        IdPublicLegalFormation = this.IdPublicLegalFormation,
                        IdVersion = this.IdVersion,
                        IdSBP = lineActivity.a.IdSBP,
                        IdProgram = newProgram.Id,
                        IdSystemGoalElement = newSystemGoalElement.Id,
                        SystemGoalElement = newSystemGoalElement,
                        IdIndicatorActivity_Volume = lineActivity.a.IdIndicatorActivity_Volume,
                        IdHierarchyPeriod = lineActivity.v.IdHierarchyPeriod,
                        Value = lineActivity.v.AdditionalValue.Value,
                        IsAdditionalNeed = true,
                        ValueType = DbEnums.ValueType.Plan,
                        IdTaskCollection = dirActivity[lineActivity.a.Id]
                    };
                    iTaskVolume.Add(newTaskVolume);
                }
            }
        }

        /// <summary>
        /// формируем записи в регистр "Показатели качества задач"
        /// </summary>
        private void CreateTaskIndicatorQualityMoves(DataContext context, Program newProgram, List<TaskIndicatorQuality> iTaskIndicatorQuality)
        {
            // формируем обычные записи по значению поля Значение

            var oldTaskIndicatorQuality = context.TaskIndicatorQuality.Where(r => _arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId && !r.IdTerminator.HasValue && !r.IsAdditionalNeed).ToList();

            var tpActivityOfSBP_IndicatorQualityActivityV =
                tpIndicatorQualityActivity.Join(tpIndicatorQualityActivity_Value, a => a.Id, v => v.IdMaster,
                                                (a, v) => new { a, v }).ToList();

            var delTaskIndicatorQuality = oldTaskIndicatorQuality.ToList().Where(old =>
                                                                                 !tpActivityOfSBP_IndicatorQualityActivityV
                                                                                      .Any(line =>
                                                                                           dirActivity[line.a.IdMaster] == old.IdTaskCollection &&
                                                                                           line.v.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                                           line.a.IdIndicatorActivity == old.IdIndicatorActivity_Quality &&
                                                                                           line.v.Value == old.Value &&
                                                                                           line.a.Master.IdSBP == old.IdSBP &&
                                                                                           old.ValueType == ValueType.Plan));

            foreach (var recL in delTaskIndicatorQuality)
            {

                var rec = oldTaskIndicatorQuality.FirstOrDefault(r => r.Id == recL.Id);

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            var newTaskIndicatorQualitys = tpActivityOfSBP_IndicatorQualityActivityV.Where(line =>
                                                                                            !oldTaskIndicatorQuality
                                                                                                    .Any(old =>
                                                                                                       dirActivity[line.a.IdMaster] == old.IdTaskCollection &&
                                                                                                       line.v.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                                                       line.a.IdIndicatorActivity == old.IdIndicatorActivity_Quality &&
                                                                                                       line.v.Value == old.Value &&
                                                                                                       line.a.Master.IdSBP == old.IdSBP &&
                                                                                                       old.ValueType == ValueType.Plan));


            foreach (var line in newTaskIndicatorQualitys)
            {
                var newTaskIndicatorQuality = new TaskIndicatorQuality()
                {
                    IdRegistrator = this.Id,
                    IdRegistratorEntity = EntityId,
                    DateCreate = DateTime.Now,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdVersion = this.IdVersion,
                    IdSBP = line.a.Master.IdSBP,
                    IdProgram = newProgram.Id,
                    IdIndicatorActivity_Quality = line.a.IdIndicatorActivity,
                    IdHierarchyPeriod = line.v.IdHierarchyPeriod,
                    Value = line.v.Value ?? 0,
                    ValueType = DbEnums.ValueType.Plan,
                    IdTaskCollection = dirActivity[line.a.IdMaster]
                };
                iTaskIndicatorQuality.Add(newTaskIndicatorQuality);
            }

            // формируем записи по значению поля Доп. потребности

            oldTaskIndicatorQuality = context.TaskIndicatorQuality.Where(r => _arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId && !r.IdTerminator.HasValue && r.IsAdditionalNeed).ToList();

            tpActivityOfSBP_IndicatorQualityActivityV =
                tpIndicatorQualityActivity.Join(tpIndicatorQualityActivity_Value.Where(r => r.AdditionalValue.HasValue), a => a.Id, v => v.IdMaster,
                                                (a, v) => new { a, v }).ToList();

            delTaskIndicatorQuality = oldTaskIndicatorQuality.ToList().Where(old =>
                                                                             !tpActivityOfSBP_IndicatorQualityActivityV
                                                                                  .Any(line =>
                                                                                       dirActivity[line.a.IdMaster] == old.IdTaskCollection &&
                                                                                       line.v.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                                       line.a.IdIndicatorActivity == old.IdIndicatorActivity_Quality &&
                                                                                       line.v.AdditionalValue.Value == old.Value &&
                                                                                       old.ValueType == ValueType.Plan));

            foreach (var recL in delTaskIndicatorQuality)
            {

                var rec = oldTaskIndicatorQuality.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            newTaskIndicatorQualitys = tpActivityOfSBP_IndicatorQualityActivityV.Where(line =>
                                                    !oldTaskIndicatorQuality
                                                            .Any(old =>
                                                                dirActivity[line.a.IdMaster] == old.IdTaskCollection &&
                                                                line.v.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                line.a.IdIndicatorActivity == old.IdIndicatorActivity_Quality &&
                                                                line.v.AdditionalValue.Value == old.Value &&
                                                                old.ValueType == ValueType.Plan));


            foreach (var line in newTaskIndicatorQualitys)
            {
                var newTaskIndicatorQuality = new TaskIndicatorQuality()
                {
                    IdRegistrator = this.Id,
                    IdRegistratorEntity = EntityId,
                    DateCreate = DateTime.Now,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdVersion = this.IdVersion,
                    IdSBP = line.a.Master.IdSBP,
                    IdProgram = newProgram.Id,
                    IdIndicatorActivity_Quality = line.a.IdIndicatorActivity,
                    IdHierarchyPeriod = line.v.IdHierarchyPeriod,
                    Value = line.v.AdditionalValue.Value,
                    IsAdditionalNeed = true,
                    ValueType = DbEnums.ValueType.Plan,
                    IdTaskCollection = dirActivity[line.a.IdMaster]
                };
                iTaskIndicatorQuality.Add(newTaskIndicatorQuality);
            }
        }

        /// <summary>
        /// формируем записи в регистр "Ресурсное обеспечение программ" - по самому документу 
        /// </summary>
        private void CreateResourceMaintenance(DataContext context, Program newProgram, List<Program_ResourceMaintenance> iProgram_ResourceMaintenance)
        {
            // формируем обычные записи по значению поля Значение

            var tpResourceMaintenanceV = tpResourceMaintenance.Join(
                    tpResourceMaintenance_Value.Where(r => r.Value > 0),
                    a => a.Id, v => v.IdMaster,
                    (a, v) => new { a, v }
                ).ToList();

            var oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(r =>
                    _arrIdParent.Contains(r.IdRegistrator)
                    && r.IdRegistratorEntity == EntityId
                    && !r.IdTerminator.HasValue
                    && !r.IdTaskCollection.HasValue
                    && !r.IsAdditionalNeed
                ).ToList();

            var delProgram_ResourceMaintenances =
                oldProgram_ResourceMaintenances.Where(old =>
                    !tpResourceMaintenanceV.Any(line =>
                        (old.IdFinanceSource ?? 0) == (line.a.IdFinanceSource ?? 0)
                        && old.IdHierarchyPeriod == line.v.IdHierarchyPeriod
                        && old.Value == line.v.Value
                    )
                );

            // аннулируем те записи регистра, которые не находятся по данным в ТЧ
            foreach (var recL in delProgram_ResourceMaintenances)
            {
                var rec = oldProgram_ResourceMaintenances.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            var newProgramResourceMaintenances =
                tpResourceMaintenanceV.Where(o =>
                    !oldProgram_ResourceMaintenances.Any(n =>
                        (o.a.IdFinanceSource ?? 0) == (n.IdFinanceSource ?? 0)
                        && o.v.IdHierarchyPeriod == n.IdHierarchyPeriod
                        && o.v.Value == n.Value
                    )
                );

            // создаем записи регистра по данным в ТЧ, которые не находятся в регистре
            foreach (var rm in newProgramResourceMaintenances)
            {
                var newProgram_ResourceMaintenance = new Program_ResourceMaintenance()
                {
                    IdRegistrator = this.Id,
                    DateCreate = DateTime.Now,
                    IdRegistratorEntity = EntityId,
                    IdVersion = this.IdVersion,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdFinanceSource = rm.a.IdFinanceSource,
                    IdHierarchyPeriod = rm.v.IdHierarchyPeriod,
                    ValueType = DbEnums.ValueType.Plan,
                    Value = rm.v.Value ?? 0,
                    IdProgram = newProgram.Id,
                    IsAdditionalNeed = false
                };
                iProgram_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }


            // формируем обычные записи по значению поля Доп. потребность
            
            tpResourceMaintenanceV = tpResourceMaintenance.Join(
                    tpResourceMaintenance_Value.Where(r => r.AdditionalValue > 0),
                    a => a.Id, v => v.IdMaster,
                    (a, v) => new { a, v }
                ).ToList();

            oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(r =>
                    _arrIdParent.Contains(r.IdRegistrator)
                    && r.IdRegistratorEntity == EntityId
                    && !r.IdTerminator.HasValue
                    && !r.IdTaskCollection.HasValue
                    && r.IsAdditionalNeed
                ).ToList();

            delProgram_ResourceMaintenances =
                oldProgram_ResourceMaintenances.Where(old =>
                    !tpResourceMaintenanceV.Any(line =>
                        (old.IdFinanceSource ?? 0) == (line.a.IdFinanceSource ?? 0)
                        && old.IdHierarchyPeriod == line.v.IdHierarchyPeriod
                        && old.Value == line.v.AdditionalValue
                    )
                );

            // аннулируем те записи регистра, которые не находятся по данным в ТЧ
            foreach (var recL in delProgram_ResourceMaintenances)
            {
                var rec = oldProgram_ResourceMaintenances.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            newProgramResourceMaintenances =
                tpResourceMaintenanceV.Where(o =>
                    !oldProgram_ResourceMaintenances.Any(n =>
                        (o.a.IdFinanceSource ?? 0) == (n.IdFinanceSource ?? 0)
                        && o.v.IdHierarchyPeriod == n.IdHierarchyPeriod
                        && o.v.AdditionalValue == n.Value
                    )
                );

            // создаем записи регистра по данным в ТЧ, которые не находятся в регистре
            foreach (var rm in newProgramResourceMaintenances)
            {
                var newProgram_ResourceMaintenance = new Program_ResourceMaintenance()
                {
                    IdRegistrator = this.Id,
                    DateCreate = DateTime.Now,
                    IdRegistratorEntity = EntityId,
                    IdVersion = this.IdVersion,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdFinanceSource = rm.a.IdFinanceSource,
                    IdHierarchyPeriod = rm.v.IdHierarchyPeriod,
                    ValueType = DbEnums.ValueType.Plan,
                    Value = rm.v.AdditionalValue ?? 0,
                    IdProgram = newProgram.Id,
                    IsAdditionalNeed = true
                };
                iProgram_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }

        }

        /// <summary>
        /// формируем записи в регистр "Ресурсное обеспечение программ" - по мероприятиям
        /// </summary>
        private void CreateActivityResourceMaintenance(DataContext context, Program newProgram, List<Program_ResourceMaintenance> iProgram_ResourceMaintenance)
        {
            // формируем обычные записи по значению поля Значение

            var tpResourceMaintenanceV = tpActivityResourceMaintenance.Where(w => !w.IdBudget.HasValue).Join(
                tpActivityResourceMaintenance_Value.Where(r => r.Value > 0),
                a => a.Id, v => v.IdMaster,
                (a, v) => new { a, v }
            ).ToList();

            var oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(r =>
                    _arrIdParent.Contains(r.IdRegistrator)
                    && !r.IdTerminator.HasValue
                    && r.IdRegistratorEntity == EntityId
                    && r.IdTaskCollection.HasValue
                    && !r.IsAdditionalNeed
                ).ToList();

            var delProgram_ResourceMaintenances =
                oldProgram_ResourceMaintenances.Where(old =>
                    !tpResourceMaintenanceV.Any(line =>
                        (old.IdFinanceSource ?? 0) == (line.a.IdFinanceSource ?? 0)
                        && old.IdHierarchyPeriod == line.v.IdHierarchyPeriod
                        && old.Value == line.v.Value
                        && old.IdTaskCollection == dirActivity[line.a.IdMaster]
                     )
                );

            // аннулируем те записи регистра, которые не находятся по данным в ТЧ
            foreach (var recL in delProgram_ResourceMaintenances)
            {
                var rec = oldProgram_ResourceMaintenances.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            var newProgramResourceMaintenances =
                tpResourceMaintenanceV.Where(line =>
                    !oldProgram_ResourceMaintenances.Any(old =>
                        (old.IdFinanceSource ?? 0) == (line.a.IdFinanceSource ?? 0)
                        && old.IdHierarchyPeriod == line.v.IdHierarchyPeriod
                        && old.Value == line.v.Value
                        && old.IdTaskCollection == dirActivity[line.a.IdMaster]
                    )
                );

            // создаем записи регистра по данным в ТЧ, которые не находятся в регистре
            foreach (var rm in newProgramResourceMaintenances)
            {
                var newProgram_ResourceMaintenance = new Program_ResourceMaintenance()
                {
                    IdRegistrator = this.Id,
                    DateCreate = DateTime.Now,
                    IdRegistratorEntity = EntityId,
                    IdVersion = this.IdVersion,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdFinanceSource = rm.a.IdFinanceSource,
                    IdHierarchyPeriod = rm.v.IdHierarchyPeriod ?? 0,
                    ValueType = DbEnums.ValueType.Plan,
                    Value = rm.v.Value ?? 0,
                    IdProgram = newProgram.Id,
                    IdTaskCollection = dirActivity[rm.a.IdMaster]
                };
                iProgram_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }

            // формируем обычные записи по значению поля Доп. потребность

            tpResourceMaintenanceV = tpActivityResourceMaintenance.Where(w => !w.IdBudget.HasValue).Join(
                tpActivityResourceMaintenance_Value.Where(r => r.AdditionalValue.HasValue),
                a => a.Id, v => v.IdMaster,
                (a, v) => new { a, v }
            ).ToList();

            oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(r =>
                    _arrIdParent.Contains(r.IdRegistrator)
                    && !r.IdTerminator.HasValue
                    && r.IdRegistratorEntity == EntityId
                    && r.IdTaskCollection.HasValue
                    && r.IsAdditionalNeed
                ).ToList();

            delProgram_ResourceMaintenances =
                oldProgram_ResourceMaintenances.Where(old =>
                    !tpResourceMaintenanceV.Any(line =>
                        (old.IdFinanceSource ?? 0) == (line.a.IdFinanceSource ?? 0)
                        && old.IdHierarchyPeriod == line.v.IdHierarchyPeriod
                        && old.Value == line.v.AdditionalValue.Value
                        && old.IdTaskCollection == dirActivity[line.a.IdMaster]
                     )
                );

            // аннулируем те записи регистра, которые не находятся по данным в ТЧ
            foreach (var recL in delProgram_ResourceMaintenances)
            {
                var rec = oldProgram_ResourceMaintenances.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            newProgramResourceMaintenances =
                tpResourceMaintenanceV.Where(line =>
                    !oldProgram_ResourceMaintenances.Any(old =>
                        (old.IdFinanceSource ?? 0) == (line.a.IdFinanceSource ?? 0)
                        && old.IdHierarchyPeriod == line.v.IdHierarchyPeriod
                        && old.Value == line.v.AdditionalValue.Value
                        && old.IdTaskCollection == dirActivity[line.a.IdMaster]
                    )
                );

            // создаем записи регистра по данным в ТЧ, которые не находятся в регистре
            foreach (var rm in newProgramResourceMaintenances)
            {
                var newProgram_ResourceMaintenance = new Program_ResourceMaintenance()
                {
                    IdRegistrator = this.Id,
                    DateCreate = DateTime.Now,
                    IdRegistratorEntity = EntityId,
                    IdVersion = this.IdVersion,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdFinanceSource = rm.a.IdFinanceSource,
                    IdHierarchyPeriod = rm.v.IdHierarchyPeriod ?? 0,
                    ValueType = DbEnums.ValueType.Plan,
                    Value = rm.v.AdditionalValue.Value,
                    IsAdditionalNeed = true,
                    IdProgram = newProgram.Id,
                    IdTaskCollection = dirActivity[rm.a.IdMaster]
                };
                iProgram_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }
        }

        /// <summary>
        /// формируем записи в регистр "Объемы финансовых средств"
        /// </summary>
        private void CreateLimitVolumeAppropriations(DataContext context, Program newProgram, List<LimitVolumeAppropriations> iLimitVolumeAppropriations)
        {

            var currentBudgetId = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id;
            var RegCreate = new List<LimitVolumeAppropriations>();

            // формируем сторнируешие проводки
            if (IdParent.HasValue)
            {
                var prevVersions = this.GetIdAllVersion(context).ToArray();

                var sbpIds = context.ActivityOfSBP_Activity.Where(w => w.IdOwner == this.Id).Select(s => s.IdSBP).Distinct().ToArray();

                var ids = GetIdAllVersionDoc(context);

                var lva = context.LimitVolumeAppropriations.Where(l =>
                                                             l.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                                             l.IdVersion == this.IdVersion &&
                                                             l.IdBudget == currentBudgetId &&
                                                             l.IdValueType == (byte)ValueType.JustifiedGRBS &&
                                                             sbpIds.Contains(l.EstimatedLine.IdSBP) &&
                                                             ids.Contains(l.IdRegistrator) &&
                                                             l.IdRegistratorEntity == EntityIdStatic
                                                             ).ToList();

                // создаем сторнирующие записи регистра по данным которые находятся в регистре

                foreach (var rm in lva)
                {

                    var newLimitVolumeAppropriations = new LimitVolumeAppropriations()
                    {
                        IdPublicLegalFormation = rm.IdPublicLegalFormation,
                        IdVersion = rm.IdVersion,
                        IdBudget = rm.IdBudget,
                        IdEstimatedLine = rm.IdEstimatedLine,
                        IdTaskCollection = rm.IdTaskCollection,
                        //IsIndirectCosts = rm.IsIndirectCosts,
                        IdHierarchyPeriod = rm.IdHierarchyPeriod,
                        IdValueType = rm.IdValueType,
                        IdOKATO = rm.IdOKATO,
                        IdAuthorityOfExpenseObligation = rm.IdAuthorityOfExpenseObligation,
                        //IsMeansAUBU = rm.IsMeansAUBU,
                        HasAdditionalNeed = rm.HasAdditionalNeed,
                        Value = -rm.Value,

                    };
                    RegCreate.Add(newLimitVolumeAppropriations);
                }
            };
            // формируем обычные записи по значению поля Значение

            var tpResourceMaintenanceV = tpActivityResourceMaintenance.Where(w => w.IdBudget.HasValue).Join(
                tpActivityResourceMaintenance_Value.Where(r => r.Value > 0),
                a => a.Id, v => v.IdMaster,
                (a, v) => new { a, v }
            ).ToList();

            var findParamEstimatedLine = new FindParamEstimatedLine
            {
                IdBudget = currentBudgetId,
                IdPublicLegalFormation = IdPublicLegalFormation,
                IdSbp = IdSBP,
                IsCreate = true,
                IsKosgu000 = false,
                IsRequired = false,
                TypeLine = ActivityBudgetaryType.Costs
            };

            var blank = SBP.IdSBPType == (byte)SBPType.GeneralManager ?
                                                                context.SBP_Blank.FirstOrDefault(b => b.IdOwner == IdSBP && b.IdBudget == currentBudgetId && b.IdBlankType == (byte)BlankType.FormationGRBS)
                                                                : context.SBP_Blank.FirstOrDefault(b => b.IdOwner == SBP.IdParent && b.IdBudget == currentBudgetId && b.IdBlankType == (byte)BlankType.FormationGRBS);

            if (blank == null) return;

            var estimatedLines = context.ActivityOfSBP_ActivityResourceMaintenance.Where(w => w.IdOwner == this.Id && w.IdBudget.HasValue).GetLinesId(context, Id, EntityId, blank, findParamEstimatedLine);

            // создаем записи регистра по данным в ТЧ, которые не находятся в регистре
            foreach (var rm in tpResourceMaintenanceV.Where(w => w.v.Value.HasValue))
            {

                var estimatedLineId = estimatedLines[rm.a.Id];

                var newLimitVolumeAppropriations = new LimitVolumeAppropriations()
                {
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdVersion = this.IdVersion,
                    IdBudget = currentBudgetId,
                    IdEstimatedLine = estimatedLineId,
                    IdOKATO = rm.a.IdOKATO,
                    IdAuthorityOfExpenseObligation = rm.a.IdAuthorityOfExpenseObligation,
                    IdTaskCollection = dirActivity[rm.a.IdMaster],
                    IdHierarchyPeriod = rm.v.IdHierarchyPeriod ?? 0,
                    ValueType = DbEnums.ValueType.JustifiedGRBS,
                    Value = rm.v.Value ?? 0,
                    HasAdditionalNeed = false
                };
                RegCreate.Add(newLimitVolumeAppropriations);
            }


            // формируем обычные записи по значению поля Доп. потребность
            // создаем записи регистра по данным в ТЧ, которые не находятся в регистре

            foreach (var rm in tpResourceMaintenanceV.Where(w => w.v.AdditionalValue.HasValue))
            {
                var estimatedLineId = estimatedLines[rm.a.Id];

                var newLimitVolumeAppropriations = new LimitVolumeAppropriations()
                {
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdVersion = this.IdVersion,
                    IdBudget = currentBudgetId,
                    IdEstimatedLine = estimatedLineId,
                    IdOKATO = rm.a.IdOKATO,
                    IdAuthorityOfExpenseObligation = rm.a.IdAuthorityOfExpenseObligation,
                    IdTaskCollection = dirActivity[rm.a.IdMaster],
                    IdHierarchyPeriod = rm.v.IdHierarchyPeriod ?? 0,
                    ValueType = DbEnums.ValueType.JustifiedGRBS,
                    Value = rm.v.AdditionalValue ?? 0,
                    HasAdditionalNeed = true
                };
                RegCreate.Add(newLimitVolumeAppropriations);
            }

            var GroupRegCreate = RegCreate.GroupBy(l => new
            {
                IdPublicLegalFormation = l.IdPublicLegalFormation,
                IdVersion = l.IdVersion,
                IdBudget = l.IdBudget,
                IdEstimatedLine = l.IdEstimatedLine,
                IdTaskCollection = l.IdTaskCollection,
                IdHierarchyPeriod = l.IdHierarchyPeriod,
                ValueType = l.ValueType,
                IdOKATO = l.IdOKATO,
                IdAuthorityOfExpenseObligation = l.IdAuthorityOfExpenseObligation,
                HasAdditionalNeed = l.HasAdditionalNeed
            }).Select(g => new
            {
                IdPublicLegalFormation = this.IdPublicLegalFormation,
                IdVersion = this.IdVersion,
                IdBudget = currentBudgetId,
                IdEstimatedLine = g.Key.IdEstimatedLine,
                IdTaskCollection = g.Key.IdTaskCollection,
                IdHierarchyPeriod = g.Key.IdHierarchyPeriod,
                IdOKATO = g.Key.IdOKATO,
                IdAuthorityOfExpenseObligation = g.Key.IdAuthorityOfExpenseObligation,
                ValueType = g.Key.ValueType,
                HasAdditionalNeed = g.Key.HasAdditionalNeed,
                value = g.Sum(c => c.Value)
            }).Where(s => s.value != 0).ToList();
            foreach (var reg in GroupRegCreate)
            {
                iLimitVolumeAppropriations.Add(new LimitVolumeAppropriations()
                {
                    IdPublicLegalFormation = reg.IdPublicLegalFormation,
                    IdVersion = reg.IdVersion,
                    IdBudget = reg.IdBudget,
                    IdEstimatedLine = reg.IdEstimatedLine,
                    IdTaskCollection = reg.IdTaskCollection,
                    IdHierarchyPeriod = reg.IdHierarchyPeriod,
                    IdOKATO = reg.IdOKATO,
                    IdAuthorityOfExpenseObligation = reg.IdAuthorityOfExpenseObligation,
                    ValueType = reg.ValueType,
                    HasAdditionalNeed = reg.HasAdditionalNeed,
                    IdRegistrator = this.Id,
                    DateCreate = DateTime.Now,
                    IdRegistratorEntity = EntityId,
                    Value = reg.value
                });
            }

        }

        /// <summary>
        /// формируем записи в регистр "Программы"
        /// </summary>
        /// <param name="bNewProgram">0 - программа существует и остаётся прежней; 1 - программа создаётся новая; 2 - программа создается взамен старой</param>
        /// <returns></returns>
        private Program CreateProgram(DataContext context, out byte bNewProgram)
        {
            Program newProgram = null;

            var old_prog = context.Program.Where(r => (r.IdRegistrator == this.Id || _arrIdParent.Contains(r.IdRegistrator)) && r.IdRegistratorEntity == this.EntityId && !r.IdTerminator.HasValue);

            if (old_prog.Any())
            {
                newProgram = old_prog.FirstOrDefault();
                bNewProgram = 0;

                if (IdParent.HasValue)
                {
                    if (newProgram.IdDocType != this.IdDocType)
                    {
                        newProgram.IdTerminator = this.Id;
                        newProgram.IdTerminatorEntity = newProgram.IdRegistratorEntity;

                        bNewProgram = 2;
                    }
                }
            }
            else
            {
                bNewProgram = 1;
            }

            if (bNewProgram > 0)
            {
                newProgram = new Program()
                {
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdVersion = this.IdVersion,
                    IdDocType = this.IdDocType,
                    IdSBP = this.IdSBP,
                    IdRegistratorEntity = this.EntityId,
                    IdRegistrator = this.Id,
                    DateCreate = DateTime.Now
                };
                context.Program.Add(newProgram);
            }

            return newProgram;
        }

        /// <summary>
        /// формируем записи в регистр "Атрибуты элементов СЦ"
        /// </summary>
        private void CreateAttributeSystemGoalElement(DataContext context, SystemGoalElement newSystemGoalElement, ActivityOfSBP_SystemGoalElement lineTpSge, CPair sge, Dictionary<int, SystemGoalElement> dirSystemGoalElement)
        {
            var oldAttributeOfSystemGoalElements =
                context.AttributeOfSystemGoalElement.Where(
                    r => r.IdSystemGoalElement == newSystemGoalElement.Id && !r.IdTerminator.HasValue);

            if (oldAttributeOfSystemGoalElements.Any())
            {
                var oldAttributeOfSystemGoalElement = oldAttributeOfSystemGoalElements.FirstOrDefault();

                var sgpold = oldAttributeOfSystemGoalElement.SystemGoalElement_Parent == null ? 0 : oldAttributeOfSystemGoalElement.SystemGoalElement_Parent.SystemGoal.Id;
                var sgpnew = lineTpSge.Parent == null ? 0 : lineTpSge.Parent.SystemGoal.Id;

                if (oldAttributeOfSystemGoalElement.IdSBP == lineTpSge.IdSBP &&
                    oldAttributeOfSystemGoalElement.IdElementTypeSystemGoal == lineTpSge.IdElementTypeSystemGoal &&
                    oldAttributeOfSystemGoalElement.DateStart == lineTpSge.DateStart &&
                    oldAttributeOfSystemGoalElement.DateEnd == lineTpSge.DateEnd &&
                    sgpold == sgpnew)
                {
                    return;
                }
                else
                {
                    oldAttributeOfSystemGoalElement.IdTerminator = this.Id;
                    oldAttributeOfSystemGoalElement.IdTerminatorEntity = EntityId;
                    oldAttributeOfSystemGoalElement.DateTerminate = this.Date;
                }
            }

            var newAttributeOfSystemGoalElement = new AttributeOfSystemGoalElement()
            {
                IdRegistrator = this.Id,
                DateCreate = DateTime.Now,
                IdRegistratorEntity = EntityId,
                IdVersion = this.IdVersion,
                IdPublicLegalFormation = this.IdPublicLegalFormation,
                IdSystemGoalElement = newSystemGoalElement.Id,
                IdSBP = lineTpSge.IdSBP,
                IdElementTypeSystemGoal = lineTpSge.IdElementTypeSystemGoal ?? 0,
                DateStart = lineTpSge.DateStart ?? DateTime.Now,
                DateEnd = lineTpSge.DateEnd ?? DateTime.Now
            };

            var dir = dirSystemGoalElement.Where(r => r.Key == sge.IdParent);
            if (dir.Any())
            {
                newAttributeOfSystemGoalElement.SystemGoalElement_Parent = dir.FirstOrDefault().Value;
            }
            else if (lineTpSge.Parent != null)
            {
                var lineParent = lineTpSge.Parent;
                var parentInReg = context.SystemGoalElement
                                         .Where(r =>
                                                !r.IdTerminator.HasValue &&
                                                r.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                                r.IdVersion == this.IdVersion &&
                                                r.IdSystemGoal == lineParent.IdSystemGoal &&
                                                context.AttributeOfSystemGoalElement
                                                       .Any(a =>
                                                            !a.IdTerminator.HasValue &&
                                                            a.IdSystemGoalElement == r.Id &&
                                                            a.IdElementTypeSystemGoal ==
                                                            lineParent.IdElementTypeSystemGoal &&
                                                            a.IdSBP == lineParent.IdSBP &&
                                                            a.DateStart == lineParent.DateStart &&
                                                            a.DateEnd == lineParent.DateEnd
                                                    ));
                if (parentInReg.Any())
                {
                    newAttributeOfSystemGoalElement.SystemGoalElement_Parent = parentInReg.FirstOrDefault();
                }
            }

            context.AttributeOfSystemGoalElement.Add(newAttributeOfSystemGoalElement);
        }

        /// <summary>
        /// формируем записи в регистр "Целевые показатели" и "Значения целевых показателей"
        /// </summary>
        private void CreateGoalTargets(DataContext context, int sgeid, SystemGoalElement newSystemGoalElement, ActivityOfSBP_SystemGoalElement lineTpSge)
        {
            var oldGoalTarget = context.GoalTarget.Where(r => r.IdRegistratorEntity == EntityId && _arrIdParent.Contains(r.IdRegistrator) && r.IdSystemGoalElement == newSystemGoalElement.Id && !r.IdTerminator.HasValue);
            var loldGoalTarget = oldGoalTarget.ToList();

            var linegi = tpGoalIndicator.Where(r => r.IdMaster == lineTpSge.Id).ToList();

            var delGoalTarget = loldGoalTarget.Where(old => !linegi.Any(line => line.IdGoalIndicator == old.IdGoalIndicator));
            // аннулируем лишние записи в регистре
            foreach (var recL in delGoalTarget.ToList())
            {
                var rec = context.GoalTarget.Where(r => r.Id == recL.Id).FirstOrDefault();
                rec.Terminate(context, this.Id, EntityId, this.Date);
            }

            var newGoalTargets = linegi.Where(line => !loldGoalTarget.Any(old => old.IdGoalIndicator == line.IdGoalIndicator)).ToList();
            // создаем новые записи в регистр
            foreach (var lineGI in newGoalTargets)
            {
                var newGoalTarget = new GoalTarget()
                {
                    IdRegistrator = this.Id,
                    IdRegistratorEntity = EntityId,
                    IdVersion = this.IdVersion,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    SystemGoalElement = newSystemGoalElement,
                    IdGoalIndicator = lineGI.IdGoalIndicator,
                    DateCreate = DateTime.Now
                };
                context.GoalTarget.Add(newGoalTarget);

                foreach (var lineGIV in tpGoalIndicator_Value.Where(line => line.IdMaster == lineGI.Id).ToList())
                {
                    var newValuesGoalTarget = new ValuesGoalTarget()
                    {
                        IdRegistrator = this.Id,
                        IdRegistratorEntity = EntityId,
                        IdVersion = this.IdVersion,
                        IdPublicLegalFormation = this.IdPublicLegalFormation,
                        GoalTarget = newGoalTarget,
                        IdHierarchyPeriod = lineGIV.IdHierarchyPeriod,
                        Value = lineGIV.Value,
                        ValueType = DbEnums.ValueType.Plan,
                        DateCreate = DateTime.Now
                    };
                    context.ValuesGoalTarget.Add(newValuesGoalTarget);
                }
            }

            context.SaveChanges();

            // обновляем связанный регистр значений у тех записей, которые существовали и должны существовать
            var gt = from r in tpGoalIndicator
                     join m in tpSystemGoalElement on r.IdMaster equals m.Id
                     join g in oldGoalTarget on new
                         {
                             a1 = m.IdSystemGoal,
                             a2 = r.IdGoalIndicator
                         }
                         equals
                         new
                             {
                                 a1 = g.SystemGoalElement.IdSystemGoal,
                                 a2 = g.IdGoalIndicator
                             }
                     select new
                         {
                             ngi = r,
                             ogi = g
                         };

            // по каждому показателю цели
            foreach (var lineGI in gt.ToList())
            {
                // сначала аннулируем те которые отсутствуют в ТЧ
                var tpGIV = tpGoalIndicator_Value.Where(t => t.IdMaster == lineGI.ngi.Id);

                var valuesGoalTargets = context.ValuesGoalTarget.Where(r => r.IdRegistratorEntity == EntityId && _arrIdParent.Contains(r.IdRegistrator) && r.IdGoalTarget == lineGI.ogi.Id && !r.IdTerminator.HasValue);

                var delValuesGoalTarget = valuesGoalTargets.ToList().Where(old =>
                                                        !tpGIV.Any(line =>
                                                                  line.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                  line.Value == old.Value));
                foreach (var recL in delValuesGoalTarget)
                {
                    var rec = valuesGoalTargets.Where(r => r.IdRegistratorEntity == EntityId && _arrIdParent.Contains(r.IdRegistrator) && r.Id == recL.Id).FirstOrDefault();

                    rec.IdTerminator = this.Id;
                    rec.IdTerminatorEntity = EntityId;
                    rec.DateTerminate = this.Date;
                }

                // затем добавляем то которые отсутствуют в регистре
                var insValuesGoalTarget = tpGIV.Where(line =>
                                                        !valuesGoalTargets.Any(old =>
                                                                  line.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                  line.Value == old.Value));

                foreach (var ins in insValuesGoalTarget)
                {
                    var newValuesGoalTarget = new ValuesGoalTarget()
                    {
                        IdRegistrator = this.Id,
                        IdRegistratorEntity = EntityId,
                        IdVersion = this.IdVersion,
                        IdPublicLegalFormation = this.IdPublicLegalFormation,
                        IdGoalTarget = lineGI.ogi.Id,
                        IdHierarchyPeriod = ins.IdHierarchyPeriod,
                        Value = ins.Value,
                        ValueType = DbEnums.ValueType.Plan
                    };
                    context.ValuesGoalTarget.Add(newValuesGoalTarget);
                }
            }
        }

        /// <summary>
        /// формируем записи в регистр "Атрибуты программ"
        /// </summary>
        private void CreateAttrProgram(DataContext context, Program newProgram)
        {
            var attr_prog = context.AttributeOfProgram.Where(
                r => r.IdPublicLegalFormation == this.IdPublicLegalFormation).ToList()
                                   .Where(r => Equals(r.IdProgram, newProgram.Id) && !r.IdTerminator.HasValue);

            var needAttr = false;

            var MainGoals = tpSystemGoalElement.Where(r => r.IsMainGoal);
            if (MainGoals.Any())
            {
                var idSystemGoalMain = MainGoals.FirstOrDefault().IdSystemGoal;

                _mainSystemGoalElement =
                    context.SystemGoalElement.Where(s => s.IdPublicLegalFormation == this.IdPublicLegalFormation && s.IdSystemGoal == idSystemGoalMain).FirstOrDefault();
            }

            if (attr_prog.Any())
            {
                var oldAttributeOfProgram = attr_prog.FirstOrDefault();

                needAttr =
                    oldAttributeOfProgram.IdAnalyticalCodeStateProgram != this.IdAnalyticalCodeStateProgram
                    || !Equals(oldAttributeOfProgram.GoalSystemElement, _mainSystemGoalElement)
                    || oldAttributeOfProgram.Caption != this.Caption
                    || oldAttributeOfProgram.DateStart != this.DateStart
                    || oldAttributeOfProgram.DateEnd != this.DateEnd
                    || oldAttributeOfProgram.GoalSystemElement.IdTerminator.HasValue
                    ;

                if (needAttr)
                {
                    oldAttributeOfProgram.IdTerminator = this.Id;
                    oldAttributeOfProgram.IdTerminatorEntity = EntityId;
                    oldAttributeOfProgram.DateTerminate = this.Date;
                }
            }
            else
            {
                needAttr = true;
            }

            if (needAttr)
            {

                var idParentProgram = 0;
                if (IdMasterDoc.HasValue)
                {
                    var rootMasterDoc = CommonMethods.GetFirstVersionDoc(context, this.MasterDoc, this.MasterDoc.EntityId);

                    var stateprogramEntityId = (new StateProgram()).EntityId;
                    var prog = context.Program.Where(
                        r => r.IdRegistrator == rootMasterDoc.Id && r.IdRegistratorEntity == stateprogramEntityId);
                    if (prog.Any())
                    {
                        var firstOrDefault = prog.FirstOrDefault();
                        if (firstOrDefault != null) idParentProgram = firstOrDefault.Id;
                    }
                }

                var newAttributeOfProgram = new AttributeOfProgram()
                {
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdVersion = IdVersion,
                    IdRegistrator = Id,
                    IdRegistratorEntity = EntityId,
                    DateCreate = DateTime.Now,
                    IdAnalyticalCodeStateProgram = IdAnalyticalCodeStateProgram,
                    Caption = Caption,
                    DateStart = DateStart,
                    DateEnd = DateEnd,
                    GoalSystemElement = _mainSystemGoalElement,
                    IdProgram = newProgram.Id
                };

                if (idParentProgram != 0)
                {
                    newAttributeOfProgram.IdParent = idParentProgram;
                }

                context.AttributeOfProgram.Add(newAttributeOfProgram);
            }
        }


        public struct CPair
        {
            public int Id;
            public int? IdParent;
        }

        public struct CIndicatorValue
        {
            public int IdGoalIndicator;
            public int IdHierarchyPeriod;
            public decimal Value;
        }

        public struct CIndicator
        {
            public int IdMaster;
            public int IdGoalIndicator;
        }

        private void ExecProcessControls(DataContext context)
        {
            ExecuteControl(e => e.Control_0301(context));
            ExecuteControl(e => e.Control_0303(context));
            ExecuteControl(e => e.Control_0304(context));
            ExecuteControl(e => e.Control_0305(context));
            ExecuteControl(e => e.Control_0344(context));
            ExecuteControl(e => e.Control_0306(context));
            ExecuteControl(e => e.Control_0307(context));
            ExecuteControl(e => e.Control_0309(context));
            ExecuteControl(e => e.Control_0330(context));
            ExecuteControl(e => e.Control_0310(context));
            ExecuteControl(e => e.Control_0312(context));
            ExecuteControl(e => e.Control_0313(context));
            ExecuteControl(e => e.Control_0314(context));
            ExecuteControl(e => e.Control_0315(context));
            ExecuteControl(e => e.Control_0316(context));
            ExecuteControl(e => e.Control_0317(context));
            ExecuteControl(e => e.Control_0331(context));
            ExecuteControl(e => e.Control_0346(context));
            ExecuteControl(e => e.Control_0347(context));
            ExecuteControl(e => e.Control_0348(context));
            ExecuteControl(e => e.Control_0350(context));
            ExecuteControl(e => e.Control_0318(context));
            ExecuteControl(e => e.Control_0319(context));
            ExecuteControl(e => e.Control_0329(context));
            ExecuteControl(e => e.Control_0320(context));
            ExecuteControl(e => e.Control_0321(context));
            ExecuteControl(e => e.Control_0332(context));
            ExecuteControl(e => e.Control_0322(context));
            ExecuteControl(e => e.Control_0352(context));
            ExecuteControl(e => e.Control_0323(context));
            ExecuteControl(e => e.Control_0324(context));
            ExecuteControl(e => e.Control_0325(context));
            ExecuteControl(e => e.Control_0328(context));
        }

          #endregion


        #region Implementation of IDocSGE

        /// <summary>
        /// 
        /// </summary>
        public DateTime ParentDate { get { return IdParent.HasValue ?  Parent.Date : Date; } }
        /// <summary>
        /// 
        /// </summary>
        public DateTime ParentDateStart { get { return IdParent.HasValue ? Parent.DateStart : DateStart; } }
        /// <summary>
        /// 
        /// </summary>
        public DateTime ParentDateEnd { get { return IdParent.HasValue ? Parent.DateEnd : DateEnd; } }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ParentDateCommit { get { return IdParent.HasValue ? Parent.DateCommit : DateCommit; } }

        #endregion

        #region Implementation of ISubDocSGE

        [NotMapped]
        public int? IdAnalyticalCodeStateProgramValue { get { return IdAnalyticalCodeStateProgram ?? 0; } set { IdAnalyticalCodeStateProgram = value; } }

        #endregion

        #region Implementation of IColumnFactoryForDenormalizedTablepart

        public ColumnsInfo GetColumns(string tablepartEntityName)
        {
            if (tablepartEntityName == typeof(ActivityOfSBP_GoalIndicator).Name ||
                tablepartEntityName == typeof(ActivityOfSBP_ActivityDemandAndCapacity).Name ||
                tablepartEntityName == typeof(ActivityOfSBP_ResourceMaintenance).Name)
            {
                return GetColumnsFor_SimpleIndicator_Value();
            }
            else if (tablepartEntityName == typeof(ActivityOfSBP_Activity).Name ||
                tablepartEntityName == typeof(ActivityOfSBP_ResourceMaintenance).Name ||
                tablepartEntityName == typeof(ActivityOfSBP_ActivityResourceMaintenance).Name ||
                tablepartEntityName == typeof(ActivityOfSBP_IndicatorQualityActivity).Name)
            {
                return GetColumnsFor_AdditionalIndicator_Value();
            }

            return null;
        }

        private ColumnsInfo GetColumnsFor_SimpleIndicator_Value()
        {
            DataContext db = IoC.Resolve<DbContext>().Cast<DataContext>();

            var columns = new List<PeriodIdCaption>();

            for (int year = this.DateStart.Year; year <= this.DateEnd.Year; year++)
            {
                var period = db.HierarchyPeriod.Single(
                    p => !p.IdParent.HasValue && p.DateStart.Month == 1 && p.DateEnd.Month == 12 && p.DateStart.Year == year);
                columns.Add(new PeriodIdCaption { PeriodId = period.Id, Caption = period.Caption });
            }

            return new ColumnsInfo() {Periods = columns};
        }

        private ColumnsInfo GetColumnsFor_AdditionalIndicator_Value()
        {
            DataContext db = IoC.Resolve<DbContext>().Cast<DataContext>();

            var columns = new List<PeriodIdCaption>();

            for (int year = this.DateStart.Year; year <= this.DateEnd.Year; year++)
            {
                var period = db.HierarchyPeriod.Single(
                    p => !p.IdParent.HasValue && p.DateStart.Month == 1 && p.DateEnd.Month == 12 && p.DateStart.Year == year);
                columns.Add(new PeriodIdCaption { PeriodId = period.Id, Caption = period.Caption });
            }

            if (!HasAdditionalNeed)
            {
                return new ColumnsInfo()
                    {
                        Periods = columns,
                        Resources = new[] {Reflection<ITpAddValue>.Property(ent => ent.Value).Name}
                    };
            }
            else
            {
                return new ColumnsInfo()
                {
                    Periods = columns
                };
            }
        }

        #endregion
    }
}

