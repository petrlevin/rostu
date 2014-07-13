using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class DeleteTests
	{
		[Test]
		public void DeleteException()
		{
			Assert.Throws<ArgumentNullException>(() => new Delete("cmn", ""));
		}

		[Test]
		public void Delete()
		{
			const string expectedSqlString = "DELETE [cmn].[Reference]";
			DeleteStatement fragment = new Delete("cmn", "Reference").GetQuery();
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

		[Test]
		public void DeleteWithAlias()
		{
			const string expectedSqlString = "DELETE [a] FROM [cmn].[Reference] AS [a]";
			DeleteStatement fragment = new Delete("cmn", "Reference", "a").GetQuery();
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), fragment.Render());
		}

	}
}
