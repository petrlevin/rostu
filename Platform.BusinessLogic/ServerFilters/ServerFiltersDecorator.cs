using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.Practices.Unity;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Decorators.EventArguments;
using Platform.Dal.Interfaces;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using System.Text.RegularExpressions;

namespace Platform.BusinessLogic.ServerFilters
{
	/// <summary>
	/// Декоратор серверного фильтра
	/// </summary>
    public class ServerFiltersDecorator : TSqlStatementDecorator, IDecoratorListener, IObservableDecorator, IApplyForAggregate
	{
		/// <summary>
		/// Идентификаторы отключенных фильтров (они не будут применены).
		/// </summary>
		public List<int> DisabledFilters { get; set; }
		
		/// <summary>
		/// Значения серверных фильтров, полученные с клиента
		/// </summary>
		public FieldValues FieldValues { get; set; }

		/// <summary>
		/// Идентификатор поля для которого создается фильтр
		/// </summary>
		public int? IdEntityField { get; set; }

		/// <summary>
		/// Фильтр
		/// </summary>
		private readonly Filter _filter = null;

		/// <summary>
		/// Декоратор серверного фильтра
		/// </summary>
		/// <param name="filter">Фильтр</param>
		public ServerFiltersDecorator(Filter filter)
		{
			_filter = filter;
			DisabledFilters = new List<int>();
		}

		/// <summary>
		/// Декоратор серверного фильтра
		/// </summary>
		public ServerFiltersDecorator(): this(null)
		{
		}

		/// <summary>
		/// Построитель запроса
		/// </summary>
		private ISelectQueryBuilder _builder;

	    private bool withParents = false;

		#region Implementation of IQueryDecorator

		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			if (IdEntityField == null)
				return source;
			_builder = (queryBuilder as ISelectQueryBuilder);
			if (_builder == null)
				return source;

			SelectStatement result = (source as SelectStatement);

			Filter filter = _filter ?? DataAccess.GetFilter.Get(IdEntityField.Value);
			IFilterConditions filterConditions = _fromFilter(filter);
			if (filterConditions == null)
				return source;  // фильтры для поля не определены

			AddWhere addWhere=new AddWhere(filterConditions, LogicOperator.And, true, true);
			result = (SelectStatement) addWhere.Decorate(result, queryBuilder);
			//this.Log(result, queryBuilder);

            if (Decorated != null)
            {
                EventDataBase args = new WithParentsEventArgs(new List<bool> { withParents });
                Decorated(args);
            }

            return result;
		}

		/// <summary>
		/// Преобразование класса Filter в FilterConditions
		/// </summary>
		/// <param name="filter">Фильтр</param>
		/// <returns></returns>
		private IFilterConditions _fromFilter(Filter filter)
		{
			if (filter == null || DisabledFilters.Contains(filter.Id))
				return null;

			IFilterConditions result = new FilterConditions();

			if (filter.LogicOperator == LogicOperator.Simple)
			{
				result = _fromSimpleFilter(filter);
			}
			else
			{
				var operands = new List<IFilterConditions>(_fromFilter(filter.ChildFilter));
				if (operands.Count == 1)
				{
					result = operands[0];
				}
				else
				{
					result.Not = filter.Not;
					result.Type = filter.LogicOperator;
					result.Operands = operands;					
				}
			}
			
			return result;
		}

		/// <summary>
		/// Преобразование списка Filter в FilterConditions
		/// </summary>
		/// <param name="filters">Список фильтров</param>
		/// <returns></returns>
		private IEnumerable<FilterConditions> _fromFilter(IEnumerable<Filter> filters)
		{
			List<FilterConditions> list = new List<FilterConditions>();
			FilterConditions condition = null;
			foreach (Filter filter in filters)
			{
				if (DisabledFilters.Contains(filter.Id))
				{
					condition = null;
				}
				else if (filter.LogicOperator == LogicOperator.Simple)
				{
					condition = _fromSimpleFilter(filter);
				}
				else
				{
					condition = new FilterConditions { Not = filter.Not, Type = filter.LogicOperator, Operands = new List<IFilterConditions>(_fromFilter(filter.ChildFilter)) };
				}
				list.Add(condition);
			}
			return list.Where(a => a != null);
		}

