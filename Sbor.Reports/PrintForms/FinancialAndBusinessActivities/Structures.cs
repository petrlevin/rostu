using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.FinancialAndBusinessActivities
{
    public class DSTopOfDoc
    {
        public int Id { get; set; }
        public string Caption { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public DateTime CurrentDate { get; set; }
        public int IdPublicLegalFormation { get; set; }
        public string CapPublicLegalFormation { get; set; }
        public string Header { get; set; }
        public int BudgetYear { get; set; }
        public string sn1 { get; set; }
        public DateTime sn4 { get; set; }
        public string sn5 { get; set; }
        public string sn6 { get; set; }
        public string sn7 { get; set; }
        public string sn8 { get; set; }
        public string sn9 { get; set; }
        public string sn10 { get; set; }
        public bool HasAddValue { get; set; }
    }

    public class ListCaption
    {
        public string Caption { get; set; }
    }

    public class DSFinInd
    {
        public string Caption { get; set; }
        public bool Bold { get; set; }
        public decimal ? Value { get; set; }
        public int pleft { get; set; }
    }

    public class DSPlanIncome
    {
        public int tline { get; set; }
        public string KOSGU { get; set; }
        public string Caption { get; set; }
        public decimal? Value { get; set; }
        public decimal? Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public decimal? ExValue { get; set; }
        public decimal? ExValue1 { get; set; }
        public decimal? ExValue2 { get; set; }
        public decimal? KValue { get; set; }
        public decimal? KValue1 { get; set; }
        public decimal? KValue2 { get; set; }
        public decimal? KExValue { get; set; }
        public decimal? KExValue1 { get; set; }
        public decimal? KExValue2 { get; set; }
    }
}
