using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests.Extensions
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class StringExtensionsTests : BaseFixture
	{
		/// <summary>
		/// Тест метода ToIdentifier на получение исключения
		/// </summary>
		[Test]
		public static void ToIdentifier_Exception([Values("", "  ", null)] string value)
		{
			Assert.Throws<Exception>(() => value.ToIdentifier());
		}

		/// <summary>
		/// Тест метода ToIdentifier
		/// </summary>
		[Test]
		public static void ToIdentifier()
		{
			Identifier fragment = "str".ToIdentifier();
			Assert.AreEqual("[str]", fragment.Render());
		}

		/// <summary>
		/// Тест метода ToIdentifierWithoutQuote на получение исключения
		/// </summary>
		[Test]
		public static void ToIdentifierWithoutQuote_Exception([Values("", "  ", null)] string value)
		{
			Assert.Throws<Exception>(() => value.ToIdentifierWithoutQuote());
		}

		/// <summary>
		/// Тест метода ToIdentifierWithoutQuote
		/// </summary>
		[Test]
		public static void ToIdentifierWithoutQuote()
		{
			Identifier fragment = "str".ToIdentifierWithoutQuote();
			Assert.AreEqual("str", fragment.Render());
		}

		/// <summary>
		/// Тест метода ToLiteral на получение исключения
		/// </summary>
		[Test]
		public static void ToLiteralException()
		{
			string value = null;
			Assert.Throws<ArgumentNullException>(() => value.ToLiteral());
			//const double valueDouble = 1.2d;
			//Assert.Throws<Exception>(() => valueDouble.ToLiteral());
		}

		/// <summary>
		/// Тест метода ToLiteral
		/// </summary>
		[Test]
		public static void ToLiteral()
		{
			Literal fragment = "1".ToLiteral(LiteralType.Integer);
			Assert.AreEqual("1", fragment.Render());
		}

		/// <summary>
		/// Тест метода ToSchemaObjectName на получение исключения
		/// </summary>
		[Test]
		public static void ToSchemaObjectName_Exception([Values("", "  ", null)] string value)
		{
			Assert.Throws<Exception>(() => value.ToSchemaObjectName());
		}

		/// <summary>
		/// Тест метода ToSchemaObjectName
		/// </summary>
		[Test]
		public static void ToSchemaObjectName([Values("[Reference]", "[cmn].[Reference]")] string value)
		{
			SchemaObjectName fragment = value.ToSchemaObjectName();
			Assert.AreEqual(value, fragment.Render());
		}

		/// <summary>
		/// Тест метода ToSchemaObjectTableSource на получение исключения
		/// </summary>
		[Test]
		public static void ToSchemaObjectTableSource_Exception([Values("", "  ", null, "a", "a.a.a a")] string value)
		{
			Assert.Throws<Exception>(() => value.ToSchemaObjectTableSource());
		}

		/// <summary>
		/// Тест метода ToSchemaObjectTableSource
		/// </summary>
		[Test]
		public static void ToSchemaObjectTableSource()
		{
			SchemaObjectTableSource fragment = "cmn.Reference a".ToSchemaObjectTableSource();
			Assert.AreEqual("[cmn].[Reference] AS [a]", fragment.Render());
			
			SchemaObjectTableSource fragment2 = "Reference a".ToSchemaObjectTableSource();
			Assert.AreEqual("[Reference] AS [a]", fragment2.Render());
		}

		/// <summary>
		/// Тест метода ToColumn на получение исключения
		/// </summary>
		[Test]
		public static void ToColumn_Exception([Values("", "  ", null)] string value)
		{
			Assert.Throws<Exception>(() => value.ToColumn());
		}

		/// <summary>
		/// Тест метода ToColumn
		/// </summary>
		[Test]
		public static void ToColumn()
		{
			Column fragment = "a.field".ToColumn();
			Assert.AreEqual("[a].[field]", fragment.Render());
		}

		/// <summary>
		/// Тест метода ToSelectColumn на получение исключения
		/// </summary>
		[Test]
		public static void ToSelectColumn_Exception([Values("", "  ", null, "a", "a.a")] string value)
		{
			Assert.Throws<Exception>(() => value.ToSelectColumn());
		}

		/// <summary>
		/// Тест метода ToSelectColumn
		/// </summary>
		[Test]
		public static void ToSelectColumn()
		{
			SelectColumn fragment = "a.field fieldAlias".ToSelectColumn();
			Assert.AreEqual("[a].[field] AS [fieldAlias]", fragment.Render());
		}

		/// <summary>
		/// Тест метода IsValidString
		/// </summary>
		[Test]
		public static void IsValidString()
		{
			Assert.AreEqual(true, "asdf".IsValidString());
			Assert.AreEqual(false, "1asdf".IsValidString());
		}

		/// <summary>
		/// Тест метода IsLast
		/// </summary>
		[Test]
		public static void IsLast()
		{
			Assert.AreEqual(true, "zzz".IsLast());
			Assert.AreEqual(false, "azz".IsLast());
		}

		/// <summary>
		/// Тест метода GetNextAlias
		/// </summary>
		[Test]
		public static void GetNextAlias()
		{
			Assert.AreEqual("a", "".GetNextAlias());
			Assert.AreEqual("b", "a".GetNextAlias());
			Assert.AreEqual("aaaa", "zzz".GetNextAlias());
			Assert.AreEqual("bzz", "azz".GetNextAlias());
		}

		[Test]
		public void ToSelectStatement_Exception()
		{
			string value = "";
			Assert.Throws<Exception>(() => value.ToSelectStatement());

			value = "update [cmn].[Reference] set field=1 where id=1";
			Assert.Throws<Exception>(() => value.ToSelectStatement());

			value = "uupdate [cmn].[Reference] set field=1 where id=1";
			Assert.Throws<Exception>(() => value.ToSelectStatement());
		}

		[Test]
		public void ToSelectStatement()
		{
			const string value = "select id from cmn.reference";
			var fragment = value.ToSelectStatement();
			Assert.AreEqual("SELECT id FROM cmn.reference", fragment.Render(options));
		}

		[Test]
		public void ToTSqlStatement_Exception()
		{
			string value = "";
			Assert.Throws<Exception>(() => value.ToTSqlStatement());
			value = "select * from";
			Assert.Throws<Exception>(() => value.ToTSqlStatement());
		}

		[Test]
		public void ToTSqlStatement()
		{
			const string value = "select id from cmn.reference";
			TSqlStatement fragment = value.ToTSqlStatement();
			Assert.AreEqual("SELECT id FROM cmn.reference", fragment.Render(options));
		}
	}
}
