using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Sbor.Reports.Reports.BudgetExpenseStructure;
using Sbor.Reports.Tablepart;

namespace Sbor.Reports.Report
{
	[Report]
	partial class BudgetExpenseStructure
	{
		/// <summary>
		/// При создании нового элмента заполнить ТЧ Колонки значениями по-умолчанию
		/// </summary>
		/// <param name="context"></param>
		[Control(ControlType.Insert, Sequence.Before)]
		[ControlInitial(ExcludeFromSetup = true)]
		public void AutoInsert(DataContext context)
		{
			Columns.Add(new BudgetExpenseStructure_Columns
			{
				Owner         = this,
				IdKBKEntity   = Objects.ByName<Entity>("KVSR").Id,
				Order         = 0,
				IsGroupResult = true,
				MinLevel      = 1
			});

			Columns.Add(new BudgetExpenseStructure_Columns
			{
				Owner         = this,
				IdKBKEntity   = Objects.ByName<Entity>("RZPR").Id,
				Order         = 1,
				IsGroupResult = true,
				MinLevel      = 1
			});
			
			Columns.Add(new BudgetExpenseStructure_Columns()
			{
				Owner         = this,
				IdKBKEntity   = Objects.ByName<Entity>("KCSR").Id,
				Order         = 2,
				IsGroupResult = true,
				MinLevel      = 1
			});

			Columns.Add(new BudgetExpenseStructure_Columns()
			{
				Owner         = this,
				IdKBKEntity   = Objects.ByName<Entity>("KVR").Id,
				Order         = 3,
				IsGroupResult = true,
				MinLevel      = 1
			});
		}

		protected class OrderedColumn
		{
			/// <summary>
			/// Порядок колонки 
			/// </summary>
			public int Order { get; set; }

			/// <summary>
			/// Выводить ли групировку по колонке
			/// </summary>
			public bool IsGrouped { get; set; }

			/// <summary>
			/// Является ли колонка иерархиченой
			/// </summary>
			public bool IsHierarchy { get; set; }

			/// <summary>
			/// Уровень групировки
			/// </summary>
			public int? MinLevel { get; set; }

			/// <summary>
			/// Уровень детализации
			/// </summary>
			public int? MaxLevel { get; set; }

			/// <summary>
			/// Поле описание
			/// </summary>
			public IEntityField DescriptionField{
				get { return Entity.DescriptionField ?? Entity.CaptionField; }
			}

			/// <summary>
			/// Сущность
			/// </summary>
			public IEntity Entity { get; set; }
		}

		private IEnumerable<Entity> _allColumns;
        private IEnumerable<OrderedColumn> _orderedColumns;

		private string BuildReportQuery()
		{
			var sb = new StringBuilder();
			
			var pos = 1;
		    var resultColumns = Columns.OrderBy(c => c.Order ?? 20)
									 .Select(c => new OrderedColumn
										 {
											 Entity      = c.KBKEntity,
											 Order       = pos++,
											 IsGrouped   = c.IsGroupResult,
											 IsHierarchy = c.IsGroupResult && c.KBKEntity.Fields.Any(f=>f.Name.ToUpper() == "IDPARENT"),
											 MinLevel    = c.MinLevel,
											 MaxLevel    = c.MaxLevel
										 }).ToList();

		    if (ShowActivities ?? false)
		        resultColumns.Add(new OrderedColumn
		            {
                        Entity      = Objects.ByName<Entity>("Activity"),
                        Order       = -1,
                        IsGrouped   = true,
                        IsHierarchy = false
		            });

            if (ShowProgram ?? false)
                resultColumns.Add(new OrderedColumn
                {
                    Entity = Objects.ByName<Entity>("Program"),
                    Order = -2,
                    IsGrouped = true,
                    IsHierarchy = false
                });

            if (ShowGoals ?? false)
                resultColumns.Add(new OrderedColumn
                {
                    Entity = Objects.ByName<Entity>("SystemGoalElement"),
                    Order = -3,
                    IsGrouped = true,
                    IsHierarchy = false
                });

		    _orderedColumns = resultColumns.OrderBy(c => c.Order );
		    _allColumns  = Columns.OrderBy(c => c.Order ?? 20).Select(c => c.KBKEntity).ToList();

			sb.Append(BuildWithCteParts());
			sb.Append(BuildOuterSelectPart());
			
			return sb.ToString();
		}
		
