using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.BusinessLogic.Activity.Operations.Serialization;
using Platform.Common;
using Platform.Dal.Serialization;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Unity;
using Platforms.Tests.Common;
using Sbor;
using DependencyInjection = Platform.PrimaryEntities.Factoring.DependencyInjection;

namespace Platform.Dal.Tests
{
    [TestFixture]
    public class XmlSerializerTests:SqlTests
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

            var ser = new XmlDbSerializer(connection);
            ser.SerializeToRegistry(Objects.ById<Entity>(-1543503828), -1744830438);


        }
    }
}
