using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.PlanActivity
{
    public class DSFinancialProvision
    {
        public int Number { get; set; }
        public int KBKId { get; set; }
        public string FinanceSource { get; set; }
        public string KVSR { get; set; }
        public string RZPR { get; set; }
        public string KCSR { get; set; }
        public string KVR { get; set; }
        public string KOSGU { get; set; }
        public string KFO { get; set; }
        public string CodeSubsidy { get; set; }
        public string DFK { get; set; }
        public string DKR { get; set; }
        public string DEK { get; set; }
        public string BranchCode { get; set; }

        public int? PeriodYear { get; set; }

        public string Period { get; set; }
        public decimal? Plan { get; set; }


        public DSFinancialProvision Clone(int? year, string period, int? value)
        {
            DSFinancialProvision clone = (DSFinancialProvision)this.Clone();
            clone.PeriodYear = year;
            clone.Period = period;
            clone.Plan = value;
            return clone;
        }

        public object Clone()
        {
            object clone = new DSFinancialProvision
                {
                    Number = Number,
                    KBKId = KBKId,
                    FinanceSource = FinanceSource,
                    KVSR = KVSR,
                    RZPR = RZPR,
                    KCSR = KCSR,
                    KVR = KVR,
                    KOSGU = KOSGU,
                    KFO = KFO,
                    CodeSubsidy = CodeSubsidy,
                    DFK = DFK,
                    DKR = DKR,
                    DEK = DEK,
                    BranchCode = BranchCode,

                    PeriodYear = PeriodYear,

                    Period = Period,
                    Plan = Plan,
                };
            return clone;
        }
    }
}
