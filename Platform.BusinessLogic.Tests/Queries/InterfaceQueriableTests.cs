using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.BusinessLogic.EntityFramework;
using Platform.BusinessLogic.Queries;
using Platforms.Tests.Common;
using SomeBusiness;
using SomeBusiness.Reference;

namespace Platform.BusinessLogic.Tests.Queries
{
    [TestFixture]
    public class InterfaceQueriableTests : 
        SqlTestBase
    {

        [TestFixtureSetUp]
        public void SetUp()
        {
            using (var c = new BussinessTestContext())
            {
                //if (c.Database.Exists())
                //{
                //    c.Database.Delete();
                //}
                //c.Database.Create();
            }

            DbContextInitializer.TraceEnabled = false;
        }

        [Test]
        public void Test()
        {
            using (var c = new BussinessTestContext())
            {
                
                c.Somes.Add(new Some(){Name= "kjkj"});
                c.SaveChanges();
                var q =new InterfaceQueryable< ISome>(c.Somes);
                var result = q.Where(s => s.Name == "kjkj");
                var h = result.Where(s => s.Name == "kjkj");
                var  t = result.ToList();

                var  y = q.ToList();

                //c.Set<ISome>(Source);
            }
        }
    }
}
