using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using BaseApp.Common.Interfaces;
using BaseApp.Environment.Storages;
using BaseApp.Environment.Tests.Fixtures;
using BaseApp.Reference;
using BaseApp.SystemDimensions;
using NUnit.Framework;
using Platform.Dal.Common.Interfaces;
using Platform.Environment.Interfaces;
using Rhino.Mocks;

namespace BaseApp.Environment.Tests
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
    public class DependencyResolverTests: TestsBase
    {
        ApplicationStorage _applicationStorage;
        IUser _curentUser;
        IUser _curentUser1;
        SysDimensionsState _curentDimensions;
        SysDimensionsState _curentDimensions1;

        SessionStorage  _sessionStorage;
        SessionStorage _sessionStorage1;

        SqlConnection _dbConnection;
        SqlConnection _dbConnection1;

        RequestStorage _requestStorage;
        RequestStorage _requestStorage1;

        [SetUp]
        public void Init()
        {
            _applicationStorage = new ApplicationStorage();
            _curentUser = new User();

            _curentDimensions = new SysDimensionsState();

            _sessionStorage = new SessionStorage { CurrentUser = _curentUser, CurentDimensions = _curentDimensions };

            _dbConnection = new SqlConnection("Data Source=.;Initial Catalog=sbor;Integrated Security=false;User=bis;Password=bissupport");

            _requestStorage = new RequestStorage { DbConnection = _dbConnection };

            _curentUser1 = new User();
            _curentDimensions1 = new SysDimensionsState();

            _sessionStorage1 = new SessionStorage { CurrentUser = _curentUser1, CurentDimensions = _curentDimensions1 };

            _dbConnection1 = new SqlConnection("Data Source=.;Initial Catalog=sbor_data;Integrated Security=false;User=bis;Password=bissupport");
            _requestStorage1 = new RequestStorage
	            {
					DbConnection = _dbConnection1,
					Decorators = new List<TSqlStatementDecorator>
						{
							new FakeDecorator()
						}
	            };
        }

        //IStorageContainer<ApplicationStorage, SessionStorage, RequestStorage>
        [Test]
        public void WithIStorageContainer()
        {
            var storageContainer = MockRepository.GenerateMock<IStorageContainer<ApplicationStorage, SessionStorage, RequestStorage>>();

            storageContainer.Stub(s => s.RequestStorage).Return(_requestStorage);
            storageContainer.Stub(s => s.ApplicationStorage).Return(_applicationStorage);
            storageContainer.Stub(s => s.SessionStorage).Return(_sessionStorage);

            var dependencyResolver = new DependencyResolver(storageContainer);

            var onCurrentUserAndDimensions =  dependencyResolver.Resolve<Depends.OnCurrentUserAndDimensions>();
            var onDbConnection = dependencyResolver.Resolve<Depends.OnDbConnection>();

            Assert.AreSame(_curentUser, onCurrentUserAndDimensions.CurrentUser);
            Assert.AreSame(_curentDimensions, onCurrentUserAndDimensions.CurentDimensions);

            Assert.AreSame(_dbConnection, onDbConnection.DbConnection);
            Assert.IsNull(onDbConnection.OtherConnection);

            //Сессия поменялась
            storageContainer.BackToRecord(BackToRecordOptions.All);
            storageContainer.Replay();
            storageContainer.Stub(s => s.SessionStorage).Return(_sessionStorage1);

            var onCurrentUserAndDimensions1 = dependencyResolver.Resolve<Depends.OnCurrentUserAndDimensions>();
            Assert.AreSame(_curentUser1, onCurrentUserAndDimensions1.CurrentUser);
            Assert.AreSame(_curentDimensions1, onCurrentUserAndDimensions1.CurentDimensions);

            //Request поменялся
            storageContainer.BackToRecord(BackToRecordOptions.All);
            storageContainer.Replay();
            storageContainer.Stub(s => s.RequestStorage).Return(_requestStorage1);

            var onDbConnection1 = dependencyResolver.Resolve<Depends.OnDbConnection>();

            Assert.AreSame(_dbConnection1, onDbConnection1.DbConnection);
            Assert.IsNull(onDbConnection1.OtherConnection);
        }


        [Test]
        public void WithEnvironment()
        {
            GenerateEnvironment();

            var application = Factory.GenerateApplicationMock();
            var session = Factory.GenerateSessionMock();
            var request = Factory.GenerateRequestMock();

            var dependencyResolver = new DependencyResolver(BaseAppEnvironment);

            HttpContext.Stub(s => s.Items).Return(request);
            HttpContext.Stub(s => s.Application).Return(application);
            HttpContext.Stub(s => s.Session).Return(session);

            BaseAppEnvironment.ApplicationStart(_applicationStorage).SessionStart(_sessionStorage).RequestStart(_requestStorage);
            var onCurrentUserAndDimensions = dependencyResolver.Resolve<Depends.OnCurrentUserAndDimensions>();
            var onDbConnection = dependencyResolver.Resolve<Depends.OnDbConnection>();

            Assert.AreSame(_curentUser, onCurrentUserAndDimensions.CurrentUser);
            Assert.AreSame(_curentDimensions, onCurrentUserAndDimensions.CurentDimensions);

            Assert.AreSame(_dbConnection, onDbConnection.DbConnection);
            Assert.IsNull( onDbConnection.OtherConnection);

            var  context1 = MockRepository.GenerateMock<HttpContextBase>();
            var request1 = Factory.GenerateRequestMock();
            HttpContextProvider.BackToRecord(BackToRecordOptions.All);
            HttpContextProvider.Replay();

            HttpContextProvider.Stub(p => p.GetContext()).Return(context1);

            context1.Stub(s => s.Application).Return(application);
            context1.Stub(s => s.Session).Return(session);
            context1.Stub(s => s.Items).Return(request1);

            BaseAppEnvironment.RequestStart(_requestStorage1);
            var onCurrentUserAndDimensions1 = dependencyResolver.Resolve<Depends.OnCurrentUserAndDimensions>();
            var onDbConnection1 = dependencyResolver.Resolve<Depends.OnDbConnection>();
			
			var onDecorators = dependencyResolver.Resolve<Depends.OnDecorators>();
			Assert.IsTrue(onDecorators.Decorators.Single() is FakeDecorator);

            Assert.AreSame(_curentUser, onCurrentUserAndDimensions.CurrentUser);
            Assert.AreSame(_curentUser, onCurrentUserAndDimensions1.CurrentUser);

            Assert.AreSame(_curentDimensions, onCurrentUserAndDimensions.CurentDimensions);
            Assert.AreSame(_curentDimensions, onCurrentUserAndDimensions1.CurentDimensions);

            Assert.AreNotSame(_dbConnection, onDbConnection1.DbConnection);
            Assert.AreSame(_dbConnection1, onDbConnection1.DbConnection);
            Assert.IsNull(onDbConnection1.OtherConnection);
        }
    }
}
