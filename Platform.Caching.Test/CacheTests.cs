using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.Caching.Common;

namespace Platform.Caching.Test
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class CacheTests
    {

        public class Dolar{}


        [Test]
        public void PutCorrect()
        {
            Cache cache = new Cache();
            cache.Put("something", 1, 2, 3);
            Assert.IsNotNull(cache.Get(1, 2, 3));
            Assert.IsNull(cache.Get(2, 1, 3));
                        
        }


        [Test]
        public void PutGetSame()
        {
            Cache cache = new Cache();
            cache.Put("something",1,2,3);
            Assert.AreSame("something", cache.Get(1, 2, 3));
            cache.Put("otherthing", 1, 2, 3);
            Assert.IsNotNull(cache.Get(1, 2, 3));
            Assert.IsNull(cache.Get(2, 1, 3));
            Assert.AreNotEqual("something", cache.Get(1, 2, 3));
            Assert.AreSame("otherthing", cache.Get(1, 2, 3));
            cache.Put("thirdthing", 1, 2, 4);
            Assert.AreSame("otherthing", cache.Get(1, 2, 3));
            Assert.AreSame("thirdthing", cache.Get(1, 2, 4));
            cache.Put("greenthing", 1, 2, 4,"green");
            Assert.AreSame("otherthing", cache.Get(1, 2, 3));
            Assert.AreSame("thirdthing", cache.Get(1, 2, 4));
            Assert.AreSame("greenthing", cache.Get(1, 2, 4,"green"));
            Dolar dolar = new Dolar();
            cache.Put(dolar, 1, 2, 3, "green");
            Assert.AreSame(dolar, cache.Get(1, 2, 3, "green"));
            Assert.AreSame(dolar, cache.Get(1, 2, 3, "green"));
            cache.Put(dolar, 1, 2, 4);
            Assert.AreSame(dolar, cache.Get(1, 2, 3, "green"));
            Assert.AreSame(dolar, cache.Get(1, 2, 4));
            Assert.IsNull(cache.Get(2, 1, 3,"ffff"));








        }


    }
}
