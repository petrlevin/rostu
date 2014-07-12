using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.PlanActivity
{
    public class PlanActivityInfo
    {
        public int Id { get; set; }
        public string SBP { get; set; }
        public int Year { get; set; }
    }

    public class BudgetLevel
    {
        public string BB { get; set; }
        public string ZB { get; set; }
        public string MB { get; set; }
    }

    public class PlanActivity_Activity
    {
        public Int64 RowNumber { get; set; }
        public int id { get; set; }
        public int idOwner { get; set; }
        public int idContingent { get; set; }
        public int idIndicatorActivity { get; set; }
        public int idActivity { get; set; }

    }
}