		private string BuildUnionParts()
		{
			var parts = new List<string>();

			var reportColumns = _orderedColumns.ToList();
			var columnsCount  = _orderedColumns.Count();

			bool first = true;
			do
			{
				var columns = reportColumns.Take(columnsCount).ToList();
				parts.Add(BuildInnerPart(columns, "RegValues", "", first));
				
				foreach (var column in CustomColumns)
                    parts.Add(BuildInnerPart(columns, string.Format("[{0}Values]", column.Id), column.Name + " ", first));

                first = false;

			} while (columnsCount-- > 0); 
			
			return String.Join( " Union ", parts.Where(p => !String.IsNullOrEmpty(p)) );
		}

		private string BuildInnerPart(IEnumerable<OrderedColumn> columns, string alias, string resultColumnName = "", bool isFirst = false)
		{
			var tailColumn = columns.LastOrDefault();
			
			if (!isFirst && tailColumn != null && !tailColumn.IsGrouped )
				return String.Empty;

			var isGroupColumn = tailColumn != null && tailColumn.IsHierarchy;
				
			var selectColumnString = GetSelectColumnString("R", columns, isGroupColumn);
			var groupColumnString  = GetGroupColumnString("R", columns, isGroupColumn);

			var sb = new StringBuilder(Environment.NewLine + "Select '" + resultColumnName +"' as ValueCaption, "
                                        + ((tailColumn != null && tailColumn.Entity.Name == "Program")? "1": "0") +" as isProgram, " 
										+ selectColumnString + @" R.idHierarchyPeriod, 
											SUM(R.Value) as Value " + GetRegisterFromQuery(alias));

			if (isGroupColumn)
			{
				sb.AppendFormat(@"{1}Inner Join {0}Tree {0}Tree On ({0}Tree.id = R.id{0})
									Cross Apply ( Select * From dbo.[SplitStringAsInt]({0}Tree.[path], '|')) {0}Ids ", tailColumn.Entity.Name,
								Environment.NewLine);

				if (tailColumn.MaxLevel.HasValue || tailColumn.MinLevel.HasValue)
					sb.AppendFormat(@"{1}Inner Join {0}Tree {0}TreeInner On ({0}TreeInner.id = {0}Ids.intValue )", tailColumn.Entity.Name, Environment.NewLine);
			}
            else if (tailColumn != null && tailColumn.Entity.Name == "Program")
			    sb.AppendFormat(@"{1} Inner Join [reg].[{0}] [{0}] on [{0}].id = R.id{0}
                                    Inner Join {0}Tree {0}Tree On ({0}Tree.id = R.id{0})
									Cross Apply ( Select * From dbo.[SplitStringAsInt](
                                                Case 
                                                    When
                                                        [{0}].[idDocType] = {2} Or [{0}].[idDocType] = {3}
                                                    Then 
                                                        {0}Tree.[path]
                                                    Else 
                                                        Cast([{0}].[Id] as varchar) + '|' 
                                                    End
                                    , '|')) {0}Ids 
                                    Inner Join reg.AttributeOfProgram AP on (AP.id{0} = {0}Ids.intValue And AP.idTerminator Is Null)", tailColumn.Entity.Name,
                                Environment.NewLine, Sbor.Reference.DocType.ProgramOfSBP, Sbor.Reference.DocType.MainActivity);
            else if (tailColumn != null && tailColumn.Entity.Name == "SystemGoalElement")
                sb.AppendFormat(@"{0} Inner Join {1}Tree {1}Tree On ({1}Tree.id = R.id{1})
		                            Cross Apply ( Select * From dbo.[SplitStringAsInt]( {1}Tree.[path], '|')) {1}Ids  ", Environment.NewLine, tailColumn.Entity.Name);
			
			if (tailColumn != null)
			{
				foreach (var column in _orderedColumns.Where(c => c.IsHierarchy && c.Order < tailColumn.Order))
				{
					sb.AppendFormat(Environment.NewLine + " Inner Join {0}Tree {0}Tree On ({0}Tree.id = R.id{0}) ",
									column.Entity.Name);
				}

				if (isGroupColumn)
					if (tailColumn.MaxLevel.HasValue || (tailColumn.MinLevel.HasValue && tailColumn.MinLevel.Value > 1))
					{
						sb.AppendFormat(@"{0} Where ", Environment.NewLine);

						if (tailColumn.MaxLevel.HasValue)
							sb.AppendFormat("{0}TreeInner.[level] <= {1}{2}", tailColumn.Entity.Name, tailColumn.MaxLevel.Value,
											tailColumn.MinLevel.HasValue ? " And " : String.Empty);

						if (tailColumn.MinLevel.HasValue)
							sb.AppendFormat("{0}TreeInner.[level] >= {1}", tailColumn.Entity.Name, tailColumn.MinLevel.Value);
					}
			}

			sb.Append(Environment.NewLine + " Group By " + groupColumnString + " R.idHierarchyPeriod " +
						" Having Sum(R.Value) > 0 ");
			
			return sb.ToString();
		}

		private string GetSelectColumnString(string prefix, IEnumerable<OrderedColumn> columns, bool isTailGroup)
		{
			var sb = new StringBuilder("");
            var tail = columns.LastOrDefault();

			foreach (var column in columns.Take(columns.Count() - 1).ToList())
			{
				if (column.IsHierarchy)
					sb.AppendFormat("{0}Tree.idVisibled as Id{0}, ", column.Entity.Name);
                else if (tail != null && tail.Entity.Name == "Program" && column.Entity.Name == "SystemGoalElement")
                {
                    sb.Append(" AP.idGoalSystemElement as IdSystemGoalElement, ");
                }else
					sb.AppendFormat("{0}.id{1} as Id{1}, ", prefix, column.Entity.Name);
			}
			
			if (tail != null)
			{
                if (isTailGroup || tail.Entity.Name == "Program" || tail.Entity.Name == "SystemGoalElement")
			        sb.AppendFormat("{0}Ids.intValue as Id{0}, ", tail.Entity.Name);
			    else
			    {
                    sb.AppendFormat("{0}.id{1} as Id{1}, ", prefix, tail.Entity.Name);
			    }
			}

			var emptyColumnsCount = _orderedColumns.Count() - columns.Count();
            foreach (var column in _orderedColumns.Skip(columns.Count()).Take(emptyColumnsCount))
				sb.AppendFormat("Null as Id{0}, ", column.Entity.Name);

			return sb.ToString();
		}

		private string GetGroupColumnString(string prefix, IEnumerable<OrderedColumn> columns, bool isTailGroup)
		{
			if (!columns.Any())
				return String.Empty;

			var sb = new StringBuilder("");
            var tail = columns.LastOrDefault();

			foreach (var column in columns.Take(columns.Count() - 1).ToList())
			{
				if (column.IsHierarchy)
					sb.AppendFormat("{0}Tree.idVisibled, ", column.Entity.Name);
                else if (tail != null && tail.Entity.Name == "Program" && column.Entity.Name == "SystemGoalElement")
                {
                    sb.Append(" AP.idGoalSystemElement, ");
                }
                else
					sb.AppendFormat("{0}.id{1}, ", prefix, column.Entity.Name);
			}

			if (tail != null)
			{
                if (isTailGroup || tail.Entity.Name == "Program" || tail.Entity.Name == "SystemGoalElement")
					sb.AppendFormat("{0}Ids.intValue, ", tail.Entity.Name);
				else
					sb.AppendFormat("{0}.id{1}, ", prefix, tail.Entity.Name);
			}
			return sb.ToString();
		}

		private string GetValueString()
		{
			var del = 1;

			switch (UnitDimension.Symbol)
			{
				case "руб":
					return "Cast( Result.Value as numeric(22,2) ) as Value";
				case "10^3 руб":
					del = 1000;
					break;
				case "10^6 руб":
					del = 1000000;
					break;
			}

			return "Cast( round(Result.Value/" + del + ",1) as numeric(21,1) ) as Value";
		}

		private string GetRegisterFromQuery(string alias)
		{
			return Environment.NewLine + " From " + alias + " R ";
		}
		private string GetRegisterWhereQuery()
		{
			return String.Empty; 
		}

		public List<HeaderInfo> GetHeader()
		{
			var budgetYear = Budget.Year;
			   
			var periodString = String.Empty;
			switch (PeriodOption)
			{
				case Sbor.DbEnums.PeriodOption.OFG:
					periodString = "НА " + budgetYear + " ГОД";        
					break;
				case Sbor.DbEnums.PeriodOption.PlanOFG:
					periodString = String.Format("НА {0} И ПЛАНОВЫЙ ПЕРИОД {1} И {2} ГОДОВ", budgetYear, budgetYear + 1, budgetYear + 2);        
				break;
				case Sbor.DbEnums.PeriodOption.Plan:
				periodString = String.Format("НА ПЛАНОВЫЙ ПЕРИОД {0} И {1} ГОДОВ", budgetYear + 1, budgetYear + 2);        
				break;
			}

			var unitDim = String.Empty;
			switch (UnitDimension.Symbol)
			{
				case "руб":
					unitDim = "рублей";
					break;
				case "10^3 руб":
					unitDim = "тыс. рублей";
					break;
				case "10^6 руб":
					unitDim = "млн. рублей";
					break;
			}

			var caption = String.Format("{0} {1}", String.IsNullOrEmpty(Caption) ? "СТРУКТУРА РАСХОДОВ БЮДЖЕТА" : Caption, periodString);

			return new List<HeaderInfo>
			{
				new HeaderInfo
					{
						Caption = caption,
						Unit = unitDim,
						ReportDate = (IsApprovedOnly ?? false) ? (ReportDate ?? DateTime.Now).ToShortDateString() : DateTime.Now.ToShortDateString()
					}
			};
		}

		public List<BudgetExpenseLine> GetMain()
		{
			var query = BuildReportQuery();
			var dc = IoC.Resolve<DbContext>().Cast<DataContext>();

			var result = dc.Database.SqlQuery<BudgetExpenseLine>(query).ToList();

			#region Добавляем колонки
			var budgetYear = Budget.Year;

			var years = new List<String>();
			switch (PeriodOption)
			{
				case Sbor.DbEnums.PeriodOption.OFG:
					years.Add(budgetYear.ToString());
					break;
				case Sbor.DbEnums.PeriodOption.PlanOFG:
					years.Add(budgetYear.ToString());
					years.Add((budgetYear+1).ToString());
					years.Add((budgetYear+2).ToString());
					break;
				case Sbor.DbEnums.PeriodOption.Plan:
					years.Add((budgetYear+1).ToString());
					years.Add((budgetYear+2).ToString());
					break;
			}

			var customColumns = CustomColumns.Select(c => c.Name).ToList();

			if (!result.Any())
			{
                result.AddRange(years.Select(year => new BudgetExpenseLine { Caption = "ИТОГО:", Value = 0, Year = year, Sort = Convert.ToInt32(year) }));

				result.AddRange(customColumns.SelectMany(customColumn => years, (customColumn, year) => new BudgetExpenseLine
					{
						Caption = "ИТОГО:",
						Value = 0,
						Year = customColumn + " " + year
					}));
			}
			else
			{
				foreach (var year in years)
				{
					if (result.All(l => l.Year != year))
					{
                        result.Add(new BudgetExpenseLine { Caption = "ИТОГО:", Value = 0, Year = year, Sort = Convert.ToInt32(year) });
					}
				}

				foreach (var column in customColumns)
				{
					foreach (var year in years)
					{
						var cName = column + " " + year;
						if (result.All(l => l.Year != cName))
                            result.Add(new BudgetExpenseLine { Caption = "ИТОГО:", Value = 0, Year = cName, Sort = Convert.ToInt32(year) });
					}
				}
			}

			#endregion
			
			return result;
		}

		public List<ColumnsInfo> GetColumnsInfo()
		{
			short order = 0;

			return Columns.OrderBy(c => c.Order ?? 20).ToList().Select(column => new ColumnsInfo
				{
					Name = column.KBKEntity.Name, Caption = (column.KBKEntity.Name == "KVSR" ? "КВСР" : column.KBKEntity.Caption), Order = order++
				}).ToList();
		}
	}

}
