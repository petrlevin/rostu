using System;

namespace Sbor.Reports.StateProgramGoalIndicatorValue
{
    public class DSMain:ICloneable
    {
        public int ProgramId { get; set; }
        public int ProgramType { get; set; } //ГП, пГП, ДЦП
        public string ProgramNumber { get; set; } //14,26

        public string ProgramName { get; set; } //1,15,27
        public string AnalyticalCode { get; set; }
        public int? Executive { get; set; } //3
        public DateTime? ImplementationPeriodStart { get; set; } //2,16,28
        public DateTime? ImplementationPeriodEnd { get; set; } //2,16,28
        public string GoalIndicatorNumber { get; set; }
        public int? GoalIndicatorId { get; set; }
        public string GoalIndicatorName { get; set; }
        public string GoalIndicatorUnitDimension { get; set; }
        public string YearCaption { get; set; }
        public int? Year { get; set; }
        public decimal? Value { get; set; }

        public DSMain Clone(int? year, int? value)
        {
            DSMain clone = (DSMain)this.Clone();
            clone.Year = year;
            clone.Value = value;
            return clone;
        }

        public object Clone()
        {

            //object clone = new DSMain
            //{
            //    ProgramId=ProgramId,
            //    ProgramType=ProgramType,
            //    ProgramNumber=ProgramNumber,

            //    ProgramName=ProgramName,
            //    AnalyticalCode = AnalyticalCode,
            //    Executive=Executive,
            //    ImplementationPeriodStart=ImplementationPeriodStart,
            //    ImplementationPeriodEnd=ImplementationPeriodEnd,
            //    GoalIndicatorNumber=GoalIndicatorNumber,
            //    GoalIndicatorId=GoalIndicatorId,
            //    GoalIndicatorName=GoalIndicatorName,
            //    GoalIndicatorUnitDimension=GoalIndicatorUnitDimension,
            //    YearCaption=YearCaption,
            //    Year = Year,
            //    Value = Value
            //};
            //return clone;

            return this.MemberwiseClone();
      }

    }
}
