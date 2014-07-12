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
using Sbor.Reports.SummaryReportOfSelectedIndicators;

namespace Sbor.Reports.Report
{
    /// <summary>
    /// Межбюджетные трансферты
    /// </summary>
    [Report]
    public partial class SummaryReportOfSelectedIndicators
    {
        public List<PrimeDataSet> MainData()
        {
            var res = new List<PrimeDataSet>();

            res.Add(new PrimeDataSet()
                {
                    caption = Caption,
                    curDate =
                        (ByApproved.HasValue && ByApproved.Value)
                            ? DateReport.Value.ToShortDateString()
                            : DateTime.Now.ToShortDateString(),
                    udname = this.UnitDimension.Caption,
                    budgetyear = this.Budget.Year,
                    budget = this.Budget.Caption
                });

            return res;
        }

        public List<TableSet> TableData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var res = new List<TableSet>();

            List<LimitVolumeAppropriations> limitVolumeAppropriationses;
            var idSourcesDataReports = this.IdSourcesDataReports.Value;

            var volumeAppropriationses =
                context.LimitVolumeAppropriations.Where(
                    r =>
                    r.IdTaskCollection.HasValue &&
                    r.IdBudget == this.IdBudget && (!(this.ByApproved ?? false) || ((this.ByApproved ?? false) && r.DateCommit <= DateReport)) &&
                    r.IdPublicLegalFormation == this.IdPublicLegalFormation);

