using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.ReportingServices;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Tablepart;

namespace Sbor.Reports.PrintForms.FinancialAndBusinessActivities
{
    /// <summary>
    /// Для просмотра: http://localhost/sbor3/Services/PrintForm.aspx?entityName=FinancialAndBusinessActivities&printFormClassName=FinancialAndBusinessActivitiesPf&docId=16
    /// </summary>
    [PrintForm(Caption = "Печатная форма")]
    public class FinancialAndBusinessActivitiesPf : PrintFormBase
    {
        public FinancialAndBusinessActivitiesPf(int docId) : base(docId) { }

        public List<DSTopOfDoc> DSTopOfDoc()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.FinancialAndBusinessActivities.Single(s => s.Id == DocId);
            var ppo = context.PublicLegalFormation.Single(s => s.Id == doc.IdPublicLegalFormation);

            var sbp = doc.SBP;

            return new List<DSTopOfDoc>() { 
                new DSTopOfDoc()
                {
                    Id = DocId, 
                    Caption = doc.DocumentCaption, 
                    CurrentDate = DateTime.Now,
                    IdPublicLegalFormation = ppo.Id,
                    CapPublicLegalFormation = ppo.Caption,
                    Header = doc.ToString(),
                    BudgetYear = doc.Budget.Year,
                    sn1 = sbp.Organization.Description,
                    sn4 = doc.Date, 
                    sn5 = sbp.Organization.Okpo,
                    sn6 = sbp.Organization.INN,
                    sn7 = sbp.Organization.KPP,
                    sn8 = sbp.Parent.Organization.Description,
                    sn9 = sbp.Organization.LegalAddress,
                    sn10 = sbp.Organization.PostAdress,
                    HasAddValue = doc.IsExtraNeed
                } 
            };
        }

        public List<ListCaption> DSGoals_sn11()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var tbl = context.FBA_DepartmentActivityGoal.Where(s => s.IdOwner == DocId);

