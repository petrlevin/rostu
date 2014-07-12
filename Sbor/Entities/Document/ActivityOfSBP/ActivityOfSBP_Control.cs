using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using EntityFramework.Extensions;
using Microsoft.Practices.Unity.Utility;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.Common.Exceptions;
using Platform.Common.Extensions;
using BaseApp.Activity.Operations;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Registry;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;

using System;
using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using BaseApp.Reference;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Reference;
using Sbor.Registry;
using System.Linq;
using Sbor.Logic;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

// ReSharper disable CheckNamespace
namespace Sbor.Document
// ReSharper restore CheckNamespace
{
    partial class ActivityOfSBP
    {
        private void InitMaps(DataContext context)
        {
            if (PublicLegalFormation == null)
                PublicLegalFormation = context.PublicLegalFormation.SingleOrDefault(w => w.Id == IdPublicLegalFormation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctType"></param>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1500)]
        public void ControlPeriod(DataContext context, ControlType ctType)
        {
            var minYear = context.HierarchyPeriod.Min(c => c.Year);
            var maxYear = context.HierarchyPeriod.Max(c => c.Year);

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
                context.ActivityOfSBP.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                        .Select(s => s.Number).Distinct().ToList();

            Number = sc.GetNextCode();
        }

        /// <summary>   
        /// Контроль "Удаление документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0343", InitialCaption = "Удаление документа")]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 0)]
        public void Control_0343(DataContext context)
        {
            List<string> list = context.StateProgram_DepartmentGoalProgramAndKeyActivity.Where(w =>
                w.IdDocumentEntity == EntityId && w.IdDocument == Id
                && !context.StateProgram.Any(a => a.IdParent == w.IdOwner)
            ).Select(s => s.Owner.Header + ", статус: " + s.Owner.DocStatus.Caption).ToList();

            Controls.Check(list, "Невозможно удалить документ, так как он входит в качестве основного мероприятия в актуальную редакцию государственной программы (подпрограммы ГП):<br>{0}<br>Сначала необходимо исключить документ из государственной программы (подпрограммы ГП).");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = -1000)]
        public void AutoSetHeader(DataContext context)
        {
            Header = ToString();
        }

        /// <summary>   
        /// Обработка "Выравнивание табличных частей под срок реализации документа - удаление лишних данных"
        /// </summary> 
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Update | ControlType.Insert, Sequence.After, ExecutionOrder = 30)]
        public void AligningTableOfDates(DataContext context)
        {
            DocSGEMethod.AlignTableOnDates(context, 
                                           Id, 
                                           ActivityOfSBP_ActivityDemandAndCapacity_Value.EntityIdStatic,
                                           DateStart.DateYearStart(),
                                           DateEnd.DateYearEnd());
            DocSGEMethod.AlignTableOnDates(context, Id, ActivityOfSBP_Activity_Value.EntityIdStatic, DateStart.DateYearStart(), DateEnd.DateYearEnd());
            DocSGEMethod.AlignTableOnDates(context, Id, ActivityOfSBP_GoalIndicator_Value.EntityIdStatic, DateStart.DateYearStart(), DateEnd.DateYearEnd());
            DocSGEMethod.AlignTableOnDates(context, Id, ActivityOfSBP_IndicatorQualityActivity_Value.EntityIdStatic, DateStart.DateYearStart(), DateEnd.DateYearEnd());
            DocSGEMethod.AlignTableOnDates(context, Id, ActivityOfSBP_ResourceMaintenance_Value.EntityIdStatic, DateStart.DateYearStart(), DateEnd.DateYearEnd());
            DocSGEMethod.AlignTableOnDates(context, Id, ActivityOfSBP_ActivityResourceMaintenance_Value.EntityIdStatic, DateStart.DateYearStart(), DateEnd.DateYearEnd());
        }

        /// <summary>   
        /// Контроль "Проверка срока реализации документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0301", InitialCaption = "Проверка срока реализации документа")]
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 20)]
        public void Control_0301(DataContext context)
        {
            DocSGEMethod.CommonControl_0101(this);
        }

        /// <summary>   
        /// Контроль "Проверка наличия бланка формирования ГРБС"
        /// </summary> 
        [ControlInitial(InitialUNK = "0345", InitialCaption = "Проверка наличия бланка формирования ГРБС")]
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 20)]
        public void Control_0345(DataContext context)
        {
            SBP sbp;
            IBudget curbudget;
            var sbpBlanks = GetBlanks(context, this.SBP, out sbp, out curbudget);

            if (!sbpBlanks.Any(r => r.IdBlankType == (int)DbEnums.BlankType.FormationGRBS))
            {
                var smes =
                    "В справочник «СБП» для СБП «{0}» необходимо добавить бланк «Формирование ГРБС» с бюджетом «{1}»";
                Controls.Throw(string.Format(smes, sbp.Caption, curbudget.Caption));
            }
        }

        private IQueryable<SBP_Blank> GetBlanks(DataContext context, SBP thissbp, out SBP sbp, out IBudget curbudget)
        {
            var budget = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget;
            curbudget = budget;

            var SBPBlank = new SBP();

            if (thissbp.IdSBPType == (int) SBPType.GeneralManager)
            {
                sbp = thissbp;
                SBPBlank = thissbp;
            }
            else if (thissbp.IdSBPType == (int) SBPType.Manager)
            {
                sbp = thissbp.Parent;
                SBPBlank = thissbp.Parent;
            }
            else
            {
                sbp = thissbp;
                return null;
            }

            var sbpBlanks = context.SBP_Blank.Where(r =>
                                                    r.IdOwner == SBPBlank.Id &&
                                                    r.IdBudget == budget.Id);
            return sbpBlanks;
        }

        /// <summary>   
        /// Контроль "Проверка даты документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0302", InitialCaption = "Проверка даты документа")]
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_0302(DataContext context)
        {
            DocSGEMethod.CommonControl_0102(this);
        }

        /// <summary>   
        /// Контроль "Проверка наличия элементов СЦ в документе"
        /// </summary> 
        [ControlInitial(InitialUNK = "0303", InitialCaption = "Проверка наличия элементов СЦ в документе")]
        public void Control_0303(DataContext context)
        {
            const string msg = "Не указан ни один элемент СЦ, реализующийся в рамках текущего документа.";

            var erD = context.ActivityOfSBP_SystemGoalElement.Where(r => r.IdOwner == Id && !r.FromAnotherDocumentSE);

            if (!erD.Any())
                Controls.Throw(msg);
        }

        /// <summary>   
        /// Контроль "Проверка наличия документа-дубля"
        /// </summary> 
        [ControlInitial(InitialUNK = "0304", InitialCaption = "Проверка наличия документа-дубля")]
        public void Control_0304(DataContext context)
        {
            const string sMsg = "В системе уже имеется документ {0} с версией '{1}', основной целью '{2}' " +
                                "и сроком реализации {3} - {4} гг.<br>" +
                                "{5}<br>" +
                                "Запрещается создавать однотипные документы с одинаковыми реквизитами.";

            var err =
                from d in context.ActivityOfSBP
                                 .Where(r =>
                                        r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                        r.IdVersion == IdVersion &&
                                        r.IdDocType == IdDocType &&
                                        (r.IdDocStatus == DocStatus.Project ||
                                        r.IdDocStatus == DocStatus.Changed ||
                                        r.IdDocStatus == DocStatus.Approved ||
                                        r.IdDocStatus == DocStatus.Denied))
                                 .ToList()
                                 .Where(r =>
                                        !_arrIdParent.Contains(r.Id) &&
                                        r.Id != Id)
                join gse in context.ActivityOfSBP_SystemGoalElement.Where(r => r.IsMainGoal).ToList()
                    on d.Id equals gse.IdOwner
                join gseSelf in context.ActivityOfSBP_SystemGoalElement.ToList()
                                       .Where(r => r.IdOwner == Id && r.IsMainGoal)
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
                           }).ToList();

            if (errl.Any())
            {
                var ferr = errl.FirstOrDefault();

                Controls.Throw(string.Format(sMsg,
                                                                 DocType.Caption,
                                                                 Version.Caption,
// ReSharper disable PossibleNullReferenceException
                                                                 ferr.g,
// ReSharper restore PossibleNullReferenceException
                                                                 ferr.ds.ToString("dd.MM.yyyy"),
                                                                 ferr.de.ToString("dd.MM.yyyy"),
                                                                 ferr.c
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка вхождения сроков элементов СЦ в срок реализации документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0305", InitialCaption = "Проверка вхождения сроков элементов СЦ в срок реализации документа")]
        public void Control_0305(DataContext context)
        {
            const string sMsg = "Сроки реализации следующих элементов СЦ выходят за пределы срока реализации документа " +
                                "{0} - {1} гг:<br>" +
                                "{2}";
            var err =
                tpSystemGoalElement
                    .Where(r => !r.FromAnotherDocumentSE)
                    .ToList()
                    .Where(g => !DocSGEMethod.HasEntrance(this, g))
                    .Select(g => g.SystemGoal.Caption)
                    .ToList();

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg,
                                                                 DateStart.ToShortDateString(),
                                                                 DateEnd.ToShortDateString(),
                                                                 string.Join( "<br/>", err)
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия типа элемента СЦ и документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0306", InitialCaption = "Проверка соответствия типа элемента СЦ и документа")]
        public void Control_0306(DataContext context)
        {
            const string sMsg = "Следующие элементы СЦ не могут реализовываться в рамках документа {0}," +
                                "так как такая связь не соответствует настройкам в справочнике «Типы элементов СЦ»:<br>" +
                                "{1}";

            var err =
                tpSystemGoalElement.Where(r => !r.FromAnotherDocumentSE)
                                   .Where(g => !context.ElementTypeSystemGoal_Document
                                                       .Where(tp => tp.IdOwner == g.IdElementTypeSystemGoal)
                                                       .Select(s => s.IdDocType)
                                                       .Contains(IdDocType)
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
                                                                 DocType.Caption,
                                                                 err0.Aggregate((a, b) => a + "<br>" + b)
                                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия модели СЦ в рамках документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0307", InitialCaption = "Проверка соответствия модели СЦ в рамках документа")]
        public void Control_0307(DataContext context)
        {
            //есть 
            //a0 = Документ .строкаТЧ_ЭлементыСЦ 
            //a1 = a0 .Наименование .Тип
            //a2 = a0 .Вышестоящий .Тип
            //b0 = МодельСЦ (где Статус = В работе)
            //b1 = b0 .Тип 
            //b2 = b0 .Вышестоящий .Тип

            //мы должны найти для a0 такое b0, чтобы a1 = b1 и a2 = b2
            //если не нашли - значит выводим сообщение контроля 
            //"В документе присутствуют связи, которые не соответствуют настроенной Модели СЦ:
            // a2 - a1"

            const string sMsg = "В документе присутствуют связи, которые не соответствуют настроенной Модели СЦ:<br>" +
                                "{0}";

            var modelsc = context.GetModelSG(IdPublicLegalFormation).ToList();

            var docm = tpSystemGoalElement
                              .Where(r =>
                                     !r.FromAnotherDocumentSE)
                              .Select(s =>
                                      new SGModel
                                      {
                                          ElementType = s.ElementTypeSystemGoal,
                                          ElementParentType = (s.Parent == null ? null : s.Parent.ElementTypeSystemGoal )
                                      });

            var err = docm.Where(r => !modelsc.Any(m => m.ElementType == r.ElementType && m.ElementParentType == r.ElementParentType));

            if (err.Any())
            {
                var err0 = err.ToList().Distinct().Select(s => string.Format("{0} - {1}", s.ElementParentType == null ? "<не указан>" : s.ElementParentType.Caption, s.ElementType.Caption));

                Controls.Throw(string.Format(sMsg, err0.Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Правильность дерева элементов СЦ в документе"
        /// </summary> 
        [ControlInitial(InitialUNK = "0309", InitialCaption = "Правильность дерева элементов СЦ в документе")]
        public void Control_0309(DataContext context)
        {
            const string sMsg = "Для следующих элементов СЦ требуется указать вышестоящий элемент СЦ из текущего или из другого документа СЦ:<br>" +
                                "{0}";

            var err =
                tpSystemGoalElement.Where(r => !r.FromAnotherDocumentSE && !r.IdParent.HasValue)
                                   .Select(s => s.SystemGoal.Caption)
                                   .ToList();

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
        [ControlInitial(InitialUNK = "0310", InitialCaption = "Соответствие сроков реализации нижестоящего элемента с вышестоящим")]
        public void Control_0310(DataContext context)
        {
            const string sMsg = "Указаны неверные сроки. " +
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
                var err0 = err.ToList().Select(s => string.Format("{0} {1} - {2}", s.s, s.ds.Value.ToShortDateString(), s.de.Value.ToShortDateString()));
                Controls.Throw(string.Format(sMsg, err0.Aggregate((a, b) => a + "<br/>" + b) ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия модели СЦ между документами"
        /// </summary> 
        [ControlInitial(InitialUNK = "0308", InitialCaption = "Проверка соответствия модели СЦ между документами", InitialSkippable = true, InitialManaged = true)]
        public void Control_0308(DataContext context, string errstr)
        {
            const string sMsg = "У элементов СЦ из текущего документа обнаружены нижестоящие элементы, с которыми нарушается соответствие настроенной Модели СЦ:  <br>" +
                                "{0}<br>" +
                                "У документов с указанными нижестоящими элементами СЦ будет установлен признак «Требует уточнения».";

            if (errstr != string.Empty)
            {
                Controls.Throw(string.Format(sMsg, errstr));
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия сроков реализации с нижестоящими элементами СЦ из других документов"
        /// </summary> 
        [ControlInitial(InitialUNK = "0311", InitialCaption = "Проверка соответствия сроков реализации с нижестоящими элементами СЦ из других документов", InitialSkippable = true, InitialManaged = true)]
        public void Control_0311(DataContext context, string errstr)
        {
            const string sMsg = "У элементов СЦ из текущего документа обнаружены нижестоящие элементы, с которыми нарушается соответствие сроков реализации:  <br>" +
                                "{0}<br>" +
                                "У документов с указанными нижестоящими элементами СЦ будет установлен признак «Требует уточнения».";

            if (!string.IsNullOrEmpty(errstr))
                Controls.Throw(string.Format(sMsg, errstr));
        }

        private void LogicControl0308_0311(DataContext context,
                                           out IEnumerable<RegCommLink> docregs0308,
                                           out IEnumerable<RegCommLink> docregs0311,
                                           out string errstr0308,
                                           out string errstr0311)
        {
            var modelsc = (context.GetModelSG(IdPublicLegalFormation)).ToList();

            // находим в регистре Элементы СЦ все записи созданные данным ЭД или его предками
            IQueryable<SystemGoalElement> regsge = DocSGEMethod.GetRegDataOfParentDocs(context, _arrIdParent, EntityId, Id);

            // Для каждого элемента из ТЧ Элементы системы целеполагания, у которого флажок «Из другого документа СЦ» = Ложь  или (Тип утверждающего документа  = ШапкаДокумента.Тип документа и Тип реализующего документа <> ШапкаДокумента.Тип документа), 
            var tpsge =
                context.ActivityOfSBP_SystemGoalElement.Where(
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
                                                                                 !((_arrIdParent.Contains(r.IdRegistrator) || r.IdRegistrator == this.Id) &&
                                                                                   r.IdRegistratorEntity == this.Id)
                                                     )
                                                     on rl.rsge.Id equals attrchild.IdSystemGoalElement_Parent
                                                 select new { attrchild, rl.lsge });

            var asgel = attributeOfSystemGoalElements.ToList();

            var err308 = asgel.Where(s =>
                                     !modelsc
                                          .Any(m =>
                                               m.ElementType == s.attrchild.ElementTypeSystemGoal &&
                                               m.ElementParentType == s.lsge.ElementTypeSystemGoal))
                              .ToArray();

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
        [ControlInitial(InitialUNK = "0312", InitialCaption = "Проверка наличия элементов СЦ из ТЧ Элементы СЦ в других документах СЦ")]
        public void Control_0312(DataContext context)
        {
            const string sMsg = "Следующие элементы СЦ уже добавлены в другие документы системы целеполагания:<br>" +
                                "{0}";

            var err =
                (from tpsge in tpSystemGoalElement.Where(r =>
                                                         r.FromAnotherDocumentSE ||
                                                         r.SystemGoal.IdDocType_CommitDoc != IdDocType)
                 join rsge in context.SystemGoalElement.Where(r => !r.IdTerminator.HasValue && r.IdVersion == this.IdVersion && r.IdRegistratorEntity == this.EntityId) on tpsge.IdSystemGoal equals rsge.IdSystemGoal
                 where !_arrIdParent.Contains(rsge.IdRegistrator)
                 orderby tpsge.SystemGoal.Caption
                 select new
                 {
                     tpsge.SystemGoal,
                     rsge.IdRegistrator,
                     rsge.RegistratorEntity
                 }).ToList().Distinct().ToList();

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg,  err.Select(s =>
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
        [ControlInitial(InitialUNK = "0313", InitialCaption = "Наличие вышестоящих элементов СЦ в проектных документах")]
        public void Control_0313(DataContext context)
        {
            const string sMsg1 = "Следующие элементы СЦ не найдены ни в одном проектном или утвержденном документе системы целеполагания:<br>" +
                                 "{0}<br>" +
                                 "Возможно, что в текущем документе указаны устаревшие реквизиты этих элементов СЦ. Или требуется добавить эти элементы в документы СЦ.";

            const string sMsg2 = "Следующие элементы СЦ добавлены в несколько документов системы целеполагания. Невозможно определить, с каким элементом СЦ требуется установить связь:<br>" +
                                 "{0}";


            var tpsge = tpSystemGoalElement.Where(r =>
                                                                     r.FromAnotherDocumentSE ||
                                                                     r.SystemGoal.IdDocType_CommitDoc != IdDocType &&
                                                                     r.SystemGoal.IdDocType_ImplementDoc == IdDocType);

            var ms1 = new StringBuilder();
            var ms2 = new StringBuilder();

            foreach (var tp in tpsge)
            {
                var attributeOfSystemGoalElements =
                    context.AttributeOfSystemGoalElement.Where(r =>
                                                               !r.IdTerminator.HasValue &&
                                                               !r.SystemGoalElement.IdTerminator.HasValue &&
                                                               r.IdVersion == IdVersion &&
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
                                     tp.DateStart.Value.ToShortDateString(),
                                     tp.DateEnd.Value.ToShortDateString() );
                }
                else
                {
                    if (attributeOfSystemGoalElements.Count() > 1)
                    {
                        ms2.AppendFormat("{0} {1} «{2}» <br> {3} - {4} <br> ",
                                         tp.IdSBP.HasValue ? tp.SBP.Caption : "",
                                         tp.ElementTypeSystemGoal.Caption,
                                         tp.SystemGoal.Caption,
                                         tp.DateStart.Value.ToShortDateString(),
                                         tp.DateEnd.Value.ToShortDateString() );

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
        [ControlInitial(InitialUNK = "0314", InitialCaption = "Наличие целевых показателей", InitialSkippable = true, InitialManaged = true)]
        public void Control_0314(DataContext context)
        {
            var sMsg = "У следующих элементов СЦ отсутствуют целевые показатели:<br>" +
                       "{0}";

            var err =
                tpSystemGoalElement
                    .Where(r =>
                           r.IdOwner == Id &&
                           !r.FromAnotherDocumentSE &&
                           tpGoalIndicator.All(t => t.IdMaster != r.Id))
                    .Select(s => s.SystemGoal.Caption).ToList();

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg, err.Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Наличие значений у целевых показателей"
        /// </summary> 
        [ControlInitial(InitialUNK = "0315", InitialCaption = "Наличие значений у целевых показателей")]
        public void Control_0315(DataContext context)
        {
            var sMsg = "У следующих целевых показателей не задано ни одного значения:<br>" +
                       "{0}";

            var err =
                tpGoalIndicator.Where(r => tpGoalIndicator_Value.All(i => i.IdMaster != r.Id))
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
        [ControlInitial(InitialUNK = "0316", InitialCaption = "Наличие значений целевых показателей, выходящих за пределы срока реализации элемента СЦ")]
        public void Control_0316(DataContext context)
        {
            var sMsg = "У следующих целевых показателей обнаружены значения, выходящие за срок реализации элемента СЦ:<br>" +
                       "{0}";

            var tpGoalIndicatorV = tpGoalIndicator.Join(tpGoalIndicator_Value, a => a.Id, v => v.IdMaster,
                                                        (a, v) => new { a, v }).ToList();

            var err =
                tpGoalIndicatorV.Join(tpSystemGoalElement, tpgiv => tpgiv.a.IdMaster, tpsge => tpsge.Id, (tpgiv, tpsge) => new { tpgiv, tpsge }).ToList().Where(r => !r.tpgiv.v.HierarchyPeriod.HasEntrance(
                    r.tpsge.DateStart.HasValue ? r.tpsge.DateStart.Value.DateYearStart() : DateStart,
                    r.tpsge.DateEnd.HasValue ? r.tpsge.DateEnd.Value.DateYearEnd() : DateEnd))
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
        /// Контроль "Проверка наличия строк в ТЧ «Ресурсное обеспечение»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0317", InitialCaption = "Проверка наличия строк в ТЧ «Ресурсное обеспечение»", InitialManaged = true)]
        public void Control_0317(DataContext context)
        {
            const string Msg = "В документе нет ни одной строки в таблице «Ресурсное обеспечение»";

            if (!context.ActivityOfSBP_ResourceMaintenance.Any(r => r.IdOwner == this.Id))
                Controls.Throw(Msg);
        }

        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения документа за один период в разрезе ИФ и без разреза по ИФ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0318", InitialCaption = "Проверка наличия ресурсного обеспечения документа за один период в разрезе ИФ и без разреза по ИФ")]
        public void Control_0318(DataContext context)
        {
            const string sMsg = "За один период не допускается указывать сумму ресурсного обеспечения в разрезе источников финансирования и без источника финансирования." +
                                "Ошибки обнаружены в таблице «{0}» по периодам {1}";

            var resourceMaintenances = tpResourceMaintenance.Join(tpResourceMaintenance_Value, a => a.Id,
                                                                  v => v.IdMaster, (a, v) => new { a, v });

            var rm0s = resourceMaintenances.Where(r => !r.a.IdFinanceSource.HasValue);
            if (rm0s.Any())
            {
                var rm0 = rm0s.Select(s => s.v.HierarchyPeriod);

                var errrm = resourceMaintenances.Where(r =>
                                                       r.a.IdFinanceSource.HasValue &&
                                                       rm0.Any(r0 => r.v.HierarchyPeriod.HasIntersection(r0)))
                                                .OrderBy(o => o.v.HierarchyPeriod.DateStart)
                                                .Select(s => s.v.HierarchyPeriod.Caption);


                if (errrm.Any())
                {
                    Controls.Throw(string.Format(sMsg, "Ресурсное обеспечение",
                                                                     errrm.Aggregate((a, b) => a + ", " + b)));
                }
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения мероприятия за один период в разрезе ИФ и без разреза по ИФ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0322", InitialCaption = "Проверка наличия ресурсного обеспечения мероприятия за один период в разрезе ИФ и без разреза по ИФ")]
        public void Control_0322(DataContext context)
        {
            var sMsg = "За один период не допускается указывать сумму ресурсного обеспечения " +
                       "в разрезе источников финансирования и без источника финансирования.<br>" +
                       "Ошибки обнаружены в таблице «Ресурсное обеспечение мероприятий»:<br>" +
                       "{0}";

            var activityResourceMaintenances =
                tpActivityResourceMaintenance.Join(tpActivityResourceMaintenance_Value, a => a.Id, v => v.IdMaster,
                                                   (a, v) => new { a, v });

            var rma0s = activityResourceMaintenances.Where(r => !r.a.IdFinanceSource.HasValue);
            if (rma0s.Any())
            {
                var rm0 = rma0s.Select(s => new { s.a.Master, s.v.HierarchyPeriod });

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
                        }).ToList();

                if (errrm.Any())
                {
                    var ms = new StringBuilder();
                    foreach (var sg in errrm.Select(s => s.e).Distinct().OrderBy(o => o.SystemGoal.Caption))
                    {
                        ms.AppendFormat("Элемент СЦ «{0}»<br>", sg.SystemGoal.Caption);
                        foreach (var atp in errrm.Where(r => r.e == sg).Select(s => s.a).Distinct().OrderBy(o => o.Activity.Caption))
                        {
                            ms.AppendFormat(" - {0} - {1}<br>", atp.Activity.Caption,
                                            (atp.IdContingent.HasValue ? atp.Contingent.Caption : ""));

                            var h = errrm.Where(r => r.e == sg && r.a == atp)
                                         .Select(s => s.p)
                                         .OrderBy(o => o)
                                         .Aggregate((a, b) => a + ", " + b);

                            ms.AppendFormat("Периоды: {0}.<br>", h);
                        }

                        Controls.Throw(string.Format(sMsg, ms));
                    }
                }
            }
        }

        /// <summary>   
        /// Контроль "Наличие мероприятий у элементов СЦ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0319", InitialCaption = "Наличие мероприятий у элементов СЦ")]
        public void Control_0319(DataContext context)
        {
            var sMsg = "У следующих элементов СЦ отсутствуют мероприятия:<br>" +
                      "{0}";

            var err = tpSystemGoalElement.Where(s => !tpSystemGoalElement.Any(r => r.IdParent == s.Id) && !s.FromAnotherDocumentSE && !tpActivity.Where(a => a.IdMaster == s.Id).Any()).Select(s => s.SystemGoal.Caption).ToList();

            if (err.Any())
                Controls.Throw(string.Format(sMsg, err.Aggregate((a, b) => a + "<br>" + b)));
        }


        /// <summary>   
        /// Контроль "Наличие объемов у мероприятий"
        /// </summary> 
        [ControlInitial(InitialUNK = "0320", InitialCaption = "Наличие объемов у мероприятий")]
        public void Control_0320(DataContext context)
        {
            var sMsg =
                "У следующих мероприятий не указаны значения показателей объема:<br>" +
                "{0}";

            var err = tpActivity.Where(a => !tpActivity_Value.Any(v => v.IdMaster == a.Id)).ToList();

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
                               .Select(s => string.Format(" - {0}{1}", s.a, s.c));

                    ms.AppendFormat("{0}<br>", act.Aggregate((a, b) => a + "<br>" + b));
                }

                Controls.Throw(string.Format(sMsg, ms));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия строк в ТЧ «Ресурсное обеспечение мероприятий»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0321", InitialCaption = "Проверка наличия строк в ТЧ «Ресурсное обеспечение мероприятий»", InitialSkippable = true, InitialManaged = true)]
        public void Control_0321(DataContext context)
        {
            const string sMsg = "У следующих мероприятий не указаны объемы ресурсного обеспечения:<br>" +
                                "{0}";

            var err =
                tpActivity.Where(a => tpActivityResourceMaintenance.All(v => v.IdMaster != a.Id))
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
        /// Контроль "Проверка на равенство объемов ресурсного обеспечения мероприятий c объемами финансирования программы"
        /// </summary> 
        [ControlInitial(InitialUNK = "0323", InitialCaption = "Проверка на равенство объемов ресурсного обеспечения мероприятий c объемами финансирования программы", InitialManaged = true)]
        public void Control_0323(DataContext context)
        {
            #region Часть 1

            var sMsg = "Сумма ресурсного обеспечения мероприятий не равна сумме ресурсного обеспечения всего документа.<br>" +
                                "Неравенство обнаружено по строкам:" +
                                "{0}";

            var rm =
                tpResourceMaintenance
                    .Join(tpResourceMaintenance_Value, a => a.Id,
                          v => v.IdMaster, (a, v) => new { a, v })
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

            var act_rm = tpActivityResourceMaintenance
                .Join(
                    tpActivityResourceMaintenance_Value, a => a.Id,
                    v => v.IdMaster, (a, v) => new { a, v })
                .ToList()
                .GroupBy(g =>
                         new StateProgram.ResPair
                         {
                             fs = g.a.FinanceSource,
                             hp = g.v.HierarchyPeriod
                         })
                .Select(s =>
                        new StateProgram.KeyVal
                        {
                            Key = s.Key,
                            Value = s.Sum(ss => (ss.v.Value ?? 0))
                        });

            var rm0 = rm.Where(r => !act_rm.Any(u => u.Key.fs == r.Key.fs && u.Key.hp == r.Key.hp))
                        .Select(s =>
                                new StateProgram.KeyValD
                                {
                                    Key = s.Key,
                                    Value1 = s.Value,
                                    Value2 = 0
                                });

            var unRm0 = act_rm.Where(r => !rm.Any(u => u.Key.fs == r.Key.fs && u.Key.hp == r.Key.hp))
                              .Select(s =>
                                      new StateProgram.KeyValD
                                      {
                                          Key = s.Key,
                                          Value1 = 0,
                                          Value2 = s.Value
                                      });

            var diffrm = from r in rm
                         join u in act_rm on new { r.Key.fs, r.Key.hp } equals new { u.Key.fs, u.Key.hp }
                         where r.Value != u.Value
                         select new StateProgram.KeyValD
                         {
                             Key = r.Key,
                             Value1 = r.Value,
                             Value2 = u.Value
                         };

            var err = rm0.Union(unRm0).Union(diffrm).OrderBy(s => s.Key.hp.DateStart);

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg,
                                                                 err.Select(s => s.ToString("Сумма по мероприятиям"))
                                                                    .Aggregate((a, b) => a + "<br>" + b)));
            }

            #endregion Часть 1

            #region Часть 2

            if (HasMasterDoc)
            {
                sMsg =
                    "Сумма доп.потребностей ресурсного обеспечения мероприятий не равна сумме доп.потребностей ресурсного обеспечения всего документа.<br>" +
                    "Неравенство обнаружено по строкам:" +
                    "{0}";

                rm =
                    tpResourceMaintenance
                        .Join(tpResourceMaintenance_Value, a => a.Id,
                              v => v.IdMaster, (a, v) => new {a, v})
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

                act_rm = tpActivityResourceMaintenance
                    .Join(
                        tpActivityResourceMaintenance_Value, a => a.Id,
                        v => v.IdMaster, (a, v) => new {a, v})
                    .ToList()
                    .GroupBy(g =>
                             new StateProgram.ResPair
                                 {
                                     fs = g.a.FinanceSource,
                                     hp = g.v.HierarchyPeriod
                                 })
                    .Select(s =>
                            new StateProgram.KeyVal
                                {
                                    Key = s.Key,
                                    Value = s.Sum(ss => (ss.v.AdditionalValue ?? 0))
                                });

                rm0 = rm.Where(r => !act_rm.Any(u => u.Key.fs == r.Key.fs && u.Key.hp == r.Key.hp))
                        .Select(s =>
                                new StateProgram.KeyValD
                                    {
                                        Key = s.Key,
                                        Value1 = s.Value,
                                        Value2 = 0
                                    });

                unRm0 = act_rm.Where(r => !rm.Any(u => u.Key.fs == r.Key.fs && u.Key.hp == r.Key.hp))
                              .Select(s =>
                                      new StateProgram.KeyValD
                                          {
                                              Key = s.Key,
                                              Value1 = 0,
                                              Value2 = s.Value
                                          });

                diffrm = from r in rm
                         join u in act_rm on new {r.Key.fs, r.Key.hp} equals new {u.Key.fs, u.Key.hp}
                         where r.Value != u.Value
                         select new StateProgram.KeyValD
                             {
                                 Key = r.Key,
                                 Value1 = r.Value,
                                 Value2 = u.Value
                             };

                err = rm0.Union(unRm0).Union(diffrm).OrderBy(s => s.Key.hp.DateStart);

                if (err.Any())
                {
                    Controls.Throw(string.Format(sMsg,
                                                 err.Select(s => s.ToString("Сумма по мероприятиям"))
                                                    .Aggregate((a, b) => a + "<br>" + b)));
                }
            }

            #endregion Часть 1
        }

        /// <summary>   
        /// Контроль "Наличие показателей качества у мероприятия"
        /// </summary> 
        [ControlInitial(InitialUNK = "0324", InitialCaption = "Наличие показателей качества у мероприятия", InitialSkippable = true, InitialManaged = true)]
        public void Control_0324(DataContext context)
        {
            var sMsg = "У следующих мероприятий не указаны показатели качества:<br>" +
                       "{0}";

            var err = tpActivity.Where(r => !tpIndicatorQualityActivity.Any(i => i.IdMaster == r.Id))
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
        [ControlInitial(InitialUNK = "0325", InitialCaption = "Наличие значений показателей качества у мероприятий")]
        public void Control_0325(DataContext context)
        {
            var sMsg = "У следующих мероприятий не указаны значения показателей качества:<br>" +
                       "{0}";

            var err =
                (from t in tpIndicatorQualityActivity.Where(iqa => !tpIndicatorQualityActivity_Value.Any(r => r.IdMaster == iqa.Id))
                 join a in tpActivity on t.IdMaster equals a.Id
                 join m in tpSystemGoalElement on a.IdMaster equals m.Id
                 select new
                 {
                     sg = m.SystemGoal,
                     m = a,
                     qa = t.IndicatorActivity.Caption
                 })
                                          .Distinct();

            if (err.Any())
            {
                var ss = new StringBuilder();
                foreach (var lsg in err.Select(s => s.sg).Distinct().OrderBy(o => o.Caption))
                {
                    ss.AppendFormat("Элемент СЦ «{0}»<br>", lsg.Caption);

                    var las = err.Where(s => Equals(s.sg, lsg))
                                 .Select(s => s.m)
                                 .OrderBy(o => o.Activity.Caption);

                    foreach (var la in las)
                    {
                        ss.Append(
                            string.Format("{0}{1}", la.Activity.Caption,
                                          (la.IdContingent.HasValue ? " - контингент " + la.Contingent.Caption : "")) + "<br>");

                        var lia =
                            err.Where(r => Equals(r.sg, lsg) && Equals(r.m, la))
                               .Select(s => s.qa)
                               .OrderBy(o => o)
                               .Distinct();

                        ss.Append(lia.Aggregate((a, b) => a + "<br>" + b) + "<br>");
                    }
                }
                Controls.Throw(string.Format(sMsg, ss));
            }
        }

        /// <summary>   
        /// Контроль "Проверка утверждения элементов из другого документа СЦ"
        /// </summary> 
        [ControlInitial(InitialUNK = "0327", InitialCaption = "Проверка утверждения элементов из другого документа СЦ")]
        public void Control_0327(DataContext context)
        {
            var sMsg = "Необходимо скорректировать дату текущего документа. <br>" +
                       "Следующие элементы СЦ, которые являются вышестоящими для элементов из текущего документа, не утверждены или утверждены более поздней датой:<br>" +
                       "{0}<br>" +
                       "Дата текущего документа: {1}"
                       ;

            var errh =
                from tpGse in context.ActivityOfSBP_SystemGoalElement.Where(r =>
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
        /// Контроль "Проверка наличия в системе вышестоящего документа на статусе «Проект» или «Утвержден»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0328", InitialCaption = "Проверка наличия в системе вышестоящего документа на статусе «Проект» или «Утвержден»")]
        public void Control_0328(DataContext context)
        {
            if (!IdMasterDoc.HasValue)
                return;

            const string msg = "Вышестоящий документ {0} находится на статусе отличном от «Проект» или «Утвержден»";

            var lastRevisionId = CommonMethods.FindLastRevisionId(context, StateProgram.EntityIdStatic, IdMasterDoc.Value);
            if (lastRevisionId == 0)
                throw new PlatformException("Отсутствует вышестоящий документ! Возможные проблемы: 1. dbo.GetLastVersionId, 2. сломан ключ в базе");
            
            var masterDocument = context.StateProgram.FirstOrDefault(r => r.Id == lastRevisionId);
            if (masterDocument == null)
                throw new PlatformException("Отсутствует вышестоящий документ! Возможные проблемы: dbo.GetLastVersionId");

            if (masterDocument.IdDocStatus != DocStatus.Project && masterDocument.IdDocStatus != DocStatus.Approved)
                Controls.Throw(string.Format(msg, masterDocument));
        }

        /// <summary>   
        /// Контроль "Проверка наличия одинаковых мероприятий в разных программах"
        /// </summary> 
        [ControlInitial(InitialUNK = "0329", InitialCaption = "Проверка наличия одинаковых мероприятий в разных программах")]
        public void Control_0329(DataContext context)
        {
            var sMsg = "Следующие мероприятия уже осуществляются в рамках других программ:<br>" +
                       "{0}<br>";

            var err = (from tp in context.ActivityOfSBP_Activity
                                         .Where(r => r.IdOwner == Id)
                                         .Select(s =>
                                                 new
                                                 {
                                                     s.Activity,
                                                     s.Contingent,
                                                     s.SBP
                                                 })
                                         .Distinct()
                       join tc in
                           context.TaskCollection.Where(r => r.IdPublicLegalFormation == IdPublicLegalFormation)
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
                                                r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                r.IdVersion == IdVersion &&
                                                !r.IdTerminator.HasValue &&
                                                r.IdRegistrator != Id &&
                                                !_arrIdParent.Contains(r.IdRegistrator))
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
                                                                   IdPublicLegalFormation &&
                                                                   r.IdVersion == IdVersion &&
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
                foreach (var prog in err.Select(s => new { s.prog, s.nameprog }).Distinct().OrderBy(o => o.nameprog))
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
        [ControlInitial(InitialUNK = "0330", InitialCaption = "Проверка наличия в документе нескольких элементов СЦ с признаком «Основная цель»")]
        public void Control_0330(DataContext context)
        {
            var sMsg = "В таблице «Элементы СЦ» не должно быть несколько основных целей.";


            var erD = tpSystemGoalElement.Where(r => r.IsMainGoal);

            if (erD.Count() > 1)
            {
                Controls.Throw(sMsg);
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0331", InitialCaption = "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение»")]
        public void Control_0331(DataContext context)
        {
            var sMsg = "В таблице «Ресурсное обеспечение» необходимо заполнить сумму, хотя бы за один период по строке:<br>" +
                "{0}";


            var err = tpResourceMaintenance.Where(r => !tpResourceMaintenance_Value.Any(v => v.IdMaster == r.Id))
                                           .Select(s => s.IdFinanceSource.HasValue ? s.FinanceSource.Caption : "<не указано>")
                                           .ToList();

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg,
                                                                 err.Select(s => "Источник: " + s)
                                                                    .Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение мероприятий»"
        /// </summary> 
        [ControlInitial(InitialUNK = "0332", InitialCaption = "Проверка наличия хотя бы одного значения по строке в таблице «Ресурсное обеспечение мероприятий»")]
        public void Control_0332(DataContext context)
        {
            var sMsg = "В таблице «Ресурсное обеспечение мероприятий» необходимо заполнить сумму, хотя бы за один период по строке:<br>" +
                       "{0}";


            var err = tpActivityResourceMaintenance.Where(a => !tpActivityResourceMaintenance_Value.Any(v => v.IdMaster == a.Id)).ToList();

            if (err.Any())
            {
                var erD = from r in err
                          join a in tpActivity on r.IdMaster equals a.Id
                          join s in tpSystemGoalElement on a.IdMaster equals s.Id
                          select new { r, a, s };

                var ms = new StringBuilder();
                foreach (var el in erD.Select(r => r.s).Distinct().OrderBy(o => o.SystemGoal.Caption))
                {
                    ms.AppendFormat("Элемент СЦ «{0}»<br>", el.SystemGoal.Caption);

                    foreach (var act in erD.Where(t => t.s == el).Select(r => r.a).Distinct().OrderBy(o => o.Activity.Caption))
                    {
                        ms.AppendFormat("-{0}{1}<br>", act.Activity.Caption,
                                        (act.IdContingent.HasValue ? " - " + act.Contingent.Caption : ""));

                        var isf =
                            erD.Where(t => t.a == act)
                               .Select(r => r.r)
                               .Select(s => s.IdFinanceSource.HasValue ? s.FinanceSource.Caption : "<не указано>");

                        ms.AppendFormat("Источник: {0}<br>", isf.Aggregate((a, b) => a + "," + b));
                    }
                }

                Controls.Throw(string.Format(sMsg, ms));
            }
        }

        /// <summary>   
        /// Контроль "Очистка доп. потребностей"
        /// </summary> 
        [ControlInitial(InitialSkippable = true, InitialCaption = "Очистка доп. потребностей", InitialUNK = "0334")]
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 50)]
        public void Control_0334(DataContext context, ActivityOfSBP old)
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
        public void AutoControl_0334end(DataContext context)
        {
            if (!this.HasAdditionalNeed)
            {
                DocSGEMethod.DeclineAddValueInTp(context, ActivityOfSBP_ResourceMaintenance_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, ActivityOfSBP_Activity_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, ActivityOfSBP_ActivityResourceMaintenance_Value.EntityIdStatic, this.Id);
                DocSGEMethod.DeclineAddValueInTp(context, ActivityOfSBP_IndicatorQualityActivity_Value.EntityIdStatic, this.Id);
            }
        }



        /// <summary>   
        /// Контроль "Проверка срока реализации документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0342", InitialCaption = "Проверка элементов СЦ на соответствие реквизитам документа", InitialSkippable = true)]
        public void Control_0342(DataContext context, int[] items)
        {
            //список индетификаторов элементов СЦ из справочника «Система целеполагания» 
            var tpsgid = context.ActivityOfSBP_SystemGoalElement.Where(w => items.Contains(w.Id) && w.FromAnotherDocumentSE == false
                                                                                                 && w.IsMainGoal == true).Select(d => d.IdSystemGoal);

            switch (tpsgid.Count())
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
                                                            && tpsgid.Contains(w.Id)
                                                            && w.IdSBP == IdSBP
                                                            //&& w.IdDocType_CommitDoc == IdDocType
                                                            

                        ).Select(t => t.Id).ToList();
                    //список не актуальных СЦ из ТЧ документа
                    var res = context.ActivityOfSBP_SystemGoalElement.Where(w => items.Contains(w.Id)).Except(context.ActivityOfSBP_SystemGoalElement.Where(d => items.Contains(d.Id) && actuldoc.Contains(d.IdSystemGoal)));
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
                            context.ActivityOfSBP_SystemGoalElement.Remove(item);
                        }
                        context.SaveChanges();
                    }
                    break;
            }


            //список индетификаторов элементов СЦ из справочника «Система целеполагания» 
            tpsgid = context.ActivityOfSBP_SystemGoalElement.Where(w => items.Contains(w.Id) && w.FromAnotherDocumentSE == false
                                                                                                 && w.IsMainGoal == false).Select(d => d.IdSystemGoal);

            if (tpsgid.Any())
            {
                
                    //актулальные записи выделенных строк тч СЦ из справочника «Система целеполагания»
                    var actuldoc = context.SystemGoal.Where(w =>
                                                            w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && (w.IdDocType_CommitDoc == IdDocType || w.IdDocType_ImplementDoc == IdDocType)
                                                            && w.DateStart >= DateStart && w.DateEnd <= DateEnd
                                                            && w.IdRefStatus == (byte)RefStats.Work
                                                            && tpsgid.Contains(w.Id)
                                                            && w.IdSBP == IdSBP
                        //&& w.IdDocType_CommitDoc == IdDocType


                        ).Select(t => t.Id).ToList();
                    //список не актуальных СЦ из ТЧ документа
                    var res = context.ActivityOfSBP_SystemGoalElement.Where(w => items.Contains(w.Id)).Except(context.ActivityOfSBP_SystemGoalElement.Where(d => items.Contains(d.Id) && actuldoc.Contains(d.IdSystemGoal)));
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
                                "Следующие элементы СЦ справочника «Система целеполагания» не соответствуют реквизитам документа «Тип документа», «Ответственный исполнитель», «Срок реализации с», «Срок реализации по»:{0} <br>" +
                                "Удалить данные элементы из таблицы «Элементы СЦ» документа?", str);
                        Controls.Throw(st);
                        foreach (var item in res)
                        {
                            context.ActivityOfSBP_SystemGoalElement.Remove(item);
                        }
                        context.SaveChanges();
                    }
            }

        }



        /// <summary>   
        /// Проверка даты прекращения
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка даты прекращения", InitialUNK = "0339")]
        public void Control_0339(DataContext context)
        {
            var sMsg = "Дата прекращения не может быть меньше даты утверждения.<br>" +
                       "Дата утверждения: {0}<br>" +
                       "Дата прекращения: {1}.";

            if (this.DateTerminate.HasValue && this.Date > this.DateTerminate)
            {
                Controls.Throw(string.Format(sMsg, this.Date.ToString("dd.MM.yyyy"), this.DateTerminate.Value.ToString("dd.MM.yyyy")));
            }
        }

        /// <summary>   
        /// Проверка реализации мероприятий в других программах
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка реализации мероприятий в других программах", InitialUNK = "0341")]
        public void Control_0341(DataContext context)
        {
            const string sMsg = "Нельзя отменить прекращение документа. Следующие мероприятия уже реализуются в рамках других программ: <br>" +
                                "{0}";

            var taskVolumes = context.TaskVolume.Where(r =>
                r.IdPublicLegalFormation == IdPublicLegalFormation &&
                r.IdVersion == IdVersion &&
                (!r.IdTerminator.HasValue || r.IdTerminator.HasValue && r.DateTerminate > DateTerminate)).ToList();

            var tpActivityV = (from a in tpActivity
                               join v in tpActivity_Value.Where(r => r.Value > 0) on a.Id equals v.IdMaster
                               select new
                               {
                                   a,
                                   v
                               }).ToList();

            var err = (from a in tpActivityV
                       join tv in taskVolumes on
                           new
                           {
                               tc = RegisterMethods.FindTaskCollection(context, this.IdPublicLegalFormation, a.a.IdActivity, a.a.IdContingent),
                               ye = a.v.HierarchyPeriod.Year,
                               sbp = a.a.SBP
                           }
                           equals
                           new
                           {
                               tc = tv.TaskCollection,
                               ye = tv.HierarchyPeriod.Year,
                               sbp = tv.SBP
                           }
                       /*                        orderby new
                                               {
                                                   cSbp = tv.SBP.Caption,
                                                   tc = tv.TaskCollection.Activity.Caption,
                                                   period = tv.HierarchyPeriod.Year
                                               }*/
                       select new
                       {
                           tc = tv.TaskCollection,
                           year = tv.HierarchyPeriod.Year,
                           cSbp = tv.SBP,
                           iddoc = tv.IdRegistrator,
                           identity = tv.IdRegistratorEntity
                       }).ToList();

            var sb = new StringBuilder();

            foreach (var er in err)
            {
                if (_arrIdParent.Contains(er.iddoc) && er.identity == this.EntityId)
                {
                    continue;
                }

                string cdoc;
                var doc = context.Set<IIdentitied>(er.identity).FirstOrDefault(i => i.Id == er.iddoc);

                if (doc is ActivityOfSBP)
                {
                    cdoc = ((ActivityOfSBP)doc).Header;
                }
                else if (doc is LongTermGoalProgram)
                {
                    cdoc = ((LongTermGoalProgram)doc).Header;
                }
                else if (doc is StateProgram)
                {
                    cdoc = ((StateProgram)doc).Header;
                }
                else
                {
                    cdoc = doc.ToString();
                }

                sb.AppendFormat("- {0} - <br>" +
                                "{1}{2} <br>" +
                                "Период: {3} ({4})",
                    er.cSbp.Caption,
                    er.tc.Activity.Caption,
                    !er.tc.IdContingent.HasValue ? "" : (" - " + er.tc.Contingent.Caption),
                    er.year,
                    cdoc);
            }

            if (sb.ToString() != string.Empty)
            {
                Controls.Throw(string.Format(sMsg, sb));
            }
        }

        /// <summary>   
        /// Проверка использования мероприятий из прекращаемой ДВ
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка использования мероприятий из прекращаемой ДВ", InitialUNK = "0340")]
        public void Control_0340(DataContext context)
        {
            const string msg = "Для прекращения действия документа необходимо исключить следующие мероприятия из ЭД «План деятельности» по учреждениям: <br>" +
                       "{0}";
            
            var sbpIds = context.Database.SqlQuery<int>("Select * From dbo.GetChildrens("+ IdSBP +", (" +
                                                            "Select TOP 1 EF.id from ref.Entity E " +
                                                            "Left Join ref.EntityField EF on EF.[idEntity] = E.id " +
                                                            "Where E.Name = 'SBP' and EF.Name = 'idParent'), " +
                                                        "0)  ").ToArray();

            var query = context.TaskVolume.Where(tv => tv.IdVersion == IdVersion &&
                                                       (!tv.DateTerminate.HasValue || tv.DateTerminate > DateTerminate ) &&
                                                       sbpIds.Contains(tv.IdSBP));

            var errors = new List<string>();

            foreach (var activity in Activity.ToList() )
            {

                var result = query.Where(tv => tv.TaskCollection.IdActivity == activity.IdActivity &&
                                               tv.TaskCollection.IdContingent == activity.IdContingent &&
                                               tv.HierarchyPeriod.Year ==
                                               context.ActivityOfSBP_Activity_Value.Where(
                                                   a => a.IdMaster == activity.Id)
                                                       .Select(v => v.HierarchyPeriod.Year).FirstOrDefault());
                if ( result.Any( ) )
                {
                    var err = new StringBuilder(
                                          activity.Activity.Caption +
                                          (activity.Contingent == null ? "" : (" - " + activity.Contingent.Caption)));
                    
                    var temp = result.Select(r => new {r.HierarchyPeriod.Year, r.SBP.Caption}).ToList();

                    foreach (var sbpItem in temp.Select(s=>s.Caption).ToList() )
                    {
                        var years = String.Join(",", temp.Where(s=>s.Caption == sbpItem).Select(s=>s.Year).ToList());
                        err.Append( "<br/> &nbsp; -" + sbpItem + ". Период: " + years + " ");    
                    }

                    errors.Add(err.ToString());
                }
            }

            if ( errors.Any() )
                Controls.Throw(string.Format(msg, string.Join("<br/>", errors) ));
            
           /* var taskVolumes = context.TaskVolume.Where(r =>
                r.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                r.IdVersion == this.IdVersion &&
                (!r.IdTerminator.HasValue || r.IdTerminator.HasValue && r.DateTerminate > this.DateTerminate));

            var tpActivityV = (from a in tpActivity
                               join v in tpActivity_Value.Where(r => r.Value > 0) on a.Id equals v.IdMaster
                               select new
                               {
                                   tc = DocSGEMethod.FindTaskCollection(context, this.IdPublicLegalFormation, a.IdActivity, a.IdContingent),
                                   a,
                                   v
                               });

            var coltv = (from a in tpActivityV
                        join tv in taskVolumes on
                            new
                            {
                                tc = a.tc.Id,
                                ye = a.v.HierarchyPeriod.Year
                            }
                            equals
                            new
                            {
                                tc = tv.TaskCollection.Id,
                                ye = tv.HierarchyPeriod.Year
                            }
                        select new tmpCtv()
                        {
                            tc = tv.TaskCollection,
                            year = tv.HierarchyPeriod.Year,
                            pSbp = a.a.SBP,
                            cSbp = tv.SBP,
                            iddoc = tv.IdRegistrator,
                            identity = tv.IdRegistratorEntity
                        }).ToList();

            var sb = new StringBuilder();
            
            var sbps = context.SBP.Where(r => r.IdPublicLegalFormation == this.IdPublicLegalFormation).ToList();
            var err = coltv.ToList().Where(r => IsParentSbp(sbps, r)).Distinct().ToList();

            foreach (var tc in err.Select(s => s.tc).OrderBy(o => o).Distinct())
            {
                sb.AppendFormat("{0}{1}<br>", tc.Activity.Caption, !tc.IdContingent.HasValue ? "" : " - " + tc.Contingent.Caption);

                foreach (var cSbp in err.Where(r => r.tc == tc).Select(s => s.cSbp).OrderBy(o => o).Distinct())
                {
                    var years = err.Where(r => r.tc == tc && r.cSbp == cSbp).Select(s => s.year.ToString()).OrderBy(o => o).Distinct();

                    sb.AppendFormat(" - {0}. Период: {1}<br>", cSbp.Caption, years.Aggregate((a, b) => a + ", " + b));
                }
            }

            if (sb.ToString() != string.Empty)
            {
                Controls.Throw(string.Format(sMsg, sb));
            }*/
        }


        /// <summary>   
        /// Контроль "Проверка элементов СЦ на соответствие реквизитам документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0344", InitialCaption = "Проверка элементов СЦ на соответствие реквизитам документа")]
        public void Control_0344(DataContext context)
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
        /// Проверка заполнения строк ресурсного обеспечения в соответствии с бланком формирования ГРБС
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка заполнения строк ресурсного обеспечения в соответствии с бланком формирования ГРБС", InitialUNK = "0346")]
        public void Control_0346(DataContext context)
        {
            var actrms = context.ActivityOfSBP_ActivityResourceMaintenance.Where(r => r.IdOwner == this.Id && r.IdBudget.HasValue).ToList();

            SBP sbp;
            IBudget curbudget;
            var sbpBlanks = GetBlanks(context, this.SBP, out sbp, out curbudget);

            if (!sbpBlanks.Any((r => r.IdBlankType == (int)DbEnums.BlankType.FormationGRBS)))
            {
                return;
            }

            var sbpBlank = sbpBlanks.FirstOrDefault((r => r.IdBlankType == (int)DbEnums.BlankType.FormationGRBS));
  
            if (actrms.Any(a => !sbpBlank.CheckByBlank(a)))
            {
                Controls.Throw("В таблице «Ресурсное обеспечение мероприятий» указаны строки, не соответствующие бланку «Формирование ГРБС».");
            }

        }

        /// <summary>   
        /// Проверка заполнения строк действующими КБК
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка заполнения строк действующими КБК", InitialUNK = "0347")]
        public void Control_0347(DataContext context)
        {
            SBP sbp;
            IBudget curbudget;
            var sbpBlanks = GetBlanks(context, this.SBP, out sbp, out curbudget);

            if (!sbpBlanks.Any(r => r.IdBlankType == (int)DbEnums.BlankType.FormationGRBS))
            {
                return;
            }

            //Мероприятие(Контингент) : Список ошибок
            var errors = this.GetWrongVersioningKBK(sbpBlanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.FormationGRBS), typeof(ActivityOfSBP_ActivityResourceMaintenance), "ActivityOfSBP_Activity", context);

            if (errors.Any())
            {
                var errorMsg = new StringBuilder("В таблице «Ресурсное обеспечение мероприятий» указаны строки с недействующими КБК (выделены жирным шрифтом):<br/>");

                foreach (var error in errors)
                {
                    errorMsg.Append("<br/>" + error.Key);

                    foreach (var value in error.Value)
                        errorMsg.Append("<br/>" + value);
                }

                Controls.Throw(errorMsg.ToString());
            }

        }

        /// <summary>   
        ///Проверка на не превышение предельных объемов ассигнований
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований", InitialUNK = "0348")]
        public void Control_0348(DataContext context)
        {
            var strerr = Control_0348_0350(context);

            if (strerr.ToString() != string.Empty)
            {
                Controls.Throw(string.Format("Объем обоснованных ассигнований превышает предельный объем бюджетных ассигнований по следующим ведомствам:<br>{0}",
                    strerr));
            }
        }

        /// <summary>   
        ///Проверка на не превышение предельных объемов ассигнований по доп. потребностям
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований по доп. потребностям", InitialUNK = "0350")]
        public void Control_0350(DataContext context)
        {
            var strerr = Control_0348_0350(context, true);

            if (strerr.ToString() != string.Empty)
            {
                Controls.Throw(string.Format("Объем обоснованных ассигнований по дополнительным потребностям, превышает предельный объем бюджетных ассигнований по дополнительным потребностям по следующим ведомствам:<br>{0}",
                    strerr));
            }
        }

        /// <summary>
        /// общий метод проверки для контролей 0348 и 0350
        /// </summary>
        /// <returns>строки с превышением</returns>
        private string Control_0348_0350(DataContext context, bool fadditional = false)
        {
            var tpResourceMaintenanceV =
                (from rm in tpActivityResourceMaintenance.Where(r => r.IdBudget.HasValue && (r.FinanceSource.IdFinanceSourceType == (byte)FinanceSourceType.RegionalBudgetRF || r.FinanceSource.IdFinanceSourceType == (byte)FinanceSourceType.FederalBudget))
                 join rmv in tpActivityResourceMaintenance_Value on rm.Id equals rmv.IdMaster
                 join act in tpActivity on rm.IdMaster equals act.Id
                 join sbp in context.SBP.Where(r => r.IdPublicLegalFormation == this.IdPublicLegalFormation) on act.IdSBP equals sbp.Id
                 select new
                 {
                     rm,
                     rmv,
                     act,
                     sbp
                 }).ToList();

            const byte mandatory = (byte)DbEnums.BlankValueType.Mandatory;

            var idBudget = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id;
            // Предварительная выборка из регистра «Объемы финансовых средств»:
            //ППО=ШапкаДокумента.ППО
            //Бюджет=Текущее системное измерение.Бюджет
            //Версия=ШапкаДокумента.Версия
            //Тип значения = План или Доведено
            //Средства АУ/БУ = ложь

            var sbps = tpResourceMaintenanceV.Select(s => s.sbp).Distinct();
            var sbpIds = sbps.Select(s => s.Id).Union(sbps.Select(s => s.IdParent ?? 0)).Distinct().ToArray();

            var lva = context.LimitVolumeAppropriations.Where(r =>
                                                              r.IdBudget == idBudget &&
                                                              (r.EstimatedLine.FinanceSource.IdFinanceSourceType == (byte)FinanceSourceType.RegionalBudgetRF || 
                                                              r.EstimatedLine.FinanceSource.IdFinanceSourceType == (byte)FinanceSourceType.FederalBudget) &&
                                                              r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                              r.IdVersion == IdVersion &&
                                                              !r.IsMeansAUBU &&
                                                              sbpIds.Contains(r.EstimatedLine.IdSBP) &&
                                                              (r.HasAdditionalNeed ?? false) == fadditional &&
                                                              (((r.IdValueType == (int)DbEnums.ValueType.Plan || 
                                                              r.IdValueType == (int)DbEnums.ValueType.Bring) && 
                                                              r.IdRegistratorEntity == LimitBudgetAllocations.EntityIdStatic)
                                                              || (r.IdValueType == (int)DbEnums.ValueType.JustifiedGRBS && !_arrIdParent.Contains(r.IdRegistrator)))
                                                              ).
                              Select(s => new
                              {
                                  estl = s.EstimatedLine,
                                  reg = s,
                                  sbp = s.EstimatedLine.SBP
                              }).
                              ToList();

            var lvac = lva.Where(r => r.reg.IdValueType == (int) DbEnums.ValueType.JustifiedGRBS).ToList();

            var sbpblanks = context.SBP_Blank.Where(r => sbpIds.Contains(r.IdOwner)).ToList();

            var strerr = new StringBuilder();

            foreach (var asbp in sbps.OrderBy(o => o.Caption))
            {
                var strerrBySbp = new StringBuilder();

                if (asbp.IdSBPType == (int)DbEnums.SBPType.GeneralManager)
                {
                    var blankFormationGRBS = sbpblanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.FormationGRBS && r.IdOwner == asbp.Id);
                    var blankBringingGRBS = sbpblanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.BringingGRBS && r.IdOwner == asbp.Id);

                    #region раздел a) из постановки

                    var fFinanceSource = (blankFormationGRBS.IdBlankValueType_FinanceSource == mandatory && blankBringingGRBS.IdBlankValueType_FinanceSource == mandatory);
                    var fKCSR = (blankFormationGRBS.IdBlankValueType_KCSR == mandatory && blankBringingGRBS.IdBlankValueType_KCSR == mandatory);
                    var fKFO = (blankFormationGRBS.IdBlankValueType_KFO == mandatory && blankBringingGRBS.IdBlankValueType_KFO == mandatory);
                    var fKOSGU = (blankFormationGRBS.IdBlankValueType_KOSGU == mandatory && blankBringingGRBS.IdBlankValueType_KOSGU == mandatory);
                    var fKVR = (blankFormationGRBS.IdBlankValueType_KVR == mandatory && blankBringingGRBS.IdBlankValueType_KVR == mandatory);
                    var fKVSR = (blankFormationGRBS.IdBlankValueType_KVSR == mandatory && blankBringingGRBS.IdBlankValueType_KVSR == mandatory);
                    var fDEK = (blankFormationGRBS.IdBlankValueType_DEK == mandatory && blankBringingGRBS.IdBlankValueType_DEK == mandatory);
                    var fDFK = (blankFormationGRBS.IdBlankValueType_DFK == mandatory && blankBringingGRBS.IdBlankValueType_DFK == mandatory);
                    var fDKR = (blankFormationGRBS.IdBlankValueType_DKR == mandatory && blankBringingGRBS.IdBlankValueType_DKR == mandatory);
                    var fRZPR = (blankFormationGRBS.IdBlankValueType_RZPR == mandatory && blankBringingGRBS.IdBlankValueType_RZPR == mandatory);
                    var fBranchCode = (blankFormationGRBS.IdBlankValueType_BranchCode == mandatory && blankBringingGRBS.IdBlankValueType_BranchCode == mandatory);
                    var fCodeSubsidy = (blankFormationGRBS.IdBlankValueType_CodeSubsidy == mandatory && blankBringingGRBS.IdBlankValueType_CodeSubsidy == mandatory);
                    var fExpenseObligationType = (blankFormationGRBS.IdBlankValueType_ExpenseObligationType == mandatory && blankBringingGRBS.IdBlankValueType_ExpenseObligationType == mandatory);

                    var fFinanceSourceb = (blankBringingGRBS.IdBlankValueType_FinanceSource == mandatory);
                    var fKCSRb = (blankBringingGRBS.IdBlankValueType_KCSR == mandatory);
                    var fKFOb = (blankBringingGRBS.IdBlankValueType_KFO == mandatory);
                    var fKOSGUb = (blankBringingGRBS.IdBlankValueType_KOSGU == mandatory);
                    var fKVRb = (blankBringingGRBS.IdBlankValueType_KVR == mandatory);
                    var fKVSRb = (blankBringingGRBS.IdBlankValueType_KVSR == mandatory);
                    var fDEKb = (blankBringingGRBS.IdBlankValueType_DEK == mandatory);
                    var fDFKb = (blankBringingGRBS.IdBlankValueType_DFK == mandatory);
                    var fDKRb = (blankBringingGRBS.IdBlankValueType_DKR == mandatory);
                    var fRZPRb = (blankBringingGRBS.IdBlankValueType_RZPR == mandatory);
                    var fBranchCodeb = (blankBringingGRBS.IdBlankValueType_BranchCode == mandatory);
                    var fCodeSubsidyb = (blankBringingGRBS.IdBlankValueType_CodeSubsidy == mandatory);
                    var fExpenseObligationTypeb = (blankBringingGRBS.IdBlankValueType_ExpenseObligationType == mandatory);

                    // 1.1 - сумма документа
                    var rm = tpResourceMaintenanceV.Where(r => r.sbp == asbp).
                                                    Select(s => new
                                                    {
                                                        FinanceSource = fFinanceSource ? s.rm.FinanceSource : null,
                                                        KCSR = fKCSR ? s.rm.KCSR : null,
                                                        KFO = fKFO ? s.rm.KFO : null,
                                                        KOSGU = fKOSGU ? s.rm.KOSGU : null,
                                                        KVR = fKVR ? s.rm.KVR : null,
                                                        KVSR = fKVSR ? s.rm.KVSR : null,
                                                        DEK = fDEK ? s.rm.DEK : null,
                                                        DFK = fDFK ? s.rm.DFK : null,
                                                        DKR = fDKR ? s.rm.DKR : null,
                                                        RZPR = fRZPR ? s.rm.RZPR : null,
                                                        BranchCode = fBranchCode ? s.rm.BranchCode : null,
                                                        CodeSubsidy = fCodeSubsidy ? s.rm.CodeSubsidy : null,
                                                        ExpenseObligationType = fExpenseObligationType ? s.rm.ExpenseObligationType : null,
                                                        hp = s.rmv.HierarchyPeriod,
                                                        Value = fadditional ? (s.rmv.AdditionalValue ?? 0) : (s.rmv.Value ?? 0)
                                                    }).
                                                    GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp }).
                                                    Select(s =>
                                                           new CLineRM()
                                                           {
                                                               line = new EstimatedLine()
                                                               {
                                                                   FinanceSource = s.Key.FinanceSource,
                                                                   KCSR = s.Key.KCSR,
                                                                   KFO = s.Key.KFO,
                                                                   KOSGU = s.Key.KOSGU,
                                                                   KVR = s.Key.KVR,
                                                                   KVSR = s.Key.KVSR,
                                                                   DEK = s.Key.DEK,
                                                                   DFK = s.Key.DFK,
                                                                   DKR = s.Key.DKR,
                                                                   RZPR = s.Key.RZPR,
                                                                   BranchCode = s.Key.BranchCode,
                                                                   CodeSubsidy = s.Key.CodeSubsidy,
                                                                   ExpenseObligationType = s.Key.ExpenseObligationType,
                                                                   SBP = asbp,
                                                                   PublicLegalFormation = this.PublicLegalFormation
                                                               },
                                                               hp = s.Key.hp,
                                                               Value = s.Sum(ss => ss.Value)
                                                           });

                    // 2.2 - План ГРБС
                    var lvaPlan = lva.
                        Where(r => r.estl.SBP.Id == asbp.Id && r.reg.IdValueType == (int)DbEnums.ValueType.Plan).//СБП = ТЧ.Мероприятия.Исполнитель и Тип значения = План
                        Select(s => new
                        {
                            FinanceSource = fFinanceSource ? s.estl.FinanceSource : null,
                            KCSR = fKCSR ? s.estl.KCSR : null,
                            KFO = fKFO ? s.estl.KFO : null,
                            KOSGU = fKOSGU ? s.estl.KOSGU : null,
                            KVR = fKVR ? s.estl.KVR : null,
                            KVSR = fKVSR ? s.estl.KVSR : null,
                            DEK = fDEK ? s.estl.DEK : null,
                            DFK = fDFK ? s.estl.DFK : null,
                            DKR = fDKR ? s.estl.DKR : null,
                            RZPR = fRZPR ? s.estl.RZPR : null,
                            BranchCode = fBranchCode ? s.estl.BranchCode : null,
                            CodeSubsidy = fCodeSubsidy ? s.estl.CodeSubsidy : null,
                            ExpenseObligationType = fExpenseObligationType ? s.estl.ExpenseObligationType : null,
                            hp = s.reg.HierarchyPeriod,
                            Value = s.reg.Value,
                            sbp = s.estl.SBP
                        }).
                        GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp, g.sbp }).
                        Select(s =>
                                new CLineRM()
                                {
                                    line = new EstimatedLine()
                                    {
                                        FinanceSource = s.Key.FinanceSource,
                                        KCSR = s.Key.KCSR,
                                        KFO = s.Key.KFO,
                                        KOSGU = s.Key.KOSGU,
                                        KVR = s.Key.KVR,
                                        KVSR = s.Key.KVSR,
                                        DEK = s.Key.DEK,
                                        DFK = s.Key.DFK,
                                        DKR = s.Key.DKR,
                                        RZPR = s.Key.RZPR,
                                        BranchCode = s.Key.BranchCode,
                                        CodeSubsidy = s.Key.CodeSubsidy,
                                        ExpenseObligationType = s.Key.ExpenseObligationType,
                                        SBP = asbp,
                                        PublicLegalFormation = this.PublicLegalFormation
                                    },
                                    hp = s.Key.hp,
                                    Value = s.Sum(ss => ss.Value),
                                    sbp = s.Key.sbp
                                });

                    // 3. - Доведено
                    var lvaDoved = lva.
                        Where(r => r.estl.SBP.Id == asbp.Id && r.reg.IdValueType == (int)DbEnums.ValueType.Bring). //СБП = ТЧ.Мероприятия.Исполнитель и Тип значения = Доведено
                        Select(s => new
                        {
                            FinanceSource = fFinanceSourceb ? s.estl.FinanceSource : null,
                            KCSR = fKCSRb ? s.estl.KCSR : null,
                            KFO = fKFOb ? s.estl.KFO : null,
                            KOSGU = fKOSGUb ? s.estl.KOSGU : null,
                            KVR = fKVRb ? s.estl.KVR : null,
                            KVSR = fKVSRb ? s.estl.KVSR : null,
                            DEK = fDEKb ? s.estl.DEK : null,
                            DFK = fDFKb ? s.estl.DFK : null,
                            DKR = fDKRb ? s.estl.DKR : null,
                            RZPR = fRZPRb ? s.estl.RZPR : null,
                            BranchCode = fBranchCodeb ? s.estl.BranchCode : null,
                            CodeSubsidy = fCodeSubsidyb ? s.estl.CodeSubsidy : null,
                            ExpenseObligationType = fExpenseObligationTypeb ? s.estl.ExpenseObligationType : null,
                            hp = s.reg.HierarchyPeriod,
                            Value = s.reg.Value,
                            sbp = s.estl.SBP
                        }).
                        GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp, g.sbp }).
                        Select(s =>
                                new CLineRM()
                                {
                                    line = new EstimatedLine()
                                    {
                                        FinanceSource = s.Key.FinanceSource,
                                        KCSR = s.Key.KCSR,
                                        KFO = s.Key.KFO,
                                        KOSGU = s.Key.KOSGU,
                                        KVR = s.Key.KVR,
                                        KVSR = s.Key.KVSR,
                                        DEK = s.Key.DEK,
                                        DFK = s.Key.DFK,
                                        DKR = s.Key.DKR,
                                        RZPR = s.Key.RZPR,
                                        BranchCode = s.Key.BranchCode,
                                        CodeSubsidy = s.Key.CodeSubsidy,
                                        ExpenseObligationType = s.Key.ExpenseObligationType,
                                        SBP = asbp,
                                        PublicLegalFormation = this.PublicLegalFormation
                                    },
                                    hp = s.Key.hp,
                                    Value = s.Sum(ss => ss.Value),
                                    sbp = s.Key.sbp
                                });

                    // 4. Обосновано ГРБС
                    var lvaObasGRBS = lva.
                        Where(r =>
                            r.estl.SBP.Id == asbp.Id && r.reg.IdValueType == (int)DbEnums.ValueType.JustifiedGRBS). //СБП = ТЧ.Мероприятия.Исполнитель и Тип значения = Обосновано ГРБС
                        Select(s => new
                        {
                            FinanceSource = fFinanceSource ? s.estl.FinanceSource : null,
                            KCSR = fKCSR ? s.estl.KCSR : null,
                            KFO = fKFO ? s.estl.KFO : null,
                            KOSGU = fKOSGU ? s.estl.KOSGU : null,
                            KVR = fKVR ? s.estl.KVR : null,
                            KVSR = fKVSR ? s.estl.KVSR : null,
                            DEK = fDEK ? s.estl.DEK : null,
                            DFK = fDFK ? s.estl.DFK : null,
                            DKR = fDKR ? s.estl.DKR : null,
                            RZPR = fRZPR ? s.estl.RZPR : null,
                            BranchCode = fBranchCode ? s.estl.BranchCode : null,
                            CodeSubsidy = fCodeSubsidy ? s.estl.CodeSubsidy : null,
                            ExpenseObligationType = fExpenseObligationType ? s.estl.ExpenseObligationType : null,
                            hp = s.reg.HierarchyPeriod,
                            Value = s.reg.Value,
                            sbp = s.estl.SBP
                        }).
                        GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp, g.sbp }).
                        Select(s =>
                                new CLineRM()
                                {
                                    line = new EstimatedLine()
                                    {
                                        FinanceSource = s.Key.FinanceSource,
                                        KCSR = s.Key.KCSR,
                                        KFO = s.Key.KFO,
                                        KOSGU = s.Key.KOSGU,
                                        KVR = s.Key.KVR,
                                        KVSR = s.Key.KVSR,
                                        DEK = s.Key.DEK,
                                        DFK = s.Key.DFK,
                                        DKR = s.Key.DKR,
                                        RZPR = s.Key.RZPR,
                                        BranchCode = s.Key.BranchCode,
                                        CodeSubsidy = s.Key.CodeSubsidy,
                                        ExpenseObligationType = s.Key.ExpenseObligationType,
                                        SBP = asbp,
                                        PublicLegalFormation = this.PublicLegalFormation
                                    },
                                    hp = s.Key.hp,
                                    Value = s.Sum(ss => ss.Value),
                                    sbp = s.Key.sbp
                                });

                    // 5. Для каждой сгруппированной строки с «Обосновано ГРБС»  попытаться  найти соответствующую ей сгруппированную строку с «План ГРБС» и с «Доведено». 
                    var sravn = from line in rm
                                join plan in lvaPlan on
                                    new
                                    {
                                        FinanceSource = line.line.FinanceSource,
                                        KCSR = line.line.KCSR,
                                        KFO = line.line.KFO,
                                        KOSGU = line.line.KOSGU,
                                        KVR = line.line.KVR,
                                        KVSR = line.line.KVSR,
                                        DEK = line.line.DEK,
                                        DFK = line.line.DFK,
                                        DKR = line.line.DKR,
                                        RZPR = line.line.RZPR,
                                        BranchCode = line.line.BranchCode,
                                        CodeSubsidy = line.line.CodeSubsidy,
                                        ExpenseObligationType = line.line.ExpenseObligationType,
                                        hp = line.hp
                                    }
                                    equals
                                    new
                                    {
                                        FinanceSource = plan.line.FinanceSource,
                                        KCSR = plan.line.KCSR,
                                        KFO = plan.line.KFO,
                                        KOSGU = plan.line.KOSGU,
                                        KVR = plan.line.KVR,
                                        KVSR = plan.line.KVSR,
                                        DEK = plan.line.DEK,
                                        DFK = plan.line.DFK,
                                        DKR = plan.line.DKR,
                                        RZPR = plan.line.RZPR,
                                        BranchCode = plan.line.BranchCode,
                                        CodeSubsidy = plan.line.CodeSubsidy,
                                        ExpenseObligationType = plan.line.ExpenseObligationType,
                                        hp = plan.hp
                                    }
                                    into planT
                                from plan0 in planT.DefaultIfEmpty()
                                join doved in lvaDoved on
                                    new
                                    {
                                        FinanceSource = line.line.FinanceSource,
                                        KCSR = line.line.KCSR,
                                        KFO = line.line.KFO,
                                        KOSGU = line.line.KOSGU,
                                        KVR = line.line.KVR,
                                        KVSR = line.line.KVSR,
                                        DEK = line.line.DEK,
                                        DFK = line.line.DFK,
                                        DKR = line.line.DKR,
                                        RZPR = line.line.RZPR,
                                        BranchCode = line.line.BranchCode,
                                        CodeSubsidy = line.line.CodeSubsidy,
                                        ExpenseObligationType = line.line.ExpenseObligationType,
                                        hp = line.hp
                                    }
                                    equals
                                    new
                                    {
                                        FinanceSource = doved.line.FinanceSource,
                                        KCSR = doved.line.KCSR,
                                        KFO = doved.line.KFO,
                                        KOSGU = doved.line.KOSGU,
                                        KVR = doved.line.KVR,
                                        KVSR = doved.line.KVSR,
                                        DEK = doved.line.DEK,
                                        DFK = doved.line.DFK,
                                        DKR = doved.line.DKR,
                                        RZPR = doved.line.RZPR,
                                        BranchCode = doved.line.BranchCode,
                                        CodeSubsidy = doved.line.CodeSubsidy,
                                        ExpenseObligationType = doved.line.ExpenseObligationType,
                                        hp = doved.hp
                                    }
                                    into dovedT
                                from doved0 in dovedT.DefaultIfEmpty()
                                join obasGrbs in lvaObasGRBS on
                                    new
                                    {
                                        FinanceSource = line.line.FinanceSource,
                                        KCSR = line.line.KCSR,
                                        KFO = line.line.KFO,
                                        KOSGU = line.line.KOSGU,
                                        KVR = line.line.KVR,
                                        KVSR = line.line.KVSR,
                                        DEK = line.line.DEK,
                                        DFK = line.line.DFK,
                                        DKR = line.line.DKR,
                                        RZPR = line.line.RZPR,
                                        BranchCode = line.line.BranchCode,
                                        CodeSubsidy = line.line.CodeSubsidy,
                                        ExpenseObligationType = line.line.ExpenseObligationType,
                                        hp = line.hp
                                    }
                                    equals
                                    new
                                    {
                                        FinanceSource = obasGrbs.line.FinanceSource,
                                        KCSR = obasGrbs.line.KCSR,
                                        KFO = obasGrbs.line.KFO,
                                        KOSGU = obasGrbs.line.KOSGU,
                                        KVR = obasGrbs.line.KVR,
                                        KVSR = obasGrbs.line.KVSR,
                                        DEK = obasGrbs.line.DEK,
                                        DFK = obasGrbs.line.DFK,
                                        DKR = obasGrbs.line.DKR,
                                        RZPR = obasGrbs.line.RZPR,
                                        BranchCode = obasGrbs.line.BranchCode,
                                        CodeSubsidy = obasGrbs.line.CodeSubsidy,
                                        ExpenseObligationType = obasGrbs.line.ExpenseObligationType,
                                        hp = obasGrbs.hp
                                    }
                                    into obasGrbsT
                                from obasGrbs0 in obasGrbsT.DefaultIfEmpty()
                                select
                                    new
                                    {
                                        line.line,
                                        line.hp,
                                        sum = line.Value,
                                        plan = (plan0 == null) ? 0 : plan0.Value,
                                        doved = (doved0 == null) ? 0 : doved0.Value,
                                        obosnovano = (obasGrbs0 == null) ? 0 : obasGrbs0.Value
                                    };

                    var gsravn = sravn.GroupBy(g =>
                                               new
                                               {
                                                   g.line.FinanceSource,
                                                   g.line.KCSR,
                                                   g.line.KFO,
                                                   g.line.KOSGU,
                                                   g.line.KVR,
                                                   g.line.KVSR,
                                                   g.line.DEK,
                                                   g.line.DFK,
                                                   g.line.DKR,
                                                   g.line.RZPR,
                                                   g.line.BranchCode,
                                                   g.line.CodeSubsidy,
                                                   g.line.ExpenseObligationType,
                                                   g.hp
                                               }).
                                       Select(s =>
                                              new
                                              {
                                                  line = s.Key,
                                                  sum = s.Sum(ss => ss.sum),
                                                  plan = s.Sum(ss => ss.plan),
                                                  doved = s.Sum(ss => ss.doved),
                                                  obosnovano = s.Sum(ss => ss.obosnovano)
                                              }).
                                       Select(s =>
                                              new
                                              {
                                                  s.line,
                                                  s.sum,
                                                  s.plan,
                                                  s.doved,
                                                  s.obosnovano,
                                                  razn = s.plan - s.obosnovano - s.doved - s.sum
                                              }).
                                       Where(r => !(r.razn >= 0));

                    foreach (var line in gsravn)
                    {
                        var str =
                            (fExpenseObligationTypeb ? ", Тип РО: " + (line.line.ExpenseObligationType == null ? "" : ((ExpenseObligationType)line.line.ExpenseObligationType).Caption()) : "") +
                            (fFinanceSourceb ? ", ИФ: " + (line.line.FinanceSource == null ? "" : line.line.FinanceSource.Code) : "") +
                            (fKFOb ? ", КФО: " + (line.line.KFO == null ? "" : line.line.KFO.Code) : "") +
                            (fKVSRb ? ", КВСР: " + (line.line.KVSR == null ? "" : line.line.KVSR.Caption) : "") +
                            (fRZPRb ? ", РЗПР: " + (line.line.RZPR == null ? "" : line.line.RZPR.Code) : "") +
                            (fKCSRb ? ", КЦСР: " + (line.line.KCSR == null ? "" : line.line.KCSR.Code) : "") +
                            (fKVRb ? ", КВР: " + (line.line.KVR == null ? "" : line.line.KVR.Code) : "") +
                            (fKOSGUb ? ", КОСГУ: " + (line.line.KOSGU == null ? "" : line.line.KOSGU.Code) : "") +
                            (fDKRb ? ", ДКР: " + (line.line.DKR == null ? "" : line.line.DKR.Code) : "") +
                            (fDEKb ? ", ДЭК: " + (line.line.DEK == null ? "" : line.line.DEK.Code) : "") +
                            (fDFKb ? ", ДФК: " + (line.line.DFK == null ? "" : line.line.DFK.Code) : "") +
                            (fCodeSubsidyb ? ", Код субсидии: " + (line.line.CodeSubsidy == null ? "" : line.line.CodeSubsidy.Code) : "") +
                            (fBranchCodeb ? ", Отраслевой код: " + (line.line.BranchCode == null ? "" : line.line.BranchCode.Code) : "");

                        strerrBySbp.AppendFormat(
                            "{0}{1}, - Объем средств = {2}, Обоснованные ассигнования = {3}, Распределено = {4}, Сумма документа ={5}, Разность ={6}<br>",
                            line.line.hp.DateStart.Year.ToString(),
                            str,
                            line.plan.ToString(),
                            line.obosnovano.ToString(),
                            line.doved.ToString(),
                            line.sum.ToString(),
                            line.razn.ToString());
                    }

                    #endregion
                }
                else
                {
                    var blankFormationGRBS = sbpblanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.FormationGRBS && r.IdOwner == asbp.IdParent);
                    var blankBringingGRBS = sbpblanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.BringingGRBS && r.IdOwner == asbp.IdParent);
                    var blankBringingRBS = sbpblanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.BringingRBS && r.IdOwner == asbp.IdParent);

                    #region раздел b) из постановки

                    var fFinanceSource = (blankFormationGRBS.IdBlankValueType_FinanceSource == mandatory && blankBringingRBS.IdBlankValueType_FinanceSource == mandatory);
                    var fKCSR = (blankFormationGRBS.IdBlankValueType_KCSR == mandatory && blankBringingRBS.IdBlankValueType_KCSR == mandatory);
                    var fKFO = (blankFormationGRBS.IdBlankValueType_KFO == mandatory && blankBringingRBS.IdBlankValueType_KFO == mandatory);
                    var fKOSGU = (blankFormationGRBS.IdBlankValueType_KOSGU == mandatory && blankBringingRBS.IdBlankValueType_KOSGU == mandatory);
                    var fKVR = (blankFormationGRBS.IdBlankValueType_KVR == mandatory && blankBringingRBS.IdBlankValueType_KVR == mandatory);
                    var fKVSR = (blankFormationGRBS.IdBlankValueType_KVSR == mandatory && blankBringingRBS.IdBlankValueType_KVSR == mandatory);
                    var fDEK = (blankFormationGRBS.IdBlankValueType_DEK == mandatory && blankBringingRBS.IdBlankValueType_DEK == mandatory);
                    var fDFK = (blankFormationGRBS.IdBlankValueType_DFK == mandatory && blankBringingRBS.IdBlankValueType_DFK == mandatory);
                    var fDKR = (blankFormationGRBS.IdBlankValueType_DKR == mandatory && blankBringingRBS.IdBlankValueType_DKR == mandatory);
                    var fRZPR = (blankFormationGRBS.IdBlankValueType_RZPR == mandatory && blankBringingRBS.IdBlankValueType_RZPR == mandatory);
                    var fBranchCode = (blankFormationGRBS.IdBlankValueType_BranchCode == mandatory && blankBringingRBS.IdBlankValueType_BranchCode == mandatory);
                    var fCodeSubsidy = (blankFormationGRBS.IdBlankValueType_CodeSubsidy == mandatory && blankBringingRBS.IdBlankValueType_CodeSubsidy == mandatory);
                    var fExpenseObligationType = (blankFormationGRBS.IdBlankValueType_ExpenseObligationType == mandatory && blankBringingRBS.IdBlankValueType_ExpenseObligationType == mandatory);

                    // 1.1 - сумма документа
                    var rm = tpResourceMaintenanceV.Where(r => r.sbp == asbp).
                                                    Select(s => new
                                                    {
                                                        FinanceSource = fFinanceSource ? s.rm.FinanceSource : null,
                                                        KCSR = fKCSR ? s.rm.KCSR : null,
                                                        KFO = fKFO ? s.rm.KFO : null,
                                                        KOSGU = fKOSGU ? s.rm.KOSGU : null,
                                                        KVR = fKVR ? s.rm.KVR : null,
                                                        KVSR = fKVSR ? s.rm.KVSR : null,
                                                        DEK = fDEK ? s.rm.DEK : null,
                                                        DFK = fDFK ? s.rm.DFK : null,
                                                        DKR = fDKR ? s.rm.DKR : null,
                                                        RZPR = fRZPR ? s.rm.RZPR : null,
                                                        BranchCode = fBranchCode ? s.rm.BranchCode : null,
                                                        CodeSubsidy = fCodeSubsidy ? s.rm.CodeSubsidy : null,
                                                        ExpenseObligationType = fExpenseObligationType ? s.rm.ExpenseObligationType : null,
                                                        hp = s.rmv.HierarchyPeriod,
                                                        Value = fadditional ? (s.rmv.AdditionalValue ?? 0) : (s.rmv.Value ?? 0)
                                                    }).
                                                    GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp }).
                                                    Select(s =>
                                                           new CLineRM()
                                                           {
                                                               line = new EstimatedLine()
                                                               {
                                                                   FinanceSource = s.Key.FinanceSource,
                                                                   KCSR = s.Key.KCSR,
                                                                   KFO = s.Key.KFO,
                                                                   KOSGU = s.Key.KOSGU,
                                                                   KVR = s.Key.KVR,
                                                                   KVSR = s.Key.KVSR,
                                                                   DEK = s.Key.DEK,
                                                                   DFK = s.Key.DFK,
                                                                   DKR = s.Key.DKR,
                                                                   RZPR = s.Key.RZPR,
                                                                   BranchCode = s.Key.BranchCode,
                                                                   CodeSubsidy = s.Key.CodeSubsidy,
                                                                   ExpenseObligationType = s.Key.ExpenseObligationType,
                                                                   SBP = asbp,
                                                                   PublicLegalFormation = this.PublicLegalFormation
                                                               },
                                                               hp = s.Key.hp,
                                                               Value = s.Sum(ss => ss.Value)
                                                           });


                    // 2 - План ГРБС
                    var lvaPlan = lva.
                        Where(r => r.estl.SBP.Id == asbp.Id && r.reg.IdValueType == (int)DbEnums.ValueType.Plan).//СБП = ТЧ.Мероприятия.Исполнитель и Тип значения = План
                        Select(s => new
                        {
                            FinanceSource = fFinanceSource ? s.estl.FinanceSource : null,
                            KCSR = fKCSR ? s.estl.KCSR : null,
                            KFO = fKFO ? s.estl.KFO : null,
                            KOSGU = fKOSGU ? s.estl.KOSGU : null,
                            KVR = fKVR ? s.estl.KVR : null,
                            KVSR = fKVSR ? s.estl.KVSR : null,
                            DEK = fDEK ? s.estl.DEK : null,
                            DFK = fDFK ? s.estl.DFK : null,
                            DKR = fDKR ? s.estl.DKR : null,
                            RZPR = fRZPR ? s.estl.RZPR : null,
                            BranchCode = fBranchCode ? s.estl.BranchCode : null,
                            CodeSubsidy = fCodeSubsidy ? s.estl.CodeSubsidy : null,
                            ExpenseObligationType = fExpenseObligationType ? s.estl.ExpenseObligationType : null,
                            hp = s.reg.HierarchyPeriod,
                            Value = s.reg.Value,
                            sbp = s.estl.SBP
                        }).
                        GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp, g.sbp }).
                        Select(s =>
                                new CLineRM()
                                {
                                    line = new EstimatedLine()
                                    {
                                        FinanceSource = s.Key.FinanceSource,
                                        KCSR = s.Key.KCSR,
                                        KFO = s.Key.KFO,
                                        KOSGU = s.Key.KOSGU,
                                        KVR = s.Key.KVR,
                                        KVSR = s.Key.KVSR,
                                        DEK = s.Key.DEK,
                                        DFK = s.Key.DFK,
                                        DKR = s.Key.DKR,
                                        RZPR = s.Key.RZPR,
                                        BranchCode = s.Key.BranchCode,
                                        CodeSubsidy = s.Key.CodeSubsidy,
                                        ExpenseObligationType = s.Key.ExpenseObligationType,
                                        SBP = asbp,
                                        PublicLegalFormation = this.PublicLegalFormation
                                    },
                                    hp = s.Key.hp,
                                    Value = s.Sum(ss => ss.Value),
                                    sbp = s.Key.sbp
                                });

                    // 3 - обосновано ГРБС
                    var lvaObasGRBS = lva.
                         Where(r =>
                             r.estl.SBP.Id == asbp.Id && r.reg.IdValueType == (int)DbEnums.ValueType.JustifiedGRBS). //СБП = ТЧ.Мероприятия.Исполнитель и Тип значения = Обосновано ГРБС
                         Select(s => new
                         {
                             FinanceSource = fFinanceSource ? s.estl.FinanceSource : null,
                             KCSR = fKCSR ? s.estl.KCSR : null,
                             KFO = fKFO ? s.estl.KFO : null,
                             KOSGU = fKOSGU ? s.estl.KOSGU : null,
                             KVR = fKVR ? s.estl.KVR : null,
                             KVSR = fKVSR ? s.estl.KVSR : null,
                             DEK = fDEK ? s.estl.DEK : null,
                             DFK = fDFK ? s.estl.DFK : null,
                             DKR = fDKR ? s.estl.DKR : null,
                             RZPR = fRZPR ? s.estl.RZPR : null,
                             BranchCode = fBranchCode ? s.estl.BranchCode : null,
                             CodeSubsidy = fCodeSubsidy ? s.estl.CodeSubsidy : null,
                             ExpenseObligationType = fExpenseObligationType ? s.estl.ExpenseObligationType : null,
                             hp = s.reg.HierarchyPeriod,
                             Value = s.reg.Value,
                             sbp = s.estl.SBP
                         }).
                         GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp, g.sbp }).
                         Select(s =>
                                 new CLineRM()
                                 {
                                     line = new EstimatedLine()
                                     {
                                         FinanceSource = s.Key.FinanceSource,
                                         KCSR = s.Key.KCSR,
                                         KFO = s.Key.KFO,
                                         KOSGU = s.Key.KOSGU,
                                         KVR = s.Key.KVR,
                                         KVSR = s.Key.KVSR,
                                         DEK = s.Key.DEK,
                                         DFK = s.Key.DFK,
                                         DKR = s.Key.DKR,
                                         RZPR = s.Key.RZPR,
                                         BranchCode = s.Key.BranchCode,
                                         CodeSubsidy = s.Key.CodeSubsidy,
                                         ExpenseObligationType = s.Key.ExpenseObligationType,
                                         SBP = asbp,
                                         PublicLegalFormation = this.PublicLegalFormation
                                     },
                                     hp = s.Key.hp,
                                     Value = s.Sum(ss => ss.Value),
                                     sbp = s.Key.sbp
                                 });

                    // 4. Для каждой сгруппированной строки с «Обосновано ГРБС»  попытаться  найти соответствующую ей сгруппированную строку с «План ГРБС»  
                    var sravn = from line in rm
                                join plan in lvaPlan on
                                    new
                                    {
                                        FinanceSource = line.line.FinanceSource,
                                        KCSR = line.line.KCSR,
                                        KFO = line.line.KFO,
                                        KOSGU = line.line.KOSGU,
                                        KVR = line.line.KVR,
                                        KVSR = line.line.KVSR,
                                        DEK = line.line.DEK,
                                        DFK = line.line.DFK,
                                        DKR = line.line.DKR,
                                        RZPR = line.line.RZPR,
                                        BranchCode = line.line.BranchCode,
                                        CodeSubsidy = line.line.CodeSubsidy,
                                        ExpenseObligationType = line.line.ExpenseObligationType,
                                        hp = line.hp
                                    }
                                    equals
                                    new
                                    {
                                        FinanceSource = plan.line.FinanceSource,
                                        KCSR = plan.line.KCSR,
                                        KFO = plan.line.KFO,
                                        KOSGU = plan.line.KOSGU,
                                        KVR = plan.line.KVR,
                                        KVSR = plan.line.KVSR,
                                        DEK = plan.line.DEK,
                                        DFK = plan.line.DFK,
                                        DKR = plan.line.DKR,
                                        RZPR = plan.line.RZPR,
                                        BranchCode = plan.line.BranchCode,
                                        CodeSubsidy = plan.line.CodeSubsidy,
                                        ExpenseObligationType = plan.line.ExpenseObligationType,
                                        hp = plan.hp
                                    }
                                    into planT
                                from plan0 in planT.DefaultIfEmpty()
                                join obasGrbs in lvaObasGRBS on
                                    new
                                    {
                                        FinanceSource = line.line.FinanceSource,
                                        KCSR = line.line.KCSR,
                                        KFO = line.line.KFO,
                                        KOSGU = line.line.KOSGU,
                                        KVR = line.line.KVR,
                                        KVSR = line.line.KVSR,
                                        DEK = line.line.DEK,
                                        DFK = line.line.DFK,
                                        DKR = line.line.DKR,
                                        RZPR = line.line.RZPR,
                                        BranchCode = line.line.BranchCode,
                                        CodeSubsidy = line.line.CodeSubsidy,
                                        ExpenseObligationType = line.line.ExpenseObligationType,
                                        hp = line.hp
                                    }
                                    equals
                                    new
                                    {
                                        FinanceSource = obasGrbs.line.FinanceSource,
                                        KCSR = obasGrbs.line.KCSR,
                                        KFO = obasGrbs.line.KFO,
                                        KOSGU = obasGrbs.line.KOSGU,
                                        KVR = obasGrbs.line.KVR,
                                        KVSR = obasGrbs.line.KVSR,
                                        DEK = obasGrbs.line.DEK,
                                        DFK = obasGrbs.line.DFK,
                                        DKR = obasGrbs.line.DKR,
                                        RZPR = obasGrbs.line.RZPR,
                                        BranchCode = obasGrbs.line.BranchCode,
                                        CodeSubsidy = obasGrbs.line.CodeSubsidy,
                                        ExpenseObligationType = obasGrbs.line.ExpenseObligationType,
                                        hp = obasGrbs.hp
                                    }
                                    into obasGrbsT
                                from obasGrbs0 in obasGrbsT.DefaultIfEmpty()
                                select
                                    new
                                    {
                                        line.line,
                                        line.hp,
                                        sum = line.Value,
                                        plan = (plan0 == null) ? 0 : plan0.Value,
                                        obosnovano = (obasGrbs0 == null) ? 0 : obasGrbs0.Value
                                    };

                    var gsravn = sravn.GroupBy(g =>
                                               new
                                               {
                                                   g.line.FinanceSource,
                                                   g.line.KCSR,
                                                   g.line.KFO,
                                                   g.line.KOSGU,
                                                   g.line.KVR,
                                                   g.line.KVSR,
                                                   g.line.DEK,
                                                   g.line.DFK,
                                                   g.line.DKR,
                                                   g.line.RZPR,
                                                   g.line.BranchCode,
                                                   g.line.CodeSubsidy,
                                                   g.line.ExpenseObligationType,
                                                   g.hp
                                               }).
                                       Select(s =>
                                              new
                                              {
                                                  line = s.Key,
                                                  sum = s.Sum(ss => ss.sum),
                                                  obosnovano = s.Sum(ss => ss.obosnovano),
                                                  plan = s.Sum(ss => ss.plan)
                                              }).
                                       Select(s =>
                                              new
                                              {
                                                  s.line,
                                                  s.sum,
                                                  s.obosnovano,
                                                  s.plan,
                                                  razn = s.plan - s.obosnovano - s.sum
                                              }).
                                       Where(r => !(r.razn >= 0));

                    foreach (var line in gsravn)
                    {
                        var str =
                             (fExpenseObligationType ? ", Тип РО: " + (line.line.ExpenseObligationType == null ? "" : ((ExpenseObligationType)line.line.ExpenseObligationType).Caption()) : "") +
                             (fFinanceSource ? ", ИФ: " + (line.line.FinanceSource == null ? "" : line.line.FinanceSource.Code) : "") +
                             (fKFO ? ", КФО: " + (line.line.KFO == null ? "" : line.line.KFO.Code) : "") +
                             (fKVSR ? ", КВСР: " + (line.line.KVSR == null ? "" : line.line.KVSR.Caption) : "") +
                             (fRZPR ? ", РЗПР: " + (line.line.RZPR == null ? "" : line.line.RZPR.Code) : "") +
                             (fKCSR ? ", КЦСР: " + (line.line.KCSR == null ? "" : line.line.KCSR.Code) : "") +
                             (fKVR ? ", КВР: " + (line.line.KVR == null ? "" : line.line.KVR.Code) : "") +
                             (fKOSGU ? ", КОСГУ: " + (line.line.KOSGU == null ? "" : line.line.KOSGU.Code) : "") +
                             (fDKR ? ", ДКР: " + (line.line.DKR == null ? "" : line.line.DKR.Code) : "") +
                             (fDEK ? ", ДЭК: " + (line.line.DEK == null ? "" : line.line.DEK.Code) : "") +
                             (fDFK ? ", ДФК: " + (line.line.DFK == null ? "" : line.line.DFK.Code) : "") +
                             (fCodeSubsidy ? ", Код субсидии: " + (line.line.CodeSubsidy == null ? "" : line.line.CodeSubsidy.Code) : "") +
                             (fBranchCode ? ", Отраслевой код: " + (line.line.BranchCode == null ? "" : line.line.BranchCode.Code) : "");


                        strerrBySbp.AppendFormat(
                            "{0}{1}, - Объем средств = {2}, Обоснованные ассигнования = {3}, Сумма документа ={5}, Разность ={4}<br>",
                            line.line.hp.DateStart.Year.ToString(),
                            str,
                            line.plan.ToString(),
                            line.obosnovano.ToString(),
                            line.razn.ToString(),
                            line.sum.ToString());
                    }

                    #endregion
                }

                if (strerrBySbp.ToString() != string.Empty)
                {
                    strerr.AppendFormat("Ведомство: {0}<br> Превышение обнаружено по строкам:<br>{1}",
                        asbp.Caption,
                        strerrBySbp.ToString()
                        );
                }
            }
            return strerr.ToString();
        }

        /// <summary>   
        ///Проверка на не превышение предельных объемов ассигнований с учетом даты утверждения
        /// </summary> 
        [ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований с учетом даты утверждения", InitialUNK = "0349")]
        public void Control_0349(DataContext context)
        {
            var tpResourceMaintenanceV =
                (from rm in tpActivityResourceMaintenance.Where(r => r.IdBudget.HasValue)
                 join rmv in tpActivityResourceMaintenance_Value on rm.Id equals rmv.IdMaster
                 join act in tpActivity on rm.IdMaster equals act.Id
                 join sbp in context.SBP.Where(r => r.IdPublicLegalFormation == this.IdPublicLegalFormation) on act.IdSBP equals sbp.Id
                 select new
                 {
                     rm,
                     rmv,
                     act,
                     sbp
                 }).ToList();

            var mandatory = (byte)DbEnums.BlankValueType.Mandatory;

            var idbud = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id;
            // Предварительная выборка из регистра «Объемы финансовых средств»:
            //ППО=ШапкаДокумента.ППО
            //Бюджет=Текущее системное измерение.Бюджет
            //Версия=ШапкаДокумента.Версия
            //Тип значения = План или Доведено
            //Средства АУ/БУ = ложь

            var sbps = tpResourceMaintenanceV.Select(s => s.sbp).Distinct();
            var sbpIds = tpResourceMaintenanceV.Select(s => s.sbp.Id).Union(tpResourceMaintenanceV.Select(s => s.sbp.IdParent ?? 0)).Distinct().ToArray();

            var lva = context.LimitVolumeAppropriations.Where(r =>
                                                              r.IdBudget == idbud &&
                                                              r.EstimatedLine.FinanceSource.IdFinanceSourceType != (byte)FinanceSourceType.UnconfirmedFunds &&
                                                              (r.IdValueType == (int)DbEnums.ValueType.Plan || r.IdValueType == (int)DbEnums.ValueType.Bring) &&
                                                              r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                              r.IdVersion == IdVersion &&
                                                              !r.IsMeansAUBU &&
                                                              !(r.HasAdditionalNeed ?? false) &&
                                                              (r.DateCommit.HasValue && r.DateCommit <= this.Date) &&
                                                              sbpIds.Contains(r.EstimatedLine.IdSBP)
                                                              && r.IdRegistratorEntity == LimitBudgetAllocations.EntityIdStatic
                                                              ).
                              Select(s => new
                              {
                                  estl = s.EstimatedLine,
                                  reg = s,
                                  sbp = s.EstimatedLine.SBP
                              }).
                              ToList();

            var sbpblanks = context.SBP_Blank.Where(r => sbpIds.Contains(r.IdOwner)).ToList();

            var strerr = new StringBuilder();

            foreach (var asbp in sbps.OrderBy(o => o.Caption))
            {
                var blankFormationGRBS = sbpblanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.FormationGRBS && r.IdOwner == asbp.Id);
                var blankBringingGRBS = sbpblanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.BringingGRBS && r.IdOwner == asbp.Id);

                var strerrBySbp = new StringBuilder();

                if (asbp.IdSBPType == (int)DbEnums.SBPType.GeneralManager)
                {
                    #region раздел a) из постановки

                    var fFinanceSource = (blankFormationGRBS.IdBlankValueType_FinanceSource == mandatory && blankBringingGRBS.IdBlankValueType_FinanceSource == mandatory);
                    var fKCSR = (blankFormationGRBS.IdBlankValueType_KCSR == mandatory && blankBringingGRBS.IdBlankValueType_KCSR == mandatory);
                    var fKFO = (blankFormationGRBS.IdBlankValueType_KFO == mandatory && blankBringingGRBS.IdBlankValueType_KFO == mandatory);
                    var fKOSGU = (blankFormationGRBS.IdBlankValueType_KOSGU == mandatory && blankBringingGRBS.IdBlankValueType_KOSGU == mandatory);
                    var fKVR = (blankFormationGRBS.IdBlankValueType_KVR == mandatory && blankBringingGRBS.IdBlankValueType_KVR == mandatory);
                    var fKVSR = (blankFormationGRBS.IdBlankValueType_KVSR == mandatory && blankBringingGRBS.IdBlankValueType_KVSR == mandatory);
                    var fDEK = (blankFormationGRBS.IdBlankValueType_DEK == mandatory && blankBringingGRBS.IdBlankValueType_DEK == mandatory);
                    var fDFK = (blankFormationGRBS.IdBlankValueType_DFK == mandatory && blankBringingGRBS.IdBlankValueType_DFK == mandatory);
                    var fDKR = (blankFormationGRBS.IdBlankValueType_DKR == mandatory && blankBringingGRBS.IdBlankValueType_DKR == mandatory);
                    var fRZPR = (blankFormationGRBS.IdBlankValueType_RZPR == mandatory && blankBringingGRBS.IdBlankValueType_RZPR == mandatory);
                    var fBranchCode = (blankFormationGRBS.IdBlankValueType_BranchCode == mandatory && blankBringingGRBS.IdBlankValueType_BranchCode == mandatory);
                    var fCodeSubsidy = (blankFormationGRBS.IdBlankValueType_CodeSubsidy == mandatory && blankBringingGRBS.IdBlankValueType_CodeSubsidy == mandatory);
                    var fExpenseObligationType = (blankFormationGRBS.IdBlankValueType_ExpenseObligationType == mandatory && blankBringingGRBS.IdBlankValueType_ExpenseObligationType == mandatory);

                    var fFinanceSourceb = (blankBringingGRBS.IdBlankValueType_FinanceSource == mandatory);
                    var fKCSRb = (blankBringingGRBS.IdBlankValueType_KCSR == mandatory);
                    var fKFOb = (blankBringingGRBS.IdBlankValueType_KFO == mandatory);
                    var fKOSGUb = (blankBringingGRBS.IdBlankValueType_KOSGU == mandatory);
                    var fKVRb = (blankBringingGRBS.IdBlankValueType_KVR == mandatory);
                    var fKVSRb = (blankBringingGRBS.IdBlankValueType_KVSR == mandatory);
                    var fDEKb = (blankBringingGRBS.IdBlankValueType_DEK == mandatory);
                    var fDFKb = (blankBringingGRBS.IdBlankValueType_DFK == mandatory);
                    var fDKRb = (blankBringingGRBS.IdBlankValueType_DKR == mandatory);
                    var fRZPRb = (blankBringingGRBS.IdBlankValueType_RZPR == mandatory);
                    var fBranchCodeb = (blankBringingGRBS.IdBlankValueType_BranchCode == mandatory);
                    var fCodeSubsidyb = (blankBringingGRBS.IdBlankValueType_CodeSubsidy == mandatory);
                    var fExpenseObligationTypeb = (blankBringingGRBS.IdBlankValueType_ExpenseObligationType == mandatory);

                    // 1.1 - обосновано ГРБС
                    var rm = tpResourceMaintenanceV.Where(r => r.sbp == asbp).
                                                    Select(s => new
                                                    {
                                                        FinanceSource = fFinanceSource ? s.rm.FinanceSource : null,
                                                        KCSR = fKCSR ? s.rm.KCSR : null,
                                                        KFO = fKFO ? s.rm.KFO : null,
                                                        KOSGU = fKOSGU ? s.rm.KOSGU : null,
                                                        KVR = fKVR ? s.rm.KVR : null,
                                                        KVSR = fKVSR ? s.rm.KVSR : null,
                                                        DEK = fDEK ? s.rm.DEK : null,
                                                        DFK = fDFK ? s.rm.DFK : null,
                                                        DKR = fDKR ? s.rm.DKR : null,
                                                        RZPR = fRZPR ? s.rm.RZPR : null,
                                                        BranchCode = fBranchCode ? s.rm.BranchCode : null,
                                                        CodeSubsidy = fCodeSubsidy ? s.rm.CodeSubsidy : null,
                                                        ExpenseObligationType = fExpenseObligationType ? s.rm.ExpenseObligationType : null,
                                                        hp = s.rmv.HierarchyPeriod,
                                                        Value = s.rmv.Value ?? 0
                                                    }).
                                                    GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp }).
                                                    Select(s =>
                                                           new CLineRM()
                                                           {
                                                               line = new EstimatedLine()
                                                               {
                                                                   FinanceSource = s.Key.FinanceSource,
                                                                   KCSR = s.Key.KCSR,
                                                                   KFO = s.Key.KFO,
                                                                   KOSGU = s.Key.KOSGU,
                                                                   KVR = s.Key.KVR,
                                                                   KVSR = s.Key.KVSR,
                                                                   DEK = s.Key.DEK,
                                                                   DFK = s.Key.DFK,
                                                                   DKR = s.Key.DKR,
                                                                   RZPR = s.Key.RZPR,
                                                                   BranchCode = s.Key.BranchCode,
                                                                   CodeSubsidy = s.Key.CodeSubsidy,
                                                                   ExpenseObligationType = s.Key.ExpenseObligationType,
                                                                   SBP = asbp,
                                                                   PublicLegalFormation = this.PublicLegalFormation
                                                               },
                                                               hp = s.Key.hp,
                                                               Value = s.Sum(ss => ss.Value)
                                                           });


                    // 2.2 - План ГРБС
                    var lvaPlan = lva.
                        Where(r => r.estl.SBP.Id == asbp.Id && r.reg.IdValueType == (int)DbEnums.ValueType.Plan).//СБП = ТЧ.Мероприятия.Исполнитель и Тип значения = План
                        Select(s => new
                        {
                            FinanceSource = fFinanceSource ? s.estl.FinanceSource : null,
                            KCSR = fKCSR ? s.estl.KCSR : null,
                            KFO = fKFO ? s.estl.KFO : null,
                            KOSGU = fKOSGU ? s.estl.KOSGU : null,
                            KVR = fKVR ? s.estl.KVR : null,
                            KVSR = fKVSR ? s.estl.KVSR : null,
                            DEK = fDEK ? s.estl.DEK : null,
                            DFK = fDFK ? s.estl.DFK : null,
                            DKR = fDKR ? s.estl.DKR : null,
                            RZPR = fRZPR ? s.estl.RZPR : null,
                            BranchCode = fBranchCode ? s.estl.BranchCode : null,
                            CodeSubsidy = fCodeSubsidy ? s.estl.CodeSubsidy : null,
                            ExpenseObligationType = fExpenseObligationType ? s.estl.ExpenseObligationType : null,
                            hp = s.reg.HierarchyPeriod,
                            Value = s.reg.Value,
                            sbp = s.estl.SBP
                        }).
                        GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp, g.sbp }).
                        Select(s =>
                                new CLineRM()
                                {
                                    line = new EstimatedLine()
                                    {
                                        FinanceSource = s.Key.FinanceSource,
                                        KCSR = s.Key.KCSR,
                                        KFO = s.Key.KFO,
                                        KOSGU = s.Key.KOSGU,
                                        KVR = s.Key.KVR,
                                        KVSR = s.Key.KVSR,
                                        DEK = s.Key.DEK,
                                        DFK = s.Key.DFK,
                                        DKR = s.Key.DKR,
                                        RZPR = s.Key.RZPR,
                                        BranchCode = s.Key.BranchCode,
                                        CodeSubsidy = s.Key.CodeSubsidy,
                                        ExpenseObligationType = s.Key.ExpenseObligationType,
                                        SBP = asbp,
                                        PublicLegalFormation = this.PublicLegalFormation
                                    },
                                    hp = s.Key.hp,
                                    Value = s.Sum(ss => ss.Value),
                                    sbp = s.Key.sbp
                                });

                    // 3. - Доведено
                    var lvaDoved = lva.
                        Where(r => r.estl.SBP.Id == asbp.Id && r.reg.IdValueType == (int)DbEnums.ValueType.Bring). //СБП = ТЧ.Мероприятия.Исполнитель и Тип значения = Доведено
                        Select(s => new
                        {
                            FinanceSource = fFinanceSourceb ? s.estl.FinanceSource : null,
                            KCSR = fKCSRb ? s.estl.KCSR : null,
                            KFO = fKFOb ? s.estl.KFO : null,
                            KOSGU = fKOSGUb ? s.estl.KOSGU : null,
                            KVR = fKVRb ? s.estl.KVR : null,
                            KVSR = fKVSRb ? s.estl.KVSR : null,
                            DEK = fDEKb ? s.estl.DEK : null,
                            DFK = fDFKb ? s.estl.DFK : null,
                            DKR = fDKRb ? s.estl.DKR : null,
                            RZPR = fRZPRb ? s.estl.RZPR : null,
                            BranchCode = fBranchCodeb ? s.estl.BranchCode : null,
                            CodeSubsidy = fCodeSubsidyb ? s.estl.CodeSubsidy : null,
                            ExpenseObligationType = fExpenseObligationTypeb ? s.estl.ExpenseObligationType : null,
                            hp = s.reg.HierarchyPeriod,
                            Value = s.reg.Value,
                            sbp = s.estl.SBP
                        }).
                        GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp, g.sbp }).
                        Select(s =>
                                new CLineRM()
                                {
                                    line = new EstimatedLine()
                                    {
                                        FinanceSource = s.Key.FinanceSource,
                                        KCSR = s.Key.KCSR,
                                        KFO = s.Key.KFO,
                                        KOSGU = s.Key.KOSGU,
                                        KVR = s.Key.KVR,
                                        KVSR = s.Key.KVSR,
                                        DEK = s.Key.DEK,
                                        DFK = s.Key.DFK,
                                        DKR = s.Key.DKR,
                                        RZPR = s.Key.RZPR,
                                        BranchCode = s.Key.BranchCode,
                                        CodeSubsidy = s.Key.CodeSubsidy,
                                        ExpenseObligationType = s.Key.ExpenseObligationType,
                                        SBP = asbp,
                                        PublicLegalFormation = this.PublicLegalFormation
                                    },
                                    hp = s.Key.hp,
                                    Value = s.Sum(ss => ss.Value),
                                    sbp = s.Key.sbp
                                });

                    // 4. Для каждой сгруппированной строки с «Обосновано ГРБС»  попытаться  найти соответствующую ей сгруппированную строку с «План ГРБС» и с «Доведено». 
                    var sravn = from line in rm
                                join plan in lvaPlan on
                                    new
                                    {
                                        FinanceSource = line.line.FinanceSource,
                                        KCSR = line.line.KCSR,
                                        KFO = line.line.KFO,
                                        KOSGU = line.line.KOSGU,
                                        KVR = line.line.KVR,
                                        KVSR = line.line.KVSR,
                                        DEK = line.line.DEK,
                                        DFK = line.line.DFK,
                                        DKR = line.line.DKR,
                                        RZPR = line.line.RZPR,
                                        BranchCode = line.line.BranchCode,
                                        CodeSubsidy = line.line.CodeSubsidy,
                                        ExpenseObligationType = line.line.ExpenseObligationType,
                                        hp = line.hp
                                    }
                                    equals
                                    new
                                    {
                                        FinanceSource = plan.line.FinanceSource,
                                        KCSR = plan.line.KCSR,
                                        KFO = plan.line.KFO,
                                        KOSGU = plan.line.KOSGU,
                                        KVR = plan.line.KVR,
                                        KVSR = plan.line.KVSR,
                                        DEK = plan.line.DEK,
                                        DFK = plan.line.DFK,
                                        DKR = plan.line.DKR,
                                        RZPR = plan.line.RZPR,
                                        BranchCode = plan.line.BranchCode,
                                        CodeSubsidy = plan.line.CodeSubsidy,
                                        ExpenseObligationType = plan.line.ExpenseObligationType,
                                        hp = plan.hp
                                    }
                                    into planT
                                from plan0 in planT.DefaultIfEmpty()
                                join doved in lvaDoved on
                                    new
                                    {
                                        FinanceSource = line.line.FinanceSource,
                                        KCSR = line.line.KCSR,
                                        KFO = line.line.KFO,
                                        KOSGU = line.line.KOSGU,
                                        KVR = line.line.KVR,
                                        KVSR = line.line.KVSR,
                                        DEK = line.line.DEK,
                                        DFK = line.line.DFK,
                                        DKR = line.line.DKR,
                                        RZPR = line.line.RZPR,
                                        BranchCode = line.line.BranchCode,
                                        CodeSubsidy = line.line.CodeSubsidy,
                                        ExpenseObligationType = line.line.ExpenseObligationType,
                                        hp = line.hp
                                    }
                                    equals
                                    new
                                    {
                                        FinanceSource = doved.line.FinanceSource,
                                        KCSR = doved.line.KCSR,
                                        KFO = doved.line.KFO,
                                        KOSGU = doved.line.KOSGU,
                                        KVR = doved.line.KVR,
                                        KVSR = doved.line.KVSR,
                                        DEK = doved.line.DEK,
                                        DFK = doved.line.DFK,
                                        DKR = doved.line.DKR,
                                        RZPR = doved.line.RZPR,
                                        BranchCode = doved.line.BranchCode,
                                        CodeSubsidy = doved.line.CodeSubsidy,
                                        ExpenseObligationType = doved.line.ExpenseObligationType,
                                        hp = doved.hp
                                    }
                                    into dovedT
                                from doved0 in dovedT.DefaultIfEmpty()
                                select
                                    new
                                    {
                                        line.line,
                                        line.hp,
                                        obosnovano = line.Value,
                                        plan = (plan0 == null) ? 0 : plan0.Value,
                                        doved = (doved0 == null) ? 0 : doved0.Value
                                    };

                    var gsravn = sravn.GroupBy(g =>
                                               new
                                               {
                                                   g.line.FinanceSource,
                                                   g.line.KCSR,
                                                   g.line.KFO,
                                                   g.line.KOSGU,
                                                   g.line.KVR,
                                                   g.line.KVSR,
                                                   g.line.DEK,
                                                   g.line.DFK,
                                                   g.line.DKR,
                                                   g.line.RZPR,
                                                   g.line.BranchCode,
                                                   g.line.CodeSubsidy,
                                                   g.line.ExpenseObligationType,
                                                   g.hp
                                               }).
                                       Select(s =>
                                              new
                                              {
                                                  line = s.Key,
                                                  obosnovano = s.Sum(ss => ss.obosnovano),
                                                  plan = s.Sum(ss => ss.plan),
                                                  doved = s.Sum(ss => ss.doved)
                                              }).
                                       Select(s =>
                                              new
                                              {
                                                  s.line,
                                                  s.obosnovano,
                                                  s.plan,
                                                  s.doved,
                                                  razn = s.plan - s.obosnovano - s.doved
                                              }).
                                       Where(r => !(r.razn >= 0));

                    foreach (var line in gsravn)
                    {
                        var str =
                            (fExpenseObligationTypeb ? " Тип РО: " + (line.line.ExpenseObligationType == null ? "" : ((ExpenseObligationType)line.line.ExpenseObligationType).Caption()) : "") +
                            (fFinanceSourceb ? ", ИФ: " + (line.line.FinanceSource == null ? "" : line.line.FinanceSource.Code) : "") +
                            (fKFOb ? ", КФО: " + (line.line.KFO == null ? "" : line.line.KFO.Code) : "") +
                            (fKVSRb ? ", КВСР: " + (line.line.KVSR == null ? "" : line.line.KVSR.Caption) : "") +
                            (fRZPRb ? ", РЗПР: " + (line.line.RZPR == null ? "" : line.line.RZPR.Code) : "") +
                            (fKCSRb ? ", КЦСР: " + (line.line.KCSR == null ? "" : line.line.KCSR.Code) : "") +
                            (fKVRb ? ", КВР: " + (line.line.KVR == null ? "" : line.line.KVR.Code) : "") +
                            (fKOSGUb ? ", КОСГУ: " + (line.line.KOSGU == null ? "" : line.line.KOSGU.Code) : "") +
                            (fDKRb ? ", ДКР: " + (line.line.DKR == null ? "" : line.line.DKR.Code) : "") +
                            (fDEKb ? ", ДЭК: " + (line.line.DEK == null ? "" : line.line.DEK.Code) : "") +
                            (fDFKb ? ", ДФК: " + (line.line.DFK == null ? "" : line.line.DFK.Code) : "") +
                            (fCodeSubsidyb ? ", Код субсидии: " + (line.line.CodeSubsidy == null ? "" : line.line.CodeSubsidy.Code) : "") +
                            (fBranchCodeb ? ", Отраслевой код: " + (line.line.BranchCode == null ? "" : line.line.BranchCode.Code) : "");

                        strerrBySbp.AppendFormat(
                            "{0}, {1}, - Объем средств = {2}, Обоснованные ассигнования = {3}, Распределено = {4}, Разность ={5}<br>",
                            line.line.hp.DateStart.Year.ToString(),
                            str,
                            line.plan.ToString(),
                            line.obosnovano.ToString(),
                            line.doved.ToString(),
                            line.razn.ToString());
                    }

                    #endregion
                }
                else
                {
                    var blankBringingRBS = sbpblanks.FirstOrDefault(r => r.IdBlankType == (int)DbEnums.BlankType.BringingRBS && r.IdOwner == asbp.Id);

                    #region раздел b) из постановки

                    var fFinanceSource = (blankFormationGRBS.IdBlankValueType_FinanceSource == mandatory && blankBringingRBS.IdBlankValueType_FinanceSource == mandatory);
                    var fKCSR = (blankFormationGRBS.IdBlankValueType_KCSR == mandatory && blankBringingRBS.IdBlankValueType_KCSR == mandatory);
                    var fKFO = (blankFormationGRBS.IdBlankValueType_KFO == mandatory && blankBringingRBS.IdBlankValueType_KFO == mandatory);
                    var fKOSGU = (blankFormationGRBS.IdBlankValueType_KOSGU == mandatory && blankBringingRBS.IdBlankValueType_KOSGU == mandatory);
                    var fKVR = (blankFormationGRBS.IdBlankValueType_KVR == mandatory && blankBringingRBS.IdBlankValueType_KVR == mandatory);
                    var fKVSR = (blankFormationGRBS.IdBlankValueType_KVSR == mandatory && blankBringingRBS.IdBlankValueType_KVSR == mandatory);
                    var fDEK = (blankFormationGRBS.IdBlankValueType_DEK == mandatory && blankBringingRBS.IdBlankValueType_DEK == mandatory);
                    var fDFK = (blankFormationGRBS.IdBlankValueType_DFK == mandatory && blankBringingRBS.IdBlankValueType_DFK == mandatory);
                    var fDKR = (blankFormationGRBS.IdBlankValueType_DKR == mandatory && blankBringingRBS.IdBlankValueType_DKR == mandatory);
                    var fRZPR = (blankFormationGRBS.IdBlankValueType_RZPR == mandatory && blankBringingRBS.IdBlankValueType_RZPR == mandatory);
                    var fBranchCode = (blankFormationGRBS.IdBlankValueType_BranchCode == mandatory && blankBringingRBS.IdBlankValueType_BranchCode == mandatory);
                    var fCodeSubsidy = (blankFormationGRBS.IdBlankValueType_CodeSubsidy == mandatory && blankBringingRBS.IdBlankValueType_CodeSubsidy == mandatory);
                    var fExpenseObligationType = (blankFormationGRBS.IdBlankValueType_ExpenseObligationType == mandatory && blankBringingRBS.IdBlankValueType_ExpenseObligationType == mandatory);

                    // 1.1 - обосновано ГРБС
                    var rm = tpResourceMaintenanceV.Where(r => r.sbp == asbp).
                                                    Select(s => new
                                                    {
                                                        FinanceSource = fFinanceSource ? s.rm.FinanceSource : null,
                                                        KCSR = fKCSR ? s.rm.KCSR : null,
                                                        KFO = fKFO ? s.rm.KFO : null,
                                                        KOSGU = fKOSGU ? s.rm.KOSGU : null,
                                                        KVR = fKVR ? s.rm.KVR : null,
                                                        KVSR = fKVSR ? s.rm.KVSR : null,
                                                        DEK = fDEK ? s.rm.DEK : null,
                                                        DFK = fDFK ? s.rm.DFK : null,
                                                        DKR = fDKR ? s.rm.DKR : null,
                                                        RZPR = fRZPR ? s.rm.RZPR : null,
                                                        BranchCode = fBranchCode ? s.rm.BranchCode : null,
                                                        CodeSubsidy = fCodeSubsidy ? s.rm.CodeSubsidy : null,
                                                        ExpenseObligationType = fExpenseObligationType ? s.rm.ExpenseObligationType : null,
                                                        hp = s.rmv.HierarchyPeriod,
                                                        Value = s.rmv.Value ?? 0
                                                    }).
                                                    GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp }).
                                                    Select(s =>
                                                           new CLineRM()
                                                           {
                                                               line = new EstimatedLine()
                                                               {
                                                                   FinanceSource = s.Key.FinanceSource,
                                                                   KCSR = s.Key.KCSR,
                                                                   KFO = s.Key.KFO,
                                                                   KOSGU = s.Key.KOSGU,
                                                                   KVR = s.Key.KVR,
                                                                   KVSR = s.Key.KVSR,
                                                                   DEK = s.Key.DEK,
                                                                   DFK = s.Key.DFK,
                                                                   DKR = s.Key.DKR,
                                                                   RZPR = s.Key.RZPR,
                                                                   BranchCode = s.Key.BranchCode,
                                                                   CodeSubsidy = s.Key.CodeSubsidy,
                                                                   ExpenseObligationType = s.Key.ExpenseObligationType,
                                                                   SBP = asbp,
                                                                   PublicLegalFormation = this.PublicLegalFormation
                                                               },
                                                               hp = s.Key.hp,
                                                               Value = s.Sum(ss => ss.Value)
                                                           });


                    // 2.2 - План ГРБС
                    var lvaPlan = lva.
                        Where(r => r.estl.SBP.Id == asbp.Id && r.reg.IdValueType == (int)DbEnums.ValueType.Plan).//СБП = ТЧ.Мероприятия.Исполнитель и Тип значения = План
                        Select(s => new
                        {
                            FinanceSource = fFinanceSource ? s.estl.FinanceSource : null,
                            KCSR = fKCSR ? s.estl.KCSR : null,
                            KFO = fKFO ? s.estl.KFO : null,
                            KOSGU = fKOSGU ? s.estl.KOSGU : null,
                            KVR = fKVR ? s.estl.KVR : null,
                            KVSR = fKVSR ? s.estl.KVSR : null,
                            DEK = fDEK ? s.estl.DEK : null,
                            DFK = fDFK ? s.estl.DFK : null,
                            DKR = fDKR ? s.estl.DKR : null,
                            RZPR = fRZPR ? s.estl.RZPR : null,
                            BranchCode = fBranchCode ? s.estl.BranchCode : null,
                            CodeSubsidy = fCodeSubsidy ? s.estl.CodeSubsidy : null,
                            ExpenseObligationType = fExpenseObligationType ? s.estl.ExpenseObligationType : null,
                            hp = s.reg.HierarchyPeriod,
                            Value = s.reg.Value,
                            sbp = s.estl.SBP
                        }).
                        GroupBy(g => new { g.FinanceSource, g.KCSR, g.KFO, g.KOSGU, g.KVR, g.KVSR, g.DEK, g.DFK, g.DKR, g.RZPR, g.BranchCode, g.CodeSubsidy, g.ExpenseObligationType, g.hp, g.sbp }).
                        Select(s =>
                                new CLineRM()
                                {
                                    line = new EstimatedLine()
                                    {
                                        FinanceSource = s.Key.FinanceSource,
                                        KCSR = s.Key.KCSR,
                                        KFO = s.Key.KFO,
                                        KOSGU = s.Key.KOSGU,
                                        KVR = s.Key.KVR,
                                        KVSR = s.Key.KVSR,
                                        DEK = s.Key.DEK,
                                        DFK = s.Key.DFK,
                                        DKR = s.Key.DKR,
                                        RZPR = s.Key.RZPR,
                                        BranchCode = s.Key.BranchCode,
                                        CodeSubsidy = s.Key.CodeSubsidy,
                                        ExpenseObligationType = s.Key.ExpenseObligationType,
                                        SBP = asbp,
                                        PublicLegalFormation = this.PublicLegalFormation
                                    },
                                    hp = s.Key.hp,
                                    Value = s.Sum(ss => ss.Value),
                                    sbp = s.Key.sbp
                                });

                    // 3. Для каждой сгруппированной строки с «Обосновано ГРБС»  попытаться  найти соответствующую ей сгруппированную строку с «План ГРБС»  
                    var sravn = from line in rm
                                join plan in lvaPlan on
                                    new
                                    {
                                        FinanceSource = line.line.FinanceSource,
                                        KCSR = line.line.KCSR,
                                        KFO = line.line.KFO,
                                        KOSGU = line.line.KOSGU,
                                        KVR = line.line.KVR,
                                        KVSR = line.line.KVSR,
                                        DEK = line.line.DEK,
                                        DFK = line.line.DFK,
                                        DKR = line.line.DKR,
                                        RZPR = line.line.RZPR,
                                        BranchCode = line.line.BranchCode,
                                        CodeSubsidy = line.line.CodeSubsidy,
                                        ExpenseObligationType = line.line.ExpenseObligationType,
                                        hp = line.hp
                                    }
                                    equals
                                    new
                                    {
                                        FinanceSource = plan.line.FinanceSource,
                                        KCSR = plan.line.KCSR,
                                        KFO = plan.line.KFO,
                                        KOSGU = plan.line.KOSGU,
                                        KVR = plan.line.KVR,
                                        KVSR = plan.line.KVSR,
                                        DEK = plan.line.DEK,
                                        DFK = plan.line.DFK,
                                        DKR = plan.line.DKR,
                                        RZPR = plan.line.RZPR,
                                        BranchCode = plan.line.BranchCode,
                                        CodeSubsidy = plan.line.CodeSubsidy,
                                        ExpenseObligationType = plan.line.ExpenseObligationType,
                                        hp = plan.hp
                                    }
                                    into planT
                                from plan0 in planT.DefaultIfEmpty()
                                select
                                    new
                                    {
                                        line.line,
                                        line.hp,
                                        obosnovano = line.Value,
                                        plan = (plan0 == null) ? 0 : plan0.Value,
                                    };

                    var gsravn = sravn.GroupBy(g =>
                                               new
                                               {
                                                   g.line.FinanceSource,
                                                   g.line.KCSR,
                                                   g.line.KFO,
                                                   g.line.KOSGU,
                                                   g.line.KVR,
                                                   g.line.KVSR,
                                                   g.line.DEK,
                                                   g.line.DFK,
                                                   g.line.DKR,
                                                   g.line.RZPR,
                                                   g.line.BranchCode,
                                                   g.line.CodeSubsidy,
                                                   g.line.ExpenseObligationType,
                                                   g.hp
                                               }).
                                       Select(s =>
                                              new
                                              {
                                                  line = s.Key,
                                                  obosnovano = s.Sum(ss => ss.obosnovano),
                                                  plan = s.Sum(ss => ss.plan)
                                              }).
                                       Select(s =>
                                              new
                                              {
                                                  s.line,
                                                  s.obosnovano,
                                                  s.plan,
                                                  razn = s.plan - s.obosnovano
                                              }).
                                       Where(r => !(r.razn >= 0));

                    foreach (var line in gsravn)
                    {
                        var str =
                             (fExpenseObligationType ? " Тип РО: " + (line.line.ExpenseObligationType == null ? "" : ((ExpenseObligationType)line.line.ExpenseObligationType).Caption()) : "") +
                             (fFinanceSource ? ", ИФ: " + (line.line.FinanceSource == null ? "" : line.line.FinanceSource.Code) : "") +
                             (fKFO ? ", КФО: " + (line.line.KFO == null ? "" : line.line.KFO.Code) : "") +
                             (fKVSR ? ", КВСР: " + (line.line.KVSR == null ? "" : line.line.KVSR.Caption) : "") +
                             (fRZPR ? ", РЗПР: " + (line.line.RZPR == null ? "" : line.line.RZPR.Code) : "") +
                             (fKCSR ? ", КЦСР: " + (line.line.KCSR == null ? "" : line.line.KCSR.Code) : "") +
                             (fKVR ? ", КВР: " + (line.line.KVR == null ? "" : line.line.KVR.Code) : "") +
                             (fKOSGU ? ", КОСГУ: " + (line.line.KOSGU == null ? "" : line.line.KOSGU.Code) : "") +
                             (fDKR ? ", ДКР: " + (line.line.DKR == null ? "" : line.line.DKR.Code) : "") +
                             (fDEK ? ", ДЭК: " + (line.line.DEK == null ? "" : line.line.DEK.Code) : "") +
                             (fDFK ? ", ДФК: " + (line.line.DFK == null ? "" : line.line.DFK.Code) : "") +
                             (fCodeSubsidy ? ", Код субсидии: " + (line.line.CodeSubsidy == null ? "" : line.line.CodeSubsidy.Code) : "") +
                             (fBranchCode ? ", Отраслевой код: " + (line.line.BranchCode == null ? "" : line.line.BranchCode.Code) : "");

                        strerrBySbp.AppendFormat(
                            "{0}, {1}, - Объем средств = {2}, Обоснованные ассигнования = {3}, Разность ={4}<br>",
                            line.line.hp.DateStart.Year.ToString(),
                            str,
                            line.plan.ToString(),
                            line.obosnovano.ToString(),
                            line.razn.ToString());
                    }

                    #endregion
                }

                if (strerrBySbp.ToString() != string.Empty)
                {
                    strerr.AppendFormat("Ведомство: {0}<br> Превышение обнаружено по строкам:<br>{1}",
                        asbp.Caption,
                        strerrBySbp.ToString()
                        );
                }
            }
            if (strerr.ToString() != string.Empty)
            {
                Controls.Throw(string.Format("Объем обоснованных ассигнований превышает предельный объем бюджетных ассигнований по следующим ведомствам:<br>{0}",
                    strerr));
            }


        }

        /// <summary>   
        /// Контроль "Проверка соответствия текущего бланка доведения с актуальным "
        /// </summary>         
        [ControlInitial(InitialUNK = "0351", InitialCaption = "Проверка соответствия текущего бланка доведения с актуальным ", InitialSkippable = true)]
        public void Control_0351(DataContext context)
        {
            var IdCurBudget = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id;

            var newBlanks =
                context.SBP_Blank.Where(r =>
                                               (((this.SBP.SBPType == DbEnums.SBPType.GeneralManager) &&
                                                 r.IdOwner == this.SBP.Id) ||
                                                ((this.SBP.SBPType == DbEnums.SBPType.Manager) &&
                                                 r.IdOwner == this.SBP.IdParent)) &&
                                               r.IdBlankType == (byte) DbEnums.BlankType.FormationGRBS &&
                                               r.IdBudget == IdCurBudget);

            var oldBlanks = context.ActivityOfSBP_SBPBlankActual.Where(r => r.IdOwner == this.Id && r.SBP_BlankHistory.IdBudget == IdCurBudget);

            if (!newBlanks.Any() || !oldBlanks.Any())
            {
                return;
            }

            var newBlank = newBlanks.FirstOrDefault();
            var oldBlank = oldBlanks.FirstOrDefault().SBP_BlankHistory;

            if (!SBP_BlankHelper.IsEqualBlank(newBlank, oldBlank))
                Controls.Throw(string.Format("Был изменен бланк «{0}». " +
                                             "Необходимо актуализировать сведения в таблице «Ресурсное обеспечение мероприятий», " +
                                             "в строках будут очищены КБК, не соответствующие бланку формирования, и выполнится группировка сметных строк.",
                                             newBlank.BlankType.Caption()));
        }
        public class CLineRM
        {
            public EstimatedLine line;
            public HierarchyPeriod hp;
            public SBP sbp;
            public decimal Value;
        }

        /// <summary>   
        /// Контроль "Проверка уникальности сметной строки"
        /// </summary>         
        [ControlInitial(InitialUNK = "0352", InitialCaption = "Проверка уникальности сметной строки", InitialSkippable = false)]
        public void Control_0352(DataContext context)
        {
            var docid = this.Id;
            Control0352(context);
        }

        public void Control0352(DataContext context)
        {
            var duble = context.ActivityOfSBP_ActivityResourceMaintenance.
                                Where(r => r.IdOwner == this.Id).
                                ToList().
                                GroupBy(r =>
                                        new
                                            {
                                                r.Master,
                                                r.BranchCode,
                                                r.CodeSubsidy,
                                                r.DEK,
                                                r.DFK,
                                                r.DKR,
                                                r.ExpenseObligationType,
                                                r.FinanceSource,
                                                r.KCSR,
                                                r.KOSGU,
                                                r.KFO,
                                                r.KVR,
                                                r.KVSR,
                                                r.RZPR,
                                                r.OKATO,
                                                r.AuthorityOfExpenseObligation
                                            }).
                                Select(s =>
                                       new
                                           {
                                               s.Key,
                                               c = s.Count()
                                           }).
                                Where(r => r.c > 1);

            if (duble.Any())
            {
                var sb = new StringBuilder();
                foreach (var master in duble.Select(s => s.Key.Master))
                {
                    sb.AppendFormat("- {0} {1}<br>", master.Activity.Caption,
                                    master.IdContingent.HasValue ? " - " + master.Contingent.Caption : "");

                    sb.AppendFormat("{0}<br>", duble.Where(r => r.Key.Master == master).
                                                     Distinct().
                                                     Select(s =>
                                                            "Тип РО " + (s.Key.ExpenseObligationType).Caption() +
                                                            ", ИФ " + s.Key.FinanceSource.Code +
                                                            (s.Key.KFO == null ? "" : ", КФО " + s.Key.KFO.Code) +
                                                            (s.Key.KVSR == null ? "" : ", КВСР " + s.Key.KVSR.Caption) +
                                                            (s.Key.RZPR == null ? "" : ", РзПР " + s.Key.RZPR.Code) +
                                                            (s.Key.KCSR == null ? "" : ", КЦСР " + s.Key.KCSR.Code) +
                                                            (s.Key.KVR == null ? "" : ", КВР " + s.Key.KVR.Code) +
                                                            (s.Key.KOSGU == null ? "" : ", КОСГУ " + s.Key.KOSGU.Code) +
                                                            (s.Key.DFK == null ? "" : ", ДФК " + s.Key.DFK.Code) +
                                                            (s.Key.DKR == null ? "" : ", ДКР " + s.Key.DKR.Code) +
                                                            (s.Key.DEK == null ? "" : ", ДЕК " + s.Key.DEK.Code) +
                                                            (s.Key.CodeSubsidy == null
                                                                 ? ""
                                                                 : ", Код субсидии " + s.Key.CodeSubsidy.Code) +
                                                            (s.Key.BranchCode == null
                                                                 ? ""
                                                                 : ", Отраслевой код " + s.Key.BranchCode.Code) +
                                                            (s.Key.OKATO == null ? "" : ", Территория " + s.Key.OKATO.Caption)).
                                                     Aggregate((a, b) => a + "<br>" + b));
                }
                Controls.Throw(string.Format(
                    "В таблице «Ресурсное обеспечение мероприятий» указаны неуникальные строки:<br>{0}", sb));
            }
        }

        /// <summary>   
        /// Контроль "Проверка согласования документов, входящих в программу"
        /// </summary>         
        [ControlInitial(InitialUNK = "0353", InitialCaption = "Проверка согласования документов, входящих в программу", InitialSkippable = false)]
        public void Control_0353(DataContext context)
        {
            
        }
    }
}
