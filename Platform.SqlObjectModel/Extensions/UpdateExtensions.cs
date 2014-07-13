using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Extensions
{
	/// <summary>
	/// Расширения для объекта UpdateStatement
	/// </summary>
	public static class UpdateExtensions
	{

        /// <summary>
        /// Добавляет в запрос выражение WHERE или происоединяет к существующему тип соединения AND
        /// </summary>
        /// <param name="updateStatement">Выражение, в которое присходит добавление</param>
        /// <param name="searchCondition">Условие поиска</param>
        /// <returns>UpdateStatement</returns>
        public static UpdateStatement Where(this UpdateStatement updateStatement, Expression searchCondition)
        {
            return Where(updateStatement, BinaryExpressionType.And, searchCondition);
        }

		/// <summary>
		/// Добавляет в запрос выражение WHERE или происоединяет к существующему
		/// </summary>
		/// <param name="updateStatement">Выражение, в которое присходит добавление</param>
		/// <param name="joinType">Тип соединения с существующим выражением (только AND или OR)</param>
		/// <param name="searchCondition">Условие поиска</param>
        /// <returns>UpdateStatement</returns>
		public static UpdateStatement Where(this UpdateStatement updateStatement, BinaryExpressionType joinType, Expression searchCondition)
		{
			if (updateStatement == null)
				throw new Exception("Where: передан пустой deleteStatement");
			if (searchCondition == null)
				throw new Exception("Where: передан пустой searchCondition");
			if (joinType != BinaryExpressionType.Or && joinType != BinaryExpressionType.And)
				throw new Exception("Where: Допускается только AND или OR");

			if (updateStatement.WhereClause == null || updateStatement.WhereClause.SearchCondition == null)
				updateStatement.WhereClause = new WhereClause { SearchCondition = searchCondition };
			else
				updateStatement.WhereClause.SearchCondition = new BinaryExpression
				{
					BinaryExpressionType = joinType,
					FirstExpression =
						updateStatement.WhereClause.
							SearchCondition,
					SecondExpression = searchCondition
				};

			return updateStatement;
		}

		/// <summary>
		/// Реализация SetClauses как выражение fieldName=@fieldName
		/// </summary>
		/// <param name="statement">Расширяемое выражение</param>
		/// <param name="fieldsName">Наименование полей</param>
		/// <returns>UpdateStatement</returns>
		public static UpdateStatement SetAsParameters(this UpdateStatement statement, List<string> fieldsName)
		{
			if (statement == null)
				throw new Exception("SetAsParameters: не указан statement");

			foreach (string fieldName in fieldsName)
			{
				statement.SetClauses.Add(new AssignmentSetClause { Column = fieldName.ToColumn(), NewValue = ("@" + fieldName).ToLiteral(LiteralType.Variable) });
			}
			return statement;
		}

		/// <summary>
		/// Реализация SetClauses как выражение fieldName = значение
		/// </summary>
		/// <param name="statement">Расширяемое выражение</param>
		/// <param name="values">Словарь "имя поля", "значение"</param>
		/// <returns>UpdateStatement</returns>
		public static UpdateStatement SetAsValues(this UpdateStatement statement, Dictionary<string, object> values)
		{
			if (statement == null)
				throw new Exception("SetAsValues: не указан statement");
			if (values == null || values.Count==0)
				throw new Exception("SetAsValues: не указан values");
			
			foreach (KeyValuePair<string, object> value in values)
			{
				statement.SetClauses.Add(new AssignmentSetClause { Column = value.Key.ToColumn(), NewValue = value.Value.ToLiteral() });
			}
			return statement;
		}
	}
}
