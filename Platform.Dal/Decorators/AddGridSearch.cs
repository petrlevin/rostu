using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Decorators.EventArguments;
using Platform.Dal.Interfaces;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Реализация декоратора поиска в Grid-е
	/// </summary>
	public class AddGridSearch : SelectDecoratorBase, IDecoratorListener, IApplyForAggregate, IOrdered
	{
		private ISelectQueryBuilder _builder;

		private readonly string _search;

		private readonly List<string> _visibleFields;

		private readonly List<EntityFieldType> _generikLinks = new List<EntityFieldType>
			{
				EntityFieldType.DocumentEntity,
				EntityFieldType.ReferenceEntity,
				EntityFieldType.TablepartEntity,
				EntityFieldType.ToolEntity,
			};

		/// <summary>
		/// Поля, которые будут учтены при поиске
		/// </summary>
		public IEnumerable<IEntityField> SourceFields { get; set; }

		/// <summary>
		/// Дефолтный конструктор
		/// </summary>
		private AddGridSearch()
		{
		}

		public AddGridSearch(string search, List<string> visibleFields)
		{
			//_search = Regex.Replace(search.Trim(), "\"|\'", "");
			_search = search.Trim().Trim(new char[] { '\"', '\'' });
			_search = _search.Replace("%", @"\%");
			_search = _search.Replace("_", @"\_");
			_search = _search.Replace(",", @".");
			_visibleFields = visibleFields;
		}

		#region Implementation of ITSqlStatementDecorator

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder == null || string.IsNullOrEmpty(_search))
				return source;

			AddCaptions addCaptions = new AddCaptions();
            AddDescriptions addDescriptions = new AddDescriptions();
			AddJoinedFields addJoinedFields = new AddJoinedFields();
			_prepare();

			SelectStatement result = source;

			QuerySpecification query = (result.QueryExpression as QuerySpecification);

			if (query==null)
				throw new Exception("Не реализовано для " + result.QueryExpression.GetType());

			List<TSqlFragment> before = query.SelectElements.ToList();
			if (!query.SelectElements.Any(a => ((a as Column) != null && (a as Column).Identifiers.Last().Value.EndsWith("_Caption", StringComparison.OrdinalIgnoreCase)) || ((a as SelectColumn) != null && ((a as SelectColumn).ColumnName as Identifier).Value.EndsWith("_Caption", StringComparison.OrdinalIgnoreCase))))
			{
				result = (SelectStatement)addCaptions.Decorate(result, queryBuilder);
                result = (SelectStatement)addDescriptions.Decorate(result, queryBuilder);
				result = (SelectStatement)addJoinedFields.Decorate(result, queryBuilder);
			}
			Expression expression = null;
			foreach (TSqlFragment selectElement in query.SelectElements)
			{
				if (selectElement is Column)
				{
					string tableAlias=(selectElement as Column).Identifiers.First().Value;
					string columnName = (selectElement as Column).Identifiers.Last().Value;
					string name = columnName;
					if (columnName.EndsWith("_Caption"))
						name = columnName.Substring(0, columnName.Length - 8);
					if (!_visibleFields.Contains(name, StringComparer.OrdinalIgnoreCase))
						continue;
					
					IEntityField entityField =
						SourceFields.SingleOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

					EntityFieldType[] number = new[]
						{
							EntityFieldType.Int,
							EntityFieldType.BigInt,
							EntityFieldType.TinyInt,
							EntityFieldType.SmallInt,
							EntityFieldType.Numeric,
                            EntityFieldType.Money
						};
					if (entityField!=null && (entityField.EntityFieldType == EntityFieldType.String || entityField.EntityFieldType==EntityFieldType.Text))
					{
						expression = expression.AddExpression(Helper.CreateLikePredicate(Helper.CreateColumn(tableAlias, name), "%" + _search + "%"),
						                                      BinaryExpressionType.Or);
					} else if (entityField!=null && number.Contains(entityField.EntityFieldType))
					{
						SchemaObjectName schemaObjectName = new SchemaObjectName();
						schemaObjectName.Identifiers.Add(new Identifier {Value = "nvarchar", QuoteType = QuoteType.NotQuoted});

						expression = expression.AddExpression(Helper.CreateLikePredicate((selectElement as Column).Cast(SqlDataTypeOption.NVarChar, 50), "%" + _search + "%"), BinaryExpressionType.Or);
					}
					/*
					else if (entityField != null)
					{
						throw new Exception("Не реализовано для " + entityField.EntityFieldType);
					}*/
				} 
				else if (selectElement is SelectColumn)
				{
					string columnName = ((selectElement as SelectColumn).ColumnName as Identifier).Value;
					string name = columnName;
					if (columnName.EndsWith("_Caption"))
						name = columnName.Substring(0, columnName.Length - 8);
					
					if (!_visibleFields.Contains(name, StringComparer.OrdinalIgnoreCase))
						continue;
					
					IEntityField entityField =
						SourceFields.SingleOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
					if (entityField != null && (entityField.EntityFieldType == EntityFieldType.Link || (entityField.IdCalculatedFieldType.HasValue && entityField.CalculatedFieldType==CalculatedFieldType.Joined)))
					{
						expression =
							expression.AddExpression(
								Helper.CreateLikePredicate((selectElement as SelectColumn).Expression, "%" + _search + "%"),
								BinaryExpressionType.Or);
					}
					if (entityField != null && _generikLinks.Contains(entityField.EntityFieldType))
					{
						expression =
							expression.AddExpression(
								Helper.CreateLikePredicate((selectElement as SelectColumn).Expression, "%" + _search + "%"),
								BinaryExpressionType.Or);
					}
				}
				/* else
				{
					throw new Exception("Не реализовано для " + selectElement.GetType());
				}*/
			}

            if (expression == null)
                return source;

			query.SelectElements.Clear();
			foreach (TSqlFragment sqlFragment in before)
			{
				query.SelectElements.Add(sqlFragment);
			}

			if ((result.QueryExpression as QuerySpecification).WhereClause==null)
			{
				(result.QueryExpression as QuerySpecification).WhereClause = new WhereClause
					{
						SearchCondition = expression.ToParenthesisExpression()
					};
			} else
			{
				(result.QueryExpression as QuerySpecification).WhereClause.SearchCondition =
					Helper.CreateBinaryExpression(
						(result.QueryExpression as QuerySpecification).WhereClause.SearchCondition.ToParenthesisExpression(),
						expression.ToParenthesisExpression(), BinaryExpressionType.And);
			}
			//this.Log(result, queryBuilder);
			return result;
		}

		#endregion

		private void _prepare()
		{
			if (SourceFields == null)
			{
				SourceFields = _builder.Entity.Fields;
			}
		}

		#region Implementation of IDecoratorListener

		public void OnDecorated(TSqlStatementDecorator sender, EventDatas eventDatas)
		{
			var list = new List<IEntityField>();

			foreach (SourceEntityFieldsEventArgs args in eventDatas.OfType<SourceEntityFieldsEventArgs>())
			{
				list.AddRange(args);
			}

			if (SourceFields == null)
				SourceFields = new List<IEntityField>();
			list = list.Where(a => !SourceFields.Any(b => b.Name.Equals(a.Name, StringComparison.OrdinalIgnoreCase))).ToList();
			SourceFields = SourceFields.Concat(list).Distinct();
		}

		#endregion

		#region Implementation of IOrdered

		/// <summary>
		/// Список декораторов, до которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> Before
		{
			get
			{
				return new List<Type>()
					{
						typeof (AddHierarcyFilter),
					};
			}
		}

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> After { get; set; }
		/*public IEnumerable<Type> After
		{
			get
			{
				return new List<Type>()
					{
						typeof (AddCaptions),
						typeof (AddJoinedFields)
					};
			}
		}*/


		/// <summary>
		/// Если декоратор хочет быть первым после указанных в списке <see cref="After"/> он должен вернуть 'true'
		/// </summary>
		public Order WantBe { get; private set; }

		#endregion
	}
}
