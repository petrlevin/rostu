using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Reports.AnalysisBAofDirectAndIndirectCost;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class AnalysisBAofDirectAndIndirectCost
    {
        public List<DSHeader> DataSetHeader()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var unitCaptions = new Dictionary<string, string>();
            unitCaptions.Add("383","рублей");
            unitCaptions.Add("384","тыс. рублей");
            unitCaptions.Add("385","млн. рублей");

            string unit;
            if (!unitCaptions.TryGetValue(UnitDimension.OKEICode, out unit))
            {
                unit = UnitDimension.Caption;
            }

            return new List<DSHeader>() { new DSHeader() {
                BudgetYearStart = Budget.Year,
                BudgetYearEnd = Budget.Year + 2,
                CurrentDate = (ByApproved == true && DateReport.HasValue) ? DateReport.Value : DateTime.Now,
                CapUnit = unit,
                ByActivity = ByActivity ?? false,
                BySBP = BySBP ?? false
            }};
        }

        public List<DSMain> DataSetMain()
        {
            var context = IoC.Resolve<DbContext>().Cast<DataContext>();

            string query = string.Format(
                "declare @types table (id tinyint) " +
                "insert into @types " +
                "select idActivityType from ml.AnalysisBAofDirectAndIndirectCost_ActivityType " +
                "where idAnalysisBAofDirectAndIndirectCost = {0} " +

                "declare @isFilterTypes bit " +
                "set @isFilterTypes = isnull((select top 1 1 from @types), 0) " +

                "declare @GRBSs table (id int) " +
                "insert into @GRBSs " +
                "select idSBP from ml.AnalysisBAofDirectAndIndirectCost_SBP " +
                "where idAnalysisBAofDirectAndIndirectCost = {0} " +

                "declare @isFilterGRBS bit " +
                "set @isFilterGRBS = isnull((select top 1 1 from @GRBSs), 0) " +

                "declare @SBPs table (idGRBS int, idSBP int, idSBPType int) " +
                ";with cte as ( " +
                "   select sbp.id as idGRBS, sbp.id as idSBP, sbp.idSBPType "+
                "   from ref.SBP as sbp left join @GRBSs as f on f.id = sbp.id "+
                "   where sbp.idSBPType = {1} and sbp.idPublicLegalFormation = {2} and (@isFilterGRBS = 0 or f.id is not null) "+
                "   union all " +
                "   select parent.idGRBS, child.id as idSBP, child.idSBPType " +
                "   from cte as parent inner join ref.SBP as child on child.idParent = parent.idSBP " +
                ") " +
                "insert into @SBPs " +
                "select distinct * from cte where idSBPType in ({3},{4},{5}) " +
                
                "declare @data table ( " +
	            "    CapGRBS nvarchar(max), " +
	            "    CapActivity nvarchar(max), " +
	            "    CapSBP nvarchar(max), " +
	            "    [Year] int, " +
	            "    isIndirectCosts bit, " +
	            "    isContentAsset bit,  " +
	            "    Value numeric(18,2) ) " +

                "insert into @data " +
                "select " +
                "   isnull(grbs_org.[Description],grbs_org.Caption) as CapGRBS, " +
	            (ByActivity == true ? " isnull(a.FullCaption, a.Caption) as CapActivity, " : "" ) +
	            (BySBP == true ? " isnull(sbp_org.[Description],sbp_org.Caption) as CapSBP, " : "") +
	            "	hp.[Year] as [Year], " +
	            "	lva.isIndirectCosts, " +
                (ByActivity == true ? "	case a.idActivityType when {6} then 1 else 0 end as isContentAsset,  " : "") +
	            "	sum(Value) as Value " +
                "from reg.LimitVolumeAppropriations as lva " +
                "   inner join reg.EstimatedLine as el on el.id = lva.idEstimatedLine " +
                "   inner join @SBPs as sbps on sbps.idSBP = el.idSBP " +
                "   inner join ref.SBP as grbs on grbs.id = sbps.idGRBS " +
                "   inner join ref.Organization as grbs_org on grbs_org.id = grbs.idOrganization " +
                "   inner join ref.SBP as sbp on sbp.id = sbps.idSBP " +
                "   inner join ref.Organization as sbp_org on sbp_org.id = sbp.idOrganization " +
                "   inner join reg.TaskCollection as tc on tc.id = lva.idTaskCollection " +
                "   inner join ref.Activity as a on a.id = tc.idActivity " +
                "   left join @types as ft on ft.id = a.id " +
                "   inner join ref.HierarchyPeriod as hp on hp.id = lva.idHierarchyPeriod " +
                "where lva.idBudget = {7} and lva.idVersion = {8} and isnull(lva.HasAdditionalNeed,0) = 0 " +
                (ByApproved == true && DateReport.HasValue ? string.Format(" lva.DateCommit <= '{0}' ", DateReport.Value.ToString("yyyy-MM-dd")) : "") +
                "   and (@isFilterTypes = 0 or ft.id is not null) " +
                "group by " +
                "   isnull(grbs_org.[Description],grbs_org.Caption), " +
                (ByActivity == true ? " isnull(a.FullCaption, a.Caption), " : "" ) +
                (BySBP == true ? " isnull(sbp_org.[Description],sbp_org.Caption), " : "") +
                "   hp.[Year], " +
                "   lva.isIndirectCosts " +
                (ByActivity == true ? "	,case a.idActivityType when {6} then 1 else 0 end " : "") +

                "select CapGRBS, CapActivity, CapSBP, [Year], OrdColumn, CapColumn, sum(Value) as Value from (" +
                "select CapGRBS, CapActivity, CapSBP, [Year], sum(Value) as Value, " + // Колонка Всего
                "   {9} as OrdColumn, " +
                "   '{10}' as CapColumn, " +
                "from @data " +
                "group by CapGRBS, CapActivity, CapSBP, [Year] " +
                "union all " +
                "select CapGRBS, CapActivity, CapSBP, [Year], sum(Value) as Value, " + // Колонки прямых и косвенных
                "   case isIndirectCosts when 0 then {11} else {13} end as OrdColumn, " +
                "   case isIndirectCosts when 0 then '{12}' else '{14}' end as CapColumn, " +
                "from @data " +
                "group by CapGRBS, CapActivity, CapSBP, [Year], isIndirectCosts " +
                (ByActivity == true 
                    ? "union all " +
                      "select CapGRBS, CapActivity, CapSBP, [Year], sum(Value) as Value, " + // Колонки содержание имущества
                      "   {15} as OrdColumn, " +
                      "   '{16}' as CapColumn, " +
                      "from @data " +
                      "where isContentAsset = 1 " + 
                      "group by CapGRBS, CapActivity, CapSBP, [Year] "
                    : "") +
                "union all " +
                "select k.*, y.*, null as Value, d.* " + // Это добиваем пустыми значениями незаполненные колонки в данных
                "from (select top 1 CapGRBS, CapActivity, CapSBP from @data) as k, " +
                "   (select {15} as [Year] union all select {15}+1 union all select {15}+2) as y, " +
                "   (select {9} as OrdColumn,'{10}' as CapColumn union all select {11},'{12}' " + (ByActivity == true ? "union all select {13},'{14}'" : "") + ") as d " +
                ") group by CapGRBS, CapActivity, CapSBP, [Year], OrdColumn, CapColumn "

                , 

                Id, (byte)SBPType.GeneralManager, Budget.IdPublicLegalFormation,
                (byte)SBPType.TreasuryEstablishment, (byte)SBPType.BudgetEstablishment, (byte)SBPType.IndependentEstablishment,
                (byte)ActivityType.ContentAsset,
                IdBudget, IdVersion,
                1, "Всего",
                2, "В т.ч. прямые",
                3, "В т.ч. косвенные",
                4, "В т.ч. содержание имущества",
                Budget.Year
            );

            return context.Database.SqlQuery<DSMain>(query).ToList();
        }
    }
}
