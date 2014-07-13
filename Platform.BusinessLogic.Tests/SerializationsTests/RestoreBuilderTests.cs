using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.BusinessLogic.Activity.Operations.Serialization;
using Platform.Common;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Unity;
using Platforms.Tests.Common;

namespace Platform.BusinessLogic.Tests.SerializationsTests
{
    [TestFixture]
    public class RestoreBuilderTests : SqlTests
    {

        [SetUp]
        public void SetUp()
        {
            IUnityContainer uc = new UnityContainer();
            DependencyInjection.RegisterIn(uc, true, false, connectionString);
            IoC.InitWith(new DependencyResolverBase(uc));
        }


        [Test]
        public void Test()
        {
            var restoreBuilder = new RestoreBuilder(Objects.ById<Entity>(-1543503828));
            var result = restoreBuilder.Build();
            

        }

    }
}
