namespace Sbor.Reports.InterBudgetaryTransfers
{
    public class PrimeDataSet
    {
        public string caption { get; set; }
        public string header { get; set; }
        public string curDate { get; set; }
        public string namefirstcolumn { get; set; }
        public string udname { get; set; }
        public int countYear { get; set; }
    }

    public class TableSet
    {
        public int typeline { get; set; }
        public int? numline { get; set; }
        public int YearHP { get; set; }
        public string columnname { get; set; }
        public string keycolumnname { get; set; }
        public int iHP { get; set; }
        public string budgetlevel { get; set; }
        public string keybudgetlevel { get; set; }
        public string okato { get; set; }
        public decimal? value { get; set; }
    }
}
