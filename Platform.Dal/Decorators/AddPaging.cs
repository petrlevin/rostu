using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Interfaces;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Декоратор, нaкладывающий фильтр для вывода конкретной страницы из выборки
	/// </summary>
	public class AddPaging : TSqlStatementDecorator, IDecoratorListener, IOrdered
	{
		/// <summary>
		/// Описание фильтра пейджинга
		/// </summary>
		private IPaging _paging;

		/// <summary>
		/// Псевдоним для сортируемого поля.
		/// Пример: ROW_NUMBER() OVER ( ORDER BY [denormalized].[id]) AS [RowNumber],
		/// denormalized - псевдоним.
		/// </summary>
		private string _alias;

		/// <summary>
		/// Построитель запроса, откуда будет взята информация о пэйджинге
		/// </summary>
		private ISelectQueryBuilder _builder;

		/// <summary>
		/// Декоратор, нaкладывающий фильтр для вывода конкретной страницы из выборки.
		/// </summary>
		/// <param name="cteAlias"></param>
		/// <param name="paging"></param>
		public AddPaging(string cteAlias, IPaging paging)
		{
			_alias = cteAlias;
			_paging = paging;
		}

		public AddPaging()
		{
		}

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder"> </param>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder==null)
				return source;
			_prepare();

			if (string.IsNullOrEmpty(_alias))
				throw new Exception("AddPaging: передан пустой _alias");
			if (source==null)
				throw new Exception("AddPaging: передан пустой source");
			if ((source as SelectStatement) == null)
				throw new Exception("AddPaging: не реализовано для " + source.GetType().ToString());

			SelectStatement oldSelectStatement = (source as SelectStatement);
			if (oldSelectStatement.QueryExpression==null)
				throw new Exception("AddPaging: у переданого source пустой QueryExpression");
			if ((oldSelectStatement.QueryExpression as QuerySpecification)==null)
				throw new Exception("AddPaging: не реализовано для " + oldSelectStatement.QueryExpression.GetType().ToString());

			if (_paging == null)
				return source;

			OrderByClause orderByClause = oldSelectStatement.OrderByClause;
			if (orderByClause == null)
			{
				string captionFieldName = _alias + "." +
				                          (_builder.Entity.EntityType != EntityType.Multilink
					                           ? _builder.Entity.CaptionField.Name
					                           : "id");
				orderByClause = new OrderByClause();
				orderByClause.OrderByElements.Add(new ExpressionWithSortOrder {Expression = captionFieldName.ToColumn()});
			}
			(oldSelectStatement.QueryExpression as QuerySpecification).SelectElements.Add(Helper.CreateRowNumber(orderByClause, "RowNumber"));
			FunctionCall function = Helper.CreateFunctionCall("COUNT", new List<Expression> { "1".ToLiteral(LiteralType.Integer) });
			function.OverClause = new OverClause();
			(oldSelectStatement.QueryExpression as QuerySpecification).SelectElements.Add(Helper.CreateColumn(function,
																											  "_TotalRow"));
			if (oldSelectStatement.WithCommonTableExpressionsAndXmlNamespaces==null || oldSelectStatement.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Count == 0)
			{
				SelectStatement result = new SelectStatement();
				result.QueryExpression = new QuerySpecification();
				(result.QueryExpression as QuerySpecification).FromClauses.Add(oldSelectStatement.ToQueryDerivedTable("a"));

				List<TSqlFragment> fields =
					(List<TSqlFragment>) (oldSelectStatement.QueryExpression as QuerySpecification).SelectElements;

				foreach (TSqlFragment field in fields)
				{
					if (field is Column)
					{
						(result.QueryExpression as QuerySpecification).SelectElements.Add(field);
					}
					else if (field is SelectColumn)
					{
						if (!((field as SelectColumn).ColumnName as Identifier).Value.Equals("RowNumber"))
						{
							(result.QueryExpression as QuerySpecification).SelectElements.Add(
								("a." + ((field as SelectColumn).ColumnName as Identifier).Value).ToColumn());
						}
					}
				}

				result.Where(BinaryExpressionType.And,
				             Helper.CreateBetween("RowNumber".ToColumn(), _paging.Start, _paging.Start + _paging.Count - 1));

				OrderByClause orderBy = new OrderByClause();
				orderBy.OrderByElements.Add(new ExpressionWithSortOrder {Expression = "RowNumber".ToColumn()});
				result.OrderByClause = orderBy;
				return result;
			} else
			{
				CommonTableExpression commonTableExpression = new CommonTableExpression();
				commonTableExpression.ExpressionName = "AddPaging".ToIdentifier();
				commonTableExpression.Subquery = new Subquery {QueryExpression = oldSelectStatement.QueryExpression};

				List<TSqlFragment> fields =
					(List<TSqlFragment>)(oldSelectStatement.QueryExpression as QuerySpecification).SelectElements;
				QuerySpecification querySpecification = new QuerySpecification();
				querySpecification.FromClauses.Add(Helper.CreateSchemaObjectTableSource("", "AddPaging", _builder.AliasName));
				oldSelectStatement.QueryExpression = querySpecification;
				oldSelectStatement.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(commonTableExpression);

				foreach (TSqlFragment field in fields)
				{
					if (field is Column)
					{
						(oldSelectStatement.QueryExpression as QuerySpecification).SelectElements.Add(("a."+(field as Column).Identifiers.Last().Value).ToColumn());
					}
					else if (field is SelectColumn)
					{
						SelectColumn newField = (field as SelectColumn);
						if (newField.ColumnName==null && (newField.Expression is Column))
						{
							(oldSelectStatement.QueryExpression as QuerySpecification).SelectElements.Add(
								("a." + (newField.Expression as Column).Identifiers.Last().Value).ToColumn());
						}
						if (newField.ColumnName != null && !(newField.ColumnName as Identifier).Value.Equals("RowNumber"))
						{
							(oldSelectStatement.QueryExpression as QuerySpecification).SelectElements.Add(
								("a." + ((field as SelectColumn).ColumnName as Identifier).Value).ToColumn());
						}
					}
				}
				oldSelectStatement.Where(BinaryExpressionType.And,
							 Helper.CreateBetween("a.RowNumber".ToColumn(), _paging.Start, _paging.Start + _paging.Count - 1));

				OrderByClause orderBy = new OrderByClause();
				orderBy.OrderByElements.Add(new ExpressionWithSortOrder { Expression = "a.RowNumber".ToColumn() });
				oldSelectStatement.OrderByClause = orderBy;

				//commonTableExpression.Subquery

				return oldSelectStatement;
			}
			
		}

		/// <summary>
		/// Подготовка
		/// </summary>
		private void _prepare()
		{
			if (string.IsNullOrEmpty(_alias))
				_alias = !string.IsNullOrEmpty(_builder.AliasName) ? _builder.AliasName : _builder.Entity.Name;
			
			if (_paging == null)
				_paging = _builder.Paging;
		}

		#region Implementation of IDecoratorListener

		public void OnDecorated(TSqlStatementDecorator sender, EventDatas eventArgs)
		{
			//var tableAliasEventArgs = eventArgs.OfType<TableAliasEventArgs>().LastOrDefault(); // внимание! будет получено имя, переданное в последнем уведомителе
			//if (tableAliasEventArgs != null)
			//{
			//	_alias = tableAliasEventArgs.TableAlias;
			//}
		}

		#endregion

		#region Implementation of IOrdered

		/// <summary>
		/// Список декораторов, до которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> Before { get; private set; }

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		public IEnumerable<Type> After
		{
			get
			{
				return new List<Type>
					{
						typeof (AddOrder)
					};
			}
		}

		/// <summary>
		/// Если декоратор хочет быть первым после указанных в списке <see cref="After"/> он должен вернуть 'true'
		/// </summary>
		public Order WantBe { get; private set; }

		#endregion
	}
}
