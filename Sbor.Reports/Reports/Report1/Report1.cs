using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.ReportingServices;
using Platform.BusinessLogic.ReportingServices.Reports;
using Sbor.Reports.Reports;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class Report1
    {
        public List<DataSet1> DataSet1()
        {
            List<DataSet1> res = this.FinanceSource.Select(a => new DataSet1() { Name = a.Caption, Age = 4 }).ToList();
            return res;
        }
    }
}
