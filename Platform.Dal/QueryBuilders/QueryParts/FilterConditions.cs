using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Dal.Common.Interfaces;

namespace Platform.Dal.QueryBuilders.QueryParts
{
	/// <summary>
	/// Условия отбора данных. 
	/// Представляет логическое выражение.
	/// </summary>
	/// <remarks>
	/// <see cref="http://dv:8080/pages/viewpage.action?pageId=13599558">Настройка списка</see>
	/// <see cref="http://dv:8080/pages/viewpage.action?pageId=13599165">Серверные фильтры</see> (Внимание! Данный класс - это НЕ серверный фильтр, а общий).
	/// </remarks>
	public class FilterConditions : IFilterConditions
	{
		/// <summary>
		/// Возвращает конкатенация переданных условий. x AND y AND z AND ...
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static IFilterConditions Concat(params IFilterConditions[] list)
		{
			var operands = new List<IFilterConditions>();
			operands.AddRange(list.Where(fc => fc != null));

			if (operands.Count == 0)
				return null;
			if (operands.Count == 1)
				return operands[0];

			return new FilterConditions
				{
					Type = LogicOperator.And,
					Operands = operands
				};
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IFilterConditions operator &(FilterConditions first, IFilterConditions second)
        {
            return Concat(first, second);
        }

        /// <summary>
        /// создание условия на равенство поля и значения
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FilterConditions Equal(string field, object value)
        {
            return new FilterConditions()
                             {
                                 Field = field,
                                 Not = false,
                                 Operator = ComparisionOperator.Equal,
                                 Type = LogicOperator.Simple,
                                 Value = value
                             };

        }

		/// <summary>
		/// Тип выражения
		/// </summary>
		public LogicOperator Type { get; set; }

		/// <summary>
		/// Отрицание выражения.
		/// </summary>
		public bool Not { get; set; }
		
		/// <summary>
		/// Поле - левый операнд условного выражения
		/// </summary>
		public string Field { get; set; }

		/// <summary>
		/// Условие заданное текстом sql-запроса.
		/// </summary>
		public string Sql { get; set; }

		/// <summary>
		/// Бинарный оператор
		/// </summary>
		public ComparisionOperator Operator { get; set; }
		
		/// <summary>
		/// Значение. Тип значения вычисляется из типа поля. Значение может быть списком (если Operator = InList).
		/// </summary>
		public object Value { get; set; }
		
		/// <summary>
		/// Список операндов одной из логической операции И, Или, Не. 
		/// </summary>
		public List<IFilterConditions> Operands { get; set; }

		/// <summary>
		/// Преобразование FilterConditions в Expression
		/// </summary>
		/// <param name="tableAlias">Алиас таблицы</param>
		/// <returns></returns>
		public Expression ToExpression(string tableAlias="", bool createColumn = true)
		{
			Expression result = null;
			if (!(string.IsNullOrWhiteSpace(Sql)))
			{
				Subquery subQuery = Sql.ToSelectStatement().ToSubquery();
				result = Helper.CreateInPredicate(Helper.CreateColumn(tableAlias, Field), subQuery, Not);
				return result;
			}
			switch (Type)
			{
				case LogicOperator.Simple:
			        {
			            Expression column = createColumn ? Helper.CreateColumn(tableAlias, Field) : Field.ToColumn();

						switch (Operator)
						{
							case ComparisionOperator.InList:
								if (Value != null && (Value as string)!=null)
								{
                                    result = Helper.CreateInPredicate(column, (Value as string), Not);
								}
								else if (Value != null && (Value as IEnumerable) != null)
								{
                                    result = Helper.CreateInPredicate(column, (Value as IEnumerable), Not);
								}
								else
								{
									throw new Exception("Необходимо указать Value или Sql");
								}
								break;
							case ComparisionOperator.Like:
                                result = Helper.CreateLikePredicate(column, Value.ToString(), Not);
								break;
							case ComparisionOperator.IsNull:
                                result = Helper.CreateCheckIsNull(column);
								break;
							case ComparisionOperator.IsNotNull:
                                result = Helper.CreateCheckIsNull(column, false);
								break;
							case ComparisionOperator.InSameDate:
						        result = Helper.CreateBinaryExpression(
                                    Helper.CreateFunctionCall("DATEDIFF", new List<Expression>()
                                        {
                                            "day".ToLiteral(LiteralType.Variable), 
                                            Field.ToColumn(), 
                                            Value.ToLiteral()
                                        }),
						            0.ToLiteral(), 
                                    BinaryExpressionType.Equals);
						        break;
                                
                            default:
                                result = Helper.CreateBinaryExpression(column, Value.ToLiteral(), ToBinaryExpressionType(Operator));
								if (Not)
									result = result.ToUnaryExpression(UnaryExpressionType.Not);
								break;
						}
					}
					break;
				case LogicOperator.Or:
                    result = ToExpression(Operands, BinaryExpressionType.Or, tableAlias, createColumn).ToParenthesisExpression();
					if (Not)
						result = result.ToUnaryExpression(UnaryExpressionType.Not);
					break;
				case LogicOperator.And:
                    result = ToExpression(Operands, BinaryExpressionType.And, tableAlias, createColumn).ToParenthesisExpression();
					if (Not)
						result = result.ToUnaryExpression(UnaryExpressionType.Not);
					break;
			}
			return result;
		}

		/// <summary>
		/// Преобразование FilterConditions в Expression
		/// </summary>
		/// <param name="filters">Список фильтров</param>
		/// <param name="binaryExpressionType">Тип логического объединения элементов списка</param>
		/// <param name="tableAlias">Алиас таблицы</param>
		/// <returns></returns>
		private Expression ToExpression(IEnumerable<IFilterConditions> filters, BinaryExpressionType binaryExpressionType, string tableAlias = "", bool createColumn = true)
		{
			if (filters==null)
				throw new ArgumentNullException("filters");
			Expression result = null;
			//BinaryExpression result = new BinaryExpression { BinaryExpressionType = binaryExpressionType };
			foreach (FilterConditions filter in filters)
			{
				result = result.AddExpression(filter.ToExpression(tableAlias, createColumn), binaryExpressionType);
                
				/*if (result.FirstExpression == null)
				{
					result.FirstExpression = filter.ToExpression(tableAlias);
				}
				else if (result.SecondExpression == null)
				{
					result.SecondExpression = filter.ToExpression(tableAlias);
				}
				else
				{
					result = result.AddExpression(filter.ToExpression(), binaryExpressionType);
				}*/
			}
			return result;
		}

		/// <summary>
		/// Преобразование ComparisionOperator в BinaryExpressionType
		/// </summary>
		/// <param name="operation"></param>
		/// <returns></returns>
		private BinaryExpressionType ToBinaryExpressionType(ComparisionOperator operation)
		{
			switch (operation)
			{
				case ComparisionOperator.Equal:
					return BinaryExpressionType.Equals;
				case ComparisionOperator.LessOrEqual:
					return BinaryExpressionType.LessThanOrEqualTo;
				case ComparisionOperator.Less:
					return BinaryExpressionType.LessThan;
				case ComparisionOperator.GreaterOrEqual:
					return BinaryExpressionType.GreaterThanOrEqualTo;
				case ComparisionOperator.Greater:
					return BinaryExpressionType.GreaterThan;
				default:
					throw new Exception("Не реализовано для " + operation.ToString());
			}
		}

        /// <summary>
        /// Перебирает все элементы дерева условий данного объекта
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<IFilterConditions> action)
        {
            forEach(Operands, action);
        }

        private void forEach(List<IFilterConditions> operands, Action<IFilterConditions> action)
        {
            if (operands == null)
                return;

            foreach (IFilterConditions operand in operands)
            {
                action(operand);
                forEach(operand.Operands, action);
            }
        }
	}
}
