using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Registry;
using Platform.Common;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Unity;
using Platforms.Tests.Common;

namespace Platform.BusinessLogic.Tests
{
    [TestFixture]
    public class RegistryManagerTests: SqlTests
    {
        [SetUp]
        public void SetUp()
        {
            IUnityContainer uc = new UnityContainer();
            DependencyInjection.RegisterIn(uc, true, false, connectionString);
            IoC.InitWith(new DependencyResolverBase(uc));
            
        }

    
        [Test]
        public void Tests()
        {
            using (var dbContext = new Sbor.DataContext())
            {
                RegistryManager rm = new RegistryManager(dbContext);

                var result = rm.GetInfo(Objects.ById<Entity>(-1744830429), 1);
                var y = 9;
                
            //    var curQuery = dbContext.Set<IHasRegistrator>(-1744830428)
            //                                 .Where(r => (r.IdRegistrator == 1))
            //                                 .Select(r => new HCaption() {Caption = "jjhj"});








            //var q = curQuery.GroupBy(hc => hc.Caption)
            //             .Select(g => new RecordsInfo() {Caption = g.Key, Count = g.Count()});
            
            //var t =  q.ToList();

            }
        }
    }
}
