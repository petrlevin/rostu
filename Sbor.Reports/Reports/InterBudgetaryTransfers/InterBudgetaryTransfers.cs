using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using BaseApp.Reference;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Reports.Tablepart;
using Sbor.Reports.InterBudgetaryTransfers;

namespace Sbor.Reports.Report
{
    /// <summary>
    /// Межбюджетные трансферты
    /// </summary>
    [Report]
    public partial class InterBudgetaryTransfers
    {
        [Control(ControlType.Insert, Sequence.Before)]
        [ControlInitial(ExcludeFromSetup = true)]
        public void AutoInsert(DataContext context)
        {
            List<string> lcbl = new List<string>();
            lcbl.Add("Городской округ");
            lcbl.Add("Муниципальный район");
            lcbl.Add("Городское поселение");
            lcbl.Add("Сельское поселение");

            var bl = context.BudgetLevel.Where(s => lcbl.Contains(s.Caption));

            foreach (var budgetLevel in bl)
            {
                this.BudgetLevel.Add(budgetLevel);
            }
        }

        public List<PrimeDataSet> MainData()
        {
            var res = new List<PrimeDataSet>();

            int count = 0;
            if (IsShowYear.Value)
                count++;
            if (IsShowYear1.Value)
                count++;
            if (IsShowYear2.Value)
                count++;

            res.Add(new PrimeDataSet()
                {
                    caption = Caption,
                    header = Header,
                    curDate =
                        (ByApproved.HasValue && ByApproved.Value)
                            ? DateReport.Value.ToShortDateString()
                            : DateTime.Now.ToShortDateString(),
                    udname = this.UnitDimension.Caption,
                    countYear = count,
                    namefirstcolumn = "Наименования " +
                                      this.BudgetLevel
                                          .ToList()
                                          .Select(s => new {s, o = orderBL[s.Caption]})
                                          .OrderBy(o => o.o)
                                          .Select(s => namedBLs[s.s.Caption])
                                          .Aggregate((a, b) => a + ", " + b)
                });

            return res;
        }

        public static readonly Dictionary<string, int> orderBL = new Dictionary<string, int>
            {
                {"Городской округ", 1},
                {"Муниципальный район", 2},
                {"Городское поселение", 3},
                {"Сельское поселение", 4}
            };

        public static readonly Dictionary<string, string> namedBL = new Dictionary<string, string>
            {
                {"Городской округ", "Городские округа"},
                {"Муниципальный район", "Муниципальные районы"},
                {"Городское поселение", "Городские поселения"},
                {"Сельское поселение", "Сельские поселения"}
            };

        public static readonly Dictionary<string, string> namedBLs = new Dictionary<string, string>
            {
                {"Городской округ", "городских округов"},
                {"Муниципальный район", "муниципальных районов"},
                {"Городское поселение", "городских поселений"},
                {"Сельское поселение", "сельских поселений"}
            };

        private static int firstYear;
        List<TableSet> res;
        private static int icr;
        private static int numline;
        private static List<CFilter> filters;
        private static List<IBT_RuleFilterKBK_ExpenseObligationTypeT> rfkbk_eot;

        public List<TableSet> TableData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            res = new List<TableSet>();

            #region подготовка выборки данных

            var okatos = context.OKATO.Where(r => r.IdBudgetLevel.HasValue).ToList();

            var orderedBudgetLevel =
                this.BudgetLevel.Select(s => new {s, o = orderBL[s.Caption]})
                    .OrderBy(o => o.o)
                    .Select(s => s.s)
                    .ToList();

            var idsbl = orderedBudgetLevel.Select(s => s.Id);
            var lva0 = context.LimitVolumeAppropriations.Where(r =>
                                                              r.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                              r.IdBudget == IdBudget &&
                                                              r.IdVersion == IdVersion &&
                                                              r.EstimatedLine.IdKOSGU.HasValue &&
                                                              r.EstimatedLine.KOSGU.Code == "251" &&
                                                              r.IdOKATO.HasValue && //idsbl.Contains(r.OKATO.IdBudgetLevel ?? 0) &&
                                                              ((!this.ByApproved.Value) || (this.ByApproved.Value && r.DateCommit <= DateReport)) &&
                                                              !(r.HasAdditionalNeed.HasValue && r.HasAdditionalNeed.Value));

