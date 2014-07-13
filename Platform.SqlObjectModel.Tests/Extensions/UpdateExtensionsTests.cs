using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel.Tests.Extensions
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class UpdateExtensionsTests : BaseFixture
	{
		[Test]
		public void Where_Exception()
		{
			UpdateStatement statement = null;
			Assert.Throws<Exception>(() => statement.Where(BinaryExpressionType.And, null));

			statement = new Update("cmn", "Reference").GetQuery();
			Assert.Throws<Exception>(() => statement.Where(BinaryExpressionType.And, null));
			BinaryExpression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																		BinaryExpressionType.Equals);
			Assert.Throws<Exception>(() => statement.Where(BinaryExpressionType.Divide, expression));
		}

		[Test]
		public void Where()
		{
			BinaryExpression expression = Helper.CreateBinaryExpression(Helper.CreateColumn("", "field"), 1.ToLiteral(),
																		BinaryExpressionType.Equals);
			UpdateStatement statement = new Update("cmn", "Reference").GetQuery();
			statement.Where(BinaryExpressionType.Or, expression);
			string expectedSqlString = "UPDATE [cmn].[Reference]\r\nSET  WHERE [field] = 1";
			Assert.AreEqual(expectedSqlString, statement.Render(options));

			statement.Where(BinaryExpressionType.And, expression);
			expectedSqlString = "UPDATE [cmn].[Reference]\r\nSET  WHERE [field] = 1 AND [field] = 1";
			Assert.AreEqual(expectedSqlString, statement.Render(options));
		}

		[Test]
		public void SetAsParameters_Exception()
		{
			UpdateStatement statement = null;
			Assert.Throws<Exception>(() => statement.SetAsParameters(new List<string> { "id" }));
		}

		[Test]
		public void SetAsParameters()
		{
			UpdateStatement statement = new Update("cmn", "Reference").GetQuery();
			statement.SetAsParameters(new List<string> {"id"});
			const string expectedSqlString = "UPDATE [cmn].[Reference] SET [id] = @id";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}

		[Test]
		public void SetAsValues_Exception()
		{
			UpdateStatement statement = null;
			Assert.Throws<Exception>(() => statement.SetAsValues(null));
			statement = new UpdateStatement();
			Assert.Throws<Exception>(() => statement.SetAsValues(null));
			Assert.Throws<Exception>(() => statement.SetAsValues(new Dictionary<string, object>()));
		}

		[Test]
		public void SetAsValues()
		{
			UpdateStatement statement = new Update("cmn", "Reference").GetQuery();
			statement.SetAsValues(new Dictionary<string, object>{ {"id", 1}});
			const string expectedSqlString = "UPDATE [cmn].[Reference] SET [id] = 1";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}

	}
}
