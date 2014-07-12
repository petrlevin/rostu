using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sbor.Document;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Tablepart;

namespace Sbor.Tests
{
    [TestFixture]
    class CommonMethodsTest
    {
        #region Test Kbk
        public class TestKbk : KBK, ILineCost
        {
            public bool IsEqual(TestKbk other)
            {
                //
                if (this.IdBranchCode.HasValue && !other.IdBranchCode.HasValue ||
                    !this.IdBranchCode.HasValue && other.IdBranchCode.HasValue)
                    return false;
                
                if (this.IdBranchCode.HasValue && other.IdBranchCode.HasValue &&
                        this.IdBranchCode.Value != other.IdBranchCode.Value)
                    return false;
                //
                if (this.IdCodeSubsidy.HasValue && !other.IdCodeSubsidy.HasValue ||
                   !this.IdCodeSubsidy.HasValue && other.IdCodeSubsidy.HasValue)
                    return false;

                if (this.IdCodeSubsidy.HasValue && other.IdCodeSubsidy.HasValue &&
                        this.IdCodeSubsidy.Value != other.IdCodeSubsidy.Value)
                    return false;
                //
                if (this.IdDEK.HasValue && !other.IdDEK.HasValue ||
                    !this.IdDEK.HasValue && other.IdDEK.HasValue)
                    return false;

                if (this.IdDEK.HasValue && other.IdDEK.HasValue &&
                        this.IdDEK.Value != other.IdDEK.Value)
                    return false;
                //
                if (this.IdDKR.HasValue && !other.IdDKR.HasValue ||
                   !this.IdDKR.HasValue && other.IdDKR.HasValue)
                    return false;

                if (this.IdDKR.HasValue && other.IdDKR.HasValue &&
                        this.IdDKR.Value != other.IdDKR.Value)
                    return false;
                //
                if (this.IdDFK.HasValue && !other.IdDFK.HasValue ||
                    !this.IdDFK.HasValue && other.IdDFK.HasValue)
                    return false;

                if (this.IdDFK.HasValue && other.IdDFK.HasValue &&
                        this.IdDFK.Value != other.IdDFK.Value)
                    return false;
                //
                if (this.IdExpenseObligationType.HasValue && !other.IdExpenseObligationType.HasValue ||
                   !this.IdExpenseObligationType.HasValue && other.IdExpenseObligationType.HasValue)
                    return false;

                if (this.IdExpenseObligationType.HasValue && other.IdExpenseObligationType.HasValue &&
                        this.IdExpenseObligationType.Value != other.IdExpenseObligationType.Value)
                    return false;
                //
                if (this.IdFinanceSource.HasValue && !other.IdFinanceSource.HasValue ||
                    !this.IdFinanceSource.HasValue && other.IdFinanceSource.HasValue)
                    return false;

                if (this.IdFinanceSource.HasValue && other.IdFinanceSource.HasValue &&
                        this.IdFinanceSource.Value != other.IdFinanceSource.Value)
                    return false;
                //
                if (this.IdKCSR.HasValue && !other.IdDKR.HasValue ||
                   !this.IdKCSR.HasValue && other.IdDKR.HasValue)
                    return false;

                if (this.IdKCSR.HasValue && other.IdKCSR.HasValue &&
                        this.IdKCSR.Value != other.IdKCSR.Value)
                    return false;
                //
                if (this.IdKFO.HasValue && !other.IdKFO.HasValue ||
                   !this.IdKFO.HasValue && other.IdKFO.HasValue)
                    return false;

                if (this.IdKFO.HasValue && other.IdKFO.HasValue &&
                        this.IdKFO.Value != other.IdKFO.Value)
                    return false;
                //
                if (this.IdKOSGU.HasValue && !other.IdKOSGU.HasValue ||
                    !this.IdKOSGU.HasValue && other.IdKOSGU.HasValue)
                    return false;

                if (this.IdKOSGU.HasValue && other.IdKOSGU.HasValue &&
                        this.IdKOSGU.Value != other.IdKOSGU.Value)
                    return false;
                //
                if (this.IdKVR.HasValue && !other.IdKVR.HasValue ||
                   !this.IdKVR.HasValue && other.IdKVR.HasValue)
                    return false;

                if (this.IdKVR.HasValue && other.IdKVR.HasValue &&
                        this.IdKVR.Value != other.IdKVR.Value)
                    return false;
                //
                if (this.IdKVSR.HasValue && !other.IdKVSR.HasValue ||
                    !this.IdKVSR.HasValue && other.IdKVSR.HasValue)
                    return false;

                if (this.IdKVSR.HasValue && other.IdKVSR.HasValue &&
                        this.IdKVSR.Value != other.IdKVSR.Value)
                    return false;
                //
                if (this.IdRZPR.HasValue && !other.IdRZPR.HasValue ||
                   !this.IdRZPR.HasValue && other.IdRZPR.HasValue)
                    return false;

                if (this.IdRZPR.HasValue && other.IdRZPR.HasValue &&
                        this.IdRZPR.Value != other.IdRZPR.Value)
                    return false;

                return true;

            }
        }

