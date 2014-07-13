using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Activity.Operations;
using BaseApp.Common.Interfaces;
using NUnit.Framework;
using Platform.BusinessLogic.Activity;
using Platforms.Tests.Common;
using Rhino.Mocks;

namespace BaseApp.Tests
{
    [TestFixture]
    public class StartedOperationsTests :SqlTests
    {
        [Test]
        public void Test()
        {
            using (var c = new DataContext())
            {
                var so = c.StartedOperation.Create();
                
                
            }
        }
        
        [Test]
        public void BeginOperation()
        {
            using (var c = new DataContext())
            {

                //var eo = c.EntityOperation.FirstOrDefault(eo => eo.Operation.Name.ToLower() == "edit");
                var locks = new Locks(connection);
                var user = MockRepository.GenerateStub<IUser>();
                var od = new OperationDispatcher(c,locks,user);
                //od.BeginOperation();
            }
        }
    }
}
