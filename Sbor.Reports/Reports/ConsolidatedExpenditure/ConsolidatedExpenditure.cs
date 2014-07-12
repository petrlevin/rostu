using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
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
using Sbor.Reports.ConsolidatedExpenditure;

namespace Sbor.Reports.Report
{
    /// <summary>
    /// Консолидированные расходы
    /// </summary>
    [Report]
    public partial class ConsolidatedExpenditure
    {
        public List<PrimeDataSet> MainData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var res = new List<PrimeDataSet>();

            res.Add(new PrimeDataSet()
            {
                year = context.HierarchyPeriod.Single(s=> s.Id == IdHierarchyPeriod).Caption,
                curDate =
                    (IsApprovedOnly.HasValue && IsApprovedOnly.Value)
                        ? ReportDate.Value.ToShortDateString()
                        : DateTime.Now.ToShortDateString(),
                UnitDimension = context.UnitDimension.Single(s=>s.Id == IdUnitDimension).Caption
            });

            return res;
        }

        public List<TableSet> TableData()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var res = new List<TableSet>();

            var query = BuildReportQuery();
            var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

            var result = dc.Database.SqlQuery<regDate>(query).ToList();

            var delimiter = (this.UnitDimension.Symbol == "10^3 руб") ? 1000
                                : ((this.UnitDimension.Symbol == "10^6 руб") ? 1000000 : 1);

            var SumData = result.Select(s=> new
                              {
                                  idRZPR = s.idRZPR,
                                  RZPRCaption = s.RZPRCaption,
                                  idKOSGU = s.idKOSGU,
                                  KOSGUCaption = s.KOSGUCaption,
                                  sn10 = (s.idKOSGU != 35 ? s.Value: 0),
                                  sn11 = (s.idKOSGU == 35 ? s.Value: 0),
                                  sn12 = (s.idBudgetLevel == -1879048162 ? s.Value: 0),
                                  sn13 = (s.idBudgetLevel == -1879048160 ? s.Value: 0),
                                  sn14 = (s.idBudgetLevel == -1879048159 ? s.Value : 0),
                                  sn15 = (s.idBudgetLevel == -1879048157 || s.idBudgetLevel == -1879048156 ? s.Value : 0),
                              }
                ).GroupBy(g => new { g.idRZPR, g.RZPRCaption, g.idKOSGU, g.KOSGUCaption }).Select(s => new
                    {
                        idRZPR = s.Key.idRZPR,
                        RZPRCaption = s.Key.RZPRCaption,
                        idKOSGU = s.Key.idKOSGU,
                        KOSGUCaption = s.Key.KOSGUCaption,
                        sn10 = s.Sum(ss => ss.sn10) / delimiter,
                        sn11 = s.Sum(ss => ss.sn11) / delimiter,
                        sn12 = s.Sum(ss => ss.sn12) / delimiter,
                        sn13 = s.Sum(ss => ss.sn13) / delimiter,
                        sn14 = s.Sum(ss => ss.sn14) / delimiter,
                        sn15 = s.Sum(ss => ss.sn15) / delimiter
                    }).ToList();

            var sum_sn10 = SumData.Sum(s => s.sn10);
            var sum_sn11 = SumData.Sum(s => s.sn11);
            var sum_sn12 = SumData.Sum(s => s.sn12);
            var sum_sn13 = SumData.Sum(s => s.sn13);
            var sum_sn14 = SumData.Sum(s => s.sn14);
            var sum_sn15 = SumData.Sum(s => s.sn15);

            var ts = new TableSet()
            {
                typeline = 1,
                col1 = "Расходы бюджета - ИТОГО, в том числе",
                col3 = "X",
                col6 = sum_sn10,
                col7 = sum_sn11, 
                col8 = sum_sn12,
                col10 = sum_sn13, 
                col11 = sum_sn14,
                col12 = sum_sn15
            };
            res.Add(ts);

            foreach (var cbr in SumData)
            {
                ts = new TableSet()
                {
                    typeline = 2,
                    col1 = cbr.KOSGUCaption,
                    col3 = "000 ",
                    col6 = cbr.sn10,
                    col7 = cbr.sn11,
                    col8 = cbr.sn12,
                    col10 = cbr.sn13,
                    col11 = cbr.sn14,
                    col12 = cbr.sn15,

                };
                res.Add(ts);
            }


            return res;
        }

        private string BuildReportQuery()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"
                        declare
                        @id int,
                        @IsApprovedOnly int,
                        @ReportDate date,
                        @IdSourcesDataReports int

                        select 
                        @id = {0},
                        @IsApprovedOnly = {1},
                        @ReportDate = '{2}',
                        @IdSourcesDataReports = {3}

                        Select EL.idRZPR, RZPR.Caption as RZPRCaption, EL.idKOSGU, KOSGU.Caption as KOSGUCaption, PLF.idBudgetLevel, LVA.Value 
                        from reg.LimitVolumeAppropriations as LVA
                        inner join tp.ConsolidatedExpenditure_PPO as PPO on 
													                        PPO.idOwner = @id and
													                        LVA.idPublicLegalFormation = PPO.idPublicLegalFormation and 
													                        LVA.idBudget = PPO.idBudget and
													                        LVA.idVersion = PPO.idVersion
                        inner join reg.TaskCollection as TC on TC.id = LVA.idTaskCollection													
                        inner join reg.TaskVolume as TV on TC.id = tv.idTaskCollection
                        inner join reg.EstimatedLine as EL on LVA.idEstimatedLine = EL.id
                        inner join ref.PublicLegalFormation as PLF on LVA.idPublicLegalFormation = PLF.id 
                        inner join ref.KOSGU as KOSGU on EL.idKOSGU = KOSGU.id 
                        inner join ref.RZPR as RZPR on EL.idRZPR = RZPR.id 
                        where 
                        (
	                        (@IsApprovedOnly = 1 and LVA.DateCommit <= @ReportDate and TV.DateCommit <= @ReportDate and (TV.DateTerminate > @ReportDate or TV.DateTerminate is null) ) or 
	                        (@IsApprovedOnly = 0 and TV.idTerminator is null)
                        ) and
                        ( 
	                        (@IdSourcesDataReports = 0 and LVA.IdValueType = 4) or 
	                        (@IdSourcesDataReports = 2 and LVA.IdValueType = 9) or 
	                        (@IdSourcesDataReports = 3 and (LVA.IdValueType = 10 or LVA.IdValueType = 4)) or 
	                        (@IdSourcesDataReports = 4 and (LVA.IdValueType = 11 or LVA.IdValueType = 9)) 
                        )
                        and LVA.HasAdditionalNeed = 0 and EL.idRZPR is not null and EL.idKOSGU is not null

            ", this.Id, this.IsApprovedOnly.Value ? 1 : 0, ReportDate.Value.ToString(new CultureInfo("en-US")), this.IdSourcesDataReports.Value);

            sb.Append(GetBaseFilter());

            return sb.ToString();
        }
    }
}