		/// <summary>
		/// Преобразование Filter с типом LogicOperator.Simple в FilterConditions
		/// </summary>
		/// <param name="filter">Обрабатываемый фильтр</param>
		/// <returns></returns>
		private FilterConditions _fromSimpleFilter(Filter filter)
		{
		    withParents = withParents || filter.WithParents;
			FilterConditions result = new FilterConditions();
			if (filter.IdLeftEntityField==null)
				throw new Exception("Не указано поле для левой части выражения фильтра");
			EntityField leftEntityField = Objects.ById<EntityField>(filter.IdLeftEntityField.Value);
			result.Field = leftEntityField.Name;
			result.Not = filter.Not;
			result.Type = filter.LogicOperator;
			result.Operator = filter.ComparisionOperator;
			if (!string.IsNullOrWhiteSpace(filter.RightValue))
				result.Value = filter.ComparisionOperator == ComparisionOperator.InList ? filter.RightValue : _convertFromString(leftEntityField, filter.RightValue);
			else if (!string.IsNullOrWhiteSpace(filter.RightSqlExpression))
			{
				string sql = filter.RightSqlExpression;
				if (FieldValues != null)
				{
					foreach (KeyValuePair<int, object> keyValuePair in FieldValues)
					{
						var fieldName = Objects.ById<EntityField>(keyValuePair.Key).Name;
						sql = sql.Replace(string.Format("{{{0}}}", fieldName), keyValuePair.Value.ToSqlValue());
					}
				}
				result.Operator = ComparisionOperator.InList;
				result.Sql = sql;

                checkSqlExpression(result.Sql, filter);
			}
			else if (filter.IdRightEntityField != null)
			{
			    if (!FieldValues.ContainsKey(filter.IdRightEntityField.Value))
			    {
                    throw new PlatformException(string.Format(
                        "Не указано значение для правой части выражения фильтра select * from ref.Filter where id = {0}. " +
                        "Ожидалось, что объект fieldvalues, приходящий с клиента, будет содержать значение для поля с id = {1}", 
                        filter.Id, filter.IdRightEntityField.Value
                        ));
			    }
                result.Value = FieldValues[filter.IdRightEntityField.Value];
			}
			
			return result;
		}

		private object _convertFromString(EntityField field, string value)
		{
			switch (field.EntityFieldType)
			{
				case EntityFieldType.Int:
					return Convert.ToInt32(value);
				case EntityFieldType.TinyInt:
					return Convert.ToByte(value);
				case EntityFieldType.SmallInt:
					return Convert.ToInt16(value);
				case EntityFieldType.BigInt:
					return Convert.ToInt64(value);
				case EntityFieldType.String:
				case EntityFieldType.Text:
					return value;
				case EntityFieldType.Bool:
                    return Convert.ToBoolean(value=="1"?"true":(value=="0")?"false":value);
				case EntityFieldType.Numeric:
                case EntityFieldType.Money:
					return Convert.ToDecimal(value);
				case EntityFieldType.DateTime:
					return Convert.ToDateTime(value);
				case EntityFieldType.Guid:
					return new Guid(value);
                case EntityFieldType.FileLink:
				case EntityFieldType.Link:
					if (field.EntityLink == null)
						throw new Exception("Не найдена сущность на котору ссылается поле '" + field.Name + "'");
					EntityField entityFieldId = (EntityField) field.EntityLink.Fields.SingleOrDefault(a => a.Name == "id");
					if (entityFieldId == null)
						throw new Exception("У сущности '" + field.EntityLink.Name + "' не найдено поле 'Id'");

					return _convertFromString(entityFieldId, value);
				default:
					throw new Exception("Не рализовано для " + field.EntityFieldType);
			}
		}

        private void checkSqlExpression(string sql, Filter filter)
        {
            sql = sql.TrimStart(' ', '\t');

            if (sql.Length < 6 || !sql.Substring(0, 6).Equals("select", StringComparison.OrdinalIgnoreCase))
                return;

            // здесь считаем, что sql представляет собой sql выражение на выборку

            try
            {
                sql.ToTSqlStatement();
            }
            catch (Exception ex)
            {
                Regex placeholdersRegex = new Regex(@"\{\w+\}");
                IEnumerable<string> placeholdersList = placeholdersRegex.Matches(sql)
                    .Cast<Match>()
                    .Select(a => a.Value);
                string placeholders = string.Empty;
                if (placeholdersList.Any())
                    placeholders = placeholdersList.Aggregate((a, b) => string.Format("{0}, {1}", a, b));

                throw new PlatformException(
                    string.Format("В качестве правого выражения фильтр {0} использует SQL. " +
                                  "После подстановки в это выражение значений (коллекция fieldValues, приходящая с клиента), " +
                                  "от которых зависит поле, на которое наложен данный фильтр, итоговое SQL выражение стало некорректным. " +
                                  "В итоговом SQL выражение остались незаполненными следующие плэйсхолдеры: {1}.",
                                  filter.Id, placeholders), ex);
            }
        }

		#endregion

		#region Implementation of IDecoratorListener

		public void OnDecorated(TSqlStatementDecorator sender, EventDatas eventArgs)
		{
			foreach (DisabledServerFiltersEventArgs args in eventArgs.OfType<DisabledServerFiltersEventArgs>())
			{
				DisabledFilters.AddRange(args);
			}
			DisabledFilters = DisabledFilters.Distinct().ToList();
		}

		#endregion

	    public new event OnDecoratedHandler Decorated;
	}
}
