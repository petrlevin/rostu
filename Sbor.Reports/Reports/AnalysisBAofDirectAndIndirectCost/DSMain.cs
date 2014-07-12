using System;

namespace Sbor.Reports.AnalysisBAofDirectAndIndirectCost
{
    public class DSMain
    {

        public string CapGRBS { get; set; }
        public string CapActivity { get; set; }
        public string CapSBP { get; set; }

        public int Year { get; set; }

        public int OrdColumn { get; set; }
        public string CapColumn { get; set; }

        public decimal? Value { get; set; }
    }
}
