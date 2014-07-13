using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Platform.Caching.Test
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class KeyTests
    {
		[ExcludeFromCodeCoverage]
		class Some
        {
            public Object RefObject { get; set; }
            public Object ValObject { get; set; }
            public override bool Equals(object obj)
            {
                if (obj.GetType() != GetType())
                    return false;
                Some other = obj as Some;
                if (RefObject != other.RefObject)
                    return false;
                if (!Object.Equals(ValObject,other.ValObject))
                    return false;
                return true;
            }

            public override int GetHashCode()
            {

                return 100;
            }

        }

        [Test]
        public void ShouldBeEqual()
        {
            Key k1 = new Key(1);
            Key k2 = new Key(1);
            Assert.AreEqual(k1,k2);
            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            k1 = new Key("Gloria in Excelsis Deo");
            k2 = new Key("Gloria in Excelsis Deo");
            Assert.AreEqual(k1, k2);
            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            k1 = new Key("Gloria in Excelsis Deo",1,500);
            k2 = new Key("Gloria in Excelsis Deo",1,500);
            Assert.AreEqual(k1, k2);
            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            k1 = new Key(true,"Gloria in Excelsis Deo", 500);
            k2 = new Key(true,"Gloria in Excelsis Deo", 500);
            Assert.AreEqual(k1, k2);
            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            k1 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            Assert.AreEqual(k1, k2);
            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            k1 = new Key(89541,true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            Assert.AreEqual(k1, k2);
            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            Object instance = new Object();

            k1 = new Key(new Some {RefObject = instance , ValObject = "QWERTY"} , 89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(new Some {RefObject = instance , ValObject = "QWERTY"} ,89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            Assert.AreEqual(k1, k2);
            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            


        }


        [Test]
        public void ShouldNotBeEqual()
        {
            Key k1 = new Key(1);
            Key k2 = new Key(2);
            Assert.AreNotEqual(k1, k2);
            
            k1 = new Key("Gloria in Excelsis Deo");
            k2 = new Key("Gloria in Excelsis Deo ha ha ha");
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(new Object());
            k2 = new Key("Gloria in Excelsis Deo ha ha ha");
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(600);
            k2 = new Key("Gloria in Excelsis Deo ha ha ha");
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(600);
            k2 = new Key(600,"Gloria in Excelsis Deo ha ha ha");
            Assert.AreNotEqual(k1, k2);
            
            k1 = new Key("Gloria in Excelsis Deo", 1, 500);
            k2 = new Key("Gloria in Excelsis Deo", 1, 5000);
            Assert.AreNotEqual(k1, k2);
            
            k1 = new Key(true, "Gloria in Excelsis Deo", 5);
            k2 = new Key(true, "Gloria in Excelsis Deo", 500);
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(true, "Gloria in Excelsis Deo", 500);
            k2 = new Key(false, "Gloria in Excelsis Deo", 500);
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89077, "Sic transit gloria mundi");
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(false, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(false, "Gloria in Excelsis Deo oh oh oh", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(false, "Gloria in Excelsis Deo ha ha ha", 500, 0.89076, "Sic transit gloria mundi");
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi", "Ups");
            Assert.AreNotEqual(k1, k2);


            k1 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi Ups");
            k2 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi", "Ups");
            Assert.AreNotEqual(k1, k2);

            k1 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundis","Forever", 890 , new short());
            k2 = new Key(true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi", "Ups");
            Assert.AreNotEqual(k1, k2);

            Assert.AreEqual(k1.GetHashCode(), k2.GetHashCode());
            k1 = new Key(89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            Assert.AreEqual(k1, k2);

            Object instance = new Object();
            k1 = new Key(new Some { RefObject = instance, ValObject = "_QWERTY" }, 89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(new Some { RefObject = instance, ValObject = "QWERTY" }, 89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            Assert.AreNotEqual(k1, k2);

            Object instance1 = new Object();
            k1 = new Key(new Some { RefObject = instance, ValObject = "QWERTY" }, 89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            k2 = new Key(new Some { RefObject = instance1, ValObject = "QWERTY" }, 89541, true, "Gloria in Excelsis Deo", 500, 0.89076, "Sic transit gloria mundi");
            Assert.AreNotEqual(k1, k2);




        }

    }
}
