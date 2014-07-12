namespace Sbor.Reports.ConsolidatedExpenditure
{
    public class PrimeDataSet
    {
        public string year { get; set; }
        public string curDate { get; set; }
        public string UnitDimension { get; set; }
        
    }

    public class regDate
    {
        public int idRZPR { get; set; }
        public string RZPRCaption { get; set; }
        public int idKOSGU { get; set; }
        public string KOSGUCaption { get; set; }
        public int idBudgetLevel { get; set; }
        public int Value { get; set; }
    }

    public class TableSet
    {
        public int typeline { get; set; }
        public string col1 { get; set; }
        public string col3 { get; set; }
        public int col6 { get; set; }
        public int col7 { get; set; }
        public int col8 { get; set; }
        public int col10 { get; set; }
        public int col11 { get; set; }
        public int col12 { get; set; }
    }
}
