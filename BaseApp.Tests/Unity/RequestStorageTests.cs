using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using BaseApp.DbEnums;
using BaseApp.Environment;
using BaseApp.Environment.Storages;
using BaseApp.References;
using BaseApp.SystemDimensions;
using BaseApp.Tests.Unity.Fixtures;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Common;

namespace BaseApp.Tests.Unity
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class RequestStorageTests
	{
		private const string userName = "User Name";
		private const string connStr1 = "Initial Catalog=sbor1";
		private const string connStr2 = "Initial Catalog=sbor2";

		private PlatformEnvironmentMock env;

		/// <summary>
		/// Инициируем старт приложения и одной сессии и двух запросов
		/// </summary>
		[SetUp]
		public void Init()
		{
			env = new PlatformEnvironmentMock();
			IoC.InitWith(env);

			var app = new ApplicationStorage();
			var session1 = new SessionStorage
				{
					CurrentUser = new User() { Name = userName },
					CurentDimensions = new SysDimensionsState() 
					{
						{ SysDimension.PublicLegalFormation, Guid.Empty }
					}
				};
			var req1 = new RequestStorage()
			{
				DbConnection = new SqlConnection(connStr1)
			};

			var req2 = new RequestStorage()
			{
				DbConnection = new SqlConnection(connStr2)
			};

			// Среда из одной сессии и двух запросов
			env
				.ApplicationStart(app)
				.SessionStart(session1)
				.RequestStart(req1)
				.RequestStart(req2);
		}

		[Test]
		public void TestFirstRequest()
		{
			env.SetState(1, 1);
			var obj1 = IoC.Resolve<Depends.OnDbConnection>();
			Assert.AreEqual(connStr1, obj1.DbConnection.ConnectionString);
		}

		[Test]
		public void TestSecondRequest()
		{
			env.SetState(1, 2);
			var obj2 = IoC.Resolve<Depends.OnDbConnection>();
			Assert.AreEqual(connStr2, obj2.DbConnection.ConnectionString);
		}

		[Test]
		public void NonDependantConnectionShouldBeNull()
		{
			env.SetState(1, 1);
			var obj2 = IoC.Resolve<Depends.OnDbConnection>();
			Assert.IsNull(obj2.OtherConnection);
		}
	}
}
