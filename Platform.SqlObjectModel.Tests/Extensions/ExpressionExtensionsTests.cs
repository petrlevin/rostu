using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests.Extensions
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class ExpressionExtensionsTests : BaseFixture
	{
		#region Expression
		/// <summary>
		/// Тест метода AddExpression(this Expression expression, Expression addExpression, BinaryExpressionType binaryExpressionType) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра expression == null</remarks>
		[Test]
		public void AddExpressionExpressionException()
		{
			Expression expression = null;
			Assert.Throws<ArgumentNullException>(() => expression.AddExpression(null, BinaryExpressionType.And));
		}

		/// <summary>
		/// Тест метода AddExpression(this Expression expression, Expression addExpression, BinaryExpressionType binaryExpressionType) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра addExpression == null</remarks>
		[Test]
		public void AddExpressionAddExpressionException()
		{
			Expression expression = new BinaryExpression();
			Assert.Throws<ArgumentNullException>(() => expression.AddExpression(null, BinaryExpressionType.And));
		}

		/// <summary>
		/// Тест метода AddExpression(this Expression expression, Expression addExpression, BinaryExpressionType binaryExpressionType) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра joinType != BinaryExpressionType.And && joinType != BinaryExpressionType.Or</remarks>
		[Test]
		public void AddExpressionJoinTypeException()
		{
			Expression expression = new BinaryExpression();
			Assert.Throws<ArgumentException>(() => expression.AddExpression(new BinaryExpression(), BinaryExpressionType.Equals));
		}

		/// <summary>
		/// Тест метода AddExpression(this Expression expression, Expression addExpression, BinaryExpressionType binaryExpressionType)
		/// </summary>
		/// <remarks>expression == null && addExpression != null</remarks>
		[Test]
		public void AddExpressionNullExpression()
		{
			Expression expression = null;
			Expression addExpression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																			  BinaryExpressionType.Equals);
			expression = expression.AddExpression(addExpression, BinaryExpressionType.Or);
			Assert.AreEqual("[field] = 1", expression.Render(options));
		}

		/// <summary>
		/// Тест метода AddExpression(this Expression expression, Expression addExpression, BinaryExpressionType binaryExpressionType)
		/// </summary>
		/// <remarks>expression != null && addExpression != null && joinType == BinaryExpressionType.Or</remarks>
		[Test]
		public void AddExpressionNotNullExpressionOr()
		{
			Expression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																			  BinaryExpressionType.Equals);
			Expression addExpression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 2.ToLiteral(),
																			  BinaryExpressionType.Equals);
			expression = expression.AddExpression(addExpression, BinaryExpressionType.Or);
			Assert.AreEqual("[field] = 1 OR [field] = 2", expression.Render(options));
		}

		/// <summary>
		/// Тест метода AddExpression(this Expression expression, Expression addExpression, BinaryExpressionType binaryExpressionType)
		/// </summary>
		/// <remarks>expression != null && addExpression != null && joinType == BinaryExpressionType.And</remarks>
		[Test]
		public void AddExpressionNotNullExpressionAnd()
		{
			Expression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																			  BinaryExpressionType.Equals);
			Expression addExpression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 2.ToLiteral(),
																			  BinaryExpressionType.Equals);
			expression = expression.AddExpression(addExpression, BinaryExpressionType.And);
			Assert.AreEqual("[field] = 1 AND [field] = 2", expression.Render(options));
		}

		#endregion

		#region ParenthesisExpression
		[Test]
		public void ToParenthesisExpressionExpressionException()
		{
			Expression expression = null;
			Assert.Throws<ArgumentNullException>(() => expression.ToParenthesisExpression());
		}

		[Test]
		public void ToParenthesisExpression()
		{
			Expression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																  BinaryExpressionType.Equals);
			expression = expression.ToParenthesisExpression();
			Assert.AreEqual("([field] = 1)", expression.Render());
		}
		#endregion

		#region UnaryExpression
		/// <summary>
		/// Тест метода ToUnaryExpression(this Expression expression, UnaryExpressionType unaryExpressionType) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра expression == null</remarks>
		[Test]
		public void ToUnaryExpressionExpressionException()
		{
			Expression expression = null;
			Assert.Throws<ArgumentNullException>(() => expression.ToUnaryExpression(UnaryExpressionType.Not));
		}

		[Test]
		public void ToUnaryExpression()
		{
			Expression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																  BinaryExpressionType.Equals);
			UnaryExpression fragment = expression.ToUnaryExpression(UnaryExpressionType.Not);
			Assert.AreEqual("NOT [field] = 1", fragment.Render());

			expression = Helper.CreateColumn("", "field");
			fragment = expression.ToUnaryExpression(UnaryExpressionType.BitwiseNot);
			Assert.AreEqual("~[field]", fragment.Render());

			fragment = expression.ToUnaryExpression(UnaryExpressionType.IsNotNull);
			Assert.AreEqual("[field] IS NOT NULL", fragment.Render());

			fragment = expression.ToUnaryExpression(UnaryExpressionType.IsNull);
			Assert.AreEqual("[field] IS NULL", fragment.Render());

			fragment = expression.ToUnaryExpression(UnaryExpressionType.IsNull);
			Assert.AreEqual("[field] IS NULL", fragment.Render());

			fragment = expression.ToUnaryExpression(UnaryExpressionType.Negative);
			Assert.AreEqual("-[field]", fragment.Render());

			fragment = expression.ToUnaryExpression(UnaryExpressionType.Positive);
			Assert.AreEqual("+[field]", fragment.Render());
		}
		#endregion

		#region CastCall
		[Test]
		public void Cast()
		{
			Column column = Helper.CreateColumn("", "field");
			CastCall fragment = column.Cast(SqlDataTypeOption.NVarChar, 50);
			Assert.AreEqual("CAST ([field] AS NVARCHAR (50))", fragment.Render());
		}
		#endregion
	}
}
