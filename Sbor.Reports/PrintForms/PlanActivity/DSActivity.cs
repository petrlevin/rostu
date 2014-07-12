using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.PlanActivity
{
    public class DSActivity:ICloneable
    {
        public int Number { get; set; }

        public byte ActivityTypeId { get; set; }
        public string ActivityType { get; set; }
        public int ActivityId { get; set;}
        public string ActivityName { get; set; }
        public string Contingent { get; set; }
        public string IndicatorType { get; set; }
        public string IndicatorName { get; set; }
        public string UnitDimension { get; set; }

        public int?   PeriodYear { get; set; }
        public string   Period { get; set; }
        public decimal? Plan { get; set; }
        public decimal? AdditionalNeed { get; set; }

        public DSActivity Clone(int? year, string period, int? value)
        {
            DSActivity clone = (DSActivity)this.Clone();
            clone.PeriodYear = year;
            clone.Period = period;
            clone.Plan = value;
            return clone;
        }

        public object Clone()
        {
            object clone = new DSActivity
                {
                    Number = Number,

                    ActivityTypeId = ActivityTypeId,
                    ActivityType = ActivityType,
                    ActivityId = ActivityId,
                    ActivityName = ActivityName,
                    Contingent = Contingent,
                    IndicatorType = IndicatorType,
                    IndicatorName = IndicatorName,
                    UnitDimension = UnitDimension,

                    PeriodYear = PeriodYear,
                    Period = Period,
                    Plan = Plan,
                    AdditionalNeed = AdditionalNeed,
                };
            return clone;
        }
    }
}
