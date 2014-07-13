
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Interfaces;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;
using Rhino.Mocks;
using SomeBusiness;

namespace Platform.BusinessLogic.Tests
{

    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class ControlLauncherTests
    {


        [Test]
        public void ControlCalls()
        {
            IControlLauncher cl = new ControlLauncher(new DbContext("c"), new ControlDispatcherBase());
            IControlImpl controlImpl = MockRepository.GenerateStub<IControlImpl>();

            SomeEntity se = new SomeEntity();
            se.ControlImpl = controlImpl;

            

            cl.ProcessControls(ControlType.Update, Sequence.After, se, se);
            controlImpl.AssertWasCalled(ci => ci.Do());
            cl.ProcessControls(ControlType.Update, Sequence.After, se, se);
            controlImpl.AssertWasCalled(ci => ci.Do());
            Assert.AreEqual(se, controlImpl.EntityValue);
            Assert.AreEqual(ControlType.Update, controlImpl.ControlType);
            Assert.AreEqual(Sequence.After, controlImpl.Sequence);
        }

        [Test]
        public void SpecificDBContextControl()
        {
            IControlLauncher cl = new ControlLauncher(new SomeContext("c"),new ControlDispatcherBase());
            IControlImpl firstImpl = MockRepository.GenerateStub<IControlImpl>();
            

            var se = new SpecifiedContextEntity();
            se.FirstImpl= firstImpl;
            

            

            cl.ProcessControls(ControlType.Update, Sequence.After, se, se);
            firstImpl.AssertWasCalled(ci => ci.Do());
        }




        [Test]
        public void BothControlCalls()
        {
            IControlLauncher cl = new ControlLauncher(new DbContext("c"), new ControlDispatcherBase());
            IControlImpl firstImpl = MockRepository.GenerateStub<IControlImpl>();
            IControlImpl secondImpl = MockRepository.GenerateStub<IControlImpl>();

            OtherEntity e = new OtherEntity();
            e.FirstImpl = firstImpl;
            e.SecondImpl = secondImpl;

            

            cl.ProcessControls( ControlType.Update, Sequence.After, e, e);
            cl.ProcessControls( ControlType.Update, Sequence.After, e, e);
            firstImpl.AssertWasCalled(ci => ci.Do(),o=>o.Repeat.Twice());
            secondImpl.AssertWasCalled(ci => ci.Do(),o=>o.Repeat.Twice());

            Assert.AreEqual(e, firstImpl.EntityValue);
            Assert.AreEqual(e, secondImpl.EntityValue);
        }


        [Test]
        public void ControlWithLessParametersDefinedCalls()
        {
            IControlLauncher cl = new ControlLauncher(new DbContext("c"), new ControlDispatcherBase());

            IControlImpl fourthImpl = MockRepository.GenerateStub<IControlImpl>();

            OtherEntity e = new OtherEntity();
            e.FourthImpl = fourthImpl;


            

            cl.ProcessControls(ControlType.Update, Sequence.Before, e, e);

            fourthImpl.AssertWasCalled(ci => ci.Do());


            Assert.AreEqual(e, fourthImpl.EntityValue);

        }

        [Test]
        public void ControlWithoutDefinedParametersCalls()
        {
            IControlLauncher cl = new ControlLauncher(new DbContext("c"), new ControlDispatcherBase());

            IControlImpl fifthImpl = MockRepository.GenerateStub<IControlImpl>();

            OtherEntity e = new OtherEntity();
            e.FifthImpl = fifthImpl;


            

            cl.ProcessControls( ControlType.Update, Sequence.Before, e, e);

            fifthImpl.AssertWasCalled(ci => ci.Do());


            Assert.AreEqual(e, fifthImpl.EntityValue);

        }


        [Test]
        public void CorrectExecutionOrder()
        {
            IControlLauncher cl = new ControlLauncher(new DbContext("c"), new ControlDispatcherBase());

            var e = new EntityWithOrederedControls();

            

            cl.ProcessControls(ControlType.Update, Sequence.After, e, e);

            


            Assert.AreEqual(e.ControlNames[0], "FirstControl");
            Assert.AreEqual(e.ControlNames[1], "SecondControl");
            Assert.AreEqual(e.ControlNames[2], "ThirdControl");
            Assert.AreEqual(e.ControlNames[3], "FourthControl");
            Assert.AreEqual(e.ControlNames[4], "FifthControl");
            Assert.AreEqual(e.ControlNames[5], "SixControl");

        }




        [Test]
        public void ControlWithManyControlTypesParametersCalls()
        {
            IControlLauncher cl = new ControlLauncher(new DbContext("c"), new ControlDispatcherBase());

            IControlImpl sixImpl = MockRepository.GenerateStub<IControlImpl>();

            OtherEntity e = new OtherEntity();
            e.SixImpl = sixImpl;


            

            cl.ProcessControls( ControlType.Update, Sequence.After, e, e);
            Assert.AreEqual(e, sixImpl.EntityValue);
            Assert.AreEqual(ControlType.Update, sixImpl.ControlType);




            cl.ProcessControls( ControlType.Insert, Sequence.After, e, e);
            Assert.AreEqual(e, sixImpl.EntityValue);
            Assert.AreEqual(ControlType.Insert, sixImpl.ControlType);


            cl.ProcessControls( ControlType.Delete, Sequence.After, e, e);

            sixImpl.AssertWasCalled(ci => ci.Do(), option => option.Repeat.Twice());




        }






        [Test]
        public void CallsOnlyPaticularTypeAndSequence()
        {
            IControlLauncher cl = new ControlLauncher(new DbContext("c"), new ControlDispatcherBase());
            IControlImpl firstImpl = MockRepository.GenerateStub<IControlImpl>();
            IControlImpl secondImpl = MockRepository.GenerateStub<IControlImpl>();
            IControlImpl thirdImpl = MockRepository.GenerateStub<IControlImpl>();
            IControlImpl fourthImpl = MockRepository.GenerateStub<IControlImpl>();

            OtherEntity e = new OtherEntity();
            e.FirstImpl = firstImpl;
            e.SecondImpl = secondImpl;
            e.ThirdImpl = secondImpl;
            e.FourthImpl = fourthImpl;

            

            cl.ProcessControls(ControlType.Update, Sequence.After, e, e);
            firstImpl.AssertWasCalled(ci => ci.Do());
            secondImpl.AssertWasCalled(ci => ci.Do());
            thirdImpl.AssertWasNotCalled(ci => ci.Do());
            fourthImpl.AssertWasNotCalled(ci => ci.Do());

            cl.ProcessControls(ControlType.Update, Sequence.After, e, e);
            firstImpl.AssertWasCalled(ci => ci.Do());
            secondImpl.AssertWasCalled(ci => ci.Do());

            thirdImpl.AssertWasNotCalled(ci => ci.Do());
            fourthImpl.AssertWasNotCalled(ci => ci.Do());

        }

        [Test]
        [ExpectedException(typeof(ControlDefinitionException))]
        public void BadDefinedControl()
        {
            IControlLauncher cl = new ControlLauncher(new DbContext("c"), new ControlDispatcherBase());

            var e = new BadDefinedControlEntity();
            

            cl.ProcessControls(ControlType.Update, Sequence.After, e, e);

        }

        [Test]
        [ExpectedException(typeof(ControlExecutionException))]
        public void BadExecutedControl()
        {
            IControlLauncher cl = new ControlLauncher((c, be) => { },new DbContext("c"), new ControlDispatcherBase());

            var e = new BadExecutionControlEntity();
            

            cl.ProcessControls(ControlType.Update, Sequence.After, e, e);

        }


        [Test]
        [ExpectedException(typeof(ControlResponseException))]
        public void ControlFailed()
        {
            IControlLauncher cl = new ControlLauncher((dc, be) => { }, new DbContext("c"), new ControlDispatcherBase());

            var e = new ControlFailedEntity();
            

            try
            {
                cl.ProcessControls(ControlType.Update, Sequence.After, e, e);
            }
            catch (ControlResponseException ex)
            {
                Assert.AreEqual(typeof(ControlFailedEntity).GetMethod("Control"), ex.Action);
                Assert.AreEqual(typeof(ControlFailedEntity), ex.DeclaringType);
                Assert.AreEqual(e, ex.Target);
                throw;
            }


        }


        public class BadDefinedControlEntity : BaseEntity, IBaseEntity
        {


            [ControlAttribute(ControlType.Update, Sequence.After)]
            public void Control(int controlType)
            {
            }


        }

        public class BadExecutionControlEntity : BaseEntity, IBaseEntity
        {


            [ControlAttribute(ControlType.Update, Sequence.After)]
            public void Control(ControlType controlType)
            {
                throw new Exception("тратата");
            }


        }

        public class ControlFailedEntity : BaseEntity, IBaseEntity
        {


            [ControlAttribute(ControlType.Update, Sequence.After)]
            public void Control(ControlType controlType)
            {
                Controls.Throw("тратата");
            }


        }


        public class SpecifiedContextEntity : BaseEntity, IBaseEntity
        {
            public IControlImpl FirstImpl { set; get; }

            [ControlAttribute(ControlType.Update, Sequence.After)]
            public void FirstControl(SomeContext context, ControlType controlType, Sequence sequence,
                                    SpecifiedContextEntity newEntityValue)
            {
                if (FirstImpl != null)
                {
                    FirstImpl.DbContext = context;
                    FirstImpl.ControlType = controlType;
                    FirstImpl.Sequence = sequence;
                    FirstImpl.EntityValue = this;
                    FirstImpl.NewEntityValue = newEntityValue;
                    FirstImpl.Do();

                }
            }


        }



        public class OtherEntity : BaseEntity, IBaseEntity
        {
            public IControlImpl FirstImpl { set; get; }
            public IControlImpl SecondImpl { set; get; }
            public IControlImpl ThirdImpl { set; get; }
            public IControlImpl FourthImpl { set; get; }
            public IControlImpl FifthImpl { set; get; }
            public IControlImpl SixImpl { set; get; }

            [ControlAttribute(ControlType.Update, Sequence.After)]
            public void FirstControl(DbContext context, ControlType controlType, Sequence sequence,
                                    OtherEntity newEntityValue)
            {
                if (FirstImpl != null)
                {
                    FirstImpl.ControlType = controlType;
                    FirstImpl.Sequence = sequence;
                    FirstImpl.EntityValue = this;
                    FirstImpl.NewEntityValue = newEntityValue;
                    FirstImpl.Do();
                }
            }

            [ControlAttribute(ControlType.Update, Sequence.After)]
            public void SecondControl(ControlType controlType, Sequence sequence,
                                    OtherEntity newEntityValue)
            {
                if (SecondImpl != null)
                {
                    SecondImpl.ControlType = controlType;
                    SecondImpl.Sequence = sequence;
                    SecondImpl.EntityValue = this;
                    SecondImpl.NewEntityValue = newEntityValue;
                    SecondImpl.Do();
                }
            }

            [ControlAttribute(ControlType.Insert, Sequence.After)]
            public void ThirdControl(ControlType controlType, Sequence sequence,
                                    OtherEntity newEntityValue)
            {
                if (ThirdImpl != null)
                {
                    ThirdImpl.ControlType = controlType;
                    ThirdImpl.Sequence = sequence;
                    ThirdImpl.EntityValue = this;
                    ThirdImpl.NewEntityValue = newEntityValue;
                    ThirdImpl.Do();
                }
            }

            [ControlAttribute(ControlType.Update, Sequence.Before)]
            public void FourthControl(ControlType controlType)
            {
                if (FourthImpl != null)
                {
                    FourthImpl.ControlType = controlType;

                    FourthImpl.EntityValue = this;

                    FourthImpl.Do();
                }
            }

            [ControlAttribute(ControlType.Update, Sequence.Before)]
            public void FifthControl()
            {
                if (FifthImpl != null)
                {
                    FifthImpl.EntityValue = this;

                    FifthImpl.Do();
                }
            }


            [ControlAttribute(ControlType.Update | ControlType.Insert, Sequence.After)]
            public void SixControl(ControlType controlType, Sequence sequence,
                                    OtherEntity newEntityValue)
            {
                if (SixImpl != null)
                {
                    SixImpl.ControlType = controlType;
                    SixImpl.Sequence = sequence;
                    SixImpl.EntityValue = this;
                    SixImpl.NewEntityValue = newEntityValue;
                    SixImpl.Do();
                }
            }



        }


        public class EntityWithOrederedControls : BaseEntity, IBaseEntity
        {
            public List<string> ControlNames = new List<string>();


            [ControlAttribute(ControlType.Update, Sequence.After, ExecutionOrder = 4)]
            public void FourthControl(ControlType controlType)
            {
                ControlNames.Add("FourthControl");
            }


            [ControlAttribute(ControlType.Update, Sequence.After, ExecutionOrder = 2)]
            public void SecondControl(ControlType controlType, Sequence sequence,
                                    EntityWithOrederedControls newEntityValue)
            {
                ControlNames.Add("SecondControl");
            }


            [ControlAttribute(ControlType.Update, Sequence.After, ExecutionOrder = 1)]
            public void FirstControl(DbContext context, ControlType controlType, Sequence sequence,
                                    EntityWithOrederedControls newEntityValue)
            {
                ControlNames.Add("FirstControl");
            }


            [ControlAttribute(ControlType.Any, Sequence.Any, ExecutionOrder = 3)]
            public void ThirdControl(ControlType controlType, Sequence sequence,
                                    EntityWithOrederedControls newEntityValue)
            {
                ControlNames.Add("ThirdControl");
            }

            [ControlAttribute(ControlType.Update | ControlType.Insert, Sequence.Any, ExecutionOrder = 6)]
            public void SixControl(ControlType controlType, Sequence sequence,
                                    EntityWithOrederedControls newEntityValue)
            {
                ControlNames.Add("SixControl");
            }


            [ControlAttribute(ControlType.Any, Sequence.Any, ExecutionOrder = 5)]
            public void FifthControl()
            {
                ControlNames.Add("FifthControl");
            }




        }



        public class SomeEntity : BaseEntity, IBaseEntity
        {
            public IControlImpl ControlImpl { set; get; }

            [ControlAttribute(ControlType.Update, Sequence.After)]
            public void SomeControl(ControlType controlType, Sequence sequence,
                                    SomeEntity newEntityValue)
            {

                ControlImpl.ControlType = controlType;
                ControlImpl.Sequence = sequence;
                ControlImpl.EntityValue = this;
                ControlImpl.NewEntityValue = newEntityValue;
                ControlImpl.Do();
            }


        }
    }

    public interface IControlImpl
    {
        DbContext DbContext { get; set; }
        ControlType ControlType { get; set; }
        Sequence Sequence { get; set; }
        IBaseEntity EntityValue { get; set; }
        IBaseEntity NewEntityValue { get; set; }
        void Do();

    }
}

