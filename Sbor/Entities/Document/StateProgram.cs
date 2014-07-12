using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Text;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.Utils;
using Platform.Utils.Extensions;
using Sbor.CommonControls;
using Sbor.Interfaces;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Tablepart;
using BaseApp.Reference;
using Sbor.Document;
using System.Linq;
using Sbor.Logic;

using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Document
{
    public partial class StateProgram : ISubDocSGE, IColumnFactoryForDenormalizedTablepart, IClarificationDoc, IDocStatusTerminate, IPpoVerDoc, IAddNeed
    {
        private SystemGoalElement MainSystemGoalElement;

        private List<StateProgram_GoalIndicator> tpGoalIndicator;
        private List<StateProgram_GoalIndicator_Value> tpGoalIndicator_Value;
        private List<StateProgram_ResourceMaintenance> tpResourceMaintenance;
        private List<StateProgram_ResourceMaintenance_Value> tpResourceMaintenance_Value;
        private List<StateProgram_SystemGoalElement> tpSystemGoalElement;
        private List<StateProgram_CoExecutor> tpCoExecutor;
        private List<StateProgram_ListSubProgram> tpListSubProgram;
        private List<StateProgram_DepartmentGoalProgramAndKeyActivity> tpDepartmentGoalProgramAndKeyActivity;
        private List<StateProgram_SubProgramResourceMaintenance> tpSubProgramResourceMaintenance;
        private List<StateProgram_SubProgramResourceMaintenance_Value> tpSubProgramResourceMaintenance_Value;
        private List<StateProgram_DGPKAResourceMaintenance> tpDGPKAResourceMaintenance;
        private List<StateProgram_DGPKAResourceMaintenance_Value> tpDGPKAResourceMaintenance_Value;

        private int[] arrRegisters;
        private int[] arrIdParent;

        public override string ToString()
        {
            return String.Format("{0} № {1} от {2}", this.DocType.Caption, Number, Date.ToString("dd.MM.yyyy"));
        }

        #region Функции для сервисов

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
            var qTp = context.StateProgram_SystemGoalElement.Where(w => w.IdOwner == Id).ToList();

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
                context.StateProgram_SystemGoalElement.Add(new StateProgram_SystemGoalElement()
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
                context.StateProgram_SystemGoalElement.Remove(item);
            }

            context.SaveChanges();

            // теперь удаляем лишние данные для СЦ из другого документа
            var qGi1 = context.StateProgram_GoalIndicator.Where(w => w.IdOwner == Id && w.Master.FromAnotherDocumentSE);
            foreach (var item in qGi1)
            {
                context.StateProgram_GoalIndicator.Remove(item);
            }

            context.SaveChanges();

            // для записей из нашего документа обновляем вычислемые хранимые поля (в т.ч. восстанавливаем иерархию), показатели, значения показателей
            int[] items = context.StateProgram_SystemGoalElement.Where(w => w.IdOwner == Id).Select(s => s.Id).ToArray();
            RefreshData_SystemGoalElement(context, items);
            FillData_GoalIndicator_Value(context, items);
        }

        public void RefreshData_SystemGoalElement(DataContext context, int[] items, bool flag = false)
        {
            if (flag)
                ExecuteControl(e => e.Control_0135(context, items));

            var list = context.StateProgram_SystemGoalElement.Where(w => w.IdOwner == Id).Select(s => new { str = s, sg = s.SystemGoal }).ToList();
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
            var qTpSg = context.StateProgram_SystemGoalElement.Where(w => items.Contains(w.Id) && !w.FromAnotherDocumentSE);

            // подходящие показатели для обновления
            var qGi = context.SystemGoal_GoalIndicator.Where(w => w.IdVersion == IdVersion)
                             .Join(qTpSg, a => a.IdOwner, b => b.IdSystemGoal, (a, b) => a).ToList();

            // существующие показатели в тч
            var qTpGi = context.StateProgram_GoalIndicator.Where(w => w.IdOwner == Id)
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
                context.StateProgram_GoalIndicator.Add(new StateProgram_GoalIndicator()
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
                context.StateProgram_GoalIndicator.Remove(i);
            }

            context.SaveChanges();

            // теперь обновляем значения показателей
            int[] itms = context.StateProgram_GoalIndicator.Where(w => w.IdOwner == Id)
                                .Join(qTpSg, a => a.IdMaster, b => b.Id, (a, b) => a)
                                .Select(s => s.Id).ToArray();
            RefreshData_GoalIndicator_Value(context, itms);
        }

        public void RefreshData_GoalIndicator_Value(DataContext context, int[] items)
        {
            // показатели для которых обновляем значения
            var qTpGi = context.StateProgram_GoalIndicator.Where(w => items.Contains(w.Id));

            // уже существующие значения показателей
            var qTpGiv = context.StateProgram_GoalIndicator_Value.Where(w => w.IdOwner == Id && items.Contains(w.IdMaster)).ToList();

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
                context.StateProgram_GoalIndicator_Value.Add(new StateProgram_GoalIndicator_Value()
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
            //var qDelOf = context.StateProgram_GoalIndicator_Value.Where(r => r.IdOwner == this.Id && !qTpGi.Any(w => w.Id == r.IdMaster));

            foreach (var v in qDelItems)//.Union(qDelOf))
            {
                context.StateProgram_GoalIndicator_Value.Remove(v);
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

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1500)]
        public void C1ontrolPeriod(DataContext context, ControlType ctType)
        {
            

        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1500)]
        public void C12ontrolPeriod(DataContext context, ControlType ctType)
        {

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
                context.StateProgram.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                        .Select(s => s.Number).Distinct().ToList();

            Number = CommonMethods.GetNextCode(sc);
        }

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = -1000)]
        public void AutoSetHeader(DataContext context)
        {
            Header = ToString();
        }

        /// <summary>   
        /// Контроль "Удаление документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0136", InitialCaption = "Удаление документа")]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 0)]
        public void Control_0136(DataContext context)
        {
            List<string> list = context.StateProgram_ListSubProgram.Where(w =>
                w.IdDocumentEntity == EntityId && w.IdDocument == Id
                && !context.StateProgram.Any(a => a.IdParent == w.IdOwner)
            ).Select(s => s.Owner.Header + ", статус: " + s.Owner.DocStatus.Caption).ToList();

            Controls.Check(list, "Невозможно удалить документ, так как он входит в качестве подпрограммы в актуальную редакцию государственной программы:<br>{0}<br>Сначала необходимо исключить документ из государственной программы.");
        }

        /// <summary>   
        /// Контроль "Проверка срока реализации документа"
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка срока реализации документа", InitialUNK = "0101")]
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_0101(DataContext context)
        {
            DocSGEMethod.CommonControl_0101(this);
        }

        /// <summary>   
        /// Обработка "Выравнивание табличных частей под срок реализации документа - удаление лишних данных"
        /// </summary> 
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Update | ControlType.Insert, Sequence.After, ExecutionOrder = 30)]
        public void AligningTableOfDates(DataContext context)
        {
            DocSGEMethod.AlignTableOnDates(context, this.Id, StateProgram_DGPKAResourceMaintenance_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
            DocSGEMethod.AlignTableOnDates(context, this.Id, StateProgram_GoalIndicator_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
            DocSGEMethod.AlignTableOnDates(context, this.Id, StateProgram_ResourceMaintenance_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
            DocSGEMethod.AlignTableOnDates(context, this.Id, StateProgram_SubProgramResourceMaintenance_Value.EntityIdStatic, CommonMethods.DateYearStart(this.DateStart), CommonMethods.DateYearEnd(this.DateEnd));
        }

        /// <summary>   
        /// Контроль "Проверка даты документа"
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка даты документа", InitialUNK = "0103")]
        public void Control_0103(DataContext context)
        {
            DocSGEMethod.CommonControl_0102(this);
        }

        /// <summary>   
        /// Контроль "Проверка наличия документа-дубля"
        /// </summary> 
        [ControlInitial(InitialUNK = "0104", InitialCaption = "Проверка наличия документа-дубля")]
        public void Control_0104(DataContext context)
        {
            var sMsg = "В системе уже имеется документ {0} с версией '{1}', основной целью '{2}' " +
                       "и сроком реализации {3} - {4} гг.<br>" +
                       "{5}<br>" +
                       "Запрещается создавать однотипные документы с одинаковыми реквизитами.";

            var err =
                from d in context.StateProgram
                                 .Where(r => r.IdPublicLegalFormation == this.IdPublicLegalFormation &&
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
                join gse in context.StateProgram_SystemGoalElement.Where(r => r.IsMainGoal).ToList()
                    on d.Id equals gse.IdOwner
                join gseSelf in context.StateProgram_SystemGoalElement.ToList()
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
        /// Контроль "Проверка наличия элементов СЦ в документе"
        /// </summary> 
        [ControlInitial(InitialUNK = "0105", InitialCaption = "Проверка наличия элементов СЦ в документе")]
        public void Control_0105(DataContext context)
        {
            var Msg = "Не указан ни один элемент СЦ, реализующийся в рамках текущего документа.";

            var erD =
                context.StateProgram_SystemGoalElement.Where(r => r.IdOwner == this.Id && !r.FromAnotherDocumentSE);

            if (!erD.Any())
                Controls.Throw(Msg);
        }

        /// <summary>   
        /// Контроль "Проверка вхождения сроков элементов СЦ в срок реализации документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0106", InitialCaption = "Проверка вхождения сроков элементов СЦ в срок реализации документа")]
        public void Control_0106(DataContext context)
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
        [ControlInitial(InitialUNK = "0107", InitialCaption = "Проверка соответствия типа элемента СЦ и документа")]
        public void Control_0107(DataContext context)
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
                                               || g.ElementTypeSystemGoal.IdRefStatus != (byte)RefStatus.Work)
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
        [ControlInitial(InitialUNK = "0108", InitialCaption = "Проверка соответствия модели СЦ в рамках документа")]
        public void Control_0108(DataContext context)
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
        /// Контроль "Проверка соответствия модели СЦ между документами"
        /// </summary> 
        [ControlInitial(InitialUNK = "0109", InitialCaption = "Проверка соответствия модели СЦ между документами", InitialSkippable = true, InitialManaged = true)]
        public void Control_0109(DataContext context, string errstr)
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
        [ControlInitial(InitialUNK = "0112", InitialCaption = "Проверка соответствия сроков реализации с нижестоящими элементами СЦ из других документов", InitialSkippable = true, InitialManaged = true)]
        public void Control_0112(DataContext context, string errstr)
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

        private void LogicControl0109_0112(DataContext context,
                                           out IEnumerable<RegCommLink> docregs0308,
                                           out IEnumerable<RegCommLink> docregs0311,
                                           out string errstr0308,
                                           out string errstr0311)
        {
            IQueryable<SystemGoalElement> regsge;

            var modelsc = (context.GetModelSG(this.IdPublicLegalFormation)).ToList();

            // находим в регистре Элементы СЦ все записи созданные данным ЭД или его предками
            regsge = DocSGEMethod.GetRegDataOfParentDocs(context, arrIdParent, this.EntityId, this.Id);

            // Для каждого элемента из ТЧ Элементы системы целеполагания, у которого флажок «Из другого документа СЦ» = Ложь  или (Тип утверждающего документа  = ШапкаДокумента.Тип документа и Тип реализующего документа <> ШапкаДокумента.Тип документа), 
            var tpsge =
                context.StateProgram_SystemGoalElement.Where(
                    r =>
                    r.IdOwner == this.Id &&
                    (!r.FromAnotherDocumentSE ||
                     r.SystemGoal.IdDocType_CommitDoc == this.IdDocType &&
                     r.SystemGoal.IdDocType_ImplementDoc != this.IdDocType))
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
                                                                                 r.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                                                                 r.IdVersion == this.IdVersion &&
                                                                                 !r.IdTerminator.HasValue &&
                                                                                 !((arrIdParent.Contains(r.IdRegistrator) || r.IdRegistrator == this.Id) &&
                                                                                   r.IdRegistratorEntity == this.Id)
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

            StringBuilder sb = new StringBuilder();

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
        /// Контроль "Правильность дерева элементов СЦ в документе"
        /// </summary> 
        [ControlInitial(InitialUNK = "0110", InitialCaption = "Правильность дерева элементов СЦ в документе")]
        public void Control_0110(DataContext context)
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
        [ControlInitial(InitialUNK = "0111", InitialCaption = "Соответствие сроков реализации нижестоящего элемента с вышестоящим")]
        public void Control_0111(DataContext context)
        {
            var sMsg = "Указаны неверные сроки. " +
                       "Сроки реализации следующих элементов СЦ выходят за пределы сроков их вышестоящих элементов:<br>" +
                       "{0}";

            var err =
                tpSystemGoalElement
                    .Where(r =>
                           !r.FromAnotherDocumentSE &&
                           r.SystemGoal.IdParent.HasValue)
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
        /// Контроль "Проверка наличия элементов СЦ из ТЧ Элементы СЦ в других документах СЦ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0113", InitialCaption = "Проверка наличия элементов СЦ из ТЧ Элементы СЦ в других документах СЦ")]
        public void Control_0113(DataContext context)
        {
            var sMsg = "Следующие элементы СЦ уже добавлены в другие документы системы целеполагания:<br>" +
                       "{0}";

            var err =
                (from tpsge in tpSystemGoalElement.Where(r => !r.FromAnotherDocumentSE && r.SystemGoal.IdDocType_CommitDoc == this.IdDocType)
                 join rsge in context.SystemGoalElement.Where(r => !r.IdTerminator.HasValue && r.IdVersion == this.IdVersion && r.IdRegistratorEntity == this.EntityId) on tpsge.IdSystemGoal equals rsge.IdSystemGoal
                 where !arrIdParent.Contains(rsge.IdRegistrator)
                 select new
                     {
                         tpsge.SystemGoal,
                         rsge.IdRegistrator,
                         rsge.RegistratorEntity
                     }).ToList().Distinct();

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg,
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
        [ControlInitial(InitialUNK = "0114", InitialCaption = "Наличие вышестоящих элементов СЦ в проектных документах")]
        public void Control_0114(DataContext context)
        {
            var sMsg1 = "Следующие элементы СЦ не найдены ни в одном проектном или утвержденном документе системы целеполагания:<br>" +
                       "{0}";

            var sMsg2 = "Следующие элементы СЦ добавлены в несколько документов системы целеполагания. Невозможно определить, с каким элементом СЦ требуется установить связь:<br>" +
                       "{0}";


            var tpsge = tpSystemGoalElement.Where(r =>
                                                                     r.FromAnotherDocumentSE ||
                                                                     r.SystemGoal.IdDocType_CommitDoc != this.IdDocType &&
                                                                     r.SystemGoal.IdDocType_ImplementDoc == this.IdDocType);

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
        [ControlInitial(InitialUNK = "0115", InitialCaption = "Наличие целевых показателей", InitialSkippable = true, InitialManaged = true)]
        public void Control_0115(DataContext context)
        {
            var sMsg = "У следующих элементов СЦ отсутствуют целевые показатели:<br>" +
                       "{0}";

            var err =
                tpSystemGoalElement
                    .Where(r =>
                           r.IdOwner == this.Id &&
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
        [ControlInitial(InitialUNK = "0116", InitialCaption = "Наличие значений у целевых показателей")]
        public void Control_0116(DataContext context)
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
        [ControlInitial(InitialUNK = "0117", InitialCaption = "Наличие значений целевых показателей, выходящих за пределы срока реализации элемента СЦ")]
        public void Control_0117(DataContext context)
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
        [ControlInitial(InitialUNK = "0118", InitialCaption = "Проверка наличия соисполнителей в документе")]
        public void Control_0118(DataContext context)
        {
            var Msg = "В текущем документе не указано ни одного соисполнителя.";

            if (!tpCoExecutor.Any())
                Controls.Throw(Msg);
        }

        /// <summary>   
        /// Контроль "Проверка наличия строк в ТЧ «Ресурсное обеспечение»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0119", InitialCaption = "Проверка наличия строк в ТЧ «Ресурсное обеспечение»", InitialManaged = true)]
        public void Control_0119(DataContext context)
        {
            var Msg = "В документе нет ни одной строки в таблице «Ресурсное обеспечение»";

            if (!tpResourceMaintenance.Any())
                Controls.Throw(Msg);
        }

        /// <summary>   
        /// Контроль "Проверка наличия в документе подпрограммы ГП или основного мероприятия"
        /// </summary> 
        [ControlInitial(InitialUNK = "0120", InitialCaption = "Проверка наличия в документе подпрограммы ГП или основного мероприятия")]
        public void Control_0120(DataContext context)
        {
            var Msg1 =
                "Необходимо добавить в таблицу «Перечень подпрограмм» или «ВЦП и основное мероприятие»  хотя бы одну строку.";
            var Msg2 =
                "Необходимо добавить в таблицу «ВЦП и основное мероприятие» строку с типом «Основное мероприятие» или «Ведомственная целевая программа».";

            if (this.IdDocType == DocType.StateProgram) //Государственная программа
            {
                if (!tpListSubProgram.Any() && !tpDepartmentGoalProgramAndKeyActivity.Any())
                    Controls.Throw(Msg1);
            }
            else if (this.IdDocType == DocType.SubProgramSP) //Подпрограмма ГП
            {
                if (!tpDepartmentGoalProgramAndKeyActivity.Any())
                    Controls.Throw(Msg2);
            }

        }

        /// <summary>   
        /// Контроль "Правильность дерева элементов СЦ для входящих документов"
        /// </summary> 
        [ControlInitial(InitialUNK = "0121", InitialCaption = "Правильность дерева элементов СЦ для входящих документов")]
        public void Control_0121(DataContext context)
        {
            var sMsg1 =
                "Для основной цели не найден элемент в таблице «Элементы системы целеполагания» с типом утверждающего документа {0}<br>" +
                "{1}";
            var sMsg2 =
                "У основной цели вышестоящий элемент не найден в таблице «Элементы системы целеполагания» документа {0} {1}";


        }

        private static string sMsg0122 = "За один период не допускается указывать сумму ресурсного обеспечения в разрезе источников финансирования и без источника финансирования." +
                   "Ошибки обнаружены в таблице «{0}» <br>{1}";

        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения документа за один период в разрезе ИФ и без разреза по ИФ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0122", InitialCaption = "Проверка наличия ресурсного обеспечения документа за один период в разрезе ИФ и без разреза по ИФ")]
        public void Control_0122(DataContext context)
        {
            var tpResourceMaintenance0 = tpResourceMaintenance;
            var tpResourceMaintenanceValue0 = tpResourceMaintenance_Value;

            CtrlPart0122(tpResourceMaintenance0, tpResourceMaintenanceValue0);

            foreach (var listSubProgram in tpListSubProgram)
            {
                var tpSubProgramResourceMaintenance0 = tpSubProgramResourceMaintenance.Where(r => r.IdMaster == listSubProgram.Id).ToList();
                var tpSubProgramResourceMaintenance_Value0 = tpSubProgramResourceMaintenance_Value;

                CtrlPart0129(tpSubProgramResourceMaintenance0, tpSubProgramResourceMaintenance_Value0);
            }

            foreach (var programDepartmentGoalProgramAndKeyActivity in tpDepartmentGoalProgramAndKeyActivity)
            {
                var tpDGPKAResourceMaintenance0 = tpDGPKAResourceMaintenance.Where(r => r.IdMaster == programDepartmentGoalProgramAndKeyActivity.Id).ToList();
                var tpDGPKAResourceMaintenance_Value0 = tpDGPKAResourceMaintenance_Value;

                CtrlPart0130(tpDGPKAResourceMaintenance0, tpDGPKAResourceMaintenance_Value0);
            }
        }

        public static void CtrlPart0130(List<StateProgram_DGPKAResourceMaintenance> tpDGPKAResourceMaintenance0, List<StateProgram_DGPKAResourceMaintenance_Value> tpDGPKAResourceMaintenance_Value0)
        {
            var dgpkaResourceMaintenances = tpDGPKAResourceMaintenance0.Join(tpDGPKAResourceMaintenance_Value0, a => a.Id,
                                                                             v => v.IdMaster, (a, v) => new {a, v});

            var rm_v_0s = dgpkaResourceMaintenances.Where(r => !r.a.IdFinanceSource.HasValue);

            if (rm_v_0s.Any())
            {
                var rm0 = rm_v_0s.Select(s => s.v.HierarchyPeriod);

                var errrm = dgpkaResourceMaintenances.Where(r =>
                                                            r.a.IdFinanceSource.HasValue &&
                                                            rm0.Any(r0 => r.v.HierarchyPeriod.HasIntersection(r0)))
                                                     .OrderBy(o => o.v.HierarchyPeriod.DateStart)
                                                     .Select(s =>
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
                            errrm.Where(r => r.prog == prog).Select(s => s.p).Distinct().OrderBy(o => o).Aggregate((a, b) => a + ", " + b);
                        ms.AppendFormat("{0} {1}<br>", prog.Caption, h);
                    }

                    Controls.Throw(string.Format(sMsg0122, "Ресурсное обеспечение ВЦП и основных мероприятий", ms));
                }
            }
        }

        public static void CtrlPart0129(List<StateProgram_SubProgramResourceMaintenance> tpSubProgramResourceMaintenance0, List<StateProgram_SubProgramResourceMaintenance_Value> tpSubProgramResourceMaintenance_Value0)
        {
            var programResourceMaintenances = tpSubProgramResourceMaintenance0.Join(
                tpSubProgramResourceMaintenance_Value0, a => a.Id,
                v => v.IdMaster, (a, v) => new {a, v});

            var rm_sp_0s = programResourceMaintenances.Where(r => !r.a.IdFinanceSource.HasValue);

            if (rm_sp_0s.Any())
            {
                var rm0 = rm_sp_0s.Select(s => s.v.HierarchyPeriod);

                var errrm = programResourceMaintenances.Where(r =>
                                                              r.a.IdFinanceSource.HasValue &&
                                                              rm0.Any(r0 => r.v.HierarchyPeriod.HasIntersection(r0)))
                                                       .OrderBy(o => o.v.HierarchyPeriod.DateStart)
                                                       .Select(s =>
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
                            errrm.Where(r => r.prog == prog).Select(s => s.p).OrderBy(o => o).Distinct().Aggregate((a, b) => a + ", " + b);
                        ms.AppendFormat("{0} {1}<br>", prog.Caption, h);
                    }

                    Controls.Throw(string.Format(sMsg0122, "Ресурсное обеспечение подпрограмм", ms));
                }
            }
        }

        public static void CtrlPart0122(List<StateProgram_ResourceMaintenance> tpResourceMaintenance0, List<StateProgram_ResourceMaintenance_Value> tpResourceMaintenanceValue0)
        {
            var stateProgramResourceMaintenances = tpResourceMaintenance0.Join(tpResourceMaintenanceValue0, a => a.Id,
                                                                               v => v.IdMaster, (a, v) => new {a, v});

            var rm0s = stateProgramResourceMaintenances.Where(r => !r.a.IdFinanceSource.HasValue);

            if (rm0s.Any())
            {
                var rm0 = rm0s.Select(s => s.v.HierarchyPeriod);

                var errrm = stateProgramResourceMaintenances.Where(r =>
                                                                   r.a.IdFinanceSource.HasValue &&
                                                                   rm0.Any(r0 => r.v.HierarchyPeriod.HasIntersection(r0)))
                                                            .OrderBy(o => o.v.HierarchyPeriod.DateStart)
                                                            .Distinct()
                                                            .Select(s => s.v.HierarchyPeriod.Caption).Distinct();


                if (errrm.Any())
                {
                    Controls.Throw(string.Format(sMsg0122, "Ресурсное обеспечение",
                                                 "по периодам " + errrm.Aggregate((a, b) => a + ", " + b)));
                }
            }
        }

        /// <summary>   
        /// Контроль "Проверка на равенство объемов финансирования подпрограмм, ВЦП и основных мероприятий с объемами финансирования программы"
        /// </summary> 
        [ControlInitial(InitialUNK = "0123", InitialCaption = "Проверка на равенство объемов финансирования подпрограмм, ВЦП и основных мероприятий с объемами финансирования программы")]
        public void Control_0123(DataContext context)
        {
            var sMsgs = string.Empty;

            #region Часть 1
            var sMsg =
                "Сумма ресурсного обеспечения входящих документов должна быть равна сумме ресурсного обеспечения " +
                (this.IdDocType == DocType.StateProgram ? "программы" : "подпрограммы") +
                " с учетом источников финансирования.<br><br>" +
                "{0}";

            var rm = tpResourceMaintenance.Join(tpResourceMaintenance_Value.Where(r =>
                                                                                  r.HierarchyPeriod.HasEntrance(
                                                                                      CommonMethods.DateYearStart(this.DateStart),
                                                                                      CommonMethods.DateYearEnd(this.DateEnd))),
                                                a => a.Id, v => v.IdMaster, (a, v) => new {a, v})
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
                                                          Value = s.Sum(ss => ss.v.Value.HasValue ? ss.v.Value.Value : 0)
                                                      });

            var lsp_rm =
                tpSubProgramResourceMaintenance.Join(tpSubProgramResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                     (a, v) => new {a, v})
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
                                                               Value = s.Sum(ss => ss.v.Value.HasValue ? ss.v.Value.Value : 0)
                                                           });

            var dgka_rm =
                tpDGPKAResourceMaintenance.Join(tpDGPKAResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                (a, v) => new { a, v })
                                          .ToList()
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
                                                          Value = s.Sum(ss => ss.v.Value.HasValue ? ss.v.Value.Value : 0)
                                                      });

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
                                    })
                             .Where(r => r.Value2 > 0);

            var diffrm = from r in rm
                         join u in unRm on new { r.Key.fs, r.Key.hp } equals new { u.Key.fs, u.Key.hp }
                         where r.Value != u.Value
                         select new StateProgram.KeyValD()
                         {
                             Key = r.Key,
                             Value1 = r.Value,
                             Value2 = u.Value
                         };

            var err = rm0.Union(unRm0).Union(diffrm);

            if (err.Any())
            {
                sMsgs = sMsgs + string.Format(sMsg, err.Select(s => s.ToString()).Aggregate((a, b) => a + "<br>" + b));
            }

            #endregion Часть 1

            #region Часть 2

            if (HasAdditionalNeed)
            {
                sMsg =
                    "Сумма доп.потребностей ресурсного обеспечения входящих документов должна быть равна сумме доп.потребностей ресурсного обеспечения  " +
                    (this.IdDocType == DocType.StateProgram ? "программы" : "подпрограммы") +
                    " с учетом источников финансирования.<br><br>" +
                    "{0}";

                rm = tpResourceMaintenance.Join(tpResourceMaintenance_Value.Where(r =>
                                                                                  r.HierarchyPeriod.HasEntrance(
                                                                                      CommonMethods.DateYearStart(
                                                                                          this.DateStart),
                                                                                      CommonMethods.DateYearEnd(
                                                                                          this.DateEnd))),
                                                a => a.Id, v => v.IdMaster, (a, v) => new {a, v})
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
                                                          Value = s.Sum(ss => ss.v.AdditionalValue.HasValue ? ss.v.AdditionalValue.Value : 0)
                                                      });

                lsp_rm =
                    tpSubProgramResourceMaintenance.Join(tpSubProgramResourceMaintenance_Value, a => a.Id,
                                                         v => v.IdMaster,
                                                         (a, v) => new {a, v})
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
                                                                   Value = s.Sum(ss => ss.v.AdditionalValue.HasValue ? ss.v.AdditionalValue.Value : 0)
                                                               });

                dgka_rm =
                    tpDGPKAResourceMaintenance.Join(tpDGPKAResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                    (a, v) => new {a, v})
                                              .ToList()
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
                                                              Value = s.Sum(ss => ss.v.AdditionalValue.HasValue ? ss.v.AdditionalValue.Value : 0)
                                                          });

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
                                        })
                             .Where(r => r.Value2 > 0);

                diffrm = from r in rm
                         join u in unRm on new {r.Key.fs, r.Key.hp} equals new {u.Key.fs, u.Key.hp}
                         where r.Value != u.Value
                         select new StateProgram.KeyValD()
                             {
                                 Key = r.Key,
                                 Value1 = r.Value,
                                 Value2 = u.Value
                             };

                err = rm0.Union(unRm0).Union(diffrm);

                if (err.Any())
                {
                    sMsgs = ((sMsgs != string.Empty) ? sMsgs + "<br>" : "") + string.Format(sMsg, err.Select(s => s.ToString()).Aggregate((a, b) => a + "<br>" + b));
                }
            }

            #endregion Часть 2

            if (sMsgs != string.Empty)
            {
                Controls.Throw(sMsgs);
            }
        }

        public struct ResPair
        {
            public FinanceSource fs;
            public HierarchyPeriod hp;
        }

        public struct KeyVal
        {
            public ResPair Key;
            public decimal Value;
        }

        public class KeyValD
        {
            public ResPair Key;
            public decimal Value1;
            public decimal Value2;

            public string ToString(string str = "")
            {
                var args = Key.fs != null ? Key.fs.Code : "<не указан>";
                return string.Format("{1} г, ИФ {0}, Сумма по документу = {2}, {3}, Разность = {4}",
                    args,
                    Key.hp.Caption,
                    Value1,
                    (str == string.Empty ? "Сумма подпрограмм и мероприятий = " : (str + " = " )) + Value2,
                    Value1 - Value2
                    );
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия в системе вышестоящего документа на статусе «Проект»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0124", InitialCaption = "Проверка наличия в системе вышестоящего документа на статусе «Проект»")]
        public void Control_0124(DataContext context)
        {
            if (!IdMasterDoc.HasValue || IdDocType != DocType.StateProgram) //Государственная программа
                return;

            const string msg = "Вышестоящий документ {0} находится на статусе отличным от «Проект»";

            var lastRevisionId = CommonMethods.FindLastRevisionId(context, StateProgram.EntityIdStatic, IdMasterDoc.Value);
            if (lastRevisionId == 0)
                throw new PlatformException("Отсутствует вышестоящий документ! Возможные проблемы: 1. dbo.GetLastVersionId, 2. сломан ключ в базе");

            var masterDocument = context.StateProgram.FirstOrDefault(r => r.Id == lastRevisionId);
            if (masterDocument == null)
                throw new PlatformException("Отсутствует вышестоящий документ! Возможные проблемы: dbo.GetLastVersionId");

            if (masterDocument.IdDocStatus != DocStatus.Project)
                Controls.Throw(string.Format(msg, masterDocument));
        }

        /// <summary>   
        /// Контроль "Проверка статуса входящего документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0125", InitialCaption = "Проверка статуса входящего документа")]
        public void Control_0125(DataContext context)
        {
            var Msg = string.Empty;
            string sMsg1, sMsg2, sMsg3;

            sMsg1 = CommonControl_0125_0126(context, x => x == DocStatus.Terminated, "были прекращены", "Необходимо удалить их из таблицы:");

            sMsg2 = CommonControl_0125_0126(context, x => x == DocStatus.Denied, "находятся на статусе «Отказан»", "Необходимо перевести их в статусы «Черновик», «Проект» или «Утвержден»:");

            sMsg3 = CommonControl_0125_0126(context, x => x == DocStatus.CreateDocs, "находятся на статусе «Формирование документов»", "Необходимо перевести их в статусы «Черновик», «Проект» или «Утвержден»:");

            Msg = sMsg1 + sMsg2 + sMsg3;

            if (Msg != string.Empty)
            {
                Controls.Throw(Msg);
            }
        }

        /// <summary>   
        /// Контроль "Проверка утверждения входящих документов"
        /// </summary> 
        [ControlInitial(InitialUNK = "0126", InitialCaption = "Проверка утверждения входящих документов")]
        public void Control_0126(DataContext context)
        {
            string sMsg;

            sMsg = CommonControl_0125_0126(context, x => x != DocStatus.Approved, "на статусе отличном от «Утвержден»", "Необходимо утвердить документы:");

            if (sMsg != string.Empty)
            {
                Controls.Throw(sMsg);
            }
        }

        private string CommonControl_0125_0126(DataContext context, Func<int, bool> IsBad, string part1, string part2)
        {

            var sMsg = "Следующие документы из таблицы '{0}' {1}.<br> {2} <br>{3}";

            var returnstring = string.Empty;

            StringBuilder strerr;
            strerr = new StringBuilder();

            var listSubProgram = context.StateProgram_ListSubProgram.Where(r => r.IdOwner == this.Id).ToList();

            foreach (var sp in listSubProgram)
            {
                if (sp.IdDocType == DocType.LongTermGoalProgram) // Долгосрочная целевая программа
                {
                    var foundLTGP = FoundLtgp(context, sp);

                    if (foundLTGP.Any())
                    {
                        LongTermGoalProgram foundDoc = foundLTGP.FirstOrDefault();
                        var childDoc = (LongTermGoalProgram)DocSGEMethod.GetLeafDoc(context, foundDoc.EntityId, foundDoc.Id);

                        if (IsBad(childDoc.IdDocStatus))
                        {
                            strerr.AppendFormat("{0} «{1}» {2}<br>", sp.SBP.Caption, sp.SystemGoal.Caption, childDoc.Header);
                        }
                    }
                }
                else if (sp.IdDocType == DocType.SubProgramSP) // Подпрограмма ГП
                {
                    var foundSP = FoundSp(context, sp);

                    if (foundSP.Any())
                    {
                        StateProgram foundDoc = foundSP.FirstOrDefault();
                        var childDoc = (StateProgram)DocSGEMethod.GetLeafDoc(context, foundDoc.EntityId, foundDoc.Id);

                        if (IsBad(childDoc.IdDocStatus))
                        {
                            strerr.AppendFormat("{0} «{1}» {2}<br>", sp.SBP.Caption, sp.SystemGoal.Caption, childDoc.Header);
                        }
                    }
                }
            }

            if (strerr.ToString() != string.Empty)
            {
                returnstring = string.Format(sMsg, "Перечень подпрограмм", part1, part2, strerr);
            }

            strerr = new StringBuilder();

            var tpDepartmentGoalProgramAndKeyActivity =
                context.StateProgram_DepartmentGoalProgramAndKeyActivity.Where(r => r.IdOwner == this.Id).ToList();

            foreach (var line in tpDepartmentGoalProgramAndKeyActivity)
            {
                var foundActivityOfSBP = FoundActivityOfSbp(context, line);

                if (foundActivityOfSBP.Any())
                {
                    ActivityOfSBP foundDoc = foundActivityOfSBP.FirstOrDefault();
                    var childDoc = (ActivityOfSBP)DocSGEMethod.GetLeafDoc(context, foundDoc.EntityId, foundDoc.Id);

                    if (IsBad(childDoc.IdDocStatus))
                    {
                        strerr.AppendFormat("{0} «{1}» {2}<br>", line.SBP.Caption, line.SystemGoal.Caption, childDoc.Header);
                    }
                }
            }

            if (strerr.ToString() != string.Empty)
            {
                returnstring = returnstring + string.Format(sMsg, "ВЦП и основные мероприятия", part1, part2, strerr);
            }

            return returnstring;
        }

        /// <summary>   
        /// Контроль "Проверка утверждения элементов из другого документа СЦ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0124", InitialCaption = "Проверка наличия в системе вышестоящего документа на статусе «Проект»")]
        public void Control_0127(DataContext context)
        {
            var sMsg = "Необходимо скорректировать дату текущего документа. <br>" +
                       "Следующие элементы СЦ, которые являются вышестоящими для элементов из текущего документа, не утверждены или утверждены более поздней датой:<br>" +
                       "{0}<br>" +
                       "Дата текущего документа: {1}"
                       ;

            var errh =
                from tpGse in context.StateProgram_SystemGoalElement.Where(r =>
                                                                            r.IdOwner == this.Id &&
                                                                            (r.FromAnotherDocumentSE ||
                                                                            r.SystemGoal.IdDocType_CommitDoc != this.IdDocType &&
                                                                            r.SystemGoal.IdDocType_ImplementDoc == this.IdDocType)
                    )
                join gse in context.SystemGoalElement.Where(r => r.IdPublicLegalFormation == this.IdPublicLegalFormation && r.IdVersion == this.IdVersion)
                    on
                    new
                    {
                        sg = tpGse.IdSystemGoal
                    }
                    equals
                    new
                    {
                        sg = gse.IdSystemGoal
                    }
                join asge in context.AttributeOfSystemGoalElement.Where(r => r.IdPublicLegalFormation == this.IdPublicLegalFormation && r.IdVersion == this.IdVersion && !r.IdTerminator.HasValue)
                    on
                    new
                    {
                        id = gse.Id,
                        t = tpGse.IdElementTypeSystemGoal ?? 0,
                        s = tpGse.IdSBP ?? 0
                    }
                    equals
                    new
                    {
                        id = asge.IdSystemGoalElement,
                        t = asge.IdElementTypeSystemGoal,
                        s = (asge.IdSBP ?? 0)
                    }
                select new { tpGse, gse, asge };

            var err =
                errh.Where(r =>
                    r.tpGse.DateStart == r.asge.DateStart && r.tpGse.DateEnd == r.asge.DateEnd &&
                    (!r.asge.DateCommit.HasValue || r.asge.DateCommit > this.Date)).Select(r => new
                    {
                        goal = r.gse.SystemGoal.Caption,
                        r.asge.DateCommit
                    }).ToList();

            if (err.Any())
            {
                var errs = err.Select(s => string.Format("- Элемент СЦ «{0}» {1}", s.goal, (s.DateCommit.HasValue ? s.DateCommit.Value.ToString("dd.MM.yyyy") : "не утвержден")));

                Controls.Throw(string.Format(sMsg,
                                                                 errs.Aggregate((a, b) => a + "<br>" + b),
                                                                 this.Date.ToString("dd.MM.yyyy")
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия в документе нескольких элементов СЦ с признаком «Основная цель»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0128", InitialCaption = "Проверка наличия в документе нескольких элементов СЦ с признаком «Основная цель»")]
        public void Control_0128(DataContext context)
        {
            var sMsg = "В таблице «Элементы СЦ» не должно быть несколько основных целей.";


            var erD = tpSystemGoalElement.Where(r => r.IdOwner == this.Id && r.IsMainGoal);

            if (erD.Count() > 1)
            {
                Controls.Throw(sMsg);
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения подпрограмм за один период в разрезе ИФ и без разреза по ИФ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0129", InitialCaption = "Проверка наличия ресурсного обеспечения подпрограмм за один период в разрезе ИФ и без разреза по ИФ")]
        public void Control_0129(DataContext context)
        {
            var sMsg = "За один период не допускается указывать сумму ресурсного обеспечения " +
                       "в разрезе источников финансирования и без источника финансирования.<br>" +
                       "Ошибки обнаружены в таблице «Ресурсное обеспечение подпрограмм»:<br>" +
                       "{0}";

            var SubProgramResourceMaintenance =
                tpSubProgramResourceMaintenance.Join(tpSubProgramResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                   (a, v) => new { a, v });

            var rma0s = SubProgramResourceMaintenance.Where(r => !r.a.IdFinanceSource.HasValue);
            if (rma0s.Any())
            {
                var rm0 = rma0s.Select(s => new { s.a.Master, s.v.HierarchyPeriod });

                var errrm = SubProgramResourceMaintenance.Where(r =>
                                                                r.a.IdFinanceSource.HasValue &&
                                                                rm0.Any(r0 =>
                                                                        Equals(r.a.Master, r0.Master) &&
                                                                        r.v.HierarchyPeriod.HasIntersection(
                                                                            r0.HierarchyPeriod)))
                                                         .Select(
                                                             s =>
                                                             new
                                                                 {
                                                                     a = s.a.Master,
                                                                     p = s.v.HierarchyPeriod.Caption
                                                                 });

                if (errrm.Any())
                {
                    var ms = new StringBuilder();
                    foreach (var atp in errrm.Select(s => s.a).Distinct().OrderBy(o => o.Caption))
                    {
                        ms.AppendFormat(" - {0}<br>", atp.Caption);

                        var h = errrm.Where(r => r.a == atp)
                                     .Select(s => s.p)
                                     .OrderBy(o => o)
                                     .Aggregate((a, b) => a + ", " + b);

                        ms.AppendFormat("Периоды: {0}.<br>", h);
                    }

                    Controls.Throw(string.Format(sMsg, ms));
                }

            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения ВЦП и  ОМ за один период в разрезе ИФ и без разреза по ИФ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0130", InitialCaption = "Проверка наличия ресурсного обеспечения ВЦП и  ОМ за один период в разрезе ИФ и без разреза по ИФ")]
        public void Control_0130(DataContext context)
        {
            var sMsg = "За один период не допускается указывать сумму ресурсного обеспечения " +
                       "в разрезе источников финансирования и без источника финансирования.<br>" +
                       "Ошибки обнаружены в таблице «Ресурсное обеспечение ВЦП и основных мероприятий»:<br>" +
                       "{0}";

            var SubProgramResourceMaintenance =
                tpDGPKAResourceMaintenance.Join(tpDGPKAResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                   (a, v) => new { a, v });

            var rma0s = SubProgramResourceMaintenance.Where(r => !r.a.IdFinanceSource.HasValue);
            if (rma0s.Any())
            {
                var rm0 = rma0s.Select(s => new { s.a.Master, s.v.HierarchyPeriod });

                var errrm = SubProgramResourceMaintenance.Where(r =>
                                                                r.a.IdFinanceSource.HasValue &&
                                                                rm0.Any(r0 =>
                                                                        Equals(r.a.Master, r0.Master) &&
                                                                        r.v.HierarchyPeriod.HasIntersection(
                                                                            r0.HierarchyPeriod)))
                                                         .Select(
                                                             s =>
                                                             new
                                                             {
                                                                 a = s.a.Master,
                                                                 p = s.v.HierarchyPeriod.Caption
                                                             });

                if (errrm.Any())
                {
                    var ms = new StringBuilder();
                    foreach (var atp in errrm.Select(s => s.a).Distinct().OrderBy(o => o.Caption))
                    {
                        ms.AppendFormat(" - {0}<br>", atp.Caption);

                        var h = errrm.Where(r => r.a == atp)
                                     .Select(s => s.p)
                                     .OrderBy(o => o)
                                     .Aggregate((a, b) => a + ", " + b);

                        ms.AppendFormat("Периоды: {0}.<br>", h);
                    }

                    Controls.Throw(string.Format(sMsg, ms));
                }

            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0131", InitialCaption = "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение»")]
        public void Control_0131(DataContext context)
        {
            var sMsg = "В таблице «Ресурсное обеспечение» необходимо заполнить сумму, хотя бы за один период по строке:<br>" +
                "{0}";


            var err = tpResourceMaintenance.Where(r => !tpResourceMaintenance_Value.Any(v => v.IdMaster == r.Id))
                                           .Select(s => s.IdFinanceSource.HasValue ? s.FinanceSource.Code : "<не указано>")
                                           .ToList();

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg,
                                                                 err.Select(s => "Источник: " + s)
                                                                    .Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение подпрограмм»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0132", InitialCaption = "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение подпрограмм»")]
        public void Control_0132(DataContext context)
        {
            var sMsg = "В таблице «Ресурсное обеспечение подпрограмм» необходимо заполнить сумму, хотя бы за один период по строке:" +
                       "{0}<br>";


            var err = tpSubProgramResourceMaintenance.Where(a => !tpSubProgramResourceMaintenance_Value.Any(v => v.IdMaster == a.Id)).ToList();

            if (err.Any())
            {
                var erD = from r in err
                          join a in tpListSubProgram on r.IdMaster equals a.Id
                          select new { r, a };

                var ms = new StringBuilder();

                foreach (var pp in erD.Select(r => r.a).Distinct().OrderBy(o => o.Caption))
                {
                    ms.AppendFormat(" Подпрограмма «{0}»<br>", pp.Caption);

                    var isf =
                        erD.Where(t => t.a == pp)
                           .Select(r => r.r)
                           .Select(s => s.IdFinanceSource.HasValue ? s.FinanceSource.Code : "<не указано>");

                    ms.AppendFormat("Источник: {0}<br>", isf.Aggregate((a, b) => a + ", " + b));
                }

                Controls.Throw(string.Format(sMsg, ms));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение ВЦП или основных мероприятий»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0133", InitialCaption = "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение ВЦП или основных мероприятий»")]
        public void Control_0133(DataContext context)
        {
            var sMsg = "В таблице «Ресурсное обеспечение ВЦП и основных мероприятий» необходимо заполнить сумму, хотя бы за один период по строке:<br>" +
                       "{0}";


            var err = tpDGPKAResourceMaintenance.Where(a => !tpDGPKAResourceMaintenance_Value.Any(v => v.IdMaster == a.Id)).ToList();

            if (err.Any())
            {
                var erD = from r in err
                          join a in tpDepartmentGoalProgramAndKeyActivity on r.IdMaster equals a.Id
                          select new { r, a };

                var ms = new StringBuilder();

                foreach (var pp in erD.Select(r => r.a).Distinct().OrderBy(o => o.Caption))
                {
                    ms.AppendFormat("Программа «{0}»<br>", pp.Caption);

                    var isf =
                        erD.Where(t => t.a == pp)
                           .Select(r => r.r)
                           .Select(s => s.IdFinanceSource.HasValue ? s.FinanceSource.Code : "<не указано>");

                    ms.AppendFormat("Источник: {0}<br>", isf.Aggregate((a, b) => a + ", " + b));
                }

                Controls.Throw(string.Format(sMsg, ms));
            }
        }




        /// <summary>   
        /// Контроль "Проверка элементов СЦ на соответствие реквизитам документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0135", InitialCaption = "Проверка элементов СЦ на соответствие реквизитам документа", InitialSkippable = true)]
        public void Control_0135(DataContext context, int[] items)
        {
            //список индетификаторов элементов СЦ из справочника «Система целеполагания» 
            var tpsgid = context.StateProgram_SystemGoalElement.Where(w => items.Contains(w.Id)
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
                    var res = context.StateProgram_SystemGoalElement.Where(w => items.Contains(w.Id)).Except(context.StateProgram_SystemGoalElement.Where(d => items.Contains(d.Id) && actuldoc.Contains(d.IdSystemGoal)));
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
                            context.StateProgram_SystemGoalElement.Remove(item);
                        }
                        context.SaveChanges();
                    }
                    break;
            }

            var tpsgidMN = context.StateProgram_SystemGoalElement.Where(w => items.Contains(w.Id)
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
                                                            && (w.IdSBP == IdSBP  || w.IdSBP == null)
                        //&& w.IdDocType_CommitDoc == IdDocType


                        ).Select(t => t.Id).ToList();
                    //список не актуальных СЦ из ТЧ документа
                    var res = context.StateProgram_SystemGoalElement.Where(w => items.Contains(w.Id)).Except(context.StateProgram_SystemGoalElement.Where(d => items.Contains(d.Id) && actuldoc.Contains(d.IdSystemGoal)));
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
                            context.StateProgram_SystemGoalElement.Remove(item);
                        }
                        context.SaveChanges();
                    }
                    break;
            }
            
        }



        /// <summary>   
        /// Контроль "Проверка элементов СЦ на соответствие реквизитам документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0137", InitialCaption = "Проверка элементов СЦ на соответствие реквизитам документа")]
        public void Control_0137(DataContext context)
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
                    var res = tpSystemGoalElement.Where(w =>  w.FromAnotherDocumentSE == false && w.IsMainGoal == false && !actuldoc.Contains(w.IdSystemGoal));
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
                    var res_MG = tpSystemGoalElement.Where(w => w.FromAnotherDocumentSE == false && w.IsMainGoal == true && !actuldoc_MG.Contains(w.IdSystemGoal));
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
        /// Контроль "Очистка доп. потребностей"
        /// </summary> 
        [ControlInitial(InitialSkippable = true, InitialCaption = "Очистка доп. потребностей", InitialUNK = "0138")]
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 50)]
        public void Control_0138(DataContext context, StateProgram old)
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
        public void AutoControl_0138end(DataContext context)
        {
            if (!this.HasAdditionalNeed)
            {
                DocSGEMethod.DeclineAddValueInTp(context, StateProgram_ResourceMaintenance_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, StateProgram_DGPKAResourceMaintenance_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, StateProgram_SubProgramResourceMaintenance_Value.EntityIdStatic, this.Id);
            }
        }

        /// <summary>   
        /// Контроль "Проверка согласования документов, входящих в программу"
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка согласования документов, входящих в программу", InitialUNK = "0142")]
        public void Control_0142(DataContext context)
        {
            var sMsg = "Необходимо согласовать документы, входящую в данную {0}:<br>{1}";

            List<ISubDocSGE> listSubDoc =
                DocSGEMethod.GetSubDoc(context, StateProgram.EntityIdStatic, AllVersionDocIds, this).ToList().Where(r => r.IdDocStatus != -1543503849)
                .
                Union(DocSGEMethod.GetSubDoc(context, LongTermGoalProgram.EntityIdStatic, AllVersionDocIds, this).ToList().Where(r => r.IdDocStatus != -1543503849))
                .
                Union(DocSGEMethod.GetSubDoc(context, ActivityOfSBP.EntityIdStatic, AllVersionDocIds, this).ToList().Where(r => r.IdDocStatus != -1543503849))
                .ToList();

            if (listSubDoc.Any())
            {
                Controls.Throw(string.Format(sMsg, 
                    this.IdDocType == -1543503843 ? "программу" : "подпрограмму",
                    listSubDoc.Select(s => s.Header).Aggregate((a,b) => a + "<br>" + b)));
            }
        }

        #endregion

        #region Методы операций

        /// <summary>   
        /// Операция «Формирование документов»   
        /// </summary>  
        public void CreateDocs(DataContext context)
        {
            InitScopeDoc(context);
            GetDataDocTables(context);

            //Очистить поле «Причина отказа»
            ReasonCancel = null;
            
            //Выполнить контроли
            ExecMainControls(context);
            
            //Очистить поле «Причина отказа». Снять флажок «Требует уточнения»
            ReasonClarification = null;
            IsRequireClarification = false;

            ExecuteControl(e => e.Control_0125(context));

            //Проверить строки в ТЧ «Перечень подпрограмм»: 
            CreateSubProgram(context);
            //Проверить строки в ТЧ «ВЦП и основные мероприятия»: 
            CreateActivityOfSBP(context);
        }

        /// <summary>
        /// Операция "Редактировать"
        /// </summary>
        /// <param name="context"></param>
        public void Edit(DataContext context)
        {
            ExecuteControl(e => e.Control_0103(context));
            
            DateLastEdit = DateTime.Now;
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
                ExecuteControl(e => e.Control_0124(context));
                ExecuteControl(e => e.Control_0113(context));
                ExecuteControl(e => e.Control_0114(context));
            }


            IEnumerable<StateProgram_SystemGoalElement> tpSystemGoalElements
                = context.StateProgram_SystemGoalElement
                         .Where(r =>
                                r.IdOwner == this.Id &&
                                (!r.FromAnotherDocumentSE &&
                                 r.SystemGoal.IdDocType_CommitDoc == this.IdDocType &&
                                 r.SystemGoal.IdDocType_ImplementDoc != this.IdDocType));

            var newProgram = CreateProgram(context);

            IEnumerable<ActivityOfSBP.CPair> listId_sges
                = tpSystemGoalElements
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
                sgeOfParents = DocSGEMethod.GetRegDataOfParentDocs(context, arrIdParent, this.EntityId, this.Id);
            }
            else
            {
                sgeOfParents = context.SystemGoalElement.Where(r => r.IdRegistrator == this.Id && r.IdRegistratorEntity == EntityId);
            }

            CreateMainMoves(context, listId_sges, savSge, newProgram, tpSystemGoalElements, dirSystemGoalElement, sgeOfParents);

            CreateResourceMaintenance(context, newProgram);

            CreateAttrProgram(context, newProgram);

            context.SaveChanges();

            // часть: Требует уточнения

            IEnumerable<RegCommLink> docregs0308;
            IEnumerable<RegCommLink> docregs0311;
            string errstr0308;
            string errstr0311;

            LogicControl0109_0112(context,
                                  out docregs0308,
                                  out docregs0311,
                                  out errstr0308,
                                  out errstr0311);

            ExecuteControl(e => e.Control_0109(context, errstr0308));
            ExecuteControl(e => e.Control_0112(context, errstr0311));

            DocSGEMethod.SetRequireClarification(context, docregs0308.ToList(), this.Header,
                "{date}. У элементов из текущего документа имеются вышестоящие элементы из документа {this}, с которыми нарушается соответствие настроенной Модели СЦ."
            );
            DocSGEMethod.SetRequireClarification(context, docregs0311.ToList(), this.Header,
                "{date}. Сроки реализации элементов из текущего документа не соответствуют срокам реализации их вышестоящих элементов из документа {this}."
            );


            if (!IgnorControlsOnProcess)// контроли игнорируются если был вызов из Утвердить с доп. потребностями. 
                //- по каким-то причинам вышестоящая операция считается неатомарной и блокирует повторное выполнение операции над этим же документом
            {
                var prevDoc = (StateProgram) CommonMethods.GetPrevVersionDoc(context, this, EntityId);
                if (prevDoc != null)
                {
                    // Над документом, указанным в поле «Предыдущая редакция», выполнить операцию «В архив (скрытая)»
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

            RegisterMethods.RemoveFromRegistersByRegistrator(context, Id, EntityId, arrRegisters);
            using (new ControlScope())
            {
                context.SaveChanges();
            }

            RegisterMethods.ClearTerminatorByIdDoc(context, Id, EntityId, arrRegisters);

            var prevDoc = (StateProgram)CommonMethods.GetPrevVersionDoc(context, this, EntityId);
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
            ExecuteControl(e => e.Control_0114(context));
            ExecuteControl(e => e.Control_0126(context));
            ExecuteControl(e => e.Control_0127(context));

            RegisterMethods.SetRegsApproved(context, Id, Date, EntityId, AllVersionDocIds, arrRegisters);

            if (!HasAdditionalNeed)
            {
                DateCommit = DateTime.Now;

                RegisterMethods.SetRegsApproved(context, Id, Date, EntityId, AllVersionDocIds, arrRegisters);
            }
            else
            {
                // 2.	Создать новую редакцию документа 
                var newDoc = CloneSpNewDoc(context, false, DateTime.Now.Date, DocStatus.Approved);

                newDoc.ReasonTerminate = null;
                newDoc.ReasonCancel = null;
                newDoc.HasAdditionalNeed = false;

                // очистить поля «Доп. потребность» 
                DocSGEMethod.DeclineAddValueInTp(context, StateProgram_ResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
                DocSGEMethod.DeclineAddValueInTp(context, StateProgram_DGPKAResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
                DocSGEMethod.DeclineAddValueInTp(context, StateProgram_SubProgramResourceMaintenance_Value.EntityIdStatic, newDoc.Id);

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

            ExecuteControl<CommonControlAddNeed_0243>();
            ExecuteControl<CommonControlAddNeed_0244>();

            var newDoc = CloneSpNewDoc(context, false, DateTime.Now.Date, DocStatus.Approved);

            newDoc.ReasonTerminate = null;
            newDoc.ReasonCancel = null;
            newDoc.HasAdditionalNeed = false;

            DocSGEMethod.AcceptAddValueInTp(context, StateProgram_ResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
            DocSGEMethod.AcceptAddValueInTp(context, StateProgram_DGPKAResourceMaintenance_Value.EntityIdStatic, newDoc.Id);
            DocSGEMethod.AcceptAddValueInTp(context, StateProgram_SubProgramResourceMaintenance_Value.EntityIdStatic, newDoc.Id);

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
            InitScopeDoc(context);
            DateCommit = null;

            RegisterMethods.ClearRegsApproved(context, Id, EntityId, arrRegisters);
        }

        /// <summary>   
        /// Операция «Прекратить»
        /// </summary>  
        public void Terminate(DataContext context)
        {
            ExecuteControl<Control_7001>();
        }

        /// <summary>   
        /// Операция «Отменить прекращение»   
        /// </summary>  
        public void UndoTerminate(DataContext context)
        {
            ReasonTerminate = null;
            DateTerminate = null;
        }

        /// <summary>   
        /// Операция «Изменить»   
        /// </summary>  
        public void Change(DataContext context)
        {
            CloneSpNewDoc(context, this.HasAdditionalNeed, null, DocStatus.Draft);
        }

        private StateProgram CloneSpNewDoc(DataContext context, bool hasAdditionalNeed, DateTime? dateCommit,
                                                 int idDocStatus)
        {
            StateProgram newDoc = Clone(); // (StateProgram)cloner.GetResult();
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = Id;
            newDoc.IsRequireClarification = false;
            newDoc.ReasonTerminate = null;
            newDoc.DateTerminate = null;
            newDoc.DocType = this.DocType;

            newDoc.HasAdditionalNeed = hasAdditionalNeed;
            newDoc.DateCommit = dateCommit;
            newDoc.IdDocStatus = idDocStatus;

            var ids = AllVersionDocIds;
            var rootNum = context.StateProgram.Single(w => !w.IdParent.HasValue && ids.Contains(w.Id)).Number;
            newDoc.Number = rootNum + "." + ids.Count().ToString(CultureInfo.InvariantCulture);

            newDoc.Header = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();

            newDoc.Header = newDoc.ToString();
            context.SaveChanges();
            return newDoc;
        }

        /// <summary>   
        /// Операция «Отменить изменение»   
        /// </summary>
        public void UndoChange(DataContext context)
        {
            var q = context.StateProgram.Where(w => w.IdParent == Id);
            foreach (var doc in q)
            {
                context.StateProgram.Remove(doc);
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
            ExecuteControl(e => e.Control_0142(context));
        }

        #endregion

        #region Вспомогательные методы для операций

        private void GetDataDocTables(DataContext context)
        {
            tpGoalIndicator = context.StateProgram_GoalIndicator.Where(r => r.IdOwner == this.Id).ToList();

            tpGoalIndicator_Value = context.StateProgram_GoalIndicator_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpResourceMaintenance = context.StateProgram_ResourceMaintenance.Where(r => r.IdOwner == this.Id).ToList();

            tpResourceMaintenance_Value = context.StateProgram_ResourceMaintenance_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpSystemGoalElement = context.StateProgram_SystemGoalElement.Where(r => r.IdOwner == this.Id).ToList();

            tpCoExecutor = context.StateProgram_CoExecutor.Where(r => r.IdOwner == this.Id).ToList();

            tpListSubProgram = context.StateProgram_ListSubProgram.Where(r => r.IdOwner == this.Id).ToList();

            tpDepartmentGoalProgramAndKeyActivity = context.StateProgram_DepartmentGoalProgramAndKeyActivity.Where(r => r.IdOwner == this.Id).ToList();

            tpSubProgramResourceMaintenance = context.StateProgram_SubProgramResourceMaintenance.Where(r => r.IdOwner == this.Id).ToList();

            tpSubProgramResourceMaintenance_Value = context.StateProgram_SubProgramResourceMaintenance_Value.Where(r => r.IdOwner == this.Id).ToList();

            tpDGPKAResourceMaintenance = context.StateProgram_DGPKAResourceMaintenance.Where(r => r.IdOwner == this.Id).ToList();

            tpDGPKAResourceMaintenance_Value = context.StateProgram_DGPKAResourceMaintenance_Value.Where(r => r.IdOwner == this.Id).ToList();
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

        private IQueryable<ActivityOfSBP> FoundActivityOfSbp(DataContext context, StateProgram_DepartmentGoalProgramAndKeyActivity line)
        {
            var versionIds = AllVersionDocIds;
            
            return context.ActivityOfSBP.Where(r =>
                                               r.IdVersion == this.IdVersion &&
                                               r.IdPublicLegalFormation ==
                                               this.IdPublicLegalFormation &&
                                               r.IdMasterDoc.HasValue && versionIds.Contains(r.IdMasterDoc.Value) &&
                                               context.ActivityOfSBP_SystemGoalElement.Any(s=>s.IdSystemGoal == line.IdSystemGoal && 
                                                                                              s.IsMainGoal &&  
                                                                                              s.IdOwner == r.Id) );
        }

        private IQueryable<StateProgram> FoundSp(DataContext context, StateProgram_ListSubProgram sp)
        {
            var versionIds = AllVersionDocIds;

            return context.StateProgram.Where(r =>
                                               r.IdVersion == this.IdVersion &&
                                               r.IdPublicLegalFormation ==
                                               this.IdPublicLegalFormation &&
                                               r.IdMasterDoc.HasValue && versionIds.Contains(r.IdMasterDoc.Value) &&
                                               context.StateProgram_SystemGoalElement.Any(s => s.IdSystemGoal == sp.IdSystemGoal &&
                                                                                              s.IsMainGoal &&
                                                                                              s.IdOwner == r.Id));
        }

        private IQueryable<LongTermGoalProgram> FoundLtgp(DataContext context, StateProgram_ListSubProgram sp)
        {
            var versionIds = AllVersionDocIds;

            return context.LongTermGoalProgram.Where(r =>
                                               r.IdVersion == this.IdVersion &&
                                               r.IdPublicLegalFormation ==
                                               this.IdPublicLegalFormation &&
                                               r.IdMasterStateProgram.HasValue && versionIds.Contains(r.IdMasterStateProgram.Value) &&
                                               context.LongTermGoalProgram_SystemGoalElement.Any(s => s.IdSystemGoal == sp.IdSystemGoal &&
                                                                                              s.IsMainGoal &&
                                                                                              s.IdOwner == r.Id));
        }

        private void CreateSubProgram(DataContext context)
        {
            var mainGoal = SystemGoalElement.SingleOrDefault(s => s.IsMainGoal);
            if (mainGoal == null)
                throw new PlatformException("Формирование документа без выбора основной цели. Не отработали контроли");

            DocSGEMethod.CreateSubDocSGE(context,
                                         DocType.LongTermGoalProgram, // Долгосрочная целевая программа
                                         StateProgram_ListSubProgram.EntityIdStatic, 
                                         StateProgram_SystemGoalElement.EntityIdStatic,
                                         StateProgram_SubProgramResourceMaintenance.EntityIdStatic,
                                         StateProgram_SubProgramResourceMaintenance_Value.EntityIdStatic,
                                         LongTermGoalProgram_SystemGoalElement.EntityIdStatic,
                                         LongTermGoalProgram_ResourceMaintenance.EntityIdStatic,
                                         LongTermGoalProgram_ResourceMaintenance_Value.EntityIdStatic, mainGoal.Id,
                                         this);

            DocSGEMethod.CreateSubDocSGE(context,
                                         DocType.SubProgramSP, // Подпрограмма ГП
                                         StateProgram_ListSubProgram.EntityIdStatic, 
                                         StateProgram_SystemGoalElement.EntityIdStatic,
                                         StateProgram_SubProgramResourceMaintenance.EntityIdStatic,
                                         StateProgram_SubProgramResourceMaintenance_Value.EntityIdStatic,
                                         StateProgram_SystemGoalElement.EntityIdStatic,
                                         StateProgram_ResourceMaintenance.EntityIdStatic,
                                         StateProgram_ResourceMaintenance_Value.EntityIdStatic, mainGoal.Id,
                                         this);
        }

        private void CreateActivityOfSBP(DataContext context)
        {
            var mainGoal = SystemGoalElement.SingleOrDefault(s => s.IsMainGoal);
            if (mainGoal == null)
                throw new PlatformException("Формирование документа без выбора основной цели. Не отработали контроли");

            DocSGEMethod.CreateSubDocSGE(context,
                                         null,
                                         StateProgram_DepartmentGoalProgramAndKeyActivity.EntityIdStatic,
                                         StateProgram_SystemGoalElement.EntityIdStatic,
                                         StateProgram_DGPKAResourceMaintenance.EntityIdStatic,
                                         StateProgram_DGPKAResourceMaintenance_Value.EntityIdStatic,
                                         ActivityOfSBP_SystemGoalElement.EntityIdStatic,
                                         ActivityOfSBP_ResourceMaintenance.EntityIdStatic,
                                         ActivityOfSBP_ResourceMaintenance_Value.EntityIdStatic, mainGoal.Id,
                                         this);
        }


        private void ExecMainControls(DataContext context)
        {
            ExecuteControl(e => e.Control_0105(context));
            ExecuteControl(e => e.Control_0104(context));
            ExecuteControl(e => e.Control_0106(context));
            ExecuteControl(e => e.Control_0107(context));
            ExecuteControl(e => e.Control_0108(context));
            ExecuteControl(e => e.Control_0137(context));
            ExecuteControl(e => e.Control_0110(context));
            ExecuteControl(e => e.Control_0128(context));
            ExecuteControl(e => e.Control_0111(context));
            ExecuteControl(e => e.Control_0115(context));
            ExecuteControl(e => e.Control_0116(context));
            ExecuteControl(e => e.Control_0117(context));
            ExecuteControl(e => e.Control_0118(context));
            ExecuteControl(e => e.Control_0119(context));
            ExecuteControl(e => e.Control_0131(context));
            ExecuteControl(e => e.Control_0132(context));
            ExecuteControl(e => e.Control_0133(context));
            ExecuteControl(e => e.Control_0120(context));
            ExecuteControl(e => e.Control_0121(context));
            ExecuteControl(e => e.Control_0122(context));
            ExecuteControl(e => e.Control_0129(context));
            ExecuteControl(e => e.Control_0130(context));
            ExecuteControl(e => e.Control_0123(context));
        }

        private void CreateMainMoves(DataContext context, IEnumerable<ActivityOfSBP.CPair> listId_sges, IEnumerable<ActivityOfSBP.CPair> savSge, Program newProgram, IEnumerable<StateProgram_SystemGoalElement> tpSystemGoalElements, Dictionary<int, SystemGoalElement> dirSystemGoalElement, IQueryable<SystemGoalElement> sgeOfParents)
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
                        IdRegistratorEntity = EntityId,
                        DateCreate = DateTime.Now,
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

        private void CreateResourceMaintenance(DataContext context, Program newProgram)
        {
            // формируем обычные записи по значению поля Значение

            var tpResourceMaintenanceV =
                tpResourceMaintenance.Join(tpResourceMaintenance_Value.Where(r => (r.Value ?? 0) != 0), a => a.Id,
                                           v => v.IdMaster, (a, v) => new { a, v }).ToList();

            var oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(
                    r =>
                    r.IdRegistratorEntity == EntityId && arrIdParent.Contains(r.IdRegistrator) &&
                    !r.IdTerminator.HasValue && !r.IdTaskCollection.HasValue
                    && !r.IsAdditionalNeed
                    );

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
                    Program = newProgram,
                    IsAdditionalNeed = false
                };
                context.Program_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
            }

            // формируем обычные записи по значению поля Доп. потребность
            tpResourceMaintenanceV =
                tpResourceMaintenance.Join(tpResourceMaintenance_Value.Where(r => (r.AdditionalValue ?? 0) != 0), a => a.Id,
                                           v => v.IdMaster, (a, v) => new { a, v }).ToList();

            oldProgram_ResourceMaintenances =
                context.Program_ResourceMaintenance.Where(
                    r =>
                    r.IdRegistratorEntity == EntityId && arrIdParent.Contains(r.IdRegistrator) &&
                    !r.IdTerminator.HasValue && !r.IdTaskCollection.HasValue &&
                    r.IsAdditionalNeed
                    );

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
                    Program = newProgram,
                    IsAdditionalNeed = true
                };
                context.Program_ResourceMaintenance.Add(newProgram_ResourceMaintenance);
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

        private void CreateAttributeSystemGoalElement(DataContext context, SystemGoalElement newSystemGoalElement,
                                                      StateProgram_SystemGoalElement lineTpSge, ActivityOfSBP.CPair sge,
                                                      Dictionary<int, SystemGoalElement> dirSystemGoalElement)
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

        private void CreateGoalTargets(DataContext context, int sgeid, SystemGoalElement newSystemGoalElement, StateProgram_SystemGoalElement lineTpSge)
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

            var IdParentProgram = 0;
            if (this.IdMasterDoc.HasValue)
            {
				var rootMasterDoc = CommonMethods.GetFirstVersionDoc(context, this.MasterDoc, this.MasterDoc.EntityId);

                var entityId = this.EntityId;
                var prog = context.Program.Where(
                    r => r.IdRegistrator == rootMasterDoc.Id && r.IdRegistratorEntity == entityId);
                if (prog.Any())
                {
                    IdParentProgram = prog.FirstOrDefault().Id;
                }
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
                    || (oldAttributeOfProgram.IdParent ?? 0) != IdParentProgram
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

        #endregion

        #region Implementation of IDocSGE

        public DateTime ParentDate { get { return IdParent.HasValue ? Parent.Date : Date; } }
        public DateTime ParentDateStart { get { return IdParent.HasValue ? Parent.DateStart : DateStart; } }
        public DateTime ParentDateEnd { get { return IdParent.HasValue ? Parent.DateEnd : DateEnd; } }
        public DateTime? ParentDateCommit { get { return IdParent.HasValue ? Parent.DateCommit : DateCommit; } }

        #endregion

        #region Implementation of ISubDocSGE

        [NotMapped]
        public int? IdAnalyticalCodeStateProgramValue { get { return IdAnalyticalCodeStateProgram; } set { IdAnalyticalCodeStateProgram = value ?? 0; } }

        #endregion

        #region Implementation of IColumnFactoryForDenormalizedTablepart

        public ColumnsInfo GetColumns(string tablepartEntityName)
        {
            if (tablepartEntityName == typeof (StateProgram_GoalIndicator).Name)
            {
                return GetColumnsFor_SimpleIndicator_Value();
            }
            else if (tablepartEntityName == typeof (StateProgram_ResourceMaintenance).Name ||
                     tablepartEntityName == typeof (StateProgram_SubProgramResourceMaintenance).Name ||
                     tablepartEntityName == typeof (StateProgram_DGPKAResourceMaintenance).Name)
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


        #region Клонирование

        private StateProgram Clone()
        {
            var newDoc = new StateProgram()
                {
                    Caption = this.Caption,
                    Date = this.Date,
                    DateCommit = this.DateCommit,
                    DateEnd = this.DateEnd,
                    DateLastEdit = this.DateLastEdit,
                    DateStart = this.DateStart,
                    DateTerminate = this.DateTerminate,
                    Description = this.Description,
                    Header = this.Header,

                    IdAnalyticalCodeStateProgram = this.IdAnalyticalCodeStateProgram,
                    IdDocStatus = this.IdDocStatus,
                    IdDocType = this.IdDocType,
                    IdMasterDoc = this.IdMasterDoc,
                    IdParent = this.IdParent,
                    IdPublicLegalFormation = this.IdPublicLegalFormation,
                    IdResponsibleExecutantType = this.IdResponsibleExecutantType,
                    IdSBP = this.IdSBP,
                    IdVersion = this.IdVersion,

                    HasAdditionalNeed = this.HasAdditionalNeed,
                    IsApproved = this.IsApproved,
                    IsRequireClarification = this.IsRequireClarification,
                    Number = this.Number,
                    ReasonCancel = this.ReasonCancel,
                    ReasonClarification = this.ReasonClarification,
                    ReasonTerminate = this.ReasonTerminate
                };

            Clone_CoExecutor(newDoc, this);
            Clone_DepartmentGoalProgramAndKeyActivity(newDoc, this);
            Clone_SystemGoalElement(newDoc, this);
            Clone_ResourceMaintenance(newDoc, this);
            Clone_ListSubProgram(newDoc, this);

            return newDoc;
        }

        // CoExecutor

        private void Clone_CoExecutor(StateProgram newDoc, StateProgram originalDoc)
        {
            foreach (var row in originalDoc.CoExecutor)
            {
                var newRow = new StateProgram_CoExecutor()
                    {
                        Owner = newDoc,
                        IdResponsibleExecutantType = row.IdResponsibleExecutantType,
                        IdSBP = row.IdSBP
                    };

                newDoc.CoExecutor.Add(newRow);
            }
        }

        // DepartmentGoalProgramAndKeyActivity

        private void Clone_DepartmentGoalProgramAndKeyActivity(StateProgram newDoc, StateProgram originalDoc)
        {
            foreach (var row in originalDoc.DepartmentGoalProgramAndKeyActivity)
            {
                var newRow = new StateProgram_DepartmentGoalProgramAndKeyActivity()
                    {
                        Owner = newDoc,
                        ActiveDocumentCaption = row.ActiveDocumentCaption,
                        Caption = row.Caption,
                        DateEnd = row.DateEnd,
                        DateStart = row.DateStart,
                        IdActiveDocStatus = row.IdActiveDocStatus,
                        IdActiveDocument = row.IdActiveDocument,
                        IdAnalyticalCodeStateProgram = row.IdAnalyticalCodeStateProgram,
                        IdDocType = row.IdDocType,
                        IdDocument = row.IdDocument,
                        IdDocumentEntity = row.IdDocumentEntity,
                        IdResponsibleExecutantType = row.IdResponsibleExecutantType,
                        IdSBP = row.IdSBP,
                        IdSystemGoal = row.IdSystemGoal
                    };

                Clone_DGPKAResourceMaintenance(newRow, row);

                newDoc.DepartmentGoalProgramAndKeyActivity.Add(newRow);
            }
        }

        private void Clone_DGPKAResourceMaintenance(StateProgram_DepartmentGoalProgramAndKeyActivity newMaster, StateProgram_DepartmentGoalProgramAndKeyActivity originalMaster)
        {
            foreach (var row in originalMaster.StateProgram_DGPKAResourceMaintenance)
            {
                var newRow = new StateProgram_DGPKAResourceMaintenance()
                    {
                        Owner = newMaster.Owner,
                        Master = newMaster,
                        IdFinanceSource = row.IdFinanceSource
                    };

                Clone_DGPKAResourceMaintenance_Value(newRow, row);

                newMaster.StateProgram_DGPKAResourceMaintenance.Add(newRow);
            }
        }

        private void Clone_DGPKAResourceMaintenance_Value(StateProgram_DGPKAResourceMaintenance newMaster, StateProgram_DGPKAResourceMaintenance originalMaster)
        {
            foreach (var row in originalMaster.StateProgram_DGPKAResourceMaintenance_Value)
            {
                var newRow = new StateProgram_DGPKAResourceMaintenance_Value()
                    {
                        Owner = newMaster.Owner,
                        Master = newMaster,
                        IdHierarchyPeriod = row.IdHierarchyPeriod,
                        Value = row.Value,
                        AdditionalValue = row.AdditionalValue
                    };

                newMaster.StateProgram_DGPKAResourceMaintenance_Value.Add(newRow);
            }
        }

        // SystemGoalElement
            
        private void Clone_SystemGoalElement(StateProgram newDoc, StateProgram originalDoc)
        {
            // ключ = id исходной строки, значение - копия исходной строки
            Dictionary<int, StateProgram_SystemGoalElement> oldIdNewParent = new Dictionary<int, StateProgram_SystemGoalElement>();
            
            IEnumerable<StateProgram_SystemGoalElement> subset;
            subset = originalDoc.SystemGoalElement.Where(a => !a.IdParent.HasValue);

            CloneSubset_SystemGoalElement(newDoc, subset, oldIdNewParent);
        }
        
        private void CloneSubset_SystemGoalElement(StateProgram newDoc, IEnumerable<StateProgram_SystemGoalElement> originaTpElements, Dictionary<int, StateProgram_SystemGoalElement> oldIdNewParent)
        {
            foreach (var row in originaTpElements)
            {
                var newRow = new StateProgram_SystemGoalElement()
                    {
                        Owner = newDoc,
                        Parent = row.IdParent.HasValue ? oldIdNewParent[row.IdParent.Value] : null,
                        Code = row.Code,
                        DateEnd = row.DateEnd,
                        DateStart = row.DateStart,
                        FromAnotherDocumentSE = row.FromAnotherDocumentSE,
                        IdElementTypeSystemGoal = row.IdElementTypeSystemGoal,
                        IdSBP = row.IdSBP,
                        IdSystemGoal = row.IdSystemGoal,
                        IsMainGoal = row.IsMainGoal
                    };
                oldIdNewParent[row.Id] = newRow;

                Clone_GoalIndicator(newRow, row);

                newDoc.SystemGoalElement.Add(newRow);

                CloneSubset_SystemGoalElement(newDoc, row.ChildrenByidParent, oldIdNewParent);
            }
        }

        private void Clone_GoalIndicator(StateProgram_SystemGoalElement newMaster, StateProgram_SystemGoalElement originalMaster)
        {
            foreach (var row in originalMaster.StateProgram_GoalIndicator)
            {
                var newRow = new StateProgram_GoalIndicator()
                    {
                        Owner = newMaster.Owner,
                        Master = newMaster,
                        IdGoalIndicator = row.IdGoalIndicator
                    };

                Clone_GoalIndicator_Value(newRow, row);

                newMaster.StateProgram_GoalIndicator.Add(newRow);
            }
        }

        private void Clone_GoalIndicator_Value(StateProgram_GoalIndicator newMaster, StateProgram_GoalIndicator originalMaster)
        {
            foreach (var row in originalMaster.StateProgram_GoalIndicator_Value)
            {
                var newRow = new StateProgram_GoalIndicator_Value()
                    {
                        Owner = newMaster.Owner,
                        Master = newMaster,
                        IdHierarchyPeriod = row.IdHierarchyPeriod,
                        Value = row.Value
                    };

                newMaster.StateProgram_GoalIndicator_Value.Add(newRow);
            }
        }

        // ResourceMaintenance

        private void Clone_ResourceMaintenance(StateProgram newDoc, StateProgram originalDoc)
        {
            foreach (var row in originalDoc.ResourceMaintenance)
            {
                var newRow = new StateProgram_ResourceMaintenance()
                    {
                        Owner = newDoc,
                        IdFinanceSource = row.IdFinanceSource
                    };

                Clone_ResourceMaintenance_Value(newRow, row);

                newDoc.ResourceMaintenance.Add(newRow);
            }
        }

        private void Clone_ResourceMaintenance_Value(StateProgram_ResourceMaintenance newMaster, StateProgram_ResourceMaintenance originalMaster)
        {
            foreach (var row in originalMaster.StateProgram_ResourceMaintenance_Value)
            {
                var newRow = new StateProgram_ResourceMaintenance_Value()
                    {
                        Owner = newMaster.Owner,
                        Master = newMaster,
                        IdHierarchyPeriod = row.IdHierarchyPeriod,
                        Value = row.Value,
                        AdditionalValue = row.AdditionalValue
                    };

                newMaster.StateProgram_ResourceMaintenance_Value.Add(newRow);
            }
        }

        // ListSubProgram

        private void Clone_ListSubProgram(StateProgram newDoc, StateProgram originalDoc)
        {
            foreach (var row in originalDoc.ListSubProgram)
            {
                var newRow = new StateProgram_ListSubProgram()
                    {
                        Owner = newDoc,
                        ActiveDocumentCaption = row.ActiveDocumentCaption,
                        Caption = row.Caption,
                        DateEnd = row.DateEnd,
                        DateStart = row.DateStart,
                        IdActiveDocStatus = row.IdActiveDocStatus,
                        IdActiveDocument = row.IdActiveDocument,
                        IdAnalyticalCodeStateProgram = row.IdAnalyticalCodeStateProgram,
                        IdDocType = row.IdDocType,
                        IdDocument = row.IdDocument,
                        IdDocumentEntity = row.IdDocumentEntity,
                        IdResponsibleExecutantType = row.IdResponsibleExecutantType,
                        IdSBP = row.IdSBP,
                        IdSystemGoal = row.IdSystemGoal
                    };

                Clone_SubProgramResourceMaintenance(newRow, row);

                newDoc.ListSubProgram.Add(newRow);
            }
        }

        private void Clone_SubProgramResourceMaintenance(StateProgram_ListSubProgram newMaster, StateProgram_ListSubProgram originalMaster)
        {
            foreach (var row in originalMaster.StateProgram_SubProgramResourceMaintenance)
            {
                var newRow = new StateProgram_SubProgramResourceMaintenance()
                    {
                        Owner = newMaster.Owner,
                        Master = newMaster,
                        IdFinanceSource = row.IdFinanceSource
                    };

                Clone_SubProgramResourceMaintenance_Value(newRow, row);

                newMaster.StateProgram_SubProgramResourceMaintenance.Add(newRow);
            }
        }

        private void Clone_SubProgramResourceMaintenance_Value(StateProgram_SubProgramResourceMaintenance newMaster, StateProgram_SubProgramResourceMaintenance originalMaster)
        {
            foreach (var row in originalMaster.StateProgram_SubProgramResourceMaintenance_Value)
            {
                var newRow = new StateProgram_SubProgramResourceMaintenance_Value()
                    {
                        Owner = newMaster.Owner,
                        Master = newMaster,
                        IdHierarchyPeriod = row.IdHierarchyPeriod,
                        Value = row.Value,
                        AdditionalValue = row.AdditionalValue
                    };

                newMaster.StateProgram_SubProgramResourceMaintenance_Value.Add(newRow);
            }
        }


        #endregion
    }

}

