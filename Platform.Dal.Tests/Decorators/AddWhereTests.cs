using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.QueryBuilders;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;

namespace Platform.Dal.Tests.Decorators
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	class AddWhereTests
	{
        [TestFixtureSetUp]
        public void SetUp()
        {
            IoC.InitWith(new DependencyResolverBase());
        }


		[Test]
		public void AddWhere_Exception()
		{
			AddWhere where = new AddWhere(new FilterConditions());
			Assert.Throws<Exception>(() => where.Decorate(null, null));

			TSqlStatement statement = new UpdateStatement();
			Assert.Throws<Exception>(() => where.Decorate(statement, null));

			statement = new SelectStatement();
			Assert.Throws<Exception>(() => where.Decorate(statement, null));

			(statement as SelectStatement).QueryExpression = new BinaryQueryExpression();
			Assert.Throws<Exception>(() => where.Decorate(statement, null));
		}

		[Test]
		public void AddWhere()
		{
			string expectedSqlString = "SELECT [a].[id], [a].[name] FROM [ref].[Reference] AS [a] WHERE [a].[id] = 1";
			Entity entity = new Entity
				{
					EntityType = EntityType.Reference,
					Name = "Reference",
					Fields = new List<EntityField> {new EntityField {Name = "id"}, new EntityField {Name = "name"}}
				};
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			TSqlStatement statement = query.GetTSqlStatement();
			FilterConditions filter = new FilterConditions {Field = "id", Value = 1, Operator = ComparisionOperator.Equal};
			AddWhere addWhere = new AddWhere(filter);
			statement = addWhere.Decorate(statement, query);
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());

			FilterConditions filterAdd = new FilterConditions {Field = "id", Value = 2, Operator = ComparisionOperator.Equal};
			addWhere = new AddWhere(filterAdd);
			statement = addWhere.Decorate(statement, query);
			expectedSqlString =
				"SELECT [a].[id], [a].[name] FROM [ref].[Reference] AS [a] WHERE [a].[id] = 1 OR [a].[id] = 2";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());

			FilterConditions filterAdd2 = new FilterConditions {Field = "id", Value = 3, Operator = ComparisionOperator.Equal};
			addWhere = new AddWhere(filterAdd2, LogicOperator.And, true, true);
			statement = addWhere.Decorate(statement, query);
			expectedSqlString =
				"SELECT [a].[id], [a].[name] FROM [ref].[Reference] AS [a] WHERE ([a].[id] = 1 OR [a].[id] = 2) AND ([a].[id] = 3)";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
		}
	}
}
