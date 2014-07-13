using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class RecursiveSelectTests
	{
		[Test]
		public void RecursiveSelectTableNameException()
		{
			Assert.Throws<ArgumentNullException>(() => new RecursiveSelect(new List<string>(), "cmn", "", "cte", "a", "id", "idParent"));
		}

		[Test]
		public void RecursiveSelectNamePartWithException()
		{
			Assert.Throws<ArgumentNullException>(() => new RecursiveSelect(new List<string>(), "cmn", "Reference", "", "a", "id", "idParent"));
		}

		[Test]
		public void RecursiveSelectReqursiveFieldNameException()
		{
			Assert.Throws<ArgumentNullException>(() => new RecursiveSelect(new List<string>(), "cmn", "Reference", "cte", "a", "", "idParent"));
		}

		[Test]
		public void RecursiveSelectParentReqursiveFieldNameException()
		{
			Assert.Throws<ArgumentNullException>(() => new RecursiveSelect(new List<string>(), "cmn", "Reference", "cte", "a", "id", ""));
		}

		[Test]
		public void GetQueryFieldsNameException()
		{
			RecursiveSelect recursiveSelect = new RecursiveSelect((List<string>) null, "cmn", "Reference", "cte", "a", "id",
			                                                      "idParent");
			Assert.Throws<ArgumentException>(() => recursiveSelect.GetQuery());
		}

		[Test]
		public void GetQuery21FieldsNameException()
		{
			RecursiveSelect recursiveSelect = new RecursiveSelect("", "cmn", "Reference", "cte", "a", "id",
																  "idParent");
			Assert.Throws<ArgumentException>(() => recursiveSelect.GetQuery());
		}

		[Test]
		public void GetQuery22FieldsNameException()
		{
			RecursiveSelect recursiveSelect = new RecursiveSelect((string)null, "cmn", "Reference", "cte", "a", "id",
																  "idParent");
			Assert.Throws<ArgumentException>(() => recursiveSelect.GetQuery());
		}

		[Test]
		public static void RecursiveSelectGetQuery()
		{
			var fragment =
				new RecursiveSelect(new List<string>() {"name", "description"}, "cmn", "Reference", "cte", "a", "id",
									"idParent").GetQuery();
			const string expectedSqlString =
				"WITH [cte] ([name], [description])\r\nAS	 (SELECT [a].[name], [a].[description], [a].[id], [a].[idParent] FROM [cmn].[Reference] AS [a] WHERE [a].[idParent] IS NULL UNION ALL SELECT [a].[name], [a].[description], [a].[id], [a].[idParent] FROM [cmn].[Reference] AS [a] INNER JOIN [cte] AS [b] ON [a].[idParent] = [b].[id]) SELECT [a].[name], [a].[description] FROM [cte] AS [a]";

			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}


		[Test]
		public static void RecursiveSelect2GetQuery()
		{
			var fragment =
				new RecursiveSelect("name, description" , "cmn", "Reference", "cte", "a", "id",
									"idParent").GetQuery();
			const string expectedSqlString =
				"WITH [cte] ([name], [description])\r\nAS	 (SELECT [a].[name], [a].[description], [a].[id], [a].[idParent] FROM [cmn].[Reference] AS [a] WHERE [a].[idParent] IS NULL UNION ALL SELECT [a].[name], [a].[description], [a].[id], [a].[idParent] FROM [cmn].[Reference] AS [a] INNER JOIN [cte] AS [b] ON [a].[idParent] = [b].[id]) SELECT [a].[name], [a].[description] FROM [cte] AS [a]";

			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}
	}
}
