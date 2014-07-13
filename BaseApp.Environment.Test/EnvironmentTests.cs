using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BaseApp.Environment.Storages;
using NUnit.Framework;
using Platform.Environment;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace BaseApp.Environment.Tests
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
    public class EnvironmentTests:TestsBase
    {
        //Stub для HttpContext
        //= MockRepository.GenerateMock<IStateProvider>();

        [SetUp]
        public void Init()
        {
            GenerateEnvironment();
        }

        [Test]
        public void TestStorageSet()
        {
            var session = Factory.GenerateSessionMock();
            HttpContext.Stub(s=>s.Session).Return(session);
            var sessionStorage = new SessionStorage();
            BaseAppEnvironment.SessionStart(sessionStorage);
            Assert.AreSame(BaseAppEnvironment.SessionStorage, sessionStorage);
        }

        [Test]
        public void TestApplicationSet()
        {
            var applicattion = Factory.GenerateApplicationMock();
            HttpContext.Stub(s => s.Application).Return(applicattion);
            var applicationStorage = new ApplicationStorage();
            BaseAppEnvironment.ApplicationStart(applicationStorage);
            Assert.AreSame(BaseAppEnvironment.ApplicationStorage, applicationStorage);
        }

        [Test]
        public void TestRequestSet()
        {
            var request = Factory.GenerateRequestMock();
            HttpContext.Stub(s => s.Items).Return(request);
            var requestStorage = new RequestStorage();
            BaseAppEnvironment.RequestStart(requestStorage);
            Assert.AreSame(BaseAppEnvironment.RequestStorage, requestStorage);
        }
    }
}
