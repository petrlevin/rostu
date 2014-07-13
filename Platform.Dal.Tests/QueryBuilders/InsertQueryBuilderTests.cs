using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.QueryBuilders;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;
using Platform.Utils.Collections;

namespace Platform.Dal.Tests.QueryBuilders
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	class InsertQueryBuilderTests
	{
        [TestFixtureSetUp]
        public void SetUp()
        {
            IoC.InitWith(new DependencyResolverBase());
        }


        //[Test]
        //public void InsertQuery_Exception()
        //{
        //    QueryBuilder query;
        //    query =new InsertQueryBuilder();
        //    Assert.Throws<Exception>(() => query.GetTSqlStatement());
			
        //    query.Entity=new Entity();
        //    Assert.Throws<Exception>(() => query.GetTSqlStatement());
        //}

		[Test]
		public void InsertQuery()
		{
			Entity entity = new Entity
				{
					EntityType = EntityType.Reference,
					Name = "Reference",
					Fields = new List<EntityField> {new EntityField {Name = "id"}, new EntityField {Name = "name"}}
				};
            QueryBuilder query = new InsertQueryBuilder(entity, new List<string> { "id" });
			TSqlStatement statement = query.GetTSqlStatement();
			Assert.AreEqual("INSERT INTO [ref].[Reference] ([id])\r\n", statement.Render());

            query = new InsertQueryBuilder(entity, new IgnoreCaseDictionary<object> { { "id", 1 } });
			statement = query.GetTSqlStatement();
	        string expectedSqlString = "INSERT  INTO [ref].[Reference] ([id])\r\nVALUES                        (1)";
            Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());

            query = new InsertQueryBuilder(entity, fieldNames: null);
			statement = query.GetTSqlStatement();
			Assert.AreEqual("INSERT INTO [ref].[Reference] ([id], [name])\r\n", statement.Render());
		}

		[Test]
		public void TestInsertEntity()
		{
		}
	}
}
