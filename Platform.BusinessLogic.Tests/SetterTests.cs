using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.BusinessLogic.Activity.Values;
using Platform.BusinessLogic.Common.Exceptions;

namespace Platform.BusinessLogic.Tests
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class SetterTests
    {
        [Test]
        [ExpectedException(typeof(ValueResolutionException))]
        public void UnexistedPropery()
        {
            var container = new Container();

            var setter = new Setter();
            setter.Set(container,"Unexisted",6000);


        }

        [Test]
        public void ShortToInt()
        {
            var f = typeof(Int32).IsAssignableFrom(typeof(Int16));
            ;
            var container = new Container();

            var setter = new Setter();
            
            setter.Set(container, "Width", (short)45);

            Assert.AreEqual(45, container.Width);

            

        }

        [Test]
        public void IntToDecimal()
        {
            var container = new Container();

            var setter = new Setter();

            setter.Set(container, "Decimal", (int)45);

            Assert.AreEqual(45, container.Decimal);



        }


        [Test]
        public void DoubleToDecimal()
        {
            var container = new Container();

            var setter = new Setter();

            setter.Set(container, "Decimal", (double)45.7);

            Assert.AreEqual(45.7, container.Decimal);



        }



        [Test]
        public void IntToSort()
        {
            var f = typeof(Int32).IsAssignableFrom(typeof(Int16));
            ;
            var container = new Container();

            var setter = new Setter();

            setter.Set(container, "Short", (int)45);

            Assert.AreEqual(45, container.Short);

        }


        [Test]
        public void IntToIntNullable()
        {
            var f = typeof(Int32).IsAssignableFrom(typeof(Int16));
            ;
            var container = new Container();

            var setter = new Setter();

            setter.Set(container, "IntNullable", (int)45);

            Assert.AreEqual(45, container.IntNullable.Value);

        }

        [Test]
        public void LongToIntNullable()
        {
            var f = typeof(Int32).IsAssignableFrom(typeof(Int16));
            ;
            var container = new Container();

            var setter = new Setter();

            setter.Set(container, "IntNullable", (long)45);

            Assert.AreEqual(45, container.IntNullable.Value);

        }



        [Test]
        public void Null()
        {
            var container = new Container();

            var setter = new Setter();
            container.Name = "Jane";
            setter.Set(container, "Name", null);

            Assert.AreEqual(null, container.Name);

        }


        [Test]
        public void Int32()
        {
            var container = new Container();

            var setter = new Setter();
            setter.Set(container, "Width", 6000);
            Assert.AreEqual(6000,container.Width);

        }

        [Test]
        public void DateTime()
        {
            var container = new Container();

            var setter = new Setter();
            setter.Set(container, "DateTime", new DateTime(1997,2,2));

            Assert.AreEqual(new DateTime(1997, 2, 2), container.DateTime);

        }

        [Test]
        public void DateTimeToDateTimeNullable()
        {
            var container = new Container();

            var setter = new Setter();
            setter.Set(container, "DateTimeNullable", new DateTime(1997, 2, 2));

            Assert.AreEqual(new DateTime(1997, 2, 2), container.DateTimeNullable);

        }


        [Test]
        public void String()
        {
            var container = new Container();

            var setter = new Setter();
            setter.Set(container, "Name", "Jane");
            Assert.AreEqual("Jane", container.Name);

        }

        [Test]
        public void Dict()
        {
            var container = new Container();

            var setter = new Setter();
            var props = new Dictionary<string, Object>() {{"Name", "Petr"}, {"Width", 100}};
            setter.Set(container, props);
            Assert.AreEqual("Petr", container.Name);
            Assert.AreEqual(100, container.Width);

        }


        [Test]
        [ExpectedException(typeof(ValueConvertException))]
        public void BadArgumentType()
        {
            var container = new Container();

            var setter = new Setter();
                setter.Set(container, "Width", "jj5");
            
            //Assert.AreEqual(6000, container.Width);

        }



        public class Container
        {
            public decimal Decimal { get; set; }
            public int Width { get; set; }
            public int? IntNullable { get; set; }
            public short Short { get; set; }
            public string Name { get; set; }
            public DateTime DateTime { get; set; }
            public DateTime? DateTimeNullable { get; set; }
        
        }


    }
}
