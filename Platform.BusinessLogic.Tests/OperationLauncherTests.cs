using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Operations;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Actions;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;
using Rhino.Mocks;
using SomeBusiness;

namespace Platform.BusinessLogic.Tests
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class OperationLauncherTests
    {
        [Test]
        public void OperationCalls()
        {
            var od = new OperationLauncher();
            var operationImpl = MockRepository.GenerateStub<IOperationImpl>();
            var op = new Operation() {Id = 1, Name = "Clean"};

            var se = new SomeEntity();
            se.OperationImpl = operationImpl;

            var dbContext = new DbContext("c");

            od.ProcessOperation(dbContext, op, se);

            operationImpl.AssertWasCalled(ci => ci.Do());
        }

        [Test]
        public void OperationSpecifycContextCalls()
        {
            var od = new OperationLauncher();
            var operationImpl = MockRepository.GenerateStub<IOperationImpl>();
            var op = new Operation() { Id = 1, Name = "UseSpecificContext" };

            var se = new SomeEntity();
            se.OperationImpl = operationImpl;

            var dbContext = new SomeContext("h");

            od.ProcessOperation(dbContext, op, se);

            operationImpl.AssertWasCalled(ci => ci.Do());
        }


        [Test]
        [ExpectedException(typeof(OperationDefinitionException))]
        public void BadOperationDefinition()
        {
            var od = new OperationLauncher();
            var operationImpl = MockRepository.GenerateStub<IOperationImpl>();
            var op = new Operation() { Id = 1, Name = "BadOperation" };

            var se = new SomeEntity {OperationImpl = operationImpl};

            var dbContext = new DbContext("c");

            od.ProcessOperation(dbContext, op, se);
        }


        public class SomeEntity : BaseEntity, IBaseEntity
        {
            public IOperationImpl OperationImpl { set; get; }


            public void BadOperation(DbContext dbContext, int bad)
            {

                OperationImpl.EntityValue = this;
                OperationImpl.DbContext = dbContext;
                OperationImpl.Do();
            }

            public ClientActionList Clean(DbContext dbContext)
            {

                OperationImpl.EntityValue = this;
                OperationImpl.DbContext = dbContext;
                OperationImpl.Do();
                return new ClientActionList();
            }

            public ClientActionList UseSpecificContext(SomeContext dbContext)
            {

                OperationImpl.EntityValue = this;
                OperationImpl.DbContext = dbContext;
                OperationImpl.Do();
                return new ClientActionList();
            }




        }


    }
    public interface IOperationImpl
    {

        DbContext DbContext { get; set; }
        IBaseEntity EntityValue { get; set; }

        void Do();

    }



}
