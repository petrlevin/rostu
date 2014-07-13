using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Interfaces;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Декоратор, реализующий добавление условия в конструкцию WHERE
	/// </summary>
    public class AddWhere : TSqlStatementDecorator, IApplyForAggregate
	{

	    /// <summary>
	    /// Фильтр
	    /// </summary>
        protected FilterConditions _filter { get; set; }

	    /// <summary>
		/// Оператор объединения с существующим условием
		/// </summary>
        protected readonly LogicOperator _logicOperator = LogicOperator.Or;

		/// <summary>
		/// Признак группировки существующего условие скобками
		/// </summary>
        protected readonly bool _groupExist;
		
		/// <summary>
		/// Признак группировки добавляемого условие скобками
		/// </summary>
        protected readonly bool _groupAdd;

		/// <summary>
		/// Построитель запроса, откуда будет взята информация о фильтрах
		/// </summary>
        protected IDeleteQueryBuilder _builder;

        private string _aliasName { get; set; }

		/// <summary>
		/// Декоратор, реализующий добавление условия в конструкцию WHERE
		/// </summary>
		/// <param name="filter">Добавляемый фильтр</param>
		/// <param name="logicOperator">Оператор объединения с существующим условием</param>
		/// <param name="groupExist">Группировать существующее условие скобками</param>
		/// <param name="groupAdd">Группировать добавляемое условие скобками</param>
		public AddWhere(IFilterConditions filter, LogicOperator logicOperator=LogicOperator.Or, bool groupExist=false, bool groupAdd = false)
		{
            //ToDo{CORE-1014} В большинстве случаем logicOperator указывается AND, и лишь в одном, ошибочно! OR. Следует указать значение по умолчанию - AND
			_filter = (FilterConditions)filter;
			_logicOperator = logicOperator;
			_groupExist = groupExist;
			_groupAdd = groupAdd;
		}

		public AddWhere()
		{
		}

        /// <summary>
        /// ToDo{CORE-1014} убрать!
        /// </summary>
	    protected bool _createColumn = true;

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as IDeleteQueryBuilder);
			
			prepare();

			if (_filter == null)
				return source;

			if (source == null)
				throw new Exception("AddWhere.Decorate: передан пустой source");

			if (_logicOperator != LogicOperator.Or && _logicOperator != LogicOperator.And)
				throw new Exception("AddWhere: допустимые значения OR или AND");


			if ((source as SelectStatement) != null)
			{
				if (_builder!=null)
					_aliasName = (source as SelectStatement).GetAliasOnTable(_builder.Entity.Name, true);
			    if ((String.IsNullOrWhiteSpace(_aliasName)) && (_builder is ISelectQueryBuilder))
			        _aliasName = ((ISelectQueryBuilder) _builder).AliasName;
				if ((source as SelectStatement).QueryExpression == null)
					throw new Exception("AddWhere.Decorate: передан source c пустым QueryExpression");

				QuerySpecification querySpecification =
					(source as SelectStatement).QueryExpression as QuerySpecification;
				if (querySpecification == null)
					throw new Exception("AddWhere.Decorate: Не реализовано для " +
										(source as SelectStatement).QueryExpression.GetType().ToString());

			    if (querySpecification.WhereClause == null)
				{
					querySpecification.WhereClause = new WhereClause
						{
                            SearchCondition = _filter.ToExpression(_aliasName, _createColumn)
						};
				}
				else
				{
					BinaryExpression newWhere = new BinaryExpression();
					Expression first =
						querySpecification.WhereClause.SearchCondition;
					if (_groupExist)
						first = first.ToParenthesisExpression();
                    Expression second = _filter.ToExpression(_aliasName, _createColumn);
					if (_groupAdd)
						second = second.ToParenthesisExpression();
					newWhere.FirstExpression = first;
					newWhere.SecondExpression = second;
					newWhere.BinaryExpressionType = _logicOperator == LogicOperator.And
														? BinaryExpressionType.And
														: BinaryExpressionType.Or;
					querySpecification.WhereClause.SearchCondition =
						newWhere;
				}
				(source as SelectStatement).QueryExpression = querySpecification;
			}
			if ((source as UpdateStatement) != null)
			{
				if ((source as UpdateStatement).WhereClause == null)
				{
					(source as UpdateStatement).WhereClause = new WhereClause
					{
						SearchCondition = _filter.ToExpression()
					};
				}
				else
				{
					BinaryExpression newWhere = new BinaryExpression();
					Expression first =
						(source as UpdateStatement).WhereClause.SearchCondition;
					if (_groupExist)
						first = first.ToParenthesisExpression();
					Expression second = _filter.ToExpression();
					if (_groupAdd)
						second = second.ToParenthesisExpression();
					newWhere.FirstExpression = first;
					newWhere.SecondExpression = second;
					newWhere.BinaryExpressionType = _logicOperator == LogicOperator.And
														? BinaryExpressionType.And
														: BinaryExpressionType.Or;
					(source as UpdateStatement).WhereClause.SearchCondition =
						newWhere;
				}
			}
			if ((source as DeleteStatement) != null)
			{
				if ((source as DeleteStatement).WhereClause == null)
				{
					(source as DeleteStatement).WhereClause = new WhereClause
					{
						SearchCondition = _filter.ToExpression()
					};
				}
				else
				{
					BinaryExpression newWhere = new BinaryExpression();
					Expression first =
						(source as DeleteStatement).WhereClause.SearchCondition;
					if (_groupExist)
						first = first.ToParenthesisExpression();
					Expression second = _filter.ToExpression();
					if (_groupAdd)
						second = second.ToParenthesisExpression();
					newWhere.FirstExpression = first;
					newWhere.SecondExpression = second;
					newWhere.BinaryExpressionType = _logicOperator == LogicOperator.And
														? BinaryExpressionType.And
														: BinaryExpressionType.Or;
					(source as DeleteStatement).WhereClause.SearchCondition =
						newWhere;
				}
			}

			return source;
		}

	    private void prepare()
		{
			if (_filter == null)
				_filter = (FilterConditions) _builder.Conditions;
		}


	}
}
