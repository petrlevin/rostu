using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.QueryBuilders;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;
using Platforms.Tests.Common;

namespace Platform.BusinessLogic.Tests.ServerFilters
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	class ServerFiltersDecoratorTests:SqlTests
	{
		[SetUp]
		public void SetUp()
		{
			IUnityContainer uc = new UnityContainer();
			DependencyInjection.RegisterIn(uc,true,false,connectionString);
			IoC.InitWith(new DependencyResolverBase(uc));
		}

		[Test]
		public void ServerFiltersDecorator_Decorate_IdEntityField()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter filter = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "1"
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter);
			TSqlStatement statement = query.GetTSqlStatement();
			TSqlStatement statement2 = serverFiltersDecorator.Decorate(statement, query);
			Assert.AreEqual(statement.Render(), statement2.Render());
		}

		[Test]
		public void ServerFiltersDecorator_Decorate_Builder()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter filter = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "1"
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) {IdEntityField = 0};
			TSqlStatement statement = query.GetTSqlStatement();
			TSqlStatement statement2 = serverFiltersDecorator.Decorate(statement, null);
			Assert.AreEqual(statement.Render(), statement2.Render());
		}

		[Test]
		public void ServerFiltersDecorator_Decorate_Filter()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator {IdEntityField = 0};
			TSqlStatement statement = query.GetTSqlStatement();
			TSqlStatement statement2 = serverFiltersDecorator.Decorate(statement, query);
			Assert.AreEqual(statement.Render(), statement2.Render());
		}

		[Test]
		public void ServerFiltersDecorator()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter filter = new Filter
				{
					IdLogicOperator = (byte) LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "1"
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) {IdEntityField = 1};
			query.QueryDecorators.Add(serverFiltersDecorator);
			SqlCommand command = query.GetSqlCommand(new SqlConnection());
		}

		[Test]
		public void ServerFiltersDecorator_fromSimpleFilter_Exception_IdLeftEntityField()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter filter = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "1"
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) { IdEntityField = 1 };
			TSqlStatement statement = query.GetTSqlStatement();
			Assert.Throws<Exception>(() => serverFiltersDecorator.Decorate(statement, query));
		}

		[Test]
		public void ServerFiltersDecorator_fromSimpleFilter_Exception_NotExistRightPartExpression()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter filter = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) { IdEntityField = 1 };
			TSqlStatement statement = query.GetTSqlStatement();
			Assert.Throws<Exception>(() => serverFiltersDecorator.Decorate(statement, query));
		}

		[Test]
		public void ServerFiltersDecorator_fromFilter_NotSimple()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter inFilter1 = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "1"
				};
			Filter inFilter2 = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "2"
				};
			Filter filter = new Filter
				{
					IdLogicOperator = (byte) LogicOperator.Or, 
					ChildFilter = new List<Filter> {inFilter1, inFilter2}
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) { IdEntityField = 1 };
			serverFiltersDecorator.Decorate(query.GetTSqlStatement(), query);
		}

		[Test]
		public void ServerFiltersDecorator_fromFilter_NotSimple_TwoLevel()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter inFilter21 = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "2"
				};
			Filter inFilter22 = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "3"
				};
			
			Filter inFilter1 = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					RightValue = "1"
				};
			Filter inFilter2 = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.And, 
					ChildFilter = new List<Filter> { inFilter21, inFilter22 }
				};
			
			Filter filter = new Filter { IdLogicOperator = (byte)LogicOperator.Or, ChildFilter = new List<Filter> { inFilter1, inFilter2 } };
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) { IdEntityField = 1 };
			serverFiltersDecorator.Decorate(query.GetTSqlStatement(), query);
		}

		[Test]
		public void ServerFiltersDecorator_fromSimpleFilter_IdRightEntityField()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter filter = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.Equal, 
					IdRightEntityField = 1
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) { IdEntityField = 1, FieldValues = new FieldValues { {1, 22}}};
			serverFiltersDecorator.Decorate(query.GetTSqlStatement(), query);
		}

		[Test]
		public void ServerFiltersDecorator_fromSimpleFilter_RightSqlExpression()
		{
			Entity entity = Objects.ById<Entity>(1);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter filter = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 0, 
					IdComparisionOperator = (byte)ComparisionOperator.InList, 
					RightSqlExpression = "SELECT id FROM ref.EntityField WHERE [idEntity]=1 AND Name={Name}"
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) { IdEntityField = 1, FieldValues = new FieldValues { { 1, "id" } } };
			serverFiltersDecorator.Decorate(query.GetTSqlStatement(), query);
		}

		[Test]
		public void ServerFiltersDecorator1()
		{
			Entity entity = Objects.ById<Entity>(134217747);
			QueryBuilder query = new SelectQueryBuilder(entity, null);
			Filter filter = new Filter
				{
					IdLogicOperator = (byte)LogicOperator.Simple, 
					IdLeftEntityField = 134217902, 
					IdComparisionOperator = (byte)ComparisionOperator.InList, 
					RightValue = "'469762048', '603979801', '603979819', '603979825'"
				};
			ServerFiltersDecorator serverFiltersDecorator = new ServerFiltersDecorator(filter) { IdEntityField = 134217909, FieldValues = new FieldValues { { 1, "id" } } };
			serverFiltersDecorator.Decorate(query.GetTSqlStatement(), query);
		}
	}
}
