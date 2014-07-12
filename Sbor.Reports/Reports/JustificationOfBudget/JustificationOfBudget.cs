using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using BaseApp.Reference;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using System.Globalization;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Reports.JustificationOfBudget;

namespace Sbor.Reports.Report
{
    [Report]
    partial class JustificationOfBudget
    {
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert, Sequence.After, ExecutionOrder = -1500)]
        public void PreFillListRemovedFields(DataContext context, ControlType ctType)
        {
            this.ListRemovedField.Add(context.ListRemovedFields.Where(r => r.Entity.Name == "RzPR").FirstOrDefault());
            this.ListRemovedField.Add(context.ListRemovedFields.Where(r => r.Entity.Name == "KCSR").FirstOrDefault());
            this.ListRemovedField.Add(context.ListRemovedFields.Where(r => r.Entity.Name == "KVR").FirstOrDefault());
            this.ListRemovedField.Add(context.ListRemovedFields.Where(r => r.Entity.Name == "KOSGU").FirstOrDefault());
        }

        private int budgetyear;
        private bool hasRZPR;
        private bool hasKCSR;
        private bool hasKVR;
        private bool hasKOSGU;

        private bool HasCode(DataContext context, string c)
        {
            return this.ListRemovedField.Contains(
                context.ListRemovedFields.Where(r => r.Entity.Name == c).FirstOrDefault());
        }

        public List<PrimeDataSet> MainData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var u = (User) IoC.Resolve<IUser>("CurrentUser");

            budgetyear = this.Budget.Year;

            hasRZPR = HasCode(context, "RzPR");
            hasKCSR = HasCode(context, "KCSR");
            hasKVR = HasCode(context, "KVR");
            hasKOSGU = HasCode(context, "KOSGU");

            var res = new List<PrimeDataSet>();

            var sn30 = (!u.IdResponsiblePerson.HasValue ? "" : context.ResponsiblePerson.FirstOrDefault(r => r.Id == u.IdResponsiblePerson).OfficialCapacity.Caption);

            var ruks = this.SBP.Organization.ResponsiblePerson.Where(r => r.IdRoleResponsiblePerson == (int) BaseApp.DbEnums.RoleResponsiblePerson.Head);

            res.Add(new PrimeDataSet()
                {
                    curDate =
                        ByApproved.HasValue && ByApproved.Value
                            ? DateReport.Value.ToShortDateString()
                            : DateTime.Now.ToShortDateString(),
                    sn_1 = this.PublicLegalFormation.Caption,
                    sn_2 = this.Budget.Year.ToString(),
                    sn_3 = string.Format("{0} - {1}", this.Budget.Year + 1, this.Budget.Year + 2),
                    sn_4 = this.SBP.Organization.Description,
                    sn_29 = ruks.Any() ? ruks.FirstOrDefault().Caption : "",
                    sn_30 = sn30,
                    sn_31 = u.Caption,
                    sn_32 = u.Telephone,
                    OnRzPR = hasRZPR,
                    OnKVR = hasKVR,
                    OnKOSGU = hasKOSGU,
                    OnKCSR = hasKCSR
                });

            return res;
        }

        private Dictionary<int, int> dirHP;

        private string emptykey = "000000000000000";

        public List<EmptyDataSet> GetEmptyDataSet()
        {
            return new List<EmptyDataSet>(){new EmptyDataSet()};
        }

        public List<TableSet2> TableData1()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            List<TableSet2> res = new List<TableSet2>();

            budgetyear = this.Budget.Year;

            hasRZPR = HasCode(context, "RzPR");
            hasKCSR = HasCode(context, "KCSR");
            hasKVR = HasCode(context, "KVR");
            hasKOSGU = HasCode(context, "KOSGU");

            AddLineToReport(0, "1. Правовые основания возникновения действующих расходных обязательств и изменения действующих расходных обязательств", res);
            AddLineToReport(1, "", res);
            AddLineToReport(2, "", res);
            AddLineToReport(3, "", res);
            AddLineToReport(3, "", res);
            AddLineToReport(3, "", res);

            AddLineToReport(0, "2. Объем бюджетных ассигнований, необходимый на исполнение действующих расходных обязательств, тыс. руб.", res);
            AddLineToReport(4, "", res);
            AddLineToReport(5, "", res);
            AddLineToReport(6, "", res);

            res.AddRange(TableDataRes(context, Sbor.DbEnums.ExpenseObligationType.Existing));

            AddLineToReport(0, "3. Правовые основания возникновения принимаемых расходных обязательств", res);
            AddLineToReport(1, "", res);
            AddLineToReport(2, "", res);
            AddLineToReport(3, "", res);
            AddLineToReport(3, "", res);
            AddLineToReport(3, "", res);

            AddLineToReport(0, "4. Объем бюджетных ассигнований на исполнение принимаемых обязательств, тыс. руб.", res);
            AddLineToReport(4, "", res);
            AddLineToReport(5, "", res);
            AddLineToReport(6, "", res);

            res.AddRange(TableDataRes(context, Sbor.DbEnums.ExpenseObligationType.Accepted));

            AddLineToReport(9, "", res);
            AddLineToReport(10, "", res);
            AddLineToReport(10, "", res);
            AddLineToReport(10, "", res);
            AddLineToReport(11, "", res);
            return res;
        }

        private static void AddLineToReport(int typeline, string sn5, List<TableSet2> res)
        {
            TableSet2 newTableSet2;
            newTableSet2 = new TableSet2()
                {
                    typeline = typeline,
                    sn5 = sn5
                };
            res.Add(newTableSet2);
        }

        private List<TableSet2> TableDataRes(DataContext context, ExpenseObligationType expenseObligationType)
        {
            var limits = context.LimitVolumeAppropriations.
                                 Where(r =>
                                       r.IdPublicLegalFormation == this.IdPublicLegalFormation &&
                                       r.IdVersion == this.IdVersion && r.IdBudget == this.IdBudget &&
                                       r.IdValueType == (int) Sbor.DbEnums.ValueType.Justified &&
                                       ((!this.ByApproved.Value) ||
                                        (this.ByApproved.Value && r.DateCommit <= DateReport)) &&
                                       r.EstimatedLine.IdExpenseObligationType == (int) expenseObligationType &&
                                       r.IdAuthorityOfExpenseObligation.HasValue)
                                .ToList()
                                .Where(
                                    r =>
                                    r.EstimatedLine.SBP.IdSBPType == (int) Sbor.DbEnums.SBPType.TreasuryEstablishment &&
                                    GetFirstParentGRBS(r.EstimatedLine.SBP) == this.SBP &&
                                    (!this.RZPR.Any() || this.RZPR.Contains(r.EstimatedLine.RZPR)) &&
                                    (!this.KCSR.Any() || this.KCSR.Contains(r.EstimatedLine.KCSR)) &&
                                    (!this.KVR.Any() || this.KVR.Contains(r.EstimatedLine.KVR)) &&
                                    (!this.KOSGU.Any() || this.KOSGU.Contains(r.EstimatedLine.KOSGU)) &&
                                    (!this.AuthorityOfExpenseObligation.Any() ||
                                     this.AuthorityOfExpenseObligation.Contains(
                                         GetSecondParent(r.AuthorityOfExpenseObligation))))
                                .Select(s => new
                                    {
                                        RZPR = hasRZPR ? s.EstimatedLine.RZPR : null,
                                        KCSR = hasKCSR ? s.EstimatedLine.KCSR : null,
                                        KVR = hasKVR ? s.EstimatedLine.KVR : null,
                                        KOSGU = hasKOSGU ? s.EstimatedLine.KOSGU : null,
                                        CodeRO = GetSecondParent(s.AuthorityOfExpenseObligation),
                                        Ofg = ((s.HierarchyPeriod.DateStart.Year == budgetyear &&
                                                (!s.HasAdditionalNeed.HasValue || !s.HasAdditionalNeed.Value))
                                                   ? s.Value
                                                   : 0),
                                        Pfg1 = ((s.HierarchyPeriod.DateStart.Year == (budgetyear + 1) &&
                                                 (!s.HasAdditionalNeed.HasValue || !s.HasAdditionalNeed.Value))
                                                    ? s.Value
                                                    : 0),
                                        Pfg2 = ((s.HierarchyPeriod.DateStart.Year == (budgetyear + 2) &&
                                                 (!s.HasAdditionalNeed.HasValue || !s.HasAdditionalNeed.Value))
                                                    ? s.Value
                                                    : 0),
                                        OfgA = ((s.HierarchyPeriod.DateStart.Year == budgetyear)
                                                    ? s.Value
                                                    : 0),
                                        Pfg1A = ((s.HierarchyPeriod.DateStart.Year == (budgetyear + 1))
                                                     ? s.Value
                                                     : 0),
                                        Pfg2A = ((s.HierarchyPeriod.DateStart.Year == (budgetyear + 2))
                                                     ? s.Value
                                                     : 0)
                                    });

            var res0 = limits.
                GroupBy(g =>
                        new
                            {
                                g.RZPR,
                                g.KCSR,
                                g.KVR,
                                g.KOSGU,
                                g.CodeRO
                            }).
                Select(g =>
                       new
                           {
                               g.Key,
                               Ofg = g.Sum(s => s.Ofg),
                               OfgA = g.Sum(s => s.OfgA),
                               Pfg1 = g.Sum(s => s.Pfg1),
                               Pfg1A = g.Sum(s => s.Pfg1A),
                               Pfg2 = g.Sum(s => s.Pfg2),
                               Pfg2A = g.Sum(s => s.Pfg2A)
                           });

            var res = new List<TableSet2>();

            foreach (var g in res0)
            {
              var newTableSet1 = new TableSet2()
                           {
                               typeline = 7,
                               sn5 = g.Key.CodeRO.Description,
                               sn6 = (g.Key.RZPR == null) ? "" : g.Key.RZPR.Code.Substring(0, 2),
                               sn7 = (g.Key.RZPR == null) ? "" : g.Key.RZPR.Code.Substring(2, 2),
                               sn8 = (g.Key.KCSR == null) ? "" : g.Key.KCSR.Code,
                               sn9 = (g.Key.KVR == null) ? "" : g.Key.KVR.Code,
                               sn10 = (g.Key.KOSGU == null) ? "" : g.Key.KOSGU.Code,
                               sn11 = RoundToString(g.Ofg),
                               sn12 = RoundToString(g.OfgA),
                               sn13 = RoundToString(g.Pfg1),
                               sn14 = RoundToString(g.Pfg1A),
                               sn15 = RoundToString(g.Pfg2),
                               sn16 = RoundToString(g.Pfg2A)
                           };

                res.Add(newTableSet1);
            }

            var newTableSet2 = new TableSet2()
                {
                    typeline = 8,
                    sn11 = RoundToString(limits.Sum(s => s.Ofg)),
                    sn12 = RoundToString(limits.Sum(s => s.OfgA)),
                    sn13 = RoundToString(limits.Sum(s => s.Pfg1)),
                    sn14 = RoundToString(limits.Sum(s => s.Pfg1A)),
                    sn15 = RoundToString(limits.Sum(s => s.Pfg2)),
                    sn16 = RoundToString(limits.Sum(s => s.Pfg2A))
                };

            res.Add(newTableSet2);
            return res;
        }

        private AuthorityOfExpenseObligation GetSecondParent(AuthorityOfExpenseObligation child)
        {
            var res = child;
            do
            {
                if (!res.IdParent.HasValue)
                    return null;
                
                if (!res.Parent.IdParent.HasValue)
                    return res;
                
                res = res.Parent;
            } while (true);
        }

        private string RoundToString(decimal s)
        {
            return Math.Round((decimal) ((s)/1000), 1).ToString();
        }

        private SBP GetFirstParentGRBS(SBP sbp)
        {
            SBP ret = sbp;
            do
            {
                if (!ret.IdParent.HasValue)
                {
                    ret = null;
                    break;
                }
                
                ret = ret.Parent;
                if (ret.IdSBPType == (int)SBPType.GeneralManager)
                    break;
                
            } while (true);

            return ret;
        }
    }



}
