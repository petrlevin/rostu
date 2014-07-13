using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Activity.Values;
using Platform.BusinessLogic.Common.Exceptions;

namespace Platform.BusinessLogic.Tests
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class GetterTests
    {




        [Test]
        public void GetAllOfBaseNotDefault()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();
            var d = getter.GetAll(vc, t => typeof (Base).IsAssignableFrom(t), true);
            Assert.That(d.ContainsKey("First"));
            Assert.That(d.ContainsKey("Second"));
            Assert.That(d.ContainsKey("Third"));
            Assert.That(d.ContainsKey("Fourth"));
            Assert.That(!d.ContainsKey("Null"));
            Assert.AreEqual(4, d.Count);
            Assert.IsInstanceOf<Base>(d["First"]);
            Assert.IsInstanceOf<Base>(d["Second"]);
            Assert.IsInstanceOf<Base>(d["Third"]);
            Assert.IsInstanceOf<Base>(d["Fourth"]);
            Assert.AreEqual("First", ((Base) d["First"]).Caption);
            Assert.AreEqual("Second", ((Base) d["Second"]).Caption);
            Assert.AreEqual("Third", ((Base) d["Third"]).Caption);
            Assert.AreEqual("Fourth", ((Base) d["Fourth"]).Caption);

            //Assert.AreEqual("", d["Empty"]);

        }

        [Test]
        [ExpectedException(typeof(ValueExecutionException))]
        public void GetAllOfBadPropertyExecution()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();
            getter.GetAll(vc, t => t==typeof(HttpContext) , false);
        }

        [Test]
        public void GetAllOfBase()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();
            var d = getter.GetAll(vc, t => typeof (Base).IsAssignableFrom(t), false);
            Assert.That(d.ContainsKey("First"));
            Assert.That(d.ContainsKey("Second"));
            Assert.That(d.ContainsKey("Third"));
            Assert.That(d.ContainsKey("Fourth"));
            Assert.That(d.ContainsKey("Null"));
            Assert.AreEqual(5, d.Count);
            Assert.IsInstanceOf<Base>(d["First"]);
            Assert.IsInstanceOf<Base>(d["Second"]);
            Assert.IsInstanceOf<Base>(d["Third"]);
            Assert.IsInstanceOf<Base>(d["Fourth"]);
            Assert.AreEqual("First", ((Base) d["First"]).Caption);
            Assert.AreEqual("Second", ((Base) d["Second"]).Caption);
            Assert.AreEqual("Third", ((Base) d["Third"]).Caption);
            Assert.AreEqual("Fourth", ((Base) d["Fourth"]).Caption);
            Assert.IsNull(d["Null"]);
            //Assert.AreEqual("", d["Empty"]);

        }


        [Test]
        public void GetAllOfOnlyBase()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();
            var d = getter.GetAll(vc, t => typeof (Base) == t, false);
            Assert.That(d.ContainsKey("First"));
            Assert.That(d.ContainsKey("Second"));
            Assert.That(d.ContainsKey("Third"));
            Assert.That(d.ContainsKey("Null"));

            Assert.AreEqual(4, d.Count);
            Assert.IsInstanceOf<Base>(d["First"]);
            Assert.IsInstanceOf<Base>(d["Second"]);
            Assert.IsInstanceOf<Base>(d["Third"]);

            Assert.AreEqual("First", ((Base) d["First"]).Caption);
            Assert.AreEqual("Second", ((Base) d["Second"]).Caption);
            Assert.AreEqual("Third", ((Base) d["Third"]).Caption);
            Assert.IsNull(d["Null"]);
        }





        [Test]
        public void GetAllString()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();
            var d = getter.GetAll(vc, t => t == typeof (String), false);
            Assert.That(d.ContainsKey("FirstName"));
            Assert.That(d.ContainsKey("SecondName"));
            Assert.That(d.ContainsKey("Empty"));
            Assert.AreEqual(3, d.Count);
            Assert.AreEqual("Jane", d["FirstName"]);
            Assert.AreEqual("Smith", d["SecondName"]);
            Assert.AreEqual("", d["Empty"]);

        }


        [Test]
        public void GetAllInt32Int64()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();
            var d = getter.GetAll(vc, t => t == typeof (Int32) || t == typeof (Int64), false);
            Assert.That(d.ContainsKey("Length"));
            Assert.That(d.ContainsKey("Width"));
            Assert.That(d.ContainsKey("Height"));
            Assert.AreEqual(4, d.Count);
            Assert.AreEqual(5, d["Length"]);
            Assert.AreEqual(1000, d["Width"]);
            Assert.AreEqual(0, d["Height"]);
            Assert.AreEqual(5, d["LongLength"]);
        }

        [Test]
        public void GetAllScalar()
        {
            
            var vc = new BaseDefaults();
            var d = vc.GetScalarValues(false);
            Assert.That(d.ContainsKey("LongLength"));
            Assert.That(d.ContainsKey("Length"));
            Assert.That(d.ContainsKey("Width"));
            Assert.That(d.ContainsKey("Height"));
            Assert.That(d.ContainsKey("FirstName"));
            Assert.That(d.ContainsKey("SecondName"));
            Assert.That(d.ContainsKey("Empty"));
            Assert.That(d.ContainsKey("Nullable"));

            Assert.AreEqual(8, d.Count);
        }



        [Test]
        public void GetAllInt32()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();
            var d = getter.GetAll(vc, t => t == typeof (Int32), false);
            Assert.That(d.ContainsKey("Length"));
            Assert.That(d.ContainsKey("Width"));
            Assert.That(d.ContainsKey("Height"));
            Assert.AreEqual(3, d.Count);
            Assert.AreEqual(5, d["Length"]);
            Assert.AreEqual(1000, d["Width"]);
            Assert.AreEqual(0, d["Height"]);
        }

        [Test]
        public void GetAllInt32NotDEfault()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();
            var d = getter.GetAll(vc, t => t == typeof (Int32), true);
            Assert.That(d.ContainsKey("Length"));
            Assert.That(d.ContainsKey("Width"));
            Assert.AreEqual(2, d.Count);
            Assert.AreEqual(5, d["Length"]);
            Assert.AreEqual(1000, d["Width"]);
        }


        [Test]
        public void IntMethodGetNotParametrized()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();

            Assert.AreEqual(5, getter.Get(vc, "Count"));
        }


        [Test]
        public void IntMethodGet()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();

            Assert.AreEqual(5, getter.Get<Int32>(vc, "Count"));
        }


        [Test]
        public void IntPropertyGet()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();

            Assert.AreEqual(5, getter.Get<Int32>(vc, "Length"));
        }


        [Test]
        public void StringMethodGet()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();

            Assert.AreEqual("Jane", getter.Get<String>(vc, "Name"));
        }

        [Test]
        public void StringPropertyGet()
        {
            var getter = new Getter();
            var vc = new BaseDefaults();

            Assert.AreEqual("Smith", getter.Get<String>(vc, "SecondName"));
        }


        [Test]
        public void BaseGet()
        {
            var getter = new Getter();
            var vc = new Defaults();

            Assert.AreEqual("Smith", getter.Get<String>(vc, "SecondName"));
            Assert.AreEqual("Smith", getter.Get<String>(vc, "SecondName"));

        }

        [Test]
        [ExpectedException(typeof (ValueResolutionException))]
        public void BadDefinedValue()
        {
            var getter = new Getter();
            var vc = new Defaults();

            getter.Get<Int32>(vc, "BadDefinedValue");


        }

        [Test]
        [ExpectedException(typeof (ValueExecutionException))]
        public void BadExecutedValue()
        {
            var getter = new Getter();
            var vc = new Defaults();

            getter.Get<int>(vc, "BadExecutedValue");


        }





        public class Defaults : BaseDefaults
        {
            public int Number()
            {
                return 1;
            }

        }

        public class BaseDefaults
        {

            public Base First
            {
                get { return new Base() {Caption = "First"}; }
            }

            public Base Second
            {
                get { return new Base() {Caption = "Second"}; }
            }

            public Base Third
            {
                get { return new Inherited() {Caption = "Third"}; }
            }

            public Inherited Fourth
            {
                get { return new Inherited() {Caption = "Fourth"}; }
            }

            public Base Null
            {
                get { return null; }
            }


            public int Count()
            {
                return 5;
            }

            public int BadDefinedValue(int some)
            {
                return 5;
            }


            public int BadExecutedValue()
            {
                throw new Exception();
            }


            public long LongLength
            {
                get { return 5; }
            }

            public int Length
            {
                get { return 5; }
            }

            public int Width
            {
                get { return 1000; }
            }

            public int Height
            {
                get { return 0; }
            }


            public string Name()
            {
                return "Jane";
            }

            public string SecondName
            {
                get { return "Smith"; }
            }

            public string FirstName
            {
                get { return Name(); }
            }

            public string Empty
            {
                get { return ""; }
            }

            public Int32? Nullable
            {
                get { return 67; }
            }


            public HttpContext NotImplemented
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

        }

        public class Base
        {
            public string Caption { get; set; }
        }

        public class Inherited : Base
        {

        }

    }
}
