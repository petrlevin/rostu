using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Collections;

namespace Platform.SqlObjectModel.Tests.Extensions
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class InsertExtensionsTests
	{
		[Test]
		public void SourceAsParameters_Exception()
		{
			InsertStatement statement = null;
			Assert.Throws<Exception>(() => statement.SourceAsParameters());
		}

		[Test]
		public void SourceAsParameters()
		{
			InsertStatement statement = new Insert(new List<string> {"id"}, "cmn", "Reference").GetQuery();
			statement.SourceAsParameters();
			const string expectedSqlString = "INSERT INTO [cmn].[Reference] ([id]) VALUES (@id)";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}

		[Test]
		public void SourceAsValues_Exception()
		{
			InsertStatement statement = null;
			Assert.Throws<Exception>(() => statement.SourceAsValues(null));
			statement = new Insert(new List<string> { "id", "name" }, "cmn", "Reference").GetQuery();
			Assert.Throws<Exception>(() => statement.SourceAsValues(null));
			Assert.Throws<Exception>(() => statement.SourceAsValues(new IgnoreCaseDictionary<object>()));
			Assert.Throws<Exception>(() => statement.SourceAsValues(new IgnoreCaseDictionary<object> { { "id", 1 } }));
		}

		[Test]
		public void SourceAsValues()
		{
			InsertStatement statement = new Insert(new List<string> { "id", "name" }, "cmn", "Reference").GetQuery();
			statement.SourceAsValues(new IgnoreCaseDictionary<object> { { "id", 1 }, { "name", "aaa" } });
			const string expectedSqlString = "INSERT INTO [cmn].[Reference] ([id], [name]) VALUES (1, 'aaa')";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}
	}
}
