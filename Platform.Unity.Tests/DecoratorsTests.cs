using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Unity.Common;

namespace Platform.Unity.Tests
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class DecoratorsTests
    {

    

        [Test]
        public void TestWithBaseTypeRegistrtion()
        {
            IUnityContainer unityContainer = new UnityContainer();
            unityContainer.RegisterType(typeof (ISome), typeof (Some), "Some");
            unityContainer.RegisterDecorator(typeof (ISome), typeof (Decorator1), "Some");
            ISome some1 = unityContainer.Resolve<ISome>("Some");
            ISome @base = ((Decorator)some1).Inner;
            @base.Property1 = "1";
            @base.Property2 = "2";
            @base.Property3 = "3";
            @base.Property4 = "4";

            Assert.IsInstanceOf<Decorator1>(some1);
            Assert.IsInstanceOf<Some>(((Decorator)some1).Inner);
            Assert.AreSame(null, some1.Property1);
            Assert.AreSame("2", some1.Property2);
            Assert.AreSame("3", some1.Property3);
            Assert.AreSame("4", some1.Property4);
            some1.Property1 = "1000";
            some1.Property2 = "10002";
            Assert.AreSame("1", @base.Property1);
            Assert.AreSame("10002", @base.Property2);
            Assert.AreSame("1000", some1.Property1);
            ISome some2 = unityContainer.Resolve<ISome>("Some");
            Assert.AreNotSame(some2, some1);
            

            unityContainer.RegisterDecorator(typeof(ISome), typeof(Decorator2), "Some");
            some2 = unityContainer.Resolve<ISome>("Some");
            Assert.IsInstanceOf<Decorator2>(some2);
            @base = ((Decorator)((Decorator)some2).Inner).Inner;
            @base.Property1 = "1";
            @base.Property2 = "2";
            @base.Property3 = "3";
            @base.Property4 = "4";

            Assert.AreSame("3", some2.Property3);
            Assert.AreSame("4", some2.Property4);
            Assert.AreSame(null, some2.Property1);
            Assert.AreSame(null, some2.Property2);

            unityContainer.RegisterDecorator(typeof(ISome), typeof(Decorator3), "Some");
            ISome some3 = unityContainer.Resolve<ISome>("Some");
            Assert.IsInstanceOf<Decorator3>(some3);


        }

        [Test]
        public void TestWithBaseInstanceRegistrtion()
        {
            IUnityContainer unityContainer = new UnityContainer();
            Some @base = new Some {Property1 = "1",Property2 = "2",Property3 = "3",Property4 = "4"};
            unityContainer.RegisterInstance(typeof(ISome), "Some", @base);

            unityContainer.RegisterDecorator(typeof(ISome), typeof(Decorator1), "Some");
            ISome some1 = unityContainer.Resolve<ISome>("Some");
            Assert.IsInstanceOf<Decorator1>(some1);
            Assert.AreSame(((Decorator)some1).Inner,@base);
            Assert.AreSame(null,some1.Property1);
            Assert.AreSame("2",some1.Property2);
            Assert.AreSame("3",some1.Property3);
            Assert.AreSame("4",some1.Property4);
            some1.Property1 = "1000";
            some1.Property2 = "10002";
            Assert.AreSame("1",@base.Property1);
            Assert.AreSame("10002",@base.Property2);
            Assert.AreSame("1000",some1.Property1);
            ISome some2 = unityContainer.Resolve<ISome>("Some");
            Assert.AreSame(some2, some1);


            unityContainer.RegisterDecorator(typeof(ISome), typeof(Decorator2), "Some");
            some2 = unityContainer.Resolve<ISome>("Some");
            Assert.IsInstanceOf<Decorator2>(some2);
            Assert.AreSame("3", some2.Property3);
            Assert.AreSame("4", some2.Property4);
            Assert.AreSame("1000", some2.Property1);
            Assert.AreSame(null, some2.Property2);
            some2.Property2 = "Revenge";
            Assert.AreNotSame("Revenge", some1.Property2);
            Assert.AreNotSame("Revenge", @base.Property2);
            some2.Property1 = "books";
            Assert.AreNotSame("books", @base.Property1);
            Assert.AreSame("books", some1.Property1);
            unityContainer.RegisterDecorator(typeof(ISome), typeof(Decorator3), "Some");
            ISome some3 = unityContainer.Resolve<ISome>("Some");
            Assert.AreNotSame(some3, some1);
            Assert.IsInstanceOf<Decorator3>(some3);
            Assert.AreSame(((Decorator)some3).Inner, some2);
            some3.Property3 = "mine";
            some3.Property4 = "@base";
            some3.Property2 = "too2";
            some3.Property1 = "too1";

            Assert.AreNotSame("mine", @base.Property3);
            Assert.AreSame("@base", @base.Property4);
            Assert.AreNotSame("too2", @base.Property2);
            Assert.AreSame("too2", some2.Property2);
            Assert.AreSame("too1", some2.Property1);
            Assert.AreSame("too1", some1.Property1);
            Assert.AreNotSame("too1", @base.Property1);


        }


        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void BadDecoratorUse()
        {
            IUnityContainer unityContainer = new UnityContainer();
            Some @base = new Some { Property1 = "1", Property2 = "2", Property3 = "3", Property4 = "4" };
            unityContainer.RegisterInstance(typeof(ISome), "Some", @base);
            unityContainer.RegisterDecorator(typeof(ISome), typeof(BadDecorator), "Some");
            
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NoTypeToDecorate()
        {
            IUnityContainer unityContainer = new UnityContainer();
            unityContainer.RegisterDecorator(typeof(ISome), typeof(Decorator1), "Some");

        }



        public interface ISome
        {
            string Property1 { get; set; }
            string Property2 { get; set; }
            string Property3 { get; set; }
            string Property4 { get; set; }
        }

		[ExcludeFromCodeCoverage]
		abstract public class Decorator
        {
            public ISome Inner { get; protected set; }

            protected Decorator(ISome inner)
            {
                Inner = inner;
            }

            public Decorator()
            {

            }
        }

		[ExcludeFromCodeCoverage]
		public class Decorator1 : Decorator, ISome
        {
            public Decorator1(ISome inner)
                : base(inner)
            {
            }

            public string Property1 { get; set; }

            public string Property2
            {
                get { return Inner.Property2; }
                set { Inner.Property2 = value; }
            }

            public string Property3
            {
                get { return Inner.Property3; }
                set { Inner.Property3 = value; }
            }

            public string Property4
            {
                get { return Inner.Property4; }
                set { Inner.Property4 = value; }
            }
        }

		[ExcludeFromCodeCoverage]
		public class Decorator2 : Decorator, ISome
        {

            public  ISome InnerAnotherNameAndWritable
            {
                get
                {
                    return base.Inner;
                }
                set { base.Inner = value; }
            }

            public string Property2 { get; set; }

            public string Property1
            {
                get { return Inner.Property1; }
                set { Inner.Property1 = value; }
            }

            public string Property3
            {
                get { return Inner.Property3; }
                set { Inner.Property3 = value; }
            }

            public string Property4
            {
                get { return Inner.Property4; }
                set { Inner.Property4 = value; }
            }
        }

		[ExcludeFromCodeCoverage]
		public class Decorator3 : Decorator, ISome
        {
            public Decorator3(ISome inner)
                : base(inner)
            {
            }

            public string Property3 { get; set; }

            public string Property2
            {
                get { return Inner.Property2; }
                set { Inner.Property2 = value; }
            }

            public string Property1
            {
                get { return Inner.Property1; }
                set { Inner.Property1 = value; }
            }

            public string Property4
            {
                get { return Inner.Property4; }
                set { Inner.Property4 = value; }
            }
        }


		[ExcludeFromCodeCoverage]
		public class BadDecorator : ISome
        {
		    public BadDecorator(ISome some)
		    {
		        _inner = some;
		    }

		    private readonly ISome _inner;

            public string Property3 { get; set; }

            public string Property2
            {
                get { return Inner.Property2; }
                set { Inner.Property2 = value; }
            }

            protected ISome Inner
            {
                get { return _inner; }
            }

            public string Property1
            {
                get { return Inner.Property1; }
                set { Inner.Property1 = value; }
            }

            public string Property4
            {
                get { return Inner.Property4; }
                set { Inner.Property4 = value; }
            }

            
        }



        public class Some : ISome
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
        }


    }
}
