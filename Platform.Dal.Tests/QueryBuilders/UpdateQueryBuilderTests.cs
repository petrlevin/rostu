using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.QueryBuilders;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;

namespace Platform.Dal.Tests.QueryBuilders
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class UpdateQueryBuilderTests : DalTestsBase
	{
        [TestFixtureSetUp]
        public void SetUp()
        {
            IoC.InitWith(new DependencyResolverBase());
        }


        //[Test]
        //public void UpdateQueryBuilder_Exception()
        //{
        //    Assert.Throws<Exception>(() => new UpdateQueryBuilder().GetTSqlStatement());
        //    QueryBuilder query=new UpdateQueryBuilder();
        //    query.Entity=new Entity();
        //    Assert.Throws<Exception>(() => query.GetTSqlStatement());
        //}

		[Test]
		public void UpdateQueryBuilder()
		{
			string expectedSqlString = "UPDATE [a]\r\nSET  FROM [ref].[Reference] AS [a]";
			
			Entity entity = getEntity();
			var builder = new UpdateQueryBuilder(entity);
			TSqlStatement statement = builder.GetTSqlStatement();
			
			Assert.AreEqual(expectedSqlString, statement.Render(options));
		}

		[Test]
		public void UpdateQueryBuilder_Fields()
		{
			var expectedSqlString = "UPDATE [a] SET [id] = 1 FROM [ref].[Reference] AS [a]";
			
			Entity entity = getEntity();
			var builder = new UpdateQueryBuilder(entity, new Dictionary<string, object> { { "id", 1 } });
			var statement = builder.GetTSqlStatement();
			
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}

		private Entity getEntity()
		{
			return new Entity
				{
					EntityType = EntityType.Reference,
					Name = "Reference",
					Fields = new List<EntityField> {new EntityField {Name = "id"}, new EntityField {Name = "name"}}
				};
		}
	}
}
