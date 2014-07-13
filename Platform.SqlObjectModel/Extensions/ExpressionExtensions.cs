using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Extensions
{
	/// <summary>
	/// Расширения для объекта Expression
	/// </summary>
	public static class ExpressionExtensions
	{
		#region ParenthesisExpression
		/// <summary>
		/// Оборачивает выражение скобками - (expresion)
		/// </summary>
		/// <param name="expression">Выражение</param>
		/// <returns>ParenthesisExpression</returns>
		public static ParenthesisExpression ToParenthesisExpression(this Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression", "Передано пустое 'Выражение'");

			return new ParenthesisExpression {Expression = expression};
		}

		#endregion

		#region Expression
		/// <summary>
		/// Добавляет к существующему выражению еще одно выражение соединяя их логическими операторами OR или AND
		/// </summary>
		/// <param name="expression">Выражение к которому происходит добавление</param>
		/// <param name="addExpression">Добавляемое выражение</param>
		/// <param name="joinType">Логический оператор AND или OR</param>
		/// <returns>BinaryExpression</returns>
		public static Expression AddExpression(this Expression expression, Expression addExpression, BinaryExpressionType joinType)
		{
			if (expression == null && addExpression != null)
				return addExpression;
			if (expression == null)
				throw new ArgumentNullException("expression", "Передано пустое 'Выражение к которому происходит добавление'");
			if (addExpression == null)
				throw new ArgumentNullException("expression", "Передано пустое 'Добавляемое выражение'");
			if (joinType != BinaryExpressionType.And && joinType != BinaryExpressionType.Or)
				throw new ArgumentException("Допускается только AND или OR", "joinType");

			return new BinaryExpression
			{
				FirstExpression = expression,
				SecondExpression = addExpression,
				BinaryExpressionType = joinType
			};
		}
		#endregion

		#region UnaryExpression
		/// <summary>
		/// Преобразует объект Expression в объект UnaryExpression
		/// </summary>
		/// <param name="expression">Выражение</param>
		/// <param name="unaryExpressionType">Тип UnaryExpression</param>
		/// <returns>UnaryExpression</returns>
		/// <remarks>Например можно получить выражение - NOT expression, установив параметер unaryExpressionType==UnaryExpressionType.Not</remarks>
		public static UnaryExpression ToUnaryExpression(this Expression expression, UnaryExpressionType unaryExpressionType)
		{
			if (expression == null)
				throw new ArgumentNullException("expression", "Передано пустое 'Выражение'");

			return new UnaryExpression { Expression = expression, UnaryExpressionType = unaryExpressionType };
		}
		#endregion

		#region CastCall
		/// <summary>
		/// Создание выражения CAST (column as toType)
		/// </summary>
		/// <param name="expression">Выражение</param>
		/// <param name="toType">Тип к которому происходит преобразование</param>
		/// <param name="length">Длинна</param>
		/// <returns></returns>
        public static CastCall Cast(this Expression expression, SqlDataTypeOption toType, int? length = null)
        {
            return Helper.CreateCast(expression, toType, length);
		}
		#endregion


        public static Expression IsEquals(this Expression @this, Expression other)
        {
            return Helper.CreateBinaryExpression(@this, other, BinaryExpressionType.Equals);
        }

        public static Expression IsNotEquals(this Expression @this, Expression other)
        {
            return Helper.CreateBinaryExpression(@this, other, BinaryExpressionType.NotEqualToBrackets);
        }


        public static SelectColumn ToSelectColumn(this Expression expression, string  alias)
        {
            return  Helper.CreateColumn(expression, alias);
        }

	}
}
