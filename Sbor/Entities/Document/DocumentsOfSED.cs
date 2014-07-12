using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Threading;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Values;
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
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;
using Sbor.CommonControls;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Tablepart;
using BaseApp.Reference;
using Sbor.Document;
using System.Linq;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Document
{
    public partial class DocumentsOfSED : IColumnFactoryForDenormalizedTablepart, IClarificationDoc, IDocStatusTerminate, IPpoVerDoc
    {
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
                qSg0.Where(w => 
                    w.IdParent.HasValue 
                    && w.Parent.IdRefStatus == (byte)RefStats.Work
                    && w.Parent.IdDocType_CommitDoc != IdDocType
                    && !qSg0.Any(a => a.Id == w.IdParent)
                ).Select(s => new { sg = s.Parent, isOtherDocSG = true }).Distinct()
            ).ToList();

            // получаем записи из тч, с признаком из этого документа
            var qTp = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == Id).ToList();

            // чтобы удалять было не напряжно, иерархию изначально удаляем
            foreach (var item in qTp)
            {
                item.IdParent = null;
            }

            // обновляем где нужно признак из другого документа
            var qUpdateItems = qTp.Join(
                qSg, a => a.IdSystemGoal, b => b.sg.Id, 
                (a, b) => new { Tp = a, isOtherDocSG = b.isOtherDocSG }
            ).Where(w => w.Tp.IsOtherDocSG != w.isOtherDocSG);
            foreach (var Item in qUpdateItems)
            {
                Item.Tp.IsOtherDocSG = Item.isOtherDocSG;
            }

            // создаем новые записи
            var qNewItems = qSg.Where(w => !qTp.Any(a => a.IdSystemGoal == w.sg.Id));
            foreach (var item in qNewItems)
            {
                context.DocumentsOfSED_ItemsSystemGoal.Add(new DocumentsOfSED_ItemsSystemGoal()
                {
                    IdOwner = Id,
                    IdSystemGoal = item.sg.Id,
                    IsOtherDocSG = item.isOtherDocSG
                });
            }

            // удаляем устаревшие
            var qDelItems = qTp.Where(w => !qSg.Any(a => a.sg.Id == w.IdSystemGoal));
            foreach (var item in qDelItems)
            {
                context.DocumentsOfSED_ItemsSystemGoal.Remove(item);
            }

            context.SaveChanges();

            // теперь удаляем лишние данные для СЦ из другого документа
            var qGi1 = context.DocumentsOfSED_GoalIndicator.Where(w => w.IdOwner == Id && w.Master.IsOtherDocSG);
            foreach (var item in qGi1)
            {
                context.DocumentsOfSED_GoalIndicator.Remove(item);
            }

            context.SaveChanges();

            // для записей из нашего документа обновляем вычислемые хранимые поля (в т.ч. восстанавливаем иерархию), показатели, значения показателей
            int[] items = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == Id).Select(s => s.Id).ToArray();
            RefreshData_ItemsSystemGoals(context, items);
            FillData_GoalIndicatorValues(context, items);
        }

        public void RefreshData_ItemsSystemGoals(DataContext context, int[] items, bool flag = false)
        {
            if (flag)
                ExecuteControl(e => e.Control_0020(context, items));

            var list = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == Id).Select(s => new { str = s, sg = s.SystemGoal }).ToList();
            foreach (var item in list.Where(w => items.Contains(w.str.Id)))
            {
                var obj = item.sg;
                item.str.IdElementTypeSystemGoal = obj.IdElementTypeSystemGoal;
                item.str.IdSBP = obj.IdSBP;
                item.str.Code = obj.Code;
                item.str.DateStart = obj.DateStart;
                item.str.DateEnd = obj.DateEnd;
                item.str.IdParent = item.str.IsOtherDocSG ? null : list.Where(s => s.str.IdSystemGoal == item.sg.IdParent).Select(a => (int?)a.str.Id).SingleOrDefault();
            }
            context.SaveChanges();
        }

        public void FillData_GoalIndicatorValues(DataContext context, int[] items)
	    {
            // записи "Элементы СЦ" для которых будем обновлять показатели и значения показателей
            var qTpSg = context.DocumentsOfSED_ItemsSystemGoal.Where(w => items.Contains(w.Id) && !w.IsOtherDocSG);

            // подходящие показатели для обновления
            var qGi = context.SystemGoal_GoalIndicator.Where(w => w.IdVersion == IdVersion)
                             .Join(qTpSg, a => a.IdOwner, b => b.IdSystemGoal, (a, b) => a).ToList();

            // существующие показатели в тч
            var qTpGi = context.DocumentsOfSED_GoalIndicator.Where(w => w.IdOwner == Id)
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
                context.DocumentsOfSED_GoalIndicator.Add(new DocumentsOfSED_GoalIndicator()
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
                context.DocumentsOfSED_GoalIndicator.Remove(i);
            }

            context.SaveChanges();

            // теперь обновляем значения показателей
            int[] itms = context.DocumentsOfSED_GoalIndicator.Where(w => w.IdOwner == Id)
                                .Join(qTpSg, a => a.IdMaster, b => b.Id, (a, b) => a)
                                .Select(s => s.Id).ToArray();
            RefreshData_GoalIndicatorValues(context, itms);
        }

        public void RefreshData_GoalIndicatorValues(DataContext context, int[] items)
        {
            // показатели для которых обновляем значения
            var qTpGi = context.DocumentsOfSED_GoalIndicator.Where(w => items.Contains(w.Id));

            // уже существующие значения показателей
            var qTpGiv = context.DocumentsOfSED_GoalIndicatorValue.Where(w => w.IdOwner == Id && items.Contains(w.IdMaster)).ToList();

            // данные по значениям показателей для обновления
            var qGiv = context.SystemGoal_GoalIndicatorValue.Where(w => w.Master.IdVersion == IdVersion).Join(
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
                context.DocumentsOfSED_GoalIndicatorValue.Add(new DocumentsOfSED_GoalIndicatorValue()
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
            foreach (var v in qDelItems)
            {
                context.DocumentsOfSED_GoalIndicatorValue.Remove(v);
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
        public DocumentsOfSED GetPrevVersionDoc(DataContext context, DocumentsOfSED curdoc)
        {
            if (curdoc.IdParent.HasValue)
            {
                return
                    context.DocumentsOfSED.Where(w => w.Id == curdoc.IdParent).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Контроли

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1500)]
        public void ControlPeriod(DataContext context, ControlType ctType)
        {
            int minYear = context.HierarchyPeriod.Min(c => c.DateStart.Year);
            int maxYear = context.HierarchyPeriod.Max(c => c.DateStart.Year);

            if (DateStart.Year < minYear || DateEnd.Year > maxYear)
                Controls.Throw(string.Format("Срок реализации документа выходит за пределы справочника 'Иерархия периодов' {0}-{1} гг", minYear, maxYear));
        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1000)]
        public void AutoSet(DataContext context, ControlType ctType)
        {
            if (ctType == ControlType.Insert)
            {
                var sc =
                    context.DocumentsOfSED.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                            .Select(s => s.Number).Distinct().ToList();
                Number = CommonMethods.GetNextCode(sc);
            }

            Caption = this.ToString(); // string.Format("Документ СЭР №{0} от {1}", Number, Date.ToString("dd.MM.yyyy"));
        }

        /// <summary>   
        /// Контроль "Проверка даты документа"
        /// </summary>         
        public void Control_0014(DataContext context)
        {
            if (Parent != null && Parent.Date > Date)
                Controls.Throw(string.Format(
                    "Дата документа не может быть меньше даты предыдущей редакции.<br>Дата текущего документа: {0}<br>Дата предыдущей редакции: {1}",
                    Date.ToString("dd.MM.yyyy"),
                    Parent.Date.ToString("dd.MM.yyyy")
                ));
        }

        /// <summary>   
        /// Контроль "Проверка срока реализации документа"
        /// </summary>         
        public void Control_0001(DataContext context)
        {
            if (DateStart >= DateEnd)
                Controls.Throw("Некорректный срок реализации. «Срок реализации с» должен быть меньше «Срока реализации по».");
        }

        /// <summary>   
        /// Контроль "Проверка наличия документа-дубля"
        /// </summary>         
        public void Control_0002(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            int[] statuses = new int[] { DocStatus.Project, DocStatus.Changed, DocStatus.Approved, DocStatus.Denied };

            DocumentsOfSED obj = context.DocumentsOfSED.FirstOrDefault(a =>
                a.IdPublicLegalFormation == IdPublicLegalFormation
                && a.IdVersion == IdVersion
                && a.IdDocType == IdDocType
                && statuses.Contains(a.IdDocStatus)
                && (
                    (a.DateStart >= DateStart && a.DateStart <= DateEnd)
                    || (DateStart >= a.DateStart && DateStart <= a.DateEnd)
                )
                && !ids.Contains(a.Id)
            );
            if (obj != null)
                Controls.Throw(string.Format(
                    "В системе уже имеется документ «{0}» с версией «{1}» и сроком реализации {2} - {3} гг.<br>{4}<br>"+
                    "Запрещается создавать однотипные документы с пересекающимися сроками реализации.",
                    obj.DocType.Caption,
                    obj.Version.Caption,
                    obj.DateStart.ToString("MM.yyyy"),
                    obj.DateEnd.ToString("MM.yyyy"),
                    obj.Caption
                ));
        }

        /// <summary>   
        /// Контроль "Проверка наличия элементов СЦ в документе"
        /// </summary>         
        public void Control_0003(DataContext context)
        {
            if (!context.DocumentsOfSED_ItemsSystemGoal.Any(a => a.IdOwner == Id && !a.IsOtherDocSG))
                Controls.Throw("Не указан ни один элемент СЦ, реализующийся в рамках текущего документа.");
        }

        /// <summary>   
        /// Контроль "Проверка вхождения сроков элементов СЦ в срок реализации документа"
        /// </summary>         
        public void Control_0004(DataContext context)
        {
            List<string> list = context.DocumentsOfSED_ItemsSystemGoal.Where(a => 
                a.IdOwner == Id 
                && !a.IsOtherDocSG
                && (a.DateStart < DateStart || a.DateEnd > DateEnd)
            ).Select(s => "- " + s.SystemGoal.Caption).ToList();
            if (list.Any())
                Controls.Check(list, string.Format(
                    "Сроки реализации следующих элементов СЦ выходят за пределы срока реализации документа {0} - {1} гг:<br>{{0}}",
                    DateStart.ToString("MM.yyyy"),
                    DateEnd.ToString("MM.yyyy")
                ));
        }

        /// <summary>   
        /// Контроль "Проверка соответствия типа элемента СЦ и документа"
        /// </summary>         
        public void Control_0005(DataContext context)
        {
            List<string> list = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                w.IdOwner == Id
                && !w.IsOtherDocSG
                && !context.ElementTypeSystemGoal_Document.Any(a =>
                    a.IdOwner == w.IdElementTypeSystemGoal
                    && a.Owner.IdRefStatus == (byte) RefStats.Work
                    && a.IdDocType == IdDocType
                )
            ).Select(s => s.ElementTypeSystemGoal.Caption + " «"+ s.SystemGoal.Caption +"»").ToList();
            if (list.Any())
                Controls.Check(list, string.Format(
                    "Следующие элементы СЦ не могут реализовываться в рамках документа «{0}», так как такая связь не соответствует настройкам в справочнике «Типы элементов СЦ»:<br>{{0}}",
                    DocType.Caption
                ));
        }

	    /// <summary>   
        /// Контроль "Проверка соответствия модели СЦ в рамках документа"
	    /// </summary>         
	    public void Control_0006(DataContext context)
	    {
            List<string> list = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                w.IdOwner == Id
                && !w.IsOtherDocSG
                && !context.ModelSystemGoal.Any(a =>
                    a.IdElementTypeSystemGoal == w.IdElementTypeSystemGoal
                    && (a.Parent == null ? 0 : a.Parent.IdElementTypeSystemGoal) == (w.Parent == null ? 0 : w.Parent.IdElementTypeSystemGoal)
                    && a.IdRefStatus == (byte)RefStats.Work
                )
            ).Select(s => (s.Parent == null ? "пусто" : s.Parent.ElementTypeSystemGoal.Caption) + " - " +  s.ElementTypeSystemGoal.Caption).ToList();
            if (list.Any())
                Controls.Check(list, "В документе присутствуют связи, которые не соответствуют настроенной Модели СЦ:<br>{0}");
        }

        /// <summary>   
        /// Контроль "Проверка соответствия модели СЦ между документами"
        /// </summary>         
        public void Control_0017(DataContext context)
        {
            
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == Id && !w.IsOtherDocSG);

            var qReg1 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );

            var qReg2 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue
                && (w.IdRegistratorEntity != EntityId || !ids.Contains(w.IdRegistrator))
                && w.IdSystemGoalElement_Parent.HasValue
            );

            var qLink = qTp.Join(qReg1, 
                a => a.IdSystemGoal, b => b.SystemGoalElement.IdSystemGoal, 
                (a, b) => new { Tp = a, Reg1 = b }
            ).Join(qReg2, 
                a => a.Reg1.SystemGoalElement.IdSystemGoal, b => b.SystemGoalElement_Parent.IdSystemGoal, 
                (a, b) => new { Tp = a.Tp, Reg1 = a.Reg1, Reg2 = b }
            );

            List<string> list = qLink.Where(w =>
                !context.ModelSystemGoal.Any(a =>
                    a.IdElementTypeSystemGoal == w.Reg2.IdElementTypeSystemGoal
                    && (a.Parent == null ? 0 : a.Parent.IdElementTypeSystemGoal) == w.Tp.IdElementTypeSystemGoal
                    && a.IdRefStatus == (byte)RefStats.Work
                )
            ).Select(s => 
                s.Tp.ElementTypeSystemGoal.Caption + " «" + s.Tp.SystemGoal.Caption + "»"
                + " - " + s.Reg2.ElementTypeSystemGoal.Caption + " «" + s.Reg2.SystemGoalElement.SystemGoal.Caption + "»" 
            ).OrderBy(o => o).ToList();

            if (list.Any())
                Controls.Check(list, 
                    "У элементов СЦ из текущего документа обнаружены нижестоящие элементы, с которыми нарушается соответствие настроенной Модели СЦ:<br>{0}<br><br>"+
                    "У документов с указанными нижестоящими элементами СЦ будет установлен признак «Требует уточнения»."
                );
        }

        /// <summary>   
        /// Контроль "Правильность дерева элементов СЦ в программе СЭР"
        /// </summary>         
        public void Control_0007(DataContext context)
        {
            if (IdDocType == DocType.ProgramSED) // Программа СЭР
            {
                List<string> list = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                    w.IdOwner == Id && !w.IsOtherDocSG && !w.IdParent.HasValue
                ).Select(s => s.SystemGoal.Caption).ToList();
                if (list.Any())
                    Controls.Check(list, "Для следующих элементов СЦ требуется указать вышестоящий элемент СЦ из программы СЭР или из другого документа СЦ:<br>{0}");
            }
        }

        /// <summary>   
        /// Контроль "Соответствие сроков реализации нижестоящего элемента с вышестоящим"
        /// </summary>         
        public void Control_0008(DataContext context)
        {
            List<string> list = context.DocumentsOfSED_ItemsSystemGoal.Where(a =>
                a.IdOwner == Id && !a.IsOtherDocSG
                && a.IdParent.HasValue
                && (a.DateStart < a.Parent.DateStart || a.DateEnd > a.Parent.DateEnd)
            ).Select(s => new 
            {
                s.SystemGoal.Caption, 
                DateStart = s.DateStart.Value, 
                DateEnd = s.DateEnd.Value
            }).ToList().Select(s => 
                "- " + s.Caption + " " + s.DateStart.ToString("MM.yyyy") + " - " + s.DateEnd.ToString("MM.yyyy")
            ).ToList();

            if (list.Any())
                Controls.Check(list, "Указаны неверные сроки. Сроки реализации следующих элементов СЦ выходят за пределы сроков их вышестоящих элементов:<br>{0}");
        }

        /// <summary>   
        /// Контроль "Проверка соответствия сроков реализации с нижестоящими элементами СЦ из других документов"
        /// </summary>         
        public void Control_0016(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == Id && !w.IsOtherDocSG);

            var qReg1 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );

            var qReg2 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue
                && (w.IdRegistratorEntity != EntityId || !ids.Contains(w.IdRegistrator))
                && w.IdSystemGoalElement_Parent.HasValue
            );

            var qLink = qTp.Join(qReg1,
                a => a.IdSystemGoal, b => b.SystemGoalElement.IdSystemGoal,
                (a, b) => new { Tp = a, Reg1 = b }
            ).Join(qReg2,
                a => a.Reg1.SystemGoalElement.IdSystemGoal, b => b.SystemGoalElement_Parent.IdSystemGoal,
                (a, b) => new { Tp = a.Tp, Reg1 = a.Reg1, Reg2 = b }
            );

            List<string> list = qLink.Where(w =>
                w.Reg2.DateStart < w.Reg1.DateStart || w.Reg2.DateEnd > w.Reg1.DateEnd
            ).Select(s => new 
            {
                TpCaption   = s.Tp.SystemGoal.Caption,
                TpDateStart = s.Tp.DateStart.Value,
                TpDateEnd   = s.Tp.DateEnd.Value,
                Reg2Caption   = s.Reg2.SystemGoalElement.SystemGoal.Caption,
                Reg2DateStart = s.Reg2.DateStart,
                Reg2DateEnd   = s.Reg2.DateEnd
            }).ToList().Select(s =>
                s.TpCaption + " " + s.TpDateStart.ToString("MM.yyyy") + " - " + s.TpDateEnd.ToString("MM.yyyy") + " гг"
                + " - " + s.Reg2Caption + " " + s.Reg2DateStart.ToString("MM.yyyy") + " - " + s.Reg2DateEnd.ToString("MM.yyyy") + " гг"
            ).OrderBy(o => o).ToList();

            if (list.Any())
                Controls.Check(list,
                    "У элементов СЦ из текущего документа обнаружены нижестоящие элементы, с которыми нарушается соответствие сроков реализации:<br>{0}<br><br>" +
                    "У документов с указанными нижестоящими элементами СЦ будет установлен признак «Требует уточнения»."
                );
        }

        /// <summary>   
        /// Контроль "Наличие целевых показателей"
        /// </summary>         
        public void Control_0009(DataContext context)
        {
            List<string> list = context.DocumentsOfSED_ItemsSystemGoal.Where(w => 
                w.IdOwner == Id && !w.IsOtherDocSG
                && !context.DocumentsOfSED_GoalIndicator.Any(a => a.IdMaster == w.Id)
            ).Select(s => " - " + s.SystemGoal.Caption).ToList();

            if (list.Any())
                Controls.Check(list, "У следующих элементов СЦ отсутствуют целевые показатели:<br>{0}");
        }

        /// <summary>   
        /// Контроль "Наличие значений у целевых показателей"
        /// </summary>         
        public void Control_0010(DataContext context)
        {
            var qTp = context.DocumentsOfSED_GoalIndicator.Where(w =>
                w.IdOwner == Id && !w.Master.IsOtherDocSG
                && !context.DocumentsOfSED_GoalIndicatorValue.Any(a => a.IdMaster == w.Id)
            ).Select(s => new
            {
                Id = s.Id,
                IdMaster = s.IdMaster,
                CapSystemGoal = s.Master.SystemGoal.Caption,
                CapGoalIndicator = s.GoalIndicator.Caption
            }).GroupBy(g => new { g.IdMaster, g.CapSystemGoal });

            string text = string.Empty;
            foreach (var s in qTp)
            {
                text += s.Key.CapSystemGoal + "<br>";
                foreach (var i in s)
                {
                    text += " - " + i.CapGoalIndicator + "<br>";
                }
            }

            if (!string.IsNullOrEmpty(text))
                Controls.Throw(string.Format(
                    "У следующих целевых показателей не задано ни одного значения:<br>{0}",
                    text
                ));
        }

        /// <summary>   
        /// Контроль "Наличие значений целевых показателей, выходящих за пределы срока реализации элемента СЦ"
        /// </summary>         
        public void Control_0011(DataContext context)
        {
            var qTp = context.DocumentsOfSED_GoalIndicator.Where(w =>
                w.IdOwner == Id && !w.Master.IsOtherDocSG
                && context.DocumentsOfSED_GoalIndicatorValue.Any(a => a.IdMaster == w.Id && (
                     a.HierarchyPeriod.Year < w.Master.DateStart.Value.Year
                    || a.HierarchyPeriod.DateEnd.Year > w.Master.DateEnd.Value.Year 
                ))
            ).Select(s => new
            {
                Id = s.Id,
                IdMaster = s.IdMaster,
                CapSystemGoal = s.Master.SystemGoal.Caption,
                CapGoalIndicator = s.GoalIndicator.Caption
            }).GroupBy(g => new { g.IdMaster, g.CapSystemGoal });

            string text = string.Empty;
            foreach (var s in qTp)
            {
                text += s.Key.CapSystemGoal + "<br>";
                foreach (var i in s)
                {
                    text += " - " + i.CapGoalIndicator + "<br>";
                }
            }

            if (!string.IsNullOrEmpty(text))
                Controls.Throw(string.Format(
                    "У следующих целевых показателей обнаружены значения, выходящие за срок реализации элемента СЦ:<br>{0}",
                    text
                ));
        }

        /// <summary>   
        /// Контроль "Наличие вышестоящих элементов СЦ в проектных документах"
        /// </summary>         
        public void Control_0012(DataContext context)
        {
            var q = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                w.IdOwner == Id && w.IsOtherDocSG
            ).Select(s => new 
            {
                Str = (s.SBP == null ? "" : s.SBP.Caption + " ")
                   + s.ElementTypeSystemGoal.Caption + " «" + s.SystemGoal.Caption + "»",
                DateStart = s.DateStart.Value,
                DateEnd = s.DateEnd.Value,
                Docs = context.AttributeOfSystemGoalElement.Where(r =>
                    !r.IdTerminator.HasValue && r.IdVersion == IdVersion
                    && !r.SystemGoalElement.IdTerminator.HasValue && r.SystemGoalElement.IdVersion == IdVersion
                    && r.SystemGoalElement.IdSystemGoal == s.IdSystemGoal
                    && r.IdElementTypeSystemGoal == s.IdElementTypeSystemGoal
                    && r.IdSBP == s.IdSBP
                    && r.DateStart == s.DateStart
                    && r.DateEnd == s.DateEnd
                ).Select(k => new { k.RegistratorEntity, k.IdRegistrator }).Distinct()
            }).ToList().Select(s => new
            {
                Str = s.Str + " " + s.DateStart.ToString("MM.yyyy") + " - " + s.DateEnd.ToString("MM.yyyy") + " гг.",
                Docs = s.Docs.ToList()
            });

            List<string> list = q.Where(w => !w.Docs.Any()).Select(s => s.Str).ToList();

            if (list.Any())
                Controls.Check(list, 
                    "Следующие элементы СЦ не найдены ни в одном проектном или утвержденном документе системы целеполагания:<br>{0}<br><br>"+
                    "Возможно, что в текущем документе указаны устаревшие реквизиты этих элементов СЦ. Или требуется добавить эти элементы в документы СЦ."
                );


            string text = string.Empty;

            foreach (var s in q.Where(w => w.Docs.Count() > 1))
            {
                text += s.Str + "<br>";
                foreach (var d in s.Docs)
                {
                    var doc = context.Set<IIdentitied>(d.RegistratorEntity).FirstOrDefault(i => i.Id == d.IdRegistrator);
                    text += " - " + (doc != null ? doc.ToString() : "") + "<br>";
                }
            }

            if (!string.IsNullOrEmpty(text))
                Controls.Throw(string.Format(
                    "Следующие элементы СЦ добавлены в несколько документов системы целеполагания. Невозможно определить, с каким элементом СЦ требуется установить связь:<br>{0}",
                    text
                ));
        }

        /// <summary>   
        /// Контроль "Наличие элементов-дублей в других документах СЦ"
        /// </summary>         
        public void Control_0013(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var q = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                w.IdOwner == Id && !w.IsOtherDocSG
            ).Select(s => new
            {
                Str = (s.SBP == null ? "" : s.SBP.Caption + " ")
                   + s.ElementTypeSystemGoal.Caption + " «" + s.SystemGoal.Caption + "»",
                DateStart = s.DateStart.Value,
                DateEnd = s.DateEnd.Value,
                Docs = context.SystemGoalElement.Where(r =>
                    !r.IdTerminator.HasValue && r.IdVersion == IdVersion
                    && r.IdSystemGoal == s.IdSystemGoal
                    && (r.IdRegistratorEntity != EntityId || !ids.Contains(r.IdRegistrator))
                ).Select(k => new { k.RegistratorEntity, k.IdRegistrator }).Distinct()
            }).ToList().Select(s => new
            {
                Str = s.Str + " " + s.DateStart.ToString("MM.yyyy") + " - " + s.DateEnd.ToString("MM.yyyy") + " гг.",
                Docs = s.Docs.ToList()
            });

            string text = string.Empty;

            foreach (var s in q.Where(w => w.Docs.Any()))
            {
                text += s.Str + "<br>";
                foreach (var d in s.Docs)
                {
                    var doc = context.Set<IIdentitied>(d.RegistratorEntity).FirstOrDefault(i => i.Id == d.IdRegistrator);
                    text += " - " + (doc != null ? doc.ToString() : "") + "<br>";
                }
            }

            if (!string.IsNullOrEmpty(text))
                Controls.Throw(string.Format(
                    "Следующие элементы СЦ уже добавлены в другие документы системы целеполагания:<br>{0}",
                    text
                ));
        }

        /// <summary>   
        /// Контроль "Вхождение срока реализации документа в срок вышестоящего документа"
        /// </summary>         
        public void Control_0019(DataContext context)
        {
            var q = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                w.IdOwner == Id && w.IsOtherDocSG
            ).Select(s => new
            {
                Str = (s.SBP == null ? "" : s.SBP.Caption + " ")
                   + s.ElementTypeSystemGoal.Caption + " «" + s.SystemGoal.Caption + "»",
                DateStart = s.DateStart.Value,
                DateEnd = s.DateEnd.Value,
                Docs = context.AttributeOfSystemGoalElement.Where(r =>
                    !r.IdTerminator.HasValue && r.IdVersion == IdVersion
                    && !r.SystemGoalElement.IdTerminator.HasValue && r.SystemGoalElement.IdVersion == IdVersion
                    && r.SystemGoalElement.IdSystemGoal == s.IdSystemGoal
                    && r.IdElementTypeSystemGoal == s.IdElementTypeSystemGoal
                    && r.IdSBP == s.IdSBP
                    && r.DateStart == s.DateStart
                    && r.DateEnd == s.DateEnd
                ).Select(k => new { k.RegistratorEntity, k.IdRegistrator }).Distinct()
            }).ToList().Select(s => new
            {
                Str = s.Str + " " + s.DateStart.ToString("MM.yyyy") + " - " + s.DateEnd.ToString("MM.yyyy") + " гг.",
                Docs = s.Docs.ToList()
            });

            List<string> list = new List<string>();

            foreach (var s in q.Where(w => w.Docs.Any()))
            {
                foreach (var d in s.Docs)
                {
                    //var doc = context.Set<IIdentitied>(d.RegistratorEntity).FirstOrDefault(i => i.Id == d.IdRegistrator);
                    var doc = DocSGEMethod.GetLeafDoc(context, d.RegistratorEntity.Id, d.IdRegistrator);
                    if (doc != null)
                    {
                        if ((int)doc.GetValue("IdDocStatus") == DocStatus.Draft) doc = (IHierarhy)doc.GetValue("Parent");

                        if (doc != null)
                        {
                            DateTime d1 = (DateTime)doc.GetValue("DateStart");
                            DateTime d2 = (DateTime)doc.GetValue("DateEnd");
                            if (DateStart < d1 || DateEnd > d2)
                            {
                                list.Add(doc.ToString() + " - " + d1.ToString("MM.yyyy") + " - " + d2.ToString("MM.yyyy") + " гг");
                            }
                        }
                    }
                }
            }

            list = list.Distinct().ToList();

            if (list.Any())
                Controls.Check(list, string.Format(
                    "Срок реализации текущего документа выходит за пределы срока реализации вышестоящего документа.<br>" +
                    "Текущий документ: {0}<br>" +
                    "Вышестоящий документ:<br>{{0}}",
                    DateStart.ToString("MM.yyyy") + " - " + DateEnd.ToString("MM.yyyy") + " гг"
                ));
        }

        /// <summary>   
        /// Контроль "Проверка утверждения элементов из другого документа СЦ"
        /// </summary>         
        public void Control_0015(DataContext context)
        {
            List<string> list = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                w.IdOwner == Id && w.IsOtherDocSG
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
                        DateEnd   = (DateTime)a.DateEnd
                    },
                b => new
                    {
                        b.SystemGoalElement.IdSystemGoal, 
                        b.IdElementTypeSystemGoal,  
                        IdSBP = (b.IdSBP ?? 0), 
                        b.DateStart, 
                        b.DateEnd
                    },
                (a,b) => b
            ).Where(w => !w.DateCommit.HasValue || Date < w.DateCommit).ToList().Select(s =>
                " - " + s.SystemGoalElement.SystemGoal.Caption + " " + (s.DateCommit.HasValue ? s.DateCommit.Value.ToString("dd.MM.yyyy") : "не утвержден") 
            ).ToList();

            if (list.Any())
                Controls.Check(list,
                    "Необходимо скорректировать дату текущего документа."+
                    "Следующие элементы СЦ, которые являются вышестоящими для элементов из текущего документа, не утверждены или утверждены более поздней датой:<br>{0}" +
                    string.Format("<br><br>Дата текущего документа: {0}", Date.ToString("dd.MM.yyyy")) 
                );
        }



        /// <summary>   
        /// Контроль "Проверка срока реализации документа"
        /// </summary> 
        [ControlInitial(InitialUNK = "0020", InitialCaption = "Проверка элементов СЦ на соответствие реквизитам документа", InitialSkippable = true)]
        public void Control_0020(DataContext context, int[] items)
        {
            //список индетификаторов элементов СЦ из справочника «Система целеполагания» 
            var tpsgid = context.DocumentsOfSED_ItemsSystemGoal.Where(w => items.Contains(w.Id)
                            && w.IsOtherDocSG == false).Select(d => d.IdSystemGoal);

            switch (tpsgid.Count())
            {
                case 0:
                    break;
                default:
                    //актулальные записи выделенных строк тч СЦ из справочника «Система целеполагания»
                    var actuldoc = context.SystemGoal.Where(w =>
                                                            w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && w.IdDocType_CommitDoc == IdDocType 
                                                            && w.DateStart >= DateStart && w.DateEnd <= DateEnd
                                                            && w.IdRefStatus == (byte)RefStats.Work
                                                            && tpsgid.Contains(w.Id)
                                                            
                        //&& w.IdDocType_CommitDoc == IdDocType


                        ).Select(t => t.Id).ToList();
                    //список не актуальных СЦ из ТЧ документа
                    var res = context.DocumentsOfSED_ItemsSystemGoal.Where(w => items.Contains(w.Id)).Except(context.DocumentsOfSED_ItemsSystemGoal.Where(d => items.Contains(d.Id) && actuldoc.Contains(d.IdSystemGoal)));
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
                                "Следующие элементы СЦ справочника «Система целеполагания» не соответствуют реквизитам документа «Срок реализации», «Тип документа» :{0} <br>" +
                                "Удалить данные элементы из таблицы «Элементы СЦ» документа?", str);
                        Controls.Throw(st);
                        foreach (var item in res)
                        {
                            context.DocumentsOfSED_ItemsSystemGoal.Remove(item);
                        }
                        context.SaveChanges();
                    }
                    break;
            }


        }


	    #endregion

        #region Методы операций

        #region Общие функции для операций

        private void Process_SystemGoalElement(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                w.IdOwner == Id
                && !w.IsOtherDocSG
            ).ToList();

            var qReg = context.SystemGoalElement.Where(w =>
                w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
                && !w.DateTerminate.HasValue
            ).ToList();

            var qNewItems = qTp.Where(w =>
                !qReg.Any(a => a.IdSystemGoal == w.IdSystemGoal)
            );
            foreach (DocumentsOfSED_ItemsSystemGoal item in qNewItems)
            {
                context.SystemGoalElement.Add(new SystemGoalElement()
                {
                    IdRegistrator = Id,
                    IdRegistratorEntity = EntityId,
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdVersion = IdVersion,
                    IdSystemGoal = item.IdSystemGoal,
                    DateCreate = DateTime.Now
                });
            }

            var qTermItems = qReg.Where(w =>
                !qTp.Any(a => a.IdSystemGoal == w.IdSystemGoal)
            );
            foreach (SystemGoalElement item in qTermItems)
            {
                item.IdTerminator = Id;
                item.IdTerminatorEntity = EntityId;
                item.DateTerminate = Date;
            }

            context.SaveChanges();
        }

        private void Process_AttributeOfSystemGoalElement(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.DocumentsOfSED_ItemsSystemGoal.Where(w =>
                w.IdOwner == Id
                && !w.IsOtherDocSG
            );

            var qReg0All = context.SystemGoalElement.Where(w =>
                w.IdPublicLegalFormation == IdPublicLegalFormation
                && w.IdVersion == IdVersion
                && !w.IdTerminator.HasValue
            );

            var qLink0 = qTp.Join(
                qReg0All.Where(w => w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)),
                a => a.IdSystemGoal, b => b.IdSystemGoal,
                (c,d) => new {
                    Tp = c,
                    IdReg0 = d.Id,
                    IdParentReg0 = c.Parent == null ? (int?)null : qReg0All.Where(v => v.IdSystemGoal == c.Parent.IdSystemGoal).Select(k => k.Id).FirstOrDefault()
                }
            ).ToList();

            var qReg1 = context.AttributeOfSystemGoalElement.Where(w =>
                w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
                && !w.IdTerminator.HasValue
            ).ToList();

            var qNewItems = qLink0.Where(w => 
                !qReg1.Any(a => 
                    a.IdSystemGoalElement == w.IdReg0
                    && a.IdSystemGoalElement_Parent == w.IdParentReg0
                    && a.IdElementTypeSystemGoal == w.Tp.IdElementTypeSystemGoal
                    && a.IdSBP == w.Tp.IdSBP
                    && a.DateStart == w.Tp.DateStart
                    && a.DateEnd == w.Tp.DateEnd
                )
            );
            foreach (var item in qNewItems)
            {
                context.AttributeOfSystemGoalElement.Add(new AttributeOfSystemGoalElement()
                {
                    IdRegistrator = Id,
                    IdRegistratorEntity = EntityId,
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdVersion = IdVersion,
                    IdSystemGoalElement = item.IdReg0,
                    IdSystemGoalElement_Parent = item.IdParentReg0,
                    IdElementTypeSystemGoal = item.Tp.IdElementTypeSystemGoal ?? 0,
                    IdSBP = item.Tp.IdSBP,
                    DateStart = item.Tp.DateStart.Value,
                    DateEnd = item.Tp.DateEnd.Value,
                    DateCreate = DateTime.Now
                });
            }

            var qTermItems = qReg1.Where(w =>
                !qLink0.Any(a =>
                    a.IdReg0 == w.IdSystemGoalElement
                    && a.IdParentReg0 == w.IdSystemGoalElement_Parent
                    && a.Tp.IdElementTypeSystemGoal == w.IdElementTypeSystemGoal
                    && a.Tp.IdSBP == w.IdSBP
                    && a.Tp.DateStart == w.DateStart
                    && a.Tp.DateEnd == w.DateEnd
                )
            );
            foreach (AttributeOfSystemGoalElement item in qTermItems)
            {
                item.IdTerminator = Id;
                item.IdTerminatorEntity = EntityId;
                item.DateTerminate = Date;
            }

            context.SaveChanges();
        }

        private void Process_GoalTarget(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qLink = context.DocumentsOfSED_ItemsSystemGoal.Where(a =>
                a.IdOwner == Id
                && !a.IsOtherDocSG
            ).Join(
                context.SystemGoalElement.Where(w =>
                    w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
                    && !w.DateTerminate.HasValue
                ),
                a => a.IdSystemGoal, b => b.IdSystemGoal,
                (a, b) => new { idTp = a.Id, IdReg = b.Id }
            );

            var qTp = context.DocumentsOfSED_GoalIndicator.Where(w =>
                w.IdOwner == Id
                && !w.Master.IsOtherDocSG
            ).Join(
                qLink,
                a => a.IdMaster, b => b.idTp,
                (a, b) => new { Tp = a, IdMasterReg = b.IdReg }
            ).ToList();

            var qReg = context.GoalTarget.Where(w =>
                w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
                && !w.DateTerminate.HasValue
            ).ToList();

            var qNewItems = qTp.Where(w =>
                !qReg.Any(a =>
                    a.IdGoalIndicator == w.Tp.IdGoalIndicator
                    && a.SystemGoalElement.Id == w.IdMasterReg
                )
            );
            foreach (var item in qNewItems)
            {
                context.GoalTarget.Add(new GoalTarget()
                {
                    IdRegistrator = Id,
                    IdRegistratorEntity = EntityId,
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdVersion = IdVersion,
                    IdSystemGoalElement = item.IdMasterReg,
                    IdGoalIndicator = item.Tp.IdGoalIndicator,
                    DateCreate = DateTime.Now
                });
            }

            var qTermItems = qReg.Where(w =>
                !qTp.Any(a =>
                    a.Tp.IdGoalIndicator == w.IdGoalIndicator
                    && a.IdMasterReg == w.IdSystemGoalElement
                )
            );
            foreach (GoalTarget item in qTermItems)
            {
                item.IdTerminator = Id;
                item.IdTerminatorEntity = EntityId;
                item.DateTerminate = Date;
            }

            context.SaveChanges();
        }

        private void Process_ValuesGoalTarget(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.DocumentsOfSED_GoalIndicatorValue.Where(a =>
                a.IdOwner == Id
                && !a.Master.Master.IsOtherDocSG
            ).Join(
                context.GoalTarget.Where(r =>
                    r.IdRegistratorEntity == EntityId && ids.Contains(r.IdRegistrator)
                    && !r.DateTerminate.HasValue
                ),
                a => new { Id1 = a.Master.IdGoalIndicator, Id2 = a.Master.Master.IdSystemGoal },
                b => new { Id1 = b.IdGoalIndicator, Id2 = b.SystemGoalElement.IdSystemGoal },
                (a, b) => new { Tp = a, IdReg = b.Id }
            ).ToList();

            var qReg = context.ValuesGoalTarget.Where(w =>
                w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
                && !w.DateTerminate.HasValue
            ).ToList();

            var qNewItems = qTp.Where(w =>
                !qReg.Any(a =>
                    a.IdGoalTarget == w.IdReg
                    && a.IdHierarchyPeriod == w.Tp.IdHierarchyPeriod
                    && a.IdValueType == (byte)DbEnums.ValueType.Plan
                    && a.Value == w.Tp.Value
                )
            );
            foreach (var item in qNewItems)
            {
                context.ValuesGoalTarget.Add(new ValuesGoalTarget()
                {
                    IdRegistrator = Id,
                    IdRegistratorEntity = EntityId,
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdVersion = IdVersion,
                    IdGoalTarget = item.IdReg,
                    IdHierarchyPeriod = item.Tp.IdHierarchyPeriod,
                    IdValueType = (byte)DbEnums.ValueType.Plan,
                    Value = item.Tp.Value,
                    DateCreate = DateTime.Now
                });
            }

            var qTermItems = qReg.Where(w =>
                w.IdValueType != (byte)DbEnums.ValueType.Plan
                || !qTp.Any(a =>
                    a.IdReg == w.IdGoalTarget
                    && a.Tp.IdHierarchyPeriod == w.IdHierarchyPeriod
                    && a.Tp.Value == w.Value
                )
            );
            foreach (ValuesGoalTarget item in qTermItems)
            {
                item.IdTerminator = Id;
                item.IdTerminatorEntity = EntityId;
                item.DateTerminate = Date;
            }

            context.SaveChanges();
        }

        private void RemoveAllChangesInRegs(DataContext context)
        {
            foreach (ValuesGoalTarget reg in context.ValuesGoalTarget.Where(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id))
            {
                context.ValuesGoalTarget.Remove(reg);
            }

            foreach (GoalTarget reg in context.GoalTarget.Where(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id))
            {
                context.GoalTarget.Remove(reg);
            }

            foreach (AttributeOfSystemGoalElement reg in context.AttributeOfSystemGoalElement.Where(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id))
            {
                context.AttributeOfSystemGoalElement.Remove(reg);
            }

            foreach (SystemGoalElement reg in context.SystemGoalElement.Where(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id))
            {
                context.SystemGoalElement.Remove(reg);
            }


            foreach (ValuesGoalTarget reg in context.ValuesGoalTarget.Where(w => w.IdTerminatorEntity == EntityId && w.IdTerminator == Id))
            {
                reg.IdTerminator = null;
                reg.DateTerminate = null;
            }

            foreach (GoalTarget reg in context.GoalTarget.Where(w => w.IdTerminatorEntity == EntityId && w.IdTerminator == Id))
            {
                reg.IdTerminator = null;
                reg.DateTerminate = null;
            }

            foreach (AttributeOfSystemGoalElement reg in context.AttributeOfSystemGoalElement.Where(w => w.IdTerminatorEntity == EntityId && w.IdTerminator == Id))
            {
                reg.IdTerminator = null;
                reg.DateTerminate = null;
            }

            foreach (SystemGoalElement reg in context.SystemGoalElement.Where(w => w.IdTerminatorEntity == EntityId && w.IdTerminator == Id))
            {
                reg.IdTerminator = null;
                reg.DateTerminate = null;
            }
        }

        private void SetRegsApproved(DataContext context, DateTime d)
        {
            var ids = GetIdAllVersionDoc(context);

            var q1 = context.SystemGoalElement.Where(w => 
                !w.IdTerminator.HasValue && !w.DateCommit.HasValue 
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );
            foreach (SystemGoalElement reg in q1)
            {
                reg.DateCommit = d;
                reg.IdApprovedEntity = EntityId;
                reg.IdApproved = Id;
            }

            var q2 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue && !w.DateCommit.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );
            foreach (AttributeOfSystemGoalElement reg in q2)
            {
                reg.DateCommit = d;
                reg.IdApprovedEntity = EntityId;
                reg.IdApproved = Id;
            }

            var q3 = context.GoalTarget.Where(w =>
                !w.IdTerminator.HasValue && !w.DateCommit.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );
            foreach (GoalTarget reg in q3)
            {
                reg.DateCommit = d;
                reg.IdApprovedEntity = EntityId;
                reg.IdApproved = Id;
            }

            var q4 = context.ValuesGoalTarget.Where(w =>
                !w.IdTerminator.HasValue && !w.DateCommit.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );
            foreach (ValuesGoalTarget reg in q4)
            {
                reg.DateCommit = d;
                reg.IdApprovedEntity = EntityId;
                reg.IdApproved = Id;
            }
        }

        private void ClearRegsApproved(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var q1 = context.SystemGoalElement.Where(w => w.IdApprovedEntity == EntityId && w.IdApproved == Id);
            foreach (SystemGoalElement reg in q1)
            {
                reg.DateCommit = null;
                reg.IdApprovedEntity = null;
                reg.IdApproved = null;
            }

            var q2 = context.AttributeOfSystemGoalElement.Where(w => w.IdApprovedEntity == EntityId && w.IdApproved == Id);
            foreach (AttributeOfSystemGoalElement reg in q2)
            {
                reg.DateCommit = null;
                reg.IdApprovedEntity = null;
                reg.IdApproved = null;
            }

            var q3 = context.GoalTarget.Where(w => w.IdApprovedEntity == EntityId && w.IdApproved == Id);
            foreach (GoalTarget reg in q3)
            {
                reg.DateCommit = null;
                reg.IdApprovedEntity = null;
                reg.IdApproved = null;
            }

            var q4 = context.ValuesGoalTarget.Where(w => w.IdApprovedEntity == EntityId && w.IdApproved == Id);
            foreach (ValuesGoalTarget reg in q4)
            {
                reg.DateCommit = null;
                reg.IdApprovedEntity = null;
                reg.IdApproved = null;
            }
        }

        private void SetRequireClarification(DataContext context, List<RegCommLink> listDoc, string mess)
        {
            foreach (var d in listDoc)
            {
                var docs = context.Set<IClarificationDoc>(d.RegistratorEntity);
                var doc = docs.FirstOrDefault(i => i.Id == d.IdRegistrator);

                if (doc != null)
                {
                    do
                    {
                        var next = docs.FirstOrDefault(i => i.IdParent == doc.Id);
                        if (next != null)
                        {
                            doc = next;
                            continue;
                        }
                        break;
                    } while (true);

                    doc.IsRequireClarification = true;

                    string msg = mess
                        .Replace("{date}", DateTime.Now.ToString("dd.MM.yyyy"))
                        .Replace("{this}", this.Caption);

                    doc.ReasonClarification += (string.IsNullOrEmpty(doc.ReasonClarification) ? "" : "\r\n") + msg;

                    context.Entry(doc).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }

        private void SetRequireClarification0016(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == Id && !w.IsOtherDocSG);

            var qReg1 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );

            var qReg2 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue
                && (w.IdRegistratorEntity != EntityId || !ids.Contains(w.IdRegistrator))
                && w.IdSystemGoalElement_Parent.HasValue
            );

            var qLink = qTp.Join(qReg1,
                a => a.IdSystemGoal, b => b.SystemGoalElement.IdSystemGoal,
                (a, b) => new { Tp = a, Reg1 = b }
            ).Join(qReg2,
                a => a.Reg1.SystemGoalElement.IdSystemGoal, b => b.SystemGoalElement_Parent.IdSystemGoal,
                (a, b) => new { Tp = a.Tp, Reg1 = a.Reg1, Reg2 = b }
            ).Where(w =>
                w.Reg2.DateStart < w.Reg1.DateStart || w.Reg2.DateEnd > w.Reg1.DateEnd
            );

            List<RegCommLink> qRes = qLink.Select(s => new RegCommLink 
            {
                RegistratorEntity = s.Reg2.RegistratorEntity, 
                IdRegistrator = s.Reg2.IdRegistrator
            }).Distinct().ToList();

            SetRequireClarification(context, qRes, 
                "{date}. Сроки реализации элементов из текущего документа не соответствуют срокам реализации их вышестоящих элементов из документа {this}."
            );
        }

        private void SetRequireClarification0017(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == Id && !w.IsOtherDocSG);

            var qReg1 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );

            var qReg2 = context.AttributeOfSystemGoalElement.Where(w =>
                !w.IdTerminator.HasValue
                && (w.IdRegistratorEntity != EntityId || !ids.Contains(w.IdRegistrator))
                && w.IdSystemGoalElement_Parent.HasValue
            );

            var qLink = qTp.Join(qReg1, 
                a => a.IdSystemGoal, b => b.SystemGoalElement.IdSystemGoal, 
                (a, b) => new { Tp = a, Reg1 = b }
            ).Join(qReg2, 
                a => a.Reg1.SystemGoalElement.IdSystemGoal, b => b.SystemGoalElement_Parent.IdSystemGoal, 
                (a, b) => new { Tp = a.Tp, Reg1 = a.Reg1, Reg2 = b }
            );

            List<RegCommLink> qRes = qLink.Where(w =>
                !context.ModelSystemGoal.Any(a =>
                    a.IdElementTypeSystemGoal == w.Reg2.IdElementTypeSystemGoal
                    && (a.Parent == null ? 0 : a.Parent.IdElementTypeSystemGoal) == w.Tp.IdElementTypeSystemGoal
                    && a.IdRefStatus == (byte)RefStats.Work
                )
            ).Select(s => new RegCommLink
            {
                RegistratorEntity = s.Reg2.RegistratorEntity,
                IdRegistrator = s.Reg2.IdRegistrator
            }).Distinct().ToList();

            SetRequireClarification(context, qRes, 
                "{date}. У элементов из текущего документа имеются вышестоящие элементы из документа {this}, с которыми нарушается соответствие настроенной Модели СЦ."
            );
        }

        #endregion


	    /// <summary>   
	    /// Операция «Создать»   
	    /// </summary>  
	    public void Create(DataContext context)
	    {
	        DateLastEdit = DateTime.Now;
            ExecuteControl(e => e.Control_0001(context));
        }

        /// <summary>   
        /// Операция «Редактировать»   
        /// </summary>  
        public void Edit(DataContext context)
        {
            DateLastEdit = DateTime.Now;
            ExecuteControl(e => e.Control_0014(context));
            ExecuteControl(e => e.Control_0001(context));
        }

	    /// <summary>   
        /// Операция «Обработать»   
        /// </summary>  
        public void Process(DataContext context)
        {
            ReasonCancel = null;

            ExecuteControl(e => e.Control_0001(context));
            ExecuteControl(e => e.Control_0002(context));
            ExecuteControl(e => e.Control_0003(context));
            ExecuteControl(e => e.Control_0004(context));
            ExecuteControl(e => e.Control_0005(context));
            ExecuteControl(e => e.Control_0006(context));
            //ExecuteControl(e => e.Control_0017(context));
            ExecuteControl(e => e.Control_0007(context));
            ExecuteControl(e => e.Control_0008(context));
            //ExecuteControl(e => e.Control_0016(context));
            ExecuteControl(e => e.Control_0009(context));
            ExecuteControl(e => e.Control_0010(context));
            ExecuteControl(e => e.Control_0011(context));
            ExecuteControl(e => e.Control_0012(context));
            ExecuteControl(e => e.Control_0013(context));
            ExecuteControl(e => e.Control_0019(context));

            IsRequireClarification = false;
            ReasonClarification = null;

            Process_SystemGoalElement(context);
            Process_AttributeOfSystemGoalElement(context);
            Process_GoalTarget(context);
            Process_ValuesGoalTarget(context);

            ExecuteControl(e => e.Control_0017(context));
            ExecuteControl(e => e.Control_0016(context));

            SetRequireClarification0016(context);
            SetRequireClarification0017(context);

            var prevDoc = GetPrevVersionDoc(context, this);
            if (prevDoc != null)
            {
                prevDoc.ExecuteOperation(e => e.Archive(context));
            }
        }

        /// <summary>
        /// Операция «Отменить обработку»
        /// </summary>  
        public void UndoProcess(DataContext context)
        {
            ExecuteControl<CommonControl_7004>();

            RemoveAllChangesInRegs(context);
            using (new ControlScope())
            {
                context.SaveChanges();
            }

            var prevDoc = GetPrevVersionDoc(context, this);
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
            ExecuteControl(e => e.Control_0012(context));
            ExecuteControl(e => e.Control_0015(context));

            DateCommit = DateTime.Now.Date;

            SetRegsApproved(context, Date);
        }

        /// <summary>   
        /// Операция «Отменить утверждение»   
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            DateCommit = null;

            ClearRegsApproved(context);
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
            Clone cloner = new Clone(this);
            DocumentsOfSED newDoc = (DocumentsOfSED)cloner.GetResult();
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.IdParent = Id;
            newDoc.IsRequireClarification = false;
            newDoc.DateCommit = null;
            newDoc.ReasonTerminate = null;
            newDoc.DateTerminate = null;
            newDoc.DateLastEdit = null;

            newDoc.Date = DateTime.Now.Date;

            var ids = GetIdAllVersionDoc(context);
            var rootNum = context.DocumentsOfSED.Single(w => !w.IdParent.HasValue && ids.Contains(w.Id)).Number;
            newDoc.Number = rootNum + "." + ids.Count().ToString(CultureInfo.InvariantCulture);

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();

            newDoc.Caption = newDoc.ToString();
            context.SaveChanges();
        }

	    /// <summary>   
	    /// Операция «Отменить изменение»   
	    /// </summary>
	    public void UndoChange(DataContext context)
	    {
	        var q = context.DocumentsOfSED.Where(w => w.IdParent == Id);
	        foreach (var doc in q)
	        {
	            context.DocumentsOfSED.Remove(doc);
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

        #endregion

        #region Implementation of IColumnFactoryForDenormalizedTablepart

        public ColumnsInfo GetColumns(string tablepartEntityName)
        {
            if (tablepartEntityName == typeof(DocumentsOfSED_GoalIndicator).Name)
                return GetColumnsFor_DocumentsOfSED_GoalIndicatorValue();

            return null;
        }

        private ColumnsInfo GetColumnsFor_DocumentsOfSED_GoalIndicatorValue()
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

        #endregion
    }
}

