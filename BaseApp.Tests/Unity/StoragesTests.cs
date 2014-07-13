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

namespace BaseApp.Tests.Unity
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class StoragesTests
	{
		private PlatformEnvironmentMock env;

		[SetUp]
		public void Init()
		{
			env = new PlatformEnvironmentMock();
		}

		[Test]
		public void ShouldInjectDependencies()
		{
			var theUserName = "The User Name";
			var connStr1 = "Initial Catalog=sbor1";

			var session = new SessionStorage
			{
				CurrentUser = new User { Name = theUserName },
				CurentDimensions = new SysDimensionsState
					{
						{ SysDimension.PublicLegalFormation, Guid.Empty }
					}
			};
			var request = new RequestStorage
			{
				DbConnection = new SqlConnection(connStr1)
			};

			env.SessionStart(session);
			env.RequestStart(request);


			var obj2 = env.Container.Resolve<Depends.OnDbConnection>();
			var obj = env.Container.Resolve<Depends.OnCurrentUserAndDimensions>();

			Assert.AreEqual(connStr1, obj2.DbConnection.ConnectionString);
			Assert.AreEqual(theUserName, obj.CurrentUser.Name);
			Assert.AreEqual(Guid.Empty, obj.CurentDimensions[SysDimension.PublicLegalFormation]);
		}

		[Test]
		public void OnlyDependantPropertiesShouldBeResolved()
		{
			var connStr1 = "Initial Catalog=sbor1";
			var request = new RequestStorage
			{
				DbConnection = new SqlConnection(connStr1)
			};

			var obj = env.RequestStart(request).Container.Resolve<Depends.OnDbConnection>();

			Assert.AreEqual(connStr1, obj.DbConnection.ConnectionString);
			Assert.IsNull(obj.OtherConnection);
		}

		/// <summary>
		/// Разные запросы должны открывать собственные соединения с БД.
		/// </summary>
		[Test]
		public void DifferentRequestsShouldHaveDifferentConnections()
		{
			var connStr1 = "Initial Catalog=sbor1";
			var connStr2 = "Initial Catalog=sbor2";

			var reqStorage1 = new RequestStorage()
			{
				DbConnection = new SqlConnection(connStr1)
			};

			var reqStorage2 = new RequestStorage()
			{
				DbConnection = new SqlConnection(connStr2)
			};

			env.RequestStart(reqStorage1);
			env.RequestStart(reqStorage2);

			var obj1 = env.SetState(1, 1).Container.Resolve<Depends.OnDbConnection>();
			var obj2 = env.SetState(1, 2).Container.Resolve<Depends.OnDbConnection>();

			Assert.AreEqual(connStr1, obj1.DbConnection.ConnectionString);
			Assert.AreEqual(connStr2, obj2.DbConnection.ConnectionString);
		}
	}
}
