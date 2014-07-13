using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.Multilink;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Dal.Tests;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;

namespace Platform.Dal.Tests
{
	/// <summary>
	/// Внимание! Тесты запросов с декораторами находятся в BaseApp.Tests.
	/// </summary>
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class QueryFactoryTests : DalTestsBase
	{
		[Test]
		public void Select()
		{
			const string expected = @"
			WITH [Entity] AS (
				SELECT 
					[a].[id], 
					[a].[Name], 
					ROW_NUMBER() OVER ( ORDER BY [a].[Name] ASC) AS [RowNumber] 
			FROM 
				[ref].[Entity] AS [a] 
			WHERE 
				[a].[Name] = 'some string value'
			) 
			SELECT [a].[id], [a].[Name] FROM [Entity] AS [a] WHERE [RowNumber] BETWEEN 0 AND 9 ORDER BY [RowNumber] 
			";

			/*
			 * Представим, что мы в панели навигации кликаем на какую-либо сущность.
			 * У нас открывается таб со списком элементов данной сущности.
			 * Что отправляется на сервер?
			 * - Имя сущности
			 * - Список полей
			 * - Клиентские фильтры, пэйджинг, поиск, сортировка
			 */

			Entity entity = //Entity.ById(1);
				new Entity()
					{
						Name = "Entity",
						IdEntityType = (int)EntityType.Reference,
						Fields = new List<EntityField>()
							{
								new EntityField() { Name = "id", IdEntityFieldType = (int)EntityFieldType.Int },
								new EntityField() { Name = "Name", IdEntityFieldType = (int)EntityFieldType.String },
							}
					};
			//Form form = new Form { Entity = entity };

			ISelectQueryBuilder selectBuilder = new QueryFactory(entity).Select();
			
			selectBuilder.Fields = new List<string>
				{
					"id",
					"Name"
				};

			selectBuilder.Paging = new Paging() {Start = 0, Count = 10};

			selectBuilder.Order = new Order() { { "Name", true } };

			selectBuilder.Conditions = new FilterConditions()
				{
					Field = "Name",
					Value = "some string value"
				};
			string sql = selectBuilder.GetSqlCommand(this.connection).CommandText;

			Assert.AreEqual(expected.ToTSqlStatement().Render(), sql);
		}
	}
}