            if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r => 
                                                          r.IdValueType == (int)Sbor.DbEnums.ValueType.Justified &&
                                                          r.EstimatedLine.SBP.IdSBPType == (byte)Sbor.DbEnums.SBPType.TreasuryEstablishment)
                                                     .ToList();
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.JustificationBudget)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r => 
                                                          r.IdValueType == (int)Sbor.DbEnums.ValueType.JustifiedGRBS)
                                                     .ToList();
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r =>
                                                          (r.IdValueType == (int)Sbor.DbEnums.ValueType.BalancingIFDB_Estimate || r.IdValueType == (int)Sbor.DbEnums.ValueType.Justified) &&
                                                          r.EstimatedLine.SBP.IdSBPType == (byte)Sbor.DbEnums.SBPType.TreasuryEstablishment)
                                                     .ToList();
            }
            else if (idSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceActivityOfSBP)
            {
                limitVolumeAppropriationses = volumeAppropriationses.
                                                      Where(r =>
                                                          (r.IdValueType == (int)Sbor.DbEnums.ValueType.BalancingIFDB_ActivityOfSBP || r.IdValueType == (int)Sbor.DbEnums.ValueType.JustifiedGRBS))
                                                     .ToList();
            }
            else
            {
                return res;
            }

            var y1 = this.Budget.Year;
            var y2 = this.Budget.Year+1;
            var y3 = this.Budget.Year+2;

            var delimiter = (this.UnitDimension.Symbol == "10^3 руб") ? 1000
                                : ((this.UnitDimension.Symbol == "10^6 руб") ? 1000000 : 1);

            #region данные по РзПР

            var costByRZPR = limitVolumeAppropriationses.
                Where(r => r.EstimatedLine.IdRZPR.HasValue).
                Select(s => new
                    {
                        y = s.HierarchyPeriod.Year,
                        code = s.EstimatedLine.RZPR,
                        value = s.Value
                    }).
                GroupBy(g => new
                    {
                        g.y,
                        code = g.code
                    }).
                Select(s => new
                    {
                        code = s.Key.code,
                        y = s.Key.y,
                        value = s.Sum(ss => ss.value)/delimiter
                    }).
                Select(s => new
                    {
                        s.code,
                        y1 = (s.y == y1) ? s.value : 0,
                        y2 = (s.y == y2) ? s.value : 0,
                        y3 = (s.y == y3) ? s.value : 0
                    }).
                GroupBy(g => g.code).
                Select(s => new
                {
                    code = s.Key,
                    y1 = s.Sum(ss => ss.y1),
                    y2 = s.Sum(ss => ss.y2),
                    y3 = s.Sum(ss => ss.y3)
                }).
                OrderBy(o => o.code.Code);

            var sum_y1 = costByRZPR.Sum(s => s.y1);
            var sum_y2 = costByRZPR.Sum(s => s.y2);
            var sum_y3 = costByRZPR.Sum(s => s.y3);

            var ts = new TableSet()
            {
                typeline = 1,
                sn7 = "РАСХОДЫ – всего, в том числе",
                sn8 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(sum_y1, (delimiter == 1 ? 2 : 1))),
                sn9 = "100",
                sn10 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(sum_y2, (delimiter == 1 ? 2 : 1))),
                sn11 = "100",
                sn12 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(sum_y3, (delimiter == 1 ? 2 : 1))),
                sn13 = "100",
                sn14 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y1 == 0 ? 0 : Math.Round((sum_y2 / sum_y1) * 100, 2)),
                sn15 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y2 == 0 ? 0 : Math.Round((sum_y3 / sum_y2) * 100, 2))
            };
            res.Add(ts);

            foreach (var cbr in costByRZPR)
            {
                ts = new TableSet()
                    {
                        typeline = 2,
                        sn7 = cbr.code.Caption,
                        sn8 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(cbr.y1, (delimiter == 1 ? 2 : 1))),
                        sn9 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y1 == 0 ? 0 : Math.Round((cbr.y1 / sum_y1) * 100, 2)),
                        sn10 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(cbr.y2, (delimiter == 1 ? 2 : 1))),
                        sn11 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y2 == 0 ? 0 : Math.Round((cbr.y2 / sum_y2) * 100, 2)),
                        sn12 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(cbr.y3, (delimiter == 1 ? 2 : 1))),
                        sn13 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y3 == 0 ? 0 : Math.Round((cbr.y3 / sum_y3) * 100, 2)),
                        sn14 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(cbr.y1 == 0 ? 0 : Math.Round((cbr.y2 / cbr.y1) * 100, 2)),
                        sn15 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(cbr.y2 == 0 ? 0 : Math.Round((cbr.y3 / cbr.y2) * 100, 2))
                    };
                res.Add(ts);
            }

            #endregion

            ts = new TableSet()
            {
                typeline = 0
            };
            res.Add(ts);


            #region данные по КОСГУ

            var costByKOSGU = limitVolumeAppropriationses.
                Where(r => r.EstimatedLine.IdKOSGU.HasValue).
                Select(s => new
                {
                    y = s.HierarchyPeriod.Year,
                    code = s.EstimatedLine.KOSGU,
                    value = s.Value
                }).
                GroupBy(g => new
                {
                    g.y,
                    code = g.code
                }).
                Select(s => new
                {
                    code = s.Key.code,
                    y = s.Key.y,
                    value = s.Sum(ss => ss.value) / delimiter
                }).
                Select(s => new
                {
                    s.code,
                    y1 = (s.y == y1) ? s.value : 0,
                    y2 = (s.y == y2) ? s.value : 0,
                    y3 = (s.y == y3) ? s.value : 0
                }).
                GroupBy(g => g.code).
                Select(s => new
                    {
                       code = s.Key,
                       y1 = s.Sum(ss => ss.y1),
                       y2 = s.Sum(ss => ss.y2),
                       y3 = s.Sum(ss => ss.y3)
                    }).
                OrderBy(o => o.code.Code);

            sum_y1 = costByKOSGU.Sum(s => s.y1);
            sum_y2 = costByKOSGU.Sum(s => s.y2);
            sum_y3 = costByKOSGU.Sum(s => s.y3);

            ts = new TableSet()
            {
                typeline = 1,
                sn7 = "СПРАВОЧНО – расходы по КОСГУ",
                sn8 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(sum_y1, (delimiter == 1 ? 2 : 1))),
                sn9 = "100",
                sn10 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(sum_y2, (delimiter == 1 ? 2 : 1))),
                sn11 = "100",
                sn12 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(sum_y3, (delimiter == 1 ? 2 : 1))),
                sn13 = "100",
                sn14 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y1 == 0 ? 0 : Math.Round((sum_y2 / sum_y1) * 100, 2)),
                sn15 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y2 == 0 ? 0 : Math.Round((sum_y3 / sum_y2) * 100, 2))
            };
            res.Add(ts);

            foreach (var cbr in costByKOSGU)
            {
                ts = new TableSet()
                {
                    typeline = 2,
                    sn7 = cbr.code.Caption,
                    sn8 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(cbr.y1, (delimiter == 1 ? 2 : 1))),
                    sn9 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y1 == 0 ? 0 : Math.Round((cbr.y1 / sum_y1) * 100, 2)),
                    sn10 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(cbr.y2, (delimiter == 1 ? 2 : 1))),
                    sn11 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y2 == 0 ? 0 : Math.Round((cbr.y2 / sum_y2) * 100, 2)),
                    sn12 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(Math.Round(cbr.y3, (delimiter == 1 ? 2 : 1))),
                    sn13 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(sum_y3 == 0 ? 0 : Math.Round((cbr.y3 / sum_y3) * 100, 2)),
                    sn14 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(cbr.y1 == 0 ? 0 : Math.Round((cbr.y2 / cbr.y1) * 100, 2)),
                    sn15 = Sbor.Logic.DocSGEMethod.ExcludeLastZeros(cbr.y2 == 0 ? 0 : Math.Round((cbr.y3 / cbr.y2) * 100, 2))
                };
                res.Add(ts);
            }

            #endregion

            return res;
        }
    }
}