        public TestKbk GetTestKbk()
        {
            return new TestKbk()
            {
                IdBranchCode = 1,
                IdCodeSubsidy = 2,
                IdDEK = null,
                IdDKR = 3,
                IdDFK = 4,
                IdExpenseObligationType = null,
                IdFinanceSource = 5,
                IdKCSR = 6,
                IdKFO = 7,
                IdKOSGU = null,
                IdKVR = 8,
                IdKVSR = 9,
                IdRZPR = null
            };
        }

        [Test]
        public void TestLineCostSetter()
        {
            var test = GetTestKbk();

            var other = new TestKbk();
            other.SetLineCostValues(test);

            Assert.IsTrue(other.IsEqual(test) );
        }

        [Test]
        public void TestLineCostClone()
        {
            var test = GetTestKbk();
            var other = test.CloneAsLineCost<TestKbk>();

            Assert.IsTrue(other.IsEqual(test));
        }
        #endregion

        #region GetNextCode
        [Test]
        public void TestGetNextCode1()
        {
            var test = new List<string>();
            var result = test.GetNextCode();

            Assert.AreEqual(result, "1");
        }

        [Test]
        public void TestGetNextCode2()
        {
            var test = new List<string>(){"1", "3", "2"};
            var result = test.GetNextCode();

            Assert.AreEqual(result, "4");
        }

        [Test]
        public void TestGetNextCode3()
        {
            var test = new List<string>() { "1", "3.1", "3", "4", "4.1", "4.2" };
            var result = test.GetNextCode();

            Assert.AreEqual(result, "5");
        }
        #endregion

        #region GetQueryString

        [Test]
        public void TestQueryString1()
        {
            var test = new List<string> {"a", "b", "c"};
            var result = test.GetQueryString();

            Assert.AreEqual("a, b, c", result);
        }

        [Test]
        public void TestQueryString2()
        {
            var test = new List<string> { "a", "b", "c" };
            var result = test.GetQueryString("alias");

            Assert.AreEqual("alias.a, alias.b, alias.c", result);
        }

        [Test]
        public void TestQueryString3()
        {
            var test = new List<string> { "a", "b", "c" };
            var result = test.GetQueryString("    al   ");

            Assert.AreEqual("al.a, al.b, al.c", result);
        }

        #endregion

        #region GetString

        [Test]
        public void TestGetString1()
        {
            var test = new List<string> { "a", "b", "c" };
            var result = test.GetString();

            Assert.AreEqual("a b c", result);
        }

        [Test]
        public void TestGetString2()
        {
            var test = new List<string> { "a", "b", "c" };
            var result = test.GetString(", ");

            Assert.AreEqual("a, b, c", result);
        }

        [Test]
        public void TestGetString3()
        {
            var test = new List<string> ();
            var result = test.GetString(", ");

            Assert.AreEqual(String.Empty, result);
        }

        #endregion

        [Test]
        public void TestVersioned()
        {
            SBP_Blank b = new SBP_Blank()
                {
                    IdBlankValueType_KOSGU = 1,
                    IdBlankValueType_DEK = 1,
                    IdBlankValueType_KVSR = 2
                };

            var doc = new FinancialAndBusinessActivities();
            doc.Id = 40;

            doc.GetWrongVersioningKBK(b, typeof (FBA_CostActivities), "FBA_Activity", null );


        }
    }
}
