using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.DbEnums;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.BusinessLogic.Denormalizer.Decorator;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;
using Platform.Dal.Decorators;

namespace Platform.BusinessLogic.SummaryAggregates
{
    /// <summary>
    /// Декоратор, получающий запрос, выполнение которого вернет одну запись для итоговой строки грида.
    /// </summary>
    public class AddAggregateDecorator : SelectDecoratorBase, IOrdered
    {
        public AddAggregateDecorator(IEntity entity, string hierarchyFieldName = null)
            : this(AggregatesAnalyzer.GetAggregates(entity.Id), hierarchyFieldName)
        {
        }

        private List<IAggregateInfo> aggregates;
        private string hierarchyFieldName;

        public AddAggregateDecorator(IEnumerable<IAggregateInfo> aggregates, string hierarchyFieldName = "")
        {
            if (aggregates == null) throw new ArgumentNullException("aggregates");
            this.aggregates = aggregates.ToList();
            this.hierarchyFieldName = hierarchyFieldName;
        }

        public List<string> Fields
        {
            get {return  aggregates.Select(ai => ai.Field).ToList(); }
        }

        protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
        {
            const string cteAlias = "Aggregate_cte";

            var builder = (ISelectQueryBuilder)queryBuilder;
            if (source.WithCommonTableExpressionsAndXmlNamespaces == null)
                source.WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces();

            check();
			if (!string.IsNullOrWhiteSpace(hierarchyFieldName))
			{
				AddHierarcyFilter decorator = new AddHierarcyFilter(hierarchyFieldName, false);
				source = (SelectStatement) decorator.Decorate(source, queryBuilder);
				SchemaObjectName baseSchemaObject =
					((source.QueryExpression as QuerySpecification).FromClauses[0] as SchemaObjectTableSource).SchemaObject;
				SchemaObjectName secondSchemaObject = new SchemaObjectName();
				foreach (Identifier identifier in baseSchemaObject.Identifiers)
				{
					secondSchemaObject.Identifiers.Add(identifier);
				}
				SchemaObjectTableSource secondTable = new SchemaObjectTableSource();
				secondTable.Alias = "b".ToIdentifier();
				secondTable.SchemaObject = secondSchemaObject;
				source = source.Join(QualifiedJoinType.LeftOuter, secondTable,
				                     Helper.CreateBinaryExpression("a.id".ToColumn(), ("b." + hierarchyFieldName).ToColumn(),
				                                                   BinaryExpressionType.Equals));
				source = source.Where(BinaryExpressionType.And, Helper.CreateCheckIsNull("b.id".ToColumn()));
				WhenClause when = Helper.CreateWhenClause(Helper.CreateCheckIsNull("b.id".ToColumn()), "1".ToLiteral(LiteralType.Integer));
				source.AddField(Helper.CreateColumn(Helper.CreateCaseExpression(null, new List<WhenClause> {when}, "0".ToLiteral(LiteralType.Integer)), "isLeaf"));
			}
			source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(ToCte(source, cteAlias));
			source.QueryExpression = getQuery(cteAlias, builder.AliasName);
			return source;
        }

		private void check()
        {
            //if (aggregates.Any(a => a.Function == ))
        }

        private CommonTableExpression ToCte(SelectStatement source, string cteAlias)
        {
            return source.QueryExpression.ToSubquery().ToCommonTableExpression(cteAlias);
        }

        private QuerySpecification getQuery(string cteAlias, string tableAlias)
        {
            var result = new QuerySpecification();
            result.FromClauses.Add(Helper.CreateSchemaObjectTableSource("", cteAlias, tableAlias));

            var fields = aggregates.Select(aggregateInfo => new Field()
            {
                Alias = aggregateInfo.Field,
                Experssion = Helper.CreateFunctionCall("ISNULL", new List<Expression> { getExpression(aggregateInfo, tableAlias), 0.ToLiteral() })
            }).ToList();

            result.AddFields(fields);
            return result;
        }

		private Expression getExpression(IAggregateInfo aggregateInfo, string tableAlias)
		{
			if(string.IsNullOrWhiteSpace(hierarchyFieldName))
			{
				return getExpression(aggregateInfo.Function, String.Format("{0}.{1}", tableAlias, aggregateInfo.Field).ToColumn());
			} else
			{
				switch (aggregateInfo.Function)
				{
					case AggregateFunction.Sum:
						{
							WhenClause when = Helper.CreateWhenClause(Helper.CreateBinaryExpression((tableAlias + ".isSelectable").ToColumn(), 1.ToLiteral(), BinaryExpressionType.Equals), String.Format("{0}.{1}", tableAlias, aggregateInfo.Field).ToColumn());
							return getExpression(aggregateInfo.Function, Helper.CreateCaseExpression(null, new List<WhenClause> { when }, 0.ToLiteral()));
						}
					case AggregateFunction.LeafSum:
						{
							WhenClause when = Helper.CreateWhenClause(Helper.CreateBinaryExpression((tableAlias + ".isLeaf").ToColumn(), 1.ToLiteral(), BinaryExpressionType.Equals), String.Format("{0}.{1}", tableAlias, aggregateInfo.Field).ToColumn());
							return getExpression(aggregateInfo.Function, Helper.CreateCaseExpression(null, new List<WhenClause> { when }, 0.ToLiteral()));
						}
					default:
						throw new NotImplementedException(aggregateInfo.Function.ToString());
				}
			}
		}

		private Expression getExpression(AggregateFunction func, Expression expression)
		{
			return Helper.CreateFunctionCall(getFunctionName(func), expression);
		}

        private string getFunctionName(AggregateFunction func)
        {
            switch (func)
            {
                case AggregateFunction.Sum:
                case AggregateFunction.LeafSum:
                    return "SUM";
                default:
                    throw new NotImplementedException(func.ToString());
            }
        }

        #region Implements IOrdered

        public IEnumerable<Type> Before
        {
            get { return null; }
        }

        public IEnumerable<Type> After
        {
            get { return null; }
        }

        public Order WantBe
        {
            get { return Order.Last; }
        }

        #endregion
        
    }
}
