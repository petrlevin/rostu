using System;

namespace Sbor.Reports.Reports.DirectionAndFundingOfDepartment
{
    public class DSMain
    {

        public int Id { get; set; }
        public string Caption { get; set; }

        public string RN1 { get; set; }
        public int IdSystemGoalElement { get; set; }
        public string CapSystemGoalElement { get; set; }

        public string RN2 { get; set; }
        public int IdTaskCollection { get; set; }
        public string CapActivity { get; set; }

        public string FinanceSource { get; set; }
        public string KVSR { get; set; }
        public string RZPR { get; set; }
        public string KCSR { get; set; }
        public string KVR { get; set; }

        public int? Year { get; set; }
        public decimal? BaseValue { get; set; }
        public decimal? Value { get; set; }

        public DSMain Clone(int? year, decimal? baseValue, decimal? value)
        {
            DSMain clone = this.Clone();

            clone.Year = year;
            clone.BaseValue = baseValue;
            clone.Value = value;

            return clone;
        }

        public DSMain Clone()
        {
            DSMain clone = new DSMain()
            {
                Id = Id,
                Caption = Caption,

                RN1 = RN1,
                IdSystemGoalElement = IdSystemGoalElement,
                CapSystemGoalElement = CapSystemGoalElement,

                RN2 = RN2,
                IdTaskCollection = IdTaskCollection,
                CapActivity = CapActivity,

                FinanceSource = FinanceSource,
                KVSR = KVSR,
                RZPR = RZPR,
                KCSR = KCSR,
                KVR = KVR
            };

            return clone;
        }
    }
}
