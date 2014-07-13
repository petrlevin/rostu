using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platform.Unity.Tests
{
    [TestFixture]
    public class LifiTimeManagerFactoryTests
    {
        [Test]
        public void Test()
        {
            var fact = new LifiTimeManagerFactory<Storage>();
            var stor = new Storage();
            var man =fact.CreateManager(() => stor, s => s.Some);
            var s1 = man.GetValue();
            Assert.IsNull(s1);
            s1 = new Some();
            man.SetValue(s1);
            Assert.AreSame(stor.Some,s1);
            var s2 = man.GetValue();
            Assert.AreSame(s1, s2);

        }

        public class Storage
        {
            public Some Some { get; set; }
        }

        public class Some
        {
            
        }
    }
}
