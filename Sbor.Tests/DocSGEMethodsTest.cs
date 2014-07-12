using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.BusinessLogic.Common.Exceptions;
using Sbor.Document;
using Sbor.Logic;
using Sbor.Reference;
using BaseApp.Reference;

namespace Sbor.Tests
{
    [TestFixture]
    class DocSGEMethodsTest
    {
        public class TestDoc : IDocSGE
        {
            public int IdPublicLegalFormation { get; set; }
            public PublicLegalFormation PublicLegalFormation { get; set; }
            public int IdVersion { get; set; }
            public BaseApp.Reference.Version Version { get; set; }
            public int IdDocType { get; set; }
            public DocType DocType { get; set; }
            public string Number { get; set; }
            public DateTime Date { get; set; }
            public DateTime DateStart { get; set; }
            public DateTime DateEnd { get; set; }
            public DateTime? DateCommit { get; set; }
            public string Description { get; set; }
            public int IdDocStatus { get; set; }
            public int Id { get; set; }
            public int? IdParent { get; private set; }
            public DateTime ParentDate { get; private set; }
            public DateTime ParentDateStart { get; private set; }
            public DateTime ParentDateEnd { get; private set; }
            public DateTime? ParentDateCommit { get; private set; }
            public string Caption { get; set; }
            public string Header { get; set; }
            public int[] AllVersionDocIds
            {
                get { return new int[]{}; }
            }



            public bool HasAdditionalNeed
            {
                get
                {
                    return false;
                }
                set
                {
                    return;
                }
            }
        }

        #region CommonControl_0101

        [Test]
        public void TestCommonControl0101_1()
        {
            IDocSGE doc = new TestDoc();
            doc.DateStart = DateTime.Now;
            doc.DateEnd = DateTime.Now.AddDays(1);

            Assert.DoesNotThrow( () => DocSGEMethod.CommonControl_0101(doc) );

            doc.DateStart = DateTime.Now;
            doc.DateEnd = DateTime.Now.AddDays(-1);

            Assert.Throws<ControlResponseException>(() => DocSGEMethod.CommonControl_0101(doc));

            doc.DateStart = DateTime.Now;
            doc.DateEnd = DateTime.Now;

            Assert.Throws<ControlResponseException>(() => DocSGEMethod.CommonControl_0101(doc));
        }

        #endregion
    }
}
