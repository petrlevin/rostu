using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Platform.SqlObjectModel.Extensions;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;

namespace Platform.SqlObjectModel.Tests.Extensions
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class SelectExtensionsTests
	{
		#region Join
		[Test]
		public void JoinWithTable()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			select.JoinWithTable(QualifiedJoinType.Inner, "cmn", "ReferenceFields", "Reference", "id", "idReference");
			const string expectedSqlString = "SELECT [a].[id] FROM [cmn].[Reference] AS [a] INNER JOIN [cmn].[ReferenceFields] AS [ReferenceFields] ON [a].[id] = [ReferenceFields].[idReference]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), select.Render());
		}

		[Test]
		public void Join()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			select.Join(QualifiedJoinType.Inner, "cmn", "ReferenceFields", "a", "id", "idReference");
			const string expectedSqlString = "SELECT [a].[id] FROM [cmn].[Reference] AS [a] INNER JOIN [cmn].[ReferenceFields] AS [ReferenceFields] ON [a].[id] = [ReferenceFields].[idReference]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), select.Render());
		}

		[Test]
		public void JoinSelectStatementException()
		{
			SelectStatement select = null;
			Assert.Throws<ArgumentNullException>(() => select.Join(QualifiedJoinType.Inner, "", "", "", "", "", ""));
		}

		[Test]
		public void JoinSelectStatementQueryExpressionException()
		{
			SelectStatement select = new SelectStatement();
			Assert.Throws<ArgumentException>(() => select.Join(QualifiedJoinType.Inner, "", "", "", "", "", ""));
		}

		[Test]
		public void Join2()
		{
			SelectStatement select = new Select(new List<string> {"id"}, "cmn", "Reference", "a").GetQuery();
			select.Join(QualifiedJoinType.Inner, "cmn", "ReferenceFields", "b", "a", "id", "idReference");
			const string expectedSqlString = "SELECT [a].[id] FROM [cmn].[Reference] AS [a] INNER JOIN [cmn].[ReferenceFields] AS [b] ON [a].[id] = [b].[idReference]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), select.Render());
		}

		[Test]
		public void Join2AliasNull()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			select.Join(QualifiedJoinType.Inner, "cmn", "ReferenceFields", "", "a", "id", "idReference");
			const string expectedSqlString = "SELECT [a].[id] FROM [cmn].[Reference] AS [a] INNER JOIN [cmn].[ReferenceFields] AS [b] ON [a].[id] = [b].[idReference]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), select.Render());
		}

		[Test]
		public void Join2SelectStatementException()
		{
			SelectStatement select = null;
			Assert.Throws<ArgumentNullException>(() => select.Join(QualifiedJoinType.Inner, (TableSource)null, (Expression)null));
		}

		[Test]
		public void Join2QueryExpressionException()
		{
			SelectStatement select = new SelectStatement();
			Assert.Throws<ArgumentException>(() => select.Join(QualifiedJoinType.Inner, (TableSource)null, (Expression)null));
		}

		[Test]
		public void Join2JoinTableSourceException()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.Throws<ArgumentNullException>(() => select.Join(QualifiedJoinType.Inner, (TableSource)null, (Expression)null));
		}

		[Test]
		public void Join2SearchConditionException()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.Throws<ArgumentNullException>(() => select.Join(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource("cmn", "ReferenceField", "b"), (Expression)null));
		}
		#endregion

		#region Fields
		[Test]
		public void FieldsSelectStatementException()
		{
			SelectStatement select = null;
			Assert.Throws<ArgumentNullException>(() => select.Fields("", null));
		}

		[Test]
		public void FieldsQueryExpressionException()
		{
			SelectStatement select = new SelectStatement();
			Assert.Throws<ArgumentException>(() => select.Fields("", null));
		}

		[Test]
		public void FieldsAliasNameException([Values ("", "  ", null)]string param)
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.Throws<ArgumentNullException>(() => select.Fields(param, null));
		}

		[Test]
		public void Fields()
		{
			SelectStatement fragment = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			fragment.Fields("a", new List<string> { "name" });
			const string expectedSqlString = "SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		[Test]
		public void Fields2()
		{
			SelectStatement fragment = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			fragment.Fields(new List<Field> { new Field {Experssion = "a.name".ToColumn()} });
			const string expectedSqlString = "SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		[Test]
		public void Fields3()
		{
			SelectStatement fragment = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			fragment.AddFields(new List<SelectColumn> { new SelectColumn {ColumnName = "newField".ToIdentifier(), Expression = "a.name".ToColumn()} });
			const string expectedSqlString = "SELECT [a].[id], [a].[name] AS [newField] FROM [cmn].[Reference] AS [a]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}
		#endregion

		#region Where
		[Test]
		public void WhereSearchConditionException()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.Throws<ArgumentNullException>(() => select.Where(BinaryExpressionType.And, null));
		}

		[Test]
		public void WhereJoinTypeException()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.Throws<ArgumentException>(() => select.Where(BinaryExpressionType.GreaterThan, Helper.CreateBinaryExpression(Helper.CreateColumn("a", "id"), "1".ToLiteral(LiteralType.Integer),
													   BinaryExpressionType.Equals)));
		}

		[Test]
		public void Where()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			select.Where(BinaryExpressionType.And,
						 Helper.CreateBinaryExpression(Helper.CreateColumn("a", "id"), "1".ToLiteral(LiteralType.Integer),
													   BinaryExpressionType.Equals));
			const string expectedSqlStringAnd = "SELECT [a].[id] FROM [cmn].[Reference] AS [a] WHERE [a].[id] = 1";
			Assert.AreEqual(expectedSqlStringAnd.ToTSqlStatement().Render(), select.Render());

			select.Where(BinaryExpressionType.Or,
						 Helper.CreateBinaryExpression(Helper.CreateColumn("a", "id"), "2".ToLiteral(LiteralType.AsciiStringLiteral),
													   BinaryExpressionType.Equals));
			const string expectedSqlStringOr =
				"SELECT [a].[id] FROM [cmn].[Reference] AS [a] WHERE [a].[id] = 1 OR [a].[id] = '2'";
			Assert.AreEqual(expectedSqlStringOr.ToTSqlStatement().Render(), select.Render());
		}
		/*
		[Test]
		public void Where_Exception()
		{
			SelectStatement select = null;
			Assert.Throws<Exception>(() => select.Where(BinaryExpressionType.And, null));

			select = new SelectStatement();
			Assert.Throws<Exception>(() => select.Where(BinaryExpressionType.And, null));

			select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.Throws<Exception>(() => select.Where(BinaryExpressionType.And, null));
			Assert.Throws<Exception>(() => select.Where(BinaryExpressionType.GreaterThan, Helper.CreateBinaryExpression(Helper.CreateColumn("a", "id"), "1".ToLiteral(LiteralType.Integer),
													   BinaryExpressionType.Equals)));
		}

		[Test]
		public void Where()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			select.Where(BinaryExpressionType.And,
						 Helper.CreateBinaryExpression(Helper.CreateColumn("a", "id"), "1".ToLiteral(LiteralType.Integer),
													   BinaryExpressionType.Equals));
			string expectedSqlString = "SELECT [a].[id] FROM [cmn].[Reference] AS [a] WHERE [a].[id] = 1";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), select.Render());

			select.Where(BinaryExpressionType.Or,
						 Helper.CreateBinaryExpression(Helper.CreateColumn("a", "id"), "2".ToLiteral(LiteralType.AsciiStringLiteral),
													   BinaryExpressionType.Equals));
			expectedSqlString =
				"SELECT [a].[id] FROM [cmn].[Reference] AS [a] WHERE [a].[id] = 1 OR [a].[id] = '2'";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), select.Render());
		}
		*/
		#endregion

		/// <summary>
		/// Тест метода NextAlias на получение исключения
		/// </summary>
		[Test]
		public void NextAlias_Exception()
		{
			SelectStatement select = null;
			Assert.Throws<ArgumentNullException>(() => select.NextAlias());
			
			select = new SelectStatement();
			Assert.Throws<ArgumentException>(() => select.NextAlias());

			select.QueryExpression = new BinaryQueryExpression();
			Assert.Throws<Exception>(() => select.NextAlias());

			select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			(select.QueryExpression as QuerySpecification).FromClauses.Add(new PivotedTableSource());
			Assert.Throws<Exception>(() => select.NextAlias());
		}

		/// <summary>
		/// Тест метода NextAlias
		/// </summary>
		[Test]
		public void NextAlias()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.AreEqual("b", select.NextAlias());

			SelectStatement select2 = new SelectStatement();
			select2.QueryExpression = Helper.CreateQuerySpecification(select.ToQueryDerivedTable("a"), new List<string> {"id"});
			Assert.AreEqual("b", select2.NextAlias());

			select.Join(QualifiedJoinType.Inner, select.ToQueryDerivedTable("b"),
						Helper.CreateBinaryExpression("a", "id", "b", "id", BinaryExpressionType.Equals));
			Assert.AreEqual("c", select.NextAlias());
		}

		/// <summary>
		/// Тест метода ToSubquery на получение исключения
		/// </summary>
		[Test]
		public void ToSubquery_Exception()
		{
			SelectStatement select = null;
			Assert.Throws<ArgumentNullException>(() => select.ToSubquery());

			select = new SelectStatement();
			Assert.Throws<ArgumentException>(() => select.ToSubquery());
			
			select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			select.WithCommonTableExpressionsAndXmlNamespaces=new WithCommonTableExpressionsAndXmlNamespaces();
			Assert.Throws<Exception>(() => select.ToSubquery());
		}

		/// <summary>
		/// Тест метода ToQueryDerivedTable на получение исключения
		/// </summary>
		[Test]
		public void ToQueryDerivedTable_Exception()
		{
			SelectStatement select = null;
			Assert.Throws<ArgumentNullException>(() => select.ToQueryDerivedTable());

			select = new SelectStatement();
			Assert.Throws<ArgumentException>(() => select.ToQueryDerivedTable());
		}

		/// <summary>
		/// Тест метода ToQueryDerivedTable
		/// </summary>
		[Test]
		public void ToQueryDerivedTable()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			SelectStatement fragment = new SelectStatement
				{
					QueryExpression = Helper.CreateQuerySpecification(@select.ToQueryDerivedTable(),
					                                                  new List<string> {"id"})
				};
			string expectedSqlString = "SELECT [b].[id] FROM (SELECT [a].[id] FROM [cmn].[Reference] AS [a]) AS [b]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			fragment = new SelectStatement
				{
					QueryExpression = Helper.CreateQuerySpecification(@select.ToQueryDerivedTable("b"),
					                                                  new List<string> {"id"})
				};
			expectedSqlString = "SELECT [b].[id] FROM (SELECT [a].[id] FROM [cmn].[Reference] AS [a]) AS [b]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		[Test]
		public void GetAliasOnTableSelectStatementException()
		{
			SelectStatement select = null;
			Assert.Throws<ArgumentNullException>(() => select.GetAliasOnTable("Reference"));
		}

		[Test]
		public void GetAliasOnTableSelectStatementQueryExpressionException()
		{
			SelectStatement select = new SelectStatement();
			Assert.Throws<ArgumentException>(() => select.GetAliasOnTable("Reference"));
		}

		[Test]
		public void GetAliasOnTableTableNameException()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.Throws<Exception>(() => select.GetAliasOnTable(""));
		}

		[Test]
		public void GetAliasOnTable_Exception()
		{
			/*
			SelectStatement select = null;
			Assert.Throws<Exception>(() => select.GetAliasOnTable("Reference"));
			
			select = new SelectStatement();
			Assert.Throws<Exception>(() => select.GetAliasOnTable("Reference"));

			select.QueryExpression = new BinaryQueryExpression();
			Assert.Throws<Exception>(() => select.GetAliasOnTable("Reference"));

			select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.Throws<Exception>(() => select.GetAliasOnTable(""));

			(select.QueryExpression as QuerySpecification).FromClauses.Add(new PivotedTableSource());
			Assert.Throws<Exception>(() => select.GetAliasOnTable("ReferenceFields"));
			*/
		}

		[Test]
		public void GetAliasOnTable()
		{
			SelectStatement select = new Select(new List<string> { "id" }, "cmn", "Reference", "a").GetQuery();
			Assert.AreEqual("a", select.GetAliasOnTable("Reference"));

			select.Join(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b"),
						Helper.CreateBinaryExpression(Helper.CreateColumn("a", "id"),
													  Helper.CreateColumn("b", "idReference"),
													  BinaryExpressionType.Equals));
			Assert.AreEqual("b", select.GetAliasOnTable("ReferenceFields"));

			select = new Select(new List<string> { "id" }, "cmn", "ReferenceFields", "a").GetQuery();
			select.Join(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource("cmn", "Reference", "b"),
						Helper.CreateBinaryExpression(Helper.CreateColumn("a", "idReference"),
													  Helper.CreateColumn("b", "id"),
													  BinaryExpressionType.Equals));
			select.Join(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource("cmn", "Reference", "c"),
						Helper.CreateBinaryExpression(Helper.CreateColumn("b", "id"),
													  Helper.CreateColumn("c", "id"),
													  BinaryExpressionType.Equals));
			Assert.AreEqual("b", select.GetAliasOnTable("Reference"));
			Assert.AreEqual("a", select.GetAliasOnTable("ReferenceFields"));
		}

		/// <summary>
		/// Тест метода ToSelectStatementWithCommonTableExpressions на получение исключения
		/// </summary>
		[Test]
		public void ToSelectStatementWithCommonTableExpressions_Exception()
		{
			SelectStatement selectStatement = null;
			Assert.Throws<ArgumentNullException>(() => selectStatement.ToSelectStatementWithCommonTableExpressions("cte"));
			selectStatement = new SelectStatement {QueryExpression = new BinaryQueryExpression()};
			Assert.Throws<Exception>(() => selectStatement.ToSelectStatementWithCommonTableExpressions("cte"));
		}

		/// <summary>
		/// Тест метода ToSelectStatementWithCommonTableExpressions
		/// </summary>
		[Test]
		public void ToSelectStatementWithCommonTableExpressions()
		{
			SelectStatement selectStatement =
				new Select(new List<string> {"id", "name"}, "cmn", "Reference", "a").GetQuery();
			SelectStatement fragment = selectStatement.ToSelectStatementWithCommonTableExpressions("cte");
			const string expectedSqlString =
				"WITH [cte]\r\nAS (SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a]) SELECT [a].[id], [a].[name] FROM [cte] AS [a]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		/// <summary>
		/// Тест метода OrderBy на получение исключения
		/// </summary>
		[Test]
		public void OrderBy_Exception()
		{
			SelectStatement selectStatement = null;
			Assert.Throws<Exception>(() => selectStatement.OrderBy(new List<ExpressionWithSortOrder>()));
		}

		/// <summary>
		/// Тест метода OrderBy на получение исключения
		/// </summary>
		[Test]
		public void OrderBy()
		{
			SelectStatement fragment = new Select(new List<string> { "id", "name" }, "cmn", "Reference", "a").GetQuery();
			fragment.OrderBy(new List<ExpressionWithSortOrder> { new ExpressionWithSortOrder {Expression = "id".ToColumn() } });
			string expectedSqlString = "SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a] ORDER BY [id]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
			fragment.OrderBy(new List<ExpressionWithSortOrder> { new ExpressionWithSortOrder { Expression = "name".ToColumn(), SortOrder = SortOrder.Descending} });
			expectedSqlString = "SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a] ORDER BY [id], [name] DESC";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

	}
}
