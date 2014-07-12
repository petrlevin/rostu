using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Auditing.EfModel;
using Sbor.Reports.TestAuditReport;
using Platform.BusinessLogic.ReportingServices.Reports;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class TestAuditReport
    {
        public List<LastUsers> LastUsers()
        {
            List<LastUsers> result = new List<LastUsers>();
            using (var db = new AuditDataContext())
            {
                result = db.Data.Select(a => new LastUsers() { TheGist = a.After }).ToList();
            }
            return result;
        }
    }
}
