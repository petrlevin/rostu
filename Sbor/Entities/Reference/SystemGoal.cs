using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Logic;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Sbor.Tablepart;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Reference
{
	public partial class SystemGoal : ReferenceEntity, IColumnFactoryForDenormalizedTablepart
	{
        private void InitMaps(DataContext context)
        {
            if (ElementTypeSystemGoal == null) 
                ElementTypeSystemGoal = context.ElementTypeSystemGoal.SingleOrDefault(a => a.Id == IdElementTypeSystemGoal);

            if (SBP == null && IdSBP.HasValue) 
                SBP = context.SBP.SingleOrDefault(a => a.Id == IdSBP);

            if (Parent == null && IdParent.HasValue)
                Parent = context.SystemGoal.SingleOrDefault(a => a.Id == IdParent);

            if (PublicLegalFormation == null)
                PublicLegalFormation = context.PublicLegalFormation.SingleOrDefault(w => w.Id == IdPublicLegalFormation);

            if (DocType_CommitDoc == null)
                DocType_CommitDoc = context.DocType.Single(a => a.Id == IdDocType_CommitDoc);

            if (DocType_ImplementDoc == null && IdDocType_ImplementDoc.HasValue)
                DocType_ImplementDoc = context.DocType.Single(a => a.Id == IdDocType_ImplementDoc);
        }

        [Control(ControlType.Insert|ControlType.Update, Sequence.Before, ExecutionOrder = 0)]
        public void AutoSet(DataContext context, ControlType controlType)
        {
            if (controlType == ControlType.Insert)
            {
                InitMaps(context);

                if (PublicLegalFormation.IdMethodofFormingCode_GoalSetting == (byte)BaseApp.DbEnums.MethodofFormingCode.Auto || string.Equals(Code, "Автоматически"))
                {
                    var sc =
                        context.SystemGoal.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation)
                               .Select(s => s.Code).Distinct().ToList();

                    Code = CommonMethods.GetNextCode(sc);
                }
            }

            if (IdDocType_ImplementDoc.HasValue && IdDocType_CommitDoc == IdDocType_ImplementDoc.Value)
            {
                IdDocType_ImplementDoc = null;
            }

            DateStart = new DateTime(DateStart.Year, DateStart.Month, 1);
            DateEnd = new DateTime(DateEnd.Year, DateEnd.Month, 1).AddMonths(1).AddDays(-1);
        }

        /// <summary>   
        /// Контроль "Проверка соответствия типа элемента СЦ и документа"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 110)]
        public void Control_501101(DataContext context, SystemGoal old)
        {

            if (
                (old != null && IdRefStatus == (decimal) RefStatus.New && old.IdRefStatus == (decimal) RefStatus.New)
                || (IdRefStatus == (decimal) RefStatus.Work)
                )
            {
                var q = context.ElementTypeSystemGoal_Document.Where(w =>
                    w.IdOwner == IdElementTypeSystemGoal
                    && w.Owner.IdRefStatus == (byte)RefStats.Work
                ).ToList();

                int idDocType = IdDocType_CommitDoc;
                bool fail = !q.Any(a => a.IdDocType == idDocType);
                if (!fail && IdDocType_ImplementDoc.HasValue)
                {
                    idDocType = IdDocType_ImplementDoc.Value;
                    fail = !q.Any(a => a.IdDocType == idDocType);
                }

                if (fail)
                {
                    InitMaps(context);

                    Controls.Throw(String.Format(
                        "Для элемента СЦ с типом «{0}» нельзя указывать тип документа «{1}».<br>" +
                        "Такая связь не соответствует настройкам в справочнике «Типы элементов СЦ».",
                        ElementTypeSystemGoal.Caption,
                        context.DocType.Single(s => s.Id == idDocType).Caption
                    ));
                }
            }
        }

        /// <summary>   
        /// Контроль "Проверка заполнения поля «Ответственный исполнитель»"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 120)]
        public void Control_501102(DataContext context)
        {
            bool isDoc_ActivityOfSBP = context.DocType.Any(a => 
                (a.Id == IdDocType_CommitDoc || a.Id == (IdDocType_ImplementDoc ?? 0))
                && a.IdEntity == ActivityOfSBP.EntityIdStatic // документ Деятельность ведомства
            );

            if (isDoc_ActivityOfSBP) 
            {
                if (!IdSBP.HasValue)
                {
                    InitMaps(context);
                    Controls.Throw(string.Format(
                        "Необходимо указать ответственного исполнителя, так как элемент СЦ предназначен для документа «{0}».",
                        DocType_CommitDoc.Caption
                    ));
                }
            }     
        }

        /// <summary>   
        /// Контроль "Проверка уникальности элемента СЦ"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 130)]
        public void Control_501103(DataContext context)
        {
            SystemGoal obj = context.SystemGoal.FirstOrDefault(a =>
                a.IdPublicLegalFormation == IdPublicLegalFormation
                && a.IdElementTypeSystemGoal == IdElementTypeSystemGoal
                && a.Caption == Caption
                && (a.IdSBP ?? 0) == (IdSBP ?? 0)
                && (a.DateStart <= DateEnd && a.DateEnd >= DateStart)
                && a.Id != Id
            );
            if (obj != null)
                Controls.Throw(
                    "В справочнике уже имеется элемент СЦ с такими же реквизитами и пересекающимся сроком реализации:<br>" +
                    obj.ElementTypeSystemGoal.Caption + "<br>" +
                    obj.Caption + "<br>" +
                    (obj.SBP != null ? obj.SBP.Caption + "<br>" : string.Empty) +
                    obj.DateStart.ToString("MM.yyyy") + "-" + obj.DateEnd.ToString("MM.yyyy")
                );
        }

        /// <summary>   
        /// Контроль "Проверка соответствия модели СЦ"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 140)]
        public void Control_501104(DataContext context, SystemGoal oSystemGoal, ControlType ct)
        {
            if (ct == ControlType.Update && oSystemGoal.IdRefStatus == (byte)RefStats.Work && IdRefStatus == (byte)RefStats.New) // с нового на работу
                return;

            if (IdRefStatus == (byte) RefStats.New || IdRefStatus == (byte) RefStats.Work)
            {
                InitMaps(context);

                int idP = (Parent == null ? 0 : Parent.IdElementTypeSystemGoal);
                bool fail = !context.ModelSystemGoal.Any(a =>
                    a.IdRefStatus == (byte)RefStats.Work
                    && (a.Parent == null ? 0 : a.Parent.IdElementTypeSystemGoal) == idP
                    && a.IdElementTypeSystemGoal == IdElementTypeSystemGoal
                );

                if (fail)
                {
                    Controls.Throw(string.Format(
                        "Связь «{0}» - «{1}» не соответствует настроенной Модели СЦ.",
                        Parent == null ? "пусто" : context.ElementTypeSystemGoal.SingleOrDefault(s => s.Id == Parent.IdElementTypeSystemGoal).Caption,
                        ElementTypeSystemGoal.Caption
                    ));
                }
            }
        }

        /// <summary>   
        /// Контроль "Проверка срока реализации элемента СЦ"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 150)]
        public void Control_501110(DataContext context)
        {
            if (DateStart >= DateEnd)
            {
                Controls.Throw("Некорректный срок реализации. «Срок реализации с» должен быть меньше «Срока реализации по».");
            }
        }

        /// <summary>   
        /// Контроль "Соответствие сроков реализации нижестоящего элемента с вышестоящим"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 160)]
        public void Control_501105(DataContext context)
        {
            if (IdParent.HasValue)
            {
                InitMaps(context);
                if (DateStart < Parent.DateStart || DateEnd > Parent.DateEnd)
                    Controls.Throw(string.Format(
                        "Указан неверный срок. Срок реализации выходит за пределы срока вышестоящего элемента СЦ:<br>{0}<br>{1} - {2}",
                        Parent.Caption,
                        Parent.DateStart.ToString("MM.yyyy"),
                        Parent.DateEnd.ToString("MM.yyyy")
                    ));
            }
        }

        /// <summary>   
        /// Контроль "Изменение срока реализации вышестоящего элемента СЦ"
        /// </summary>         
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 170)]
        public void Control_501108(DataContext context)
        {
            List<string> list = context.SystemGoal.Where(w =>
                w.IdParent == Id
                && (w.DateStart < DateStart || w.DateEnd > DateEnd)
            ).ToList().Select(s =>
                s.Caption + " " + s.DateStart.ToString("MM.yyyy") + " - " + s.DateEnd.ToString("MM.yyyy") + " гг."
            ).ToList();

            if (list.Any())
            {
                Controls.Check(list, "Изменение срока приводит к несоответствию со сроками реализации нижестоящих элементов СЦ:<br>{0}");
            }
        }

	    /// <summary>   
        /// Контроль "Удаление значений показателей при сокращении срока реализации элемента СЦ"
	    /// </summary>         
	    [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 180)]
	    public void Control_501111(DataContext context, SystemGoal old)
	    {
            List<string> list = new List<string>();
            for (int y = old.DateStart.Year; y <  DateStart.Year;   y++) list.Add(y.ToString(CultureInfo.InvariantCulture));
            for (int y = DateEnd.Year + 1;   y <= old.DateEnd.Year; y++) list.Add(y.ToString(CultureInfo.InvariantCulture));

            if (list.Any())
            {
                Controls.Throw(string.Format(
                    "Был изменен срок реализации элемента СЦ. Из таблицы «Целевые показатели» будут удалены значения по годам: {0}.",
                    string.Join(", ", list)
                ));
            }
        }

        /// <summary>   
        /// Продожение контроля "Удаление значений показателей при сокращении срока реализации элемента СЦ"
        /// </summary>         
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 181)]
        public void Control_501111a(DataContext context, SystemGoal old)
        {
            List<int> list = new List<int>();
            for (int y = old.DateStart.Year; y <  DateStart.Year;   y++) list.Add(y);
            for (int y = DateEnd.Year + 1;   y <= old.DateEnd.Year; y++) list.Add(y);

            if (list.Any())
            {
                var qd = context.SystemGoal_GoalIndicatorValue.Where(w => w.IdOwner == Id && list.Contains(w.HierarchyPeriod.Year));
                foreach (SystemGoal_GoalIndicatorValue i in qd)
                {
                    context.SystemGoal_GoalIndicatorValue.Remove(i);
                }

                context.SaveChanges();
            }
        }

        /// <summary>   
        /// Контроль "Наличие целевых показателей"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 190)]
        public void Control_501106(DataContext context)
        {
            if (IdRefStatus != (byte)RefStats.Work)
                return;

            if (!context.SystemGoal_GoalIndicator.Any(a => a.IdOwner == Id))
            {
                Controls.Throw("Не указан ни один целевой показатель.");
            }
        }

        /// <summary>   
        /// Контроль "Наличие значений у целевых показателей"
        /// </summary>         
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 200)]
        public void Control_501107(DataContext context)
        {
            if (IdRefStatus != (byte)RefStats.Work)
                return;

            List<string> list = context.SystemGoal_GoalIndicator.Where(a =>
                a.IdOwner == Id && !a.SystemGoal_GoalIndicatorValue.Any()
            ).Select(s => s.GoalIndicator.Caption + " - " + s.Version.Caption).ToList();

            if (list.Any())
            {
                Controls.Check(list, "У следующих целевых показателей не задано ни одного значения:<br>{0}");
            }
        }

        /// <summary>   
        /// Контроль "Изменение типа вышестоящего элемента СЦ"
        /// </summary>         
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 210)]
        public void Control_501109(DataContext context)
        {
            List<string> list = context.SystemGoal.Where(w =>
                w.IdParent == Id
                && !context.ModelSystemGoal.Any(a =>
                    a.IdElementTypeSystemGoal == w.IdElementTypeSystemGoal
                    && (a.Parent == null ? 0 : a.Parent.IdElementTypeSystemGoal) == IdElementTypeSystemGoal // это если бы After: (w.Parent == null ? 0 : w.Parent.IdElementTypeSystemGoal)
                    && a.IdRefStatus == (byte)RefStats.Work
                )
            ).ToList().Select(s =>
                s.ElementTypeSystemGoal.Caption + " «" + s.Caption + "»"
            ).ToList();

            if (list.Any())
            {
                Controls.Check(list, 
                    "Изменение типа приводит к нарушению Модели СЦ.<br>Нарушения обнаружены в следующих нижестоящих элементах:<br>{0}"
                );
            }
        }

        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 220)]
        public void DeleteGoalIndicatorParent(DataContext context, SystemGoal old)
        {
            if (old.IdParent  != IdParent)
            {
                var res = context.SystemGoal_GoalIndicatorParent.Where(s => s.IdOwner == Id);
                foreach (var item in res)
                {
                    context.SystemGoal_GoalIndicatorParent.Remove(item);
                }
                context.SaveChanges();
            }
        }

		#region Implementation of IColumnFactoryForDenormalizedTablepart

        public ColumnsInfo GetColumns(string tablepartEntityName)
		{
			if (tablepartEntityName == typeof(SystemGoal_GoalIndicator).Name)
				return GetColumnsFor_SystemGoal_GoalIndicatorValue();

			return null;
		}

        private ColumnsInfo GetColumnsFor_SystemGoal_GoalIndicatorValue()
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

