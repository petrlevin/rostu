using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Common;

using Platform.PrimaryEntities.Factoring;
using Platform.Unity;

namespace Platforms.Tests.Common
{
	[ExcludeFromCodeCoverage]
	public class SqlTests : SqlTestBase
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            UnityContainer uc = new UnityContainer();

            DependencyInjection.RegisterIn(uc, false, false, connectionString);
            IoCServices.InitWith(new DependencyResolverBase(uc),false);

        }


    }
}
