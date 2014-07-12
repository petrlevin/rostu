using System.Linq;
using System.Collections.Generic;
using BaseApp.Numerators;
using Platform.BusinessLogic.Common.Enums;
using Platform.Common.Extensions;
using Sbor.DbEnums;
using Platform.BusinessLogic.Activity.Controls;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

// ReSharper disable CheckNamespace
namespace Sbor.Reference
// ReSharper restore CheckNamespace
{
    public partial class SBP
    {
        private void InitMaps(DataContext context)
        {
            if (Parent == null && IdParent.HasValue)
                Parent = context.SBP.SingleOrDefault(a => a.Id == IdParent);
        }

        /// <summary>   
        /// Контроль "Проверка уникальности СБП"
        /// </summary>
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_500501(DataContext context)
        {
			/*проверка на уровне триггера есть
			 * SBP obj = context.SBP.FirstOrDefault(a =>
				a.IdPublicLegalFormation == IdPublicLegalFormation
				&& a.IdOrganization == IdOrganization
				&& a.IdSBPType == IdSBPType
				&& a.Id != Id
				&& (
				   ((a.ValidityFrom ?? DateTime.MinValue) <= (ValidityFrom ?? DateTime.MinValue)
						&& (a.ValidityTo ?? DateTime.MaxValue) >= (ValidityTo ?? DateTime.MaxValue))
				|| ((ValidityFrom ?? DateTime.MinValue) <= (a.ValidityFrom ?? DateTime.MinValue)
						&& (ValidityTo ?? DateTime.MaxValue) >= (a.ValidityTo ?? DateTime.MaxValue))
				) && true
			);

			if (obj != null)
				Controls.Throw(string.Format(
					"В справочнике на указанный период действия уже имеется СБП с такой же организацией и типом:<br>{0}{1}{2}{3}",
					obj.Caption,
					obj.ValidityFrom.HasValue || obj.ValidityTo.HasValue ? " -" : "",
					obj.ValidityFrom.HasValue ? obj.ValidityFrom.Value.ToString(" с dd.MM.yyyy") : "",
					obj.ValidityTo.HasValue ? obj.ValidityTo.Value.ToString(" по dd.MM.yyyy") : ""
				));*/
        }

        /// <summary>   
        /// Контроль "Проверка наличия и типа вышестоящего СБП"
        /// </summary>
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 20)]
        public void Control_500502(DataContext context)
        {
            if (SBPType != SBPType.GeneralManager) // не Главный распорядитель БС
            {
                InitMaps(context);

                var lst = new Dictionary<SBPType, bool?>();

                if (SBPType == SBPType.Manager) // Распорядитель БС
                {
                    lst.Add(SBPType.GeneralManager, null); // + Главный распорядитель БС
                }
                else if (SBPType == SBPType.TreasuryEstablishment) // Казенное учреждение
                {
                    lst.Add(SBPType.GeneralManager, null); // + Главный распорядитель БС
                    lst.Add(SBPType.Manager, null);        // + Распорядитель БС
                }
                else if (SBPType == SBPType.IndependentEstablishment
                         || SBPType == SBPType.BudgetEstablishment) // Автономное учреждение или Бюджетное учреждение
                {
                    lst.Add(SBPType.TreasuryEstablishment, true);  // + Казенное учреждение - Учередитель
                }
                else
                {
                    return;
                }

                bool fail;

                if (IdParent.HasValue)
                {
                    bool? parentIsFounder;
                    bool found = lst.TryGetValue(Parent.SBPType, out parentIsFounder);
                    fail = !found || (parentIsFounder.HasValue && Parent.IsFounder != parentIsFounder);
                }
                else
                    fail = true;

                if (fail)
                {
                    var mess = string.Empty;
// ReSharper disable LoopCanBeConvertedToQuery
                    foreach (KeyValuePair<SBPType, bool?> typ in lst)
// ReSharper restore LoopCanBeConvertedToQuery
                    {
                        mess += " - " + typ.Key.Caption() + (typ.Value == true ? " - Учредитель" : string.Empty) + "<br>";
                    }
                    Controls.Throw(string.Format(
                        "Для СБП с типом «{0}» необходимо указать вышестоящий СБП с одним из следующих допустимых типов:<br>{1}",
                        SBPType.Caption(),
                        mess
                    ));
                }
            }
        }


        #region test

        //[Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 100)]
        //public void TestControlSbp789()
        //{
        //    Controls.Throw("О господи ты боже мой! ");
        //}


        //[Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10000)]
        //public void TestControlSbp1()
        //{

        //    Controls.Throw("Тра та та та");
        //}


        //[Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10000)]
        //public void TestControlSbp2()
        //{
        //    Controls.Throw("Бла бла бла бла ");
        //}




        //[Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 100000)]
        //public void TestControlSbpFuck()
        //{
        //    Controls.Throw("Иди себе .. ");
        //}


        #endregion
        /// <summary>   
        /// Контроль "Проверка отсутствия вышестоящего СБП у ГРБС"
        /// </summary>
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 30)]
        public void Control_500503(DataContext context)
        {
            if (SBPType == SBPType.GeneralManager && IdParent.HasValue)
                Controls.Throw("Для СБП с типом «Главный распорядитель БС» нельзя указывать вышестоящего СБП.");
        }

        /// <summary>   
        /// Контроль "Проверка заполнения поля «КВСР/КАДБ/КАИФ»"
        /// </summary>
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 40)]
        public void Control_500504(DataContext context)
        {
            if (!IdKVSR.HasValue && (SBPType == SBPType.GeneralManager || SBPType == SBPType.Manager || SBPType == SBPType.TreasuryEstablishment))
                Controls.Throw("Необходимо заполнить поле «КВСР/КАДБ/КАИФ».");
        }

        /// <summary>   
        /// Контроль "Наличие необходимых бланков у СБП"
        /// </summary>
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 50)]
        public void Control_500505(DataContext context)
        {
            if (IdRefStatus != (byte)RefStats.Work)
                return;

            int idBudget = new BaseAppNumerators().IdBudget();
            string capBudget = context.Budget.Single(s => s.Id == idBudget).Caption; // это будет наименование бюджета

            var blanks = context.SBP_Blank.Where(w => w.IdOwner == Id && w.IdBudget == idBudget).ToList();

            var list = new List<string>();

            if (SBPType == SBPType.GeneralManager)
            {
                if (blanks.All(a => a.IdBlankType != (byte)BlankType.BringingGRBS)) list.Add("Доведение ГРБС");
                if (blanks.All(a => a.IdBlankType != (byte)BlankType.FormationGRBS)) list.Add("Формирование ГРБС");

                var childSbPs = context.SBP.Where(a => a.IdParent == Id && a.IdRefStatus != (byte)RefStats.Archive).ToList();

                if (childSbPs.Any(a => a.SBPType == SBPType.Manager))
                {
                    if (blanks.All(a => a.IdBlankType != (byte)BlankType.BringingRBS)) list.Add("Доведение РБС");
                }

                if (childSbPs.Any(a => a.IdSBPType == (byte)SBPType.TreasuryEstablishment))
                {
                    if (blanks.All(a => a.IdBlankType != (byte)BlankType.BringingKU))  list.Add("Доведение КУ");
                    if (blanks.All(a => a.IdBlankType != (byte)BlankType.FormationKU)) list.Add("Формирование КУ");
                }
            }
            else if (SBPType == SBPType.Manager)
            {
                if (blanks.All(a => a.IdBlankType != (byte)BlankType.BringingKU))  list.Add("Доведение КУ");
                if (blanks.All(a => a.IdBlankType != (byte)BlankType.FormationKU)) list.Add("Формирование КУ");
            }
            else if (SBPType == SBPType.TreasuryEstablishment && IsFounder)
            {
                if (blanks.All(a => a.IdBlankType != (byte)BlankType.BringingAUBU))  list.Add("Доведение АУ/БУ");
                if (blanks.All(a => a.IdBlankType != (byte)BlankType.FormationAUBU)) list.Add("Формирование АУ/БУ");
            }

            if (list.Any())
            {
                Controls.Check(list, string.Format(
                    "Отсутствуют необходимые бланки в бюджете «{0}». Для текущего СБП требуется создать бланки:<br>{{0}}",
                    capBudget
                ));
            }

            list = new List<string>();

            var parentBlanks = context.SBP_Blank.Where(w => w.IdOwner == IdParent && w.IdBudget == idBudget).ToList();

            if (SBPType == SBPType.Manager)
            {
                if (parentBlanks.All(a => a.IdBlankType != (byte)BlankType.BringingRBS)) list.Add("Доведение РБС");
                if (parentBlanks.All(a => a.IdBlankType != (byte)BlankType.FormationGRBS)) list.Add("Формирование ГРБС");
            }
            else if (SBPType == SBPType.TreasuryEstablishment) // SBORIII-399  && !IsFounder)
            {
                if (parentBlanks.All(a => a.IdBlankType != (byte)BlankType.BringingKU))  list.Add("Доведение КУ");
                if (parentBlanks.All(a => a.IdBlankType != (byte)BlankType.FormationKU)) list.Add("Формирование КУ");
            }
            else if (SBPType == SBPType.IndependentEstablishment || SBPType == SBPType.BudgetEstablishment)
            {
                if (parentBlanks.All(a => a.IdBlankType != (byte)BlankType.BringingAUBU))  list.Add("Доведение АУ/БУ");
                if (parentBlanks.All(a => a.IdBlankType != (byte)BlankType.FormationAUBU)) list.Add("Формирование АУ/БУ");
            }

            if (list.Any())
            {
                Controls.Check(list, string.Format(
                    "Отсутствуют необходимые бланки в бюджете «{0}». Для СБП «{1}» требуется создать бланки:<br>{{0}}",
                    capBudget,
                    context.SBP.Where(w => w.Id == IdParent).Select(s => s.Caption).FirstOrDefault() ?? ""
                ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка указания периодов планирования в документах АУ/БУ"
        /// </summary>
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 110)]
        public void Control_500511(DataContext context)
        {
            if (IdRefStatus != (byte)RefStats.Work)
                return;

            if (SBPType == SBPType.TreasuryEstablishment && IsFounder)
            {
                int idBudget = new BaseAppNumerators().IdBudget();
                if (!context.SBP_PlanningPeriodsInDocumentsAUBU.Any(a => a.IdBudget == idBudget))
                {
                    Controls.Throw(string.Format(
                        "Не указаны периоды планирования в документах АУ/БУ в бюджете «{0}».",
                        context.Budget.Single(s => s.Id == idBudget).Caption
                    ));
                    
                }
            }
        }

        ///// <summary>   
        ///// Контроль ""
        ///// </summary>
        //[Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 0)]
        //public void Control_(DataContext context)
        //{
        //}
    }
}

