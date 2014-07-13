using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Caching.Common;
using Platform.Common;
using Platform.PrimaryEntities.Factoring.FactoryElements;
using Platform.PrimaryEntities.Factoring.Strategies;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Tests.Mocks;
using Platform.Unity;
using Rhino.Mocks;

namespace Platform.PrimaryEntities.Factoring.Tests.Strategies
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class DefaultTests
    {

        [Test]
        public void ShouldGetSameResultAsMockedAndLoadWithExpectedArgsCalled()
        {

            var strategy = new Default<IDictionary<string, object>, SelectMock>();
            var factoryElementMock = MockRepository.GeneratePartialMock<BaseFactoryElement<SomeMetadata, IDictionary<string, object>>, IFactoryElement<SomeMetadata>>(strategy);
            IFactory factoryMock = MockRepository.GenerateMock<IFactory>();
            var expectedObject = new SomeMetadata();
            factoryElementMock.Stub(f => f.Load<IDictionary<string, object>>(executeResultJohn)).IgnoreArguments().Return(expectedObject);

            ISelect<IDictionary<string, object>> select = strategy.CreateSelect(1, "id", "SomeMetadata");
            SomeMetadata result = strategy.GetResult(select, (IFactoryElement<SomeMetadata>)factoryElementMock);

            factoryElementMock.AssertWasCalled(f => f.Load<IDictionary<string, object>>(executeResultJohn));
            Assert.AreSame(expectedObject, result);
        }

        [Test]
        [Repeat(3)]
        public void ShouldGetJohn()
        {

            var strategy = new Default<IDictionary<string, object>, SelectMock>();
            var factoryElement = new OfSingle<SomeMetadata, IDictionary<string, object>>(strategy);
            IFactory factoryMock = MockRepository.GenerateMock<IFactory>();



            ISelect<IDictionary<string, object>> select = strategy.CreateSelect(1, "id", "SomeMetadata");
            SomeMetadata result = strategy.GetResult(select, (IFactoryElement<SomeMetadata>)factoryElement);


            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("John", result.Name);
            Assert.AreEqual("Red", result.Color);
            Assert.AreEqual("Perfect", result.Skill);
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            UnityContainer uc = new UnityContainer();
            uc.RegisterInstance<IFactory>("MetadataObjectsFactory", MockRepository.GenerateMock<IFactory>());

            IoC.InitWith(new DependencyResolverBase(uc));

        }


        public static IEnumerable Strategies
        {
            get
            {
                yield return new Default<IDictionary<string, object>, SelectMock>();


                UseCache<IDictionary<string, object>> useCache = new UseCache<IDictionary<string, object>>(
                    new Default<IDictionary<string, object>, SelectMock>());
                useCache.Cache = MockRepository.GenerateMock<ICache>();
                yield return useCache;


            }
        }  

        

        [Test, TestCaseSource("Strategies")]
        public void GetManyParameterId(IFactoryStrategy<IDictionary<string, object>> strategy)
        {

            //IFactoryStrategy strategy = new Default<IDictionary<string, object>, SelectMock>();
            var factoryElement = new OfSingle<SomeMetadata, IDictionary<string, object>>(strategy);
            



            ISelect<IDictionary<string, object>> select = strategy.CreateSelect(1, "id", "SomeMetadata");
            SomeMetadata result = strategy.GetResult(select, (IFactoryElement<SomeMetadata>)factoryElement);


            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("John", result.Name);
            Assert.AreEqual("Red", result.Color);
            Assert.AreEqual("Perfect", result.Skill);

            select = strategy.CreateSelect(2, "id", "SomeMetadata");
            result = strategy.GetResult(select, (IFactoryElement<SomeMetadata>)factoryElement);

            Assert.AreEqual(2, result.Id);
            Assert.AreEqual("Paul", result.Name);
            Assert.AreEqual("Green", result.Color);
            Assert.AreEqual("Poor", result.Skill);

            select = strategy.CreateSelect(3, "id", "SomeMetadata");
            result = strategy.GetResult(select, (IFactoryElement<SomeMetadata>)factoryElement);

            Assert.AreEqual(3, result.Id);
            Assert.AreEqual("Jane", result.Name);
            Assert.AreEqual("Red", result.Color);
            Assert.AreEqual("Very Poor", result.Skill);


        }


        [Test]
        public void GetManyParameterName()
        {

            var strategy = new Default<IDictionary<string, object>, SelectMock>();
            var factoryElement = new OfSingle<SomeMetadata, IDictionary<string, object>>(strategy);
            IFactory factoryMock = MockRepository.GenerateMock<IFactory>();



            ISelect<IDictionary<string, object>> select = strategy.CreateSelect("John", "Name", "SomeMetadata");
            SomeMetadata result = strategy.GetResult(select, (IFactoryElement<SomeMetadata>)factoryElement);


            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("John", result.Name);
            Assert.AreEqual("Red", result.Color);
            Assert.AreEqual("Perfect", result.Skill);

            select = strategy.CreateSelect("Paul", "Name", "SomeMetadata");
            result = strategy.GetResult(select, (IFactoryElement<SomeMetadata>)factoryElement);

            Assert.AreEqual(2, result.Id);
            Assert.AreEqual("Paul", result.Name);
            Assert.AreEqual("Green", result.Color);
            Assert.AreEqual("Poor", result.Skill);

            select = strategy.CreateSelect("Jane", "Name", "SomeMetadata");
            result = strategy.GetResult(select, (IFactoryElement<SomeMetadata>)factoryElement);

            Assert.AreEqual(3, result.Id);
            Assert.AreEqual("Jane", result.Name);
            Assert.AreEqual("Red", result.Color);
            Assert.AreEqual("Very Poor", result.Skill);


        }


        private static IDictionary<string, object> john = new Dictionary<string, object>()
                                                              {
                                                                  {"Name", "John"},
                                                                  {"Color", "Red"},
                                                                  {"Id", 1},
                                                                  {"Skill", "Perfect"}
                                                              };

        private static IDictionary<string, object> paul = new Dictionary<string, object>()
                                                              {
                                                                  {"Name", "Paul"},
                                                                  {"Color", "Green"},
                                                                  {"Id", 2},
                                                                  {"Skill", "Poor"}
                                                              };

        private static IDictionary<string, object> jane = new Dictionary<string, object>()

                                                              {
                                                                  {"Name", "Jane"},
                                                                  {"Color", "Red"},
                                                                  {"Id", 3},
                                                                  {"Skill", "Very Poor"}
                                                              };


        private static IDictionary<string, object> max = new Dictionary<string, object>()

                                                             {
                                                                 {"Name", "Max"},
                                                                 {"Color", "Blue"},
                                                                 {"Id", 4},
                                                                 {"Skill", "Bad"}
                                                             };




        static private List<IDictionary<string, object>> executeResultJohn = new List<IDictionary<string, object>>()
                                    {
                                        john

                                    };

        private static List<IDictionary<string, object>> executeResultPaul = new List<IDictionary<string, object>>()

                                    {
                                        paul
                                    };
        static private List<IDictionary<string, object>> executeResultJane = new List<IDictionary<string, object>>()
                                    {
                                        jane
                                    };
        static private List<IDictionary<string, object>> executeResultMax = new List<IDictionary<string, object>>()
                                    {
                                        max
                                    };



        //public class SelectItemsMock : SelectBase, ISelect<IDictionary<string, object>>
        //{

        //}


        public class SelectMock : SelectBase, ISelect<IDictionary<string, object>>
        {




            public virtual IEnumerable<IDictionary<string, object>> Execute()
            {
                if (ParameterName == "id")
                {
                    if (Parameter.Equals(1))
                    {


                        return executeResultJohn;
                    }
                    if (Parameter.Equals(2))
                    {

                        return executeResultPaul;

                    }

                    if (Parameter.Equals(3))
                    {


                        return executeResultJane;

                    }

                    if (Parameter.Equals(4))
                    {

                        return executeResultMax;
                    }
                }



                if (ParameterName == "Name")
                {
                    if (Parameter.Equals("John"))
                    {


                        return executeResultJohn;
                    }
                    if (Parameter.Equals("Paul"))
                    {

                        return executeResultPaul;

                    }

                    if (Parameter.Equals("Jane"))
                    {


                        return executeResultJane;

                    }

                    if (Parameter.Equals("Max"))
                    {

                        return executeResultMax;
                    }
                }
                throw new Exception("Не попали");
            }



        }






    }
}
