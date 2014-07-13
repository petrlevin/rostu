using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Decorators.EventArguments;
using Platform.Dal.Interfaces;
using Platform.Log;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Common.Interfaces;
using Order = Platform.Utils.Common.Order;

namespace Platform.BusinessLogic.Denormalizer.Decorator
{
    public class DenormalizerDecorator : SelectDecoratorBase, IObservableDecorator, IOrdered, IApplyForAggregate
	{
		private ISelectQueryBuilder _builder;

		#region Public Properties

        public DenormalizedTablepartAnalyzer TpAnalyzer { get; set; }

		/// <summary>
		/// Идентификаторы периодов, соответствующие значимым колонкам денормализованной ТЧ
		/// </summary>
		public IEnumerable<int> DenormalizedPeriods { get; set; }

		#endregion


		/// <summary>
		/// Поведение по умолчанию - декоратор применяется перед добавлением условий в Where
		/// </summary>
		public DenormalizerDecorator()
		{
			Before = new List<Type> { typeof(AddWhere) };
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
		    //return source;
			check();
			_builder = (queryBuilder as ISelectQueryBuilder);

			string sourceTpAlias = _builder.AliasName; // source.GetAliasOnTable(queryBuilder.Entity.Name);
			string childTpAlias = source.NextAlias();
			string denormalizedAlias = sourceTpAlias; // "denormalized";

			joinWithChildTp(source, childTpAlias);

			var query = (QuerySpecification)source.QueryExpression;
            query.GroupByClause = getGroupByClause(query.SelectElements);
            addAggregateFields(query, childTpAlias);

            // творческая часть закончена, далее пакуем сформированный подзапрос

			query = Helper.CreateQuerySpecification(
				new QueryDerivedTable { Subquery = query.ToSubquery(), Alias = denormalizedAlias.ToIdentifier() },
				query.GetSelectedColumns()
				);

			source.QueryExpression = query;

			if (Decorated != null)
			{
                EventDataBase args = new SourceEntityFieldsEventArgs(getAggregateFields().Concat(_builder.Entity.Fields).Distinct());
				Decorated(args);
			}

			return source;
		}

		private void joinWithChildTp(SelectStatement source, string childTpAlias)
		{
			BinaryExpression masterCheck = Helper.CreateBinaryExpression(
                (childTpAlias + "." + TpAnalyzer.MasterField.Name).ToColumn(),
                (_builder.AliasName + ".id").ToColumn(),
				BinaryExpressionType.Equals);
			BinaryExpression ownersCheck = Helper.CreateBinaryExpression(
                (childTpAlias + "." + TpAnalyzer.OwnerField.Name).ToColumn(),
                (_builder.AliasName + "." + TpAnalyzer.OwnerInMasterTpField.Name).ToColumn(),
				BinaryExpressionType.Equals);

			Expression joinCondition = masterCheck.AddExpression(ownersCheck, BinaryExpressionType.And);

			source.Join(
				QualifiedJoinType.LeftOuter,
                Helper.CreateSchemaObjectTableSource(TpAnalyzer.ChildTp.Schema, TpAnalyzer.ChildTp.Name, childTpAlias),
				joinCondition
				);
		}

		/// <summary>
		/// Универсальный метод добавления полей в запрос
		/// </summary>
		/// <param name="query"></param>
		/// <param name="tableAlias"></param>
        private void addAggregateFields(QuerySpecification query, string tableAlias)
		{
			foreach (TSqlFragment selectElement in getAggregateFieldsExpr(tableAlias))
			{
				query.SelectElements.Add(selectElement);
			}
		}

		/// <summary>
		/// Инструкция GROUP BY с полями
		/// </summary>
		/// <param name="masterTableAlias"></param>
		/// <returns></returns>
		private GroupByClause getGroupByClause(IList<TSqlFragment> fields)
		{
		    if (!fields.Any())
		        return null;

			GroupByClause groupByClause = new GroupByClause();
            foreach (TSqlFragment field in fields)
			{
				ExpressionGroupingSpecification expressionGroupingSpecification = new ExpressionGroupingSpecification
					{
                        Expression = (field is SelectColumn) ? (field as SelectColumn).Expression : (Expression) field
					};
				groupByClause.GroupingSpecifications.Add(expressionGroupingSpecification);
			}
			return groupByClause;
		}

		/// <summary>
		/// Значимые поля, по одному для каждого периода
		/// </summary>
		/// <param name="childTpAlias"></param>
		/// <returns></returns>
		private IEnumerable<TSqlFragment> getAggregateFieldsExpr(string childTpAlias)
		{
			var result = new List<TSqlFragment>();
			foreach (int period in DenormalizedPeriods)
			{
			    foreach (IEntityField valueField in TpAnalyzer.ValueFields)
			    {
                    WhenClause whenThen = Helper.CreateWhenClause(
                        period.ToString().ToLiteral(LiteralType.Integer),
                        Helper.CreateColumn(childTpAlias, valueField.Name));

                    CaseExpression caseExpr = Helper.CreateCaseExpression(
                        Helper.CreateColumn(childTpAlias, TpAnalyzer.HierarchyPeriodField.Name),
                        new List<WhenClause> { whenThen },
                        ((object)null).ToLiteral());

                    SelectColumn selectColumn = new SelectColumn
                    {
                        Expression = Helper.CreateFunctionCall("SUM", new List<Expression> { caseExpr }),
                        ColumnName = DenormalizedTablepartAnalyzer.GetColumnNameBy(valueField.Name, period).ToIdentifier()
                    };

                    result.Add(selectColumn);
			    }
			}
			return result;
		}

        private IEnumerable<IEntityField> getAggregateFields()
        {
            return TpAnalyzer.GetConfiguredValueFields(DenormalizedPeriods);
        }

		/// <summary>
		/// Проверки на корректность исходных данных для декоратора
		/// </summary>
		private void check()
		{
			if (this.DenormalizedPeriods == null)
				this.DenormalizedPeriods = new List<int>();
		}

		#endregion

		#region Implementation of IObservableDecorator

		public new event OnDecoratedHandler Decorated;

		#endregion

		#region Implementation of IOrderedTSqlDecorator

		/// <summary>
		/// Список декораторов, до которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> Before { get; private set; }

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> After { get; private set; }

	    public Order WantBe
	    {
	        get { return Order.DoesNotMatter; }
	    }

	    #endregion
    }
}
