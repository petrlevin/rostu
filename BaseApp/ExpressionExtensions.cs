using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace BaseApp
{
	/// <summary>
	/// Помошник в работе с выражениями
	/// </summary>
	public static class ExpressionExtensions
	{
		/// <summary>
		/// Преобразовать в SQL-код
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static string ToTsql(this Expression expression)
		{
			return expression._parseExpression();
		}

		private static string _parseExpression(this Expression expression)
		{
			if (expression == null)
				return null;
			switch (expression.NodeType)
			{
				case ExpressionType.Lambda:
					return expression._parseLambda();
				case ExpressionType.Equal:
					return expression._parseEqual();
				case ExpressionType.MemberAccess:
					return expression._parseMember();
				default:
					throw new NotImplementedException(expression.NodeType.ToString());
			}
		}

		private static string _parseExpression(this Expression expression, MemberInfo memberInfo)
		{
			if (expression == null)
				return null;
			switch (expression.NodeType)
			{
				case ExpressionType.Parameter:
					return expression._parseParameter(memberInfo);
				case ExpressionType.Constant:
					return expression._parseConstant(memberInfo);
				default:
					throw new NotImplementedException(expression.NodeType.ToString());
			}
		}

		private static string _parseLambda(this Expression expression)
		{
			return (expression as LambdaExpression).Body._parseExpression();
		}
		
		private static string _parseEqual(this Expression expression)
		{
			return (expression as BinaryExpression).Left._parseExpression() + " = " + (expression as BinaryExpression).Right._parseExpression();
		}

		private static string _parseMember(this Expression expression)
		{
			MemberExpression internalExpression = (expression as MemberExpression);
			return internalExpression.Expression._parseExpression(internalExpression.Member);
		}

		private static string _parseParameter(this Expression expression, MemberInfo memberInfo)
		{
			return memberInfo.Name;
		}

		private static string _parseConstant(this Expression expression, MemberInfo memberInfo)
		{
			string paramName = memberInfo.Name;
			ConstantExpression internalExpression = (expression as ConstantExpression);
			PropertyInfo property = internalExpression.Value.GetType().GetProperty(paramName);
			return Convert.ToString(property.GetValue(internalExpression.Value));
		}
	}
}
