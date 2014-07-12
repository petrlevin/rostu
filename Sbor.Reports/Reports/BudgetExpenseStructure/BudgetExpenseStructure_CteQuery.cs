using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Platform.BusinessLogic;
using Platform.Common;
using Sbor.Reports.Tablepart;

namespace Sbor.Reports.Report
{
   partial class BudgetExpenseStructure
	{
		private string BuildWithCteParts()
		{
			var withQueries = new List<string>() { BuildRegisterPart() };

			if (ShowProgram ?? false)
				withQueries.Add(BuildProgramPart());

			if (ShowGoals ?? false)
				withQueries.Add(BuildSystemGoalPart());

			foreach (var column in CustomColumns.ToList())
				withQueries.Add(BuildCustomPart(column));
			
			foreach (var column in _orderedColumns.Where(c => c.IsHierarchy))
				withQueries.Add(BuildWithCtePart(column));

			return "With " + String.Join(",", withQueries);
		}
		
	   private static string BuildWithCtePart(OrderedColumn column)
		{
			var withQuery = String.Format( @"{0}Tree as (
								Select R.id, 
										R.idParent, 
										R.id as idRoot, 
										Cast( cast(id as varchar) + '|' as varchar) as [path],
                                        {3}
										1 as [level],
										id as [idVisibled]
								From [{1}].[{0}] R where R.idParent Is Null
								Union All 
									Select R.id, 
											R.idParent, 
											tree.idRoot as idRoot, 
											Cast( tree.[path] + cast(R.id as varchar) + '|' as varchar) as [path],
                                            {4}
											tree.[level] + 1 as [level], {2} As [idVisibled]
									From [{1}].[{0}] R
									Inner Join {0}Tree tree on tree.id = R.idParent)", column.Entity.Name, 
                                                                                        column.Entity.Schema,
                                                                                        column.MaxLevel.HasValue ? @"
											                                                Case 
											                                                When (tree.[level] > " + (column.MaxLevel.Value - 1) + @")
												                                                Then tree.[idVisibled]
												                                                Else R.id
											                                                End " : "R.id",
                                                                                         column.Entity.Name.ToUpper() == "KCSR" ? 
                                                                                            String.Format(@"Cast( Row_Number() OVER (		
											                                                    PARTITION BY R.idParent
											                                                    Order By R.{0} ) + 100 as varchar) as [position], ", column.Entity.CaptionField.Name) : String.Empty,
                                                                                        column.Entity.Name.ToUpper() == "KCSR" ? 
                                                                                            @"Cast( tree.position + '.' + Cast( Row_Number() OVER (		
												                                                PARTITION BY R.idParent
												                                                Order By R.Code ) + 100 as varchar) as varchar) as position," : String.Empty );

			return withQuery;
		}

	   private string BuildCustomPart(BudgetExpenseStructure_CustomColumn column)
		{
			var sb = new StringBuilder();

			sb.AppendFormat(@" [{0}Values] as (
							Select * From RegValues R ", column.Id);

			sb.Append(GetCustomFilter(column));

			sb.Append(")");

			return sb.ToString();
		}

	   private string BuildProgramPart()
	   {
		   return @" ProgramTree as (
							Select P.id, 
									AP.idParent as idParent,
									P.id as idRoot,
									Cast( cast(P.id as varchar) + '|' as varchar) as [path],
									Cast( Row_Number() OVER (		
											PARTITION BY AP.idParent
											Order By AC.AnalyticalCode ) + 100 as varchar) as position
								From
									[reg].[Program] P
									Inner Join [reg].[AttributeOfProgram] AP on AP.idProgram = P.id
                                    Left Join [ref].[AnalyticalCodeStateProgram] AC on AC.id = AP.idAnalyticalCodeStateProgram
								Where 
									AP.idParent is null
									And AP.idTerminator is null
							Union All 		
								Select P.id,
									   AP.idParent as idParent,
									   T.idRoot as idRoot,
									   Cast( T.[path] + cast(P.id as varchar) + '|' as varchar) as [path],
									   Cast( T.position + '.' + Cast( Row_Number() OVER (		
											PARTITION BY AP.idParent
											Order By AC.AnalyticalCode ) + 100 as varchar) as varchar) as position
								From
									[reg].[Program] P
									Inner Join [reg].[AttributeOfProgram] AP on AP.idProgram = P.id
                                    Inner Join [ref].[AnalyticalCodeStateProgram] AC on AC.id = AP.idAnalyticalCodeStateProgram
									Inner Join ProgramTree T on T.id = AP.idParent
								Where AP.idTerminator Is Null
						) ";
	   }

	   private string BuildSystemGoalPart()
	   {
           return @"SystemGoalElementPreTree as (
							Select E.id, 
									A.idSystemGoalElement_Parent as idParent,
									E.id as idRoot,
									Cast( cast(E.id as varchar) + '|' as varchar) as [path]
								From
									[reg].[SystemGoalElement] E
									Inner Join [reg].[AttributeOfSystemGoalElement] A on A.idSystemGoalElement = E.id
								Where 
									A.idSystemGoalElement_Parent is null
									And A.idTerminator is null
							Union All 		
								Select E.id,
									   A.idSystemGoalElement_Parent as idParent,
									   T.idRoot as idRoot,
									   Cast( T.[path] + cast(E.id as varchar) + '|' as varchar) as [path]
								From
									[reg].[SystemGoalElement] E
									Inner Join [reg].[AttributeOfSystemGoalElement] A on A.idSystemGoalElement = E.id
									Inner Join SystemGoalElementPreTree T on T.id = A.idSystemGoalElement_Parent
								Where A.idTerminator Is Null
						), 
						SystemGoalElementsByRegValues as (
							Select Distinct TreeIds.intValue [Id]
							From SystemGoalElementPreTree E
								Inner Join RegValues R on R.IdSystemGoalElement = E.id
									Cross Apply (Select * from dbo.SplitStringAsInt(E.[path], '|') ) TreeIds
							Group By
								TreeIds.intValue
							Having Sum(R.Value) > 0
						)
						
						,SystemGoalElementTree as (
							Select E.idRoot as Id, 
									Null as idParent,
									E.idRoot as idRoot,
									Cast( cast(E.idRoot as varchar) + '|' as varchar) as [path],
									Cast('' as varchar) as number,
									Cast( Row_Number() OVER (		
											PARTITION BY E.idParent
											Order By E.id ) + 100 as varchar) as position
								From
									SystemGoalElementPreTree E
									Inner Join SystemGoalElementsByRegValues R on R.Id = E.id
								Where
									E.idParent Is Null
							Union All 		
								Select E.id,
									   E.idParent as idParent,
									   T.idRoot as idRoot,
									   Cast( T.[path] + cast(E.id as varchar) + '|' as varchar) as [path],
									   Cast(
										Case 
											When T.idParent Is Null 
												Then ''
												Else
													T.number + '.' 
											End
										+ Cast(Row_Number() OVER (Order By E.id ) as varchar) as varchar) as number,
									   Cast( T.position + '.' + Cast( Row_Number() OVER (		
											PARTITION BY E.idParent
											Order By E.id ) + 100 as varchar) as varchar) as position
								From
									SystemGoalElementPreTree E
									Inner Join SystemGoalElementTree T on T.id = E.idParent
									Inner Join SystemGoalElementsByRegValues V on V.id = E.id
						)";
	   }

	   private string BuildRegisterPart()
		{
			var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

			var selectQuery = new StringBuilder();
			var fromQuery = new StringBuilder();
			var whereQuery = new StringBuilder();

			selectQuery.Append(@"Select L.idHierarchyPeriod, L.Value, EL.*");

			fromQuery.Append(@" From reg.LimitVolumeAppropriations L 
												Inner Join reg.EstimatedLine EL on EL.id = L.idEstimatedLine");

			whereQuery.AppendFormat(@"{2} Where L.idBudget = {0} 
								And ISNULL(L.HasAdditionalNeed, 0) = 0 
								And L.idVersion = {1} {2}", IdBudget, IdVersion, Environment.NewLine);

            if ((ShowActivities ?? false) || (ShowProgram ?? false) || (ShowGoals ?? false))
			{
				selectQuery.Append(", TC.idActivity");
				fromQuery.AppendFormat("{0} Inner Join reg.TaskCollection TC on TC.id = L.idTaskCollection", Environment.NewLine);
			}

	       if (IdSourcesDataReports == (byte) DbEnums.SourcesDataReports.LimitBudgetAllocations)
	           fromQuery.Append(Environment.NewLine + "Inner Join ref.SBP S on S.id = EL.idSBP");
            else
                if ((ShowProgram ?? false) || (ShowGoals ?? false))
                {
                    selectQuery.Append(", TV.idProgram");

                    if (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates ||
                        IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates)
                        fromQuery.Append(Environment.NewLine + "Inner Join ref.SBP S on S.id = EL.idSBP");

                    fromQuery.AppendFormat(@"{0} 
    Inner Join reg.TaskVolume TV on (
												    TC.id = TV.idTaskCollection
												    And TV.idSBP = {1}
												    And TV.idHierarchyPeriod = L.idHierarchyPeriod
										    )", Environment.NewLine, (IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.BudgetEstimates ||
                        IdSourcesDataReports == (byte)DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates) ? " S.idParent " : " EL.idSBP ");

                    whereQuery.AppendFormat("{0} And TV.idValueType = {1}", Environment.NewLine, (byte)Sbor.DbEnums.ValueType.Plan);
                }

	       if (ShowGoals ?? false)
	       {
	           selectQuery.Append(", AP.idGoalSystemElement as IdSystemGoalElement ");

               fromQuery.AppendFormat(@"{0} Inner Join reg.Program P On P.id = TV.idProgram
	            Inner Join reg.AttributeOfProgram AP On (AP.idProgram = TV.idProgram And AP.idTerminator Is Null)", Environment.NewLine);
	       }

	       switch (PeriodOption)
			{
				case Sbor.DbEnums.PeriodOption.OFG:
					whereQuery.Append(Environment.NewLine + " And L.idHierarchyPeriod = " + Budget.Year.GetIdHierarchyPeriodYear(dc));
					break;
				case Sbor.DbEnums.PeriodOption.Plan:
					whereQuery.AppendFormat("{0} And ( L.idHierarchyPeriod = {1} Or L.idHierarchyPeriod = {2} ) ", Environment.NewLine, (Budget.Year + 1).GetIdHierarchyPeriodYear(dc), (Budget.Year + 2).GetIdHierarchyPeriodYear(dc));
					break;
				case Sbor.DbEnums.PeriodOption.PlanOFG:
					whereQuery.AppendFormat("{0} And ( L.idHierarchyPeriod = {1} Or L.idHierarchyPeriod = {2} Or L.idHierarchyPeriod = {3} ) ", Environment.NewLine, (Budget.Year).GetIdHierarchyPeriodYear(dc), (Budget.Year + 1).GetIdHierarchyPeriodYear(dc), (Budget.Year + 2).GetIdHierarchyPeriodYear(dc));
					break;
			}

			if (IsApprovedOnly ?? false)
			{
				whereQuery.AppendFormat("{1} And L.DateCommit <= '{0}' ", (ReportDate ?? DateTime.Now).ToString("s"),
										Environment.NewLine);

				if ( (ShowProgram ?? false) ||  (ShowGoals ?? false) )
				{
					whereQuery.AppendFormat("{1} And TV.DateCommit <= '{0}' And ( TV.DateTerminate Is Null Or TV.DateTerminate > '{0}' )", (ReportDate ?? DateTime.Now).ToString("s"),
											Environment.NewLine);
				}

			}
			else
			{
                if ((ShowProgram ?? false) || (ShowGoals ?? false))
					whereQuery.AppendFormat("{0} And TV.idTerminator Is Null ", Environment.NewLine);
			}

			whereQuery.Append(Environment.NewLine);

			switch (SourcesDataReports)
			{
				case DbEnums.SourcesDataReports.BudgetEstimates:
					whereQuery.AppendFormat(" And L.idValueType = {0} ", (byte)Sbor.DbEnums.ValueType.Justified);
					break;
				case DbEnums.SourcesDataReports.JustificationBudget:
					whereQuery.AppendFormat(" And L.idValueType = {0} ", (byte)Sbor.DbEnums.ValueType.JustifiedGRBS);
					break;
				case DbEnums.SourcesDataReports.InstrumentBalancingSourceEstimates:
					whereQuery.AppendFormat(" And ( L.idValueType = {0} Or L.idValueType = {1}) ", (byte)Sbor.DbEnums.ValueType.Justified, (byte)Sbor.DbEnums.ValueType.BalancingIFDB_Estimate);
					break;
				case DbEnums.SourcesDataReports.InstrumentBalancingSourceActivityOfSBP:
					whereQuery.AppendFormat(" And ( L.idValueType = {0} Or L.idValueType = {1}) ", (byte)Sbor.DbEnums.ValueType.JustifiedGRBS, (byte)Sbor.DbEnums.ValueType.BalancingIFDB_ActivityOfSBP);
					break;
                case DbEnums.SourcesDataReports.LimitBudgetAllocations:
                    whereQuery.AppendFormat(" And L.idValueType = {0} And S.IdSBPType = {1} ", (byte)Sbor.DbEnums.ValueType.Plan, (byte)Sbor.DbEnums.SBPType.GeneralManager);
                    break;
			}

			whereQuery.Append(GetBaseFilter());

			var result = "RegValues as ( " + selectQuery + fromQuery + whereQuery + " )";
			return result;
		}
	}
}
