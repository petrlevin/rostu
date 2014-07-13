using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.DbEnums;
using BaseApp.Environment.Dependencies;
using BaseApp.Environment.Storages;
using BaseApp.References;
using BaseApp.SystemDimensions;
using BaseApp.Tests.Unity.Fixtures;
using NUnit.Framework;
using Microsoft.Practices.Unity;
using Platform.Common;

namespace BaseApp.Tests.Unity
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class SessionStorageTests
	{
		private const string userName = "The User Name";
		private const string connStr1 = "Initial Catalog=sbor1";
		private const string connStr2 = "Initial Catalog=sbor1";

		private PlatformEnvironmentMock env;

		/// <summary>
		/// Инициируем среду
		/// </summary>
		[SetUp]
		public void Init()
		{
			env = new PlatformEnvironmentMock();
			IoC.InitWith(env);

			var app = new ApplicationStorage();
			var session1 = new SessionStorage
			{
				CurrentUser = new User { Name = userName },
				CurentDimensions = new SysDimensionsState
					{
						{ SysDimension.PublicLegalFormation, Guid.Empty }
					}
			};
			var request1a = new RequestStorage
			{
				DbConnection = new SqlConnection(connStr1)
			};

			// Среда из одной сессии и одного реквеста
			env
				.ApplicationStart(app)
				.SessionStart(session1)
				.RequestStart(request1a);
		}

		[Test]
		public void ConnectionShouldBeResolved()
		{
			env.SetState(1, 1);
			var obj = IoC.Resolve<Depends.OnDbConnection>();
			Assert.AreEqual(connStr1, obj.DbConnection.ConnectionString);
		}

		[Test]
		public void UserShouldBeResolved()
		{
			env.SetState(1, 1);
			var obj = IoC.Resolve<Depends.OnCurrentUserAndDimensions>();
			Assert.AreEqual(userName, obj.CurrentUser.Name);
		}

		[Test]
		public void CurentDimensionsShouldBeResolved()
		{
			env.SetState(1, 1);
			var obj = IoC.Resolve<Depends.OnCurrentUserAndDimensions>();
			Assert.AreEqual(Guid.Empty, obj.CurentDimensions[SysDimension.PublicLegalFormation]);
		}
	}
}
