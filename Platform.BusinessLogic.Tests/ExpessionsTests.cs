using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.BusinessLogic.Activity.Values;

namespace Platform.BusinessLogic.Tests
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class ExpessionsTests
    {
        [Test]
        public void Test()
        {
            var e = new Evaluator();
            var result = e.Evaluate("5+{Some}+{Other}",new SomeClass() {Some=9,Other=100});
            Assert.AreEqual(114,result);
        }

        [Test]
        public void Test1()
        {
            var e = new Evaluator();
            var result = e.Evaluate("5+'{StringProperty}'+{Other}", new SomeClass() { StringProperty = "gg90", Other = 100 });
            Assert.AreEqual("5gg90100", result);
        }

        [Test]
        public void Test2()
        {
            var e = new Evaluator();
            var result = e.Evaluate("{StringProperty}", new SomeClass() { StringProperty = "gg90", Other = 100 });
            Assert.AreEqual("gg90", result);
        }


        public class SomeClass
        {
            public int Some { get; set; }
            public int Other { get; set; }
            public string StringProperty { get; set; }
        }
    }
}
