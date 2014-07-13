using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class UpdateTests
	{
		[Test]
		public void UpdateException()
		{
			Assert.Throws<ArgumentNullException>(() => new Update("cmn", ""));
		}

		[Test]
		public void Update()
		{
			UpdateStatement fragment = new Update("cmn", "Reference").GetQuery();
			Assert.AreEqual("UPDATE [cmn].[Reference]\r\nSET    ", fragment.Render());
		}
		
		[Test]
		public void UpdateWithAlias()
		{
			UpdateStatement fragment = new Update("cmn", "Reference", "a").GetQuery();
			Assert.AreEqual("UPDATE [a]\r\nSET    \r\nFROM   [cmn].[Reference] AS [a]", fragment.Render());
		}
	}
}
