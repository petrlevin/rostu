using System.Collections.Generic;
using NUnit.Framework;
using Sbor.Logic.Hierarchy;

namespace Sbor.Tests
{
    [TestFixture]
    public class HierarchyTest
    {
        public class TestEntity
        {
            public int Id;
            public int? ParentId;
            public string Name;
            public int TypeId;

            public TestEntity(int id, int? parentId, int typeId, string name)
            {
                Id = id;
                ParentId = parentId;
                Name = name;
                TypeId = typeId;
            }

        }

        public IEnumerable<TestEntity> GetTestCollection1()
        {
            var temp = new List<TestEntity>
                {
                    new TestEntity(1, null, 1, "1"),
                    new TestEntity(2, 1, 2, "1.1"),
                    new TestEntity(3, 1, 3, "1.2"),
                    new TestEntity(4, 1, 3, "1.3"),
                    new TestEntity(5, 1, 3, "1.4"),
                    new TestEntity(6, 2, 3, "1.1.1"),
                    new TestEntity(7, 3, 5, "1.2.1")
                };

            return temp;
        }

        [Test]
        public void TestNearestParent()
        {
            var test = GetTestCollection1();

            var correctResults = new Dictionary<int, KeyValuePair<int, int>>
                {
                    {2, new KeyValuePair<int, int>(1,1) },
                    {3, new KeyValuePair<int, int>(1,1) },
                    {4, new KeyValuePair<int, int>(1,1) },
                    {5, new KeyValuePair<int, int>(1,1) },
                    {6, new KeyValuePair<int, int>(2,1) },
                    {7, new KeyValuePair<int, int>(1,2) },
                   
                };

            for (var testAnchorId = 2; testAnchorId <= 7; testAnchorId++)
            {
                var result = test.NearestParentId(testAnchorId, idS => idS.Id, idP => idP.ParentId,
                                                    find => find.TypeId == 1 || find.TypeId == 2);
                
                var correctResult = correctResults[testAnchorId];

                Assert.AreEqual(result.Key, correctResult.Key);
                Assert.AreEqual(result.Value, correctResult.Value);
            }
        }

        [Test]
        public void TestParentIds1()
        {
            var test = GetTestCollection1();

            var testAnchorId = 7;
            var result = test.GetParentsIds(testAnchorId, idS => idS.Id, idPS => idPS.ParentId,
                                            stop => false);

            Assert.AreEqual( result.Count, 2);
            Assert.AreEqual( result[1], 2);
            Assert.AreEqual( result[3], 1);
        }

        [Test]
        public void TestParentIds2()
        {
            var test = GetTestCollection1();

            var testAnchorId = 7;
            var result = test.GetParentsIds(testAnchorId, idS => idS.Id, idPS => idPS.ParentId,
                                            stop => stop.TypeId == 1);

            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(result[1], 2);
            Assert.AreEqual(result[3], 1);
        }

        [Test]
        public void TestParentIds3()
        {
            var test = GetTestCollection1();

            var testAnchorId = 7;
            var result = test.GetParentsIds(testAnchorId, idS => idS.Id, idPS => idPS.ParentId,
                                            stop => stop.TypeId == 3);

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[3], 1);
        }

        [Test]
        public void TestDescendants1()
        {
            var test = GetTestCollection1();

            var testAnchorId = 1;
            var result = test.GetDescendantsIds(testAnchorId, idS => idS.Id, idPS => idPS.ParentId);

            Assert.AreEqual(6, result.Count);

            foreach (var i in new[]{2,3,4,5,6,7})
            {
                Assert.Contains(i, result);    
            }
        }

        [Test]
        public void TestDescendants2()
        {
            var test = GetTestCollection1();

            var testAnchorId = 4;
            var result = test.GetDescendantsIds(testAnchorId, idS => idS.Id, idPS => idPS.ParentId);

            Assert.AreEqual(0, result.Count);

        }

        [Test]
        public void TestDescendants3()
        {
            var test = GetTestCollection1();

            var testAnchorId = 3;
            var result = test.GetDescendantsIds(testAnchorId, idS => idS.Id, idPS => idPS.ParentId);

            Assert.AreEqual(1, result.Count);

            Assert.Contains(7, result);
        }

    }
}
