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
	public class InsertTests
	{
		[Test]
		public void InsertFieldsNameException()
		{
			Assert.Throws<ArgumentNullException>(() => new Insert(null, "cmn", "Reference"));
		}

		[Test]
		public void InsertTableNameException()
		{
			Assert.Throws<ArgumentNullException>(() => new Insert(new List<string> { "id" }, "cmn", ""));
		}

		[Test]
		public void Insert()
		{
			InsertStatement fragment = new Insert(new List<string> {"id"}, "cmn", "Reference").GetQuery();
			Assert.AreEqual("INSERT INTO [cmn].[Reference] ([id])\r\n", fragment.Render());
		}
	}
}
