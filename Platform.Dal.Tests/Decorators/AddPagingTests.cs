using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;

namespace Platform.Dal.Tests.Decorators
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	class AddPagingTests
	{
        [TestFixtureSetUp]
        public void SetUp()
        {
            IoC.InitWith(new DependencyResolverBase());
        }


		[Test]
		public void AddPaging_Exception()
		{
			AddPaging addPaging = new AddPaging("alias", new Paging { Start = 0, Count = 10 });

			TSqlStatement statement = null;
			Assert.Throws<Exception>(() => addPaging.Decorate(statement, null));
			
			statement=new UpdateStatement();
			Assert.Throws<Exception>(() => addPaging.Decorate(statement, null));

			statement = new SelectStatement();
			Assert.Throws<Exception>(() => addPaging.Decorate(statement, null));

			(statement as SelectStatement).QueryExpression = new BinaryQueryExpression();
			Assert.Throws<Exception>(() => addPaging.Decorate(statement, null));

			Entity entity = new Entity
			{
				EntityType = EntityType.Reference,
				Name = "Reference",
				Fields = new List<EntityField> { new EntityField { Name = "id" }, new EntityField { Name = "name" } }
			};
			IPaging paging = new Paging { Count = 10, Start = 1 };
            SelectQueryBuilder builder = new SelectQueryBuilder(entity) { Paging = paging };
			statement = builder.GetTSqlStatement();
			addPaging = new AddPaging();
			Assert.Throws<Exception>(() => addPaging.Decorate(statement, builder));
		}

		[Test]
		public void AddPaging()
		{
	        const string expectedSqlString = @"
			WITH [alias] AS (
				SELECT [a].[id], [a].[name], ROW_NUMBER() OVER ( ORDER BY [a].[name]) AS [RowNumber] 
				FROM [ref].[Reference] AS [a]
			) 
			SELECT [a].[id], [a].[name] FROM [alias] AS [a] WHERE [RowNumber] BETWEEN 1 AND 10 ORDER BY [RowNumber] 
			";

			var sourceSql = "WITH [cte] AS (SELECT [a].[id], [a].[name] FROM [ref].[Reference]) SELECT [a].[id], [a].[name] FROM [cte] ORDER BY [name]";
			TSqlStatement statement = sourceSql.ToTSqlStatement();

			//SelectStatement selectStatement = (statement as SelectStatement);
			//OrderByClause orderByClause = selectStatement.OrderByClause;
			//(selectStatement.QueryExpression as QuerySpecification).SelectElements.Add(Helper.CreateRowNumber(orderByClause, "RowNumber"));
			//FunctionCall function = Helper.CreateFunctionCall("COUNT", new List<Expression> {"1".ToLiteral(LiteralType.Integer)});
			//function.OverClause = new OverClause();
			//(selectStatement.QueryExpression as QuerySpecification).SelectElements.Add(Helper.CreateColumn(function, "_TotalRow"));

			IPaging paging = new Paging { Count = 10, Start = 1 };
			AddPaging addPaging=new AddPaging("alias", paging);
			statement = addPaging.Decorate(statement, null);

			var tt = statement.Render();
		}

		[Test]
		public void AddPaging2()
		{
			
		}

	}
}
