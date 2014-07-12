namespace Sbor.Reports.PrintForms.LongTermGoalProgram
{
    public class DataSetHierarchy //: IHierachy
    {
        public int Id { get; set; }
        public int? IdParent { get; set; }
        public string HierarchyNumber { get; set; }
        public string Value { get; set; }
    }
}