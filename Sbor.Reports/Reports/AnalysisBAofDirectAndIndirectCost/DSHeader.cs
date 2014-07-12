using System;

namespace Sbor.Reports.AnalysisBAofDirectAndIndirectCost
{
    public class DSHeader
    {
        public int BudgetYearStart { get; set; }
        public int BudgetYearEnd { get; set; }
        public DateTime CurrentDate { get; set; }
        public string CapUnit { get; set; }
        public bool ByActivity { get; set; }
        public bool BySBP { get; set; }
    }
}
