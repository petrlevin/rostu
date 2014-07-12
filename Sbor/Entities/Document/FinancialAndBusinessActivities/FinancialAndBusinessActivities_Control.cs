using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common.Extensions;
using Sbor.DbEnums;
using Sbor.Logic;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;

// ReSharper disable CheckNamespace
namespace Sbor.Document
// ReSharper restore CheckNamespace
{
    partial class FinancialAndBusinessActivities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctType"></param>
        /// <param name="old"></param>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1000)]
        public void AutoSet(DataContext context, ControlType ctType, FinancialAndBusinessActivities old)
        {
            Caption = this.ToString();
        }


        #region Контроли

        /// <summary>
        /// Проверка уникальности документа
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка уникальности документа", InitialUNK = "0901")]
        public void Control_0901(DataContext context)
        {
            if (IdParent.HasValue)
                return;

            var doc =
                context.FinancialAndBusinessActivities.FirstOrDefault(
                    d => d.Id != Id && d.IdPublicLegalFormation == IdPublicLegalFormation
                         && d.IdBudget == IdBudget
                         && d.IdVersion == IdVersion
                         && d.IdSBP == IdSBP);

            if (doc == null) return;

            Controls.Throw(
                String.Format(
                    "Уже существует документ «План финансово-хозяйственной деятельности» для учреждения {0}.",
                    doc.SBP.Caption));
        }

        /// <summary>
        /// Проверка наличия бланков доведения и формирования АУ/БУ
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка наличия бланков доведения и формирования АУ/БУ", InitialUNK = "0902")]
        public void Control_0902(DataContext context)
        {
            if (!SBP.IdParent.HasValue)
                Controls.Throw(String.Format("У СБП {0} не установлено вышестоящее учреждение", SBP.Caption));

            if (context.SBP_Blank.Count(b => b.IdOwner == SBP.IdParent && b.IdBudget == IdBudget &&
                                             (b.IdBlankType == (byte)BlankType.FormationAUBU || b.IdBlankType == (byte)BlankType.BringingAUBU)) < 2)
            {
                var sbpParent = SBP.Parent ?? context.SBP.FirstOrDefault(s => s.Id == SBP.IdParent);
                if (sbpParent == null)
                    Controls.Throw(String.Format("Отсутствует СБП с id = {0}", SBP.IdParent));

                Controls.Throw(
                    String.Format(
                        "У СБП «{0}» отсутствует бланк доведения АУ/БУ и/или формирования АУ/БУ. Действие не выполнено.",
                        sbpParent.Caption));
            }
        }

        /// <summary>
        /// Проверка коэффициентов распределения косвенных расходов
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка коэффициентов распределения косвенных расходов", InitialUNK = "0907")]
        public void Control_0907(DataContext context)
        {
            foreach (var tpDistMethods in DistributionMethodss)
            {
                if (tpDistMethods.IdIndirectCostsDistributionMethod == (byte)IndirectCostsDistributionMethod.M4)
                {
                    if (tpDistMethods.FBA_ActivitiesDistribution.Sum(s => s.FactorOFG) != 100 ||
                        tpDistMethods.FBA_ActivitiesDistribution.Sum(s => s.FactorPFG1) != 100 ||
                        tpDistMethods.FBA_ActivitiesDistribution.Sum(s => s.FactorPFG2) != 100)
                        Controls.Throw(
                            "Для метода «Задаваемый коэффициент распределения» сумма коэффициентов за каждый год (поля Коэф. ОФГ/ПФГ1/ПФГ2) должна быть равна 100%.");
                }
            }
        }

        /// <summary>
        /// Проверка строк поступлений с незаполненными суммами
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка строк поступлений с незаполненными суммами", InitialUNK = "0908")]
        public void Control_0908(DataContext context)
        {
	        bool error = Convert.ToBoolean(context.ExecuteScalarCommand(
		        string.Format(
			        "select 1 as kol from tp.FBA_PlannedVolumeIncome a left outer join tp.FBA_PlannedVolumeIncome_value b on b.idMaster=a.id where a.idOwner={0} group by a.id having SUM(b.Value)=0 OR COUNT(b.id)=0",
			        Id)) ?? false);
			if (error)
				Controls.Throw("В документе введены строки поступлений с незаполненными суммами. Для каждой строки необходимо указать хотя бы одну сумму.");
			/*
			foreach (var pvi in PlannedVolumeIncomes)
            {
                if (!pvi.FBA_PlannedVolumeIncome_value.Any() || pvi.FBA_PlannedVolumeIncome_value.Sum(s => s.Value) == 0)
                    Controls.Throw("В документе введены строки поступлений с незаполненными суммами. Для каждой строки необходимо указать хотя бы одну сумму.");
            }
			*/
        }

        /// <summary>
        /// Проверка строк расходов с незаполненными суммами
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка строк расходов с незаполненными суммами", InitialUNK = "0909")]
        public void Control_0909(DataContext context)
        {
	        bool error = Convert.ToBoolean(context.ExecuteScalarCommand(
		        string.Format(
					"select 1 from tp.FBA_CostActivities a left outer join tp.FBA_CostActivities_value b on b.idMaster=a.id where a.idOwner={0} group by a.id having COUNT(b.id)=0 OR (SUM(b.Value)=0 AND SUM(b.Value2)=0)",
			        Id)) ?? false);
			if (error)
				Controls.Throw("В документе введены строки расходов с незаполненными суммами. Для каждой строки необходимо указать хотя бы одну сумму.");
			/*
			foreach (var ca in CostActivitiess)
            {
                if (!ca.FBA_CostActivities_value.Any() || (ca.FBA_CostActivities_value.Sum(s => s.Value) == 0 && ca.FBA_CostActivities_value.Sum(s => s.Value2) == 0))
                    Controls.Throw("В документе введены строки расходов с незаполненными суммами. Для каждой строки необходимо указать хотя бы одну сумму.");
            }
			*/
        }

        /// <summary>
        /// Проверка КФО по собственной деятельности
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка КФО по собственной деятельности", InitialUNK = "0910")]
        public void Control_0910(DataContext context)
        {
            var sBuilder = new StringBuilder();

            var error = CostActivitiess.Where(w => w.IdKFO != null && w.Master.IsOwnActivity && w.KFO.IsIncludedInBudget)
                                       .GroupBy(s => new { s.IdMaster, s.IdKFO }).ToList();

            foreach (var er in error)
            {
                var activity = context.FBA_Activity.Single(s => s.Id == er.Key.IdMaster);
                var kfo = context.KFO.Single(s => s.Id == er.Key.IdKFO);
                sBuilder.AppendFormat("<br>Мероприятие: {0}, КФО: {1} {2}", activity.Activity.Caption, kfo.Code, kfo.Caption);
            }

            var mess = sBuilder.ToString();

            if (!string.IsNullOrEmpty(mess))
                Controls.Throw("Не допускается вносить расходы за счет средств, отнесенных к бюджетным, на мероприятия, не предусмотренные в плане деятельности учреждения." + mess);
        }

        /// <summary>
        /// Превышение расходов по средствам от собственной деятельности и остаткам над плановыми суммами поступлений
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Превышение расходов по средствам от собственной деятельности и остаткам над плановыми суммами поступлений", InitialUNK = "0911")]
        public void Control_0911(DataContext context)
        {
			var firstCosts = CostActivities_values.Where(
		        w =>
		        (w.Master.IdKFO.HasValue && !w.Master.KFO.IsIncludedInBudget) ||
		        (w.Master.IdFinanceSource.HasValue && w.Master.FinanceSource.FinanceSourceType == FinanceSourceType.Remains)).
		        GroupBy(
			        g =>
			        new
				        {
					        g.Master.FinanceSource,
					        g.Master.KFO,
					        g.Master.CodeSubsidy,
					        g.HierarchyPeriod.Year,
					        g.HierarchyPeriod.DateStart.Month,
					        HierarchyPeriodCaption= g.HierarchyPeriod.Caption
				        }).Select(s => new {s.Key.FinanceSource, s.Key.KFO, s.Key.CodeSubsidy, s.Key.Year, s.Key.Month, s.Key.HierarchyPeriodCaption, SummaByMonth = s.Sum(a => a.Value)}).ToList();

			var secondCosts =
				firstCosts.Select(
			        s =>
			        new
				        {
					        s.FinanceSource,
					        s.KFO,
					        s.CodeSubsidy,
					        s.Year,
					        s.Month,
					        s.HierarchyPeriodCaption,
							SummaByMonthUp = firstCosts.Where(w => w.FinanceSource == s.FinanceSource && w.KFO == s.KFO && w.CodeSubsidy == s.CodeSubsidy && w.Year == s.Year && w.Month <= s.Month).Sum(a => a.SummaByMonth)
				        }).ToList();
			var secondPlan=firstCosts.Select(s=> 			        new
				        {
					        s.FinanceSource,
					        s.KFO,
					        s.CodeSubsidy,
					        s.Year,
					        s.Month,
					        s.HierarchyPeriodCaption,
							SummaByMonthUp = PlannedVolumeIncome_values.Where(w => w.Master.FinanceSource == s.FinanceSource && w.Master.KFO == s.KFO && w.Master.CodeSubsidy == s.CodeSubsidy && w.HierarchyPeriod.Year == s.Year && w.HierarchyPeriod.DateStart.Month <= s.Month).Sum(a => a.Value)
				        }).ToList();

	        var result = secondCosts.Join(secondPlan, left => new
		        {
			        left.FinanceSource,
			        left.KFO,
			        left.CodeSubsidy,
			        left.Year,
			        left.Month,
			        left.HierarchyPeriodCaption
		        }, right => new
			        {
				        right.FinanceSource,
				        right.KFO,
				        right.CodeSubsidy,
				        right.Year,
				        right.Month,
				        right.HierarchyPeriodCaption
			        }, (left, right) => new
				        {
					        left.FinanceSource,
					        left.KFO,
					        left.CodeSubsidy,
					        left.Year,
					        left.Month,
					        left.HierarchyPeriodCaption,
					        secondCostsValue = left.SummaByMonthUp,
					        secondPlanValue = right.SummaByMonthUp
				        }).Where(w => w.secondPlanValue - w.secondCostsValue < 0).ToList();

			if (!result.Any())
				return;

	        StringBuilder textMessage = new StringBuilder();
            var i = 1;

            foreach (var item in result)
            {
				textMessage.AppendFormat(
					"<br>{0}. КФО: {1}, Источник: {2}, Код субсидии: {3};<br>Период: {4};<br>Поступления с начала года:{5}<br>Расходы с начала года:{6}<br>Превышение:{7}",
					i, item.KFO.Code, item.FinanceSource != null ? item.FinanceSource.Code : "", item.CodeSubsidy != null ? item.CodeSubsidy.Code : "", item.HierarchyPeriodCaption, item.secondPlanValue, item.secondCostsValue, item.secondCostsValue - item.secondPlanValue);
				i++;
            }
			string mess = textMessage.ToString();

			if (!string.IsNullOrEmpty(mess))
			{
				Controls.Throw("Обнаружено превышение сумм расходов над поступлениями:" + mess);
			}

			/*
			var costs = CostActivitiess.Where(w => (w.IdKFO != null && !w.KFO.IsIncludedInBudget)
                                                   ||
                                                   (w.IdFinanceSource != null && w.FinanceSource.IdFinanceSourceType ==
                                                    (byte)FinanceSourceType.Remains)
                ).Join(CostActivities_values, activities => activities.Id, value => value.IdMaster,
                       (activities, value) => new
                       {
                           activities.IdFinanceSource,
                           activities.IdKFO,
                           activities.IdCodeSubsidy,
                           value.IdHierarchyPeriod,
                           value.HierarchyPeriod.Year,
                           value.HierarchyPeriod.DateStart.Month,
                           value.Value
                       }).GroupBy(
                               g => new { g.IdFinanceSource, g.IdKFO, g.IdCodeSubsidy, g.IdHierarchyPeriod, g.Year, g.Month })
                                       .Select(s => new
                                       {
                                           s.Key.IdFinanceSource,
                                           s.Key.IdKFO,
                                           s.Key.IdCodeSubsidy,
                                           s.Key.IdHierarchyPeriod,
                                           s.Key.Year,
                                           s.Key.Month,
                                           Sum =
                                                    CostActivities_values.Where(
                                                        w => w.HierarchyPeriod.Year == s.Key.Year
                                                             && w.HierarchyPeriod.DateStart.Month <= s.Key.Month
                                                             && w.Master.IdFinanceSource == s.Key.IdFinanceSource
                                                             && w.Master.IdKFO == s.Key.IdKFO
                                                             && w.Master.IdCodeSubsidy == s.Key.IdCodeSubsidy)
                                                                         .Sum(su => su.Value)
                                       }).ToList();

            var sBuilder = new StringBuilder();
            var i = 1;

            foreach (var cost in costs)
            {
                var incSum = PlannedVolumeIncome_values.Where(w => w.Master.IdFinanceSource == cost.IdFinanceSource
                                                                   && w.Master.IdKFO == cost.IdKFO
                                                                   && w.Master.IdCodeSubsidy == cost.IdCodeSubsidy
                                                                   && w.HierarchyPeriod.Year == cost.Year
                                                                   && w.HierarchyPeriod.DateStart.Month <= cost.Month)
                                                       .Sum(su => su.Value);
                if (incSum - cost.Sum < 0)
                {
                    var kfo = context.KFO.Single(s => s.Id == cost.IdKFO);
                    var source = context.FinanceSource.SingleOrDefault(s => s.Id == cost.IdFinanceSource);
                    var subsidy = context.CodeSubsidy.SingleOrDefault(s => s.Id == cost.IdCodeSubsidy);
                    var hierarchy = context.HierarchyPeriod.Single(s => s.Id == cost.IdHierarchyPeriod);

                    sBuilder.AppendFormat(
                        "<br>{0}. КФО: {1}, Источник: {2}, Код субсидии: {3};<br>Период: {4};<br>Поступления с начала года:{5}<br>Расходы с начала года:{6}<br>Превышение:{7}",
                        i, kfo.Code, source != null ? source.Code : "", subsidy != null ? subsidy.Code : "", hierarchy.Caption, incSum, cost.Sum, cost.Sum - incSum);
                    i++;
                }
            }

            var mess = sBuilder.ToString();

            if (!string.IsNullOrEmpty(mess))
            {
                Controls.Throw("Обнаружено превышение сумм расходов над поступлениями:" + mess);
            }
			*/
        }

        /// <summary>
        /// Превышение расходов за счет субсидий над суммами, предусмотренными учредителем
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Превышение расходов за счет субсидий над суммами, предусмотренными учредителем", InitialUNK = "0912")]
        public void Control_0912(DataContext context)
        {
            Control0912_0916(context, true);
        }

        /// <summary>
        /// Превышение расходов за счет субсидий над суммами, предусмотренными учредителем
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Превышение расходов за счет субсидий над суммами, предусмотренными учредителем", InitialUNK = "0928")]
        public void Control_0928(DataContext context)
        {
            Control0912_0916(context, true);
        }

        /// <summary>
        /// Проверка заполнения полей КБК по бланку
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка заполнения полей КБК по бланку", InitialUNK = "0913")]
        public void Control_0913(DataContext context)
        {
            var blank =
                SBP.Parent.SBP_Blank.SingleOrDefault(
                    w => w.IdBudget == IdBudget && w.IdBlankType == (byte)BlankType.FormationAUBU);

            if (blank == null)
                Controls.Throw("Отсутствует бланк Формирование АУ/БУ");


            var sBuilder = new StringBuilder();


            foreach (var activity in Activity)
            {
                var listFields = new List<String>();
                //Осторожно говно код ToDo: сделать адекватно и красиво
                foreach (var fca in activity.FBA_CostActivities)
                {
                    if (blank.IdBlankValueType_FinanceSource == (byte)BlankValueType.Mandatory && !fca.IdFinanceSource.HasValue)
                        listFields.Add("Источник");
                    if (blank.IdBlankValueType_BranchCode == (byte)BlankValueType.Mandatory && !fca.IdBranchCode.HasValue)
                        listFields.Add("Отраслевой код");
                    if (blank.IdBlankValueType_CodeSubsidy == (byte)BlankValueType.Mandatory && !fca.IdCodeSubsidy.HasValue)
                        listFields.Add("Код субсидий");
                    if (blank.IdBlankValueType_DEK == (byte)BlankValueType.Mandatory && !fca.IdDEK.HasValue)
                        listFields.Add("ДЕК");
                    if (blank.IdBlankValueType_DFK == (byte)BlankValueType.Mandatory && !fca.IdDFK.HasValue)
                        listFields.Add("ДФК");
                    if (blank.IdBlankValueType_DKR == (byte)BlankValueType.Mandatory && !fca.IdDKR.HasValue)
                        listFields.Add("ДКР");
                    if (blank.IdBlankValueType_ExpenseObligationType == (byte)BlankValueType.Mandatory && !fca.IdExpenseObligationType.HasValue)
                        listFields.Add("Тип РО");
                    if (blank.IdBlankValueType_KCSR == (byte)BlankValueType.Mandatory && !fca.IdKCSR.HasValue)
                        listFields.Add("КЦСР");
                    if (blank.IdBlankValueType_KFO == (byte)BlankValueType.Mandatory && !fca.IdKFO.HasValue)
                        listFields.Add("КФО");
                    if (blank.IdBlankValueType_KOSGU == (byte)BlankValueType.Mandatory && !fca.IdKOSGU.HasValue)
                        listFields.Add("КОСГУ");
                    if (blank.IdBlankValueType_KVR == (byte)BlankValueType.Mandatory && !fca.IdKVR.HasValue)
                        listFields.Add("КВР");
                    if (blank.IdBlankValueType_KVSR == (byte)BlankValueType.Mandatory && !fca.IdKVSR.HasValue)
                        listFields.Add("КВСР");
                    if (blank.IdBlankValueType_RZPR == (byte)BlankValueType.Mandatory && !fca.IdRZPR.HasValue)
                        listFields.Add("РзПР");
                }

                if (listFields.Any())
                    sBuilder.AppendFormat("<br>Мероприятие '{0}': {1}", activity.Activity.Caption, listFields.GetString(","));
            }

            var mess = sBuilder.ToString();

            if (!string.IsNullOrEmpty(mess))
            {
                Controls.Throw(mess);
            }

        }

        /// <summary>
        /// Проверка соответствия расходов и объемов мероприятий из задания
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка соответствия расходов и объемов мероприятий из задания", InitialUNK = "0914")]
        public void Control_0914(DataContext context)
        {
            var taskVolumes = context.TaskVolume.Where(w => w.IdSBP == IdSBP
															&& w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && w.IdBudget == IdBudget
                                                            && w.IdVersion == IdVersion
                                                            && w.HierarchyPeriod.Year >= Budget.Year
                                                            && w.HierarchyPeriod.Year <= Budget.Year + 2
                                                            && w.IdTerminator == null)
                                     .GroupBy(
                                         g =>
                                         new
                                         {
                                             g.TaskCollection.IdActivity,
                                             g.TaskCollection.IdContingent,
                                             g.HierarchyPeriod.Year
                                         })
                                     .Select(s => new
                                     {
                                         s.Key.IdActivity,
                                         s.Key.IdContingent,
                                         s.Key.Year,
                                         sum = s.Sum(su => su.Value)
                                     }).ToList();

            var activity = Activity.Where(w => !w.IsOwnActivity)
                                    .Join(CostActivitiess, fbaActivity => fbaActivity.Id,
                                          activities => activities.IdMaster, (fbaActivity, activities) => new
                                          {
                                              fbaActivity.IdActivity,
                                              fbaActivity.IdContingent,
                                              activities.Id
                                          })
                                    .Join(CostActivities_values, arg => arg.Id, value => value.IdMaster,
                                          (arg, value) => new
                                          {
                                              arg.IdActivity,
                                              arg.IdContingent,
                                              value.HierarchyPeriod.Year,
                                              value.Value
                                          })
                                    .GroupBy(g => new { g.IdActivity, g.IdContingent, g.Year })
                                    .Select(s => new
                                    {
                                        s.Key.IdActivity,
                                        s.Key.IdContingent,
                                        s.Key.Year,
                                        sum = s.Sum(su => su.Value)
                                    }).ToList();


            var sBuilder = new StringBuilder();
            var sBuilder2 = new StringBuilder();

            foreach (var taskVolume in taskVolumes)
            {
                var act =
                    activity.Where(
                        w => w.IdActivity == taskVolume.IdActivity
                            && w.IdContingent == taskVolume.IdContingent
                            && w.Year == taskVolume.Year).Sum(s => s.sum);

                if (act == 0)
                {
                    var activityCaption = context.Activity.Single(s => s.Id == taskVolume.IdActivity).Caption;
                    var contingent = context.Contingent.SingleOrDefault(s => s.Id == taskVolume.IdContingent);
                    var contingentCaption = contingent != null ? contingent.Caption : "";

                    sBuilder.AppendFormat("<br> -{0} ({1}),{2};", activityCaption, contingentCaption, taskVolume.Year);
                }
            }

            foreach (var act in activity)
            {
                var tv =
                    taskVolumes.Where(
                        w => w.IdActivity == act.IdActivity
                            && w.IdContingent == act.IdContingent
                            && w.Year == act.Year).Sum(s => s.sum);

                if (tv == 0)
                {
                    var activityCaption = context.Activity.Single(s => s.Id == act.IdActivity).Caption;
                    var contingent = context.Contingent.SingleOrDefault(s => s.Id == act.IdContingent);
                    var contingentCaption = contingent != null ? contingent.Caption : "";

                    sBuilder2.AppendFormat("<br> -{0} ({1}),{2};", activityCaption, contingentCaption, act.Year);
                }
            }

            var mess = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(sBuilder.ToString()))
                mess.Add("Планом деятельности предусмотрено выполнение следующих мероприятий, но не введены строки расходов:", sBuilder.ToString());
            if (!string.IsNullOrEmpty(sBuilder2.ToString()))
                mess.Add("В документе предусмотрены расходы по мероприятиям за периоды, в которых планом деятельности не предусмотрено выполнение этих мероприятий:", sBuilder2.ToString());

            var message = string.Empty;
            var i = 1;
            foreach (var mes in mess)
            {
                message += (i > 1 ? "<br>" : "") + i + ". " + mes.Key + mes.Value;
                i++;
            }
            if (!string.IsNullOrEmpty(message))
                Controls.Throw(message);
        }

        /// <summary>
        /// Проверка утверждения мероприятий
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка утверждения мероприятий", InitialUNK = "0915")]
        public void Control_0915(DataContext context)
        {
            var sBuilder = new StringBuilder();
            var error = false;

            var tasks = context.TaskVolume.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation
                                                      && w.IdBudget == IdBudget
                                                      && w.IdVersion == IdVersion
                                                      && w.IdSBP == IdSBP
                                                      && (w.DateTerminate == null || w.DateTerminate > Date))
                               .ToList()
                               .Where(w => w.DateCommit != null && w.DateCommit.Value.Date <= Date.Date)
                               .Select(s => new { s.TaskCollection.IdActivity, s.TaskCollection.IdContingent })
                               .Distinct();


            var docTasks =
                Activity.Where(w => !w.IsOwnActivity).Select(s => new { s.IdActivity, s.IdContingent }).ToList();

            foreach (var task in tasks)
            {
                var existsActivity = docTasks.Any(f => f.IdActivity == task.IdActivity
                                                       && f.IdContingent == task.IdContingent);
                if (!existsActivity)
                {
                    var act = context.Activity.Single(s => s.Id == task.IdActivity);
                    var con = context.Contingent.SingleOrDefault(s => s.Id == task.IdContingent);

                    sBuilder.AppendFormat("<br/>{0} ({1})", act.Caption, con != null ? con.Caption : "");
                    error = true;
                }
            }

            if (error)
                Controls.Throw("Скорректируйте перечень мероприятий  либо дату ПФХД. Планом деятельности учреждения на дату ПФХД предусмотрено выполнение следующих мероприятий, отсутствующих в документе: " + sBuilder);

            foreach (var task in docTasks)
            {
                var existsActivity = tasks.Any(f => f.IdActivity == task.IdActivity
                                                       && f.IdContingent == task.IdContingent);
                if (!existsActivity)
                {
                    var act = context.Activity.Single(s => s.Id == task.IdActivity);
                    var con = context.Contingent.SingleOrDefault(s => s.Id == task.IdContingent);

                    sBuilder.AppendFormat("<br>{0} ({1})", act.Caption, con != null ? con.Caption : "");
                    error = true;
                }
            }

            if (error)
                Controls.Throw("Проверьте утверждение плана деятельности. " +
                               "План деятельности учреждения должен быть утвержден, и его дата должна быть не больше даты ПФХД. " +
                               "При необходимости, скорректируйте дату ПФХД. На дату " + Date.ToShortDateString() + " планом деятельности не утверждены мероприятия:" + sBuilder);
        }

        /// <summary>
        /// Проверка утверждения предельных объемов субсидий.
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка утверждения предельных объемов субсидий", InitialUNK = "0916")]
        public void Control_0916(DataContext context)
        {
            Control0912_0916(context, false);
        }

        /// <summary>
        /// Проверка актуальности СБП, кодов КБК.
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка актуальности СБП, кодов КБК.", InitialUNK = "0917")]
        public void Control_0917(DataContext context)
        {
            //Проверка на актульность СБП
            if (((SBP.ValidityFrom.HasValue && SBP.ValidityFrom.Value > Date) || (SBP.ValidityTo.HasValue && SBP.ValidityTo.Value <= Date)))
                Controls.Throw("В поле Учреждение указан СБП, не действующий на дату документа.");

            //Мероприятие(Контингент) : Список ошибок
            var errors =new Dictionary<string, List<string>>();
            //var errors = this.GetWrongVersioningKBK(BlankFormationAUBU, typeof(FBA_PlannedVolumeIncome), "FBA_Activity", context, "", "IdFBA_Activity");

            
            foreach (var pvl in PlannedVolumeIncomes)
            {
                var result = new List<string> { pvl.FinanceSource.ToString() };
                bool hasErrors = false;

                if (IsActual(pvl.KFO))
                {
                    hasErrors = true;
                    result.Add("<b>" + pvl.KFO + "</b>");
                }
                else
                    result.Add(pvl.KFO.ToString());

                if (pvl.IdCodeSubsidy.HasValue)
                    if (IsActual(pvl.CodeSubsidy))
                    {
                        hasErrors = true;
                        result.Add("<b>" + pvl.CodeSubsidy + "</b>");
                    }
                    else
                        result.Add(pvl.CodeSubsidy.ToString());

                var error = hasErrors ? String.Join(", ", result) : null;

                if (error != null)
                {
                    if (pvl.IdFBA_Activity != null)
                    {
                        var con = pvl.FBA_Activity.Contingent;
                        var key = pvl.FBA_Activity.Activity.Caption + "(" +
                                  (con != null ? con.Caption : "") + ")";
                        var value = "- " + error + "<br/>";
                        if (errors.ContainsKey(key))
                            errors[key].Add(value);
                        else
                            errors.Add(key, new List<string> { value });
                    }
                    else
                    {
                        const string key = "";
                        var value = "- " + error + "<br/>";
                        if (errors.ContainsKey(key))
                            errors[key].Add(value);
                        else
                            errors.Add(key, new List<string> { value });
                        }
                    }
                }
            
            if (errors.Any())
            {
                Controls.Throw(FormatMessage(errors, "В таблице «Плановые объемы поступлений» указаны строки с недействующими КБК (выделены жирным шрифтом):"));
            }

            var vErrors = this.GetWrongVersioningKBK(BlankFormationAUBU, typeof (FBA_CostActivities), "FBA_Activity", context);
            
            
            /*foreach (var expense in CostActivitiess)
            {
                var error = expense.CheckVersioningKbk(Date);
                if (error != null)
                {
                    var key = expense.Master.Activity.Caption + "(" +
                              expense.Master.Contingent.Caption + ")";
                    var value = "- " + error + "<br/>";

                    if (errors.ContainsKey(key))
                        errors[key].Add(value);
                    else
                        errors.Add(key, new List<string> { value });
                }
            }*/

            if (vErrors.Any())
            {
                Controls.Throw(FormatMessage(vErrors, "В таблице «Расходы по мероприятиям» указаны строки с недействующими КБК (выделены жирным шрифтом):"));
            }
        }

        /// <summary>
        /// Проверка даты документа
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка даты документа", InitialUNK = "0918")]
        public void Control_0918(DataContext context)
        {
            if (IdParent.HasValue)
            {
                var parentDoc = context.FinancialAndBusinessActivities.Single(s => s.Id == IdParent);
                if (parentDoc.Date > Date)
                {
                    const string sMsg = "Дата документа не может быть меньше даты предыдущей редакции.<br>" +
                                        "Дата текущего документа: {0}<br>" +
                                        "Дата предыдущей редакции: {1}";

                    Controls.Throw(string.Format(sMsg, Date.ToShortDateString(), parentDoc.Date.ToShortDateString()));
                }
            }
        }

        /// <summary>
        /// Очистка доп. потребности
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Очистка доп. потребности", InitialUNK = "0919", InitialSkippable = true)]
        public void Control_0919(DataContext context)
        {
            if (IsExtraNeed == false)
            {
                if (context.FBA_CostActivities_value.Any(a => a.IdOwner == Id && a.Value2.HasValue))
                {
                    Controls.Throw("Признак «Вести доп. потребности» отключен. Все доп. потребности в документе будут очищены. Продолжить?");
                }
            }
        }

        /// <summary>
        /// Проверка признака «Вести доп. потребности»
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка признака «Вести доп. потребности»", InitialUNK = "0920", InitialSkippable = true)]
        public void Control_0920(DataContext context)
        {
            if (IsExtraNeed)
                Controls.Throw("Документ ведется с доп. потребностями. Вы запустили операцию утверждения базовых значений. Будет создана и утверждена новая редакция документа с очищенными данными по доп. потребностям. Продолжить?");
        }

        /// <summary>
        ///Проверка признака «Вести доп. потребности»
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка признака «Вести доп. потребности»", InitialUNK = "0921")]
        public void Control_0921(DataContext context)
        {
            if (!IsExtraNeed)
                Controls.Throw("В документе отсутствуют значения по доп. потребностям. Воспользуйтесь операцией «Утвердить».");
        }


        /// <summary>
        ///Проверка признака «Вести доп. потребности»
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка признака «Вести доп. потребности»", InitialUNK = "0922", InitialSkippable = true)]
        public void Control_0922(DataContext context)
        {
            if (IsExtraNeed)
                Controls.Throw("Будет создана и утверждена новая редакция документа – данные по доп. потребностям будут суммированы с базовыми значениями. " +
                               "Продолжить?");
        }


        /// <summary>
        ///Проверка соответствия мероприятий в ТЧ и регистре
        /// </summary>
        [ControlInitial(InitialCaption = "Проверка соответствия мероприятий в ТЧ и регистре", InitialUNK = "0925", InitialSkippable = true, InitialManaged = false)]
        public void Control_0925(IEnumerable<string> deletingActivities, bool hasIndirect = false)
        {
            if (deletingActivities.Any())
            {
                var msg = !hasIndirect ? "Следующие мероприятия отсутствуют в проектном документе «План деятельности» и будут удалены из таблицы: <br/> " 
                                      : @"Следующие мероприятия отсутствуют в проектном документе «План деятельности» и будут удалены из таблицы «Мероприятия», 
                                           а также из таблицы «Мероприятия для распределения», т.к. они участвовали в распределении косвенных расходов. 
                                           Необходимо перераспределить косвенные расходы. <br/>" ;
                Controls.Throw(
                    msg + string.Join("<br/>", deletingActivities.ToList()) );
            }
        }

        /// <summary>
        /// Превышение сумм доп. потребностей за счет субсидий над суммами доп. потребностей, предусмотренными учредителем
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Превышение сумм доп. потребностей за счет субсидий над суммами доп. потребностей, предусмотренными учредителем", InitialUNK = "0926")]
        public void Control_0926(DataContext context)
        {
            Control0926_0929(context);
        }

        /// <summary>
        /// Превышение сумм доп. потребностей за счет субсидий над суммами доп. потребностей, предусмотренными учредителем
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Превышение сумм доп. потребностей за счет субсидий над суммами доп. потребностей, предусмотренными учредителем", InitialUNK = "0929")]
        public void Control_0929(DataContext context)
        {
            Control0926_0929(context);
        }

        private void Control0926_0929(DataContext context)
        {
            var blanks =
                SBP.Parent.SBP_Blank.Where(
                    w => w.IdBudget == IdBudget
                         && (w.IdBlankType == (byte) BlankType.BringingAUBU
                             || w.IdBlankType == (byte) BlankType.FormationAUBU)).ToList();

            var bKfo = blanks.All(w => w.IdBlankValueType_KFO == (byte) BlankValueType.Mandatory);
            var bBranchCode = blanks.All(w => w.IdBlankValueType_BranchCode == (byte) BlankValueType.Mandatory);
            var bCodeSubsidy = blanks.All(w => w.IdBlankValueType_CodeSubsidy == (byte) BlankValueType.Mandatory);
            var bDek = blanks.All(w => w.IdBlankValueType_DEK == (byte) BlankValueType.Mandatory);
            var bDfk = blanks.All(w => w.IdBlankValueType_DFK == (byte) BlankValueType.Mandatory);
            var bDkr = blanks.All(w => w.IdBlankValueType_DKR == (byte) BlankValueType.Mandatory);
            var bExpenseObligationType =
                blanks.All(w => w.IdBlankValueType_ExpenseObligationType == (byte) BlankValueType.Mandatory);
            var bFinanceSource = blanks.All(w => w.IdBlankValueType_FinanceSource == (byte) BlankValueType.Mandatory);
            var bKcsr = blanks.All(w => w.IdBlankValueType_KCSR == (byte) BlankValueType.Mandatory);
            var bKosgu = blanks.All(w => w.IdBlankValueType_KOSGU == (byte) BlankValueType.Mandatory);
            var bKvr = blanks.All(w => w.IdBlankValueType_KVR == (byte) BlankValueType.Mandatory);
            var bKvsr = blanks.All(w => w.IdBlankValueType_KVSR == (byte) BlankValueType.Mandatory);
            var bRzpr = blanks.All(w => w.IdBlankValueType_RZPR == (byte) BlankValueType.Mandatory);

            var costs =
                CostActivitiess.Where(
                    w =>
                    (w.IdKFO != null && w.KFO.IsIncludedInBudget) &&
                    (w.IdFinanceSource != null &&
                     w.FinanceSource.IdFinanceSourceType != (byte) FinanceSourceType.Remains))
                               .Join(CostActivities_values, activities => activities.Id, value => value.IdMaster,
                                     (activities, value) => new
                                         {
                                             IdFinanceSource = bFinanceSource ? activities.IdFinanceSource : null,
                                             IdKFO = bKfo ? activities.IdKFO : null,
                                             IdCodeSubsidy = bCodeSubsidy ? activities.IdCodeSubsidy : null,
                                             IdBranchCode = bBranchCode ? activities.IdBranchCode : null,
                                             IdDEK = bDek ? activities.IdDEK : null,
                                             IdDFK = bDfk ? activities.IdDFK : null,
                                             IdDKR = bDkr ? activities.IdDKR : null,
                                             IdExpenseObligationType =
                                                                bExpenseObligationType
                                                                    ? activities.IdExpenseObligationType
                                                                    : null,
                                             IdKCSR = bKcsr ? activities.IdKCSR : null,
                                             IdKOSGU = bKosgu ? activities.IdKOSGU : null,
                                             IdKVR = bKvr ? activities.IdKVR : null,
                                             IdKVSR = bKvsr ? activities.IdKVSR : null,
                                             IdRZPR = bRzpr ? activities.IdRZPR : null,
                                             value.IdHierarchyPeriod,
                                             value.HierarchyPeriod.Year,
                                             value.HierarchyPeriod.DateStart.Month,
                                             value.Value2
                                         }).GroupBy(
                                             g => new
                                                 {
                                                     g.IdFinanceSource,
                                                     g.IdKFO,
                                                     g.IdCodeSubsidy,
                                                     g.IdBranchCode,
                                                     g.IdDEK,
                                                     g.IdDFK,
                                                     g.IdDKR,
                                                     g.IdExpenseObligationType,
                                                     g.IdKCSR,
                                                     g.IdKOSGU,
                                                     g.IdKVR,
                                                     g.IdKVSR,
                                                     g.IdRZPR,
                                                     g.IdHierarchyPeriod,
                                                     g.Year,
                                                     g.Month
                                                 }).Select(s => new
                                                     {
                                                         s.Key.IdFinanceSource,
                                                         s.Key.IdKFO,
                                                         s.Key.IdCodeSubsidy,
                                                         s.Key.IdBranchCode,
                                                         s.Key.IdDEK,
                                                         s.Key.IdDFK,
                                                         s.Key.IdDKR,
                                                         s.Key.IdExpenseObligationType,
                                                         s.Key.IdKCSR,
                                                         s.Key.IdKOSGU,
                                                         s.Key.IdKVR,
                                                         s.Key.IdKVSR,
                                                         s.Key.IdRZPR,
                                                         s.Key.IdHierarchyPeriod,
                                                         s.Key.Year,
                                                         s.Key.Month,
                                                         Sum = CostActivities_values.Where(
                                                             w => w.HierarchyPeriod.Year == s.Key.Year
                                                                  && w.HierarchyPeriod.DateStart.Month <= s.Key.Month
                                                                  &&
                                                                  (!bFinanceSource ||
                                                                   w.Master.IdFinanceSource == s.Key.IdFinanceSource)
                                                                  && (!bKfo || w.Master.IdKFO == s.Key.IdKFO)
                                                                  &&
                                                                  (!bCodeSubsidy ||
                                                                   w.Master.IdCodeSubsidy == s.Key.IdCodeSubsidy)
                                                                  &&
                                                                  (!bBranchCode || w.Master.IdBranchCode == s.Key.IdBranchCode)
                                                                  && (!bDek || w.Master.IdDEK == s.Key.IdDEK)
                                                                  && (!bDfk || w.Master.IdDFK == s.Key.IdDFK)
                                                                  && (!bDkr || w.Master.IdDKR == s.Key.IdDKR)
                                                                  &&
                                                                  (!bExpenseObligationType ||
                                                                   w.Master.IdExpenseObligationType ==
                                                                   s.Key.IdExpenseObligationType)
                                                                  && (!bKcsr || w.Master.IdKCSR == s.Key.IdKCSR)
                                                                  && (!bKosgu || w.Master.IdKOSGU == s.Key.IdKOSGU)
                                                                  && (!bKvr || w.Master.IdKVR == s.Key.IdKVR)
                                                                  && (!bKvsr || w.Master.IdKVSR == s.Key.IdKVSR)
                                                                  && (!bRzpr || w.Master.IdRZPR == s.Key.IdRZPR)
                                                                    ).Sum(su => su.Value2)
                                                     }).ToList();

            var income =
                context.LimitVolumeAppropriations.Where(
                    w =>
                    w.IdPublicLegalFormation == IdPublicLegalFormation
                    && w.IdBudget == IdBudget
                    && w.IdVersion == IdVersion
                    && w.EstimatedLine.IdSBP == IdSBP
                    && w.IdValueType == (byte) ValueType.Plan
                    && w.HasAdditionalNeed == true).Select(s => new
                        {
                            IdFinanceSource = bFinanceSource ? s.EstimatedLine.IdFinanceSource : null,
                            IdKFO = bKfo ? s.EstimatedLine.IdKFO : null,
                            IdCodeSubsidy = bCodeSubsidy ? s.EstimatedLine.IdCodeSubsidy : null,
                            IdBranchCode = bBranchCode ? s.EstimatedLine.IdBranchCode : null,
                            IdDEK = bDek ? s.EstimatedLine.IdDEK : null,
                            IdDFK = bDfk ? s.EstimatedLine.IdDFK : null,
                            IdDKR = bDkr ? s.EstimatedLine.IdDKR : null,
                            IdExpenseObligationType = bExpenseObligationType ? s.EstimatedLine.IdExpenseObligationType : null,
                            IdKCSR = bKcsr ? s.EstimatedLine.IdKCSR : null,
                            IdKOSGU = bKosgu ? s.EstimatedLine.IdKOSGU : null,
                            IdKVR = bKvr ? s.EstimatedLine.IdKVR : null,
                            IdKVSR = bKvsr ? s.EstimatedLine.IdKVSR : null,
                            IdRZPR = bRzpr ? s.EstimatedLine.IdRZPR : null,
                            s.IdHierarchyPeriod,
                            s.HierarchyPeriod.Year,
                            s.HierarchyPeriod.DateStart.Month,
                            s.Value
                        }).GroupBy(g => new
                            {
                                g.IdFinanceSource,
                                g.IdKFO,
                                g.IdCodeSubsidy,
                                g.IdBranchCode,
                                g.IdDEK,
                                g.IdDFK,
                                g.IdDKR,
                                g.IdExpenseObligationType,
                                g.IdKCSR,
                                g.IdKOSGU,
                                g.IdKVR,
                                g.IdKVSR,
                                g.IdRZPR,
                                g.Year,
                                g.Month
                            }).Select(s => new
                                {
                                    s.Key.IdFinanceSource,
                                    s.Key.IdKFO,
                                    s.Key.IdCodeSubsidy,
                                    s.Key.IdBranchCode,
                                    s.Key.IdDEK,
                                    s.Key.IdDFK,
                                    s.Key.IdDKR,
                                    s.Key.IdExpenseObligationType,
                                    s.Key.IdKCSR,
                                    s.Key.IdKOSGU,
                                    s.Key.IdKVR,
                                    s.Key.IdKVSR,
                                    s.Key.IdRZPR,
                                    s.Key.Year,
                                    s.Key.Month,
                                    Value = s.Sum(su => su.Value),
                                }).ToList();

            var sBuilder = new StringBuilder();
            var i = 1;
            foreach (var cost in costs)
            {
                var incSum = income.Where(w => w.IdFinanceSource == cost.IdFinanceSource
                                               && w.IdKFO == cost.IdKFO
                                               && w.IdCodeSubsidy == cost.IdCodeSubsidy
                                               && w.IdBranchCode == cost.IdBranchCode
                                               && w.IdDEK == cost.IdDEK
                                               && w.IdDFK == cost.IdDFK
                                               && w.IdDKR == cost.IdDKR
                                               && w.IdExpenseObligationType == cost.IdExpenseObligationType
                                               && w.IdKCSR == cost.IdKCSR
                                               && w.IdKOSGU == cost.IdKOSGU
                                               && w.IdKVR == cost.IdKVR
                                               && w.IdKVSR == cost.IdKVSR
                                               && w.IdRZPR == cost.IdRZPR
                                               && w.Year == cost.Year
                                               && w.Month <= cost.Month).Sum(su => su.Value);

                if (incSum - cost.Sum < 0)
                {
                    var kfo = context.KFO.SingleOrDefault(s => s.Id == cost.IdKFO);
                    var source = context.FinanceSource.SingleOrDefault(s => s.Id == cost.IdFinanceSource);
                    var expenseObligationType = cost.IdExpenseObligationType != null
                                                    ? ((ExpenseObligationType) cost.IdExpenseObligationType).Caption()
                                                    : null;
                    var kcsr = context.KCSR.SingleOrDefault(s => s.Id == cost.IdKCSR);
                    var kvr = context.KVR.SingleOrDefault(s => s.Id == cost.IdKVR);
                    var kosgu = context.KOSGU.SingleOrDefault(s => s.Id == cost.IdKOSGU);
                    var dkr = context.DKR.SingleOrDefault(s => s.Id == cost.IdDKR);
                    var dfk = context.DFK.SingleOrDefault(s => s.Id == cost.IdDFK);
                    var dek = context.DEK.SingleOrDefault(s => s.Id == cost.IdDEK);
                    var branchCode = context.BranchCode.SingleOrDefault(s => s.Id == cost.IdBranchCode);
                    var rzpr = context.RZPR.SingleOrDefault(s => s.Id == cost.IdRZPR);
                    var kvsr = context.KVSR.SingleOrDefault(s => s.Id == cost.IdKVSR);
                    var subsidy = context.CodeSubsidy.SingleOrDefault(s => s.Id == cost.IdCodeSubsidy);
                    var hierarchy = context.HierarchyPeriod.Single(s => s.Id == cost.IdHierarchyPeriod);

                    var kbk = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
                                            expenseObligationType != null
                                                ? "Тип РО: " + expenseObligationType + ","
                                                : "",
                                            kfo != null ? "КФО: " + kfo.Code + "," : "",
                                            source != null ? "Источник: " + source.Code + "," : "",
                                            kvsr != null ? "КВСР: " + kvsr.Caption + "," : "",
                                            rzpr != null ? "РзПР: " + rzpr.Code + "," : "",
                                            kcsr != null ? "КЦСР: " + kcsr.Code + "," : "",
                                            kvr != null ? "КВР: " + kvr.Code + "," : "",
                                            kosgu != null ? "КОСГУ: " + kosgu.Code + "," : "",
                                            dkr != null ? "ДКР: " + dkr.Code + "," : "",
                                            dfk != null ? "ДФК: " + dfk.Code + "," : "",
                                            dek != null ? "ДЭК: " + dek.Code + "," : "",
                                            subsidy != null ? "Код субсидии: " + subsidy.Code + "," : "",
                                            branchCode != null ? "Отраслевой код: " + branchCode.Code : ""
                        );

                    if (kbk.EndsWith(","))
                        kbk = kbk.Remove(kbk.Length - 1);

                    sBuilder.AppendFormat(
                        "<br>{0}. {1}:{2}{3}{4}{5}",
                        i,
                        kbk,
                        "<br>Период: " + hierarchy.Caption,
                        "<br>Поступления с начала года: " + incSum,
                        "<br>Расходы с начала года: " + cost.Sum,
                        "<br>Превышение: " + (cost.Sum - incSum));
                    i++;
                }
            }

            var mess = sBuilder.ToString();

            if (!string.IsNullOrEmpty(mess))
            {
                Controls.Throw(
                    "Обнаружено превышение сумм дополнительных потребностей за счет субсидий над суммами дополнительных потребностей, предусмотренными учредителем:" +
                    mess);
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия текущего бланка формирования с актуальным"
        /// </summary>         
        [ControlInitial(InitialUNK = "0927", InitialCaption = "Проверка соответствия текущего бланка формирования с актуальным", InitialSkippable = true)]
        public void Control_0927(DataContext context)
        {
            var newBlanks =
                context.SBP_Blank.Where(r =>
                                        r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.IdParent &&
                                        r.IdBlankType == (byte)DbEnums.BlankType.FormationAUBU);

            var oldBlank = this.SBP_BlankActual;

            var newBlank = newBlanks.FirstOrDefault();

            bool fc;
            if (oldBlank == null)
            {
                fc = true;
            }
            else
            {
                fc = !SBP_BlankHelper.IsEqualBlank(newBlank, oldBlank);
            }

            if (fc)
                Controls.Throw(string.Format("Был изменен бланк «{0}». " +
                                             "Необходимо актуализировать сведения в таблицах «Расходы по мероприятиям» и «Косвенные расходы». " +
                                             "В строках перечисленных таблиц будут очищены КБК, не соответствующие бланку формирования, и выполнится группировка сметных строк.",
                                             newBlank.BlankType.Caption()));
        }

        /// <summary>   
        /// Контроль "Проверка на округление до сотен"
        /// </summary>
        [ControlInitial(InitialUNK = "0930", InitialCaption = "Проверка на округление до сотен", InitialManaged = true)]
        public void Control_0930(DataContext context)
        {

            var tp = context.FBA_CostActivities_value.Where(r => r.IdOwner == this.Id)
                            .Select(s => new { v = s, c = s.Master, a = s.Master.Master }).ToList()
                            .Where(r =>
                                (r.v.Value.HasValue && !CommonMethods.IsRound100(r.v.Value.Value)) ||
                                (r.v.Value2.HasValue && !CommonMethods.IsRound100(r.v.Value2.Value)))
                            .Select(s => new {s.v, s.c, s.a, e = s.c.GetEstimatedLine(context).ToString()});

            if (!tp.Any())
            {
                return;
            }

            var msg = "Для сметных строк указана сумма, не округленная до сотен:<br>{0}";
            var sb = new StringBuilder();

            foreach (var t in tp.Select(s => s.a).Distinct().OrderBy(o => o.Activity.Caption))
            {
                sb.AppendFormat("{0} {1}<br>", t.Activity.Caption,
                                t.IdContingent.HasValue ? " - " + t.Contingent.Caption : "");

                foreach (var l in tp.Where(r => r.a == t).Select(s => s.e).Distinct().OrderBy(o => o))
                {
                    sb.AppendFormat("    - {0} <br>", l);
                }
            }

            Controls.Throw(string.Format(msg, sb.Replace("Вид бюджетной деятельности Расходы, ","")));
        }

        #endregion

        #region Методы контролей

        private string FormatMessage(Dictionary<string, List<string>> errors, string mess)
        {
            var errorMsg = new StringBuilder(mess);

            foreach (var error in errors)
            {
                errorMsg.Append("<br/>" + error.Key);

                foreach (var value in error.Value)
                    errorMsg.Append("<br/>" + value);
            }

            return errorMsg.ToString();
        }

        private bool IsActual(IVersioning obj)
        {
            return (obj.ValidityFrom.HasValue && obj.ValidityFrom.Value > Date) ||
                    (obj.ValidityTo.HasValue && obj.ValidityTo.Value <= Date);
        }

        private void Control0912_0916(DataContext context, bool is0912)
        {
            var blanks =
                SBP.Parent.SBP_Blank.Where(
                    w => w.IdBudget == IdBudget
                         && (w.IdBlankType == (byte)BlankType.BringingAUBU
                             || w.IdBlankType == (byte)BlankType.FormationAUBU)).ToList();

            var bKfo = blanks.All(w => w.IdBlankValueType_KFO == (byte)BlankValueType.Mandatory);
            var bBranchCode = blanks.All(w => w.IdBlankValueType_BranchCode == (byte)BlankValueType.Mandatory);
            var bCodeSubsidy = blanks.All(w => w.IdBlankValueType_CodeSubsidy == (byte)BlankValueType.Mandatory);
            var bDek = blanks.All(w => w.IdBlankValueType_DEK == (byte)BlankValueType.Mandatory);
            var bDfk = blanks.All(w => w.IdBlankValueType_DFK == (byte)BlankValueType.Mandatory);
            var bDkr = blanks.All(w => w.IdBlankValueType_DKR == (byte)BlankValueType.Mandatory);
            var bExpenseObligationType = blanks.All(w => w.IdBlankValueType_ExpenseObligationType == (byte)BlankValueType.Mandatory);
            var bFinanceSource = blanks.All(w => w.IdBlankValueType_FinanceSource == (byte)BlankValueType.Mandatory);
            var bKcsr = blanks.All(w => w.IdBlankValueType_KCSR == (byte)BlankValueType.Mandatory);
            var bKosgu = blanks.All(w => w.IdBlankValueType_KOSGU == (byte)BlankValueType.Mandatory);
            var bKvr = blanks.All(w => w.IdBlankValueType_KVR == (byte)BlankValueType.Mandatory);
            var bKvsr = blanks.All(w => w.IdBlankValueType_KVSR == (byte)BlankValueType.Mandatory);
            var bRzpr = blanks.All(w => w.IdBlankValueType_RZPR == (byte)BlankValueType.Mandatory);

            var costs =
                CostActivitiess.Where(
                    w =>
                    (w.IdKFO != null && w.KFO.IsIncludedInBudget) &&
                    (w.IdFinanceSource != null &&
                     w.FinanceSource.IdFinanceSourceType != (byte)FinanceSourceType.Remains))
                               .Join(CostActivities_values, activities => activities.Id, value => value.IdMaster,
                                     (activities, value) => new
                                     {
                                         IdFinanceSource = bFinanceSource ? activities.IdFinanceSource : null,
                                         IdKFO = bKfo ? activities.IdKFO : null,
                                         IdCodeSubsidy = bCodeSubsidy ? activities.IdCodeSubsidy : null,
                                         IdBranchCode = bBranchCode ? activities.IdBranchCode : null,
                                         IdDEK = bDek ? activities.IdDEK : null,
                                         IdDFK = bDfk ? activities.IdDFK : null,
                                         IdDKR = bDkr ? activities.IdDKR : null,
                                         IdExpenseObligationType = bExpenseObligationType ? activities.IdExpenseObligationType : null,
                                         IdKCSR = bKcsr ? activities.IdKCSR : null,
                                         IdKOSGU = bKosgu ? activities.IdKOSGU : null,
                                         IdKVR = bKvr ? activities.IdKVR : null,
                                         IdKVSR = bKvsr ? activities.IdKVSR : null,
                                         IdRZPR = bRzpr ? activities.IdRZPR : null,
                                         value.IdHierarchyPeriod,
                                         value.HierarchyPeriod.Year,
                                         value.HierarchyPeriod.DateStart.Month,
                                         value.Value
                                     }).GroupBy(
                                           g => new
                                           {
                                               g.IdFinanceSource,
                                               g.IdKFO,
                                               g.IdCodeSubsidy,
                                               g.IdBranchCode,
                                               g.IdDEK,
                                               g.IdDFK,
                                               g.IdDKR,
                                               g.IdExpenseObligationType,
                                               g.IdKCSR,
                                               g.IdKOSGU,
                                               g.IdKVR,
                                               g.IdKVSR,
                                               g.IdRZPR,
                                               g.IdHierarchyPeriod,
                                               g.Year,
                                               g.Month
                                           }).Select(s => new
                                           {
                                               s.Key.IdFinanceSource,
                                               s.Key.IdKFO,
                                               s.Key.IdCodeSubsidy,
                                               s.Key.IdBranchCode,
                                               s.Key.IdDEK,
                                               s.Key.IdDFK,
                                               s.Key.IdDKR,
                                               s.Key.IdExpenseObligationType,
                                               s.Key.IdKCSR,
                                               s.Key.IdKOSGU,
                                               s.Key.IdKVR,
                                               s.Key.IdKVSR,
                                               s.Key.IdRZPR,
                                               s.Key.IdHierarchyPeriod,
                                               s.Key.Year,
                                               s.Key.Month,
                                               Sum = CostActivities_values.Where(
                                                            w => w.HierarchyPeriod.Year == s.Key.Year
                                                                 && w.HierarchyPeriod.DateStart.Month <= s.Key.Month
                                                                 && (!bFinanceSource || w.Master.IdFinanceSource == s.Key.IdFinanceSource)
                                                                 && (!bKfo || w.Master.IdKFO == s.Key.IdKFO)
                                                                 && (!bCodeSubsidy || w.Master.IdCodeSubsidy == s.Key.IdCodeSubsidy)
                                                                 && (!bBranchCode || w.Master.IdBranchCode == s.Key.IdBranchCode)
                                                                 && (!bDek || w.Master.IdDEK == s.Key.IdDEK)
                                                                 && (!bDfk || w.Master.IdDFK == s.Key.IdDFK)
                                                                 && (!bDkr || w.Master.IdDKR == s.Key.IdDKR)
                                                                 && (!bExpenseObligationType || w.Master.IdExpenseObligationType == s.Key.IdExpenseObligationType)
                                                                 && (!bKcsr || w.Master.IdKCSR == s.Key.IdKCSR)
                                                                 && (!bKosgu || w.Master.IdKOSGU == s.Key.IdKOSGU)
                                                                 && (!bKvr || w.Master.IdKVR == s.Key.IdKVR)
                                                                 && (!bKvsr || w.Master.IdKVSR == s.Key.IdKVSR)
                                                                 && (!bRzpr || w.Master.IdRZPR == s.Key.IdRZPR)
                                                                 ).Sum(su => su.Value)
                                           }).ToList();

            var income =
                context.LimitVolumeAppropriations.Where(
                    w =>
                    w.IdPublicLegalFormation == IdPublicLegalFormation
                    && w.IdBudget == IdBudget
                    && w.IdVersion == IdVersion
                    && w.EstimatedLine.IdSBP == IdSBP
                    && w.IdValueType == (byte)ValueType.Plan
                    && (w.HasAdditionalNeed == false || w.HasAdditionalNeed == null)
                    && (is0912 || w.DateCommit <= DateCommit)).Select(s => new
                    {
                        IdFinanceSource = bFinanceSource ? s.EstimatedLine.IdFinanceSource : null,
                        IdKFO = bKfo ? s.EstimatedLine.IdKFO : null,
                        IdCodeSubsidy = bCodeSubsidy ? s.EstimatedLine.IdCodeSubsidy : null,
                        IdBranchCode = bBranchCode ? s.EstimatedLine.IdBranchCode : null,
                        IdDEK = bDek ? s.EstimatedLine.IdDEK : null,
                        IdDFK = bDfk ? s.EstimatedLine.IdDFK : null,
                        IdDKR = bDkr ? s.EstimatedLine.IdDKR : null,
                        IdExpenseObligationType = bExpenseObligationType ? s.EstimatedLine.IdExpenseObligationType : null,
                        IdKCSR = bKcsr ? s.EstimatedLine.IdKCSR : null,
                        IdKOSGU = bKosgu ? s.EstimatedLine.IdKOSGU : null,
                        IdKVR = bKvr ? s.EstimatedLine.IdKVR : null,
                        IdKVSR = bKvsr ? s.EstimatedLine.IdKVSR : null,
                        IdRZPR = bRzpr ? s.EstimatedLine.IdRZPR : null,
                        s.IdHierarchyPeriod,
                        s.HierarchyPeriod.Year,
                        s.HierarchyPeriod.DateStart.Month,
                        s.Value
                    }).GroupBy(g => new
                    {
                        g.IdFinanceSource,
                        g.IdKFO,
                        g.IdCodeSubsidy,
                        g.IdBranchCode,
                        g.IdDEK,
                        g.IdDFK,
                        g.IdDKR,
                        g.IdExpenseObligationType,
                        g.IdKCSR,
                        g.IdKOSGU,
                        g.IdKVR,
                        g.IdKVSR,
                        g.IdRZPR,
                        g.Year,
                        g.Month
                    }).Select(s => new
                    {
                        s.Key.IdFinanceSource,
                        s.Key.IdKFO,
                        s.Key.IdCodeSubsidy,
                        s.Key.IdBranchCode,
                        s.Key.IdDEK,
                        s.Key.IdDFK,
                        s.Key.IdDKR,
                        s.Key.IdExpenseObligationType,
                        s.Key.IdKCSR,
                        s.Key.IdKOSGU,
                        s.Key.IdKVR,
                        s.Key.IdKVSR,
                        s.Key.IdRZPR,
                        s.Key.Year,
                        s.Key.Month,
                        Value = s.Sum(su => su.Value),
                    }).ToList();

            var sBuilder = new StringBuilder();
            var i = 1;
            foreach (var cost in costs)
            {
                var incSum = income.Where(w => w.IdFinanceSource == cost.IdFinanceSource
                                               && w.IdKFO == cost.IdKFO
                                               && w.IdCodeSubsidy == cost.IdCodeSubsidy
                                               && w.IdBranchCode == cost.IdBranchCode
                                               && w.IdDEK == cost.IdDEK
                                               && w.IdDFK == cost.IdDFK
                                               && w.IdDKR == cost.IdDKR
                                               && w.IdExpenseObligationType == cost.IdExpenseObligationType
                                               && w.IdKCSR == cost.IdKCSR
                                               && w.IdKOSGU == cost.IdKOSGU
                                               && w.IdKVR == cost.IdKVR
                                               && w.IdKVSR == cost.IdKVSR
                                               && w.IdRZPR == cost.IdRZPR
                                               && w.Year == cost.Year
                                               && w.Month <= cost.Month).Sum(su => su.Value);

                if (incSum - cost.Sum < 0)
                {
                    var kfo = context.KFO.SingleOrDefault(s => s.Id == cost.IdKFO);
                    var source = context.FinanceSource.SingleOrDefault(s => s.Id == cost.IdFinanceSource);
                    var expenseObligationType = cost.IdExpenseObligationType != null
                                                    ? ((ExpenseObligationType)cost.IdExpenseObligationType).Caption()
                                                    : null;
                    var kcsr = context.KCSR.SingleOrDefault(s => s.Id == cost.IdKCSR);
                    var kvr = context.KVR.SingleOrDefault(s => s.Id == cost.IdKVR);
                    var kosgu = context.KOSGU.SingleOrDefault(s => s.Id == cost.IdKOSGU);
                    var dkr = context.DKR.SingleOrDefault(s => s.Id == cost.IdDKR);
                    var dfk = context.DFK.SingleOrDefault(s => s.Id == cost.IdDFK);
                    var dek = context.DEK.SingleOrDefault(s => s.Id == cost.IdDEK);
                    var branchCode = context.BranchCode.SingleOrDefault(s => s.Id == cost.IdBranchCode);
                    var rzpr = context.RZPR.SingleOrDefault(s => s.Id == cost.IdRZPR);
                    var kvsr = context.KVSR.SingleOrDefault(s => s.Id == cost.IdKVSR);
                    var subsidy = context.CodeSubsidy.SingleOrDefault(s => s.Id == cost.IdCodeSubsidy);
                    var hierarchy = context.HierarchyPeriod.Single(s => s.Id == cost.IdHierarchyPeriod);

                    var kbk = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
                                            expenseObligationType != null
                                                ? "Тип РО: " + expenseObligationType + ","
                                                : "",
                                            kfo != null ? "КФО: " + kfo.Code + "," : "",
                                            source != null ? "Источник: " + source.Code + "," : "",
                                            kvsr != null ? "КВСР: " + kvsr.Caption + "," : "",
                                            rzpr != null ? "РзПР: " + rzpr.Code + "," : "",
                                            kcsr != null ? "КЦСР: " + kcsr.Code + "," : "",
                                            kvr != null ? "КВР: " + kvr.Code + "," : "",
                                            kosgu != null ? "КОСГУ: " + kosgu.Code + "," : "",
                                            dkr != null ? "ДКР: " + dkr.Code + "," : "",
                                            dfk != null ? "ДФК: " + dfk.Code + "," : "",
                                            dek != null ? "ДЭК: " + dek.Code + "," : "",
                                            subsidy != null ? "Код субсидии: " + subsidy.Code + "," : "",
                                            branchCode != null ? "Отраслевой код: " + branchCode.Code : ""
                        );

                    if (kbk.EndsWith(","))
                        kbk = kbk.Remove(kbk.Length - 1);

                    sBuilder.AppendFormat(
                        "<br>{0}. {1}:{2}{3}{4}{5}",
                        i,
                        kbk,
                        "<br>Период: " + hierarchy.Caption,
                        "<br>Поступления с начала года: " + incSum,
                        "<br>Расходы с начала года: " + cost.Sum,
                        "<br>Превышение: " + (cost.Sum - incSum));
                    i++;
                }
            }

            var mess = sBuilder.ToString();

            if (!string.IsNullOrEmpty(mess))
            {
                Controls.Throw("Обнаружено превышение сумм расходов над предельными объемами субсидий, предусмотренными учредителем:" + mess);
            }
        }

        #endregion

    }
}
