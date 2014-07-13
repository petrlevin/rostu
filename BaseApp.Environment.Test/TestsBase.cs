using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Platform.Environment;
using Rhino.Mocks;

namespace BaseApp.Environment.Tests
{
	[ExcludeFromCodeCoverage]
	public class TestsBase
    {
        protected HttpContextBase HttpContext;
        protected IHttpContextProvider HttpContextProvider;
        protected BaseAppEnvironment BaseAppEnvironment;

        protected void GenerateEnvironment()
        {
            HttpContext = MockRepository.GeneratePartialMock<HttpContextBase>();
            HttpContextProvider = MockRepository.GenerateMock<IHttpContextProvider>();
            HttpContextProvider.Stub(p => p.GetContext()).Return(HttpContext);
            BaseAppEnvironment = new BaseAppEnvironment(HttpContextProvider);
        }
    }
}
