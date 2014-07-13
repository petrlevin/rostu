using System;
using System.Diagnostics.CodeAnalysis;
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
	class DeleteQueryBuilderTests
	{
        [TestFixtureSetUp]
        public void SetUp()
        {
            IoC.InitWith(new DependencyResolverBase());
        }


        //[Test]
        //public void DeleteQuery_Exception()
        //{
        //    DeleteQueryBuilder builder = new DeleteQueryBuilder(new Entity());
        //    Assert.Throws<Exception>(() => builder.GetTSqlStatement());
			
        //    Assert.Throws<Exception>(() => new DeleteQueryBuilder().GetTSqlStatement());
        //}

		[Test]
		public void DeleteQuiery()
		{
			Entity entity = new Entity {EntityType = EntityType.Reference, Name = "Reference"};

            QueryBuilder query = new DeleteQueryBuilder(entity);
			TSqlStatement statement = query.GetTSqlStatement();
	        const string expectedSqlString = "DELETE [a] FROM [ref].[Reference] AS [a]";
            Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}

	}
}
