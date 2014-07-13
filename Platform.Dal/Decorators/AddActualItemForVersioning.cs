using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Interfaces;
using Platform.Dal.Requirements;
using Platform.Log;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Decorators
{
    /// <summary>
    /// Строит выборку из версионной сущности с ограничением по указанной точке актуальности
    /// </summary>
    public class AddActualItemForVersioning : TSqlStatementDecorator, IAcceptRequirements
    {
        private ISelectQueryBuilder _builder;

        private DateTime? _actualDate;


        private List<String> _requieredSourceFields = new List<String>();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="actualDate">Дата актуальности</param>
        public AddActualItemForVersioning(DateTime? actualDate)
        {
            _actualDate = actualDate;
        }

        private AddActualItemForVersioning()
        {
            //ибо не надо
        }

        #region Implementation of ITSqlStatementDecorator

        /// <summary>
        /// Применить декоратор
        /// </summary>
        /// <param name="source">Расширяемое выражение</param>
        /// <param name="queryBuilder">Построитель</param>
        /// <returns>TSqlStatement</returns>
        protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
        {
            _builder = (queryBuilder as ISelectQueryBuilder);
            if ((_builder == null) || (_builder != null && !_builder.Entity.IsVersioning))
                return source;
            if (!(source is SelectStatement))
                throw new Exception("Не предусмотрено для " + source.GetType().ToString());
            
			SelectStatement result = (source as SelectStatement);
            //Добавление WHERE условия
            FunctionCall isNullA = Helper.CreateFunctionCall("ISNULL", new List<Expression> { (_builder.AliasName + ".ValidityFrom").ToColumn(), "01.01.0001".ToLiteral() });
			FunctionCall isNullB = Helper.CreateFunctionCall("ISNULL", new List<Expression> { "b.ValidityFrom".ToColumn(), "01.01.0001".ToLiteral() });
			if (_actualDate != null)
            {
				BinaryExpression firstExpression = Helper.CreateBinaryExpression(isNullA,
                                                                                 _actualDate.ToLiteral(),
                                                                                 BinaryExpressionType.LessThanOrEqualTo);
                result.Where(BinaryExpressionType.And, firstExpression);
            }

            
			FunctionCall isNullExpression = Helper.CreateFunctionCall("ISNULL", new List<Expression> {
					                                                 (_builder.AliasName + ".idRoot").ToColumn(),
					                                                 (_builder.AliasName + ".id").ToColumn()
				                                                 });
            FunctionCall isNullExpressionB = Helper.CreateFunctionCall("ISNULL",
                                                             new List<Expression>
				                                                 {
					                                                 ("b.idRoot").ToColumn(),
					                                                 ("b.id").ToColumn()
				                                                 });
            List<Expression> partitions = new List<Expression> { isNullExpression };
	        SelectColumn minDate =
		        Helper.CreateColumn(
			        Helper.CreateFunctionCall("MAX", isNullA,
			                                  Helper.CreateOverClause(null, partitions)), "minDate");
			result.AddFields(new List<SelectColumn> { minDate });
			if (result.WithCommonTableExpressionsAndXmlNamespaces==null)
				result.WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces();

	        CommonTableExpression commonTableExpression = new CommonTableExpression
		        {
			        ExpressionName = "BeforeActualItem".ToIdentifier(),
			        Subquery = new Subquery {QueryExpression = result.QueryExpression}
		        };


			QuerySpecification querySpecification = new QuerySpecification();
			querySpecification.FromClauses.Add(Helper.CreateSchemaObjectTableSource("", "BeforeActualItem", "a"));
            BinaryExpression firstJoinExpression = Helper.CreateBinaryExpression(isNullB, "a.minDate".ToColumn(),
                                                                                 BinaryExpressionType.Equals);

            BinaryExpression joinExpression = new BinaryExpression
                {
                    FirstExpression = firstJoinExpression.ToParenthesisExpression(),
                    SecondExpression = Helper.CreateBinaryExpression(isNullExpression, isNullExpressionB, BinaryExpressionType.Equals),
                    BinaryExpressionType = BinaryExpressionType.And
                };

			querySpecification.AddJoin(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource(_builder.Entity.Schema, _builder.Entity.Name, "b"), joinExpression);
			result.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(commonTableExpression);
			List<TSqlFragment> fields =
				(List<TSqlFragment>)(result.QueryExpression as QuerySpecification).SelectElements;
	        result.QueryExpression = querySpecification;
			foreach (TSqlFragment field in fields)
			{
				if (field is Column)
				{
					(result.QueryExpression as QuerySpecification).SelectElements.Add(field);
				}
				else if (field is SelectColumn)
				{
					(result.QueryExpression as QuerySpecification).SelectElements.Add(("a." + ((field as SelectColumn).ColumnName as Identifier).Value).ToColumn());
				}
			}
			if (_requieredSourceFields != null)
			{

				(result.QueryExpression as QuerySpecification).AddFields(_builder.AliasName, _requieredSourceFields, QueryExtensions.OnExists.Ignore);

			}
			WhenClause when =
				Helper.CreateWhenClause(
					Helper.CreateBinaryExpression("a.id".ToColumn(), "b.id".ToColumn(), BinaryExpressionType.Equals), "NULL".ToLiteral(LiteralType.Null));

			SelectColumn selectColumn = new SelectColumn
			{
				Expression = Helper.CreateCaseExpression(null, new List<WhenClause> { when }, "b.id".ToColumn()),
				ColumnName = "idActualItem".ToIdentifier()
			};
			result.AddFields(new List<SelectColumn> { selectColumn });
			//this.Log(result, queryBuilder);
            return result;
        }

        #endregion

        public IEnumerable<IRequirement> Accept(IEnumerable<IRequirement> requirements)
        {
            foreach (var result in requirements.OfType<SourceFields>())
            {
                _requieredSourceFields.AddRange(result.Fields);
            }
            _requieredSourceFields = _requieredSourceFields.Distinct().ToList();

            return requirements;

        }
    }
}
