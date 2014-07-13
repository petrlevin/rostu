using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Platform.PrimaryEntities.Factoring.FactoryElements;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.PrimaryEntities.Tests.Mocks;
using Rhino.Mocks;

namespace Platform.PrimaryEntities.Factoring.Tests.FactoryElements
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class OfSingleTests
    {
        [Test]
        public void StrategyShouldCallCreateSelectAndGetData()
        {
            Metadata.GetObjects = () => new PrimaryEntities.Tests.Mocks.Factory();
            var strategyMock = MockRepository.GenerateMock<IFactoryStrategy<IDictionary<string, object>>>();

            IBaseFactoryElement<SomeMetadata> factoryElement =
                new OfSingle<SomeMetadata, IDictionary<string, object>>(strategyMock);



            strategyMock.Expect(s => s.CreateSelect(10, "id", "SomeMetadata")).Return(null);
            strategyMock.Expect(s => s.GetResult<SomeMetadata>(null, factoryElement)).Return(new SomeMetadata());

            factoryElement.CreateById(10);
            strategyMock.AssertWasCalled(s => s.CreateSelect(10, "id", "SomeMetadata"));
            strategyMock.AssertWasCalled(s => s.GetResult<SomeMetadata>(null, factoryElement));


        }

        [Test]
        public void CreateById()
        {
            Metadata.GetObjects = () => MockRepository.GenerateMock<IFactory>();
            var strategyMock = MockRepository.GenerateMock<IFactoryStrategy<IDictionary<string, object>>>();

            IBaseFactoryElement<SomeMetadata> factoryElement =
                new OfSingle<SomeMetadata, IDictionary<string, object>>(strategyMock);


            var returnSome = new SomeMetadata() { Color = "Green" };
            strategyMock.Expect(s => s.CreateSelect(10, "id", "SomeMetadata")).Return(null);
            strategyMock.Expect(s => s.GetResult<SomeMetadata>(null, factoryElement)).Return(returnSome);

            var some =factoryElement.CreateById(10);
            Assert.AreSame(returnSome,some);



        }


        [Test]
        public void CreateByName()
        {
            Metadata.GetObjects = () => MockRepository.GenerateMock<IFactory>();
            var strategyMock = MockRepository.GenerateMock<IFactoryStrategy<IDictionary<string, object>>>();

            IFactoryElement<SomeMetadata> factoryElement =
                new OfSingle<SomeMetadata, IDictionary<string, object>>(strategyMock);


            var returnSome = new SomeMetadata() { Color = "Green" };
            strategyMock.Expect(s => s.CreateSelect("Peter", "Name", "SomeMetadata")).Return(null);
            strategyMock.Expect(s => s.GetResult<SomeMetadata>(null, factoryElement)).Return(returnSome);

            var selectMock = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            strategyMock.Expect(s => s.CreateSelect("John", "Name", "SomeMetadata")).Return(selectMock);
            Metadata.GetObjects = ()=>new PrimaryEntities.Tests.Mocks.Factory();
            strategyMock.Expect(s => s.GetResult<SomeMetadata>(selectMock, factoryElement)).Return(new SomeMetadata());

            var some = factoryElement.CreateByName("Peter");
            Assert.AreSame(returnSome, some);

            var some1 = factoryElement.CreateByName("John");
            Assert.AreNotSame(returnSome, some1);

            strategyMock.VerifyAllExpectations();




        }



        [Test]
        public void CreateByIdUseBaseStrategy()
        {
            var strategyMock = (IFactoryStrategy<IDictionary<string, object>>)MockRepository.GeneratePartialMock<BaseFactoryStrategy<IDictionary<string, object>>, IFactoryStrategy<IDictionary<string, object>>>();
            var selectMock1 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock2 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();


            IBaseFactoryElement<SomeMetadata> factoryElement =
                new OfSingle<SomeMetadata, IDictionary<string, object>>(strategyMock);

            IFactory factoryMock = MockRepository.GenerateMock<IFactory>();
            

            strategyMock.Stub(s => s.GetFactory()).Return(factoryMock);
            strategyMock.Stub(s => s.CreateSelect(10, "id", "SomeMetadata")).Return(selectMock1);
            strategyMock.Stub(s => s.CreateSelect(6, "id", "SomeMetadata")).Return(selectMock2);
            

            selectMock1.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "John" }, 
                                                                          { "Color", "Red" }, 
                                                                          { "Id", 10},
                                                                          { "Skill", "Perfect" }
                                                                      } 
                                                                });


            selectMock2.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Paul" }, 
                                                                          { "Color", "Green" }, 
                                                                          { "Id", 6},
                                                                          { "Skill", "Poor" }
                                                                      } 
                                                                });



            var some = factoryElement.CreateById(10);

            strategyMock.AssertWasCalled(s => s.CreateSelect(10, "id", "SomeMetadata"));
            strategyMock.AssertWasCalled(s => s.GetResult<SomeMetadata>(selectMock1, factoryElement));
            strategyMock.AssertWasCalled(s => s.GetFactory());
            selectMock1.AssertWasCalled(s => s.Execute());
            

            Assert.AreEqual("John", some.Name);
            Assert.AreEqual("Red", some.Color);
            Assert.AreEqual(10, some.Id);
            Assert.AreEqual("Perfect", some.Skill);

            some = factoryElement.CreateById(10);
            Assert.AreEqual("John", some.Name);
            Assert.AreEqual("Red", some.Color);
            Assert.AreEqual(10, some.Id);
            Assert.AreEqual("Perfect", some.Skill);

            some = factoryElement.CreateById(6);
            Assert.AreEqual("Paul", some.Name);
            Assert.AreEqual("Green", some.Color);
            Assert.AreEqual(6, some.Id);
            Assert.AreEqual("Poor", some.Skill);

        }


        [Test]
        public void CreateByNameUseBaseStrategy()
        {
            var strategyMock = (IFactoryStrategy<IDictionary<string, object>>)MockRepository.GeneratePartialMock<BaseFactoryStrategy<IDictionary<string, object>>, IFactoryStrategy<IDictionary<string, object>>>();
            var selectMock1 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock2 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();


            IFactoryElement<SomeMetadata> factoryElement =
                new OfSingle<SomeMetadata, IDictionary<string, object>>(strategyMock);

            IFactory factoryMock = MockRepository.GenerateMock<IFactory>();
            

            strategyMock.Stub(s => s.GetFactory()).Return(factoryMock);
            strategyMock.Stub(s => s.CreateSelect("John", "Name", "SomeMetadata")).Return(selectMock1);
            strategyMock.Stub(s => s.CreateSelect("Paul", "Name", "SomeMetadata")).Return(selectMock2);
            

            selectMock1.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "John" }, 
                                                                          { "Color", "Red" }, 
                                                                          { "Id", 10},
                                                                          { "Skill", "Perfect" }
                                                                      } 
                                                                });


            selectMock2.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Paul" }, 
                                                                          { "Color", "Green" }, 
                                                                          { "Id", 6},
                                                                          { "Skill", "Poor" }
                                                                      } 
                                                                });



            var some = factoryElement.CreateByName("John");

            strategyMock.AssertWasCalled(s => s.CreateSelect("John", "Name", "SomeMetadata"));
            strategyMock.AssertWasCalled(s => s.GetResult<SomeMetadata>(selectMock1, factoryElement));
            strategyMock.AssertWasCalled(s => s.GetFactory());
            selectMock1.AssertWasCalled(s => s.Execute());

            Assert.AreEqual("John", some.Name);
            Assert.AreEqual("Red", some.Color);
            Assert.AreEqual(10, some.Id);
            Assert.AreEqual("Perfect", some.Skill);

            some = factoryElement.CreateByName("John");
            Assert.AreEqual("John", some.Name);
            Assert.AreEqual("Red", some.Color);
            Assert.AreEqual(10, some.Id);
            Assert.AreEqual("Perfect", some.Skill);

            some = factoryElement.CreateByName("Paul");
            Assert.AreEqual("Paul", some.Name);
            Assert.AreEqual("Green", some.Color);
            Assert.AreEqual(6, some.Id);
            Assert.AreEqual("Poor", some.Skill);




        }


        [Test]
        public void CreateByNameAndIdUseBaseStrategyManyMetadatas()
        {
            var strategyMock = (IFactoryStrategy<IDictionary<string, object>>)MockRepository.GeneratePartialMock<BaseFactoryStrategy<IDictionary<string, object>>, IFactoryStrategy<IDictionary<string, object>>>();
            var selectMock1 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock2 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock3 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock4 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock5 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock6 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();
            var selectMock7 = MockRepository.GenerateMock<ISelect<IDictionary<string, object>>>();



            IFactoryElement<SomeMetadata> factoryofSome =
                new OfSingle<SomeMetadata, IDictionary<string, object>>(strategyMock);

            IFactoryElement<OtherMetadata> factoryofOther =
                new OfSingle<OtherMetadata, IDictionary<string, object>>(strategyMock);


            IFactory factoryMock = MockRepository.GenerateMock<IFactory>();
            

            strategyMock.Stub(s => s.GetFactory()).Return(factoryMock);

            strategyMock.Stub(s => s.CreateSelect("John", "Name", "SomeMetadata")).Return(selectMock1);
            strategyMock.Stub(s => s.CreateSelect("Paul", "Name", "SomeMetadata")).Return(selectMock2);
            strategyMock.Stub(s => s.CreateSelect(10, "id", "SomeMetadata")).Return(selectMock3);
            strategyMock.Stub(s => s.CreateSelect(5, "id", "SomeMetadata")).Return(selectMock4);
            strategyMock.Stub(s => s.CreateSelect(1, "id", "OtherMetadata")).Return(selectMock5);
            strategyMock.Stub(s => s.CreateSelect(50000, "id", "OtherMetadata")).Return(selectMock6);
            strategyMock.Stub(s => s.CreateSelect("Jane", "Name", "OtherMetadata")).Return(selectMock7);

            

            selectMock1.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "John" }, 
                                                                          { "Color", "Red" }, 
                                                                          { "Id", 10},
                                                                          { "Skill", "Perfect" }
                                                                      } 
                                                                });


            selectMock2.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Paul" }, 
                                                                          { "Color", "Green" }, 
                                                                          { "Id", 6},
                                                                          { "Skill", "Poor" }
                                                                      } 
                                                                });

            selectMock3.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "John" }, 
                                                                          { "Color", "Red" }, 
                                                                          { "Id", 10},
                                                                          { "Skill", "Perfect" }
                                                                      } 
                                                                });

            selectMock4.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Max" }, 
                                                                          { "Color", "Blue" }, 
                                                                          { "Id", 5},
                                                                          { "Skill", "Exelent" }
                                                                      } 
                                                                });

            selectMock5.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Max" }, 
                                                                          { "Voice", "Loud" }, 
                                                                          { "Id", 1},
                                                                          { "IsHere", true}
                                                                      } 
                                                                });

            selectMock6.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Anna" }, 
                                                                          { "Voice", "Tender" }, 
                                                                          { "Id", 50000},
                                                                          { "IsHere", false}
                                                                      } 
                                                                });

            selectMock7.Stub(s => s.Execute()).Return(new List<IDictionary<string, object>>() 
                                                                { new Dictionary<string, object>()
                                                                      {
                                                                          { "Name", "Jane" }, 
                                                                          { "Voice", "Rude" }, 
                                                                          { "Id", 6},
                                                                          { "IsHere", true}
                                                                      } 
                                                                });


            var some = factoryofSome.CreateByName("John");
            Assert.AreEqual("John", some.Name);
            Assert.AreEqual("Red", some.Color);
            Assert.AreEqual(10, some.Id);
            Assert.AreEqual("Perfect", some.Skill);

            var other = factoryofOther.CreateById(50000);
            Assert.AreEqual("Anna", other.Name);
            Assert.AreEqual("Tender", other.Voice);
            Assert.AreEqual(50000, other.Id);
            Assert.AreEqual(false, other.IsHere);

            some = factoryofSome.CreateByName("Paul");
            Assert.AreEqual("Paul", some.Name);
            Assert.AreEqual("Green", some.Color);
            Assert.AreEqual(6, some.Id);
            Assert.AreEqual("Poor", some.Skill);

            some = factoryofSome.CreateById(10);
            Assert.AreEqual("John", some.Name);
            Assert.AreEqual("Red", some.Color);
            Assert.AreEqual(10, some.Id);
            Assert.AreEqual("Perfect", some.Skill);


            other = factoryofOther.CreateById(1);
            Assert.AreEqual("Max", other.Name);
            Assert.AreEqual("Loud", other.Voice);
            Assert.AreEqual(1, other.Id);
            Assert.AreEqual(true, other.IsHere);

            some = factoryofSome.CreateById(5);
            Assert.AreEqual("Max", some.Name);
            Assert.AreEqual("Blue", some.Color);
            Assert.AreEqual(5, some.Id);
            Assert.AreEqual("Exelent", some.Skill);

            other = factoryofOther.CreateByName("Jane");
            Assert.AreEqual("Jane", other.Name);
            Assert.AreEqual("Rude", other.Voice);
            Assert.AreEqual(6, other.Id);
            Assert.AreEqual(true, other.IsHere);

        }



    }
}
