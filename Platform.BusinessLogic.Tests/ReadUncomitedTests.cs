using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;
using Platform.BusinessLogic.Reference;
using SomeBusiness;
using SomeBusiness.Reference;


namespace Platform.BusinessLogic.Tests
{
	[ExcludeFromCodeCoverage]
	public class ReadUncomitedTests
    {
        [Test]
        public void Tests()
        {
            //using (var c = new DataContext())
            //{
            //    var count =c.Operation.Count();
            //    using (var t = new TransactionScope(       TransactionScopeOption.Required,                                                   new TransactionOptions
            //                                                  {
            //                                                      IsolationLevel =
            //                                                          IsolationLevel.ReadCommitted
            //                                                  }))
            //    {
            //        c.Operation.Add(new Operation(){Name="ghhghghs" ,Caption = "hghhghghghg"});
            //        c.SaveChanges();
            //        var i = c.Operation.Where(o => o.Name == "ghhghghs");
            //    }
                    
            //}
        }
    }
}
