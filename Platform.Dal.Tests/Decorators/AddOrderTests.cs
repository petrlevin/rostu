using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;

namespace Platform.Dal.Tests.Decorators
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	class AddOrderTests
    {
         [TestFixtureSetUp]
         public void SetUp()
         {
             IoC.InitWith(new DependencyResolverBase());
         }
    

		[Test]
		public void AddOrder_Exception()
		{
			Order order = new Order {{"name", true}};
			Entity entity = new Entity
			{
				EntityType = EntityType.Reference,
				Name = "Reference",
				Fields = new List<EntityField> { new EntityField { Name = "id" }, new EntityField { Name = "name" } }
			};
            var builder = new SelectQueryBuilder(entity) { Order = order };

			AddOrder addOrder = new AddOrder();
			Assert.Throws<Exception>(() => addOrder.Decorate(null, builder));
			Assert.Throws<Exception>(() => addOrder.Decorate(new UpdateStatement(), builder));
		}

		[Test]
		public void AddOrder_Asc()
		{
			string expectedSql = "SELECT [a].[id], [a].[name] FROM [ref].[Reference] AS [a] ORDER BY [a].[name] ASC";

			SelectQueryBuilder builder = getInitialBuilder();
			Order order = new Order { { "name", true } };
			
			builder.Order = order;
			Assert.AreEqual(expectedSql.ToTSqlStatement().Render(), decorateAndRender(builder));
		}

		[Test]
		public void AddOrder_Desc()
		{
			var expectedSql = "SELECT [a].[id], [a].[name] FROM [ref].[Reference] AS [a] ORDER BY [a].[name] DESC";
			
			SelectQueryBuilder builder = getInitialBuilder();
			Order order = new Order { { "name", false } };
			
			builder.Order = order;
			Assert.AreEqual(expectedSql.ToTSqlStatement().Render(), decorateAndRender(builder));
		}

		private SelectQueryBuilder getInitialBuilder()
		{
			Entity entity = new Entity
			{
				EntityType = EntityType.Reference,
				Name = "Reference",
				Fields = new List<EntityField> { new EntityField { Name = "id" }, new EntityField { Name = "name" } }
			};
            return new SelectQueryBuilder(entity);
		}

		private string decorateAndRender(SelectQueryBuilder builder)
		{
			AddOrder addOrder = new AddOrder();
			var statement = addOrder.Decorate(builder.GetTSqlStatement(), builder);
			return statement.Render();
		}
	}
}
