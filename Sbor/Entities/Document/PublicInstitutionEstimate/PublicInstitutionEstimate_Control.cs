using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BaseApp;
using BaseApp.Activity.Operations;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.Common.Exceptions;
using Platform.Common.Extensions;
using Platform.Utils.Extensions;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Registry;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;
using EntityFramework.Extensions;

// ReSharper disable CheckNamespace
namespace Sbor.Document
// ReSharper restore CheckNamespace
{
	public partial class PublicInstitutionEstimate
	{
		#region Доп. методы

		private IEnumerable<string> CheckLimitAllocations0714(DataContext context, IEnumerable<ISBP_Blank> blanks, bool withDate = false, int? idKosgu000 = null)
		{
			var limitErrors = new List<string>();

			// ReSharper disable PossibleInvalidOperationException
			var query = GetQueryForAllocations(blanks, IdSBP, typeof(PublicInstitutionEstimate_Expense).Name, withDate, AdditionalNeed: false);
			// ReSharper restore PossibleInvalidOperationException
			var year = Budget.Year;

			for (int i = 0; i < 3; i++)
			{
				var yearlimitErrors = new List<String>();

				var idHierarchy = year.GetIdHierarchyPeriodYear(context);
				var yearQuery = String.Format(query, (i == 0) ? "OFG" : (i == 1 ? "PFG1" : "PFG2"), idHierarchy);

				var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

				if (idKosgu000.HasValue)
				{
					//Проходим по строкам, для которых имеются совпадения с записями в регистрах
					foreach (var volumeAppropriationResult in result.Where(r => r.PlanValue.HasValue))
					{
						volumeAppropriationResult.PlanValue = volumeAppropriationResult.PlanValue ?? 0;

						if (volumeAppropriationResult.Value > volumeAppropriationResult.PlanValue)
						{
							var estimatedLine = volumeAppropriationResult.GetEstimatedLine(context);
							
							limitErrors.Add(
								String.Format(
									"{4}, {0} - Нераспределенный остаток = {1}, Объем средств из документа = {2}, Разность = {3}",
									estimatedLine,
									volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue,
									volumeAppropriationResult.Value,
									volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue - volumeAppropriationResult.Value,
									year));
						}
					}

					result = result.Where(r => !r.PlanValue.HasValue && !r.BringValue.HasValue).ToList();
					var kosguResult = new List<LimitVolumeAppropriationResult>();
					//Проходим по строкам, для которых не найденно соответствий. И либо сразу добавляем ошибку, либо проставляем косгу = 0
					foreach (var volumeAppropriationResult in result)
					{
						var idKOSGU = volumeAppropriationResult.IdKOSGU;

						volumeAppropriationResult.PlanValue = volumeAppropriationResult.PlanValue ?? 0;

						//Проставляем КОСГУ = 0
						volumeAppropriationResult.IdKOSGU = idKosgu000;

						if (!volumeAppropriationResult.GetQueryForExistingReg(context, "EstimatedLine").Any())
						{
							volumeAppropriationResult.IdKOSGU = idKOSGU;
							var estimatedLine = volumeAppropriationResult.GetEstimatedLine(context);

							yearlimitErrors.Add(
							 String.Format(
								 "{4}, {0} - Объем средств = {2}, Обоснованные ассигнования = {1}, Разность = {3};",
								 estimatedLine,
								 volumeAppropriationResult.Value,
								 volumeAppropriationResult.PlanValue,
								 volumeAppropriationResult.PlanValue - volumeAppropriationResult.Value,
								 year));

							//result.Remove(volumeAppropriationResult);
						}
						else
						{
							//volumeAppropriationResult.Value = null;
							volumeAppropriationResult.PlanValue = null;
							volumeAppropriationResult.BringValue = null;

							kosguResult.Add(volumeAppropriationResult);
						}
					}

					//Остались строки для которых имеются записи в регистра с КОСГУ = 0
					var kosguKbkResults = new List<LimitVolumeAppropriationResult>();
					foreach (var limitVolumeAppropriationResult in kosguResult)
					{
						var t = limitVolumeAppropriationResult.CloneAsLineCost<LimitVolumeAppropriationResult>();
						t.Value = null;
						t.BringValue = null;
						t.PlanValue = null;

						kosguKbkResults.Add(t);
					}
					kosguKbkResults = kosguKbkResults.Distinct().ToList();

					foreach (var limitVolumeAppropriationResult in kosguKbkResults)
					{
						var planLimitVolumeAppropriationsQuery =
							String.Format("Select ISNULL(SUM(L.Value), 0) " +
										  "From reg.LimitVolumeAppropriations L " +
										  "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
										  "Where L.idHierarchyPeriod = {5} And " +
											"L.idPublicLegalFormation = {1} And " +
											"L.idBudget = {2} And " +
											"L.idValueType = {0} And " +
											"L.idVersion = {3} {4}",
												(int)ValueType.Plan,
												IdPublicLegalFormation, IdBudget, IdVersion, limitVolumeAppropriationResult.GetWhereQuery("EL"), idHierarchy);
						var planValue = context.Database.SqlQuery<decimal?>(planLimitVolumeAppropriationsQuery).FirstOrDefault() ?? 0;

						var docValue = kosguResult.AsQueryable().ApplyWhere(limitVolumeAppropriationResult).Sum(s => s.Value);

						if (docValue > planValue)
						{
							var estimatedLine = limitVolumeAppropriationResult.GetEstimatedLine(context);

							limitErrors.Add(
								String.Format(
									"{4}, {0} - Нераспределенный остаток = {1}, Объем средств из документа = {2}, Разность = {3}",
									estimatedLine,
									planValue,
									docValue,
									planValue - docValue,
									year));

							yearlimitErrors.Add(
							String.Format(
								"{4}, {0} - Объем средств = {2}, Обоснованные ассигнования = {1}, Разность = {3};",
								estimatedLine,
								docValue,
								planValue,
								planValue - docValue,
								year));
						}
					}
				}
				else
					foreach (var r in result)
					{
						r.Value = r.Value ?? 0;
						r.PlanValue = r.PlanValue ?? 0;

						if (r.Value > r.PlanValue)
						{
							//var estimatedLine = r.GetEstimatedLine(context);
							var estimatedLine = r.ToStringAsEstimatedLine(context);

							yearlimitErrors.Add(
								String.Format(
									"{4}, {0} - Объем средств = {2}, Обоснованные ассигнования = {1}, Разность = {3};",
									estimatedLine,
									r.Value,
									r.PlanValue,
									r.PlanValue - r.Value,
									year));
						}
					}

				if (yearlimitErrors.Any())
				{
					limitErrors.Add(String.Join("<br/>", yearlimitErrors));
				}

				year++;
			}

			return limitErrors;
		}

		private IEnumerable<String> CheckLimitAllocations0717(DataContext context, IEnumerable<ISBP_Blank> blanks, bool withDate = false)
		{
			var limitErrors = new List<string>();

			// ReSharper disable PossibleInvalidOperationException
			var query = GetQueryForAllocations(blanks, IdSBP, typeof(PublicInstitutionEstimate_FounderAUBUExpense).Name, withDate, isMeanAuBu: true, AdditionalNeed: false);
			// ReSharper restore PossibleInvalidOperationException
			var year = Budget.Year;

			for (int i = 0; i < 3; i++)
			{
				var yearlimitErrors = new List<String>();

				var idHierarchy = year.GetIdHierarchyPeriodYear(context);
				var yearQuery = String.Format(query, (i == 0) ? "OFG" : (i == 1 ? "PFG1" : "PFG2"), idHierarchy);

				var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

				foreach (var r in result)
				{
					r.Value = r.Value ?? 0;
					r.PlanValue = r.PlanValue ?? 0;

					if (r.Value > r.PlanValue)
					{
						var estimatedLine = r.GetEstimatedLine(context);

						yearlimitErrors.Add(
							String.Format(
								"{4}, {0} - Объем средств = {2}, Обоснованные ассигнования = {1}, Разность = {3};",
								estimatedLine,
								r.Value,
								r.PlanValue,
								r.PlanValue - r.Value,
								year));
					}
				}

				if (yearlimitErrors.Any())
				{
					limitErrors.Add(String.Join("<br/>", yearlimitErrors));
				}

				year++;
			}

			return limitErrors;
		}

		private IEnumerable<string> CheckLimitAllocations0730(DataContext context, IEnumerable<ISBP_Blank> blanks, bool withDate = false, int? idKosgu000 = null)
		{
			var limitErrors = new List<string>();

			var query = GetQueryForAllocations(blanks, IdSBP, typeof(PublicInstitutionEstimate_Expense).Name, withDate, AdditionalNeed: true);

			var year = Budget.Year;

			for (int i = 0; i < 3; i++)
			{
				var yearlimitErrors = new List<String>();

				var idHierarchy = year.GetIdHierarchyPeriodYear(context);
				var yearQuery = String.Format(query, (i == 0) ? "AdditionalOFG" : (i == 1 ? "AdditionalPFG1" : "AdditionalPFG2"), idHierarchy);

				var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

				if (idKosgu000.HasValue)
				{
					//Проходим по строкам, для которых имеются совпадения с записями в регистрах
					foreach (var volumeAppropriationResult in result.Where(r => r.PlanValue.HasValue))
					{
						volumeAppropriationResult.PlanValue = volumeAppropriationResult.PlanValue ?? 0;

						if (volumeAppropriationResult.Value > volumeAppropriationResult.PlanValue)
						{
							var estimatedLine = volumeAppropriationResult.GetEstimatedLine(context);

							limitErrors.Add(
								String.Format(
									"{4}, {0} - Объем средств = {1}, Обоснованные ассигнования = {2}, Разность = {3}",
									estimatedLine,
									volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue,
									volumeAppropriationResult.Value,
									volumeAppropriationResult.PlanValue - volumeAppropriationResult.BringValue - volumeAppropriationResult.Value,
									year));
						}
					}

					result = result.Where(r => !r.PlanValue.HasValue && !r.BringValue.HasValue).ToList();
					var kosguResult = new List<LimitVolumeAppropriationResult>();
					//Проходим по строкам, для которых не найденно соответствий. И либо сразу добавляем ошибку, либо проставляем косгу = 0
					foreach (var volumeAppropriationResult in result)
					{
						var idKOSGU = volumeAppropriationResult.IdKOSGU;

						volumeAppropriationResult.PlanValue = volumeAppropriationResult.PlanValue ?? 0;

						//Проставляем КОСГУ = 0
						volumeAppropriationResult.IdKOSGU = idKosgu000;

						if (!volumeAppropriationResult.GetQueryForExistingReg(context, "EstimatedLine").Any())
						{
							volumeAppropriationResult.IdKOSGU = idKOSGU;
							var estimatedLine = volumeAppropriationResult.GetEstimatedLine(context);

							yearlimitErrors.Add(
							 String.Format(
								 "{4}, {0} - Объем средств = {2}, Обоснованные ассигнования = {1}, Разность = {3};",
								 estimatedLine,
								 volumeAppropriationResult.Value,
								 volumeAppropriationResult.PlanValue,
								 volumeAppropriationResult.PlanValue - volumeAppropriationResult.Value,
								 year));

							//result.Remove(volumeAppropriationResult);
						}
						else
						{
							//volumeAppropriationResult.Value = null;
							volumeAppropriationResult.PlanValue = null;
							volumeAppropriationResult.BringValue = null;

							kosguResult.Add(volumeAppropriationResult);
						}
					}

					//Остались строки для которых имеются записи в регистра с КОСГУ = 0
					var kosguKbkResults = new List<LimitVolumeAppropriationResult>();
					foreach (var limitVolumeAppropriationResult in kosguResult)
					{
						var t = limitVolumeAppropriationResult.CloneAsLineCost<LimitVolumeAppropriationResult>();
						t.Value = null;
						t.BringValue = null;
						t.PlanValue = null;

						kosguKbkResults.Add(t);
					}
					kosguKbkResults = kosguKbkResults.Distinct().ToList();

					foreach (var limitVolumeAppropriationResult in kosguKbkResults)
					{
						var planLimitVolumeAppropriationsQuery =
							String.Format("Select ISNULL(SUM(L.Value), 0) " +
										  "From reg.LimitVolumeAppropriations L " +
										  "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
										  "Where L.idHierarchyPeriod = {5} And " +
											"L.idPublicLegalFormation = {1} And " +
											"L.idBudget = {2} And " +
											"L.idValueType = {0} And " +
											"L.idVersion = {3} {4}",
												(int)ValueType.Plan,
												IdPublicLegalFormation, IdBudget, IdVersion, limitVolumeAppropriationResult.GetWhereQuery("EL"), idHierarchy);
						var planValue = context.Database.SqlQuery<decimal?>(planLimitVolumeAppropriationsQuery).FirstOrDefault() ?? 0;

						var docValue = kosguResult.AsQueryable().ApplyWhere(limitVolumeAppropriationResult).Sum(s => s.Value);

						if (docValue > planValue)
						{
							var estimatedLine = limitVolumeAppropriationResult.GetEstimatedLine(context);

							limitErrors.Add(
								String.Format(
									"{4}, {0} - Нераспределенный остаток = {1}, Объем средств из документа = {2}, Разность = {3}",
									estimatedLine,
									planValue,
									docValue,
									planValue - docValue,
									year));

							yearlimitErrors.Add(
							String.Format(
								"{4}, {0} - Объем средств = {2}, Обоснованные ассигнования = {1}, Разность = {3};",
								estimatedLine,
								docValue,
								planValue,
								planValue - docValue,
								year));
						}
					}
				}
				else
					foreach (var r in result)
					{
						r.Value = r.Value ?? 0;
						r.PlanValue = r.PlanValue ?? 0;

						if (r.Value > r.PlanValue)
						{
							//var estimatedLine = r.GetEstimatedLine(context);
							var estimatedLine = r.ToStringAsEstimatedLine(context);

							yearlimitErrors.Add(
								String.Format(
									"{4}, {0} - Объем средств = {2}, Обоснованные ассигнования = {1}, Разность = {3};",
									estimatedLine,
									r.Value,
									r.PlanValue,
									r.PlanValue - r.Value,
									year));
						}
					}

				if (yearlimitErrors.Any())
				{
					limitErrors.Add(String.Join("<br/>", yearlimitErrors));
				}

				year++;
			}

			return limitErrors;
		}

		private IEnumerable<String> CheckLimitAllocations0731(DataContext context, IEnumerable<ISBP_Blank> blanks, bool withDate = false)
		{
			var limitErrors = new List<string>();

			// ReSharper disable PossibleInvalidOperationException
			var query = GetQueryForAllocations(blanks, IdSBP, typeof(PublicInstitutionEstimate_FounderAUBUExpense).Name, withDate, isMeanAuBu: true, AdditionalNeed: true);
			// ReSharper restore PossibleInvalidOperationException
			var year = Budget.Year;

			for (int i = 0; i < 3; i++)
			{
				var yearlimitErrors = new List<String>();

				var idHierarchy = year.GetIdHierarchyPeriodYear(context);
				var yearQuery = String.Format(query, (i == 0) ? "AdditionalOFG" : (i == 1 ? "AdditionalPFG1" : "AdditionalPFG2"), idHierarchy);

				var result = context.Database.SqlQuery<LimitVolumeAppropriationResult>(yearQuery).ToList();

				foreach (var r in result)
				{
					r.Value = r.Value ?? 0;
					r.PlanValue = r.PlanValue ?? 0;

					if (r.PlanValue - r.Value < 0)
					{
						var estimatedLine = r.GetEstimatedLine(context);

						yearlimitErrors.Add(
							String.Format(
								"{4}, {0} - Объем средств = {2}, Обоснованные ассигнования = {1}, Разность = {3};",
								estimatedLine,
								r.Value,
								r.PlanValue,
								r.PlanValue - r.Value,
								year));
					}
				}

				if (yearlimitErrors.Any())
				{
					limitErrors.Add(String.Join("<br/>", yearlimitErrors));
				}

				year++;
			}

			return limitErrors;
		}


		private string GetQueryForAllocations(IEnumerable<ISBP_Blank> blanks, int idQuerySBP, string tpName, bool withDate = false, bool isMeanAuBu = false, bool isIncludeHaving = true, bool AdditionalNeed = false)
		{
			var costLineProperties = SBP_BlankHelper.GetBlanksCostMandatoryProperties(blanks).ToList();
			
			const String limitAllocationYear = "{0}";
			const String limitVolumeIdHierarchy = "{1}";
			
			var kbkString = costLineProperties.GetQueryString();
			var estimatedLineKbkString = costLineProperties.GetQueryString("EL");

			var estimatedLineKbkRequieredConditions = new StringBuilder();
			foreach (var costLineProperty in costLineProperties)
				estimatedLineKbkRequieredConditions.Append("EL." + costLineProperty + " IS NOT NULL And ");

			var limitAllocationQuery = String.Format("Select {0}, " +
												"{1} as value, " +
												"Null as regValue " +
												"From tp.{3} " +
												"Where idOwner = {2}", kbkString, limitAllocationYear, Id, tpName);

			var planLimitVolumeAppropriationsQuery = String.Format("Select {0}, " +
																   "Null as value, " +
																   "L.Value as regValue " +
																   "From reg.LimitVolumeAppropriations L " +
																   "Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine " +
																   "Where EL.idSBP = {7} And " +
																   "(L.idRegistratorEntity <> {8} Or L.idRegistrator Not In ({9}) ) And " +
																   "{6} L.idValueType = {1} And " +
																   "L.idHierarchyPeriod = {2} And " +
																   "L.idPublicLegalFormation = {3} And " +
																   "L.idBudget = {4} And " +
																   "L.idVersion = {5} And " +
																   "L.isMeansAUBU = {10} And " +
																   "ISNULL( L.HasAdditionalNeed, 0 ) = {12} " +
																   "{11}",
																   estimatedLineKbkString,
																   (int) ValueType.Plan,
																   limitVolumeIdHierarchy,
																   IdPublicLegalFormation, IdBudget, IdVersion,
																   estimatedLineKbkRequieredConditions,
																   idQuerySBP,
																   EntityId,
																   PrevVersionIds.GetQueryString(),
																   isMeanAuBu ? 1 : 0,
																   withDate
																	   ? (String.Format(
																		   " And L.DateCommit Is Not Null And L.DateCommit <= '{0}' ",
																		   Date.ToString(new CultureInfo("en-US"))))
																	   : " ",
																   AdditionalNeed ? 1 : 0);

			var innerQuery = limitAllocationQuery + " Union All " + planLimitVolumeAppropriationsQuery;

			var cmd = String.Format("Select {0}, " +
							 "CAST(SUM(G.Value) as numeric(20,2) ) as value, " +
							 "CAST(SUM(G.regValue) as numeric(20,2) ) as planValue " +
							 "From ({1}) G " +
							 "Group By {0} " +
							 (isIncludeHaving ? "Having SUM(G.Value) > 0 " : String.Empty), kbkString, innerQuery, (int)ValueType.Plan, (int)ValueType.Bring);

			return cmd;
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(ExcludeFromSetup = true)]
		[Control(ControlType.Insert | ControlType.Update, Sequence.After, 10)]
		public void AutoSet(DataContext context)
		{
			Caption = ToString();
		}

		/// <summary>
		/// Проверка уникальности документа
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка уникальности документа", InitialUNK = "0701")]
		public void Control_0701(DataContext context)
		{
			if (IdParent.HasValue)
				return;

			if ( context.PublicInstitutionEstimate.Any(d => d.Id != Id //&& d.IdPublicLegalFormation == IdPublicLegalFormation
																	  && d.IdVersion == IdVersion
																	  && d.IdSBP == IdSBP
																	  && d.IdBudget == IdBudget) )
				Controls.Throw(String.Format("Уже существует документ «Смета казенного учреждения» для учреждения «{0}».", SBP.Caption));
		}

		/// <summary>
		/// Проверка наличия бланков доведения и формирования АУ/БУ
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка наличия бланков доведения и формирования АУ/БУ", InitialUNK = "0702")]
		public void Control_0702(DataContext context)
		{
			if (!context.SBP_Blank.Any(b => b.IdOwner == SBP.IdParent && b.IdBudget == IdBudget && b.IdBlankType == (byte)BlankType.FormationKU))
				Controls.Throw(String.Format("У СБП «{0}» отсутствует бланк формирования КУ. Действие не выполнено.", SBPParent.Caption));
		}

		/// <summary>
		/// Проверка строк в ТЧ Расходы с незаполненными суммами
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка строк в ТЧ Расходы с незаполненными суммами", InitialUNK = "0703")]
		public void Control_0703(DataContext context)
		{
			if ((Expenses.Any(e => (!e.OFG.HasValue || e.OFG.Value == 0) && (!e.PFG1.HasValue || e.PFG1.Value == 0) && (!e.PFG2.HasValue || e.PFG2.Value == 0))) ||
					(FounderAUBUExpenses.Any(e => (!e.OFG.HasValue || e.OFG.Value == 0) && (!e.PFG1.HasValue || e.PFG1.Value == 0) && (!e.PFG2.HasValue || e.PFG2.Value == 0))))
				Controls.Throw("В документе введены строки расходов с незаполненными суммами. Для каждой строки необходимо указать хотя бы одну сумму.");
		}

		/// <summary>
		/// Проверка коэффициентов распределения косвенных расходов
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка коэффициентов распределения косвенных расходов", InitialUNK = "0704")]
		public void Control_0704(DataContext context)
		{
			/*if (DistributionMethods.All(d => d.IdIndirectCostsDistributionMethod != (byte)IndirectCostsDistributionMethod.M4))
				return;*/

			//Мероприятия для распределения с типом М4
			var distributiveActivities = DistributionActivities.AsQueryable()
															   .Where(a => DistributionMethods.AsQueryable()
																						.Where(d => d.IdIndirectCostsDistributionMethod == (byte)IndirectCostsDistributionMethod.M4)
																						.Any(d => d.Id == a.IdMaster))
															   .GroupBy(e => true)
															   .Select(e => new
															   {
																   FactorOFG = e.Sum(i => i.FactorOFG.Value),
																   FactorPFG1 = e.Sum(i => i.FactorPFG1.Value),
																   FactorPFG2 = e.Sum(i => i.FactorPFG2.Value)
															   })
															   .FirstOrDefault() ?? new
															   {
																   FactorOFG = 100,
																   FactorPFG1 = 100,
																   FactorPFG2 = 100
															   };

			if (distributiveActivities.FactorOFG != 100 ||
					distributiveActivities.FactorPFG1 != 100 ||
						distributiveActivities.FactorPFG2 != 100)
				Controls.Throw("Для метода «Задаваемый коэффициент распределения» сумма коэффициентов за каждый год (поля Коэф. ОФГ/ПФГ1/ПФГ2) должна быть равна 100%.");
		}

		/// <summary>
		///Проверка даты документа
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка даты документа", InitialUNK = "0705")]
		public void Control_0705(DataContext context)
		{
			if (!IdParent.HasValue || Parent == null) return;
			if ((Date - Parent.Date).TotalDays < 0)
				Controls.Throw("Дата документа должна быть больше или равна дате предыдущей редакции. <br/>" +
							   "Дата текущего документа: " + Date.ToShortDateString() + "<br/>" +
							   "Дата предыдущей редакции: " + Parent.Date.ToShortDateString());
		}

		/// <summary>
		/// Проверка заполнения полей КБК по бланку.
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка заполнения полей КБК по бланку.", InitialUNK = "0706")]
		public void Control_0706(DataContext context)
		{
			//Проверить сметные строки в ТЧ «Предельные объемы бюджетных ассигнований» 
			// на корректность заполнения кодов БК в соответствии с бланком «Доведение» (см. Примечание).
			//Если обнаружено несоответствие,  то действие не выполнять и выдать сообщение: 
			var errors = new List<String>();

			if (ExpensesList.Any(expense => !BlankFormationKU.CheckByBlank(expense)))
				errors.Add("- Расходы <br/>");

			if (SBP.IsFounder)
			{
				if (FounderExpensesList.Any(fe => !BlankFormationKU.CheckByBlank(fe)))
					errors.Add("- Расходы учредителя по мероприятиям АУ/БУ <br/>");
			}

			if (errors.Any())
				Controls.Throw(String.Format("Обязательность заполнения полей КБК определена бланком «{0}». Не заполнены поля в следующих таблицах:<br/> {1}", BlankFormationKU.BlankType.Caption(), errors.GetString()));
		}

		/// <summary>
		/// Проверка актуальности СБП, кодов КБК.
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка актуальности СБП, кодов КБК.", InitialUNK = "0707")]
		public void Control_0707(DataContext context)
		{
			//Проверка на актульность СБП
			if (((SBP.ValidityFrom.HasValue && SBP.ValidityFrom.Value > Date) || (SBP.ValidityTo.HasValue && SBP.ValidityTo.Value <= Date)))
				Controls.Throw("В поле Учреждение указан СБП, не действующий на дату документа.");

			//Мероприятие(Контингент) : Список ошибок
			var errors = this.GetWrongVersioningKBK(BlankFormationKU, typeof (PublicInstitutionEstimate_Expense), "PublicInstitutionEstimate_Activity", context, " - Таблица «Расходы» - ");

			/*foreach (var expense in ExpensesList)
			{
				var error = expense.CheckVersioningKbk(Date);
				if (error != null)
				{
					var key = expense.Master.Activity.Caption;

					if (expense.Master.Contingent != null)
						key += "(" + expense.Master.Contingent.Caption + ")";

					var value = "-	Таблица «Расходы» - " + error + "<br/>";
					if (errors.ContainsKey(key))
						errors[key].Add(value);
					else
						errors.Add(key, new List<string> { value });
				}
			}*/

			if (SBP.IsFounder)
			{
				var founderErrors = this.GetWrongVersioningKBK(BlankFormationKU, typeof(PublicInstitutionEstimate_FounderAUBUExpense), "PublicInstitutionEstimate_ActivityAUBU", context, " - Таблица «Расходы учредителя по деятельности АУ/БУ» - ");

				foreach (var founderError in founderErrors)
				{
					if (errors.ContainsKey(founderError.Key))
						errors[founderError.Key] = errors[founderError.Key].Union(founderError.Value).ToList();
					else
						errors.Add(founderError.Key,  founderError.Value );    
				}
			}

			/*    foreach (var expense in FounderExpensesList)
				{
					var error = expense.CheckVersioningKbk(Date);
					if (error != null)
					{
						var key = expense.Master.Activity.Caption;
						if (expense.Master.Contingent != null)
							key += "(" + expense.Master.Contingent.Caption + ")";

						var value = "-	Таблица «Расходы учредителя по деятельности АУ/БУ» - " + error + "<br/>";

						if (errors.ContainsKey(key))
							errors[key].Add(value);
						else
							errors.Add(key, new List<string> { value });
					}
				}*/

			if (errors.Any())
			{
				var errorMsg = new StringBuilder("В таблицах документа указаны строки с недействующими КБК (выделены жирным шрифтом):<br/>");

				foreach (var error in errors)
				{
					errorMsg.Append("<br/>" + error.Key );

					foreach (var value in error.Value)
						errorMsg.Append("<br/>" + value);
				}

				Controls.Throw(errorMsg.ToString());
			}
		}


		/// <summary>
		///Проверка наличия хотя бы одного мероприятия в документе
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка наличия хотя бы одного мероприятия в документе", InitialUNK = "0708")]
		public void Control_0708(DataContext context)
		{
			if (!context.PublicInstitutionEstimate_Activity.FastAny(a=> a.IdOwner==Id))
				Controls.Throw("Необходимо в таблицу «Мероприятия» добавить хотя бы одну строку");
		}

		/// <summary>
		/// Проверка строк с отрицательными значениями по суммовым полям
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка строк с отрицательными значениями по суммовым полям", InitialUNK = "0709")]
		public void Control_0709(DataContext context)
		{
			var limitErrors = context.PublicInstitutionEstimate_Expense
								 .Where(l => l.IdOwner == Id).ToList()
								 .Select(l => new
								 {
									 activity = l.Master.Activity.Caption,
									 hasError = l.HasNonPositiveSum(context)
								 })
								 .Where(l => l.hasError).ToList().Select(l => String.Format("- Таблица «Расходы» Мероприятие «{0}»", l.activity)).ToList();

			limitErrors = limitErrors.Union(context.PublicInstitutionEstimate_IndirectExpenses
							   .Where(l => l.IdOwner == Id).ToList()
							   .Select(l => new
							   {
								   method = l.Master.IndirectCostsDistributionMethod.Caption(),
								   hasError = l.HasNonPositiveSum(context)
							   })
							   .Where(l => l.hasError).ToList().Select(l => String.Format("- Таблица «Косвенные расходы» Метод распределения «{0}»", l.method))).ToList();


			if (SBP.IsFounder)
				limitErrors = limitErrors.Union(context.PublicInstitutionEstimate_FounderAUBUExpense
								   .Where(l => l.IdOwner == Id).ToList()
								   .Select(l => new
								   {
									   activity = l.Master.Activity.Caption,
									   hasError = l.HasNonPositiveSum(context)
								   })
								   .Where(l => l.hasError).ToList().Select(l => String.Format("- Таблица «Расходы учредителя по деятельности АУ/БУ» Мероприятие «{0}»", l.activity))).ToList();

			if (limitErrors.Any())
			{
				var msg = new StringBuilder();
				foreach (var limitError in limitErrors)
					msg.AppendFormat(" - {0}<br/>", limitError);

				Controls.Throw(
					String.Format("В таблице «Предельные объемы бюджетных ассигнований» указаны строки с нулевыми суммами:<br/>{0}", msg));
			}
		}

		/// <summary>
		/// Проверка наличия в плане деятельности запланированных объемов мероприятий по годам планирования расходов по собственной деятельности
		/// </summary>
		/// <param name="context"></param>
		/// <param name="idExpense"></param>
		[ControlInitial(InitialCaption = "Проверка наличия в плане деятельности запланированных объемов мероприятий по годам планирования расходов по собственной деятельности", InitialUNK = "0710")]
		public void Control_0710(DataContext context, int? idExpense = null)
		{
			var budgetYear = Budget.Year;
			var errors = new List<int>();
            var erorsview = new Dictionary<int, string>();

			var checkExpenses = idExpense.HasValue ? Expenses.Where(e => e.Id == idExpense).ToList() : ExpensesList;

			for (int i = 0; i < 3; i++)
			{
				var idHierarchyPeriod = (budgetYear + i).GetIdHierarchyPeriodYear(context);
				// ReSharper disable LoopCanBeConvertedToQuery
				foreach (var expense in checkExpenses)
				// ReSharper restore LoopCanBeConvertedToQuery
				{
					var value = (i == 0) ? expense.OFG : (i == 1 ? expense.PFG1 : expense.PFG2);

					if (value.HasValue && value.Value != 0)
					{
						if (!context.TaskVolume.Any(t => t.IdVersion == IdVersion &&
														 !t.IdTerminator.HasValue &&
														 t.IdSBP == IdSBP &&
														 t.TaskCollection.IdActivity == expense.Master.IdActivity &&
														 t.TaskCollection.IdContingent == expense.Master.IdContingent &&
														 t.IdValueType == (byte)ValueType.Plan &&
														 (!t.ActivityAUBU.HasValue || !t.ActivityAUBU.Value) &&
														 t.IdHierarchyPeriod == idHierarchyPeriod))
						{
							errors.Add(budgetYear + i);
                            erorsview.Add(budgetYear + i, expense.Master.Activity.Caption + (expense.Master.Contingent == null ? "" : ", " + expense.Master.Contingent.Caption));
                            //break;
						}

					}

				}
			}

            if (erorsview.Any())
            {
                var msg =
                    new StringBuilder(
                        "По следующим годам не могут быть указаны суммы расходов, так как по ним не планировался объем мероприятия в документе «План деятельности»:<br/>");

                foreach (var erorview in erorsview.Distinct())
                    msg.Append(erorview.Key.ToString() + ", " + erorview.Value + "<br/>");

                Controls.Throw(msg.ToString());
            }

            //if (errors.Any())
            //{
            //    var msg =
            //        new StringBuilder(
            //            "По следующим годам не могут быть указаны суммы расходов, так как по ним не планировался объем данного мероприятия в документе «План деятельности»:<br/>");

            //    foreach (var error in errors.Distinct().ToList())
            //        msg.Append(error + "<br/>");

            //    Controls.Throw(msg.ToString());
            //}
		}

		/// <summary>
		/// Проверка на заполнение поля «Территория» для межбюджетных расходов
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка на заполнение поля «Территория» для межбюджетных расходов", InitialUNK = "0711")]
		public void Control_0711(DataContext context)
		{
			if (Expenses.Any(e => e.KOSGU != null && e.KOSGU.Code == "251" && !e.IdOKATO.HasValue))
				Controls.Throw("Для межбюджетных расходов (КОСГУ=251) необходимо указать Территорию.");
		}

		/// <summary>
		/// Проверка заполнения поля «Территория» для косвенных расходов
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка заполнения поля «Территория» для косвенных расходов", InitialUNK = "0712")]
		public void Control_0712(DataContext context)
		{
			if (Expenses.Any(e => e.IdOKATO.HasValue && e.IsIndirectCosts.HasValue && e.IsIndirectCosts.Value))
				Controls.Throw("Поле «Территория» у косвенного расхода должно быть пустым.");
		}

		/// <summary>
		///Проверка наличия хотя бы одной строки расхода в документе
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка наличия хотя бы одной строки расхода в документе", InitialUNK = "0713")]
		public void Control_0713(DataContext context)
		{
			var errors = new List<String>();

			if (!context.PublicInstitutionEstimate_Expense.FastAny(a => a.IdOwner == Id))
				errors.Add("- Расходы <br/>");

			if (SBP.IsFounder && !context.PublicInstitutionEstimate_FounderAUBUExpense.FastAny(a=> a.IdOwner==Id))
				errors.Add("- Расходы учредителя по деятельности АУ/БУ <br/>");

			if (errors.Any())
				Controls.Throw("Необходимо добавить хотя бы одну строку расхода в таблицах документа:<br/>" + errors.GetString());
		}

		/// <summary>
		/// Проверка на непревышение предварительных объемов ассигнований по собственной деятельности
		/// </summary>
		[ControlInitial(InitialCaption = "Проверка на непревышение предварительных объемов ассигнований по собственной деятельности", InitialUNK = "0714")]
		public void Control_0714(DataContext context)
		{
			var properties = BlankBringingKU.GetBlankCostMandatoryProperties();

			var query = new StringBuilder(@"Select Top 1 1 as Id 
											From reg.LimitVolumeAppropriations L 
											Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine 
											Where L.idVersion = " + IdVersion + @" And 
													L.idBudget = " + IdBudget + @" And 
													L.idPublicLegalFormation = " + IdPublicLegalFormation + @" And 
													L.idValueType = " + ((byte) ValueType.Plan).ToString() + @" And 
													EL.idSBP = " + IdSBP + @" 
											Group By ");
			query.Append(string.Join(", ", properties.Select(p => "EL." + p)));
			query.Append(@" Having Sum(L.Value) <> 0 And ( ");
			query.Append(string.Join(" Or ", properties.Select(p => "El." + p + " Is Null ") ));
			query.Append(" )");

			if ( context.Database.SqlQuery<int>( query.ToString() ).Any() )
				Controls.Throw("В документе «План деятельности» по учреждению «" + SBP.Caption + "» заполнение обязательных полей КБК не соответствует бланку «Доведение КУ»");
			
		   

			Control_0714_0721(context);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="withDate">Дата записей из регистра &lt;= ШапкаДокумента.Дата </param>
		/// <param name="additionalMsg">Дополнительный текст сообщения контроля</param>
		private void Control_0714_0721(DataContext context, bool withDate = false, string additionalMsg = null)
		{
			//Если в бланке доведения КУ вышестоящего СБП КОСГУ указан как обязательный, 
			//то отобрать из регистра «Объемы финансовых средств» КОСГУ из строк с типом значения «План» 
			//(в разрезе годов планирования с учетом ППО, Бюджет, Версия) с СБП= вышестоящий СБП для СБП документа.
			
			var idKosgu000 = BlankBringingKU.BlankValueType_KOSGU == BlankValueType.Mandatory ?
								(int?)context.KOSGU.Where(k => k.Code == "000").Select(k => k.Id).FirstOrDefault() : null;
			if (idKosgu000 == 0 || (idKosgu000.HasValue &&
				!context.LimitVolumeAppropriations.Any(l => l.IdPublicLegalFormation == IdPublicLegalFormation && l.IdBudget == IdBudget && l.IdVersion == IdVersion)))
				idKosgu000 = null;

			var limitErrors = CheckLimitAllocations0714(context, new[] { BlankBringingKU, BlankFormationKU }, withDate, idKosgu000).ToList();

			if (limitErrors.Any())
			{
				var msg = "Превышение обнаружено по строкам: <br/>" + String.Join("<br/>", limitErrors);

				var commonPartMsg = new StringBuilder("Всего объем средств: ");
				var prevVersionIds = PrevVersionIds;

				/*var costLineProperties = SBP_BlankHelper.GetBlanksCostMandatoryProperties(new []{parentSBPBlankBringingKU, parentSBPBlankFormationKU} ).ToList();

				var planValuesQuery = context.LimitVolumeAppropriations.Where(l => l.EstimatedLine.IdSBP == IdSBP &&
																			  (l.IdRegistratorEntity != Id || !prevVersionIds.Contains(l.IdRegistrator)) &&
																			  l.IdValueType == (byte)ValueType.Plan &&
																			  l.IdPublicLegalFormation == IdPublicLegalFormation &&
																			  l.IdBudget == IdBudget &&
																			  l.IdVersion == IdVersion &&
																			  !l.IsMeansAUBU);

				foreach (var costLineProperty in costLineProperties)
					planValuesQuery = planValuesQuery.Where("EstimatedLine." + costLineProperty + ".HasValue");

				var planValues = planValuesQuery
										.GroupBy(l => l.IdHierarchyPeriod)
										.Select(g => new { g.Key, Value = g.Sum(l => l.Value) })
										.ToList();*/

				var planValues = context.LimitVolumeAppropriations.Where(l => l.EstimatedLine.IdSBP == IdSBP &&
																			(l.IdRegistratorEntity != Id || !prevVersionIds.Contains(l.IdRegistrator)) &&
																			l.IdValueType == (byte)ValueType.Plan &&
																			l.IdPublicLegalFormation == IdPublicLegalFormation &&
																			l.IdBudget == IdBudget &&
																			l.IdVersion == IdVersion &&
																			!l.IsMeansAUBU)
									  .GroupBy(l => l.IdHierarchyPeriod)
									  .Select(g => new { g.Key, Value = g.Sum(l => l.Value) })
									  .ToList();

				var docValues = Expenses.GroupBy(s => true).Select(g => new
				{
					valueFirstYear = g.Sum(s => s.OFG),
					valueSecondYear = g.Sum(s => s.PFG1),
					valueThirdYear = g.Sum(s => s.PFG2)
				}).FirstOrDefault();

				for (int year = Budget.Year; year <= Budget.Year + 2; year++)
				{
					var yearPlanValue = planValues.Where(v => v.Key == year.GetIdHierarchyPeriodYear(context)).Select(v => v.Value).FirstOrDefault();
					commonPartMsg.AppendFormat("{0} г. = {1}; ", year, yearPlanValue);
				}

				if (docValues == null)
					throw new PlatformException("В ТЧ Расходы не введено не одного значения!");

				commonPartMsg.Append("<br/>Всего обоснованных ассигнований: ");
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year, docValues.valueFirstYear);
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year + 1, docValues.valueSecondYear);
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year + 2, docValues.valueThirdYear);

				Controls.Throw(
					String.Format("{2}Объем обоснованных ассигнований по собственной деятельности превышает предельный объем бюджетных ассигнований по данному учреждению. <br/> {0} <br/> {1}",
						commonPartMsg, msg, additionalMsg ?? ""));
			}
		}

		/// <summary>
		/// Проверка на несоответствие общего набора задач учредителя в ТЧ «Мероприятие АУ/БУ» и подведомственных учреждений 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="isCreate"></param>
		[ControlInitial(InitialCaption = "Проверка на несоответствие общего набора задач учредителя в ТЧ «Мероприятие АУ/БУ» и подведомственных учреждений ", InitialUNK = "0715", InitialManaged = true, InitialSkippable = true)]
		public void Control_0715(DataContext context, bool isCreate = true)
		{
			var regValues = GetExpensesForAloneSubjects(context).ToList();

			//var docTaskCollections = isCreate ? ActivitiesAUBU.Select(a => new {a.IdActivity, a.IdContingent, a.Id }).ToList()
			//    : AloneAndBudgetInstitutionExpenses.Select(a => new {a.Master.IdActivity, a.Master.IdContingent, a.Id }).ToList();

			var docTaskCollections = ActivitiesAUBU.Select(a => new { a.IdActivity, a.IdContingent, a.Id }).ToList();

			if (regValues.Any(r => !docTaskCollections.Any(c => c.IdActivity == r.IdActivity && c.IdContingent == r.IdContingent)))
			{
				var errors = new List<String>();
				// ReSharper disable LoopCanBeConvertedToQuery
				foreach (var value in regValues.Where(r => !docTaskCollections.Any(c => c.IdActivity == r.IdActivity && c.IdContingent == r.IdContingent)))
				// ReSharper restore LoopCanBeConvertedToQuery
				{
					var activity = context.Activity.Where(a => a.Id == value.IdActivity).Select(a => a.Caption).FirstOrDefault();
					var contingent = context.Contingent.Where(c => c.Id == value.IdContingent).Select(c => c.Caption).FirstOrDefault();

					errors.Add(activity + " - " + contingent);
				}

				var msg = new StringBuilder();
				msg.Append("В таблицу «Мероприятия АУ/БУ» не добавлены следующие мероприятия:<br/>");

				foreach (var error in errors.Distinct())
				{
					msg.Append(error + "<br/>");
				}

				Controls.Throw(msg.ToString());

				var year = Budget.Year;

				foreach (var value in regValues.Where(r => !docTaskCollections.Any(c => c.IdActivity == r.IdActivity && c.IdContingent == r.IdContingent)))
				{

					var activity = ActivitiesAUBU.FirstOrDefault(a => a.IdActivity == value.IdActivity &&
																	  a.IdContingent == value.IdContingent);
					if (activity == null)
					{
						activity = new PublicInstitutionEstimate_ActivityAUBU
						{
							Owner = this,
							IdActivity = value.IdActivity,
							IdContingent = value.IdContingent
						};
						context.PublicInstitutionEstimate_ActivityAUBU.Add(activity);
					}


					var expense = PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense
														.GetQueryByKBK(AloneAndBudgetInstitutionExpenses.AsQueryable(), value)
														.FirstOrDefault(e => e.Master.IdActivity == value.IdActivity &&
																		((!e.Master.IdContingent.HasValue && !value.IdContingent.HasValue)
																			|| (e.Master.IdContingent == value.IdContingent)));

					if (expense == null)
					{
						expense = new PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense(value)
						{
							Owner = this,
							Master = activity,

							OFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : null,
							PFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : null,
							PFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : null,
							AdditionalOFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : null,
							AdditionalPFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : null,
							AdditionalPFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : null,
						};
						context.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense.Add(expense);
					}
					else
					{
						expense.OFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.OFG;
						expense.PFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.PFG1;
						expense.PFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.PFG2;
						expense.AdditionalOFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalOFG;
						expense.AdditionalPFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalPFG1;
						expense.AdditionalPFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalPFG2;
					}

				}

				context.SaveChanges();

			}
		}

		/// <summary>
		/// Проверка наличия в плане деятельности запланированных объемов мероприятий по годам планирования расходов по деятельности АУ/БУ
		/// </summary>
		/// <param name="context"></param>
		/// <param name="element"></param>
		[ControlInitial(InitialCaption = "Проверка наличия в плане деятельности запланированных объемов мероприятий по годам планирования расходов по деятельности АУ/БУ", InitialUNK = "0716")]
		public void Control_0716(DataContext context, PublicInstitutionEstimate_FounderAUBUExpense element)
		{
			var budgetYear = Budget.Year;

			var errors = new List<int>();

			if (element != null)
			{
				var years = new Dictionary<int, int>()
					{
					  {budgetYear.GetIdHierarchyPeriodYear(context), budgetYear},  
					  {(budgetYear+1).GetIdHierarchyPeriodYear(context), (budgetYear+1)},
					  {(budgetYear+2).GetIdHierarchyPeriodYear(context), (budgetYear+2)}  
					};
				var hierarchyPeriods = years.Select(y => y.Key).ToArray();

				var yearIds = context.TaskVolume.Where(t => t.IdVersion == IdVersion &&
													   t.IdPublicLegalFormation == IdPublicLegalFormation &&
													   !t.IdTerminator.HasValue &&
													   t.IdSBP == IdSBP &&
													   t.TaskCollection.IdActivity == element.Master.IdActivity &&
													   t.TaskCollection.IdContingent == element.Master.IdContingent &&
													   t.IdValueType == (byte) ValueType.Plan &&
													   (t.ActivityAUBU.HasValue && t.ActivityAUBU.Value) &&
													   hierarchyPeriods.Contains(t.IdHierarchyPeriod) )
										   .GroupBy(t => t.IdHierarchyPeriod)
										   .Select(g => g.Key)
										   .ToArray();

				if ((element.OFG ?? 0) > 0 && ( !yearIds.Contains(budgetYear.GetIdHierarchyPeriodYear(context))) )
					errors.Add(budgetYear);

				if ((element.PFG1 ?? 0) > 0 && (!yearIds.Contains((budgetYear+1).GetIdHierarchyPeriodYear(context))))
					errors.Add(budgetYear + 1 );

				if ((element.PFG2 ?? 0) > 0 && (!yearIds.Contains((budgetYear + 2).GetIdHierarchyPeriodYear(context))))
					errors.Add(budgetYear + 2);    
					
				//argh
				//errors = years.Where(y => !yearIds.Contains(y.Key)).Select(year => year.Value).ToList();
			}
			else
				for (int i = 0; i < 3; i++)
				{
					var idHierarchyPeriod = (budgetYear + i).GetIdHierarchyPeriodYear(context);

				
					// ReSharper disable LoopCanBeConvertedToQuery
					foreach (var expense in FounderAUBUExpenses)
						// ReSharper restore LoopCanBeConvertedToQuery
					{
						var value = (i == 0) ? expense.OFG : (i == 1 ? expense.PFG1 : expense.PFG2);

						if (value.HasValue && value.Value != 0)
						{
							if (!context.TaskVolume.Any(t => t.IdVersion == IdVersion &&
																t.IdPublicLegalFormation == IdPublicLegalFormation &&
																!t.IdTerminator.HasValue &&
																t.IdSBP == IdSBP &&
																t.TaskCollection.IdActivity == expense.Master.IdActivity &&
																t.TaskCollection.IdContingent ==
																expense.Master.IdContingent &&
																t.IdValueType == (byte) ValueType.Plan &&
																(t.ActivityAUBU.HasValue && t.ActivityAUBU.Value) &&
																t.IdHierarchyPeriod == idHierarchyPeriod))
							{
								errors.Add(budgetYear + i);
								break;
							}
						}

					}
				}

			if (errors.Any())
			{
				var msg = new StringBuilder(
						"По следующим годам не могут быть указаны суммы расходов, так как по ним не планировался объем данного мероприятия в документе «План деятельности»:<br/>");

				foreach (var error in errors)
					msg.Append(error + "<br/>");

				Controls.Throw(msg.ToString());
			}
		}

		/// <summary>
		/// Проверка на не превышение предельных объемов ассигнований по деятельности АУ/БУ
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований по деятельности АУ/БУ", InitialUNK = "0717")]
		public void Control_0717(DataContext context)
		{
			Control_0717_0722(context);
		}

		private void Control_0717_0722(DataContext context, bool withDate = false, string additionalMsg = null)
		{
			if (SBP == null || !SBP.IdParent.HasValue)
				throw new PlatformException("У СБП типа казенное учреждение отсутствует вышестоящее учреждение");

			var parentSBP = SBP.Parent;
			if (parentSBP == null)
				throw new PlatformException(String.Format("СБП с Id = {0} не найден в БД", SBP.IdParent));

			//Если в бланке доведения КУ вышестоящего СБП КОСГУ указан как обязательный, 
			//то отобрать из регистра «Объемы финансовых средств» КОСГУ из строк с типом значения «План» 
			//(в разрезе годов планирования с учетом ППО, Бюджет, Версия) с СБП= вышестоящий СБП для СБП документа.
			var parentSBPBlankBringingKU = parentSBP.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.BlankType == BlankType.BringingKU);
			var parentSBPBlankFormationKU = parentSBP.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.BlankType == BlankType.FormationKU);

			var prevVersionIds = PrevVersionIds;

			var limitErrors = CheckLimitAllocations0717(context, new[] { parentSBPBlankBringingKU, parentSBPBlankFormationKU }, withDate).ToList();
			if (limitErrors.Any())
			{
				var msg = "Превышение обнаружено по строкам: <br/>" + String.Join("<br/>", limitErrors);

				var commonPartMsg = new StringBuilder("Всего объем средств: ");
				var planValues = context.LimitVolumeAppropriations.Where(l => l.EstimatedLine.IdSBP == IdSBP &&
																			  (l.IdRegistratorEntity != Id || !prevVersionIds.Contains(l.IdRegistrator)) &&
																			  l.IdValueType == (byte)ValueType.Plan &&
																			  l.IdPublicLegalFormation == IdPublicLegalFormation &&
																			  l.IdBudget == IdBudget &&
																			  l.IdVersion == IdVersion &&
																			  !l.IsMeansAUBU)
										.GroupBy(l => l.IdHierarchyPeriod)
										.Select(g => new { g.Key, Value = g.Sum(l => l.Value) })
										.ToList();
				var docValues = FounderAUBUExpenses.GroupBy(s => true).Select(g => new
				{
					valueFirstYear = g.Sum(s => s.OFG),
					valueSecondYear = g.Sum(s => s.PFG1),
					valueThirdYear = g.Sum(s => s.PFG2)
				}).FirstOrDefault();

				for (int year = Budget.Year; year <= Budget.Year + 2; year++)
				{
					var yearPlanValue = planValues.Where(v => v.Key == year.GetIdHierarchyPeriodYear(context)).Select(v => v.Value).FirstOrDefault();
					commonPartMsg.AppendFormat("{0} г. = {1}; ", year, yearPlanValue);
				}

				if (docValues == null)
					throw new PlatformException("В ТЧ Расходы не введено не одного значения!");

				commonPartMsg.Append("<br/>Всего обоснованных ассигнований: ");
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year, docValues.valueFirstYear);
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year + 1, docValues.valueSecondYear);
				commonPartMsg.AppendFormat("{0} г. = {1}; <br/>", Budget.Year + 2, docValues.valueThirdYear);

				Controls.Throw(
					String.Format("{2}Объем обоснованных ассигнований по деятельности АУ/БУ превышает предельный объем бюджетных ассигнований по данному учреждению. <br/> {0} {1}",
						commonPartMsg, msg, additionalMsg ?? String.Empty));
			}
		}

		/// <summary>
		/// Проверка строк в ТЧ Расходы с незаполненными суммами
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка строк в ТЧ Расходы с незаполненными суммами", InitialUNK = "0718")]
		public void Control_0718(DataContext context)
		{
			var limitErrors = context.PublicInstitutionEstimate_Expense
								   .Where(l => l.IdOwner == Id).ToList()
								   .Select(l => new { activity = l.Master.Activity.Caption, contingent = l.Master.Contingent != null ? l.Master.Contingent.Caption : "", msg = l.CheckNotNullSum(context) })
								   .Where(l => !String.IsNullOrEmpty(l.msg)).ToList()
								   .Select(l => String.Format("- Таблица «Расходы» Мероприятие «{0}» Контингент «{2}» {1}", l.activity, l.msg, l.contingent)).ToList();

			if (SBP.IsFounder)
				limitErrors = limitErrors.Union(context.PublicInstitutionEstimate_FounderAUBUExpense
								   .Where(l => l.IdOwner == Id).ToList()
								   .Select(l => new { activity = l.Master.Activity.Caption, contingent = l.Master.Contingent != null ? l.Master.Contingent.Caption : "", msg = l.CheckNotNullSum(context) })
								   .Where(l => !String.IsNullOrEmpty(l.msg)).ToList().Select(l => String.Format("- Таблица «Расходы учредителя по деятельности АУ/БУ» Мероприятие «{0}» Контингент «{2}» {1}", l.activity, l.msg, l.contingent))).ToList();

			if (limitErrors.Any())
			{
				var msg = new StringBuilder();
				foreach (var limitError in limitErrors)
					msg.AppendFormat(" - {0}<br/>", limitError);

				Controls.Throw(
					String.Format("В документе введены строки расходов с незаполненными суммами. Для каждой строки необходимо указать хотя бы одну сумму.:<br/>{0}", msg));
			}
		}

		/// <summary>
		/// Проверка утверждения мероприятий по собственной деятельности
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка утверждения мероприятий по собственной деятельности", InitialUNK = "0719")]
		public void Control_0719(DataContext context)
		{
			const string sMsg = "Скорректируйте перечень мероприятий по собственной деятельности. <br>" +
								"Планом деятельности учреждения предусмотрено выполнение следующих мероприятий, отсутствующих в документе:<br>";

			var sMsgS = "Проверьте утверждение плана деятельности. <br>" +
						"План деятельности учреждения должен быть утвержден, и его дата должна быть не больше даты Сметы казенного учреждения. <br>" +
						"При необходимости, скорректируйте дату Сметы казенного учреждения. <br>" +
						"На дату " + Date.ToShortDateString() + " планом деятельности не утверждены мероприятия:<br>";


			//GetDataDocTables(context);

			var reg = context.TaskVolume.Where(r => r.IdPublicLegalFormation == IdPublicLegalFormation &&
														r.IdBudget == IdBudget &&
														r.IdVersion == IdVersion &&
														r.IdSBP == IdSBP &&
														r.ActivityAUBU == false &&
														r.DateCommit <= Date)
										.Select(r => r.TaskCollection.Activity.Caption + (r.TaskCollection.Contingent != null ? "-" + r.TaskCollection.Contingent.Caption : String.Empty))
										.Distinct().ToList();

			var tp = context.PublicInstitutionEstimate_Activity.Where(r => r.IdOwner == Id)
										.Select(r => r.Activity.Caption + (r.Contingent != null ? "-" + r.Contingent.Caption : String.Empty))
										.Distinct().ToList();

			var differenceQueryMsgF = reg.Except(tp).ToList();//если не пусто то сообщение о неучтенных в документе мероприятиях 
			var differenceQueryMsgS = tp.Except(reg).ToList();//если не пусто то сообщение о лишних мероприятиях

			if (differenceQueryMsgF.Any())
			{
				var result = String.Join(" ", differenceQueryMsgF.Select(item => (" - " + item + ";<br/>")).ToList());
				Controls.Throw(sMsg + result);
			}

			if (differenceQueryMsgS.Any())
			{
				var result = String.Join(" ", differenceQueryMsgS.Select(item => (" - " + item + ";<br/>")).ToList());
				Controls.Throw(sMsgS + result);
			}

		}


		/// <summary>
		/// Проверка утверждения мероприятий по деятельности АУ/БУ
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка утверждения мероприятий по деятельности АУ/БУ", InitialUNK = "0720")]
		public void Control_0720(DataContext context)
		{
			if (!SBP.IsFounder)
				return;

			const string sMsg = "Скорректируйте перечень мероприятий по деятельности АУ/БУ. <br>" +
					   "Планом деятельности учреждения предусмотрено выполнение следующих мероприятий, отсутствующих в документе:<br>";

			var sMsgS = "Проверьте утверждение плана деятельности. <br>" +
						"План деятельности учреждения должен быть утвержден, и его дата должна быть не больше даты Сметы казенного учреждения. <br>" +
						"При необходимости, скорректируйте дату Сметы казенного учреждения. <br>" +
						"На дату " + Date.ToShortDateString() + " планом деятельности не утверждены мероприятия:<br>";

			var reg = context.TaskVolume.Where(r => r.IdPublicLegalFormation == IdPublicLegalFormation &&
													r.IdBudget == IdBudget &&
													r.IdVersion == IdVersion &&
													r.IdSBP == IdSBP &&
													(r.ActivityAUBU.HasValue && r.ActivityAUBU.Value) &&
													r.DateCommit <= Date)
							 .Select(t => new { t.IdTaskCollection, t.TaskCollection.Activity, t.TaskCollection.Contingent })
							 .ToList()
							 .DistinctBy(s => s.IdTaskCollection).Select(s => new { s.Activity, s.Contingent })
							 .ToList();

			var tp = ActivitiesAUBU.Select(a => new { a.Activity, a.Contingent }).ToList();

			var tplist = tp.Select(t => t.Activity.Caption + "-" + (t.Contingent != null ? t.Contingent.Caption : String.Empty)).ToList();//new List<string>();
			var reglist = reg.Select(t => t.Activity.Caption + "-" + (t.Contingent != null ? t.Contingent.Caption : String.Empty)).ToList();

			var differenceQueryMsgF = reglist.Except(tplist).ToList();//если не пусто то сообщение1 
			var differenceQueryMsgS = tplist.Except(reglist).ToList();//если не пусто то сообщение2

			if (differenceQueryMsgF.Any())
			{
				string result = String.Join(" ", differenceQueryMsgF.Select(item => " - " + (item + ";<br/>")));
				Controls.Throw(sMsg + result);
			}

			if (differenceQueryMsgS.Any())
			{
				string result = String.Join(" ", differenceQueryMsgS.Select(item => " - " + (item + ";<br/>")));
				Controls.Throw(sMsgS + result);
			}

		}


		/// <summary>
		/// Проверка утверждения предварительных объемов ассигнований по собственной деятельности
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка утверждения предварительных объемов ассигнований по собственной деятельности", InitialUNK = "0721")]
		public void Control_0721(DataContext context)
		{
			Control_0714_0721(context, true, "Возможно, требуется корректировка даты утверждения текущего документа или утверждение средств в документах «План деятельности».<br/>" +
										"На дату " + Date.ToShortDateString() + " возникают несоответствия.<br/>");
		}

		/// <summary>
		/// Проверка утверждения предварительных объемов ассигнований по деятельности АУ/БУ
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка утверждения предварительных объемов ассигнований по деятельности АУ/БУ", InitialUNK = "0722")]
		public void Control_0722(DataContext context)
		{
			if (!SBP.IsFounder)
				return;

			Control_0717_0722(context, true, "Возможно, требуется корректировка даты утверждения текущего документа или утверждение средств в документах «План деятельности».<br/>" +
											 "На дату " + Date.ToShortDateString() + " возникают несоответствия.<br/>");
		}


		//ExecuteControl<CommonControlAddNeed_0242>(); это контроль 0724
		//контроль 723 это 1936 строчка LongTermGoalProgram.cs
		/// <summary>
		/// Очистка доп. потребности
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Очистка доп. потребности", InitialUNK = "0723", InitialSkippable = true)]
		public void Control_0723(DataContext context)
		{
			if (HasAdditionalNeed.HasValue && HasAdditionalNeed.Value) 
				return;
			
			var oldDocument = OperationContext.Current.OriginalTarget as PublicInstitutionEstimate;

			if (oldDocument == null)
				throw new PlatformException("Не удалось получить оригинальные парметры документа через OperationContext.Current.OriginalTarget");

			if (HasAdditionalNeed != oldDocument.HasAdditionalNeed)
			{
				Controls.Throw("Признак «Вести доп. потребности» отключен. Все доп. потребности в документе будут очищены. Продолжить?");

				Expenses.AsQueryable()
						.Update(e => e.AdditionalOFG.HasValue || e.AdditionalPFG1.HasValue || e.AdditionalPFG2.HasValue,
								u => new PublicInstitutionEstimate_Expense
									{
										AdditionalOFG = null,
										AdditionalPFG1 = null,
										AdditionalPFG2 = null
									});

				FounderAUBUExpenses.AsQueryable()
						.Update(e => e.AdditionalOFG.HasValue || e.AdditionalPFG1.HasValue || e.AdditionalPFG2.HasValue,
								u => new PublicInstitutionEstimate_FounderAUBUExpense
								{
									AdditionalOFG = null,
									AdditionalPFG1 = null,
									AdditionalPFG2 = null
								});
				
/*                foreach ( var expense in Expenses.Where(e => e.AdditionalOFG.HasValue || e.AdditionalPFG1.HasValue || e.AdditionalPFG2.HasValue).ToList() )
				{
					expense.AdditionalOFG = null;
					expense.AdditionalPFG1 = null;
					expense.AdditionalPFG2 = null;
				}*/

				/*foreach ( var expense in FounderAUBUExpenses.Where(e => e.AdditionalOFG.HasValue || e.AdditionalPFG1.HasValue || e.AdditionalPFG2.HasValue) )
				{
					expense.AdditionalOFG = null;
					expense.AdditionalPFG1 = null;
					expense.AdditionalPFG2 = null;
				}*/
			}
		}


		/// <summary>
		/// Проверка признака «Вести доп.потребности»
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = ". Проверка признака «Вести доп.потребности»", InitialUNK = "0724", InitialSkippable = true)]
		public void Control_0724(DataContext context)
		{
			if (HasAdditionalNeed.HasValue && HasAdditionalNeed.Value)
				Controls.Throw("Документ ведется с доп. потребностями. " +
							   "Вы запустили операцию утверждения базовых значений. " +
							   "Будет создана и утверждена новая редакция документа с очищенными данными по доп. потребностям. " +
							   "Продолжить?");
		}

		/// <summary>
		///Проверка признака «Вести доп. потребности»
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка признака «Вести доп. потребности»", InitialUNK = "0725")]
		public void Control_0725(DataContext context)
		{
			if (!HasAdditionalNeed.HasValue || !HasAdditionalNeed.Value)
				Controls.Throw("В документе отсутствуют значения по доп. потребностям. Воспользуйтесь операцией «Утвердить».");
		}


		/// <summary>
		///Проверка признака «Вести доп. потребности»
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка признака «Вести доп. потребности»", InitialUNK = "0726", InitialSkippable = true)]
		public void Control_0726(DataContext context)
		{
			if (HasAdditionalNeed.HasValue && HasAdditionalNeed.Value)
				Controls.Throw("Будет создана и утверждена новая редакция документа – данные по доп. потребностям будут суммированы с базовыми значениями. " +
							   "Продолжить?");
		}

		///  <summary>
		/// Проверка уникальности сметной строки
		///  </summary>
		///  <param name="context"></param>
		/// <param name="expense"></param>
		[ControlInitial(InitialCaption = "Проверка уникальности сметной строки", InitialUNK = "0727")]
		public void Control_0727(DataContext context, PublicInstitutionEstimate_Expense expense = null)
		{
			var query = new StringBuilder();

			if (expense != null )
			{
				var idElement = expense.Id.ToString();
				query = new StringBuilder(@"Select top 1 E1.id From tp.PublicInstitutionEstimate_Expense E1 
											Where E1.id <> " + idElement + @" 
												And E1.idMaster = " + expense.IdMaster + @" 
												And ISNULL(E1.idIndirectCostsDistributionMethod, 0) = " + (expense.IdIndirectCostsDistributionMethod ?? 0) +
											  " And ISNULL(E1.idAuthorityOfExpenseObligation, 0) = " + (expense.IdAuthorityOfExpenseObligation ?? 0) + 
											  " And ISNULL(E1.IdOKATO, 0) = " + (expense.IdOKATO ?? 0) +
											  " And ISNULL(E1.isIndirectCosts, 0) = " + (expense.IsIndirectCosts == true ? 1 : 0) +
											  " ");
				
				foreach (var p in typeof(ILineCost).GetProperties())
				{
					var name = p.Name;
					var value = p.GetValue(expense);
					
					if (value != null)
						query.Append(" And E1." + name + " = " + value.ToString() + " ");
				}
			}
			else
			{
				const string commandText=
				@"SELECT [id] FROM (
						SELECT [id], 
							COUNT(1) OVER(PARTITION BY [idMaster],[idIndirectCostsDistributionMethod],[idAuthorityOfExpenseObligation],[isIndirectCosts],[IdOKATO]{0}) AS [count],
							ROW_NUMBER() OVER(PARTITION BY [idMaster],[idIndirectCostsDistributionMethod],[idAuthorityOfExpenseObligation],[isIndirectCosts],[IdOKATO]{0} ORDER BY [id]) AS [number]
						FROM [tp].[PublicInstitutionEstimate_Expense]
						WHERE [idOwner] = {1}) a
					WHERE [count]>1 AND [number]=1";

				string fields = typeof (ILineCost).GetProperties().Select(p => p.Name).Aggregate("", (current, name) => current + (", [" + name + "]"));
				query.AppendFormat(commandText, fields, Id);
			}
	
			var dublicatedIds = context.Database.SqlQuery<int>(query.ToString()).ToArray();
			if (dublicatedIds.Any())
			{
				var dublicated = ExpensesList.Where(e => dublicatedIds.Contains(e.Id)).ToList();
				var dublicatedMastersIds = dublicated.Select(e=>e.IdMaster).ToArray();
				var masters = context.PublicInstitutionEstimate_Activity.Where(a => dublicatedMastersIds.Contains(a.Id))
									 .Select( a => new{ a.Id, caption = ("-" + a.Activity.Caption + (a.IdContingent.HasValue ? "-" + a.Contingent.Caption : ""))}).ToList();

				var msg = new StringBuilder("В таблице «Расходы» указаны неуникальные строки: ");

				foreach (var m in masters)
				{
					msg.Append("<br/>" + m.caption);

					foreach (var dublicate in dublicated.Where(d=>d.IdMaster == m.Id))
					{
						msg.Append("<br/>&nbsp&nbsp" + dublicate.ToStringAsEstimatedLine(context) + 
							(dublicate.IdOKATO.HasValue ? ", Территория: " + context.OKATO.SingleOrDefault(s=> s.Id == dublicate.IdOKATO).Caption :"") +
							(dublicate.IdIndirectCostsDistributionMethod.HasValue ? ", Метод:  " + dublicate.IndirectCostsDistributionMethod.Caption() : "") +
							((dublicate.IsIndirectCosts.HasValue && dublicate.IsIndirectCosts==true) ? ", Косвенный расход " : "") 
							);
					}
				}

				Controls.Throw(msg.ToString());
			}
		}

		///  <summary>
		/// Проверка соответствия мероприятий в ТЧ и регистре
		///  </summary>
		/// <param name="deletingActivities">Список мероприятий, отсутствующих в регистре</param>
		/// <param name="hasIndirect"></param>
		[ControlInitial(InitialCaption = "Проверка соответствия мероприятий в ТЧ и регистре", InitialUNK = "0728", InitialSkippable = true, InitialManaged = false)]
		public void Control_0728(IEnumerable<String> deletingActivities, bool hasIndirect = false)
		{
			if (deletingActivities.Any())
			{
				var msg = hasIndirect ? "Следующие мероприятия отсутствуют в проектном документе «План деятельности» и будут удалены из таблицы «Мероприятия», а также из таблицы «Мероприятия для распределения», т.к. они участвовали в распределении косвенных расходов. Необходимо перераспределить косвенные расходы. <br/>"
									  : "Следующие мероприятия отсутствуют в проектном документе «План деятельности» и будут удалены из таблицы: <br/> ";

				Controls.Throw( msg + string.Join("<br/>", deletingActivities.ToList()) );
			}
		}

		//private void Control_0728_Activity(DataContext context)
		//{
		//    var year = Budget.Year;
		//    var taskVolumes = context.TaskVolume.Where(tv => tv.IdVersion == IdVersion
		//                                   && tv.IdBudget == IdBudget
		//                                   && tv.IdSBP == IdSBP
		//                                   && (tv.IdHierarchyPeriod == year.GetIdHierarchyPeriodYear(context) || 
		//                                        tv.IdHierarchyPeriod == (year + 1).GetIdHierarchyPeriodYear(context) ||
		//                                        tv.IdHierarchyPeriod == (year + 2).GetIdHierarchyPeriodYear(context))
		//                                    && tv.IdValueType == (byte)ValueType.Plan
		//                                    && (!tv.ActivityAUBU.HasValue || !tv.ActivityAUBU.Value)
		//                                    && !tv.IdTerminator.HasValue)
		//                        .GroupBy(tv=>new {tv.TaskCollection.IdActivity, tv.TaskCollection.IdContingent})
		//                        .Select(g => new {g.Key.IdActivity, g.Key.IdContingent});

		//    var nonExisted = Activities.AsQueryable()
		//                               .Where(a => !taskVolumes.Any(t => t.IdActivity == a.IdActivity && t.IdContingent == a.IdContingent))
		//                               .Select(a => new { a.Id, Caption = (a.Activity.Caption + (a.IdContingent.HasValue ? ("-" + a.Contingent.Caption) : ""))})
		//                               .ToList();

		//    if (nonExisted.Any())
		//    {
		//        Controls.Throw(
		//            "Следующие мероприятия отсутствуют в проектном документе «План деятельности» и будут удалены из таблицы:"
		//            + string.Join("<br/> - ", nonExisted.Select(a=>a.Caption).Distinct().ToList() ) +
		//            "Продолжить?");

		//        var activityIds = nonExisted.Select(n => n.Id).ToArray();
		//        context.PublicInstitutionEstimate_Activity.RemoveAll(Activities.Where(a => activityIds.Contains(a.Id)).ToList());
		//        context.SaveChanges();
		//    }

		//}

		//private void Control_0728_ActivityAUBU(DataContext context)
		//{
		//    var year = Budget.Year;
		//    var taskVolumes = context.TaskVolume.Where(tv => tv.IdVersion == IdVersion
		//                                   && tv.IdBudget == IdBudget
		//                                   && tv.IdSBP == IdSBP
		//                                   && (tv.IdHierarchyPeriod == year.GetIdHierarchyPeriodYear(context) ||
		//                                        tv.IdHierarchyPeriod == (year + 1).GetIdHierarchyPeriodYear(context) ||
		//                                        tv.IdHierarchyPeriod == (year + 2).GetIdHierarchyPeriodYear(context))
		//                                    && tv.IdValueType == (byte)ValueType.Plan
		//                                    && (tv.ActivityAUBU.HasValue && tv.ActivityAUBU.Value)
		//                                    && !tv.IdTerminator.HasValue)
		//                        .GroupBy(tv => new { tv.TaskCollection.IdActivity, tv.TaskCollection.IdContingent })
		//                        .Select(g => new { g.Key.IdActivity, g.Key.IdContingent });

		//    var nonExisted = ActivitiesAUBU.AsQueryable()
		//                               .Where(a => !taskVolumes.Any(t => t.IdActivity == a.IdActivity && t.IdContingent == a.IdContingent))
		//                               .Select(a => new { a.Id, Caption = (a.Activity.Caption + (a.IdContingent.HasValue ? ("-" + a.Contingent.Caption) : "")) })
		//                               .ToList();

		//    if (nonExisted.Any())
		//    {
		//        Controls.Throw(
		//            "Следующие мероприятия отсутствуют в проектном документе «План деятельности» и будут удалены из таблицы:"
		//            + string.Join("<br/> - ", nonExisted.Select(a => a.Caption).Distinct().ToList()) +
		//            "Продолжить?");

		//        var activityIds = nonExisted.Select(n => n.Id).ToArray();
		//        context.PublicInstitutionEstimate_ActivityAUBU.RemoveAll(ActivitiesAUBU.Where(a => activityIds.Contains(a.Id)).ToList());
		//        context.SaveChanges();
		//    }
		//}
		
		/// <summary>
		/// Проверка равенства суммы расходов учредителя по деятельности АУ/БУ и расходов автономных и бюджетных учрежденийПроверка равенства суммы расходов учредителя по деятельности АУ/БУ и расходов автономных и бюджетных учреждений
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка равенства суммы расходов учредителя по деятельности АУ/БУ и расходов автономных и бюджетных учреждений", InitialUNK = "0729")]
		public void Control_0729(DataContext context)
		{
			if (!SBP.IsFounder)
				return;

			var errors = new List<string>();

			var activities = ActivitiesAUBU.Select(a => a.Id).ToList();

			foreach (var idActivity in activities)
			{
				var founderExpense =
					FounderAUBUExpenses.AsQueryable()
									   .Where(e => e.IdMaster == idActivity)
									   .GroupBy(e => true)
									   .Select(g => new
									   {
										   ofg = g.Sum(t => t.OFG),
										   pfg1 = g.Sum(t => t.PFG1),
										   pfg2 = g.Sum(t => t.PFG2)
									   }).FirstOrDefault() ?? new { ofg = (decimal?)0, pfg1 = (decimal?)0, pfg2 = (decimal?)0 };

				var aubuExpense =
					AloneAndBudgetInstitutionExpenses.AsQueryable()
									   .Where(e => e.IdMaster == idActivity)
									   .GroupBy(e => true)
									   .Select(g => new
									   {
										   ofg = g.Sum(t => t.OFG),
										   pfg1 = g.Sum(t => t.PFG1),
										   pfg2 = g.Sum(t => t.PFG2)
									   }).FirstOrDefault() ?? new { ofg = (decimal?)0, pfg1 = (decimal?)0, pfg2 = (decimal?)0 };

				if (founderExpense.ofg != aubuExpense.ofg || founderExpense.pfg1 != aubuExpense.pfg1 || founderExpense.pfg2 != aubuExpense.pfg2)
				{
					var activity = ActivitiesAUBU.First(a => a.Id == idActivity);
					var msg = "-" + activity.Activity.Caption + (activity.Contingent == null ? string.Empty : activity.Contingent.Caption);
					errors.Add(msg);
				}

			}

			if (errors.Any())
			{
				var msg = " Сумма расходов учредителя по деятельности АУ/БУ не равна сумме расходов" +
						  " автономных и бюджетных учреждений по следующим мероприятиям: <br/>" +
						  string.Join("<br/>", errors);

				Controls.Throw(msg);
			}

		}

		/// <summary>
		/// Проверка на не превышение предельных объемов ассигнований по доп. потребностям  (собственная деятельность)
		/// </summary>
		[ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований по доп. потребностям  (собственная деятельность)", InitialUNK = "0730")]
		public void Control_0730(DataContext context)
		{
			var properties = BlankBringingKU.GetBlankCostMandatoryProperties();

			var query = new StringBuilder(@"Select Top 1 1 as Id 
											From reg.LimitVolumeAppropriations L 
											Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine 
											Where L.idVersion = " + IdVersion + @" And 
													L.idBudget = " + IdBudget + @" And 
													L.idPublicLegalFormation = " + IdPublicLegalFormation + @" And 
													L.idValueType = " + ((byte)ValueType.Plan).ToString() + @" And 
													EL.idSBP = " + IdSBP + @" 
											Group By ");
			query.Append(string.Join(", ", properties.Select(p => "EL." + p)));
			query.Append(@" Having Sum(L.Value) <> 0 And ( ");
			query.Append(string.Join(" Or ", properties.Select(p => "El." + p + " Is Null ")));
			query.Append(" )");

			if (context.Database.SqlQuery<int>(query.ToString()).Any())
				Controls.Throw("В документе «План деятельности» по учреждению «" + SBP.Caption + "» заполнение обязательных полей КБК не соответствует бланку «Доведение КУ»");


			var idKosgu000 = BlankBringingKU.BlankValueType_KOSGU == BlankValueType.Mandatory ?
								(int?)context.KOSGU.Where(k => k.Code == "000").Select(k => k.Id).FirstOrDefault() : null;
			if (idKosgu000 == 0 || (idKosgu000.HasValue &&
				!context.LimitVolumeAppropriations.Any(l => l.IdPublicLegalFormation == IdPublicLegalFormation && l.IdBudget == IdBudget && l.IdVersion == IdVersion)))
				idKosgu000 = null;

			var limitErrors = CheckLimitAllocations0730(context, new[] { BlankBringingKU, BlankFormationKU }, false, idKosgu000).ToList();

			if (limitErrors.Any())
			{
				var msg = "Превышение обнаружено по строкам: <br/>" + String.Join("<br/>", limitErrors);

				var commonPartMsg = new StringBuilder("Всего объем средств: ");
				var prevVersionIds = PrevVersionIds;

				var planValues = context.LimitVolumeAppropriations.Where(l => l.EstimatedLine.IdSBP == IdSBP &&
																			(l.IdRegistratorEntity != Id || !prevVersionIds.Contains(l.IdRegistrator)) &&
																			l.IdValueType == (byte)ValueType.Plan &&
																			l.IdPublicLegalFormation == IdPublicLegalFormation &&
																			l.IdBudget == IdBudget &&
																			l.IdVersion == IdVersion &&
																			l.HasAdditionalNeed == true &&
																			!l.IsMeansAUBU)
									  .GroupBy(l => l.IdHierarchyPeriod)
									  .Select(g => new { g.Key, Value = g.Sum(l => l.Value) })
									  .ToList();

				var docValues = Expenses.GroupBy(s => true).Select(g => new
				{
					valueFirstYear = g.Sum(s => s.AdditionalOFG),
					valueSecondYear = g.Sum(s => s.AdditionalPFG1),
					valueThirdYear = g.Sum(s => s.AdditionalPFG2)
				}).FirstOrDefault();

				for (int year = Budget.Year; year <= Budget.Year + 2; year++)
				{
					var yearPlanValue = planValues.Where(v => v.Key == year.GetIdHierarchyPeriodYear(context)).Select(v => v.Value).FirstOrDefault();
					commonPartMsg.AppendFormat("{0} г. = {1}; ", year, yearPlanValue);
				}

				if (docValues == null)
					throw new PlatformException("В ТЧ Расходы не введено не одного значения!");

				commonPartMsg.Append("<br/>Всего обоснованных ассигнований: ");
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year, docValues.valueFirstYear);
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year + 1, docValues.valueSecondYear);
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year + 2, docValues.valueThirdYear);

				Controls.Throw(
					String.Format("Сумма обоснованных ассигнований по дополнительным потребностям, запланированная на осуществление собственной деятельности, превышает предельный объем бюджетных ассигнований по дополнительным потребностям данного учреждения. <br/> {0} {1}",
						commonPartMsg, msg));
			}
		}

		/// <summary>
		/// Проверка на не превышение предельных объемов ассигнований по доп. потребностям (Деятельность АУ/БУ)
		/// </summary>
		/// <param name="context"></param>
		[ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований по доп. потребностям (Деятельность АУ/БУ)", InitialUNK = "0731")]
		public void Control_0731(DataContext context)
		{
			if (SBP == null || !SBP.IdParent.HasValue)
				throw new PlatformException("У СБП типа казенное учреждение отсутствует вышестоящее учреждение");

			var parentSBP = SBP.Parent;
			if (parentSBP == null)
				throw new PlatformException(String.Format("СБП с Id = {0} не найден в БД", SBP.IdParent));

			var parentSBPBlankBringingKU = parentSBP.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.BlankType == BlankType.BringingKU);
			var parentSBPBlankFormationKU = parentSBP.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.BlankType == BlankType.FormationKU);

			var prevVersionIds = PrevVersionIds;

			var limitErrors = CheckLimitAllocations0731(context, new[] { parentSBPBlankBringingKU, parentSBPBlankFormationKU }).ToList();
			if (limitErrors.Any())
			{
				var msg = "Превышение обнаружено по строкам: <br/>" + String.Join("<br/>", limitErrors);

				var commonPartMsg = new StringBuilder("Всего объем средств: ");
				var planValues = context.LimitVolumeAppropriations.Where(l => l.EstimatedLine.IdSBP == IdSBP &&
																			  (l.IdRegistratorEntity != Id || !prevVersionIds.Contains(l.IdRegistrator)) &&
																			  l.IdValueType == (byte)ValueType.Plan &&
																			  l.IdPublicLegalFormation == IdPublicLegalFormation &&
																			  l.IdBudget == IdBudget &&
																			  l.IdVersion == IdVersion &&
																			  l.HasAdditionalNeed == true &&
																			  !l.IsMeansAUBU)
										.GroupBy(l => l.IdHierarchyPeriod)
										.Select(g => new { g.Key, Value = g.Sum(l => l.Value) })
										.ToList();
				var docValues = FounderAUBUExpenses.GroupBy(s => true).Select(g => new
				{
					valueFirstYear = g.Sum(s => s.AdditionalOFG),
					valueSecondYear = g.Sum(s => s.AdditionalPFG1),
					valueThirdYear = g.Sum(s => s.AdditionalPFG2)
				}).FirstOrDefault();

				for (int year = Budget.Year; year <= Budget.Year + 2; year++)
				{
					var yearPlanValue = planValues.Where(v => v.Key == year.GetIdHierarchyPeriodYear(context)).Select(v => v.Value).FirstOrDefault();
					commonPartMsg.AppendFormat("{0} г. = {1}; ", year, yearPlanValue);
				}

				if (docValues == null)
					throw new PlatformException("В ТЧ Расходы не введено не одного значения!");

				commonPartMsg.Append("<br/>Всего обоснованных ассигнований: ");
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year, docValues.valueFirstYear);
				commonPartMsg.AppendFormat("{0} г. = {1}; ", Budget.Year + 1, docValues.valueSecondYear);
				commonPartMsg.AppendFormat("{0} г. = {1}; <br/>", Budget.Year + 2, docValues.valueThirdYear);

				Controls.Throw(
					String.Format("Сумма обоснованных ассигнований по дополнительным потребностям, запланированная на осуществление деятельности АУБУ, превышает предельный объем бюджетных ассигнований по дополнительным потребностям данного учреждения. <br/> {0} {1}",
						commonPartMsg, msg));
			}
		}

		/// <summary>   
		/// Контроль "Проверка соответствия текущего бланка формирования (формирования АУ/БУ) с актуальным "
		/// </summary>         
		[ControlInitial(InitialUNK = "0732", InitialCaption = "Проверка соответствия текущего бланка формирования (формирования АУ/БУ) с актуальным ", InitialSkippable = true)]
		public void Control_0732(DataContext context)
		{
			IQueryable<SBP_Blank> newBlanks;
			SBP_BlankHistory oldBlank;
			byte idchekblanktype;
			bool chk1 = true;
			bool chk2 = true;
			string msg = string.Empty;

			newBlanks = context.SBP_Blank.Where(r =>
												r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.IdParent &&
												r.IdBlankType == (byte)DbEnums.BlankType.FormationKU);
			oldBlank = this.SBP_BlankActual;

			if (!newBlanks.Any())
			{
				return;
			}

			var newBlank = newBlanks.FirstOrDefault();

			chk1 = oldBlank != null && SBP_BlankHelper.IsEqualBlank(newBlank, oldBlank);
			
			if (this.SBP.SBPType == DbEnums.SBPType.TreasuryEstablishment && this.SBP.IsFounder)
			{
				newBlanks = context.SBP_Blank.Where(r =>
													r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.Id &&
													r.IdBlankType == (byte)DbEnums.BlankType.FormationAUBU);
				oldBlank = this.SBP_BlankActualAuBu;

				if (!newBlanks.Any())
				{
					return;
				}

				newBlank = newBlanks.FirstOrDefault();
				chk2 = oldBlank != null && SBP_BlankHelper.IsEqualBlank(newBlank, oldBlank);

			}

            if (!chk1 && chk2)
            {
                msg = "Был изменен бланк «Формирование КУ». Необходимо актуализировать сведения в таблицах: «Расходы», «Косвенные расходы», «Расходы учредителя по мероприятиям АУ/БУ». В строках перечисленных таблиц будут очищены КБК, не соответствующие бланку формирования, и выполнится группировка сметных строк.";
            }
            else if (chk1 && !chk2)
            {
                msg = "Был изменен бланк «Формирование АУ/БУ». Необходимо актуализировать сведения в таблице «Расходы автономных и бюджетных учреждений», в строках будут очищены КБК, не соответствующие бланку формирования, и выполнится группировка сметных строк.";
            }
            else if (!chk1 && !chk2)
            {
                msg = "Были изменены бланки «Формирование КУ» и «Формирование АУ/БУ». Необходимо актуализировать сведения в таблицах: «Расходы», «Косвенные расходы», «Расходы учредителя по мероприятиям АУ/БУ», «Расходы автономных и бюджетных учреждений». В строках перечисленных таблиц будут очищены КБК, не соответствующие бланку формирования, и выполнится группировка сметных строк.";
            }

			if (msg != string.Empty) 
				Controls.Throw(msg);
		}

	    /// <summary>
	    /// Проверка на не превышение предельных объемов ассигнований по собственной деятельности
	    /// </summary>
	    /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований по собственной деятельности", InitialUNK = "0733")]
	    public void Control_0733(DataContext context)
	    {
            Control_0714(context);
	    }

        /// <summary>
        /// Проверка на не превышение предельных объемов ассигнований по деятельности АУ/БУ
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований по деятельности АУ/БУ", InitialUNK = "0734")]
        public void Control_0734(DataContext context)
        {
            Control_0717(context);
        }

        /// <summary>
        /// Проверка на не превышение предельных объемов ассигнований по доп. потребностям  (собственная деятельность)
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований по доп. потребностям  (собственная деятельность)", InitialUNK = "0735")]
        public void Control_0735(DataContext context)
        {
            Control_0730(context);
        }

        /// <summary>
        /// Проверка на не превышение предельных объемов ассигнований по доп. потребностям (Деятельность АУ/БУ)
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Проверка на не превышение предельных объемов ассигнований по доп. потребностям (Деятельность АУ/БУ)", InitialUNK = "0736")]
        public void Control_0736(DataContext context)
        {
            Control_0731(context);
        }

        /// <summary>   
        /// Контроль "Проверка на округление до сотен"
        /// </summary>
        [ControlInitial(InitialUNK = "0737", InitialCaption = "Проверка на округление до сотен", InitialManaged = true)]
        public void Control_0737(DataContext context)
        {

            var tp = context.PublicInstitutionEstimate_Expense.Where(r => r.IdOwner == this.Id)
                            .Select(s => new { v = s, a = s.Master }).ToList()
                            .Where(r =>
                                (r.v.OFG.HasValue && !CommonMethods.IsRound100(r.v.OFG.Value)) ||
                                (r.v.AdditionalOFG.HasValue && !CommonMethods.IsRound100(r.v.AdditionalOFG.Value)) ||
                                (r.v.PFG1.HasValue && !CommonMethods.IsRound100(r.v.PFG1.Value)) ||
                                (r.v.AdditionalPFG1.HasValue && !CommonMethods.IsRound100(r.v.AdditionalPFG1.Value)) ||
                                (r.v.PFG2.HasValue && !CommonMethods.IsRound100(r.v.PFG2.Value)) ||
                                (r.v.AdditionalPFG2.HasValue && !CommonMethods.IsRound100(r.v.AdditionalPFG2.Value))

                                )
                            .Select(s => new {s.v, s.a, e = s.v.GetEstimatedLine(context).ToString()});

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

            Controls.Throw(string.Format(msg, sb.Replace("Вид бюджетной деятельности Расходы, ", "")));
        }
	}
}
