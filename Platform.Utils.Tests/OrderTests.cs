using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;
using Platform.Utils.Orderable;

namespace Platform.Utils.Tests
{
    [TestFixture]
    public class OrderTests
    {
        [Test]
        public void Test1()
        {
            List<object> list = new List<object>();
            list.Add(new Class2());
            list.Add(new Class1());

            var orderer = new Orderer();
            var result = orderer.SetOrder(list);
            Assert.IsInstanceOf<Class1>(result[0]);

        }


        [Test]
        public void Test2()
        {
            List<object> list = new List<object>();
            list.Add(new Class3());
            list.Add(new Class2());
            list.Add(new Class1());
            

            var orderer = new Orderer();
            var result = orderer.SetOrder(list);
            Assert.IsInstanceOf<Class1>(result[0]);
            Assert.IsInstanceOf<Class2>(result[1]);

        }


        [Test]
        public void Test3()
        {
            List<object> list = new List<object>();
            list.Add(new Class3());
            list.Add(new Class2());
            list.Add(new Class1());


            var orderer = new Orderer();
            var result = orderer.SetOrder(list);
            Assert.IsInstanceOf<Class1>(result[0]);
            Assert.IsInstanceOf<Class2>(result[1]);

        }

        [Test]
        public void Test4()
        {
            List<object> list = new List<object>();

            list.Add(new Class3());
            list.Add(new Class2());
            list.Add(new Class1());
            list.Add(new Class4());


            var orderer = new Orderer();
            var result = orderer.SetOrder(list);
            Assert.IsInstanceOf<Class1>(result[0]);
            Assert.IsInstanceOf<Class2>(result[1]);
            Assert.IsInstanceOf<Class4>(result[2]);
            Assert.IsInstanceOf<Class3>(result[3]);

        }

        [Test]
        [Ignore]
        public void Test5()
        {
            List<object> list = new List<object>();
            list.Add(new Class5());
            list.Add(new Class3());
            list.Add(new Class2());
            list.Add(new Class1());
            list.Add(new Class4());


            var orderer = new Orderer();
            var result = orderer.SetOrder(list);
            Assert.IsInstanceOf<Class1>(result[0]);
            Assert.IsInstanceOf<Class2>(result[1]);
            Assert.IsInstanceOf<Class4>(result[2]);
            Assert.IsInstanceOf<Class5>(result[3]);
            Assert.IsInstanceOf<Class3>(result[4]);

        }



    }


    public class Class1 : IOrdered
    {
        public IEnumerable<Type> Before
        {
            get { return new List<Type>(); }
        }

        public IEnumerable<Type> After
        {
            get { return new List<Type>(); }
        }

        public Order WantBe
        {
            get { return Order.First; }
        }
    }

    public class Class2 : IOrdered
    {
        public IEnumerable<Type> Before
        {
            get { return new List<Type>(){typeof(Class3)}; }
        }

        public IEnumerable<Type> After
        {
            get { return new List<Type>(); }
        }

        public Order WantBe
        {
            get { return Order.DoesNotMatter; }
        }
    }

    public class Class3 : IOrdered
    {
        public IEnumerable<Type> Before
        {
            get { return null; }
        }

        public IEnumerable<Type> After
        {
            get { return null;  }
        }

        public Order WantBe
        {
            get { return Order.DoesNotMatter; }
        }
    }


    public class Class4 : IOrdered
    {
        public IEnumerable<Type> Before
        {
            get { return new List<Type>(){typeof(Class3)}; }
        }

        public IEnumerable<Type> After
        {
            get { return null; }
        }

        public Order WantBe
        {
            get { return Order.First; }
        }
    }

    public class Class5 : IOrdered
    {
        public IEnumerable<Type> Before
        {
            get { return null; }
        }

        public IEnumerable<Type> After
        {
            get
            {
                return new List<Type>(){typeof(Class4)};
            }
        }

        public Order WantBe
        {
            get { return Order.First; }
        }
    }



}