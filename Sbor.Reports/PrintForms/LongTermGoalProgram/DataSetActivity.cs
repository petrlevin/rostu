using System;

namespace Sbor.Reports.PrintForms.LongTermGoalProgram
{
    public class DataSetActivity : ICloneable//, IHierachy
    {
        public int Id { get; set; }
        public int? IdParent { get; set; }
        public string HierarchyNumber { get; set; }

        public bool IsMainGoal { get; set; }
        public string GoalNumber { get; set; }
        public string Goal { get; set; }
        public int? ActivityId { get; set; }
        public string ActivityNumber { get; set; }
        public string ActivityName { get; set; }
        public string Executor { get; set; }
        public string FeatureName { get; set; }
        public string FinanceSource { get; set; }
        public string UnitDimension { get; set; }
        public int? Year { get; set; }
        public decimal? Value { get; set; }

        public DataSetActivity Clone(int? year, int? value)
        {
            DataSetActivity clone = (DataSetActivity)this.Clone();
            clone.Year = year;
            clone.Value = value;
            return clone;
        }

        public object Clone()
        {
            object clone = new DataSetActivity
                {
                    Id=Id,
                    IdParent=IdParent,
                    HierarchyNumber=HierarchyNumber,
                    IsMainGoal=IsMainGoal,
                    GoalNumber=GoalNumber,
                    Goal=Goal,
                    ActivityNumber=ActivityNumber,
                    ActivityName=ActivityName,
                    Executor=Executor,
                    FeatureName=FeatureName,
                    FinanceSource=FinanceSource,
                    UnitDimension=UnitDimension,
                    Year=Year,
                    Value=Value
                };
            return clone;
        }
    }
}
