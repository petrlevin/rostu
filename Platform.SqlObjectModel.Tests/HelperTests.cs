using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class HelperTests : BaseFixture
	{
		#region Column, SelectColumn
		/// <summary>
		/// Исключение для Column CreateColumn(Identifier tableAlias, string fieldName)
		/// </summary>
		[Test]
		public void CreateColumnException([Values("", " ", null)]string param)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateColumn((null as Identifier), param));
		}

		/// <summary>
		/// Исключения для SelectColumn CreateColumn(string tableAliasName, string fieldName, string fieldAliasName)
		/// </summary>
		[Test]
		public void CreateSelectColumnException([Values("", "   ", null)]string param)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateColumn("", param, ""));
		}

		[Test]
		public void CreateColumnException()
		{
			Assert.Throws<Exception>(() => Helper.CreateColumn((null as Expression), "ff"));
		}

		[Test]
		public void CreateColumn1()
		{
			SelectColumn fragment = Helper.CreateColumn("field".ToColumn(), null);
			Assert.AreEqual("[field]", fragment.Render());
		}
		#endregion

		#region BinaryExpression
		/// <summary>
		/// Исключения метода BinaryExpression CreateBinaryExpression(string firstAliasName, string firstFieldName, string secondAliasName, string secondFieldName, BinaryExpressionType binaryExpressionType)
		/// </summary>
		[Test]
		public void CreateBinaryExpressionException([Values("", " ", null)]string param)
		{
			Assert.Throws<Exception>(() => Helper.CreateBinaryExpression("a", param, "b", "id", BinaryExpressionType.Equals));
			Assert.Throws<Exception>(() => Helper.CreateBinaryExpression("a", "id", "b", param, BinaryExpressionType.Equals));
		}

		/// <summary>
		/// Тест метода BinaryExpression CreateBinaryExpression(string firstAliasName, string firstFieldName, string secondAliasName, string secondFieldName, BinaryExpressionType binaryExpressionType)
		/// </summary>
		[Test]
		public void CreateBinaryExpression()
		{
			BinaryExpression fragment = Helper.CreateBinaryExpression("a", "id", "b", "id", BinaryExpressionType.Equals);
			Assert.AreEqual("[a].[id] = [b].[id]", fragment.Render());
		}


		/// <summary>
		/// Исключения метода BinaryExpression CreateBinaryExpression(Identifier firstAlias, string firstFieldName, Identifier secondAlias, string secondFieldName, BinaryExpressionType binaryExpressionType = BinaryExpressionType.Equals)
		/// </summary>
		[Test]
		public void CreateBinaryExpressionAliasIdetifierException([Values("", " ", null)]string param)
		{
			Assert.Throws<Exception>(() => Helper.CreateBinaryExpression("a".ToIdentifier(), param, "b".ToIdentifier(), "id", BinaryExpressionType.Equals));
			Assert.Throws<Exception>(() => Helper.CreateBinaryExpression("a".ToIdentifier(), "id", "b".ToIdentifier(), param, BinaryExpressionType.Equals));
		}

		/// <summary>
		/// Тест метода BinaryExpression CreateBinaryExpression(string firstAliasName, string firstFieldName, string secondAliasName, string secondFieldName, BinaryExpressionType binaryExpressionType)
		/// </summary>
		[Test]
		public void CreateBinaryExpressionAliasIdetifier()
		{
			BinaryExpression fragment = Helper.CreateBinaryExpression("a".ToIdentifier(), "id", "b".ToIdentifier(), "id", BinaryExpressionType.Equals);
			Assert.AreEqual("[a].[id] = [b].[id]", fragment.Render());
		}

		/// <summary>
		/// Исключения метода BinaryExpression CreateBinaryExpression(Expression firstExpression, Expression secondExpression, BinaryExpressionType binaryExpressionType)
		/// </summary>
		[Test]
		public void CreateBinaryExpressionOnTwoExpressionException()
		{
			Assert.Throws<Exception>(() => Helper.CreateBinaryExpression(null, new BinaryExpression(), BinaryExpressionType.Equals));
			Assert.Throws<Exception>(() => Helper.CreateBinaryExpression(new BinaryExpression(), null, BinaryExpressionType.Equals));
		}

		/// <summary>
		/// Тест метода BinaryExpression CreateBinaryExpression(Expression firstExpression, Expression secondExpression, BinaryExpressionType binaryExpressionType)
		/// </summary>
		[Test]
		public void CreateBinaryExpressionOnTwoExpression()
		{
			BinaryExpression fragment = Helper.CreateBinaryExpression("a.id".ToColumn(), 1.ToLiteral(), BinaryExpressionType.Equals);
			Assert.AreEqual("[a].[id] = 1", fragment.Render());
		}


		#endregion

		#region SchemaObjectName
		/// <summary>
		/// Исключения метода SchemaObjectName CreateSchemaObjectName(string schemaName, string tableName)
		/// </summary>
		/// <param name="value"></param>
		[Test]
		public void CreateSchemaObjectNameException([Values("", "  ", null)] string value)
		{
			Assert.Throws<Exception>(() => Helper.CreateSchemaObjectName(value, value));
		}

		/// <summary>
		/// Исключения метода SchemaObjectName CreateSchemaObjectName(params string[] values)
		/// </summary>
		[Test]
		public void CreateSchemaObjectNameArrayStringException()
		{
			Assert.Throws<Exception>(() => Helper.CreateSchemaObjectName(null));
			Assert.Throws<Exception>(() => Helper.CreateSchemaObjectName(new string[] { }));
		}

		/// <summary>
		/// Тест метода SchemaObjectName CreateSchemaObjectName(string schemaName, string tableName)
		/// </summary>
		[Test]
		public void CreateSchemaObjectName()
		{
			SchemaObjectName fragment = Helper.CreateSchemaObjectName("cmn", "Reference");
			Assert.AreEqual("[cmn].[Reference]", fragment.Render());
		}

		/// <summary>
		/// Тест метода SchemaObjectName CreateSchemaObjectName(params string[] values)
		/// </summary>
		[Test]
		public void CreateSchemaObjectNameArrayString()
		{
			SchemaObjectName fragment = Helper.CreateSchemaObjectName(new string[] { "Reference" });
			Assert.AreEqual("[Reference]", fragment.Render());

			fragment = Helper.CreateSchemaObjectName(new string[] { "cmn", "Reference" });
			Assert.AreEqual("[cmn].[Reference]", fragment.Render());

			fragment = Helper.CreateSchemaObjectName(new string[] { "Sbor", "cmn", "Reference" });
			Assert.AreEqual("[Sbor].[cmn].[Reference]", fragment.Render());
		}
		#endregion

		#region SchemaObjectTableSource
		/// <summary>
		/// Тест метода CreateSchemaObjectTableSource на получение исключения
		/// </summary>
		[Test]
		public void CreateSchemaObjectTableSourceException([Values("", "  ", null)] string value)
		{
			Assert.Throws<Exception>(() => Helper.CreateSchemaObjectTableSource(value, value, value));
		}

		/*/// <summary>
		/// Тест метода CreateSchemaObjectTableSource
		/// </summary>
		[Test]
		public void CreateSchemaObjectTableSourceException([Values("")] string schemaName, [Values("", "test")] string tableName, [Values("")] string aliasName)
		{
			Assert.Throws<Exception>(() => Helper.CreateSchemaObjectTableSource(schemaName, tableName, aliasName));
		}*/

		/// <summary>
		/// Тест метода CreateSchemaObjectTableSource
		/// </summary>
		[Test]
		public void CreateSchemaObjectTableSource()
		{
			var fragment = Helper.CreateSchemaObjectTableSource("dbo", "TableName", "tableAlias");
			Assert.AreEqual("[dbo].[TableName] AS [tableAlias]", fragment.Render());
		}
		#endregion

		#region QualifiedJoin
		/// <summary>
		/// Исключения метода CreateQualifiedJoin
		/// </summary>
		[Test]
		public void CreateQualifiedJoinException()
		{
			Assert.Throws<Exception>(() => Helper.CreateQualifiedJoin(null, null, null, QualifiedJoinType.Inner));
		}

		/// <summary>
		/// Тест метода CreateQualifiedJoin
		/// </summary>
		[Test]
		public void CreateQualifiedJoin()
		{
			const string expected = "[cmn].[Reference] AS [a] INNER JOIN\r\n[cmn].[ReferenceFields] AS [b]\r\nON [a].[id] = [b].[idReference]";

			QualifiedJoin join = Helper.CreateQualifiedJoin(
				Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a"),
				Helper.CreateSchemaObjectTableSource("cmn", "ReferenceFields", "b"),
				Helper.CreateBinaryExpression("a", "id", "b", "idReference", BinaryExpressionType.Equals), 
				QualifiedJoinType.Inner);
			
			Assert.AreEqual(expected, join.Render(options));
		}
		#endregion

		#region InPredicate

		#endregion

		#region QuerySpecification
		/// <summary>
		/// Тест метода CreateQuerySpecification и всех его перегрузок на получение исключения
		/// </summary>
		[Test]
		public void CreateQuerySpecification_Exception([Values("")] string empty, [Values("value")] string value)
		{
			TableSource tableSource = Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a");
			TableSource[] tableSources = new TableSource[] {tableSource};
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(null, null, null, null, null, null));
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(tableSources, null, null, null, null, null));
			
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(empty, value, empty, (List<String>) null));
            Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(value, empty, empty, (List<String>)null));
            Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(value, value, empty, (List<String>)null));
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(value, value, empty, new List<string>()));

			List<string> fields = new List<string>();
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(null, fields));
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(tableSource, fields));
			fields = null;
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(tableSource, fields));

			List<TSqlFragment> fields2=new List<TSqlFragment>();
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(null, fields2));
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(tableSource, fields2));
			fields2 = null;
			Assert.Throws<Exception>(() => Helper.CreateQuerySpecification(tableSource, fields2));
		}
		
		/// <summary>
		/// Тест метода CreateQuerySpecification и всех его перегрузок
		/// </summary>
		[Test]
		public void CreateQuerySpecification()
		{
			string expectedSqlString = "SELECT [a].[field] FROM [cmn].[Reference] AS [a]";
			SelectStatement fragment = new SelectStatement
				{
					QueryExpression = Helper.CreateQuerySpecification(
						new TableSource[] {Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a")},
						new TSqlFragment[] {Helper.CreateColumn("a", "field")}, null, null, null, null)
				};
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
			
			fragment.QueryExpression = Helper.CreateQuerySpecification("cmn", "Reference", "a",
																	   new List<string> {"field"});
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			fragment.QueryExpression =
				Helper.CreateQuerySpecification(Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a"),
												new List<string> {"field"});
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			fragment.QueryExpression =
				Helper.CreateQuerySpecification(Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a"),
												new List<TSqlFragment> { Helper.CreateColumn("a", "field") });
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			expectedSqlString = "SELECT [a].[field] FROM [cmn].[Reference] AS [a] WHERE [a].[field] = 1";
			WhereClause whereClause = new WhereClause
			{
				SearchCondition =
					Helper.CreateBinaryExpression(Helper.CreateColumn("a", "field"),
													  "1".ToLiteral(LiteralType.Integer),
													  BinaryExpressionType.Equals)
			};
			fragment.QueryExpression = Helper.CreateQuerySpecification(
				new TableSource[] { Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a") },
				new TSqlFragment[] { Helper.CreateColumn("a", "field") }, whereClause, null, null, null);
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			expectedSqlString =
				"SELECT [a].[field] FROM [cmn].[Reference] AS [a] WHERE [a].[field] = 1 GROUP BY [a].[field]";
			GroupByClause groupByClause = new GroupByClause();
			ExpressionGroupingSpecification expressionGroupingSpecification = new ExpressionGroupingSpecification { Expression = Helper.CreateColumn("a", "field") };
			groupByClause.GroupingSpecifications.Add(expressionGroupingSpecification);
			fragment.QueryExpression = Helper.CreateQuerySpecification(
				new TableSource[] { Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a") },
				new TSqlFragment[] { Helper.CreateColumn("a", "field") }, whereClause, groupByClause, null, null);
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			expectedSqlString =
				"SELECT [a].[field] FROM [cmn].[Reference] AS [a] WHERE [a].[field] = 1 GROUP BY [a].[field] HAVING 1 > 0";
			HavingClause havingClause = new HavingClause
			{
				SearchCondition = Helper.CreateBinaryExpression("1".ToLiteral(LiteralType.Integer),
																	"0".ToLiteral(LiteralType.Integer),
																	BinaryExpressionType.GreaterThan)
			};
			fragment.QueryExpression = Helper.CreateQuerySpecification(
				new TableSource[] { Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a") },
				new TSqlFragment[] { Helper.CreateColumn("a", "field") }, whereClause, groupByClause, havingClause, null);
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			expectedSqlString =
				"SELECT TOP 10 [a].[field] FROM [cmn].[Reference] AS [a] WHERE [a].[field] = 1 GROUP BY [a].[field] HAVING 1 > 0";
			TopRowFilter topRowFilter = new TopRowFilter { Expression = "10".ToLiteral(LiteralType.Integer) };
			fragment.QueryExpression = Helper.CreateQuerySpecification(
				new TableSource[] { Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a") },
				new TSqlFragment[] { Helper.CreateColumn("a", "field") }, whereClause, groupByClause, havingClause, topRowFilter);
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());

			expectedSqlString =
				"SELECT DISTINCT [a].[field] FROM [cmn].[Reference] AS [a] WHERE [a].[field] = 1 GROUP BY [a].[field] HAVING 1 > 0";
			fragment.QueryExpression = Helper.CreateQuerySpecification(
				new TableSource[] { Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a") },
				new TSqlFragment[] { Helper.CreateColumn("a", "field") }, whereClause, groupByClause, havingClause, null, UniqueRowFilter.Distinct);
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		/// <summary>
		/// Тест метода CreateQuerySpecification(string schemaName, string tableName, string tableAlias, string fields)
		/// </summary>
		[Test]
		public void CreateQuerySpecification1()
		{
			const string expectedSqlString = "SELECT [a].[id], [a].[name] FROM [ref].[Entity] [a]";
			SelectStatement fragment = new SelectStatement
				{
					QueryExpression = Helper.CreateQuerySpecification("ref", "Entity", "a", "id, name")
				};
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		/// <summary>
		/// Тест метода CreateQuerySpecification(TableSource tableSource, List&lt;TSqlFragment&gt; fields)
		/// </summary>
		/// <remarks>Передача параметра tableSource == null</remarks>
		[Test]
		public void CreateQuerySpecification2()
		{
			const string expectedSqlString = "SELECT [id], [name]";
			SelectStatement fragment = new SelectStatement
				{
					QueryExpression =
						Helper.CreateQuerySpecification(null, new List<TSqlFragment> {"id".ToColumn(), "name".ToColumn()})
				};
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}
		#endregion

		#region CreateBinaryQueryExpression
		/// <summary>
		/// Тест метода CreateBinaryQueryExpression на получение исключения
		/// </summary>
		[Test]
		public void CreateBinaryQueryExpression_Exception()
		{
			QuerySpecification querySpecification = new QuerySpecification();
			Assert.Throws<Exception>(() => Helper.CreateBinaryQueryExpression(null, null, BinaryQueryExpressionType.Union));
			Assert.Throws<Exception>(() => Helper.CreateBinaryQueryExpression(querySpecification, null, BinaryQueryExpressionType.Union));
		}

		/// <summary>
		/// Тест метода CreateBinaryQueryExpression
		/// </summary>
		[Test]
		public void CreateBinaryQueryExpression()
		{
	        const string expectedSqlString =
		        "SELECT [a].[id] FROM [cmn].[Reference] AS [a] UNION SELECT [a].[id] FROM [cmn].[Reference] AS [a]";
			QuerySpecification query =
				Helper.CreateQuerySpecification(
					new TableSource[] {Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a")},
					new TSqlFragment[] {Helper.CreateColumn("a", "id")}, null, null, null, null);
			BinaryQueryExpression unionQuery = Helper.CreateBinaryQueryExpression(query, query, BinaryQueryExpressionType.Union);
			SelectStatement fragment = new SelectStatement {QueryExpression = unionQuery};

            Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}
		#endregion

		/// <summary>
		/// Тест метода CreateRowNumber на получение исключения
		/// </summary>
		[Test]
		public void CreateRowNumber_Exception()
		{
			Assert.Throws<Exception>(() => Helper.CreateRowNumber(null));
			Assert.Throws<Exception>(() => Helper.CreateRowNumber(new OrderByClause()));
		}

		/// <summary>
		/// Тест метода CreateRowNumber
		/// </summary>
		[Test]
		public void CreateRowNumber()
		{
			OrderByClause orderByClause = new OrderByClause();
			orderByClause.OrderByElements.Add(new ExpressionWithSortOrder {Expression = "field".ToColumn()});
			SelectColumn fragment = Helper.CreateRowNumber(orderByClause);
			Assert.AreEqual("ROW_NUMBER() OVER ( ORDER BY [field])", fragment.Render());
			fragment = Helper.CreateRowNumber(orderByClause, "fieldAlias");
			Assert.AreEqual("ROW_NUMBER() OVER ( ORDER BY [field]) AS [fieldAlias]", fragment.Render());
		}

		#region TernaryExpression
		/// <summary>
		/// Тест метода CreateBetween на получение исключения
		/// </summary>
		/// <remarks>Передача параметра field == null или field.Identifiers.Count == 0</remarks>
		[Test]
		public void CreateBetweenFieldException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateBetween(null, null, null));
			Assert.Throws<ArgumentNullException>(() => Helper.CreateBetween(new Column(), null, null));
		}

		/// <summary>
		/// Тест метода CreateBetween на получение исключения
		/// </summary>
		/// <remarks>Передача параметра firstValue == null</remarks>
		[Test]
		public void CreateBetweenFirstValueException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateBetween("field".ToColumn(), null, null));
		}

		/// <summary>
		/// Тест метода CreateBetween на получение исключения
		/// </summary>
		/// <remarks>Передача параметра secondValue == null</remarks>
		[Test]
		public void CreateBetweenSecondValueException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateBetween("field".ToColumn(), 1, null));
		}
		
		/// <summary>
		/// Тест метода CreateBetween
		/// </summary>
		[Test]
		public void CreateBetween()
		{
			TernaryExpression fragment = Helper.CreateBetween("field".ToColumn(), 1, 2);
			Assert.AreEqual("[field] BETWEEN 1 AND 2", fragment.Render());
		}
		#endregion

		#region CaseExpression
		[Test]
		public void CreateCaseExpression()
		{
			CaseExpression fragment = Helper.CreateCaseExpression(1.ToLiteral(),
			                                                      new List<WhenClause>
				                                                      {
					                                                      new WhenClause
						                                                      {
							                                                      WhenExpression = 1.ToLiteral(),
							                                                      ThenExpression = "aaa".ToLiteral()
						                                                      }
				                                                      }, "bbb".ToLiteral());
			
			Assert.AreEqual("CASE 1 WHEN 1 THEN 'aaa' ELSE 'bbb' END", fragment.Render());
		}
		#endregion

		#region CastCall
		/// <summary>
		/// Тест метода CreateCast на получение исключения
		/// </summary>
		/// <remarks>Передача параметра expression == null</remarks>
		[Test]
		public void CreateCastExpressionException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateCast(null, SqlDataTypeOption.Int));
		}

		/// <summary>
		/// Тест метода CreateCast
		/// </summary>
		[Test]
		public void CreateCast()
		{
			CastCall fragment = Helper.CreateCast("1".ToLiteral(), SqlDataTypeOption.Int);
			Assert.AreEqual("CAST ('1' AS INT)", fragment.Render());
		}

		/// <summary>
		/// Тест метода CreateCast
		/// </summary>
		/// <remarks>С передачей параметра length</remarks>
		[Test]
		public void CreateCastWithLength()
		{
			CastCall fragment = Helper.CreateCast(1.ToLiteral(), SqlDataTypeOption.NVarChar, 50);
			Assert.AreEqual("CAST (1 AS NVARCHAR (50))", fragment.Render());
		}
		#endregion

		#region UnaryExpression
		/// <summary>
		/// Тест метода _createUnaryExpression на получение исключения
		/// </summary>
		/// <remarks>Передача параметра expression == null</remarks>
		[Test]
		public void CreateUnaryExpressionException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateCheckIsNotNull(null));
		}

		/// <summary>
		/// Тест метода CreateCheckIsNull
		/// </summary>
		[Test]
		public void CreateCheckIsNull()
		{
			UnaryExpression fragment = Helper.CreateCheckIsNull("a.field".ToColumn());
			Assert.AreEqual("[a].[field] IS NULL", fragment.Render());
		}

		/// <summary>
		/// Тест метода CreateCheckIsNotNull
		/// </summary>
		[Test]
		public void CreateCheckIsNotNull()
		{
			UnaryExpression fragment = Helper.CreateCheckIsNotNull("a.field".ToColumn());
			Assert.AreEqual("[a].[field] IS NOT NULL", fragment.Render());
		}
		
		/// <summary>
		/// Тест метода CreateCheckFieldIsNull на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра fieldName</remarks>
		[Test]
		public void CreateCheckFieldIsNullException([Values("", "a")]string param1, [Values("")]string param2)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateCheckFieldIsNull(param1, param2));
		}

		/// <summary>
		/// Тест метода CreateCheckFieldIsNull
		/// </summary>
		[Test]
		public void CreateCheckFieldIsNull()
		{
			UnaryExpression fragment = Helper.CreateCheckFieldIsNull("a", "field");
			Assert.AreEqual("[a].[field] IS NULL", fragment.Render());
		}

		/// <summary>
		/// Тест метода CreateCheckFieldIsNotNull на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра fieldName</remarks>
		[Test]
		public void CreateCheckFieldIsNotNullException([Values("", "a")]string param1, [Values("")]string param2)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateCheckFieldIsNotNull(param1, param2));
		}

		/// <summary>
		/// Тест метода CreateCheckFieldIsNotNull
		/// </summary>
		[Test]
		public void CreateCheckFieldIsNotNull()
		{
			UnaryExpression fragment = Helper.CreateCheckFieldIsNotNull("a", "field");
			Assert.AreEqual("[a].[field] IS NOT NULL", fragment.Render());
		}
		#endregion

		#region ExistsPredicate
		[Test]
		public void CreateExistsPredicate()
		{
			SelectStatement selectStatement = new SelectStatement
			{
				QueryExpression = Helper.CreateQuerySpecification(
					new TableSource[] { Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a") },
					new TSqlFragment[] { Helper.CreateColumn("a", "field") }, null, null, null, null)
			};
			ExistsPredicate fragment = Helper.CreateExistsPredicate(selectStatement);
			Assert.AreEqual("EXISTS (SELECT [a].[field]\r\n        FROM   [cmn].[Reference] AS [a])", fragment.Render());
		}
		#endregion

		#region FunctionCall
		/// <summary>
		/// Тест метода CreateFunctionCall на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра functionName</remarks>
		[Test]
		public void CreateFunctionCallException([Values("", "  ", null)]string param)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateFunctionCall(param, new List<Expression>()));
		}
		
		[Test]
		public void CreateFunctionCall()
		{
			FunctionCall fragment = Helper.CreateFunctionCall("MAX", "a.field".ToColumn());
			Assert.AreEqual("MAX([a].[field])", fragment.Render());
		}

		[Test]
		public void CreateFunctionCallWithOver()
		{
			FunctionCall fragment = Helper.CreateFunctionCall("MAX", "a.field".ToColumn(), Helper.CreateOverClause(null, null));
			Assert.AreEqual("MAX([a].[field]) OVER ()", fragment.Render());
		}

		[Test]
		public void CreateFunctionCallWithTarget()
		{
			FunctionCall fragment = Helper.CreateFunctionCall("exist", "test".ToLiteral(), null, "This");
			Assert.AreEqual("This.exist('test')", fragment.Render());
		}
		#endregion

		#region InPredicate
		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, List<string> values, LiteralType literalType, bool useNot = false) на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра fieldName</remarks>
		[Test]
		public void CreateInPredicateListStringFieldNameException([Values("", "  ", null)]string param)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate("a", param, null, LiteralType.AsciiStringLiteral));
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, List<string> values, LiteralType literalType, bool useNot = false) на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра values</remarks>
		[Test]
		public void CreateInPredicateListStringValuesException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate("a", "field", new List<string>(), LiteralType.AsciiStringLiteral));
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate("a", "field", null, LiteralType.AsciiStringLiteral));
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, List<string> values, LiteralType literalType, bool useNot = false) на получение исключения
		/// </summary>
		/// <remarks>Значения передаются в виде списка строк</remarks>
		[Test]
		public void CreateInPredicateListString()
		{
			InPredicate fragment = Helper.CreateInPredicate("a", "field", new List<string> { "a", "b" },
															LiteralType.AsciiStringLiteral);
			Assert.AreEqual("[a].[field] IN ('a', 'b')", fragment.Render());
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, string values, bool useNot = false) на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра fieldName</remarks>
		[Test]
		public void CreateInPredicateStringFieldNameException([Values("", "  ", null)]string param)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate("a", param, ""));
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, string values, bool useNot = false) на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра values</remarks>
		[Test]
		public void CreateInPredicateStringValuesException([Values("", "  ", null)]string param)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate("a", "field", param));
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, string values, bool useNot = false) 
		/// </summary>
		/// <remarks>Значения передаются в виде списка строк</remarks>
		[Test]
		public void CreateInPredicateString()
		{
			InPredicate fragment = Helper.CreateInPredicate("a", "field", "1, 2");
			Assert.AreEqual("[a].[field] IN (1, 2)", fragment.Render());
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, IEnumerable values, bool useNot = false)
		/// </summary>
		/// <remarks>Передача пустого параметра fieldName</remarks>
		[Test]
		public void CreateInPredicateIEnumerableFieldNameException([Values("", "  ", null)]string param)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate("a", param, (IEnumerable)null));
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, IEnumerable values, bool useNot = false)
		/// </summary>
		/// <remarks>Передача пустого параметра values</remarks>
		[Test]
		public void CreateInPredicateIEnumerableValuesException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate("a", "field", new List<string>()));
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate("a", "field", (IEnumerable)null));
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, IEnumerable values, bool useNot = false)
		/// </summary>
		/// <remarks>Значения передаются в виде списка строк</remarks>
		[Test]
		public void CreateInPredicateIEnumerable()
		{
			InPredicate fragment = Helper.CreateInPredicate("a", "field", new List<string> { "a", "b" });
			Assert.AreEqual("[a].[field] IN ('a', 'b')", fragment.Render());
		}

		/// <summary>
		/// Тест метода CreateInPredicate(Expression expression, Subquery subquery, bool useNot = false)
		/// </summary>
		/// <remarks>Передача пустого параметра fieldName</remarks>
		[Test]
		public void CreateInPredicateSubqueryExpressionException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate(null, new Subquery()));
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, Subquery subquery, bool useNot = false)
		/// </summary>
		/// <remarks>Передача пустого параметра subquery</remarks>
		[Test]
		public void CreateInPredicateSubquerySubqueryException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate(Helper.CreateColumn("a", "field"), (Subquery)null));
			Assert.Throws<ArgumentNullException>(() => Helper.CreateInPredicate(Helper.CreateColumn("a", "field"), new Subquery()));
		}

		/// <summary>
		/// Тест метода CreateInPredicate(string aliasName, string fieldName, Subquery subquery, bool useNot = false)
		/// </summary>
		/// <remarks>Передача пустого параметра subquery</remarks>
		[Test]
		public void CreateInPredicateSubquery()
		{
			QuerySpecification query =
				Helper.CreateQuerySpecification(
					new TableSource[] { Helper.CreateSchemaObjectTableSource("cmn", "Reference", "a") },
					new TSqlFragment[] { Helper.CreateColumn("a", "id") }, null, null, null, null);
			Subquery subquery = new Subquery { QueryExpression = query };
			InPredicate fragment = Helper.CreateInPredicate(Helper.CreateColumn("a", "field"), subquery);
			Assert.AreEqual("[a].[field] IN (SELECT [a].[id] FROM [cmn].[Reference] AS [a])", fragment.Render(options));
		}
		#endregion

		#region LikePredicate
		/// <summary>
		/// Тест метода CreateLikePredicate на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра expression</remarks>
		[Test]
		public void CreateLikePredicateExpressionException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateLikePredicate(null, ""));
			Assert.Throws<ArgumentNullException>(() => Helper.CreateLikePredicate(null, ""));
		}

		/// <summary>
		/// Тест метода CreateLikePredicate на получение исключения
		/// </summary>
		/// <remarks>Передача пустого параметра likeExpression</remarks>
		[Test]
		public void CreateLikePredicateLikeExpressionException([Values("", "  ", null)]string param)
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateLikePredicate(Helper.CreateColumn("a", "field"), param));
		}

		/// <summary>
		/// Тест метода CreateLikePredicate
		/// </summary>
		[Test]
		public void CreateLikePredicate()
		{
			LikePredicate fragment = Helper.CreateLikePredicate(Helper.CreateColumn("a", "field"), "%aaa");
			Assert.AreEqual("[a].[field] LIKE '%aaa' ESCAPE '\\'", fragment.Render());
		}
		#endregion

		#region OverClause
		[Test]
		public void CreateOverClauseOrderByClause()
		{
			OrderByClause orderByClause = new OrderByClause();
			orderByClause.OrderByElements.Add(new ExpressionWithSortOrder() {Expression = "id".ToColumn(), SortOrder = SortOrder.NotSpecified});
			OverClause fragment = Helper.CreateOverClause(orderByClause, null);
			Assert.AreEqual("OVER ( ORDER BY [id])", fragment.Render());
		}

		[Test]
		public void CreateOverClause()
		{
			OverClause fragment = Helper.CreateOverClause(null, new List<Expression> {"id".ToColumn()});
			Assert.AreEqual("OVER (PARTITION BY [id])", fragment.Render());
		}
		#endregion

		#region WhenClause
		/// <summary>
		/// Тест метода CreateWhenClause на получение исключений
		/// </summary>
		/// <remarks>Передача параметра when == null</remarks>
		[Test]
		public void CreateWhenClauseWhenException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateWhenClause(null, null));
		}

		/// <summary>
		/// Тест метода CreateWhenClause на получение исключений
		/// </summary>
		/// <remarks>Передача параметра then == null</remarks>
		[Test]
		public void CreateWhenClauseThenException()
		{
			Assert.Throws<ArgumentNullException>(() => Helper.CreateWhenClause("id".ToColumn(), null));
		}

		[Test]
		public void CreateWhenClause()
		{
			WhenClause fragment = Helper.CreateWhenClause("when".ToColumn(), "1".ToLiteral());
			Assert.AreEqual("WHEN [when] THEN '1'", fragment.Render());
		}

		#endregion
	}
}
