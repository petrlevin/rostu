using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.SqlObjectModel.Extensions;
using NUnit.Framework;

namespace Platform.SqlObjectModel.Tests.Extensions
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class TableSourceExtensionsTests : BaseFixture
	{
		/// <summary>
		/// Тест метода Join на получение исключения
		/// </summary>
		[Test]
		public void Join_Exception()
		{
			TableSource thisTableSource = null;
			Assert.Throws<Exception>(() => thisTableSource.Join(QualifiedJoinType.Inner, null, null));
			
			thisTableSource = new SchemaObjectTableSource();
			TableSource tableSource = new SchemaObjectTableSource();
			Assert.Throws<Exception>(() => thisTableSource.Join(QualifiedJoinType.Inner, null, null));
			Assert.Throws<Exception>(() => thisTableSource.Join(QualifiedJoinType.Inner, tableSource, null));
		}

		/// <summary>
		/// Тест метода Join
		/// </summary>
		[Test]
		public void Join()
		{
			TableSource thisTableSource = Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a");
			TableSource tableSource = Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b");
			var fragment = thisTableSource.Join(QualifiedJoinType.Inner, tableSource,
												Helper.CreateBinaryExpression("a", "id", "b", "idReference",
																			  BinaryExpressionType.Equals));
			
			Assert.AreEqual("[cmn].[Reference] AS [a] INNER JOIN\r\n[cmn].[ReferenceFields] AS [b]\r\nON [a].[id] = [b].[idReference]", fragment.Render(options));
		}

		/// <summary>
		/// Тест метода GetAlias на получение исключения
		/// </summary>
		[Test]
		public void GetAlias_Exception()
		{
			TableSource tableSource = null;
			Assert.Throws<Exception>(() => tableSource.GetAliasName());
			tableSource = new QualifiedJoin();
			Assert.Throws<Exception>(() => tableSource.GetAliasName());
		}

		[Test]
		public void GetAlias()
		{
			TableSource tableSource = Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a");
			Assert.AreEqual("a", tableSource.GetAliasName());

			tableSource = new SchemaObjectTableSource {SchemaObject = Helper.CreateSchemaObjectName("cmn", "Reference")};
			Assert.AreEqual("", tableSource.GetAliasName());

			tableSource = new QueryDerivedTable
				{
					Subquery =
						Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> {"id"}).ToSubquery(),
						Alias = "a".ToIdentifier()
				};
			Assert.AreEqual("a", tableSource.GetAliasName());

			tableSource = new QueryDerivedTable
			{
				Subquery =
					Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" }).ToSubquery(),
				Alias = null
			};
			Assert.AreEqual("", tableSource.GetAliasName());
		}

		[Test]
		public void GetMaxAlias_Exception()
		{
			QualifiedJoin join = null;
			Assert.Throws<Exception>(() => join.GetMaxAlias(""));

			join = new QualifiedJoin();
			Assert.Throws<Exception>(() => join.GetMaxAlias(""));

			join.FirstTableSource = Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b");
			Assert.Throws<Exception>(() => join.GetMaxAlias(""));
		}
		
		[Test]
		public void GetMaxAlias()
		{
			QuerySpecification firstQuery = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> {"id"});
			QuerySpecification secondQuery = Helper.CreateQuerySpecification("cmn", "ReferenceFields", "b", new List<string> { "id, idReference" });
			firstQuery.AddJoin(QualifiedJoinType.Inner,
							   Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b"),
							   Helper.CreateBinaryExpression("a", "id", "b", "idReference", BinaryExpressionType.Equals));
			Assert.AreEqual("b", (firstQuery.FromClauses[0] as QualifiedJoin).GetMaxAlias(""));

			firstQuery = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			firstQuery.AddJoin(QualifiedJoinType.Inner,
							   new QueryDerivedTable {Subquery = secondQuery.ToSubquery(), Alias = "c".ToIdentifier()},
							   Helper.CreateBinaryExpression("a", "id", "c", "idReference", BinaryExpressionType.Equals));
			Assert.AreEqual("c", (firstQuery.FromClauses[0] as QualifiedJoin).GetMaxAlias(""));
			
			firstQuery = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			firstQuery.AddJoin(QualifiedJoinType.Inner,
							   Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b"),
							   Helper.CreateBinaryExpression("a", "id", "b", "idReference", BinaryExpressionType.Equals));
			firstQuery.AddJoin(QualifiedJoinType.Inner,
							   Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "c"),
							   Helper.CreateBinaryExpression("a", "id", "c", "idReference", BinaryExpressionType.Equals));
			Assert.AreEqual("c", (firstQuery.FromClauses[0] as QualifiedJoin).GetMaxAlias(""));

			firstQuery = Helper.CreateQuerySpecification("cmn", "Reference", "a1", new List<string> { "id" });
			firstQuery.AddJoin(QualifiedJoinType.Inner,
							   Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b1"),
							   Helper.CreateBinaryExpression("a1", "id", "b1", "idReference", BinaryExpressionType.Equals));
			Assert.AreEqual("", (firstQuery.FromClauses[0] as QualifiedJoin).GetMaxAlias(""));

			firstQuery = Helper.CreateQuerySpecification("cmn", "Reference", "a", new List<string> { "id" });
			firstQuery.AddJoin(QualifiedJoinType.Inner,
							   new QueryDerivedTable { Subquery = secondQuery.ToSubquery(), Alias = "c1".ToIdentifier() },
							   Helper.CreateBinaryExpression("a", "id", "c1", "idReference", BinaryExpressionType.Equals));
			Assert.AreEqual("a", (firstQuery.FromClauses[0] as QualifiedJoin).GetMaxAlias(""));

		}
	}
}
