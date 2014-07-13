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
	public class QueryExtensionsTests : BaseFixture
	{
		#region AddJoin
		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType joinType, TableSource tableSource, Expression joinCondition) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра query == null</remarks>
		[Test]
		public void AddJoinQueryExpressionException()
		{
			QueryExpression query = null;
			Assert.Throws<ArgumentNullException>(() => query.AddJoin(QualifiedJoinType.Inner, null, null));
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType joinType, TableSource tableSource, Expression joinCondition) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра tableSource == null</remarks>
		[Test]
		public void AddJoinTableSourceException()
		{
			QueryExpression query = new BinaryQueryExpression();
			Assert.Throws<ArgumentNullException>(() => query.AddJoin(QualifiedJoinType.Inner, null, null));
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType joinType, TableSource tableSource, Expression joinCondition) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра joinCondition == null</remarks>
		[Test]
		public void AddJoinJoinConditionException()
		{
			QueryExpression query = new BinaryQueryExpression();
			Assert.Throws<ArgumentNullException>(() => query.AddJoin(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b"), null));
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType joinType, TableSource tableSource, Expression joinCondition) на получение исключения
		/// </summary>
		/// <remarks>Передача параметра query с типом, для которого не рализован метод</remarks>
		[Test]
		public void AddJoinNotSupportException()
		{
			QueryExpression query = new BinaryQueryExpression();
			Assert.Throws<NotImplementedException>(() => query.AddJoin(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b"), Helper.CreateBinaryExpression("a", "field", "b", "field", BinaryExpressionType.Equals)));
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType jointType, string schemaName, string tableName, string aliasName, string leftAliasName, string leftFieldName, string thisFieldName) на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра tableName</remarks>
		[Test]
		public void AddJoinTableNameException([Values("", " ", null)]string param)
		{
			QueryExpression query = new BinaryQueryExpression();
			Assert.Throws<ArgumentNullException>(() => query.AddJoin(QualifiedJoinType.Inner, "ref", param, "b", "a", "id", "idLink"));
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType jointType, string schemaName, string tableName, string aliasName, string leftAliasName, string leftFieldName, string thisFieldName) на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра leftFieldName</remarks>
		[Test]
		public void AddJoinLeftFieldNameException([Values("", " ", null)]string param)
		{
			QueryExpression query = new BinaryQueryExpression();
			Assert.Throws<ArgumentNullException>(() => query.AddJoin(QualifiedJoinType.Inner, "ref", "Table", "b", "a", param, "idLink"));
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType jointType, string schemaName, string tableName, string aliasName, string leftAliasName, string leftFieldName, string thisFieldName) на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра thisFieldName</remarks>
		[Test]
		public void AddJoinThisFieldNameException([Values("", " ", null)]string param)
		{
			QueryExpression query = new BinaryQueryExpression();
			Assert.Throws<ArgumentNullException>(() => query.AddJoin(QualifiedJoinType.Inner, "ref", "Table", "b", "a", "id", param));
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType jointType, string schemaName, string tableName, string aliasName, string leftAliasName, string leftFieldName, string thisFieldName) на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра aliasName</remarks>
		[Test]
		public void AddJoinAliasNameException([Values("", " ", null)]string param)
		{
			QueryExpression query = new BinaryQueryExpression();
			Assert.Throws<ArgumentNullException>(() => query.AddJoin(QualifiedJoinType.Inner, "ref", "Table", param, "a", "id", "idLink"));
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType joinType, TableSource tableSource, Expression joinCondition)
		/// </summary>
		[Test]
		public void AddJoin()
		{
			const string expectedSqlString =
				"SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a] INNER JOIN [cmn].[ReferenceFields] AS [b] ON [a].[field] = [b].[field]";
			QueryExpression query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id", "name" });
			query.AddJoin(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b"), Helper.CreateBinaryExpression("a", "field", "b", "field", BinaryExpressionType.Equals));
			SelectStatement fragment = new SelectStatement { QueryExpression = query };
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		/// <summary>
		/// Тест метода AddJoin(this QueryExpression query, QualifiedJoinType jointType, string schemaName, string tableName, string aliasName, string leftAliasName, string leftFieldName, string thisFieldName)
		/// </summary>
		[Test]
		public void AddJoin2()
		{
			const string expectedSqlString =
				"SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a] INNER JOIN [cmn].[ReferenceFields] AS [b] ON [a].[field] = [b].[field]";
			QueryExpression query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id", "name" });
			query.AddJoin(QualifiedJoinType.Inner, "cmn", "ReferenceFields", "b", "a", "field", "field");
			SelectStatement fragment = new SelectStatement { QueryExpression = query };
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}
		#endregion

		/// <summary>
		/// Тест метода AddFields и всех его перегрузок на получение исключения
		/// </summary>
		[Test]
		public void AddFields_Exception()
		{
			/*
			QueryExpression query = null;
			Assert.Throws<Exception>(() => query.AddFields((List<Field>)null));
			Assert.Throws<Exception>(() => query.AddFields("", (IEnumerable<String>) null));

			query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id", "name" });
			Assert.Throws<Exception>(() => query.AddFields((List<Field>)null));
            Assert.Throws<Exception>(() => query.AddFields("", (IEnumerable<String>)null));
            Assert.Throws<Exception>(() => query.AddFields("a", (IEnumerable<String>)null));

			query = new BinaryQueryExpression();
			List<Field> fields = new List<Field>();
			Assert.Throws<Exception>(() => query.AddFields(fields));
			
			List<string> fieldsName = new List<string>();
			Assert.Throws<Exception>(() => query.AddFields("a", fieldsName));
			*/
		}

		/// <summary>
		/// Тест метода AddFields и всех его перегрузок
		/// </summary>
		[Test]
		public void AddFields()
		{
			/*
			QueryExpression query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			query.AddFields(new List<Field> {new Field {Alias = "test", Experssion = Helper.CreateColumn("a", "name")}});

			SelectStatement fragment = new SelectStatement { QueryExpression = query };
			string expectedSqlString = "SELECT [a].[id], [a].[name] AS [test] FROM [cmn].[Reference] AS [a]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			query.AddFields("a", new List<string> {"name"});
			fragment = new SelectStatement { QueryExpression = query };
			expectedSqlString = "SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
			*/
		}

		#region AddField
		public void AddFieldQueryException()
		{
			QueryExpression query = null;
			Assert.Throws<ArgumentNullException>(() => query.AddField("field".ToColumn()));
		}

		public void AddFieldExpressionException()
		{
			QueryExpression query = new QuerySpecification();
			Assert.Throws<ArgumentNullException>(() => query.AddField(null));
		}

		public void AddFieldQueryNotImplementedException()
		{
			QueryExpression query = new BinaryQueryExpression();
			Assert.Throws<NotImplementedException>(() => query.AddField(null));
		}

		#endregion

		/// <summary>
		/// Тест метода AddWhere и всех его перегрузок на получение исключения
		/// </summary>
		[Test]
		public void AddWhere_Exception()
		{
			QueryExpression query = null;
			Assert.Throws<Exception>(() => query.AddWhere(BinaryExpressionType.And, null));
			
			query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id", "name" });
			Assert.Throws<Exception>(() => query.AddWhere(BinaryExpressionType.And, null));

			query = new BinaryQueryExpression();
			Assert.Throws<Exception>(() => query.AddWhere(BinaryExpressionType.And, Helper.CreateBinaryExpression("a", "field", "b", "field", BinaryExpressionType.Equals)));
		}

		/// <summary>
		/// Тест метода AddWhere и всех его перегрузок
		/// </summary>
		[Test]
		public void AddWhere()
		{
			QuerySpecification query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id", "name" });
			query.AddWhere(BinaryExpressionType.And,
						   Helper.CreateBinaryExpression("a", "field", "b", "field", BinaryExpressionType.Equals));
			SelectStatement fragment = new SelectStatement {QueryExpression = query};
			string expectedSqlString =
				"SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a] WHERE [a].[field] = [b].[field]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
			
			query.WhereClause = new WhereClause();
			query.AddWhere(BinaryExpressionType.And,
						   Helper.CreateBinaryExpression("a", "field", "b", "field", BinaryExpressionType.Equals));
			expectedSqlString =
				"SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a] WHERE [a].[field] = [b].[field]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			query.AddWhere(BinaryExpressionType.And,
						   Helper.CreateBinaryExpression("a", "field", "b", "field", BinaryExpressionType.Equals));
			expectedSqlString =
				"SELECT [a].[id], [a].[name] FROM [cmn].[Reference] AS [a] WHERE [a].[field] = [b].[field] AND [a].[field] = [b].[field]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}


		/// <summary>
		/// Тест метода Union и всех его перегрузок на получение исключения
		/// </summary>
		[Test]
		public void Union_Exception()
		{
			QueryExpression query = null;
			Assert.Throws<Exception>(() => query.Union(null));
			
			query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			Assert.Throws<Exception>(() => query.Union(null));
		}

		/// <summary>
		/// Тест метода Union и всех его перегрузок
		/// </summary>
		[Test]
		public void Union()
		{
			QueryExpression query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			SelectStatement fragment = new SelectStatement {QueryExpression = query.Union(query)};
			string expectedSqlString =
				"SELECT [a].[id] FROM [cmn].[Reference] AS [a] UNION SELECT [a].[id] FROM [cmn].[Reference] AS [a]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
			
			fragment = new SelectStatement { QueryExpression = query.Union(query, true) };
			expectedSqlString =
				"SELECT [a].[id] FROM [cmn].[Reference] AS [a] UNION ALL SELECT [a].[id] FROM [cmn].[Reference] AS [a]";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		/// <summary>
		/// Тест метода ToCommonTableExpression и всех его перегрузок на получение исключения
		/// </summary>
		[Test]
		public void ToCommonTableExpression_Exception()
		{
			Subquery subquery = null;
			Assert.Throws<Exception>(() => subquery.ToCommonTableExpression(""));
			Assert.Throws<Exception>(() => subquery.ToCommonTableExpression("", null));

			QuerySpecification query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			subquery = new Subquery { QueryExpression = query };
			Assert.Throws<Exception>(() => subquery.ToCommonTableExpression(""));
			Assert.Throws<Exception>(() => subquery.ToCommonTableExpression("", null));
			Assert.Throws<Exception>(() => subquery.ToCommonTableExpression("cte", null));
		}

		/// <summary>
		/// Тест метода ToCommonTableExpression и всех его перегрузок
		/// </summary>
		[Test]
		public void ToCommonTableExpression()
		{
			QuerySpecification query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			Subquery subquery = new Subquery { QueryExpression = query };
			CommonTableExpression commonTableExpression1 = subquery.ToCommonTableExpression("cte");
			CommonTableExpression commonTableExpression2 = subquery.ToCommonTableExpression("cte", new List<string> { "id" });
			Assert.AreEqual(" [cte]\r\nAS (SELECT [a].[id] FROM [cmn].[Reference] AS [a])", commonTableExpression1.Render(options));
			Assert.AreEqual(" [cte] ([id])\r\nAS (SELECT [a].[id] FROM [cmn].[Reference] AS [a])", commonTableExpression2.Render(options));
		}

		/// <summary>
		/// Тест метода ToSubquery и всех его перегрузок на получение исключения
		/// </summary>
		[Test]
		public void ToSubquery_Exception()
		{
			QueryExpression query = null;
			Assert.Throws<Exception>(() => query.ToSubquery());

			query = new QueryParenthesis();
			Assert.Throws<Exception>(() => query.ToSubquery());
		}

		/// <summary>
		/// Тест метода ToSubquery и всех его перегрузок
		/// </summary>
		[Test]
		public void ToSubquery()
		{
			QueryExpression query = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			Subquery fragment = query.ToSubquery();
			Assert.AreEqual("(SELECT [a].[id] FROM [cmn].[Reference] AS [a])", fragment.Render(options));
		}
	}
}
