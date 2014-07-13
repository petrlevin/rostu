using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Transactions;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;
using Platform.Utils.Collections;

namespace Platform.Dal.Tests.QueryBuilders
{
	[TestFixture]
	[ExcludeFromCodeCoverage]
	internal class SelectQueryBuilderTests : DalTestsBase
	{

		[Test]
		public void SelectQuery_Exception()
		{
			Assert.Throws<Exception>(() => new SelectQueryBuilder().GetTSqlStatement());
		}

		/// <summary>
		/// Простой запрос на выборку всех полей
		/// </summary>
		[Test]
		public void SimpleSelect()
		{
			string expectedSql = "SELECT [a].[id],[a].[name],[a].[fieldType] FROM [ref].[Reference] AS [a]";
			Entity entity = getSimpleEntity();
			var builder = new SelectQueryBuilder(entity);
			TSqlStatement statement = builder.GetTSqlStatement(); // без применения приватных декораторов
			Assert.AreEqual(expectedSql.ToTSqlStatement().Render(), statement.Render());
		}

		/// <summary>
		/// Выборка указанных полей
		/// </summary>
		[Test]
		public void SelectFieldsByNames()
		{
			Entity entity = getSimpleEntity();
			var builder = new SelectQueryBuilder(entity, new List<string> {"id"});
			TSqlStatement statement = builder.GetTSqlStatement();
			var expectedSql = "SELECT [a].[id] FROM [ref].[Reference] [a]";
			Assert.AreEqual(expectedSql.ToTSqlStatement().Render(), statement.Render());
		}

		/// <summary>
		/// Должны срабатывать приватные декораторы
		/// </summary>
		[Test]
		public void ShouldApplyPrivateDecorators()
		{
			const string expectedSql = @"
				WITH     [Reference]
				AS       (SELECT [a].[id],
								 [a].[name],
								 [a].[fieldType],
								 ROW_NUMBER() OVER ( ORDER BY [a].[name] ASC) AS [RowNumber]
						  FROM   [ref].[Reference] AS [a]
						  WHERE  [a].[id] = 1)
				SELECT   [a].[id],
						 [a].[name],
						 [a].[fieldType],
						 [b].[Caption] AS [fieldType_Caption]
				FROM     [Reference] AS [a] INNER JOIN [enm].[EntityFieldType] [b] ON [a].[fieldType]=[b].[id]
				WHERE    [RowNumber] BETWEEN 0 AND 9
				ORDER BY [RowNumber]
			";

			Entity entity = getSimpleEntity();
            var builder = new SelectQueryBuilder(entity);

			/*
			 * Для условия, пэйджинга и сортировки указать примитивные значения.
			 * Проверяется сам факт срабатывания соответствующих декораторов: AddCaptions, AddWhere, AddOrder, AddPaging.
			 * Сами же декораторы должны тестироваться не здесь (например, все разнообразие условий отбора - в AddWhereTests).
			 */
			builder.Conditions = new FilterConditions() {Field = "id", Value = 1, Operator = ComparisionOperator.Equal};
			builder.Paging = (IPaging)new Paging() {Start=0, Count=10};
			builder.Order = new Order() {{"name", true}};

			/*
			 * При срабатывании AddCaptions произойдет обращение к Entity.ById за получением сущности, на которое ссылается fieldType.
			 * ToDo: В тестах при этом не должно происходить обращения к БД, кеш должнен предварительно заполняться в инициализируюмем методе.
			 */

			SqlCommand cmd = builder.GetSqlCommand(this.connection);
			Assert.AreEqual(expectedSql.ToTSqlStatement().Render(), cmd.CommandText);
		}

		[Test]
        [Ignore]
        public void Test_Crud_Entity()
		{
			string expectedSqlString =
		        "SELECT [a].[id], [a].[Name], [a].[idEntityType], [a].[Description] FROM [ref].[Entity] AS [a]";
			Entity entity = Objects.ById<Entity>(-2147483615);
            QueryBuilder query = new SelectQueryBuilder(entity, null);
			TSqlStatement statement = query.GetTSqlStatement();
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statement.Render());
			SqlCmd sqlCmd = new SqlCmd(this.connection);
			
			FilterConditions filter = new FilterConditions { Field = "id", Value = 1, Operator = ComparisionOperator.Equal };
			AddWhere addWhere = new AddWhere(filter);
			TSqlStatement statementWithWhere = addWhere.Decorate(statement, query);
	        expectedSqlString =
				"SELECT [a].[id], [a].[Name], [a].[idEntityType], [a].[Description] FROM [ref].[Entity] AS [a] WHERE [id] = 1";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statementWithWhere.Render());

			QueryBuilder insertQuery = new InsertQueryBuilder(entity, new IgnoreCaseDictionary<object> { { "Name", "Test" }, { "idEntityType", 3 } });
			sqlCmd.ExecuteNonQuery(insertQuery.GetTSqlStatement().Render());

            QueryBuilder updateQuery = new UpdateQueryBuilder(entity, new Dictionary<string, object> { { "Name", "Test" } });
			filter = new FilterConditions { Field = "Name", Value = "Test", Operator = ComparisionOperator.Equal };
			addWhere = new AddWhere(filter);
			statement = updateQuery.GetTSqlStatement();
			statementWithWhere = addWhere.Decorate(statement, query);
	        expectedSqlString =
		        "UPDATE [a] SET [Name] = 'Test' FROM [ref].[Entity] AS [a] WHERE [Name] = 'Test'";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statementWithWhere.Render());
			sqlCmd.ExecuteNonQuery(statementWithWhere.Render());

            QueryBuilder deleteQuery = new DeleteQueryBuilder(entity);
			filter = new FilterConditions { Field = "Name", Value = "Test", Operator = ComparisionOperator.Equal };
			addWhere = new AddWhere(filter);
			statement = deleteQuery.GetTSqlStatement();
			statementWithWhere = addWhere.Decorate(statement, query);
	        expectedSqlString = "DELETE [a] FROM [ref].[Entity] AS [a] WHERE [Name] = 'Test'";
			Assert.AreEqual(expectedSqlString.ToTSqlStatement().Render(), statementWithWhere.Render());
			sqlCmd.ExecuteNonQuery(statementWithWhere.Render());
		}

		/// <summary>
		/// Возвращает сущность справочника из двух полей.
		/// </summary>
		/// <returns></returns>
		private Entity getSimpleEntity()
		{
			return new Entity
				{
					EntityType = EntityType.Reference,
					Name = "Reference",
					Fields = new List<EntityField>
						{
							new EntityField { Name = "id" }, 
							new EntityField { Name = "name" },
							new EntityField { Name = "fieldType", IdEntityFieldType = (int)EntityFieldType.Link, IdEntityLink = 4 }
						}
				};
		}

		[Test]
		public void Test()
		{
			Entity entity = Objects.ById<Entity>(-2147483615);
			SelectQueryBuilder query = new SelectQueryBuilder(entity, null);
			query.Search = "test";
			TSqlStatement statement = query.GetTSqlStatement();
			//AddGridSearch addGridSearch = new AddGridSearch();
			//statement = addGridSearch.Decorate(statement, query);
		}
	}
}
