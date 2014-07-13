using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Rhino.Mocks;

namespace BaseApp.Environment.Tests
{
	[ExcludeFromCodeCoverage]
	internal static class Factory
    {
        
        static public  HttpSessionStateBase GenerateSessionMock()
        {
            var session = MockRepository.GeneratePartialMock<HttpSessionStateBase>();
            session.Stub(s => s["SessionStorage"]).PropertyBehavior();
            return session;


        }

        static public  HttpApplicationStateBase GenerateApplicationMock()
        {
            var application = MockRepository.GeneratePartialMock<HttpApplicationStateBase>();
            application.Stub(s => s["ApplicationStorage"]).PropertyBehavior();
            return application;


        }

        static public  IDictionary GenerateRequestMock()
        {
            var request = MockRepository.GenerateMock<IDictionary>();
            request.Stub(s => s["RequestStorage"]).PropertyBehavior();
            return request;


        }

    }
}