            return
                tbl.Select(s => new ListCaption()
                    {
                        Caption = " - " + s.DepartmentGoal
                    }).ToList();
        }

        public List<ListCaption> DSActivity_sn12()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var tbl = context.FBA_Activity.Where(s => s.IdOwner == DocId);

            return
                tbl.Select(s => new ListCaption()
                {
                    Caption = s.Activity.Caption
                }).ToList();
        }

        public List<ListCaption> DSActivityPaid_sn13()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var tbl = context.FBA_Activity.Where(s =>
                                                 s.IdOwner == DocId &&
                                                 s.Activity.IdActivityType == (int)ActivityType.Service &&
                                                 (s.Activity.IdPaidType == (int)PaidType.FullPaid || s.Activity.IdPaidType == (int)PaidType.PartiallyPaid)
                );

            return
                tbl.Select(s => new ListCaption()
                {
                    Caption = s.Activity.Caption
                }).ToList();
        }

        public List<DSFinInd> DSFinIndicator_sn14()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.FinancialAndBusinessActivities.Single(s => s.Id == DocId);
            var ppo = context.PublicLegalFormation.Single(s => s.Id == doc.IdPublicLegalFormation);

            var sprFi = context.FinancialIndicator.Where(r => r.IdPublicLegalFormation == doc.IdPublicLegalFormation).ToList();

            var tp = context.FBA_FinancialIndicatorsInstitutions.Where(r => r.IdOwner == DocId && r.Value.HasValue).ToList();

            var parents = tp.Select(c => c.FinancialIndicator.IdParent).ToList();
            List<int?> extparents = new List<int?>();

            foreach (var parent in parents.Where(s => s.HasValue))
            {
                var s0 = sprFi.Single(w => w.Id == parent);
                while (s0.IdParent.HasValue)
                {
                    s0 = sprFi.Single(w => w.Id == s0.IdParent);
                    extparents.Add(s0.Id);
                }
            }

            parents = parents.Union(extparents).ToList();

            var iparents = parents.Distinct().ToArray();

            var table = 
                from s in sprFi
                join t in tp on s.Id equals t.FinancialIndicator.Id into temp
                from t0 in temp.DefaultIfEmpty()
                orderby s.RowCode
                select new
                {
                    el = s,
                    bold = !s.IdParent.HasValue,
                    izn = !s.IdParent.HasValue && iparents.Contains(s.Id),
                    vtc = s.IdParent.HasValue && iparents.Contains(s.Id),
                    haschild = iparents.Contains(s.Id),
                    value = t0 == null ? (decimal?)null : t0.Value 
                };

            var res = new List<DSFinInd>();

            foreach (var tpl in table.Where(r => r.value.HasValue || r.haschild))
            {
                var length = tpl.el.RowCode.Length;

                res.Add(new DSFinInd()
                    {
                        Caption = tpl.el.RowCode + " " + tpl.el.Caption,
                        Bold = tpl.bold,
                        Value = tpl.value,
                        pleft = length * 2
                    });

                if (tpl.izn)
                {
                    res.Add(new DSFinInd()
                    {
                        Caption = "из них:",
                        Bold = false,
                        Value = null,
                        pleft = length * 2
                    });
                }
                if (tpl.vtc)
                {
                    res.Add(new DSFinInd()
                    {
                        Caption = "в том числе:",
                        Bold = false,
                        Value = null,
                        pleft = length * 2
                    });
                }
            }

            return res;
        }

        public List<DSPlanIncome> DSPlanned()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var doc = context.FinancialAndBusinessActivities.Single(s => s.Id == DocId);
            List<DSPlanIncome> res = new List<DSPlanIncome>();

            #region получаем данные из документа

            var tpPlanVolumeIncome =
                (from l in context.FBA_PlannedVolumeIncome.Where(r => r.IdOwner == doc.Id)
                 join v in context.FBA_PlannedVolumeIncome_value.Where(r => r.IdOwner == doc.Id) on l.Id equals v.IdMaster
                 join a in context.FBA_Activity.Where(r => r.IdOwner == doc.Id) on l.IdFBA_Activity equals a.Id into tmp
                 from t in tmp.DefaultIfEmpty()
                 select new { l, v, a = (t == null ? null : t.Activity) }).ToList();

            var tpCostActivities =
                (from l in context.FBA_CostActivities.Where(r => r.IdOwner == doc.Id)
                 join a in context.FBA_Activity.Where(r => r.IdOwner == doc.Id) on l.IdMaster equals a.Id
                 join v in context.FBA_CostActivities_value.Where(r => r.IdOwner == doc.Id) on l.Id equals v.IdMaster
                 select new { l, v, a = a.Activity }).ToList();

            var budgetyear = doc.Budget.Year;

            #endregion

            #region готовим данные по Поступлениям

            var sn_19 =
                tpPlanVolumeIncome.Where(w =>
                                         w.l.FinanceSource.IdFinanceSourceType == (int) FinanceSourceType.Remains &&
                                         w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                   Sum(s => s.v.Value);

            var sn_20 =
                tpPlanVolumeIncome.Where(w =>
                                         w.l.FinanceSource.IdFinanceSourceType != (int) FinanceSourceType.Remains &&
                                         w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                   Sum(s => s.v.Value)
                +
                tpCostActivities.Where(w =>
                                       w.l.KFO.IsIncludedInBudget &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int) FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_20_1 =
                tpPlanVolumeIncome.Where(w =>
                                         w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                         w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                   Sum(s => s.v.Value)
                +
                tpCostActivities.Where(w =>
                                       w.l.KFO.IsIncludedInBudget &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_20_2 =
                tpPlanVolumeIncome.Where(w =>
                                         w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                         w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                   Sum(s => s.v.Value)
                +
                tpCostActivities.Where(w =>
                                       w.l.KFO.IsIncludedInBudget &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_21 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "4" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_21_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "4" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_21_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "4" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_22 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "5" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_22_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "5" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_22_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "5" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_23 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "6" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_23_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "6" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_23_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "6" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_24 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "7" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_24_1 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "7" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_24_2 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "7" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);
            var sn_25 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "2" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       (w.a !=null && w.a.IdActivityType == (int)ActivityType.Service) &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_25_1 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "2" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       (w.a != null && w.a.IdActivityType == (int)ActivityType.Service) &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_25_2 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "2" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       (w.a != null && w.a.IdActivityType == (int)ActivityType.Service) &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_26 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "2" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       (w.a == null || w.a.IdActivityType != (int)ActivityType.Service) &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_26_1 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "2" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       (w.a == null || w.a.IdActivityType != (int)ActivityType.Service) &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_26_2 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "2" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       (w.a == null || w.a.IdActivityType != (int)ActivityType.Service) &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_27 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "0" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_27_1 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "0" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_27_2 =
                tpPlanVolumeIncome.Where(w =>
                                       w.l.KFO.Code.Trim() == "0" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_28 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "1" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_28_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "1" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_28_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "1" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);


            var sn_29 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "4" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value2);

            var sn_29_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "4" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value2);

            var sn_29_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "4" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value2);

            var sn_30 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "5" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value2);

            var sn_30_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "5" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value2);

            var sn_30_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "5" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value2);

            var sn_31 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "6" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value2);

            var sn_31_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "6" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value2);

            var sn_31_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "6" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value2);


            var sn_32 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "1" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value2);

            var sn_32_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "1" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value2);

            var sn_32_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.Code.Trim() == "1" &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value2);

            var sn_33 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.IsIncludedInBudget &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value2);

            var sn_33_1 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.IsIncludedInBudget &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value2);

            var sn_33_2 =
                tpCostActivities.Where(w =>
                                       w.l.KFO.IsIncludedInBudget &&
                                       w.l.FinanceSource.IdFinanceSourceType != (int)FinanceSourceType.Remains &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value2);


            var sn_34 =
                tpCostActivities.Where(w =>
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_34_1 =
                tpCostActivities.Where(w =>
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_34_2 =
                tpCostActivities.Where(w =>
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_35 =
                tpCostActivities.Where(w =>
                                       w.l.IsIndirectCosts &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_35_1 =
                tpCostActivities.Where(w =>
                                       w.l.IsIndirectCosts &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_35_2 =
                tpCostActivities.Where(w =>
                                       w.l.IsIndirectCosts &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            var sn_36 =
                tpCostActivities.Where(w =>
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value2);

            var sn_36_1 =
                tpCostActivities.Where(w =>
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value2);

            var sn_36_2 =
                tpCostActivities.Where(w =>
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value2);

            var sn_37 =
                tpCostActivities.Where(w =>
                                       w.l.IsIndirectCosts &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value2);

            var sn_37_1 =
                tpCostActivities.Where(w =>
                                       w.l.IsIndirectCosts &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value2);

            var sn_37_2 =
                tpCostActivities.Where(w =>
                                       w.l.IsIndirectCosts &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value2);

            var sn_47 =
                tpPlanVolumeIncome.Where(w =>
                                         w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                   Sum(s => s.v.Value)
                -
                tpCostActivities.Where(w =>
                                       (!w.l.KFO.IsIncludedInBudget || w.l.FinanceSource.IdFinanceSourceType == (int) FinanceSourceType.Remains) &&
                                       w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                 Sum(s => s.v.Value);

            var sn_48 =
                sn_47
                +
                tpPlanVolumeIncome.Where(w =>
                                         w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                   Sum(s => s.v.Value)
                -
                tpCostActivities.Where(w =>
                                       (!w.l.KFO.IsIncludedInBudget || w.l.FinanceSource.IdFinanceSourceType == (int)FinanceSourceType.Remains) &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                 Sum(s => s.v.Value);

            var sn_49 =
                sn_48
                +
                tpPlanVolumeIncome.Where(w =>
                                         w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                   Sum(s => s.v.Value)
                -
                tpCostActivities.Where(w =>
                                       (!w.l.KFO.IsIncludedInBudget || w.l.FinanceSource.IdFinanceSourceType == (int)FinanceSourceType.Remains) &&
                                       w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                 Sum(s => s.v.Value);

            #endregion

            #region выводим в таблицу данные по Поступлениям

            res.Add(new DSPlanIncome()
            {
                tline = 1,
                Caption = "Планируемый остаток средств на начало периода",
                Value = sn_19,
                Value1 = sn_47,
                Value2 = sn_48
            });

            res.Add(new DSPlanIncome()
                {
                    tline = 2,
                    Caption = "Поступления, всего:",
                    Value = sn_20,
                    Value1 = sn_20_1,
                    Value2 = sn_20_2,
                    ExValue = sn_33,
                    ExValue1 = sn_33_1,
                    ExValue2 = sn_33_2
                });

            res.Add(new DSPlanIncome()
                {
                    tline = 5,
                    Caption = "в том числе"
                });

            res.Add(new DSPlanIncome()
                {
                    tline = 4,
                    Caption = "Субсидия на выполнение государственного (муниципального) задания",
                    Value = sn_21,
                    Value1 = sn_21_1,
                    Value2 = sn_21_2,
                    ExValue = sn_29,
                    ExValue1 = sn_29_1,
                    ExValue2 = sn_29_2
                });

            res.Add(new DSPlanIncome()
                {
                    tline = 4,
                    Caption = "Целевая субсидия",
                    Value = sn_22,
                    Value1 = sn_22_1,
                    Value2 = sn_22_2,
                    ExValue = sn_30,
                    ExValue1 = sn_30_1,
                    ExValue2 = sn_30_2
                });

            res.Add(new DSPlanIncome()
                {
                    tline = 4,
                    Caption = "Бюджетные инвестиции",
                    Value = sn_23,
                    Value1 = sn_23_1,
                    Value2 = sn_23_2,
                    ExValue = sn_31,
                    ExValue1 = sn_31_1,
                    ExValue2 = sn_31_2
                });

            res.Add(new DSPlanIncome()
                {
                    tline = 3,
                    Caption = "Поступление средств по обязательному медицинскому страхованию",
                    Value = sn_24,
                    Value1 = sn_24_1,
                    Value2 = sn_24_2
                });

            res.Add(new DSPlanIncome()
                {
                    tline = 3,
                    Caption = "Поступления от оказания учреждением услуг, предоставление которых для физических и юридических лиц осуществляется на платной основе",
                    Value = sn_25,
                    Value1 = sn_25_1,
                    Value2 = sn_25_2
                });

            res.Add(new DSPlanIncome()
                {
                    tline = 3,
                    Caption = "Поступления от иной приносящей доход деятельности, всего",
                    Value = sn_26,
                    Value1 = sn_26_1,
                    Value2 = sn_26_2
                });

            res.Add(new DSPlanIncome()
                {
                    tline = 3,
                    Caption = "Поступления от реализации ценных бумаг",
                    Value = sn_27,
                    Value1 = sn_27_1,
                    Value2 = sn_27_2
                });


            res.Add(new DSPlanIncome()
                {
                    tline = 4,
                    Caption = "Справочно:" +
                              "Бюджетные ассигнования на исполнение публичных обязательств",
                    Value = sn_28,
                    Value1 = sn_28_1,
                    Value2 = sn_28_2,
                    ExValue = sn_32,
                    ExValue1 = sn_32_1,
                    ExValue2 = sn_32_2
                });

            #endregion

            #region выводим данные по Расходам

            res.Add(new DSPlanIncome()
            {
                tline = 10,
                Caption = "Выплаты, всего:",
                Value = sn_34,
                ExValue = sn_35,
                KValue = sn_36,
                KExValue = sn_37,
                Value1 = sn_34_1,
                ExValue1 = sn_35_1,
                KValue1 = sn_36_1,
                KExValue1 = sn_37_1,
                Value2 = sn_34_2,
                ExValue2 = sn_35_2,
                KValue2 = sn_36_2,
                KExValue2 = sn_37_2
            });

            res.Add(new DSPlanIncome()
            {
                tline = 5,
                Caption = "в том числе:"
            });

            var activs = tpCostActivities.
                Select(s => s.a).
                Distinct().
                OrderBy(o => o.Caption);

            int nact = 1;

            foreach (var activity in activs)
            {
                res.Add(new DSPlanIncome()
                {
                    tline = 7,
                    Caption = string.Format("{0}. {1}", nact, activity.Caption)
                });

                nact++;

                var kosgusErr = tpCostActivities.Where(r => r.a.Id == activity.Id && r.l.KOSGU == null);
                if (kosgusErr.Any())
                {
                    Controls.Throw(string.Format("в строке расходов по мероприятию '{0}' не указан КОСГУ", activity.Caption));
                }

                var kosgus = tpCostActivities.
                    Where(r => r.a.Id == activity.Id).
                    Select(s => s.l.KOSGU).
                    OrderBy(o => o.Code).
                    Distinct().
                    ToList();

                var hikosgus = kosgus.Select(s => s.Parent).Distinct().OrderBy(o => o.Code).ToList();

                foreach (var hikosgu in hikosgus)
                {
                    #region готовим данные по Группировочным КОСГУ

                    var hkosgu_sums = tpCostActivities.Where(w => w.a == activity && w.l.KOSGU.Parent == hikosgu);

                    var sn_40_Value =
                        hkosgu_sums.Where(w =>
                                               w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                         Sum(s => s.v.Value);

                    var sn_40_Value1 =
                        hkosgu_sums.Where(w =>
                                               w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                         Sum(s => s.v.Value);

                    var sn_40_Value2 =
                        hkosgu_sums.Where(w =>
                                               w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                         Sum(s => s.v.Value);

                    var sn_40_KValue =
                        hkosgu_sums.Where(w =>
                                               w.l.IsIndirectCosts &&
                                               w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                         Sum(s => s.v.Value);

                    var sn_40_KValue1 =
                        hkosgu_sums.Where(w =>
                                               w.l.IsIndirectCosts &&
                                               w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                         Sum(s => s.v.Value);

                    var sn_40_KValue2 =
                        hkosgu_sums.Where(w =>
                                               w.l.IsIndirectCosts &&
                                               w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                         Sum(s => s.v.Value);

                    var sn_40_ExValue =
                        hkosgu_sums.Where(w =>
                                               w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                         Sum(s => s.v.Value2);

                    var sn_40_ExValue1 =
                        hkosgu_sums.Where(w =>
                                               w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                         Sum(s => s.v.Value2);

                    var sn_40_ExValue2 =
                        hkosgu_sums.Where(w =>
                                               w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                         Sum(s => s.v.Value2);

                    var sn_40_KExValue =
                        hkosgu_sums.Where(w =>
                                               w.l.IsIndirectCosts &&
                                               w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                         Sum(s => s.v.Value2);

                    var sn_40_KExValue1 =
                        hkosgu_sums.Where(w =>
                                               w.l.IsIndirectCosts &&
                                               w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                         Sum(s => s.v.Value2);

                    var sn_40_KExValue2 =
                        hkosgu_sums.Where(w =>
                                               w.l.IsIndirectCosts &&
                                               w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                         Sum(s => s.v.Value2);

                    #endregion

                    res.Add(new DSPlanIncome()
                    {
                        tline = 9,
                        Caption = hikosgu.Caption,
                        KOSGU = hikosgu.Code,
                        Value = sn_40_Value,
                        ExValue = sn_40_ExValue,
                        KValue = sn_40_KValue,
                        KExValue = sn_40_KExValue,
                        Value1 = sn_40_Value1,
                        ExValue1 = sn_40_ExValue1,
                        KValue1 = sn_40_KValue1,
                        KExValue1 = sn_40_KExValue1,
                        Value2 = sn_40_Value2,
                        ExValue2 = sn_40_ExValue2,
                        KValue2 = sn_40_KValue2,
                        KExValue2 = sn_40_KExValue2
                    });

                    res.Add(new DSPlanIncome()
                    {
                        tline = 6,
                        Caption = "из них:"
                    });

                    foreach (var kosgu in kosgus.Where(r => r.Parent == hikosgu))
                    {
                        #region готовим данные по конечным КОСГУ

                        var kosgu_sums = tpCostActivities.Where(w => w.a == activity && w.l.KOSGU == kosgu);

                        var sn_42 =
                            kosgu_sums.Where(w =>
                                                   w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                             Sum(s => s.v.Value);

                        var sn_42_1 =
                            kosgu_sums.Where(w =>
                                                   w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                             Sum(s => s.v.Value);

                        var sn_42_2 =
                            kosgu_sums.Where(w =>
                                                   w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                             Sum(s => s.v.Value);

                        var sn_43 =
                            kosgu_sums.Where(w =>
                                                   w.l.IsIndirectCosts &&
                                                   w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                             Sum(s => s.v.Value);

                        var sn_43_1 =
                            kosgu_sums.Where(w =>
                                                   w.l.IsIndirectCosts &&
                                                   w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                             Sum(s => s.v.Value);

                        var sn_43_2 =
                            kosgu_sums.Where(w =>
                                                   w.l.IsIndirectCosts &&
                                                   w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                             Sum(s => s.v.Value);

                        var sn_44 =
                            kosgu_sums.Where(w =>
                                                   w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                             Sum(s => s.v.Value2);

                        var sn_44_1 =
                            kosgu_sums.Where(w =>
                                                   w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                             Sum(s => s.v.Value2);

                        var sn_44_2 =
                            kosgu_sums.Where(w =>
                                                   w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                             Sum(s => s.v.Value2);

                        var sn_45 =
                            kosgu_sums.Where(w =>
                                                   w.l.IsIndirectCosts &&
                                                   w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                             Sum(s => s.v.Value2);

                        var sn_45_1 =
                            kosgu_sums.Where(w =>
                                                   w.l.IsIndirectCosts &&
                                                   w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                             Sum(s => s.v.Value2);

                        var sn_45_2 =
                            kosgu_sums.Where(w =>
                                                   w.l.IsIndirectCosts &&
                                                   w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                             Sum(s => s.v.Value2);

                        #endregion

                        res.Add(new DSPlanIncome()
                        {
                            tline = 9,
                            Caption = kosgu.Caption,
                            KOSGU = kosgu.Code,
                            Value = sn_42,
                            ExValue = sn_43,
                            KValue = sn_44,
                            KExValue = sn_45,
                            Value1 = sn_42_1,
                            ExValue1 = sn_43_1,
                            KValue1 = sn_44_1,
                            KExValue1 = sn_45_1,
                            Value2 = sn_42_2,
                            ExValue2 = sn_43_2,
                            KValue2 = sn_44_2,
                            KExValue2 = sn_45_2
                        });
                    }
                }

                #region готовим данные по Мероприятиям - итоги

                var activ_sums = tpCostActivities.Where(w => w.a == activity);

                var sn_46_Value =
                    activ_sums.Where(w =>
                                           w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                     Sum(s => s.v.Value);

                var sn_46_Value1 =
                    activ_sums.Where(w =>
                                           w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                     Sum(s => s.v.Value);

                var sn_46_Value2 =
                    activ_sums.Where(w =>
                                           w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                     Sum(s => s.v.Value);

                var sn_46_KValue =
                    activ_sums.Where(w =>
                                           w.l.IsIndirectCosts &&
                                           w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                     Sum(s => s.v.Value);

                var sn_46_KValue1 =
                    activ_sums.Where(w =>
                                           w.l.IsIndirectCosts &&
                                           w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                     Sum(s => s.v.Value);

                var sn_46_KValue2 =
                    activ_sums.Where(w =>
                                           w.l.IsIndirectCosts &&
                                           w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                     Sum(s => s.v.Value);

                var sn_46_ExValue =
                    activ_sums.Where(w =>
                                           w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                     Sum(s => s.v.Value2);

                var sn_46_ExValue1 =
                    activ_sums.Where(w =>
                                           w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                     Sum(s => s.v.Value2);

                var sn_46_ExValue2 =
                    activ_sums.Where(w =>
                                           w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                     Sum(s => s.v.Value2);

                var sn_46_KExValue =
                    activ_sums.Where(w =>
                                           w.l.IsIndirectCosts &&
                                           w.v.HierarchyPeriod.DateStart.Year == budgetyear).
                                     Sum(s => s.v.Value2);

                var sn_46_KExValue1 =
                    activ_sums.Where(w =>
                                           w.l.IsIndirectCosts &&
                                           w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 1)).
                                     Sum(s => s.v.Value2);

                var sn_46_KExValue2 =
                    activ_sums.Where(w =>
                                           w.l.IsIndirectCosts &&
                                           w.v.HierarchyPeriod.DateStart.Year == (budgetyear + 2)).
                                     Sum(s => s.v.Value2);
                #endregion

                res.Add(new DSPlanIncome()
                {
                    tline = 8,
                    Caption = activity.Caption,
                    KOSGU = "",
                    Value = sn_46_Value,
                    ExValue = sn_46_ExValue,
                    KValue = sn_46_KValue,
                    KExValue = sn_46_KExValue,
                    Value1 = sn_46_Value1,
                    ExValue1 = sn_46_ExValue1,
                    KValue1 = sn_46_KValue1,
                    KExValue1 = sn_46_KExValue1,
                    Value2 = sn_46_Value2,
                    ExValue2 = sn_46_ExValue2,
                    KValue2 = sn_46_KValue2,
                    KExValue2 = sn_46_KExValue2
                });
            }

            #endregion

            res.Add(new DSPlanIncome()
            {
                tline = 1,
                Caption = "Планируемый остаток средств на конец периода",
                Value = sn_47,
                Value1 = sn_48,
                Value2 = sn_49
            });

            return res;
            
        }

    }
}
