using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests.Extensions
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class DeleteExtensionsTests
	{
		/// <summary>
		/// Тест метода Where(this DeleteStatement deleteStatement, BinaryExpressionType joinType, Expression searchCondition) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра deleteStatement == null</remarks>
		[Test]
		public void WhereDeleteStatementException()
		{
			DeleteStatement statement = null;
			Assert.Throws<ArgumentNullException>(() => statement.Where(BinaryExpressionType.And, null));
		}

		/// <summary>
		/// Тест метода Where(this DeleteStatement deleteStatement, BinaryExpressionType joinType, Expression searchCondition) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра searchCondition == null</remarks>
		[Test]
		public void WhereSearchConditionException()
		{
			DeleteStatement statement = new Delete("cmn", "Reference").GetQuery();
			Assert.Throws<ArgumentNullException>(() => statement.Where(BinaryExpressionType.And, null));
		}

		/// <summary>
		/// Тест метода Where(this DeleteStatement deleteStatement, BinaryExpressionType joinType, Expression searchCondition) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра joinType != BinaryExpressionType.And && joinType != BinaryExpressionType.Or</remarks>
		[Test]
		public void WhereJoinTypeException()
		{
			DeleteStatement statement = new Delete("cmn", "Reference").GetQuery();
			BinaryExpression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																		BinaryExpressionType.Equals);
			Assert.Throws<ArgumentException>(() => statement.Where(BinaryExpressionType.Equals, expression));
		}

		/// <summary>
		/// Тест метода Where(this DeleteStatement deleteStatement, BinaryExpressionType joinType, Expression searchCondition)
		/// </summary>
		/// <remarks>deleteStatement.WhereClause == null</remarks>
		[Test]
		public void WhereNullWhereClause()
		{
			BinaryExpression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																		BinaryExpressionType.Equals);
			DeleteStatement statement = new Delete("cmn", "Reference").GetQuery();
			statement.Where(BinaryExpressionType.Or, expression);
			const string expectedSqlString = "DELETE [cmn].[Reference] WHERE [field] = 1";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}

		/// <summary>
		/// Тест метода Where(this DeleteStatement deleteStatement, BinaryExpressionType joinType, Expression searchCondition)
		/// </summary>
		/// <remarks>deleteStatement.WhereClause != null</remarks>
		[Test]
		public void WhereNotNullWhereClause()
		{
			BinaryExpression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																		BinaryExpressionType.Equals);
			DeleteStatement statement = new Delete("cmn", "Reference").GetQuery();
			statement.Where(BinaryExpressionType.Or, expression);

			statement.Where(BinaryExpressionType.And, expression);
			const string expectedSqlString = "DELETE [cmn].[Reference] WHERE [field] = 1 AND [field] = 1";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}

		/// <summary>
		/// Тест метода Where(this DeleteStatement deleteStatement, Expression searchCondition)
		/// </summary>
		/// <remarks>При вызове joinType == BinaryExpressionType.And</remarks>
		[Test]
		public void WhereNotNullWhereClauseJoinTypeAnd()
		{
			BinaryExpression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																		BinaryExpressionType.Equals);
			DeleteStatement statement = new Delete("cmn", "Reference").GetQuery();
			statement.Where(BinaryExpressionType.Or, expression);

			statement.Where(expression);
			const string expectedSqlString = "DELETE [cmn].[Reference] WHERE [field] = 1 AND [field] = 1";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}

		/*
		[Test]
		public void Where()
		{
			BinaryExpression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																		BinaryExpressionType.Equals);
			DeleteStatement statement = new Delete("cmn", "Reference").GetQuery();
			statement.Where(BinaryExpressionType.Or, expression);
			string expectedSqlString = "DELETE [cmn].[Reference] WHERE [field] = 1";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());

			statement.Where(BinaryExpressionType.And, expression);
			expectedSqlString = "DELETE [cmn].[Reference] WHERE [field] = 1 AND [field] = 1";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}
		*/
	}
}
