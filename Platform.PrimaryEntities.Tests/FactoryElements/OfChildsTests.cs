using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.PrimaryEntities.Factoring.FactoryElements;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Tests.Mocks;
using Rhino.Mocks;

namespace Platform.PrimaryEntities.Factoring.Tests.FactoryElements
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class OfChildsTests
    {
        [Test]
        public void StrategyShouldCallCreateSelectAndGetData()
        {
            Metadata.GetObjects = ()=> new PrimaryEntities.Tests.Mocks.Factory();
            var strategyMock = MockRepository.GenerateMock<IFactoryStrategy<IDictionary<string, object>>>();

            IBaseFactoryElement<IList<SomeMetadata>> factoryElement =
                new OfChilds<SomeMetadata,OtherMetadata, IDictionary<string, object>>(strategyMock);



            strategyMock.Expect(s => s.CreateSelect(10, "idOtherMetadata", "SomeMetadata")).Return(null);
            strategyMock.Expect(s => s.GetResult(null, factoryElement)).Return(new List<SomeMetadata>(){new SomeMetadata()});

            factoryElement.CreateById(10);
            strategyMock.AssertWasCalled(s => s.CreateSelect(10, "idOtherMetadata", "SomeMetadata"));
            strategyMock.AssertWasCalled(s => s.GetResult(null, factoryElement));


        }


        [Test]
        public void CreateById()
        {
            Metadata.GetObjects = () => MockRepository.GenerateMock<IFactory>();
            var strategyMock = MockRepository.GenerateMock<IFactoryStrategy<IDictionary<string, object>>>();

            IBaseFactoryElement<IList<SomeMetadata>> factoryElement =
                new OfChilds<SomeMetadata, OtherMetadata, IDictionary<string, object>>(strategyMock);


            var returnSome0 = new SomeMetadata() { Color = "Green" };
            var returnSome1 = new SomeMetadata() { Color = "Red" };
            strategyMock.Expect(s => s.CreateSelect(10, "idOtherMetadata", "SomeMetadata")).Return(null);
            strategyMock.Expect(s => s.GetResult(null, factoryElement)).Return(new List<SomeMetadata>() { returnSome0, returnSome1 });

            var somes = factoryElement.CreateById(10);
            Assert.AreSame(returnSome0, somes[0]);
            Assert.AreSame(returnSome1, somes[1]);



        }


        [Test]
        public void CreateByIdUseBaseStrategy()
        {
            var strategyMock = (IFactoryStrategy<IDictionary<string, object>>)MockRepository.GeneratePartialMock<BaseFactoryStrategy<IDictionary<string, object>>, IFactoryStrategy<IDictionary<string, object>>>();
            var selectMock1 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock2 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();


            IBaseFactoryElement<IList<SomeMetadata>> factoryElement =
                new OfChilds<SomeMetadata, OtherMetadata, IDictionary<string, object>>(strategyMock);

            IFactory factoryMock = MockRepository.GenerateMock<IFactory>();


            strategyMock.Stub(s => s.GetFactory()).Return(factoryMock);
            strategyMock.Stub(s => s.CreateSelect(1, "idOtherMetadata", "SomeMetadata")).Return(selectMock1);
            strategyMock.Stub(s => s.CreateSelect(2, "idOtherMetadata", "SomeMetadata")).Return(selectMock2);


            selectMock1.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { 
                                                                    new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "John" }, 
                                                                          { "Color", "Red" }, 
                                                                          { "Id", 10},
                                                                          { "Skill", "Perfect" }
                                                                      },
                                                                    new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Max" }, 
                                                                          { "Color", "Red" }, 
                                                                          { "Id", 15},
                                                                          { "Skill", "Perfect" }
                                                                      }
 
                                                                });


            selectMock2.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { 
                                                                    new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Paul" }, 
                                                                          { "Color", "Green" }, 
                                                                          { "Id", 16},
                                                                          { "Skill", "Poor" }
                                                                      }, 
                                                                    new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Jane" }, 
                                                                          { "Color", "Green" }, 
                                                                          { "Id", 6},
                                                                          { "Skill", "Exellent" }
                                                                      }, 
                                                                    new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Jacob" }, 
                                                                          { "Color", "Green" }, 
                                                                          { "Id", 800},
                                                                          { "Skill", "Exellent" }
                                                                      }, 

                                                                });



            var somes1 = factoryElement.CreateById(1);

            strategyMock.AssertWasCalled(s => s.CreateSelect(1, "idOtherMetadata", "SomeMetadata"));
            strategyMock.AssertWasCalled(s => s.GetResult<IList<SomeMetadata>>(selectMock1, factoryElement));
            strategyMock.AssertWasCalled(s => s.GetFactory());
            selectMock1.AssertWasCalled(s => s.Execute());

            Assert.AreEqual(2, somes1.Count);
            Assert.AreEqual("John", somes1[0].Name);
            Assert.AreEqual("Red", somes1[0].Color);
            Assert.AreEqual(10, somes1[0].Id);
            Assert.AreEqual("Perfect", somes1[0].Skill);

            Assert.AreEqual("Max", somes1[1].Name);
            Assert.AreEqual("Red", somes1[1].Color);
            Assert.AreEqual(15, somes1[1].Id);
            Assert.AreEqual("Perfect", somes1[1].Skill);

            var somes2 = factoryElement.CreateById(2);
            Assert.AreEqual(3, somes2.Count);
            Assert.AreEqual("Paul", somes2[0].Name);
            Assert.AreEqual("Green", somes2[0].Color);
            Assert.AreEqual(16, somes2[0].Id);
            Assert.AreEqual("Poor", somes2[0].Skill);

            Assert.AreEqual("Jane", somes2[1].Name);
            Assert.AreEqual("Green", somes2[1].Color);
            Assert.AreEqual(6, somes2[1].Id);
            Assert.AreEqual("Exellent", somes2[1].Skill);


        }




    }
}
