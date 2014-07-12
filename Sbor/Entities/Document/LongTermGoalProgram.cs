using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Text;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils;
using Platform.Utils.Extensions;
using Sbor.CommonControls;
using Sbor.Logic.Hierarchy;
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
    public partial class LongTermGoalProgram : ISubDocSGE, IColumnFactoryForDenormalizedTablepart, IClarificationDoc, IDocStatusTerminate, IAddNeed, IPpoVerDoc
    {

        private SystemGoalElement MainSystemGoalElement;

        private List<LongTermGoalProgram_GoalIndicator> tpGoalIndicator;
        private List<LongTermGoalProgram_GoalIndicator_Value> tpGoalIndicator_Value;
        private List<LongTermGoalProgram_ResourceMaintenance> tpResourceMaintenance;
        private List<LongTermGoalProgram_ResourceMaintenance_Value> tpResourceMaintenance_Value;
        private List<LongTermGoalProgram_SystemGoalElement> tpSystemGoalElement;
        private List<LongTermGoalProgram_CoExecutor> tpCoExecutor;
        private List<LongTermGoalProgram_ListSubProgram> tpListSubProgram;
        private List<LongTermGoalProgram_SubProgramResourceMaintenance> tpSubProgramResourceMaintenance;
        private List<LongTermGoalProgram_SubProgramResourceMaintenance_Value> tpSubProgramResourceMaintenance_Value;
        private List<LongTermGoalProgram_Activity> tpActivity;
        private List<LongTermGoalProgram_Activity_Value> tpActivity_Value;
        private List<LongTermGoalProgram_ActivityResourceMaintenance> tpActivityResourceMaintenance;
        private List<LongTermGoalProgram_ActivityResourceMaintenance_Value> tpActivityResourceMaintenance_Value;
        private List<LongTermGoalProgram_IndicatorActivity> tpIndicatorActivity;
        private List<LongTermGoalProgram_IndicatorActivity_Value> tpIndicatorActivity_Value;

        private int[] arrIdParent;
        private int[] arrRegisters;

        public override string ToString()
        {
            return String.Format("{0} № {1} от {2}", this.DocType.Caption, Number, Date.ToString("dd.MM.yyyy"));
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
                var tmp = new List<int>();
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
        public LongTermGoalProgram GetPrevVersionDoc(DataContext context, LongTermGoalProgram curdoc)
        {
            return curdoc.IdParent.HasValue ? context.LongTermGoalProgram.FirstOrDefault(w => w.Id == curdoc.IdParent) : null;
        }

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
            var qTp = context.LongTermGoalProgram_SystemGoalElement.Where(w => w.IdOwner == Id).ToList();

            // чтобы удалять было не напряжно, иерархию изначально удаляем
            foreach (var item in qTp)
            {
                item.IdParent = null;
            }

            // обновляем где нужно признак из другого документа
            var qUpdateItems = qTp.Join(
                qSg, a => a.IdSystemGoal, b => b.sg.Id,
                (a, b) => new { Tp = a, isOtherDocSG = b.isOtherDocSG }
            ).Where(w => w.Tp.FromAnotherDocumentSE != w.isOtherDocSG);
            foreach (var Item in qUpdateItems)
            {
                Item.Tp.FromAnotherDocumentSE = Item.isOtherDocSG;
            }

            // создаем новые записи
            var qNewItems = qSg.Where(w => !qTp.Any(a => a.IdSystemGoal == w.sg.Id));
            foreach (var item in qNewItems)
            {
                context.LongTermGoalProgram_SystemGoalElement.Add(new LongTermGoalProgram_SystemGoalElement()
                {
                    IdOwner = Id,
                    IdSystemGoal = item.sg.Id,
                    FromAnotherDocumentSE = item.isOtherDocSG
                });
            }

            // удаляем устаревшие
            var qDelItems = qTp.Where(w => !qSg.Any(a => a.sg.Id == w.IdSystemGoal));
            foreach (var item in qDelItems)
            {
                context.LongTermGoalProgram_SystemGoalElement.Remove(item);
            }

            context.SaveChanges();

            // теперь удаляем лишние данные для СЦ из другого документа
            var qGi1 = context.LongTermGoalProgram_GoalIndicator.Where(w => w.IdOwner == Id && w.Master.FromAnotherDocumentSE);
            foreach (var item in qGi1)
            {
                context.LongTermGoalProgram_GoalIndicator.Remove(item);
            }

            context.SaveChanges();

            // для записей из нашего документа обновляем вычислемые хранимые поля (в т.ч. восстанавливаем иерархию), показатели, значения показателей
            int[] items = context.LongTermGoalProgram_SystemGoalElement.Where(w => w.IdOwner == Id).Select(s => s.Id).ToArray();
            RefreshData_SystemGoalElement(context, items);
            FillData_GoalIndicator_Value(context, items);
        }

        public void RefreshData_SystemGoalElement(DataContext context, int[] items, bool flag = false)
        {
            if (flag)
                ExecuteControl(e => e.Control_0245(context, items));

            var list = context.LongTermGoalProgram_SystemGoalElement.Where(w => w.IdOwner == Id).Select(s => new { str = s, sg = s.SystemGoal }).ToList();
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
            var qTpSg = context.LongTermGoalProgram_SystemGoalElement.Where(w => items.Contains(w.Id) && !w.FromAnotherDocumentSE);

            // подходящие показатели для обновления
            var qGi = context.SystemGoal_GoalIndicator.Where(w => w.IdVersion == IdVersion)
                             .Join(qTpSg, a => a.IdOwner, b => b.IdSystemGoal, (a, b) => a).ToList();

            // существующие показатели в тч
            var qTpGi = context.LongTermGoalProgram_GoalIndicator.Where(w => w.IdOwner == Id)
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
                context.LongTermGoalProgram_GoalIndicator.Add(new LongTermGoalProgram_GoalIndicator()
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
                context.LongTermGoalProgram_GoalIndicator.Remove(i);
            }

            context.SaveChanges();

            // теперь обновляем значения показателей
            int[] itms = context.LongTermGoalProgram_GoalIndicator.Where(w => w.IdOwner == Id)
                                .Join(qTpSg, a => a.IdMaster, b => b.Id, (a, b) => a)
                                .Select(s => s.Id).ToArray();
            RefreshData_GoalIndicator_Value(context, itms);
        }

        public void RefreshData_GoalIndicator_Value(DataContext context, int[] items)
        {
            // показатели для которых обновляем значения
            var qTpGi = context.LongTermGoalProgram_GoalIndicator.Where(w => items.Contains(w.Id));

            // уже существующие значения показателей
            var qTpGiv = context.LongTermGoalProgram_GoalIndicator_Value.Where(w => w.IdOwner == Id && items.Contains(w.IdMaster)).ToList();

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
                context.LongTermGoalProgram_GoalIndicator_Value.Add(new LongTermGoalProgram_GoalIndicator_Value()
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

            //var qDelOf = context.LongTermGoalProgram_GoalIndicator_Value.Where(r => r.IdOwner == this.Id && !qTpGi.Any(w => w.Id == r.IdMaster));

            foreach (var v in qDelItems)//.Union(qDelOf))
            {
                context.LongTermGoalProgram_GoalIndicator_Value.Remove(v);
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

        #region Контроли

        private void InitMaps(DataContext context)
        {
            if (PublicLegalFormation == null)
                PublicLegalFormation = context.PublicLegalFormation.SingleOrDefault(w => w.Id == IdPublicLegalFormation);
        }

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1500)]
        public void ControlPeriod(DataContext context, ControlType ctType)
        {
            int minYear = context.HierarchyPeriod.Min(c => c.DateStart.Year);
            int maxYear = context.HierarchyPeriod.Max(c => c.DateStart.Year);

            if (DateStart.Year < minYear || DateEnd.Year > maxYear)
                Controls.Throw(string.Format("Срок реализации документа выходит за пределы справочника 'Иерархия периодов' {0}-{1} гг", minYear, maxYear));
        }

        /// <summary>   
        /// Генерация значения поля «Номер» с 1 до n. 
        /// </summary>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert, Sequence.Before, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            InitMaps(context);

            var sc =
                context.LongTermGoalProgram.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                        .Select(s => s.Number).Distinct().ToList();

            Number = CommonMethods.GetNextCode(sc);
        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = -1000)]
        [ControlInitial(ExcludeFromSetup = true)]
        public void AutoSetHeader(DataContext context)
        {
            Header = this.ToString();
        }

        /// <summary>   
        /// Контроль "Проверка срока реализации документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0201")] 
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_0201(DataContext context)
        {
            DocSGEMethod.CommonControl_0101(this);
        }

        /// <summary>   
        /// Контроль "Проверка даты документа"
        /// </summary>
        [ControlInitial(InitialUNK = "0202")]
        public void Control_0202(DataContext context)
        {
            DocSGEMethod.CommonControl_0102(this);
        }

        /// <summary>   
        /// Обработка "Выравнивание табличных частей под срок реализации документа - удаление лишних данных"
        /// </summary> 
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Update | ControlType.Insert, Sequence.After, ExecutionOrder = 30)]
        public void AligningTableOfDates(DataContext context)
        {
            DocSGEMethod.AlignTableOnDates(context, this.Id, LongTermGoalProgram_Activity_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
            DocSGEMethod.AlignTableOnDates(context, this.Id, LongTermGoalProgram_ActivityResourceMaintenance_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
            DocSGEMethod.AlignTableOnDates(context, this.Id, LongTermGoalProgram_GoalIndicator_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
            DocSGEMethod.AlignTableOnDates(context, this.Id, LongTermGoalProgram_IndicatorActivity_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
            DocSGEMethod.AlignTableOnDates(context, this.Id, LongTermGoalProgram_ResourceMaintenance_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
            DocSGEMethod.AlignTableOnDates(context, this.Id, LongTermGoalProgram_SubProgramResourceMaintenance_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
        }

        /// <summary>   
        /// Контроль "Проверка наличия элементов СЦ в документе"
        /// </summary> 
        [ControlInitial(InitialUNK = "0203", InitialSkippable = false, InitialCaption = "Проверка наличия элементов СЦ в документе", InitialManaged = false)]
        public void Control_0203(DataContext context)
        {
            var Msg = "Не указан ни один элемент СЦ, реализующийся в рамках текущего документа.";

            var erD =
                context.LongTermGoalProgram_SystemGoalElement.Where(r => r.IdOwner == this.Id && !r.FromAnotherDocumentSE);

            if (!erD.Any())
                Controls.Throw(Msg);
        }


        /// <summary>   
        /// Контроль "Проверка наличия документа-дубля"
        /// </summary> 
        [ControlInitial(InitialUNK = "0204", InitialSkippable = false, InitialCaption = "Проверка наличия документа-дубля", InitialManaged = false)]
        public void Control_0204(DataContext context)
        {
            var sMsg = "В системе уже имеется документ {0} с версией '{1}', основной целью '{2}' " +
                       "и сроком реализации {3} - {4} гг.<br>" +
                       "{5}<br>" +
                       "Запрещается создавать однотипные документы с одинаковыми реквизитами.";

            var err =
                from d in context.LongTermGoalProgram
                                 .Where(r =>
                                        r.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                        r.IdVersion == this.IdVersion &&
                                        r.IdDocType == this.IdDocType &&
                                        (r.IdDocStatus == DocStatus.Project ||
                                        r.IdDocStatus == DocStatus.Changed ||
                                        r.IdDocStatus == DocStatus.Approved ||
                                        r.IdDocStatus == DocStatus.Denied ||
                                        r.IdDocStatus == DocStatus.CreateDocs))
                                 .ToList()
                                 .Where(r =>
                                        !arrIdParent.Contains(r.Id) &&
                                        r.Id != this.Id)
                join gse in context.LongTermGoalProgram_SystemGoalElement.Where(r => r.IsMainGoal).ToList()
                    on d.Id equals gse.IdOwner
                join gseSelf in context.LongTermGoalProgram_SystemGoalElement.ToList()
                                       .Where(r => r.IdOwner == this.Id && r.IsMainGoal)
                    on gse.IdSystemGoal equals gseSelf.IdSystemGoal
                select new { gse, d };

            var errl = err.ToList().Where(r =>
                                          // проверка пересечения сроков
                                          DocSGEMethod.HasIntersection(this, r.d)).
                           Select(s => new
                               {
                                   g = s.gse.SystemGoal.Caption,
                                   ds = s.d.DateStart,
                                   de = s.d.DateEnd,
                                   c = s.d.ToString()
                               });

            if (errl.Any())
            {
                var ferr = errl.FirstOrDefault();

                Controls.Throw(string.Format(sMsg,
                                                                 this.DocType.Caption,
                                                                 this.Version.Caption,
                                                                 ferr.g,
                                                                 ferr.ds.ToString("dd.MM.yyyy"),
                                                                 ferr.de.ToString("dd.MM.yyyy"),
                                                                 ferr.c
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка вхождения сроков элементов СЦ в срок реализации документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0205")]
        public void Control_0205(DataContext context)
        {
            var sMsg = "Сроки реализации следующих элементов СЦ выходят за пределы срока реализации документа " +
                       "{0} - {1} гг:<br>" +
                       "{2}";
            var err =
                tpSystemGoalElement.Where(r => !r.FromAnotherDocumentSE)
                                   .ToList()
                                   .Where(g => !DocSGEMethod.HasEntrance(this, g))
                                   .Select(g => g.SystemGoal.Caption);

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg,
                                                                 this.DateStart.ToString("dd.MM.yyyy"),
                                                                 this.DateEnd.ToString("dd.MM.yyyy"),
                                                                 err.Aggregate((a, b) => a + "<br>" + b)
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия типа элемента СЦ и документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0206")]
        public void Control_0206(DataContext context)
        {
            var sMsg = "Следующие элементы СЦ не могут реализовываться в рамках документа {0}," +
                       "так как такая связь не соответствует настройкам в справочнике «Типы элементов СЦ»:<br>" +
                       "{1}";

            var err =
                tpSystemGoalElement.Where(r => !r.FromAnotherDocumentSE)
                                   .Where(g => !context.ElementTypeSystemGoal_Document
                                                       .Where(tp => tp.IdOwner == g.IdElementTypeSystemGoal)
                                                       .Select(s => s.IdDocType)
                                                       .Contains(this.IdDocType)
                                               || g.ElementTypeSystemGoal.IdRefStatus != (byte)RefStats.Work)
                                   .Select(g => new
                                   {
                                       t = g.ElementTypeSystemGoal.Caption,
                                       g = g.SystemGoal.Caption
                                   });

            if (err.Any())
            {
                var err0 = err.ToList().Select(s => string.Format("{0} «{1}»", s.t, s.g));
                Controls.Throw(string.Format(sMsg,
                                                                 this.DocType.Caption,
                                                                 err0.Aggregate((a, b) => a + "<br>" + b)
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия модели СЦ в рамках документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0207")]
        public void Control_0207(DataContext context)
        {
            //есть 
            //a0 = Документ .строкаТЧ_ЭлементыСЦ 
            //a1 = a1 .Наименование .Тип
            //a2 = a1 .Вышестоящий .Тип
            //b0 = МодельСЦ (где Статус = В работе)
            //b1 = b0 .Тип 
            //b2 = b0 .Вышестоящий .Тип

            //мы должны найти для a0 такое b0, чтобы a1 = b1 и a2 = b2
            //если не нашли - значит выводим сообщение контроля 
            //"В документе присутствуют связи, которые не соответствуют настроенной Модели СЦ:
            // a2 - a1"

            var sMsg = "В документе присутствуют связи, которые не соответствуют настроенной Модели СЦ:<br>" +
                       "{0}";

            var modelsc = context.GetModelSG(this.IdPublicLegalFormation).ToList();

            var docm = tpSystemGoalElement
                              .Where(r =>
                                     !r.FromAnotherDocumentSE)
                              .Select(s =>
                                      new SGModel()
                                      {
                                          ElementType = s.ElementTypeSystemGoal,
                                          ElementParentType =
                                              (s.IdParent.HasValue ? s.Parent.ElementTypeSystemGoal : null)
                                      });

            var err = docm.Where(r =>
                                 !modelsc
                                      .Any(m =>
                                           m.ElementType == r.ElementType &&
                                           m.ElementParentType == r.ElementParentType));

            if (err.Any())
            {
                var err0 =
                    err.ToList().Distinct()
                       .Select(s => string.Format("{0} - {1}", s.ElementParentType == null ? "<не указан>" : s.ElementParentType.Caption, s.ElementType.Caption));

                Controls.Throw(string.Format(sMsg,
                                                                 err0.Aggregate((a, b) => a + "<br>" + b)
                                                       ));
            }
        }


        /// <summary>   
        /// Контроль "Правильность дерева элементов СЦ в документе"
        /// </summary> 
        [ControlInitial(InitialUNK = "0209")]
        public void Control_0209(DataContext context)
        {
            var sMsg =
                "Для следующих элементов СЦ требуется указать вышестоящий элемент СЦ из текущего или из другого документа СЦ:<br>" +
                "{0}";

            var err =
                tpSystemGoalElement.Where(r => !r.FromAnotherDocumentSE && !r.IdParent.HasValue)
                                   .Select(s => s.SystemGoal.Caption);

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg,
                                                                 err.Aggregate((a, b) => a + "<br>" + b)
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Соответствие сроков реализации нижестоящего элемента с вышестоящим"
        /// </summary> 
        [ControlInitial(InitialUNK = "0210")]
        public void Control_0210(DataContext context)
        {
            var sMsg = "Указаны неверные сроки. " +
                       "Сроки реализации следующих элементов СЦ выходят за пределы сроков их вышестоящих элементов:<br>" +
                       "{0}";

            var err =
                tpSystemGoalElement
                    .Where(r =>
                           !r.FromAnotherDocumentSE &&
                           r.IdParent.HasValue)
                    .ToList()
                    .Where(g => // проверка вхождения сроков
                           !DocSGEMethod.HasEntrance(g, g.Parent))
                    .Select(s =>
                            new
                                {
                                    s = s.SystemGoal.Caption,
                                    ds = s.DateStart,
                                    de = s.DateEnd
                                });

            if (err.Any())
            {
                var err0 = err.ToList().Select(s => string.Format("{0} {1} - {2}", s.s, s.ds.Value.ToString("dd.MM.yyyy"), s.de.Value.ToString("dd.MM.yyyy")));
                Controls.Throw(string.Format(sMsg,
                                                                 err0.Aggregate((a, b) => a + "<br>" + b)
                                                       ));
            }
        }


        /// <summary>   
        /// Контроль "Проверка соответствия модели СЦ между документами"
        /// </summary> 
        public void Control_0208(DataContext context, string errstr)
        {
            var sMsg =
                "У элементов СЦ из текущего документа обнаружены нижестоящие элементы, с которыми нарушается соответствие настроенной Модели СЦ:  <br>" +
                "{0}<br>" +
                "У документов с указанными нижестоящими элементами СЦ будет установлен признак «Требует уточнения».";

            if (errstr != string.Empty)
            {
                if (errstr != string.Empty)
                {
                    Controls.Throw(string.Format(sMsg, errstr));
                }
            }
        }
        /// <summary>   
        /// Контроль "Проверка соответствия сроков реализации с нижестоящими элементами СЦ из других документов"
        /// </summary> 
        public void Control_0211(DataContext context, string errstr)
        {
            var sMsg =
                "У элементов СЦ из текущего документа обнаружены нижестоящие элементы, с которыми нарушается соответствие сроков реализации:  <br>" +
                "{0}<br>" +
                "У документов с указанными нижестоящими элементами СЦ будет установлен признак «Требует уточнения».";

            if (errstr != string.Empty)
            {
                Controls.Throw(string.Format(sMsg, errstr));
            }
        }

        private void LogicControl0208_0211(DataContext context,
                                           out IEnumerable<RegCommLink> docregs0308,
                                           out IEnumerable<RegCommLink> docregs0311,
                                           out string errstr0308,
                                           out string errstr0311)
        {
            IQueryable<SystemGoalElement> regsge;

            var modelsc = (context.GetModelSG(IdPublicLegalFormation)).ToList();

            // находим в регистре Элементы СЦ все записи созданные данным ЭД или его предками
            regsge = DocSGEMethod.GetRegDataOfParentDocs(context, arrIdParent, EntityId, Id);

            // Для каждого элемента из ТЧ Элементы системы целеполагания, у которого флажок «Из другого документа СЦ» = Ложь  или (Тип утверждающего документа  = ШапкаДокумента.Тип документа и Тип реализующего документа <> ШапкаДокумента.Тип документа), 
            var tpsge =
                context.LongTermGoalProgram_SystemGoalElement.Where(
                    r =>
                    r.IdOwner == Id &&
                    (!r.FromAnotherDocumentSE ||
                     r.SystemGoal.IdDocType_CommitDoc == IdDocType &&
                     r.SystemGoal.IdDocType_ImplementDoc != IdDocType))
                       .ToList();

            // Определить в регистре «Элементы СЦ» идентификатор записи с этим элементом: Аннулятор = Пусто 
            // Элемент СЦ = ТЧ Элементы СЦ. Наименование 
            // Регистратор = текущий документ или один из его предков.
            var rsges =
                tpsge.Join(regsge, lsge => lsge.IdSystemGoal, rs => rs.IdSystemGoal, (lsge, rs) => new { lsge, rsge = rs })
                     .ToList();

            // Далее попытаться найти в регистре «Атрибуты элементов СЦ» записи, у которых
            //Аннулятор = Пусто
            //Вышестоящий = найденный элемент в регистре «Элементы СЦ»
            //Регистратор <> текущий документ или один из его предков.

            var attributeOfSystemGoalElements = (from rl in rsges
                                                 join attrchild in context.AttributeOfSystemGoalElement.
                                                                           Where(r =>
                                                                                 r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                 r.IdVersion == IdVersion &&
                                                                                 !r.IdTerminator.HasValue &&
                                                                                 !((arrIdParent.Contains(r.IdRegistrator) || r.IdRegistrator == Id) &&
                                                                                   r.IdRegistratorEntity == Id)
                                                     )
                                                     on rl.rsge.Id equals attrchild.IdSystemGoalElement_Parent
                                                 select new { attrchild, rl.lsge });

            var asgel = attributeOfSystemGoalElements.ToList();

            var err308 = asgel.Where(s =>
                                                                      !modelsc
                                                                           .Any(m =>
                                                                                m.ElementType == s.attrchild.ElementTypeSystemGoal &&
                                                                                m.ElementParentType == s.lsge.ElementTypeSystemGoal));

            docregs0308 = err308.Select(s => new RegCommLink()
            {
                IdRegistrator = s.attrchild.IdRegistrator,
                RegistratorEntity = s.attrchild.RegistratorEntity
            });

            var sb = new StringBuilder();

            foreach (var g in err308.Select(r => r.lsge).Distinct())
            {

                sb.AppendFormat("{0} «{1}» <br>",
                                g.SystemGoal.Caption,
                                g.ElementTypeSystemGoal.Caption
                    );

                var at = err308.Where(r => r.lsge == g)
                               .Select(s => new
                               {
                                   childcaption = s.attrchild.SystemGoalElement.SystemGoal.Caption,
                                   childtype = s.attrchild.ElementTypeSystemGoal
                               })
                               .Select(s => string.Format("{0} «{1}» <br>",
                                                          s.childtype.Caption,
                                                          s.childcaption));

                sb.AppendFormat("{0}<br>", at.Aggregate((a, b) => a + "<br>" + b));
            }

            errstr0308 = sb.ToString();

            var err311 = asgel.Where(r =>
                                     !(r.attrchild.DateStart <= new DateTime(r.lsge.DateEnd.Value.Year, r.lsge.DateEnd.Value.Month, 1) &&
                                       r.attrchild.DateStart >= r.lsge.DateStart) &&
                                     (new DateTime(r.attrchild.DateEnd.Year, r.attrchild.DateEnd.Month, 1) <= new DateTime(r.lsge.DateEnd.Value.Year, r.lsge.DateEnd.Value.Month, 1) &&
                                      new DateTime(r.attrchild.DateEnd.Year, r.attrchild.DateEnd.Month, 1) >= r.lsge.DateStart));

            sb = new StringBuilder();

            foreach (var g in err311.Select(r => r.lsge).Distinct())
            {

                sb.AppendFormat("{0} {1} - {2} гг <br>",
                                g.SystemGoal.Caption,
                                g.SystemGoal.DateStart.ToString("MM.yyyy"),
                                g.SystemGoal.DateEnd.ToString("MM.yyyy"));

                var at = err311.Where(r => r.lsge == g)
                               .Select(s => new
                               {
                                   childcaption = s.attrchild.SystemGoalElement.SystemGoal.Caption,
                                   s.attrchild.DateStart,
                                   s.attrchild.DateEnd
                               })
                               .Select(s => string.Format(" - {0} {1} - {2} гг",
                                                          s.childcaption,
                                                          s.DateStart.ToString("MM.yyyy"),
                                                          s.DateEnd.ToString("MM.yyyy")));

                sb.AppendFormat("{0}<br>", at.Aggregate((a, b) => a + "<br>" + b));
            }

            docregs0311 = err311.Select(s => new RegCommLink()
            {
                IdRegistrator = s.attrchild.IdRegistrator,
                RegistratorEntity = s.attrchild.RegistratorEntity
            });

            errstr0311 = sb.ToString();
        }


        /// <summary>   
        /// Контроль "Проверка наличия элементов СЦ из ТЧ Элементы СЦ в других документах СЦ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0212")]
        public void Control_0212(DataContext context)
        {
            const string msg = "Следующие элементы СЦ уже добавлены в другие документы системы целеполагания:<br>" +
                       "{0}";

            var allVersionIds = AllVersionDocIds;

            var err =
                (from tpsge in tpSystemGoalElement.Where(r => !r.FromAnotherDocumentSE && r.SystemGoal.IdDocType_CommitDoc == this.IdDocType)
                 join rsge in context.SystemGoalElement.Where(r => !r.IdTerminator.HasValue && r.IdVersion == this.IdVersion && r.IdRegistratorEntity == this.EntityId) on tpsge.IdSystemGoal equals rsge.IdSystemGoal
                 where !allVersionIds.Contains(rsge.IdRegistrator)
                 select new
                 {
                     tpsge.SystemGoal,
                     rsge.IdRegistrator,
                     rsge.RegistratorEntity
                 }).ToList().Distinct();

            if (err.Any())
            {
                Controls.Throw(string.Format(msg,
                                                                 err

                                                                     .OrderBy(o => o.SystemGoal.Caption)
                                                                     .Select(s =>
                                                                             string.Format("{0} <br> - {1}",
                                                                                           s.SystemGoal.Caption,
                                                                                           (context.Set<IIdentitied>(s.RegistratorEntity).FirstOrDefault(i => i.Id == s.IdRegistrator))
                                                                                               .ToString()
                                                                                 ))
                                                                     .Aggregate((a, b) => a + "<br>" + b)
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Наличие вышестоящих элементов СЦ в проектных документах"
        /// </summary> 
        [ControlInitial(InitialUNK = "0213")]
        public void Control_0213(DataContext context)
        {
            var sMsg1 =
                "Следующие элементы СЦ не найдены ни в одном проектном или утвержденном документе системы целеполагания:<br>" +
                "{0}";

            var sMsg2 =
                "Следующие элементы СЦ добавлены в несколько документов системы целеполагания. Невозможно определить, с каким элементом СЦ требуется установить связь:<br>" +
                "{0}";


            var tpsge = context.LongTermGoalProgram_SystemGoalElement.Where(r => r.IdOwner == Id && (
                r.FromAnotherDocumentSE
                || (
                    r.SystemGoal.IdDocType_CommitDoc != this.IdDocType
                    && r.SystemGoal.IdDocType_ImplementDoc == this.IdDocType
                )
            )).ToList();

            var ms1 = new StringBuilder();
            var ms2 = new StringBuilder();

            foreach (var tp in tpsge)
            {
                var attributeOfSystemGoalElements =
                    context.AttributeOfSystemGoalElement.Where(r =>
                                                               !r.IdTerminator.HasValue &&
                                                               !r.SystemGoalElement.IdTerminator.HasValue &&
                                                               r.IdVersion == this.IdVersion &&
                                                               r.SystemGoalElement.IdSystemGoal == tp.IdSystemGoal &&
                                                               r.IdElementTypeSystemGoal == tp.IdElementTypeSystemGoal &&
                                                               (!r.IdSBP.HasValue && !tp.IdSBP.HasValue || (r.IdSBP == tp.IdSBP)) &&
                                                               r.DateStart == tp.DateStart &&
                                                               r.DateEnd == tp.DateEnd).ToList();
                if (!attributeOfSystemGoalElements.Any())
                {
                    ms1.AppendFormat("{0} {1} «{2}» <br> {3} - {4} <br> ",
                                     tp.IdSBP.HasValue ? tp.SBP.Caption : "",
                                     tp.ElementTypeSystemGoal.Caption,
                                     tp.SystemGoal.Caption,
                                     tp.DateStart.Value.ToString("dd.MM.yyyy"),
                                     tp.DateEnd.Value.ToString("dd.MM.yyyy"));
                }
                else
                {
                    if (attributeOfSystemGoalElements.Count() > 1)
                    {
                        ms2.AppendFormat("{0} {1} «{2}» <br> {3} - {4} <br> ",
                                         tp.IdSBP.HasValue ? tp.SBP.Caption : "",
                                         tp.ElementTypeSystemGoal.Caption,
                                         tp.SystemGoal.Caption,
                                         tp.DateStart.Value.ToString("dd.MM.yyyy"),
                                         tp.DateEnd.Value.ToString("dd.MM.yyyy"));

                        foreach (var asge in attributeOfSystemGoalElements)
                        {
                            var doc = DocSGEMethod.GetLastDoc_NotInStatus(context, asge.IdRegistratorEntity, asge.IdRegistrator, DocStatus.Draft);

                            ms2.AppendFormat(" - {0}<br>", doc.ToString());
                        }
                    }
                }
            }

            var Msg = string.Empty;
            if (ms1.ToString() != string.Empty)
            {
                Msg = Msg + string.Format(sMsg1, ms1);
            }
            if (ms2.ToString() != string.Empty)
            {
                Msg = Msg + string.Format(sMsg2, ms2);
            }
            if (Msg != string.Empty)
            {
                Controls.Throw(Msg);
            }

        }

        /// <summary>   
        /// Контроль "Наличие целевых показателей"
        /// </summary> 
        [ControlInitial(InitialUNK = "0214", InitialSkippable = true)]
        public void Control_0214(DataContext context)
        {
            var sMsg = "У следующих элементов СЦ отсутствуют целевые показатели:<br>" +
                       "{0}";

            var err =
                tpSystemGoalElement
                    .Where(r =>
                           !r.FromAnotherDocumentSE &&
                           !tpGoalIndicator.Any(t => t.IdMaster == r.Id))
                    .Select(s => s.SystemGoal.Caption);

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg, err.Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Наличие значений у целевых показателей"
        /// </summary> 
        [ControlInitial(InitialUNK = "0215")]
        public void Control_0215(DataContext context)
        {
            var sMsg = "У следующих целевых показателей не задано ни одного значения:<br>" +
                       "{0}";

            var err =
                tpGoalIndicator.Where(r => !tpGoalIndicator_Value.Any(i => i.IdMaster == r.Id))
                               .Join(tpSystemGoalElement, g => g.IdMaster, m => m.Id, (g, m) => new
                               {
                                   sg = m.SystemGoal.Caption,
                                   idsg = m.IdSystemGoal,
                                   a = g.GoalIndicator
                               })
                .Distinct()
                .ToList();

            if (err.Any())
            {
                var ss = new StringBuilder();
                foreach (var lsg in err.Select(s => new { s.idsg, s.sg }).Distinct().OrderBy(o => o.sg))
                {
                    ss.AppendFormat("Элемент СЦ «{0}»<br>", lsg.sg);
                    var las = err
                        .Where(s => s.idsg == lsg.idsg)
                        .Select(s => string.Format(" - {0}", s.a.Caption))
                        .OrderBy(o => o);

                    ss.Append(las.Aggregate((a, b) => a + "<br>" + b) + "<br>");
                }

                Controls.Throw(string.Format(sMsg, ss));
            }
        }

        /// <summary>   
        /// Контроль "Наличие значений целевых показателей, выходящих за пределы срока реализации элемента СЦ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0216")]
        public void Control_0216(DataContext context)
        {
            var sMsg = "У следующих целевых показателей обнаружены значения, выходящие за срок реализации элемента СЦ:<br>" +
                       "{0}";

            var tpGoalIndicatorV = tpGoalIndicator.Join(tpGoalIndicator_Value, a => a.Id, v => v.IdMaster,
                                                        (a, v) => new { a, v }).ToList();

            var err =
                tpGoalIndicatorV.Join(tpSystemGoalElement, tpgiv => tpgiv.a.IdMaster, tpsge => tpsge.Id, (tpgiv, tpsge) => new { tpgiv, tpsge }).ToList().Where(r => !r.tpgiv.v.HierarchyPeriod.HasEntrance(
                    r.tpsge.DateStart.HasValue ? CommonMethods.DateYearStart(r.tpsge.DateStart.Value) : this.DateStart,
                    r.tpsge.DateEnd.HasValue ? CommonMethods.DateYearEnd(r.tpsge.DateEnd.Value) : this.DateEnd))
                                .Distinct()
                                .Select(s =>
                                        new
                                        {
                                            m = s.tpsge,
                                            p = s.tpgiv.a
                                        });

            if (err.Any())
            {
                var ss = new StringBuilder();
                foreach (var lsg in err.Select(s => s.m).Distinct().OrderBy(o => o.SystemGoal.Caption))
                {
                    ss.AppendFormat("Элемент СЦ «{0}»<br>", lsg.SystemGoal.Caption);
                    var las = err
                        .Where(s => s.m == lsg)
                        .Select(s => string.Format(" - {0}", s.p.GoalIndicator.Caption))
                        .OrderBy(o => o);

                    ss.Append(las.Aggregate((a, b) => a + "<br>" + b) + "<br>");
                }

                Controls.Throw(string.Format(sMsg, ss));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия соисполнителей в документе"
        /// </summary> 
        [ControlInitial(InitialUNK = "0217")]
        public void Control_0217(DataContext context)
        {
            var Msg = "В текущем документе не указано ни одного соисполнителя.";

            if (!tpCoExecutor.Any())
                Controls.Throw(Msg);
        }

        /// <summary>   
        /// Контроль "Проверка наличия строк в ТЧ «Ресурсное обеспечение»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0218")]
        public void Control_0218(DataContext context)
        {
            var Msg = "В документе нет ни одной строки в таблице «Ресурсное обеспечение»";

            if (!tpResourceMaintenance.Any())
                Controls.Throw(Msg);
        }

        private static string sMsg0219 =
            "За один период не допускается указывать сумму ресурсного обеспечения в разрезе источников финансирования и без источника финансирования." +
            "Ошибки обнаружены в таблице «{0}» по периодам {1}";

        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения документа за один период в разрезе ИФ и без разреза по ИФ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0219")]
        public void Control_0219(DataContext context)
        {
            var tpResourceMaintenance0 = tpResourceMaintenance;
            var tpResourceMaintenance_Value0 = tpResourceMaintenance_Value;

            CtrlPart0219(tpResourceMaintenance0, tpResourceMaintenance_Value0);
        }

        public static void CtrlPart0219(List<LongTermGoalProgram_ResourceMaintenance> tpResourceMaintenance0, List<LongTermGoalProgram_ResourceMaintenance_Value> tpResourceMaintenance_Value0)
        {
            var resourceMaintenances = tpResourceMaintenance0.Join(tpResourceMaintenance_Value0, a => a.Id,
                                                                   v => v.IdMaster, (a, v) => new {a, v});

            var rm0s = resourceMaintenances.Where(r => !r.a.IdFinanceSource.HasValue);

            if (rm0s.Any())
            {
                var rm0 = rm0s.Select(s => s.v.HierarchyPeriod);

                var errrm = resourceMaintenances.Where(r =>
                                                       r.a.IdFinanceSource.HasValue &&
                                                       rm0.Any(r0 => r.v.HierarchyPeriod.HasIntersection(r0)))
                                                .OrderBy(o => o.v.HierarchyPeriod.DateStart)
                                                .Select(s => s.v.HierarchyPeriod.Caption).Distinct();


                if (errrm.Any())
                {
                    Controls.Throw(string.Format(sMsg0219, "Ресурсное обеспечение",
                                                 errrm.Aggregate((a, b) => a + ", " + b)));
                }
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия в документе подпрограммы ДЦП или мероприятий"
        /// </summary> 
        [ControlInitial(InitialUNK = "0220")]
        public void Control_0220(DataContext context)
        {
            var Msg1 =
                "Необходимо добавить подпрограммы ДЦП в таблицу «Перечень подпрограмм» или мероприятия в таблицу «Мероприятия».";
            var Msg2 =
                "Необходимо добавить мероприятия подпрограммы ДЦП в таблицу «Мероприятия».";

            var f1 = !tpListSubProgram.Any();

            var f2 = !tpActivity.Any();

            if (this.IdDocType == DocType.LongTermGoalProgram)
            {
                if (f1 && f2)
                {
                    Controls.Throw(Msg1);
                }
            }
            else if (this.IdDocType == DocType.SubProgramDGP)
            {
                if (f2)
                {
                    Controls.Throw(Msg2);
                }
            }
        }

        /// <summary>   
        /// Контроль "Правильность дерева элементов СЦ для подпрограмм"
        /// </summary> 
        [ControlInitial(InitialUNK = "0221")]
        public void Control_0221(DataContext context)
        {
            var sMsg1 =
                "Основные цели следующих подпрограмм должны присутствовать в числе конечных элементов таблицы «Элементы СЦ»:<br>" +
                "{0}";

            var sMsg2 =
                "Вышестоящие цели следующих подпрограмм должны присутствовать в числе конечных элементов таблицы «Элементы СЦ»:<br>" +
                "{0}";

            var listtp = tpSystemGoalElement.Where(s => !tpSystemGoalElement.Select(p => p.IdParent).ToList().Contains(s.Id)).Select(s => s.IdSystemGoal).ToList();

            var err1 = from ltp in tpListSubProgram
                      join g in context.SystemGoal
                                       .Where(w =>
                                              w.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                              w.IdDocType_CommitDoc == DocType.LongTermGoalProgram)
                          on ltp.IdSystemGoal equals g.Id
                      where !listtp.Contains(ltp.IdSystemGoal)
                      select ltp;

            var err2 = from ltp in tpListSubProgram
                       join g in context.SystemGoal
                                        .Where(w =>
                                               w.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                               w.IdDocType_CommitDoc == DocType.SubProgramDGP)
                           on ltp.IdSystemGoal equals g.Id
                       where !listtp.Contains(ltp.SystemGoal.IdParent ?? 0)
                       select ltp;

            if (err1.Any())
            {
                Controls.Throw(string.Format(sMsg1, err1
                                                                           .ToList()
                                                                           .Select(s => string.Format(" - {0}, цель «{1}»", s.Caption, s.SystemGoal.Caption))
                                                                           .OrderBy(o => o)
                                                                           .Aggregate((a, b) => a + "<br>" + b)));
            }
            if (err2.Any())
            {
                Controls.Throw(string.Format(sMsg2, err2
                                                                           .ToList()
                                                                           .Select(s => string.Format(" - {0}, вышестоящая цель «{1}»", s.Caption, s.SystemGoal.Parent.Caption))
                                                                           .OrderBy(o => o)
                                                                           .Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия строк в ТЧ «Ресурсное обеспечение подпрограмм» для каждой подпрограмм"
        /// </summary> 
        [ControlInitial(InitialUNK = "0222")]
        public void Control_0222(DataContext context)
        {
            var sMsg =
                "Для следующих подпрограмм не указаны объемы ресурсного обеспечения в таблице «Ресурсное обеспечение подпрограмм»:<br>" +
                "{0}";

            var err = tpListSubProgram.Where(l => !tpSubProgramResourceMaintenance.Any(r => r.IdMaster == l.Id));

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg, err
                                                                           .ToList()
                                                                           .Select(s => s.Caption)
                                                                           .OrderBy(o => o)
                                                                           .Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения подпрограмм за один период в разрезе ИФ и без разреза по ИФ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0223")]
        public void Control_0223(DataContext context)
        {
            foreach (var listSubProgram in tpListSubProgram)
            {
                var tpSubProgramResourceMaintenance0 = tpSubProgramResourceMaintenance.Where(r => r.IdMaster == listSubProgram.Id).ToList();
                var tpSubProgramResourceMaintenance_Value0 = tpSubProgramResourceMaintenance_Value;

                CtrlPart0223(tpSubProgramResourceMaintenance0, tpSubProgramResourceMaintenance_Value0);
            }

        }

        public static void CtrlPart0223(List<LongTermGoalProgram_SubProgramResourceMaintenance> tpSubProgramResourceMaintenance0, List<LongTermGoalProgram_SubProgramResourceMaintenance_Value> tpSubProgramResourceMaintenance_Value0)
        {
            var programResourceMaintenances =
                tpSubProgramResourceMaintenance0.Join(tpSubProgramResourceMaintenance_Value0, a => a.Id, v => v.IdMaster,
                                                      (a, v) => new {a, v});

            var rm_sp_0s = programResourceMaintenances.Where(r => !r.a.IdFinanceSource.HasValue);

            if (rm_sp_0s.Any())
            {
                var rm0 = rm_sp_0s.Select(s => new {s.a.Master, s.v.HierarchyPeriod});

                var errrm = programResourceMaintenances.Where(r =>
                                                              r.a.IdFinanceSource.HasValue &&
                                                              rm0.Any(r0 =>
                                                                      Equals(r.a.Master, r0.Master) &&
                                                                      r.v.HierarchyPeriod.HasIntersection(
                                                                          r0.HierarchyPeriod)))
                                                       .Select(
                                                           s =>
                                                           new
                                                               {
                                                                   prog = s.a.Master,
                                                                   p = s.v.HierarchyPeriod.Caption
                                                               });

                if (errrm.Any())
                {
                    var ms = new StringBuilder();
                    foreach (var prog in errrm.Select(s => s.prog).Distinct().OrderBy(o => o.Caption))
                    {
                        var h =
                            errrm.Where(r => r.prog == prog)
                                 .Select(s => s.p)
                                 .OrderBy(o => o)
                                 .Distinct()
                                 .Aggregate((a, b) => a + ", " + b);
                        ms.AppendFormat(" - {0}, периоды {1}<br>", prog.Caption, h);
                    }

                    Controls.Throw(string.Format(sMsg0219, "Ресурсное обеспечение подпрограмм", ms));
                }
            }
        }

        /// <summary>   
        /// Контроль "Наличие объемов у мероприятий"
        /// </summary> 
        [ControlInitial(InitialUNK = "0224", InitialCaption = "Наличие объемов у мероприятий")]
        public void Control_0224(DataContext context)
        {
            var tpActivity0 = tpActivity;
            var tpActivity_Value0 = tpActivity_Value;

            CtrlPart0224(tpActivity0, tpActivity_Value0);
        }

        public static void CtrlPart0224(List<LongTermGoalProgram_Activity> tpActivity0, List<LongTermGoalProgram_Activity_Value> tpActivity_Value0)
        {
            var err = tpActivity0.Where(a => !tpActivity_Value0.Any(v => v.IdMaster == a.Id)).ToList();

            if (err.Any())
            {
                var ms = new StringBuilder();
                foreach (var el in err.Select(r => r.Master).Distinct().OrderBy(o => o.SystemGoal.Caption))
                {
                    ms.AppendFormat("Элемент СЦ «{0}»<br>", el.SystemGoal.Caption);
                    var act = err.Where(r => Equals(r.Master, el))
                                 .Select(s =>
                                         new
                                             {
                                                 a = s.Activity.Caption,
                                                 c = (s.IdContingent.HasValue ? " - " + s.Contingent.Caption : "")
                                             })
                                 .OrderBy(o => o.a)
                                 .Select(s => string.Format(" - {0}{1}", s.a, s.c)).Distinct();

                    ms.AppendFormat("{0}<br>", act.Aggregate((a, b) => a + "<br>" + b));
                }

                Controls.Throw(string.Format("У следующих мероприятий не указаны значения показателей объема:<br>" +
                                             "{0}", ms));
            }
        }

        /// <summary>   
        /// Контроль "Наличие объемов ресурсного обеспечения у мероприятия"
        /// </summary> 
        [ControlInitial(InitialUNK = "0225")]
        public void Control_0225(DataContext context)
        {
            var sMsg =
                "У следующих мероприятий не указаны объемы ресурсного обеспечения:<br>" +
                "{0}";

            var activityOfSbpActivityResourceMaintenanceV =
                tpActivityResourceMaintenance
                    .Join(tpActivityResourceMaintenance_Value,
                          a => a.Id, v => v.IdMaster, (a, v) => new { a, v });

            var err =
                tpActivity.Where(a => !activityOfSbpActivityResourceMaintenanceV.Any(v => v.a.IdMaster == a.Id))
                          .Join(tpSystemGoalElement,
                                a => a.IdMaster, b => b.Id, (a, b) => new { a, b }).ToList();

            if (err.Any())
            {
                var ms = new StringBuilder();
                foreach (var el in err.Select(r => r.b).Distinct().OrderBy(o => o.SystemGoal.Caption))
                {
                    ms.AppendFormat("Элемент СЦ «{0}»<br>", el.SystemGoal.Caption);
                    var act = err.Where(r => Equals(r.b, el))
                               .Select(s =>
                                       new
                                       {
                                           a = s.a.Activity.Caption,
                                           c = (s.a.IdContingent.HasValue ? " - " + s.a.Contingent.Caption : "")
                                       })
                               .OrderBy(o => o.a)
                               .Select(s => string.Format(" - {0}{1}", s.a, s.c));

                    ms.AppendFormat("{0}<br>", act.Aggregate((a, b) => a + "<br>" + b));
                }

                Controls.Throw(string.Format(sMsg, ms));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения мероприятия за один период в разрезе ИФ и без разреза по ИФ "
        /// </summary> 
        [ControlInitial(InitialUNK = "0226")]
        public void Control_0226(DataContext context)
        {
            foreach (var activity in tpActivity)
            {
                var tpActivityResourceMaintenance0 = tpActivityResourceMaintenance.Where(r => r.IdMaster == activity.Id).ToList();
                var tpActivityResourceMaintenance_Value0 = tpActivityResourceMaintenance_Value;

                CtrlPart0226(tpActivityResourceMaintenance0, tpActivityResourceMaintenance_Value0);
            }
        }

        public static void CtrlPart0226(List<LongTermGoalProgram_ActivityResourceMaintenance> tpActivityResourceMaintenance0, List<LongTermGoalProgram_ActivityResourceMaintenance_Value> tpActivityResourceMaintenance_Value0)
        {
            var activityResourceMaintenances =
                tpActivityResourceMaintenance0.Join(tpActivityResourceMaintenance_Value0, a => a.Id, v => v.IdMaster,
                                                    (a, v) => new {a, v});

            var rma0s = activityResourceMaintenances.Where(r => !r.a.IdFinanceSource.HasValue);
            if (rma0s.Any())
            {
                var rm0 = rma0s.Select(s => new {s.a.Master, s.v.HierarchyPeriod});

                var errrm = activityResourceMaintenances.Where(r =>
                                                               r.a.IdFinanceSource.HasValue &&
                                                               rm0.Any(r0 =>
                                                                       Equals(r.a.Master, r0.Master) &&
                                                                       r.v.HierarchyPeriod.HasIntersection(r0.HierarchyPeriod)))
                                                        .Select(
                                                            s =>
                                                            new
                                                                {
                                                                    e = s.a.Master.Master,
                                                                    a = s.a.Master,
                                                                    p = s.v.HierarchyPeriod.Caption
                                                                });

                if (errrm.Any())
                {
                    var ms = new StringBuilder();
                    foreach (var sg in errrm.Select(s => s.e).Distinct().OrderBy(o => o.SystemGoal.Caption))
                    {
                        ms.AppendFormat("Элемент СЦ «{0}»<br>", sg.SystemGoal.Caption);
                        foreach (
                            var atp in
                                errrm.Where(r => r.e == sg).Select(s => s.a).Distinct().OrderBy(o => o.Activity.Caption)
                            )
                        {
                            ms.AppendFormat(" - {0} - {1}<br>", atp.Activity.Caption,
                                            (atp.IdContingent.HasValue ? atp.Contingent.Caption : ""));

                            var h = errrm.Where(r => r.e == sg && r.a == atp)
                                         .Select(s => s.p)
                                         .Distinct()
                                         .OrderBy(o => o)
                                         .Aggregate((a, b) => a + ", " + b);

                            ms.AppendFormat("Периоды: {0}.<br>", h);
                        }

                        Controls.Throw(string.Format(sMsg0219, ms));
                    }
                }
            }
        }


        /// <summary>   
        /// Контроль "Проверка на равенство объемов финансирования подпрограмм, ВЦП и основных мероприятий с объемами финансирования программы"
        /// </summary> 
        [ControlInitial(InitialUNK = "0227", InitialSkippable = false)]
        public void Control_0227(DataContext context)
        {
            var sMsgs = string.Empty;

            #region Часть 1

            var sMsg =
                "Сумма ресурсного обеспечения подпрограмм и мероприятий не равна сумме ресурсного обеспечения всего документа.<br>" +
                "Неравенство обнаружено по строкам:<br>" +
                "{0}";

            var rm = tpResourceMaintenance.Join(tpResourceMaintenance_Value.Where(r =>
                                                                                  r.HierarchyPeriod.HasEntrance(
                                                                                      CommonMethods.DateYearStart(this.DateStart),
                                                                                      CommonMethods.DateYearEnd(this.DateEnd))),
                                                a => a.Id, v => v.IdMaster,
                                                (a, v) => new { a, v })
                                          .GroupBy(g =>
                                                   new StateProgram.ResPair()
                                                   {
                                                       fs = g.a.FinanceSource,
                                                       hp = g.v.HierarchyPeriod
                                                   })
                                          .Select(s =>
                                                  new StateProgram.KeyVal()
                                                  {
                                                      Key = s.Key,
                                                      Value = s.Sum(ss => ss.v.Value ?? 0)
                                                  });

            var lsp_rm =
                tpSubProgramResourceMaintenance.Join(tpSubProgramResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                     (a, v) => new { a, v })
                                               .GroupBy(g =>
                                                        new StateProgram.ResPair()
                                                        {
                                                            fs = g.a.FinanceSource,
                                                            hp = g.v.HierarchyPeriod
                                                        })
                                               .Select(s =>
                                                       new StateProgram.KeyVal()
                                                       {
                                                           Key = s.Key,
                                                           Value = s.Sum(ss => ss.v.Value ?? 0)
                                                       });

            var dgka_rm =
                tpActivityResourceMaintenance.Join(tpActivityResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                   (a, v) => new { a, v })
                                             .GroupBy(g =>
                                                      new StateProgram.ResPair()
                                                      {
                                                          fs = g.a.FinanceSource,
                                                          hp = g.v.HierarchyPeriod
                                                      })
                                             .Select(s =>
                                                     new StateProgram.KeyVal()
                                                     {
                                                         Key = s.Key,
                                                         Value = s.Sum(ss => ss.v.Value ?? 0)
                                                     })
                                             .Where(r => r.Value > 0);

            var unRm = lsp_rm.Concat(dgka_rm)
                             .GroupBy(g => g.Key)
                             .Select(s =>
                                     new StateProgram.KeyVal()
                                     {
                                         Key = s.Key,
                                         Value = s.Sum(ss => ss.Value)
                                     })
                             .Where(r => r.Value > 0);

            var rm0 = rm.Where(r => !unRm.Any(u => Equals(u.Key.fs, r.Key.fs) && u.Key.hp == r.Key.hp))
                        .Select(s =>
                                new StateProgram.KeyValD()
                                {
                                    Key = s.Key,
                                    Value1 = s.Value,
                                    Value2 = 0
                                })
                        .Where(r => r.Value1 > 0);

            var unRm0 = unRm.Where(r => !rm.Any(u => Equals(u.Key.fs, r.Key.fs) && u.Key.hp == r.Key.hp))
                            .Select(s =>
                                    new StateProgram.KeyValD()
                                    {
                                        Key = s.Key,
                                        Value1 = 0,
                                        Value2 = s.Value
                                    });

            var diffrm = from r in rm
                         join u in unRm on new { r.Key.fs, r.Key.hp } equals new { u.Key.fs, u.Key.hp }
                         where r.Value != u.Value
                         select new StateProgram.KeyValD()
                         {
                             Key = r.Key,
                             Value1 = r.Value,
                             Value2 = u.Value
                         };

            var err = rm0.Concat(unRm0).Concat(diffrm);

            if (err.Any())
            {
                sMsgs = sMsgs + string.Format(sMsg, err.Select(s => s.ToString()).Aggregate((a, b) => a + "<br>" + b));
            }

            #endregion

            #region Часть 2

            if (HasAdditionalNeed)
            {

                sMsg =
                    "Сумма доп.потребностей ресурсного обеспечения подпрограмм и мероприятий не равна сумме доп.потребностей ресурсного обеспечения всего документа.<br>" +
                    "Неравенство обнаружено по строкам:<br>" +
                    "{0}";

                rm = tpResourceMaintenance.Join(tpResourceMaintenance_Value.Where(r =>
                                                                                  r.HierarchyPeriod.HasEntrance(
                                                                                      CommonMethods.DateYearStart(
                                                                                          this.DateStart),
                                                                                      CommonMethods.DateYearEnd(
                                                                                          this.DateEnd))),
                                                a => a.Id, v => v.IdMaster,
                                                (a, v) => new { a, v })
                                          .GroupBy(g =>
                                                   new StateProgram.ResPair()
                                                   {
                                                       fs = g.a.FinanceSource,
                                                       hp = g.v.HierarchyPeriod
                                                   })
                                          .Select(s =>
                                                  new StateProgram.KeyVal()
                                                  {
                                                      Key = s.Key,
                                                      Value = s.Sum(ss => ss.v.AdditionalValue ?? 0)
                                                  });

                lsp_rm =
                    tpSubProgramResourceMaintenance.Join(tpSubProgramResourceMaintenance_Value, a => a.Id,
                                                         v => v.IdMaster,
                                                         (a, v) => new { a, v })
                                                   .GroupBy(g =>
                                                            new StateProgram.ResPair()
                                                            {
                                                                fs = g.a.FinanceSource,
                                                                hp = g.v.HierarchyPeriod
                                                            })
                                                   .Select(s =>
                                                           new StateProgram.KeyVal()
                                                           {
                                                               Key = s.Key,
                                                               Value = s.Sum(ss => ss.v.AdditionalValue ?? 0)
                                                           });

                dgka_rm =
                    tpActivityResourceMaintenance.Join(tpActivityResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                       (a, v) => new { a, v })
                                                 .GroupBy(g =>
                                                          new StateProgram.ResPair()
                                                          {
                                                              fs = g.a.FinanceSource,
                                                              hp = g.v.HierarchyPeriod
                                                          })
                                                 .Select(s =>
                                                         new StateProgram.KeyVal()
                                                         {
                                                             Key = s.Key,
                                                             Value = s.Sum(ss => ss.v.AdditionalValue ?? 0)
                                                         })
                                                 .Where(r => r.Value > 0);

                unRm = lsp_rm.Concat(dgka_rm)
                             .GroupBy(g => g.Key)
                             .Select(s =>
                                     new StateProgram.KeyVal()
                                     {
                                         Key = s.Key,
                                         Value = s.Sum(ss => ss.Value)
                                     })
                             .Where(r => r.Value > 0);

                rm0 = rm.Where(r => !unRm.Any(u => Equals(u.Key.fs, r.Key.fs) && u.Key.hp == r.Key.hp))
                        .Select(s =>
                                new StateProgram.KeyValD()
                                {
                                    Key = s.Key,
                                    Value1 = s.Value,
                                    Value2 = 0
                                })
                        .Where(r => r.Value1 > 0);

                unRm0 = unRm.Where(r => !rm.Any(u => Equals(u.Key.fs, r.Key.fs) && u.Key.hp == r.Key.hp))
                            .Select(s =>
                                    new StateProgram.KeyValD()
                                    {
                                        Key = s.Key,
                                        Value1 = 0,
                                        Value2 = s.Value
                                    });

                diffrm = from r in rm
                         join u in unRm on new { r.Key.fs, r.Key.hp } equals new { u.Key.fs, u.Key.hp }
                         where r.Value != u.Value
                         select new StateProgram.KeyValD()
                         {
                             Key = r.Key,
                             Value1 = r.Value,
                             Value2 = u.Value
                         };

                err = rm0.Concat(unRm0).Concat(diffrm);

                if (err.Any())
                {
                    sMsgs = ((sMsgs != string.Empty) ? sMsgs + "<br>" : "") + string.Format(sMsg, err.Select(s => s.ToString()).Aggregate((a, b) => a + "<br>" + b));
                }
            }

            #endregion

            if (sMsgs != string.Empty)
            {
                Controls.Throw(sMsgs);
            }
        }

        /// <summary>   
        /// Контроль "Наличие показателей качества у мероприятия"
        /// </summary> 
        [ControlInitial(InitialUNK = "0228", InitialSkippable = true)]
        public void Control_0228(DataContext context)
        {
            var sMsg = "У следующих мероприятий не указаны показатели качества:<br>" +
                       "{0}";

            var err = tpActivity.Where(r => !tpIndicatorActivity.Any(i => i.IdMaster == r.Id))
                                .Join(tpSystemGoalElement,
                                      a => a.IdMaster, b => b.Id, (a, b) => new { a, b })
                                .ToList()
                                .Select(s =>
                                        new
                                        {
                                            sg = s.b.SystemGoal.Caption,
                                            idsg = s.b.IdSystemGoal,
                                            a = s.a.Activity.Caption,
                                            c =
                                        (s.a.IdContingent.HasValue ? " - контингент " + s.a.Contingent.Caption : "")
                                        })
                                .Distinct()
                                .ToList();

            if (err.Any())
            {
                var ss = new StringBuilder();
                foreach (var lsg in err.Select(s => new { s.idsg, s.sg }).Distinct().OrderBy(o => o.sg))
                {
                    ss.AppendFormat("Элемент СЦ «{0}»<br>", lsg.sg);
                    var las = err
                        .Where(s => s.idsg == lsg.idsg)
                        .Select(s => string.Format(" - {0}{1}", s.a, s.c))
                        .OrderBy(o => o);

                    ss.Append(las.Aggregate((a, b) => a + "<br>" + b) + "<br>");
                }

                Controls.Throw(string.Format(sMsg, ss));
            }
        }

        /// <summary>   
        /// Контроль "Наличие значений показателей качества у мероприятий"
        /// </summary> 
        [ControlInitial(InitialUNK = "0229")]
        public void Control_0229(DataContext context)
        {
            var sMsg = "У следующих мероприятий не указаны значения показателей качества:<br>" +
                       "{0}";

            var err =
                (from t in tpIndicatorActivity.Where(iqa => !tpIndicatorActivity_Value.Any(r => r.IdMaster == iqa.Id))
                join a in tpActivity on t.IdMaster equals a.Id
                join m in tpSystemGoalElement on a.IdMaster equals m.Id
                                      select new
                                              {
                                                  sg = m.SystemGoal,
                                                  m = a,
                                                  qa = t.IndicatorActivity.Caption
                                              })
                                          .Distinct()
                                          .ToList();

            if (err.Any())
            {
                var ss = new StringBuilder();
                foreach (var lsg in err.Select(s => s.sg).Distinct().OrderBy(o => o.Caption))
                {
                    ss.AppendFormat("Элемент СЦ «{0}»<br><br>", lsg.Caption);

                    var las = err.Where(s => Equals(s.sg, lsg))
                        .Select(s => s.m)
                        .OrderBy(o => o.Activity.Caption);

                    foreach (var la in las)
                    {
                        ss.Append(string.Format("{0}{1}", la.Activity.Caption, (la.IdContingent.HasValue ? " - " + la.Contingent.Caption : "")) + "<br>");

                        var lia =
                            err.Where(r => Equals(r.sg, lsg) && Equals(r.m, la))
                               .Select(s => string.Format("- {0}",s.qa))
                               .OrderBy(o => o)
                               .Distinct();

                        ss.Append(lia.Aggregate((a, b) => a + "<br>" + b) + "<br>");
                    }
                }

                Controls.Throw(string.Format(sMsg, ss));
            }
        }

        /// <summary>   
        /// Контроль "Проверка статуса входящего документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0231")]
        public void Control_0231(DataContext context)
        {
            var ids = AllVersionDocIds;

            var sMsg1 = "Следующие подпрограммы находятся на статусе «Формирование документов». Необходимо перевести их в статусы «Черновик», «Проект» или «Утвержден»:<br>{0}";
            var sMsg2 = "Следующие подпрограммы находятся на статусе «Отказан». Необходимо перевести их в статусы «Черновик», «Проект» или «Утвержден»:<br>{0}";

            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            var subPrograms = context.LongTermGoalProgram_ListSubProgram.Where(r => r.IdOwner == Id).ToList();
            foreach (var sp in subPrograms)
            {
                DocType docTyp = context.DocType.Single(w => w.Id == sp.IdDocType);
                IDocSGE curdoc =
                    DocSGEMethod.GetLeaf_SubDocSGE(context,
                        docTyp.IdEntity, LongTermGoalProgram_SystemGoalElement.EntityIdStatic, sp.IdSystemGoal,
                        x => x.IdDocType == sp.IdDocType && ids.Contains(x.IdMasterDoc ?? 0) // нужного типа, созданный одним из цепочки документов);
                    );

                if (curdoc == null) continue;

                string nameSubProgram = string.Format("- {0}{1}{2}", (sp.SBP == null ? "" : sp.SBP.Caption), " «" + sp.SystemGoal.Caption + "» ", curdoc.Caption);

                if (curdoc.IdDocStatus == DocStatus.CreateDocs)
                {
                    list1.Add(nameSubProgram);
                }

                if (curdoc.IdDocStatus == DocStatus.Denied)
                {
                    list2.Add(nameSubProgram);
                }
            }

            string sMsg = string.Empty;
            if (list1.Any()) sMsg += string.Format(sMsg1, list1.ToString("<br>"));
            if (list2.Any()) sMsg += (string.IsNullOrEmpty(sMsg) ? "" : "<br>") + string.Format(sMsg2, list2.ToString("<br>"));

            if (!string.IsNullOrEmpty(sMsg))
                Controls.Throw(sMsg);
        }

        /// <summary>   
        /// Контроль "Проверка наличия в системе вышестоящего документа на статусе «Проект»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0232")]
        public void Control_0232(DataContext context)
        {
            const string msg = "Вышестоящий документ {0} находится на статусе, отличном от «Проект» или «Утвержден».";

            if (HasMasterDoc == true && IdMasterStateProgram.HasValue)
            {
                var lastRevisionId = CommonMethods.FindLastRevisionId(context, StateProgram.EntityIdStatic, IdMasterStateProgram.Value);
                if (lastRevisionId == 0)
                    throw new PlatformException("Отсутствует вышестоящий документ! Возможные проблемы: 1. dbo.GetLastVersionId, 2. сломан ключ в базе");

                var masterDocument = context.StateProgram.FirstOrDefault(r => r.Id == lastRevisionId);
                if (masterDocument == null)
                    throw new PlatformException("Отсутствует вышестоящий документ! Возможные проблемы: dbo.GetLastVersionId");

                if (masterDocument.IdDocStatus != DocStatus.Project && masterDocument.IdDocStatus != DocStatus.Approved)
                    Controls.Throw(string.Format(msg, masterDocument.Header));
            }
            else if (IdMasterLongTermGoalProgram.HasValue)
            {
                var lastRevisionId = CommonMethods.FindLastRevisionId(context, LongTermGoalProgram.EntityIdStatic, IdMasterLongTermGoalProgram.Value);
                if (lastRevisionId == 0)
                    throw new PlatformException("Отсутствует вышестоящий документ! Возможные проблемы: 1. dbo.GetLastVersionId, 2. сломан ключ в базе");

                var masterDocument = context.LongTermGoalProgram.FirstOrDefault(r => r.Id == lastRevisionId);
                if (masterDocument == null)
                    throw new PlatformException("Отсутствует вышестоящий документ! Возможные проблемы: dbo.GetLastVersionId");

                if (masterDocument.IdDocStatus != DocStatus.Project && masterDocument.IdDocStatus != DocStatus.Approved)
                    Controls.Throw(string.Format(msg, masterDocument.Header));
            }
        }

        /// <summary>   
        /// Контроль "Проверка утверждения элементов из другого документа СЦ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0233")]
        public void Control_0233(DataContext context)
        {
            List<string> list = context.LongTermGoalProgram_SystemGoalElement.Where(w =>
                w.IdOwner == Id 
                && (
                    w.FromAnotherDocumentSE
                    || (w.SystemGoal.IdDocType_CommitDoc != IdDocType && w.SystemGoal.IdDocType_ImplementDoc == IdDocType)
                )
            ).Join(
                context.AttributeOfSystemGoalElement.Where(r =>
                    !r.IdTerminator.HasValue && r.IdVersion == IdVersion
                    && !r.SystemGoalElement.IdTerminator.HasValue && r.SystemGoalElement.IdVersion == IdVersion
                ),
                a => new
                {
                    a.IdSystemGoal,
                    IdElementTypeSystemGoal = (int)a.IdElementTypeSystemGoal,
                    IdSBP = (a.IdSBP ?? 0),
                    DateStart = (DateTime)a.DateStart,
                    DateEnd = (DateTime)a.DateEnd
                },
                b => new
                {
                    b.SystemGoalElement.IdSystemGoal,
                    b.IdElementTypeSystemGoal,
                    IdSBP = (b.IdSBP ?? 0),
                    b.DateStart,
                    b.DateEnd
                },
                (a, b) => b
            ).Where(w => !w.DateCommit.HasValue || Date < w.DateCommit).ToList().Select(s =>
                " - " + s.SystemGoalElement.SystemGoal.Caption + " " + (s.DateCommit.HasValue ? s.DateCommit.Value.ToString("dd.MM.yyyy") : "не утвержден")
            ).ToList();

            if (list.Any())
                Controls.Check(list,
                    "Необходимо скорректировать дату текущего документа." +
                    "Следующие элементы СЦ, которые являются вышестоящими для элементов из текущего документа, не утверждены или утверждены более поздней датой:<br>{0}" +
                    string.Format("<br><br>Дата текущего документа: {0}", Date.ToString("dd.MM.yyyy"))
                );
        }

        /// <summary>   
        /// Контроль "Проверка наличия одинаковых мероприятий в разных программах"
        /// </summary> 
        [ControlInitial(InitialUNK = "0234")]
        public void Control_0234(DataContext context)
        {
            const string sMsg = "Следующие мероприятия уже осуществляются в рамках других программ:<br>" +
                                "{0}<br>";

            var allVersionIds = AllVersionDocIds;

            var err = (from tp in context.LongTermGoalProgram_Activity
                                        .Where(r => r.IdOwner == this.Id)
                                        .Select(s =>
                                                new
                                                    {
                                                        s.Activity,
                                                        s.Contingent,
                                                        s.SBP
                                                    })
                                        .Distinct()
                      join tc in
                          context.TaskCollection.Where(r => r.IdPublicLegalFormation == this.IdPublicLegalFormation)
                          on new
                              {
                                  a = tp.Activity,
                                  c = tp.Contingent
                              }
                          equals new
                              {
                                  a = tc.Activity,
                                  c = tc.Contingent
                              }

                      join tv in context.TaskVolume
                                        .Where(r =>
                                               r.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                               r.IdVersion == this.IdVersion &&
                                               !r.IdTerminator.HasValue &&
                                               r.IdRegistrator != this.Id &&
                                               !allVersionIds.Contains(r.IdRegistrator))
                          on new
                              {
                                  s = tp.SBP,
                                  t = tc.Id
                              }
                          equals new
                              {
                                  s = tv.SBP,
                                  t = tv.IdTaskCollection
                              }
                      join ap in context.AttributeOfProgram.Where(r =>
                                                                  r.IdPublicLegalFormation ==
                                                                  this.IdPublicLegalFormation &&
                                                                  r.IdVersion == this.IdVersion &&
                                                                  !r.IdTerminator.HasValue)
                          on tv.IdProgram equals ap.IdProgram
                      select new
                          {
                              tc,
                              prog = tv.Program,
                              nameprog = ap.Caption
                          }).ToList();

            if (err.Any())
            {
                var ms = new StringBuilder();
                foreach (var prog in err.Select(s => new{s.prog, s.nameprog}).Distinct().OrderBy(o => o.nameprog))
                {
                    ms.AppendFormat("{0} «{1}»<br>", prog.prog.DocType.Caption, prog.nameprog);

                    ms.AppendFormat("{0}<br><br>",
                                    err.Where(r => r.prog == prog.prog)
                                       .Select(
                                           s =>
                                           string.Format(" - {0}{1}", s.tc.Activity.Caption,
                                                         (s.tc.IdContingent.HasValue
                                                              ? " - " + s.tc.Contingent.Caption
                                                              : "")))
                                       .Distinct()
                                       .Aggregate((a, b) => a + "<br>" + b));

                }
                Controls.Throw(string.Format(sMsg, ms));
            }


        }

        /// <summary>   
        /// Контроль "Проверка наличия в документе нескольких элементов СЦ с признаком «Основная цель»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0235")]
        public void Control_0235(DataContext context)
        {
            var sMsg = "В таблице «Элементы СЦ» не должно быть несколько основных целей.";


            var erD = tpSystemGoalElement.Where(r => r.IsMainGoal);

            if (erD.Count() > 1)
            {
                Controls.Throw(sMsg);
            }
        }

        /// <summary>
        /// Контроль "Проверка даты прекращения"
        /// </summary>
        [ControlInitial(InitialUNK = "0238", InitialSkippable = false, InitialManaged = false, InitialCaption = "Проверка даты прекращения")]
        public void Control_0238(DataContext context)
        {
            if (Date > DateTerminate)
            {
                Controls.Throw(string.Format("Дата прекращения не может быть меньше даты утверждения." +
                                           "<br>Дата утверждения: {0}" +
                                           "<br>Дата прекращения: {1}",
                                           Date.ToString("dd.MM.yyyy"),
                                           DateTerminate.HasValue ? DateTerminate.Value.ToString("dd.MM.yyyy"):"не установлена"));
            }
        }

        /// <summary>
        /// Контроль "Проверка использования мероприятий из прекращаемой ДЦП"
        /// </summary>
        [ControlInitial(InitialUNK = "0239", InitialSkippable = false, InitialManaged = false, InitialCaption = "Проверка использования мероприятий из прекращаемой ДЦП")]
        public void Control_0239(DataContext context)
        {
            string sMsg =
                "Для прекращения действия долгосрочной целевой программы необходимо исключить следующие мероприятия из документов «План деятельности» по учреждениям:";

            var tmp1 = (from av in context.LongTermGoalProgram_Activity_Value
                      join a in context.LongTermGoalProgram_Activity.Where(w => w.IdOwner == Id) on av.IdMaster equals a.Id
                      join tv in context.TaskVolume.Where(w => w.IdVersion == this.IdVersion && (w.DateTerminate == null || w.DateTerminate > this.DateTerminate)) on
                        new
                        {
                            a.IdActivity,
                            a.IdContingent,
                            av.IdHierarchyPeriod
                        }
                        equals
                        new
                        {
                            tv.TaskCollection.IdActivity,
                            tv.TaskCollection.IdContingent,
                            tv.IdHierarchyPeriod
                        }
                      select new
                          {
                              action = a.Activity.Caption,
                              contingent = a.Contingent.Caption,
                              period = tv.HierarchyPeriod.Caption,
                              spb = tv.SBP.Caption,

                              regSBP = tv.IdSBP,    //Регистр.СБП
                              actSBP = a.IdSBP      //Мероприятия.Исполнитель 

                          }).ToList();

            tmp1 = tmp1.Where(w=>context.LongTermGoalProgram.GetDescendantsIds(w.actSBP, idS=>idS.Id,idP=>idP.IdParent).Contains(w.regSBP)).ToList();

            var tmp2 = from a in tmp1
                      group a by new { a.action, a.contingent, a.period } into g
                      select new
                      {
                          g.Key.action,
                          g.Key.contingent,
                          g.Key.period,
                          sbps = g.Select(s => s.spb).ToList()
                      };

            if (tmp2.Any())
            {
                foreach (var t in tmp2)
                {
                    sMsg += string.Format("<br><br>{0} - {1}. Период: {2}", t.action, t.contingent, t.period);
                    foreach (var sbp in t.sbps)
                    {
                        sMsg += string.Format("<br>- {0}", sbp);
                    }
                }
                Controls.Throw(sMsg);
            }
        }

        [ControlInitial(InitialUNK = "0240", InitialSkippable = false, InitialManaged = false, InitialCaption = "Проверка реализации мероприятий в других программах")]
        public void Control_0240(DataContext context)
        {
            string sMsg =
                "Нельзя отменить прекращение программы. Следующие мероприятия уже реализуются в рамках других программ: ";

            // Регистр.Регистратор <> текущий документ или документы-предки текущего документа
            var ids = GetIdAllVersionDoc(context);

            var tmp = (from av in context.LongTermGoalProgram_Activity_Value
                        join a in context.LongTermGoalProgram_Activity.Where(w => w.IdOwner == Id) on av.IdMaster equals a.Id
                        join tv in context.TaskVolume.Where(w => w.IdVersion == this.IdVersion && !ids.Contains(w.IdRegistrator) && (w.DateTerminate == null || w.DateTerminate > this.DateTerminate)) on
                          new
                          {
                              a.IdActivity,
                              a.IdContingent,
                              av.IdHierarchyPeriod,
                              a.IdSBP      //Мероприятия.Исполнитель 
                          }
                          equals
                          new
                          {
                              tv.TaskCollection.IdActivity,
                              tv.TaskCollection.IdContingent,
                              tv.IdHierarchyPeriod,
                              tv.IdSBP    //Регистр.СБП
                          }
                        select new
                        {
                            spb = tv.SBP.Caption,
                            action = tv.TaskCollection.Activity.Caption,
                            contingent = tv.TaskCollection.Contingent.Caption,
                            period = tv.HierarchyPeriod.Caption,
                            tv.RegistratorEntity,
                            tv.IdRegistrator,
                        }).ToList();

            if (tmp.Any())
            {
                foreach (var t in tmp)
                {
                    var registratorHeader = context.Set<IIdentitied>(t.RegistratorEntity).FirstOrDefault(f => f.Id == t.IdRegistrator).ToString();
                    sMsg += string.Format("<br>{0} - {1} - {2}. Период: {3} ({4})",
                                t.spb,
                                t.action,
                                t.contingent,
                                t.period,
                                registratorHeader);
                }
                Controls.Throw(sMsg);
            }
        }


        /// <summary>   
        /// Контроль "Очистка доп. потребностей"
        /// </summary> 
        [ControlInitial(InitialSkippable = true, InitialCaption = "Очистка доп. потребностей", InitialUNK = "0241")]
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 50)]
        public void Control_0241(DataContext context, LongTermGoalProgram old)
        {
            var sMsg = "Признак «Вести доп. потребности» отключен. Все доп. потребности в документе будут очищены.";

            if (old.HasAdditionalNeed && !this.HasAdditionalNeed)
            {
                Controls.Throw(sMsg);
            }
        }
        /// <summary>   
        /// Контроль-обработчик "Очистка доп. потребностей"
        /// </summary> 
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 51)]
        public void AutoControl_0241end(DataContext context)
        {
            if (!this.HasAdditionalNeed)
            {
                DocSGEMethod.DeclineAddValueInTp(context, LongTermGoalProgram_SubProgramResourceMaintenance_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, LongTermGoalProgram_ResourceMaintenance_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, LongTermGoalProgram_Activity_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, LongTermGoalProgram_ActivityResourceMaintenance_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, LongTermGoalProgram_IndicatorActivity_Value.EntityIdStatic, this.Id);
            }
        }




        /// <summary>   
        /// Контроль "Проверка элементов СЦ на соответствие реквизитам документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0245", InitialCaption = "Проверка элементов СЦ на соответствие реквизитам документа", InitialSkippable = true)]
        public void Control_0245(DataContext context, int[] items)
        {
            //список индетификаторов элементов СЦ из справочника «Система целеполагания» 
            var tpsgid = context.LongTermGoalProgram_SystemGoalElement.Where(w => items.Contains(w.Id) 
                                                                        && w.FromAnotherDocumentSE == false 
                                                                        && w.IsMainGoal == false).Select(d => d.IdSystemGoal);

            switch (tpsgid.Count())
            {
                case 0:
                    break;
                default:
                    //актулальные записи выделенных строк тч СЦ из справочника «Система целеполагания»
                    var actuldoc = context.SystemGoal.Where(w =>
                                                            w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && (w.IdDocType_CommitDoc == IdDocType || w.IdDocType_ImplementDoc == IdDocType)
                                                            && w.DateStart >= DateStart && w.DateEnd <= DateEnd
                                                            && w.IdRefStatus == (byte)RefStats.Work
                                                            && tpsgid.Contains(w.Id)
                                                            
                        //&& w.IdDocType_CommitDoc == IdDocType


                        ).Select(t => t.Id).ToList();
                    //список не актуальных СЦ из ТЧ документа
                    var res = context.LongTermGoalProgram_SystemGoalElement.Where(w => items.Contains(w.Id)).Except(context.LongTermGoalProgram_SystemGoalElement.Where(d => items.Contains(d.Id) && actuldoc.Contains(d.IdSystemGoal)));
                    //не актуальные СЦ из справочника «Система целеполагания»
                    var strsg = context.SystemGoal.Where(t => res.Select(s => s.IdSystemGoal).Contains(t.Id));
                    //var strsg = context.ActivityOfSBP_SystemGoalElement.Where(t => t.Id == 1); 
                    //строка перечень не актуальных СЦ из ТЧ «Элементы Система целеполагания»
                    string str = null; //сисок caption
                    foreach (SystemGoal goal in strsg)
                        str = str + "<br> -" + goal.Caption;

                    if (res.Any())
                    {
                        var st =
                            string.Format(
                                "Следующие элементы СЦ справочника «Система целеполагания» не соответствуют реквизитам документа «Тип документа», «Срок реализации с», «Срок реализации по»:{0} <br>" +
                                "Удалить данные элементы из таблицы «Элементы СЦ» документа?", str);
                        Controls.Throw(st);
                        foreach (var item in res)
                        {
                            context.LongTermGoalProgram_SystemGoalElement.Remove(item);
                        }
                        context.SaveChanges();
                    }
                    break;
            }

            var tpsgidMN = context.LongTermGoalProgram_SystemGoalElement.Where(w => items.Contains(w.Id) 
                                                                            && w.FromAnotherDocumentSE == false
                                                                            && w.IsMainGoal == true).Select(d => d.IdSystemGoal);
            switch (tpsgidMN.Count())
            {
                case 0:
                    break;
                default:
                    //актулальные записи выделенных строк тч СЦ из справочника «Система целеполагания»
                    var actuldoc = context.SystemGoal.Where(w =>
                                                            w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && (w.IdDocType_CommitDoc == IdDocType || w.IdDocType_ImplementDoc == IdDocType)
                                                            //&& w.DateStart >= DateStart && w.DateEnd <= DateEnd
                                                            && w.IdRefStatus == (byte)RefStats.Work
                                                            && tpsgidMN.Contains(w.Id)
                                                            && (w.IdSBP == IdSBP || w.IdSBP == null)
                        //&& w.IdDocType_CommitDoc == IdDocType


                        ).Select(t => t.Id).ToList();
                    //список не актуальных СЦ из ТЧ документа
                    var res = context.LongTermGoalProgram_SystemGoalElement.Where(w => items.Contains(w.Id)).Except(context.LongTermGoalProgram_SystemGoalElement.Where(d => items.Contains(d.Id) && actuldoc.Contains(d.IdSystemGoal)));
                    //не актуальные СЦ из справочника «Система целеполагания»
                    var strsg = context.SystemGoal.Where(t => res.Select(s => s.IdSystemGoal).Contains(t.Id));
                    //var strsg = context.ActivityOfSBP_SystemGoalElement.Where(t => t.Id == 1); 
                    //строка перечень не актуальных СЦ из ТЧ «Элементы Система целеполагания»
                    string str = null; //сисок caption
                    foreach (SystemGoal goal in strsg)
                        str = str + "<br> -" + goal.Caption;

                    if (res.Any())
                    {
                        var st =
                            string.Format(
                                "Следующие элементы СЦ справочника «Система целеполагания» не соответствуют реквизитам документа «Тип документа», «Ответственный исполнитель»:{0} <br>" +
                                "Удалить данные элементы из таблицы «Элементы СЦ» документа?", str);
                        Controls.Throw(st);
                        foreach (var item in res)
                        {
                            context.LongTermGoalProgram_SystemGoalElement.Remove(item);
                        }
                        context.SaveChanges();
                    }
                    break;
            }

            //для множественного обновления в ТЧ 
            //var tpsg = context.LongTermGoalProgram_SystemGoalElement.Where(w => items.Contains(w.Id)
            //                                                            && w.FromAnotherDocumentSE == false
            //                                                            && w.IsMainGoal == false);
            //foreach (var sg in tpsg)
            //{
            //    List<int> idelement;
            //    if (sg.IsMainGoal == true)
            //    {
            //        idelement = context.SystemGoal.Where(w =>
            //                                             w.IdPublicLegalFormation == IdPublicLegalFormation
            //                                             &&
            //                                             (w.IdDocType_CommitDoc == IdDocType ||
            //                                              w.IdDocType_ImplementDoc == IdDocType)
            //                                             && w.DateStart >= DateStart && w.DateEnd <= DateEnd
            //                                             && w.IdRefStatus == (byte)RefStats.Work
            //                                             && w.Id == tpsg.SingleOrDefault(t => t.IdSystemGoal)
            //                                             && w.IdSBP == IdSBP).Select(t => t.Id).ToList();
            //    }
            //    else
            //    {
            //        idelement = context.SystemGoal.Where(w =>
            //                                             w.IdPublicLegalFormation == IdPublicLegalFormation
            //                                             &&
            //                                             (w.IdDocType_CommitDoc == IdDocType ||
            //                                              w.IdDocType_ImplementDoc == IdDocType)
            //                                             && w.DateStart >= DateStart && w.DateEnd <= DateEnd
            //                                             && w.IdRefStatus == (byte)RefStats.Work
            //                                             && tpsgidMN.Contains(w.Id)
            //                                             ).Select(t => t.Id).ToList();
            //    }
            //    var res = context.ActivityOfSBP_SystemGoalElement.Where(w => items.Contains(w.Id)).Except(context.ActivityOfSBP_SystemGoalElement.Where(d => items.Contains(d.Id) && idelement.Contains(d.IdSystemGoal)));
            //    //не актуальные СЦ из справочника «Система целеполагания»
            //    var strsg = context.SystemGoal.Where(t => res.Select(s => s.IdSystemGoal).Contains(t.Id));
            //    //var strsg = context.ActivityOfSBP_SystemGoalElement.Where(t => t.Id == 1); 
            //    //строка перечень не актуальных СЦ из ТЧ «Элементы Система целеполагания»
            //    string str = null; //сисок caption
            //    foreach (SystemGoal goal in strsg)
            //        str = str + "<br> -" + goal.Caption;

            //    if (res.Any())
            //    {
            //        var st =
            //            string.Format(
            //                "Следующие элементы СЦ справочника «Система целеполагания» не соответствуют реквизитам документа «Срок реализации», «Тип документа», «Тип ответственного исполнителя»:{0} <br>" +
            //                "Удалить данные элементы из таблицы «Элементы СЦ» документа?", str);
            //        Controls.Throw(st);
            //        foreach (var item in res)
            //        {
            //            context.ActivityOfSBP_SystemGoalElement.Remove(item);
            //        }
            //        context.SaveChanges();
            //    }
            //}

        }



        /// <summary>   
        /// Контроль "Проверка элементов СЦ на соответствие реквизитам документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0246", InitialCaption = "Проверка элементов СЦ на соответствие реквизитам документа")]
        public void Control_0246(DataContext context)
        {
            var sMsg = "Следующие элементы СЦ справочника «Система целеполагания» не соответствуют реквизитам документа «Срок реализации», «Тип документа»:" +
                       "{0}";
            var sMsg_MG = "Следующие элементы СЦ справочника «Система целеполагания» не соответствуют реквизитам документа «Срок реализации», «Тип документа», «Ответственный исполнитель»:" +
                       "{0}";

            //список индетификаторов элементов СЦ из справочника «Система целеполагания» 
            var tpSGE = tpSystemGoalElement.Where(w => w.FromAnotherDocumentSE == false && w.IsMainGoal == false).Select(d => d.IdSystemGoal).ToList();
            var tpSGE_MG = tpSystemGoalElement.Where(w => w.FromAnotherDocumentSE == false && w.IsMainGoal == true).Select(d => d.IdSystemGoal).ToList();

            if (tpSGE.Any() || tpSGE_MG.Any())
            {
                var err = "";

                // актулальные записи выделенных строк тч СЦ с «Основная цель» = Ложь из справочника «Система целеполагания» 
                if (tpSGE.Any())
                {
                    var actuldoc = context.SystemGoal.Where(w =>
                                                            w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && w.DateStart >= DateStart && w.DateEnd <= DateEnd
                                                            && w.IdRefStatus == (byte)RefStats.Work
                                                            && tpSGE.Contains(w.Id)
                                                            && (w.IdDocType_CommitDoc == IdDocType || w.IdDocType_ImplementDoc == IdDocType)
                        ).Select(t => t.Id).ToList();
                    //список не актуальных СЦ из ТЧ документа
                    var res = tpSystemGoalElement.Where(w => w.FromAnotherDocumentSE == false && w.IsMainGoal == false && !actuldoc.Contains(w.IdSystemGoal)).ToList();
                    if (res.Any())
                    {
                        //не актуальные СЦ из справочника «Система целеполагания»
                        var reg_id = res.Select(s => s.IdSystemGoal).ToList();
                        var strsg = context.SystemGoal.Where(t => reg_id.Contains(t.Id));

                        string str = null; //сисок caption
                        foreach (SystemGoal goal in strsg)
                            str = str + "<br> -" + goal.Caption;
                        if (res.Any())
                        {
                            err = err + string.Format(sMsg, str);
                        }
                    }
                }

                // актулальные записи выделенных строк тч СЦ с «Основная цель» = Истина из справочника «Система целеполагания» 
                if (tpSGE_MG.Any())
                {
                    var actuldoc_MG = context.SystemGoal.Where(w =>
                                                            w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && w.DateStart >= DateStart && w.DateEnd <= DateEnd
                                                            && w.IdRefStatus == (byte)RefStats.Work
                                                            && tpSGE_MG.Contains(w.Id)
                                                            && (w.IdDocType_CommitDoc == IdDocType || w.IdDocType_ImplementDoc == IdDocType)
                                                            && (w.IdSBP == IdSBP || !w.IdSBP.HasValue)
                        ).Select(t => t.Id).ToList();
                    //список не актуальных СЦ из ТЧ документа
                    var res_MG = tpSystemGoalElement.Where(w => w.FromAnotherDocumentSE == false && w.IsMainGoal == true && !actuldoc_MG.Contains(w.IdSystemGoal)).ToList();
                    if (res_MG.Any())
                    {
                        //не актуальные СЦ из справочника «Система целеполагания»
                        var reg_MG_id = res_MG.Select(s => s.IdSystemGoal).ToList();
                        var strsg_MG = context.SystemGoal.Where(t => reg_MG_id.Contains(t.Id));

                        string str_MG = null; //сисок caption
                        foreach (SystemGoal goal in strsg_MG)
                            str_MG = str_MG + "<br> -" + goal.Caption;
                        if (res_MG.Any())
                        {
                            err = err + string.Format(sMsg_MG, str_MG);
                        }
                    }
    
                }

                if (err != "")
                {
                    Controls.Throw(err);
                }
            }
        }

        /// <summary>   
        /// Контроль "Проверка согласования документов, входящих в программу"
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка согласования документов, входящих в программу", InitialUNK = "0247")]
        public void Control_0247(DataContext context)
        {
            var sMsg = "Необходимо согласовать документы, входящую в данную {0}:<br>{1}";

            List<ISubDocSGE> listSubDoc =
                DocSGEMethod.GetSubDoc(context, LongTermGoalProgram.EntityIdStatic, AllVersionDocIds, this).ToList().Where(r => r.IdDocStatus != -1543503849)
                .
                Union(DocSGEMethod.GetSubDoc(context, ActivityOfSBP.EntityIdStatic, AllVersionDocIds, this).ToList().Where(r => r.IdDocStatus != -1543503849))
                .ToList();

            if (listSubDoc.Any())
            {
                Controls.Throw(string.Format(sMsg,
                    "программу",
                    listSubDoc.Select(s => s.Header).Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка согласования нижестоящих документов"
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка согласования нижестоящих документов", InitialUNK = "0248", InitialSkippable = true, InitialManaged = true)]
        public void Control_0248(DataContext context)
        {
            if (this.IdDocType != -1543503835)
            {
                return;
            }


        }

        #endregion

        #region Методы операций

        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            DateLastEdit = DateTime.Now;
            ExecuteControl(e => e.Control_0202(context));
        }

        /// <summary>   
        /// Операция «Редактировать»   
        /// </summary>  
        public void Edit(DataContext context)
        {
            DateLastEdit = DateTime.Now;
            ExecuteControl(e => e.Control_0202(context));
        }

        /// <summary>   
        /// Операция «Формирование документов»   
        /// </summary>  
        public void CreateDocs(DataContext context)
        {
            InitScopeDoc(context);
            GetDataDocTables(context);

            ExecMainControls(context);

            CreateSubProgram(context);
            
            ReasonClarification = null;
            IsRequireClarification = false;
        }

        /// <summary>   
        /// Операция «Обработать»   
        /// </summary>  
        public void Process(DataContext context)
        {
            InitScopeDoc(context);
            GetDataDocTables(context);

            if (!IgnorControlsOnProcess)
            {
                ExecuteControl(e => e.Control_0232(context));
                ExecuteControl(e => e.Control_0212(context)); // Перенести выполнения контроля 0212 на действие "Обработать"
                ExecuteControl(e => e.Control_0213(context)); // Перенести контроль 0213 с действия "Формирование документов" на действие "Обработать"
            }

            IEnumerable<LongTermGoalProgram_SystemGoalElement> tpSystemGoalElementsLoc
                = tpSystemGoalElement
                         .Where(r =>
                                (!r.FromAnotherDocumentSE &&
                                 r.SystemGoal.IdDocType_CommitDoc == this.IdDocType &&
                                 r.SystemGoal.IdDocType_ImplementDoc != this.IdDocType));

            var newProgram = CreateProgram(context);

            IEnumerable<ActivityOfSBP.CPair> listId_sges
                = tpSystemGoalElementsLoc
                    .Select(s =>
                            new ActivityOfSBP.CPair()
                            {
                                Id = s.Id,
                                IdParent = s.IdParent
                            })
                    .ToList();

            IEnumerable<ActivityOfSBP.CPair> savSge
                = listId_sges.Where(r1 => !listId_sges.Select(r2 => r2.Id).Contains(r1.IdParent ?? 0));

            Dictionary<int, SystemGoalElement> dirSystemGoalElement = new Dictionary<int, SystemGoalElement>();

            IQueryable<SystemGoalElement> sgeOfParents;

            if (this.IdParent.HasValue)
            {
                sgeOfParents = DocSGEMethod.GetRegDataOfParentDocs(context, arrIdParent, this.EntityId);
            }
            else
            {
                sgeOfParents = context.SystemGoalElement.Where(r => r.IdRegistrator == this.Id && r.IdRegistratorEntity == EntityId);
            }

            CreateResourceMaintenance(context, newProgram);

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

            CreateTaskVolumeMoves(context, newProgram, dirSystemGoalElement);

            CreateTaskIndicatorQualityMoves(context, newProgram);

            CreateActivityResourceMaintenance(context, newProgram);

            CreateAttrProgram(context, newProgram);

            context.SaveChanges();

            // часть: Требует уточнения

            IEnumerable<RegCommLink> docregs0308;
            IEnumerable<RegCommLink> docregs0311;
            string errstr0308;
            string errstr0311;

            LogicControl0208_0211(context,
                                  out docregs0308,
                                  out docregs0311,
                                  out errstr0308,
                                  out errstr0311);

            ExecuteControl(e => e.Control_0208(context, errstr0308));
            ExecuteControl(e => e.Control_0211(context, errstr0311));

            DocSGEMethod.SetRequireClarification(context, docregs0308.ToList(), this.Header,
                "{date}. У элементов из текущего документа имеются вышестоящие элементы из документа {this}, с которыми нарушается соответствие настроенной Модели СЦ."
            );
            DocSGEMethod.SetRequireClarification(context, docregs0311.ToList(), this.Header,
                "{date}. Сроки реализации элементов из текущего документа не соответствуют срокам реализации их вышестоящих элементов из документа {this}."
            );

            if (!IgnorControlsOnProcess)// контроли игнорируются если был вызов из Утвердить с доп. потребностями. 
            //- по каким-то причинам вышестоящая операция считается неатомарной и блокирует повторное выполнение операции над этим же документом
            {
                var prevDoc = (LongTermGoalProgram) CommonMethods.GetPrevVersionDoc(context, this, EntityId);
                if (prevDoc != null)
                {
                    prevDoc.ExecuteOperation(e => e.Archive(context));
                }
            }

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

            RegisterMethods.RemoveFromRegistersByRegistrator(context, this.Id, EntityId, arrRegisters);
            using (new ControlScope())
            {
                context.SaveChanges();
            }

            RegisterMethods.ClearTerminatorByIdDoc(context, this.Id, EntityId, arrRegisters);

            var prevDoc = (LongTermGoalProgram)CommonMethods.GetPrevVersionDoc(context, this, EntityId);
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

            ExecuteControl(e => e.Control_0213(context));
            ExecuteControl(e => e.Control_0233(context));

            if (!HasAdditionalNeed)
            {
                DateCommit = DateTime.Now;

                RegisterMethods.SetRegsApproved(context, Id, Date, EntityId, AllVersionDocIds, arrRegisters);
            }
            else
            {
                // 2.	Создать новую редакцию документа 
                var newDoc = CloneLongTermGoalProgram(context, false, DateTime.Now.Date, DocStatus.Approved);

                newDoc.HasAdditionalNeed = false;

                // очистить поля «Доп. потребность» 
                DocSGEMethod.DeclineAddValueInTp(context, LongTermGoalProgram_Activity_Value.EntityIdStatic, newDoc.Id);
                DocSGEMethod.DeclineAddValueInTp(context, LongTermGoalProgram_ActivityResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
                DocSGEMethod.DeclineAddValueInTp(context, LongTermGoalProgram_IndicatorActivity_Value.EntityIdStatic, newDoc.Id);

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
                RegisterMethods.SetApproveOrTerminateByAddValue(context, arrIdParent, this.Id, newDoc.Id, this.EntityId, Program_ResourceMaintenance.EntityIdStatic, newDoc.Date);
                RegisterMethods.SetApproveOrTerminateByAddValue(context, arrIdParent, this.Id, newDoc.Id, this.EntityId, TaskIndicatorQuality.EntityIdStatic, newDoc.Date);
                RegisterMethods.SetApproveOrTerminateByAddValue(context, arrIdParent, this.Id, newDoc.Id, this.EntityId, TaskVolume.EntityIdStatic, newDoc.Date);
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

            ExecuteControl(e => e.Control_0213(context));
            ExecuteControl(e => e.Control_0233(context));

            ExecuteControl<CommonControlAddNeed_0243>();
            ExecuteControl<CommonControlAddNeed_0244>();

            var newDoc = CloneLongTermGoalProgram(context, false, DateTime.Now.Date, DocStatus.Approved);

            newDoc.HasAdditionalNeed = false;

            DocSGEMethod.AcceptAddValueInTp(context, LongTermGoalProgram_Activity_Value.EntityIdStatic, newDoc.Id);
            DocSGEMethod.AcceptAddValueInTp(context, LongTermGoalProgram_ActivityResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
            DocSGEMethod.AcceptAddValueInTp(context, LongTermGoalProgram_IndicatorActivity_Value.EntityIdStatic, newDoc.Id);

            this.IdDocStatus = DocStatus.Archive;
            context.SaveChanges();

            newDoc.IgnorControlsOnProcess = true;
            newDoc.Process(context);
            newDoc.IgnorControlsOnProcess = false;

            newDoc.DateCommit = DateTime.Now;

            RegisterMethods.SetRegsApproved(context, newDoc.Id, newDoc.Date, EntityId, AllVersionDocIds, arrRegisters);
        }

        /// <summary>   
        /// Операция «Отменить утверждение»   
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            DateCommit = null;

            InitScopeDoc(context);
            RegisterMethods.ClearRegsApproved(context, Id, EntityId, arrRegisters);
        }

        /// <summary>   
        /// Операция «Прекратить»
        /// </summary>  
        public void Terminate(DataContext context)
        {
            ExecuteControl<Control_7001>();
            
            InitScopeDoc(context);
            GetDataDocTables(context);

            ExecuteControl(e => e.Control_0238(context));
            ExecuteControl(e => e.Control_0239(context));

            //2. Аннулировать проводки (с учетом ППО, Версии):

            RegisterMethods.SetTerminatorById(context, arrIdParent, this.EntityId, this.DateTerminate ?? DateTime.Now, this.Id, this.EntityId, arrRegisters);
        }

        /// <summary>   
        /// Операция «Отменить прекращение»   
        /// </summary>  
        public void UndoTerminate(DataContext context)
        {
            var dataContext = context;
            InitScopeDoc(dataContext);
            GetDataDocTables(dataContext);

            ExecuteControl(e => e.Control_0240(context));

            DateTerminate = null;
            ReasonTerminate = null;
            
            //3. В регистрах отменить аннулирование произведенное при переводе на статус «Прекращен», очистить поле «Аннулятор» и «Дата аннулирования»

            var exoper = DocSGEMethod.GetLastExecutedOperation(dataContext, this.Id, this.EntityId, "Terminate");

            RegisterMethods.ClearTerminatorByIdDoc(dataContext, this.Id, EntityId, arrRegisters, exoper.Any() ? exoper.First() : 0);
        }

        /// <summary>   
        /// Операция «Изменить»   
        /// </summary>  
        public void Change(DataContext context)
        {
            var newDoc = CloneLongTermGoalProgram(context, this.HasAdditionalNeed, null, DocStatus.Draft);
            newDoc.Date = DateTime.Now.Date;
        }

        /// <summary>   
        /// Операция «Отменить изменение»   
        /// </summary>
        public void UndoChange(DataContext context)
        {
            var q = context.LongTermGoalProgram.Where(w => w.IdParent == Id);
            foreach (var doc in q)
            {
                context.LongTermGoalProgram.Remove(doc);
            }

            IdDocStatus = DateCommit.HasValue
                            ? DocStatus.Approved
                            : (!String.IsNullOrEmpty(ReasonCancel) ? DocStatus.Denied : DocStatus.Project);

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
            if (IdDocStatus == DocStatus.Denied)
            {
                UndoProcess(context);
            }
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
            ExecuteControl(e => e.Control_0247(context));


            ExecuteControl(e => e.Control_0248(context));

        }


        #endregion

        #region Вспомогательные методы для операций

        private void CreateMainMoves(DataContext context, IEnumerable<ActivityOfSBP.CPair> listId_sges, IEnumerable<ActivityOfSBP.CPair> savSge, Program newProgram, IEnumerable<LongTermGoalProgram_SystemGoalElement> tpSystemGoalElements, Dictionary<int, SystemGoalElement> dirSystemGoalElement, IQueryable<SystemGoalElement> sgeOfParents)
        {

            // аннулируем записи регистра с отсутствующими в ТЧ элементами системы полагания
            var list = tpSystemGoalElements.Select(s => s.IdSystemGoal);
            var sgeOfParentsOnDelete =
                sgeOfParents.Where(
                    r => !list.Contains(r.IdSystemGoal) &&
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
                        DateCreate = DateTime.Now,
                        IdRegistratorEntity = EntityId,
                        IdVersion = this.IdVersion,
                        IdPublicLegalFormation = this.IdPublicLegalFormation,
                        IdSystemGoal = lineTpSge.IdSystemGoal,
                        Program = newProgram
                    };
                    context.SystemGoalElement.Add(newSystemGoalElement);

                    if (lineTpSge.IsMainGoal)
                    {
                        MainSystemGoalElement = newSystemGoalElement;
                    }
                }

                // сопоставляем ID строки ТЧ с элементов в регистре
                dirSystemGoalElement.Add(sge.Id, newSystemGoalElement);

                context.SaveChanges();

                // создаем атрибуты
                CreateAttributeSystemGoalElement(context, newSystemGoalElement, lineTpSge, sge, dirSystemGoalElement);

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

        private Program CreateProgram(DataContext context)
        {
            Program newProgram;

            var old_prog = context.Program.Where(r => (r.IdRegistrator == this.Id || arrIdParent.Contains(r.IdRegistrator)) && r.IdRegistratorEntity == this.EntityId && !r.IdTerminator.HasValue);

            if (old_prog.Any())
            {
                newProgram = old_prog.FirstOrDefault();
            }
            else
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

        private void CreateAttrProgram(DataContext context, Program newProgram)
        {
            var attr_prog = context.AttributeOfProgram.Where(
                r => r.IdPublicLegalFormation == this.IdPublicLegalFormation).ToList()
                                   .Where(r => Equals(r.Program, newProgram) && !r.IdTerminator.HasValue);

            var needAttr = false;

            var MainGoals = tpSystemGoalElement.Where(r => r.IsMainGoal);
            if (MainGoals.Any())
            {
                var idSystemGoalMain = MainGoals.FirstOrDefault().IdSystemGoal;

                MainSystemGoalElement =
                    context.SystemGoalElement.Where(s => s.IdPublicLegalFormation == this.IdPublicLegalFormation && s.IdSystemGoal == idSystemGoalMain).FirstOrDefault();
            }

            if (attr_prog.Any())
            {
                var oldAttributeOfProgram = attr_prog.FirstOrDefault();

                needAttr =
                    oldAttributeOfProgram.IdAnalyticalCodeStateProgram != this.IdAnalyticalCodeStateProgram
                    || !Equals(oldAttributeOfProgram.GoalSystemElement, MainSystemGoalElement)
                    || oldAttributeOfProgram.Caption != this.Caption
                    || oldAttributeOfProgram.DateStart != this.DateStart
                    || oldAttributeOfProgram.DateEnd != this.DateEnd
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

                var IdParentProgram = 0;
                if (this.IdMasterLongTermGoalProgram.HasValue)
                {
                    var rootMasterDoc = CommonMethods.GetFirstVersionDoc(context, this.MasterLongTermGoalProgram, this.MasterLongTermGoalProgram.EntityId);

                    var entityId = this.EntityId;
                    var prog = context.Program.Where(
                        r => r.IdRegistrator == rootMasterDoc.Id && r.IdRegistratorEntity == entityId);
                    if (prog.Any())
                    {
                        IdParentProgram = prog.FirstOrDefault().Id;
                    }
                }

                if (this.IdMasterStateProgram.HasValue)
                {
                    var rootMasterDoc = CommonMethods.GetFirstVersionDoc(context, this.MasterStateProgram, this.MasterStateProgram.EntityId);

                    var stateprogramEntityId = (new StateProgram()).EntityId;
                    var prog = context.Program.Where(
                        r => r.IdRegistrator == rootMasterDoc.Id && r.IdRegistratorEntity == stateprogramEntityId);
                    if (prog.Any())
                    {
                        IdParentProgram = prog.FirstOrDefault().Id;
                    }
                }

                var newAttributeOfProgram = new AttributeOfProgram()
                {
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdVersion = this.IdVersion,
                    IdRegistrator = this.Id,
                    IdRegistratorEntity = EntityId,
                    DateCreate = DateTime.Now,
                    IdAnalyticalCodeStateProgram = this.IdAnalyticalCodeStateProgram,
                    Caption = this.Caption,
                    DateStart = this.DateStart,
                    DateEnd = this.DateEnd,
                    GoalSystemElement = MainSystemGoalElement,
                    Program = newProgram
                };

                if (IdParentProgram != 0)
                {
                    newAttributeOfProgram.IdParent = IdParentProgram;
                }

                context.AttributeOfProgram.Add(newAttributeOfProgram);
            }
        }

        private void CreateAttributeSystemGoalElement(DataContext context, SystemGoalElement newSystemGoalElement,
                                                      LongTermGoalProgram_SystemGoalElement lineTpSge, ActivityOfSBP.CPair sge,
                                                      Dictionary<int, SystemGoalElement> dirSystemGoalElement)
        {

            var oldAttributeOfSystemGoalElements =
                context.AttributeOfSystemGoalElement.Where(
                    r => r.IdSystemGoalElement == newSystemGoalElement.Id && !r.IdTerminator.HasValue).ToList();

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
                IdRegistratorEntity = EntityId,
                DateCreate = DateTime.Now,
                IdVersion = this.IdVersion,
                IdPublicLegalFormation = this.IdPublicLegalFormation,
                SystemGoalElement = newSystemGoalElement,
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

        private void CreateGoalTargets(DataContext context, int sgeid, SystemGoalElement newSystemGoalElement, LongTermGoalProgram_SystemGoalElement lineTpSge)
        {
            var oldGoalTarget = context.GoalTarget.Where(r => r.IdRegistratorEntity == EntityId && arrIdParent.Contains(r.IdRegistrator) && r.IdSystemGoalElement == newSystemGoalElement.Id && !r.IdTerminator.HasValue);
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

                var valuesGoalTargets = context.ValuesGoalTarget.Where(r => r.IdRegistratorEntity == EntityId && arrIdParent.Contains(r.IdRegistrator) && r.IdGoalTarget == lineGI.ogi.Id && !r.IdTerminator.HasValue);

                var delValuesGoalTarget = valuesGoalTargets.ToList().Where(old =>
                                                        !tpGIV.Any(line =>
                                                                  line.IdHierarchyPeriod == old.IdHierarchyPeriod &&
                                                                  line.Value == old.Value));
                foreach (var recL in delValuesGoalTarget)
                {
                    var rec = valuesGoalTargets.Where(r => r.IdRegistratorEntity == EntityId && arrIdParent.Contains(r.IdRegistrator) && r.Id == recL.Id).FirstOrDefault();

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
        /// формируем записи в регистр "Объемы задач"
        /// </summary>
        private void CreateTaskVolumeMoves(DataContext context, Program newProgram, Dictionary<int, SystemGoalElement> dirSystemGoalElement)
        {
            // необходимо удалить все записи в регистре которые ссылаются на не существующие в данном документе строки Элементы СЦ
            // на случай если изменился Элемент СЦ - SBORIII-1028

            var oldTaskVolume0 =
                context.TaskVolume.Where(
                    r =>
                    arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId &&
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
                        arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId &&
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
                                                                           Equals(newSystemGoalElement,
                                                                                  old.SystemGoalElement) &&
                                                                           Equals(RegisterMethods.FindTaskCollection(context,
                                                                                                               this.IdPublicLegalFormation,
                                                                                                               line.a.IdActivity, line.a.IdContingent),
                                                                               old.TaskCollection) &&
                                                                           Equals(line.a.IndicatorActivity_Volume,
                                                                                  old.IndicatorActivity_Volume) &&
                                                                           Equals(line.v.HierarchyPeriod,
                                                                                  old.HierarchyPeriod) &&
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
                                                                 Equals(newSystemGoalElement, old.SystemGoalElement) &&
                                                                 Equals(RegisterMethods.FindTaskCollection(context,
                                                                                                        this
                                                                                                            .IdPublicLegalFormation,
                                                                                                        line.a
                                                                                                            .IdActivity,
                                                                                                        line.a
                                                                                                            .IdContingent),
                                                                        old.TaskCollection) &&
                                                                 Equals(line.a.IndicatorActivity_Volume,
                                                                        old.IndicatorActivity_Volume) &&
                                                                 Equals(line.v.HierarchyPeriod,
                                                                        old.HierarchyPeriod) &&
                                                                 line.v.Value == old.Value &&
                                                                 line.a.IdSBP == old.IdSBP
                                                            ));

                foreach (var lineActivity in newTaskVolumes)
                {
                    TaskCollection curTaskCollection = RegisterMethods.FindTaskCollection(context,
                                                                                       this.IdPublicLegalFormation,
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
                        Program = newProgram,
                        SystemGoalElement = newSystemGoalElement,
                        IdIndicatorActivity_Volume = lineActivity.a.IdIndicatorActivity_Volume,
                        IdHierarchyPeriod = lineActivity.v.IdHierarchyPeriod,
                        Value = lineActivity.v.Value ?? 0,
                        ValueType = DbEnums.ValueType.Plan,
                        TaskCollection = curTaskCollection
                    };
                    context.TaskVolume.Add(newTaskVolume);
                }

                // обрабатываем данные в поле "Доп. потребности"
                oldTaskVolume =
                    context.TaskVolume.Where(
                        r =>
                        arrIdParent.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityId &&
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
                                                                           Equals(newSystemGoalElement,
                                                                                  old.SystemGoalElement) &&
                                                                           Equals(RegisterMethods.FindTaskCollection(context,
                                                                                                               this.IdPublicLegalFormation,
                                                                                                               line.a.IdActivity, line.a.IdContingent),
                                                                               old.TaskCollection) &&
                                                                           Equals(line.a.IndicatorActivity_Volume,
                                                                                  old.IndicatorActivity_Volume) &&
                                                                           Equals(line.v.HierarchyPeriod,
                                                                                  old.HierarchyPeriod) &&
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
                                                                 Equals(newSystemGoalElement, old.SystemGoalElement) &&
                                                                 Equals(RegisterMethods.FindTaskCollection(context,
                                                                                                     this.IdPublicLegalFormation,
                                                                                                     line.a.IdActivity, line.a.IdContingent),
                                                                     old.TaskCollection) &&
                                                                 Equals(line.a.IndicatorActivity_Volume,
                                                                        old.IndicatorActivity_Volume) &&
                                                                 Equals(line.v.HierarchyPeriod,
                                                                        old.HierarchyPeriod) &&
                                                                 line.v.AdditionalValue.Value == old.Value &&
                                                                 line.a.IdSBP == old.IdSBP
                                                                 ));

                foreach (var lineActivity in newTaskVolumes)
                {
                    TaskCollection curTaskCollection = RegisterMethods.FindTaskCollection(context,
                                                                                       this.IdPublicLegalFormation,
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
                        Program = newProgram,
                        SystemGoalElement = newSystemGoalElement,
                        IdIndicatorActivity_Volume = lineActivity.a.IdIndicatorActivity_Volume,
                        IdHierarchyPeriod = lineActivity.v.IdHierarchyPeriod,
                        Value = lineActivity.v.AdditionalValue.Value,
                        IsAdditionalNeed = true,
                        ValueType = DbEnums.ValueType.Plan,
                        TaskCollection = curTaskCollection
                    };
                    context.TaskVolume.Add(newTaskVolume);
                }
            }
        }

        /// <summary>
        /// формируем записи в регистр "Показатели качества задач"
        /// </summary>
        private void CreateTaskIndicatorQualityMoves(DataContext context, Program newProgram)
        {

            // формируем обычные записи по значению поля Значение

            var oldTaskIndicatorQuality = context.TaskIndicatorQuality.Where(r => arrIdParent.Contains(r.IdRegistrator) && !r.IdTerminator.HasValue && r.IdRegistratorEntity == EntityId && !r.IsAdditionalNeed).ToList();

            var tpIndicatorQualityActivityV =
                tpIndicatorActivity.Join(tpIndicatorActivity_Value, a => a.Id, v => v.IdMaster,
                                                (a, v) => new { a, v }).ToList();

            var delTaskIndicatorQuality = oldTaskIndicatorQuality.ToList().Where(old =>
                                                    !tpIndicatorQualityActivityV
                                                            .Any(line =>
                                                                 Equals(RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent), old.TaskCollection) &&
                                                                 Equals(line.v.HierarchyPeriod, old.HierarchyPeriod) &&
                                                                 Equals(line.a.IndicatorActivity, old.IndicatorActivity_Quality) &&
                                                                 line.v.Value == old.Value &&
                                                                 old.ValueType == ValueType.Plan));

            foreach (var recL in delTaskIndicatorQuality)
            {

                var rec = oldTaskIndicatorQuality.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            var newTaskIndicatorQualitys = tpIndicatorQualityActivityV.Where(line =>
                                                    !oldTaskIndicatorQuality
                                                            .Any(old =>
                                                                 Equals(RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent), old.TaskCollection) &&
                                                                 Equals(line.v.HierarchyPeriod, old.HierarchyPeriod) &&
                                                                 Equals(line.a.IndicatorActivity, old.IndicatorActivity_Quality) &&
                                                                 line.v.Value == old.Value &&
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
                    Program = newProgram,
                    IdIndicatorActivity_Quality = line.a.IdIndicatorActivity,
                    IdHierarchyPeriod = line.v.IdHierarchyPeriod,
                    Value = line.v.Value ?? 0,
                    ValueType = DbEnums.ValueType.Plan,
                    TaskCollection = RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent)
                };
                context.TaskIndicatorQuality.Add(newTaskIndicatorQuality);
            }

            // формируем записи по значению поля Доп. потребности

            oldTaskIndicatorQuality = context.TaskIndicatorQuality.Where(r => arrIdParent.Contains(r.IdRegistrator) && !r.IdTerminator.HasValue && r.IdRegistratorEntity == EntityId && r.IsAdditionalNeed).ToList();

            tpIndicatorQualityActivityV =
                tpIndicatorActivity.Join(tpIndicatorActivity_Value.Where(r => r.AdditionalValue.HasValue), a => a.Id, v => v.IdMaster,
                                                (a, v) => new { a, v }).ToList();

            delTaskIndicatorQuality = oldTaskIndicatorQuality.ToList().Where(old =>
                                                    !tpIndicatorQualityActivityV
                                                            .Any(line =>
                                                                 Equals(RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent), old.TaskCollection) &&
                                                                 Equals(line.v.HierarchyPeriod, old.HierarchyPeriod) &&
                                                                 Equals(line.a.IndicatorActivity, old.IndicatorActivity_Quality) &&
                                                                 line.v.AdditionalValue.Value == old.Value &&
                                                                 old.ValueType == ValueType.Plan));

            foreach (var recL in delTaskIndicatorQuality)
            {

                var rec = oldTaskIndicatorQuality.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            newTaskIndicatorQualitys = tpIndicatorQualityActivityV.Where(line =>
                                                    !oldTaskIndicatorQuality
                                                            .Any(old =>
                                                                 Equals(RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent), old.TaskCollection) &&
                                                                 Equals(line.v.HierarchyPeriod, old.HierarchyPeriod) &&
                                                                 Equals(line.a.IndicatorActivity, old.IndicatorActivity_Quality) &&
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
                    Program = newProgram,
                    IdIndicatorActivity_Quality = line.a.IdIndicatorActivity,
                    IdHierarchyPeriod = line.v.IdHierarchyPeriod,
                    Value = line.v.AdditionalValue ?? 0,
                    IsAdditionalNeed = true,
                    ValueType = DbEnums.ValueType.Plan,
                    TaskCollection = RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent)
                };
                context.TaskIndicatorQuality.Add(newTaskIndicatorQuality);
            }
        
        }

        /// <summary>
        /// формируем записи в регистр "Ресурсное обеспечение программ" - по самому документу 
        /// </summary>
        private void CreateResourceMaintenance(DataContext context, Program newProgram)
        {
            // формируем обычные записи по значению поля Значение
            var tpResourceMaintenanceV = tpResourceMaintenance.Join(tpResourceMaintenance_Value.Where(r => r.Value != 0), a => a.Id,
                                                                    v => v.IdMaster, (a, v) => new { a, v }).ToList();

            var oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(r =>
                                                          arrIdParent.Contains(r.IdRegistrator) &&
                                                          !r.IdTerminator.HasValue &&
                                                          r.IdRegistratorEntity == EntityId &&
                                                          !r.IdTaskCollection.HasValue &&
                                                          !r.IsAdditionalNeed).ToList();

            var delProgram_ResourceMaintenances = oldProgram_ResourceMaintenances.ToList().Where(old =>
                                                                                        !tpResourceMaintenanceV.Any(
                                                                                            line => old.IdFinanceSource == line.a.IdFinanceSource &&
                                                                                            old.IdHierarchyPeriod == line.v.IdHierarchyPeriod &&
                                                                                            old.Value == line.v.Value));

            // аннулируем те записи регистра, которые не находятся по данным в ТЧ
            foreach (var recL in delProgram_ResourceMaintenances)
            {
                var rec = oldProgram_ResourceMaintenances.Where(r => r.Id == recL.Id).FirstOrDefault(); 

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            var newProgramResourceMaintenances =
                tpResourceMaintenanceV
                    .Where(o =>
                           !oldProgram_ResourceMaintenances.Any(n =>
                                                                o.a.IdFinanceSource == n.IdFinanceSource &&
                                                                o.v.IdHierarchyPeriod == n.IdHierarchyPeriod &&
                                                                o.v.Value == n.Value));

            // создаем записи регистра по данным в ТЧ, которые не находятся в регистре
            foreach (var rm in newProgramResourceMaintenances)
            {
                var newProgram_ResourceMaintenance = new Program_ResourceMaintenance()
                {
                    IdRegistrator = this.Id,
                    IdRegistratorEntity = EntityId,
                    DateCreate = DateTime.Now,
                    IdVersion = this.IdVersion,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdFinanceSource = rm.a.IdFinanceSource,
                    IdHierarchyPeriod = rm.v.IdHierarchyPeriod,
                    ValueType = DbEnums.ValueType.Plan,
                    Value = rm.v.Value ?? 0,
                    IdProgram = newProgram.Id,
                    IsAdditionalNeed = false
                };
                context.Program_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }

            // формируем обычные записи по значению поля Доп. потребность
            tpResourceMaintenanceV = tpResourceMaintenance.Join(tpResourceMaintenance_Value.Where(r => r.AdditionalValue != 0), a => a.Id,
                                                                    v => v.IdMaster, (a, v) => new { a, v }).ToList();

            oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(r =>
                                                          arrIdParent.Contains(r.IdRegistrator) &&
                                                          !r.IdTerminator.HasValue &&
                                                          r.IdRegistratorEntity == EntityId &&
                                                          !r.IdTaskCollection.HasValue &&
                                                          r.IsAdditionalNeed).ToList();

            delProgram_ResourceMaintenances = oldProgram_ResourceMaintenances.ToList().Where(old =>
                                                                                        !tpResourceMaintenanceV.Any(
                                                                                            line => old.IdFinanceSource == line.a.IdFinanceSource &&
                                                                                            old.IdHierarchyPeriod == line.v.IdHierarchyPeriod &&
                                                                                            old.Value == line.v.AdditionalValue));

            // аннулируем те записи регистра, которые не находятся по данным в ТЧ
            foreach (var recL in delProgram_ResourceMaintenances)
            {
                var rec = oldProgram_ResourceMaintenances.Where(r => r.Id == recL.Id).FirstOrDefault();

                rec.IdTerminator = this.Id;
                rec.IdTerminatorEntity = EntityId;
                rec.DateTerminate = this.Date;
            }

            newProgramResourceMaintenances =
                tpResourceMaintenanceV
                    .Where(o =>
                           !oldProgram_ResourceMaintenances.Any(n =>
                                                                o.a.IdFinanceSource == n.IdFinanceSource &&
                                                                o.v.IdHierarchyPeriod == n.IdHierarchyPeriod &&
                                                                o.v.AdditionalValue == n.Value));

            // создаем записи регистра по данным в ТЧ, которые не находятся в регистре
            foreach (var rm in newProgramResourceMaintenances)
            {
                var newProgram_ResourceMaintenance = new Program_ResourceMaintenance()
                {
                    IdRegistrator = this.Id,
                    IdRegistratorEntity = EntityId,
                    DateCreate = DateTime.Now,
                    IdVersion = this.IdVersion,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdFinanceSource = rm.a.IdFinanceSource,
                    IdHierarchyPeriod = rm.v.IdHierarchyPeriod,
                    ValueType = DbEnums.ValueType.Plan,
                    Value = rm.v.AdditionalValue ?? 0,
                    IdProgram = newProgram.Id,
                    IsAdditionalNeed = true
                };
                context.Program_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }
        }

        /// <summary>
        /// формируем записи в регистр "Ресурсное обеспечение программ" - по мероприятиям
        /// </summary>
        private void CreateActivityResourceMaintenance(DataContext context, Program newProgram)
        {
            // формируем обычные записи по значению поля Значение

            var tpResourceMaintenanceV = tpActivityResourceMaintenance.Join(
                tpActivityResourceMaintenance_Value.Where(r => r.Value > 0),
                a => a.Id, v => v.IdMaster,
                (a, v) => new { a, v }
            ).ToList();

            var oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(r =>
                    arrIdParent.Contains(r.IdRegistrator)
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
                        && Equals(old.TaskCollection,
                            RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent))
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
                        && Equals(old.TaskCollection,
                            RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent))
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
                    TaskCollection = RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, rm.a.Master.IdActivity, rm.a.Master.IdContingent),
                    IsAdditionalNeed = false
                };
                context.Program_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }

            // формируем записи по значению поля Доп. потребность

            tpResourceMaintenanceV = tpActivityResourceMaintenance.Join(
                tpActivityResourceMaintenance_Value.Where(r => r.AdditionalValue.HasValue),
                a => a.Id, v => v.IdMaster,
                (a, v) => new { a, v }
            ).ToList();

            oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(r =>
                    arrIdParent.Contains(r.IdRegistrator)
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
                        && Equals(old.TaskCollection,
                            RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent))
                     )
                );

            // аннулируем те записи регистра, которые не находятся по данным в ТЧ
            foreach (var recL in delProgram_ResourceMaintenances)
            {
                var rec = oldProgram_ResourceMaintenances.FirstOrDefault(r => r.Id == recL.Id);

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
                        && Equals(old.TaskCollection,
                            RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, line.a.Master.IdActivity, line.a.Master.IdContingent))
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
                    TaskCollection = RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, rm.a.Master.IdActivity, rm.a.Master.IdContingent)
                };
                context.Program_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }
        }

        private LongTermGoalProgram CloneLongTermGoalProgram(DataContext context, bool hasAdditionalNeed, DateTime? dateCommit,
                                                 int idDocStatus)
        {
            Clone cloner = new Clone(this);
            LongTermGoalProgram newDoc = (LongTermGoalProgram)cloner.GetResult();
            newDoc.Date = Date;
            newDoc.IdParent = Id;
            newDoc.IsRequireClarification = false;
            newDoc.DateCommit = null;
            newDoc.ReasonTerminate = null;
            newDoc.DateTerminate = null;
            newDoc.DocType = this.DocType;
            newDoc.HasAdditionalNeed = hasAdditionalNeed;
            newDoc.DateCommit = dateCommit;
            newDoc.IdDocStatus = idDocStatus;

            var ids = AllVersionDocIds;
            var rootNum = context.LongTermGoalProgram.Single(w => !w.IdParent.HasValue && ids.Contains(w.Id)).Number;
            newDoc.Number = rootNum + "." + ids.Count().ToString(CultureInfo.InvariantCulture);

            newDoc.Header = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();
            return newDoc;
        }

        private void InitScopeDoc(DataContext context)
        {
            arrIdParent = AllVersionDocIds;

            var lRegisters = new List<int>();
            lRegisters.Add(TaskVolume.EntityIdStatic);
            lRegisters.Add(TaskIndicatorQuality.EntityIdStatic);
            lRegisters.Add(Program_ResourceMaintenance.EntityIdStatic);
            lRegisters.Add(AttributeOfSystemGoalElement.EntityIdStatic);
            lRegisters.Add(ValuesGoalTarget.EntityIdStatic);
            lRegisters.Add(GoalTarget.EntityIdStatic);
            lRegisters.Add(Sbor.Registry.SystemGoalElement.EntityIdStatic);
            lRegisters.Add(AttributeOfProgram.EntityIdStatic);
            lRegisters.Add(Program.EntityIdStatic);
            arrRegisters = lRegisters.ToArray();
        }
        
        private void GetDataDocTables(DataContext context)
        {
            tpGoalIndicator = context.LongTermGoalProgram_GoalIndicator.Where(r => r.IdOwner == this.Id).ToList();

            tpGoalIndicator_Value = context.LongTermGoalProgram_GoalIndicator_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpResourceMaintenance = context.LongTermGoalProgram_ResourceMaintenance.Where(r => r.IdOwner == this.Id).ToList();

            tpResourceMaintenance_Value = context.LongTermGoalProgram_ResourceMaintenance_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpSystemGoalElement = context.LongTermGoalProgram_SystemGoalElement.Where(r => r.IdOwner == this.Id).ToList();

            tpCoExecutor = context.LongTermGoalProgram_CoExecutor.Where(r => r.IdOwner == this.Id).ToList();

            tpListSubProgram = context.LongTermGoalProgram_ListSubProgram.Where(r => r.IdOwner == this.Id).ToList();

            tpSubProgramResourceMaintenance = context.LongTermGoalProgram_SubProgramResourceMaintenance.Where(r => r.IdOwner == this.Id).ToList();

            tpSubProgramResourceMaintenance_Value = context.LongTermGoalProgram_SubProgramResourceMaintenance_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpActivity = context.LongTermGoalProgram_Activity.Where(r => r.IdOwner == this.Id).ToList();

            tpActivity_Value = context.LongTermGoalProgram_Activity_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpActivityResourceMaintenance = context.LongTermGoalProgram_ActivityResourceMaintenance.Where(r => r.IdOwner == this.Id).ToList();

            tpActivityResourceMaintenance_Value = context.LongTermGoalProgram_ActivityResourceMaintenance_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpIndicatorActivity = context.LongTermGoalProgram_IndicatorActivity.Where(r => r.IdOwner == this.Id).ToList();

            tpIndicatorActivity_Value = context.LongTermGoalProgram_IndicatorActivity_Value.Where(r => r.IdOwner == this.Id).ToList();

        }

        private void ExecMainControls(DataContext context)
        {
            ExecuteControl(e => e.Control_0203(context));
            ExecuteControl(e => e.Control_0204(context));
            ExecuteControl(e => e.Control_0205(context));
            ExecuteControl(e => e.Control_0206(context));
            ExecuteControl(e => e.Control_0207(context));
            ExecuteControl(e => e.Control_0246(context));
            // ExecuteControl(e => e.Control_0208(context)); Перенести контроли УНК 0208 и 0211 на действие "Обработать". Выполнять после записи проводок в регистры.
            ExecuteControl(e => e.Control_0209(context));
            ExecuteControl(e => e.Control_0235(context));
            ExecuteControl(e => e.Control_0210(context));
            // ExecuteControl(e => e.Control_0211(context)); Перенести контроли УНК 0208 и 0211 на действие "Обработать". Выполнять после записи проводок в регистры.
            // ExecuteControl(e => e.Control_0212(context)); Перенести выполнения контроля 0212 на действие "Обработать"
            // ExecuteControl(e => e.Control_0213(context)); Перенести контроль 0213 с действия "Формирование документов" на действие "Обработать"
            ExecuteControl(e => e.Control_0214(context));
            ExecuteControl(e => e.Control_0215(context));
            ExecuteControl(e => e.Control_0216(context));
            ExecuteControl(e => e.Control_0217(context));
            ExecuteControl(e => e.Control_0218(context));
            ExecuteControl(e => e.Control_0219(context));
            ExecuteControl(e => e.Control_0220(context));
            ExecuteControl(e => e.Control_0221(context));
            ExecuteControl(e => e.Control_0222(context));
            ExecuteControl(e => e.Control_0223(context));
            ExecuteControl(e => e.Control_0234(context));
            ExecuteControl(e => e.Control_0224(context));
            ExecuteControl(e => e.Control_0225(context));
            ExecuteControl(e => e.Control_0226(context));
            ExecuteControl(e => e.Control_0227(context));
            ExecuteControl(e => e.Control_0228(context));
            ExecuteControl(e => e.Control_0229(context));
            ExecuteControl(e => e.Control_0231(context));
        }


        /// <summary>
        /// получить неаннулированные записи в регистре ЭлементыСЦ созданные предками данного документа
        /// </summary>
        public IQueryable<SystemGoalElement> GetRegDataOfParentDocs(DataContext context, List<int> lIdsParent)
        {
            IQueryable<SystemGoalElement> sgeOfParents;

            // получаем неаннулированные записи в регистре ЭлементыСЦ созданные предками данного документа
            sgeOfParents =
                context.SystemGoalElement.Where(
                    r => lIdsParent.Contains(r.IdRegistrator) && !r.IdTerminator.HasValue && r.IdRegistratorEntity == EntityId);
            return sgeOfParents;
        }


        /// <summary>
        /// Аннулирует записи в регистре у которых регистатор соответствует списку versionIdList от имени текущего документа
        /// </summary>
        /// <param name="registr">Коллекция регистра</param>
        /// <param name="versionIdList">список регистраторов, по которым проводить аннулирование</param>
        public void SetRegistryTerminator(IEnumerable<ICommonRegister> registr, List<int> versionIdList)
        {
            foreach (ICommonRegister r in registr.Where(w => 
                w.IdRegistratorEntity == this.EntityId 
                && versionIdList.Contains(w.IdRegistrator) 
                && w.IdTerminator == null 
                && w.IdTerminatorEntity == null))
            {
                r.IdTerminator = this.Id;
                r.IdTerminatorEntity = this.EntityId;
                r.DateTerminate = this.DateTerminate;
            }
        }

        /// <summary>
        /// Снимает аннулирование для записей в регистре у которых регистатор соответствует списку versionIdList от имени текущего документа
        /// </summary>
        public void ClearRegistryTerminator(IEnumerable<ICommonRegister> registry, List<int> versionIdList)
        {
            foreach (ICommonRegister r in registry.Where(w => 
                w.IdRegistratorEntity ==this.EntityId 
                && versionIdList.Contains(w.IdRegistrator) 
                && w.IdTerminator == this.Id 
                && w.IdTerminatorEntity == this.EntityId))
            {
                r.IdTerminator = null;
                r.IdTerminatorEntity = null;
                r.DateTerminate = null;
            }
        }

        private void CreateSubProgram(DataContext context)
        {
            var mainGoal = SystemGoalElement.SingleOrDefault(s => s.IsMainGoal);
            if (mainGoal == null)
                throw new PlatformException("Формирование документа без выбора основной цели. Не отработали контроли");

            DocSGEMethod.CreateSubDocSGE(context,
                                         DocType.SubProgramDGP,
                                         LongTermGoalProgram_ListSubProgram.EntityIdStatic, 
                                         LongTermGoalProgram_SystemGoalElement.EntityIdStatic,
                                         LongTermGoalProgram_SubProgramResourceMaintenance.EntityIdStatic,
                                         LongTermGoalProgram_SubProgramResourceMaintenance_Value.EntityIdStatic,
                                         LongTermGoalProgram_SystemGoalElement.EntityIdStatic,
                                         LongTermGoalProgram_ResourceMaintenance.EntityIdStatic,
                                         LongTermGoalProgram_ResourceMaintenance_Value.EntityIdStatic, mainGoal.Id,
                                         this);
        }

        #endregion

        #region Implementation of IDocSGE

        public DateTime ParentDate { get { return IdParent.HasValue ? Parent.Date : Date; } }
        public DateTime ParentDateStart { get { return IdParent.HasValue ? Parent.DateStart : DateStart; } }
        public DateTime ParentDateEnd { get { return IdParent.HasValue ? Parent.DateEnd : DateEnd; } }
        public DateTime? ParentDateCommit { get { return IdParent.HasValue ? Parent.DateCommit : DateCommit; } }

        #endregion

        #region Implementation of ISubDocSGE

        [NotMapped]
        public int? IdMasterDoc
        {
            get { return (HasMasterDoc == true ? IdMasterStateProgram : IdMasterLongTermGoalProgram); }

            set
            {
                if (HasMasterDoc == true)
                {
                    IdMasterStateProgram = value;
                }
                else
                {
                    IdMasterLongTermGoalProgram = value;
                }
            }
        }

        [NotMapped]
        public int? IdAnalyticalCodeStateProgramValue { get { return IdAnalyticalCodeStateProgram ?? 0; } set { IdAnalyticalCodeStateProgram = value; } }

        #endregion


        #region Implementation of IColumnFactoryForDenormalizedTablepart

        public ColumnsInfo GetColumns(string tablepartEntityName)
        {
            if (tablepartEntityName == typeof(LongTermGoalProgram_GoalIndicator).Name)
            {
                return GetColumnsFor_SimpleIndicator_Value();
            }
            else if (tablepartEntityName == typeof(LongTermGoalProgram_Activity).Name ||
                tablepartEntityName == typeof(LongTermGoalProgram_ActivityResourceMaintenance).Name ||
                tablepartEntityName == typeof(LongTermGoalProgram_IndicatorActivity).Name ||
                tablepartEntityName == typeof(LongTermGoalProgram_SubProgramResourceMaintenance).Name ||
                tablepartEntityName == typeof(LongTermGoalProgram_ResourceMaintenance).Name)
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

            return new ColumnsInfo() { Periods = columns };
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
                    Resources = new[] { Reflection<ITpAddValue>.Property(ent => ent.Value).Name }
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