            IQueryable<LimitVolumeAppropriations> lva1;

            if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates)
            {
                lva1 = lva0.Where(r => r.IdValueType == (int) Sbor.DbEnums.ValueType.Justified);
            }
            else if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.JustificationBudget)
            {
                lva1 = lva0.Where(r => r.IdValueType == (int) Sbor.DbEnums.ValueType.JustifiedGRBS);
            }
            else if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
            {
                lva1 = lva0.Where(r =>
                                  (r.IdValueType == (int) Sbor.DbEnums.ValueType.BalancingIFDB_Estimate ||
                                   r.IdValueType == (int) Sbor.DbEnums.ValueType.Justified));
            }
            else if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceActivityOfSBP)
            {
                lva1 = lva0.Where(r =>
                                  (r.IdValueType == (int) Sbor.DbEnums.ValueType.BalancingIFDB_ActivityOfSBP ||
                                   r.IdValueType == (int) Sbor.DbEnums.ValueType.JustifiedGRBS));
            }
            else
            {
                return res;
            }

            var lva = lva1.ToList();

            #endregion подготовка выборки данных

            #region подготовка результата

            var hps = context.HierarchyPeriod.Where(r => !r.IdParent.HasValue &&
                                                         ((IsShowYear.Value && r.DateStart.Year == Budget.Year) ||
                                                         (IsShowYear1.Value && r.DateStart.Year == Budget.Year + 1) ||
                                                         (IsShowYear2.Value && r.DateStart.Year == Budget.Year + 2)));

            if (!hps.Any())
            {
                return res;
            }

            firstYear = hps.FirstOrDefault().DateStart.Year;

            var dirHP = new Dictionary<HierarchyPeriod, int>();

            int i = 0;
            foreach(var y in hps)
            {
                dirHP.Add(y, i);
                i++;
            }

            numline = 1;

            var idParentField_BranchCode = context.EntityField.FirstOrDefault(r => r.IdEntity == BranchCode.EntityIdStatic && r.Name.ToLower() == "idparent").Id;
            var idParentField_CodeSubsidy = context.EntityField.FirstOrDefault(r => r.IdEntity == CodeSubsidy.EntityIdStatic && r.Name.ToLower() == "idparent").Id;
            var idParentField_DEK = context.EntityField.FirstOrDefault(r => r.IdEntity == DEK.EntityIdStatic && r.Name.ToLower() == "idparent").Id;
            var idParentField_DKR = context.EntityField.FirstOrDefault(r => r.IdEntity == DKR.EntityIdStatic && r.Name.ToLower() == "idparent").Id;
            var idParentField_DFK = context.EntityField.FirstOrDefault(r => r.IdEntity == DFK.EntityIdStatic && r.Name.ToLower() == "idparent").Id;
            var idParentField_KCSR = context.EntityField.FirstOrDefault(r => r.IdEntity == KCSR.EntityIdStatic && r.Name.ToLower() == "idparent").Id;
            var idParentField_KOSGU = context.EntityField.FirstOrDefault(r => r.IdEntity == KOSGU.EntityIdStatic && r.Name.ToLower() == "idparent").Id;
            var idParentField_KVR = context.EntityField.FirstOrDefault(r => r.IdEntity == KVR.EntityIdStatic && r.Name.ToLower() == "idparent").Id;
            var idParentField_RZPR = context.EntityField.FirstOrDefault(r => r.IdEntity == RZPR.EntityIdStatic && r.Name.ToLower() == "idparent").Id;

            rfkbk_eot = context.IBT_RuleFilterKBK_ExpenseObligationTypeT.ToList();

            filters = context.InterBudgetaryTransfers_RuleFilterKBK.Where(r => r.IdOwner == this.Id)
                                 .ToList()
                                 .Select(s =>
                                         new CFilter()
                                             {
                                                 IdMaster = s.IdMaster,
                                                 IdFilterFieldType_BranchCode = s.IdFilterFieldType_BranchCode,
                                                 IdFilterFieldType_CodeSubsidy = s.IdFilterFieldType_CodeSubsidy,
                                                 IdFilterFieldType_DEK = s.IdFilterFieldType_DEK,
                                                 IdFilterFieldType_DFK = s.IdFilterFieldType_DFK,
                                                 IdFilterFieldType_DKR = s.IdFilterFieldType_DKR,
                                                 IdFilterFieldType_ExpenseObligationType = s.IdFilterFieldType_ExpenseObligationType,
                                                 IdFilterFieldType_FinanceSource = s.IdFilterFieldType_FinanceSource,
                                                 IdFilterFieldType_KCSR = s.IdFilterFieldType_KCSR,
                                                 IdFilterFieldType_KFO = s.IdFilterFieldType_KFO,
                                                 IdFilterFieldType_KOSGU = s.IdFilterFieldType_KOSGU,
                                                 IdFilterFieldType_KVR = s.IdFilterFieldType_KVR,
                                                 IdFilterFieldType_KVSR = s.IdFilterFieldType_KVSR,
                                                 IdFilterFieldType_RZPR = s.IdFilterFieldType_RZPR,
                                                 IdsExpenseObligationType = rfkbk_eot.Where(e => e.IdOwner == s.Id).Select(e => e.IdExpenseObligationType).ToList(),
                                                 IdsBranchCode = GetCodesWithChilds(context, s.BranchCodes.Select(c => c.Id).ToList(), idParentField_BranchCode),
                                                 IdsCodeSubsidy = GetCodesWithChilds(context, s.CodeSubsidys.Select(c => c.Id).ToList(), idParentField_CodeSubsidy),
                                                 IdsDek = GetCodesWithChilds(context, s.DEKs.Select(c => c.Id).ToList(), idParentField_DEK),
                                                 IdsDfk = GetCodesWithChilds(context, s.DFKs.Select(c => c.Id).ToList(), idParentField_DFK),
                                                 IdsDkr = GetCodesWithChilds(context, s.DKRs.Select(c => c.Id).ToList(), idParentField_DKR),
                                                 IdsFinanceSource = s.FinanceSources.Select(c => c.Id).ToList(),
                                                 IdsKcsr = GetCodesWithChilds(context, s.KCSRs.Select(c => c.Id).ToList(), idParentField_KCSR),
                                                 IdsKfo = s.KFOs.Select(c => c.Id).ToList(),
                                                 IdsKosgu = GetCodesWithChilds(context, s.KOSGUs.Select(c => c.Id).ToList(), idParentField_KOSGU),
                                                 IdsKvr = GetCodesWithChilds(context, s.KVRs.Select(c => c.Id).ToList(), idParentField_KVR),
                                                 IdsKVSR = s.KVSRs.Select(c => c.Id).ToList(),
                                                 IdsRZPR = GetCodesWithChilds(context, s.RZPRs.Select(c => c.Id).ToList(), idParentField_RZPR)
                                             })
                                 .ToList();

            var custColumns = CustomizableColumns.Where(r => r.IdOwner == this.Id).ToList();

            foreach (var budgetLevel in orderedBudgetLevel)
            {

                // отбор данных по ОКАТО с нужным Уровнем бюджета
                var lvaByBL = lva.Where(r => r.OKATO.IdBudgetLevel == budgetLevel.Id);

                //// перебор по году бюджета
                //foreach (var hp in hps)
                //{
                //    res.Add(new TableSet()
                //    {
                //        typeline = 1,
                //        numline = null,
                //        columnname = "",
                //        budgetlevel = namedBL[budgetLevel.Caption],
                //        keybudgetlevel = orderBL[budgetLevel.Caption].ToString(),
                //        keycolumnname = "",
                //        okato = "",
                //        YearHP = hp.DateStart.Year,
                //        value = null
                //    });
                //}

                foreach (var okato in okatos.Where(r => r.IdBudgetLevel == budgetLevel.Id).OrderBy(o => o.Description))
                {
                    var lvaByOkato = lvaByBL.Where(r => r.OKATO == okato).ToList();

                    var hasSumByOkato = false;
                    // перебор по году бюджета
                    foreach (var hp in hps)
                    {
                        var lvaByHp = lvaByOkato.Where(r => r.HierarchyPeriod == hp);

                        // перебираем настройки колонок (иерархически)

                        int? idfcc = null;

                        var summares0 = new List<TableSet>();
                        var computeDataByColumns = ComputeDataByColumns(context, custColumns, idfcc, lvaByHp.ToList(), hp, okato, this.UnitDimension, "", out summares0);

                        var sum = computeDataByColumns.Sum(ss => ss.value);

                        if (sum>0)
                        {
                            List<TableSet> res0 = computeDataByColumns;
                            res.AddRange(res0);
                            hasSumByOkato = true;
                        }
                    }
                    if (hasSumByOkato)
                    {
                        numline++;
                    }
                }

            }

            if (this.IsShowUnallocatedReserve.Value)
            {

                var lvaByOkato = lva.Where(r => r.OKATO.IdBudgetLevel.HasValue && !idsbl.Contains(r.OKATO.IdBudgetLevel ?? 0)).ToList();
                
                // перебор по году бюджета
                foreach (var hp in hps)
                {
                    var lvaByHp = lvaByOkato.Where(r => r.HierarchyPeriod == hp);

                    // перебираем настройки колонок (иерархически)

                    int? idfcc = null;

                    var summares0 = new List<TableSet>();
                    List<TableSet> res0 = ComputeDataByColumns(context, custColumns, idfcc, lvaByHp.ToList(), hp, null,
                                                               this.UnitDimension, "", out summares0);

                    res.AddRange(res0);
                }
                
            }

            #endregion подготовка результата


            return res;
        }

        private class CFilter
        {
            public int IdMaster;
            public byte? IdFilterFieldType_BranchCode;
            public byte? IdFilterFieldType_CodeSubsidy;
            public byte? IdFilterFieldType_DEK;
            public byte? IdFilterFieldType_DFK;
            public byte? IdFilterFieldType_DKR;
            public byte? IdFilterFieldType_ExpenseObligationType;
            public byte? IdFilterFieldType_FinanceSource;
            public byte? IdFilterFieldType_KCSR;
            public byte? IdFilterFieldType_KFO;
            public byte? IdFilterFieldType_KOSGU;
            public byte? IdFilterFieldType_KVR;
            public byte? IdFilterFieldType_KVSR;
            public byte? IdFilterFieldType_RZPR;
            public List<int> IdsBranchCode;
            public List<int> IdsCodeSubsidy;
            public List<int> IdsDek;
            public List<int> IdsDfk;
            public List<int> IdsDkr;
            public List<byte> IdsExpenseObligationType;
            public List<int> IdsFinanceSource;
            public List<int> IdsKcsr;
            public List<int> IdsKfo;
            public List<int> IdsKosgu;
            public List<int> IdsKvr;
            public List<int> IdsKVSR;
            public List<int> IdsRZPR;

        }

        private static List<TableSet> ComputeDataByColumns(DataContext context, List<InterBudgetaryTransfers_CustomizableColumns> custColumns, int? idfcc, List<LimitVolumeAppropriations> lvaByHp, HierarchyPeriod hp, OKATO okato, UnitDimension unitDimension, string key, out List<TableSet> summares)
        {
            var summares0 = new List<TableSet>();
            var summares1 = new List<TableSet>();
            List<TableSet> res0 = new List<TableSet>();
            foreach (var ccs in custColumns.Where(r => r.IdParent == idfcc).OrderBy(o => o.Order))
            {
                var keycolumnname = ccs.Order.ToString();
                //var keycolumnname = key + "." + ccs.Order.ToString();

                if (!custColumns.Any(w => w.IdParent == ccs.Id))
                {
                    //вычисления детальных данных

                    #region вычисление по заданным фильтрам

                    var filtersCust = filters.Where(r => r.IdMaster == ccs.Id);
                    List<LimitVolumeAppropriations> sss = new List<LimitVolumeAppropriations>();
                    if (filtersCust.Any())
                    {
                        foreach (var filterKbk in filtersCust)
                        {

                            List<LimitVolumeAppropriations> lvabyfilter =
                                lvaByHp.Where(
                                    r =>
                                    (!filterKbk.IdFilterFieldType_BranchCode.HasValue ||
                                     (filterKbk.IdFilterFieldType_BranchCode.Value == (byte)FilterFieldType.InList && filterKbk.IdsBranchCode.Contains(r.EstimatedLine.IdBranchCode ?? 0) ||
                                     (filterKbk.IdFilterFieldType_BranchCode.Value == (byte)FilterFieldType.Except && !filterKbk.IdsBranchCode.Contains(r.EstimatedLine.IdBranchCode ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_CodeSubsidy.HasValue ||
                                     (filterKbk.IdFilterFieldType_CodeSubsidy.Value == (byte)FilterFieldType.InList && filterKbk.IdsCodeSubsidy.Contains(r.EstimatedLine.IdCodeSubsidy ?? 0) ||
                                     (filterKbk.IdFilterFieldType_CodeSubsidy.Value == (byte)FilterFieldType.Except && !filterKbk.IdsCodeSubsidy.Contains(r.EstimatedLine.IdCodeSubsidy ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_FinanceSource.HasValue ||
                                     (filterKbk.IdFilterFieldType_FinanceSource.Value == (byte)FilterFieldType.InList && filterKbk.IdsFinanceSource.Contains(r.EstimatedLine.IdFinanceSource ?? 0) ||
                                     (filterKbk.IdFilterFieldType_FinanceSource.Value == (byte)FilterFieldType.Except && !filterKbk.IdsFinanceSource.Contains(r.EstimatedLine.IdFinanceSource ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_ExpenseObligationType.HasValue ||
                                     (filterKbk.IdFilterFieldType_ExpenseObligationType.Value == (byte)FilterFieldType.InList && filterKbk.IdsExpenseObligationType.Contains(r.EstimatedLine.IdExpenseObligationType ?? 0) ||
                                     (filterKbk.IdFilterFieldType_ExpenseObligationType.Value == (byte)FilterFieldType.Except && !filterKbk.IdsExpenseObligationType.Contains(r.EstimatedLine.IdExpenseObligationType ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_DEK.HasValue ||
                                     (filterKbk.IdFilterFieldType_DEK.Value == (byte)FilterFieldType.InList && filterKbk.IdsDek.Contains(r.EstimatedLine.IdDEK ?? 0) ||
                                     (filterKbk.IdFilterFieldType_DEK.Value == (byte)FilterFieldType.Except && !filterKbk.IdsDek.Contains(r.EstimatedLine.IdDEK ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_DFK.HasValue ||
                                     (filterKbk.IdFilterFieldType_DFK.Value == (byte)FilterFieldType.InList && filterKbk.IdsDfk.Contains(r.EstimatedLine.IdDFK ?? 0) ||
                                     (filterKbk.IdFilterFieldType_DFK.Value == (byte)FilterFieldType.Except && !filterKbk.IdsDfk.Contains(r.EstimatedLine.IdDFK ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_DKR.HasValue ||
                                     (filterKbk.IdFilterFieldType_DKR.Value == (byte)FilterFieldType.InList && filterKbk.IdsDkr.Contains(r.EstimatedLine.IdDKR ?? 0) ||
                                     (filterKbk.IdFilterFieldType_DKR.Value == (byte)FilterFieldType.Except && !filterKbk.IdsDkr.Contains(r.EstimatedLine.IdDKR ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_KFO.HasValue ||
                                     (filterKbk.IdFilterFieldType_KFO.Value == (byte)FilterFieldType.InList && filterKbk.IdsKfo.Contains(r.EstimatedLine.IdKFO ?? 0) ||
                                     (filterKbk.IdFilterFieldType_KFO.Value == (byte)FilterFieldType.Except && !filterKbk.IdsKfo.Contains(r.EstimatedLine.IdKFO ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_KVR.HasValue ||
                                     (filterKbk.IdFilterFieldType_KVR.Value == (byte)FilterFieldType.InList && filterKbk.IdsKvr.Contains(r.EstimatedLine.IdKVR ?? 0) ||
                                     (filterKbk.IdFilterFieldType_KVR.Value == (byte)FilterFieldType.Except && !filterKbk.IdsKvr.Contains(r.EstimatedLine.IdKVR ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_KCSR.HasValue ||
                                     (filterKbk.IdFilterFieldType_KCSR.Value == (byte)FilterFieldType.InList && filterKbk.IdsKcsr.Contains(r.EstimatedLine.IdKCSR ?? 0) ||
                                     (filterKbk.IdFilterFieldType_KCSR.Value == (byte)FilterFieldType.Except && !filterKbk.IdsKcsr.Contains(r.EstimatedLine.IdKCSR ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_KVSR.HasValue ||
                                     (filterKbk.IdFilterFieldType_KVSR.Value == (byte)FilterFieldType.InList && filterKbk.IdsKVSR.Contains(r.EstimatedLine.IdKVSR ?? 0) ||
                                     (filterKbk.IdFilterFieldType_KVSR.Value == (byte)FilterFieldType.Except && !filterKbk.IdsKVSR.Contains(r.EstimatedLine.IdKVSR ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_KOSGU.HasValue ||
                                     (filterKbk.IdFilterFieldType_KOSGU.Value == (byte)FilterFieldType.InList && filterKbk.IdsKosgu.Contains(r.EstimatedLine.IdKOSGU ?? 0) ||
                                     (filterKbk.IdFilterFieldType_KOSGU.Value == (byte)FilterFieldType.Except && !filterKbk.IdsKosgu.Contains(r.EstimatedLine.IdKOSGU ?? 0)))) &&
                                    (!filterKbk.IdFilterFieldType_RZPR.HasValue ||
                                     (filterKbk.IdFilterFieldType_RZPR.Value == (byte)FilterFieldType.InList && filterKbk.IdsRZPR.Contains(r.EstimatedLine.IdRZPR ?? 0) ||
                                     (filterKbk.IdFilterFieldType_RZPR.Value == (byte)FilterFieldType.Except && !filterKbk.IdsRZPR.Contains(r.EstimatedLine.IdRZPR ?? 0))))
                                     ).ToList();

                            sss = sss.Union(lvabyfilter).ToList();
                        }
                    }
                    else
                    {
                        sss = lvaByHp;
                    }

                    #endregion вычисление по заданным фильтрам

                    var resvalue = sss.Sum(s => s.Value); 
                    if (unitDimension.Symbol == "10^3 руб")
                    {
                        resvalue = resvalue/1000;
                    }
                    else if (unitDimension.Symbol == "10^6 руб")
                    {
                        resvalue = resvalue / 1000000;
                    }

                    var r0 = new TableSet()
                        {
                            typeline = 2,
                            numline = numline,
                            columnname = ccs.Caption,
                            budgetlevel = okato == null ? null : namedBL[okato.BudgetLevel.Caption],
                            keybudgetlevel = okato == null ? "9" : (orderBL[okato.BudgetLevel.Caption].ToString()),
                            keycolumnname = keycolumnname,
                            okato = okato == null ? null : okato.Description,
                            YearHP = hp.DateStart.Year,
                            value = resvalue
                        };

                    res0.Add(r0);
                    summares1.Add(r0);

                }
                else
                {
                    var res00 = ComputeDataByColumns(context, custColumns, ccs.Id, lvaByHp, hp, okato, unitDimension, keycolumnname, out summares0);
                    var resvalue = summares0.Sum(s => s.value);
                    var r0 = new TableSet()
                        {
                            typeline = 2,
                            numline = numline,
                            columnname = ccs.Caption,
                            budgetlevel = okato == null ? null : namedBL[okato.BudgetLevel.Caption],
                            keybudgetlevel = okato == null ? "9" : (orderBL[okato.BudgetLevel.Caption].ToString()),
                            keycolumnname = keycolumnname,
                            okato = okato == null ? null : okato.Description,
                            YearHP = hp.DateStart.Year,
                            value = resvalue
                        };

                    summares1.Add(r0);
                    res0.Add(r0);
                    res0.AddRange(res00);
                }
            }
            summares = summares1;
            return res0;
        }

        private static List<int> GetCodesWithChilds(DataContext context, List<int> codein, int idParentField)
        {
            List<int> codeids = new List<int>();

            foreach (var codeid in codein)
            {
                var codes0 =
                    context.Database.SqlQuery<int>("select id from dbo.GetChildrens({0}, {1}, 0)",
                                                   new object[] { codeid, idParentField}).ToList();

                codeids.AddRange(codes0);
            }
            codeids.AddRange(codein);
            return codeids;
        }
    }

}