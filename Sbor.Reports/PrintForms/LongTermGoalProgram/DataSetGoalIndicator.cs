using System;

namespace Sbor.Reports.PrintForms.LongTermGoalProgram
{
    public class DataSetGoalIndicator:ICloneable//,IHierachy
    {
        public int Id { get; set;}
        public int? IdParent { get; set; }
        public string HierarchyNumber { get; set; }
        public bool IsMainGoal { get; set; }
        public string Goal { get; set; }
        public string GoalIndicator { get; set; }
        public string UnitDimension { get; set; }
        public int? Year { get; set; }
        public decimal? Value { get; set; }

        public DataSetGoalIndicator Clone(int? year , int? value)
        {
            DataSetGoalIndicator clone = (DataSetGoalIndicator)this.Clone();
            clone.Year = year;
            clone.Value = value;
            return clone;
        }

        public object Clone()
        {
            object clone = new DataSetGoalIndicator
                {
                    Id=Id,
                    IdParent=IdParent,
                    HierarchyNumber=HierarchyNumber,
                    IsMainGoal=IsMainGoal,
                    Goal=Goal,
                    GoalIndicator=GoalIndicator,
                    UnitDimension=UnitDimension,
                    Year=Year,
                    Value=Value
                };
            return clone;
        }
    }
}
