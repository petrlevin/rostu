using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	public class SelectTests
	{
		[Test]
		public void SelectFieldsNameException()
		{
			Assert.Throws<ArgumentNullException>(() => new Select((List<string>)null, "", "", ""));
		}

		[Test]
		public void SelectTableNameException()
		{
			Assert.Throws<ArgumentNullException>(() => new Select(new List<string> { "id" }, "", "", ""));
		}

		[Test]
		public void Select2GetQuery()
		{
			const string expectedSqlString = "SELECT [id].[id] FROM [cmn].[Reference] AS [id]";
			Select select = new Select("id", "cmn", "Reference", "id");
			SelectStatement fragment = select.GetQuery();

			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		[Test]
		public static void SelectGetQuery()
		{
			const string expectedSqlString = "SELECT [id].[id] FROM [cmn].[Reference] AS [id]";
			Select select = new Select(new List<string>() {"id"}, "cmn", "Reference", "id");
			SelectStatement fragment = select.GetQuery();

			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}
	}
}
