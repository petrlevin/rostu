using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Interfaces;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Реализация декоратора добавляющего фильтр по иерархии
	/// </summary>
    public class AddHierarcyFilter : TSqlStatementDecorator, IOrdered, IDecoratorListener
	{
		#region Private fields
		
        /// <summary>
		/// Значение для поля иерархии
		/// </summary>
		private List<int?> _parentFieldValues;
		
		/// <summary>
		/// Поле иерархии
		/// </summary>
		private readonly string _parentFieldName;

		private bool? _isReverse = null;

	    /// <summary>
	    /// Построитель запроса
	    /// </summary>
        private ISelectQueryBuilder _builder { get; set; }

        private SelectStatement result { get; set; }

	    private QuerySpecification resultQuerySpec
	    {
	        get { return result.QueryExpression as QuerySpecification; }
	    }

	    private bool withParents;

		private bool _withFilter = true;
	    #endregion

		#region Constructors


		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="parentFieldName">Наименование поля для построения иерархии</param>
		/// <param name="parentFieldValues">Значения для фильтра (если isReverese==true, то фильтр накладывается на поле id)</param>
		public AddHierarcyFilter(string parentFieldName, List<int?> parentFieldValues)
		{
			_parentFieldName = parentFieldName;
			_parentFieldValues = parentFieldValues;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="parentFieldName">Наименование поля для построения иерархии</param>
		/// <param name="parentFieldValues">Значения для фильтра (если isReverese==true, то фильтр накладывается на поле id)</param>
		/// <param name="isReverese">Признак направления построения иерархии(от родителя к детям - false, от детей к родителям - true)</param>
		public AddHierarcyFilter(string parentFieldName, List<int?> parentFieldValues, bool isReverese)
		{
			_parentFieldName = parentFieldName;
			_parentFieldValues = parentFieldValues;
			_isReverse = isReverese;
		}

		public AddHierarcyFilter(string parentFieldName, bool withFilter)
		{
			_parentFieldName = parentFieldName;
			_withFilter = withFilter;
		}
		#endregion


		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
		    if (!setAndCheck(source, queryBuilder))
		        return source;

            List<TSqlFragment> fields = resultQuerySpec.SelectElements.ToList(); // исходный набор полей

            if (result.WithCommonTableExpressionsAndXmlNamespaces == null)
                result.WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces();
            
            result.AddField(getIsSelectable(1.ToLiteral()));
            
            addCte("BeforeAddHierarcyFilter", result.QueryExpression.ToSubquery());

		    fields = filterFields(fields).ToList();

            addCte("AddHierarcyFilter", getAddHierarcyFilter(fields).ToSubquery());

            addCte("AfterAddHierarcyFilter", getAfterAddHierarcyFilter(fields).ToSubquery());
            
            result.QueryExpression = getResultQuery(fields);
			
            //this.Log(result, queryBuilder);
			return result;
		}

        #region query parts

        private QueryExpression getAddHierarcyFilter(List<TSqlFragment> sourceFields)
        {
            List<TSqlFragment> anchorFields = new List<TSqlFragment>(sourceFields);
            anchorFields.Add(getIsSelectableColumn());

            QuerySpecification anchorPart = Helper.CreateQuerySpecification(
                Helper.CreateSchemaObjectTableSource("", "BeforeAddHierarcyFilter", "a"), 
                anchorFields);

            List<TSqlFragment> recursiveFields =  new List<TSqlFragment>(sourceFields);
            recursiveFields.Add(getIsSelectable((withParents ? 1 : 0).ToLiteral()));

            QuerySpecification recursivePart = Helper.CreateQuerySpecification(
                Helper.CreateSchemaObjectTableSource(_builder.Entity.Schema, _builder.Entity.Name, "a"),
                recursiveFields);

            recursivePart.AddJoin(QualifiedJoinType.Inner, 
                "", "AddHierarcyFilter", "b", "a", 
                _isReverse.Value ? _parentFieldName : "id", 
                _isReverse.Value ? "id" : _parentFieldName);

            return anchorPart.Union(recursivePart, true);
        }

        private QueryExpression getAfterAddHierarcyFilter(List<TSqlFragment> fields)
        {
            QuerySpecification hierarcyQuerySpecification = new QuerySpecification();
            hierarcyQuerySpecification.FromClauses.Add(Helper.CreateSchemaObjectTableSource("", "AddHierarcyFilter", _builder.AliasName));
            GroupByClause groupByClause = new GroupByClause();
            foreach (TSqlFragment field in fields)
            {
                hierarcyQuerySpecification.SelectElements.Add(field);
                groupByClause.GroupingSpecifications.Add(new ExpressionGroupingSpecification
                {
                    Expression = (field is Column) ? (field as Column) : (field as SelectColumn).Expression
                });
            }

            hierarcyQuerySpecification.SelectElements.Add(
                getIsSelectable(Helper.CreateFunctionCall("MAX", new List<Expression> { getIsSelectableColumn() })));

            Expression parentColumn = Helper.CreateColumn(_builder.AliasName, _parentFieldName);
			if (_withFilter)
			{
				hierarcyQuerySpecification.WhereClause = new WhereClause();
				Expression whereExpression;
				if (_parentFieldValues != null)
				{
					if (_isReverse.Value)
					{
						hierarcyQuerySpecification.AddJoin(
							QualifiedJoinType.Inner,
							_builder.Entity.Schema, _builder.Entity.Name, "b", _builder.AliasName, "id", _parentFieldName);
						whereExpression = Helper.CreateInPredicate("b", "id", _parentFieldValues);
					}
					else
					{
						whereExpression = Helper.CreateInPredicate(_builder.AliasName, _parentFieldName, _parentFieldValues);
					}
				}
				else
				{
					if (_isReverse.Value)
					{
						hierarcyQuerySpecification.AddJoin(
							QualifiedJoinType.LeftOuter,
							_builder.Entity.Schema, _builder.Entity.Name, "b", _builder.AliasName, "id", _parentFieldName);
						whereExpression = Helper.CreateCheckIsNull("b.id".ToColumn());
					}
					else
					{
						whereExpression = Helper.CreateCheckIsNull(parentColumn);
					}
				}
				hierarcyQuerySpecification.WhereClause.SearchCondition = whereExpression;
			}
            hierarcyQuerySpecification.GroupByClause = groupByClause;
            return hierarcyQuerySpecification;
        }

        private QuerySpecification getResultQuery(List<TSqlFragment> fields)
        {
            var result = new QuerySpecification();
            result.FromClauses.Add(Helper.CreateSchemaObjectTableSource("", "AfterAddHierarcyFilter", "a"));
            foreach (TSqlFragment field in fields)
            {
                result.SelectElements.Add(field);
            }

            result.SelectElements.Add(getIsSelectableColumn());

            result.SelectElements.Add(Helper.CreateColumn(getIsGroupSelect(), "isGroup"));
            return result;
        }

        private Subquery getIsGroupSelect()
        {
            SelectStatement isGroupSelect = new SelectStatement();
            QuerySpecification isGroupQuery = new QuerySpecification();
            isGroupQuery.FromClauses.Add(Helper.CreateSchemaObjectTableSource(_builder.Entity.Schema, _builder.Entity.Name));
            isGroupQuery.SelectElements.Add(
                Helper.CreateCast(Helper.CreateFunctionCall("COUNT", new List<Expression> { 1.ToLiteral() }),
                                  SqlDataTypeOption.Bit));
            isGroupQuery.WhereClause = new WhereClause
            {
                SearchCondition =
                    Helper.CreateBinaryExpression((_isReverse.Value ? "id" : _parentFieldName).ToColumn(),
                                                  (_isReverse.Value ? _builder.AliasName + "." + _parentFieldName : "a.id")
                                                      .ToColumn(),
                                                  BinaryExpressionType.Equals)
            };
            isGroupSelect.QueryExpression = isGroupQuery;
            return isGroupSelect.ToSubquery();
        }

        #endregion

        #region helper methods

        private IList<TSqlFragment> filterFields(List<TSqlFragment> fields)
        {
            IList<TSqlFragment> result = new List<TSqlFragment>();

            List<string> validFields = _builder.Fields != null && _builder.Fields.Any()
                ? _builder.Fields
                : _builder.Entity.RealFields.Select(a => a.Name).ToList();

            foreach (TSqlFragment field in fields)
            {
                if (field is Column
                    && validFields.Contains((field as Column).Identifiers.Last().Value, StringComparer.OrdinalIgnoreCase)
                    )
                {
                    result.Add((_builder.AliasName + "." + (field as Column).Identifiers.Last().Value).ToColumn());
                }
                else if (field is SelectColumn)
                {
                    SelectColumn newField = (field as SelectColumn);
                    if (newField.ColumnName == null && (newField.Expression is Column))
                    {
                        result.Add((_builder.AliasName + "." + (newField.Expression as Column).Identifiers.Last().Value).ToColumn());
                    }
                    else if (newField.ColumnName != null
                        && !(newField.ColumnName as Identifier).Value.Equals("RowNumber")
                        && (validFields.Contains((newField.ColumnName as Identifier).Value, StringComparer.OrdinalIgnoreCase)))
                    {
                        result.Add((_builder.AliasName + "." + ((field as SelectColumn).ColumnName as Identifier).Value).ToColumn());
                    }
                }
            }

            return result;
        }

        private SelectColumn getIsSelectable(Expression expression)
        {
            return new SelectColumn
            {
                Expression = expression,
                ColumnName = "IsSelectable".ToIdentifier()
            };
        }

        private Column getIsSelectableColumn()
        {
            return (_builder.AliasName + ".IsSelectable").ToColumn();
        }

        private void addCte(string alias, Subquery subquery)
        {
            result.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(new CommonTableExpression()
                {
                    ExpressionName = alias.ToIdentifier(),
                    Subquery = subquery
                });
        }

	    private bool setAndCheck(TSqlStatement source, IQueryBuilder queryBuilder)
        {
            _builder = (queryBuilder as ISelectQueryBuilder);
            result = (source as SelectStatement);

            if (!_isReverse.HasValue)
            {
                _isReverse = _builder.Entity.EntityType == EntityType.Document;
            }

            return !string.IsNullOrWhiteSpace(_parentFieldName)
                   && _builder != null
                   && result != null;
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
                        typeof(AddActualItemForVersioning),
						typeof(AddCaptions),
                        typeof(AddDescriptions),
						typeof(AddJoinedFields),
						typeof(AddOrder),
						typeof(AddPaging)
					};
			}
		}

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> After { get; private set; }

		/// <summary>
		/// Если декоратор хочет быть первым после указанных в списке <see cref="After"/> он должен вернуть 'true'
		/// </summary>
		public Order WantBe { get
		{
			return Order.Last;
		}}

		#endregion

	    public void OnDecorated(TSqlStatementDecorator sender, EventDatas eventDatas)
	    {
            foreach (WithParentsEventArgs args in eventDatas.OfType<WithParentsEventArgs>())
            {
                withParents = withParents || args.Aggregate((a, b) => a || b);
            }
	    }
	}
}
