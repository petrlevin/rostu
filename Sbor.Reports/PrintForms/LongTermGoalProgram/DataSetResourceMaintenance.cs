using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.LongTermGoalProgram
{
    public class DataSetResourceMaintenance : ICloneable
    {
        public string DocType { get; set; }
        public string SubProgram { get; set; }
        public string FinanceSource { get; set; }
        public int? Year { get; set; }
        public decimal? Value { get; set; }

        public DataSetResourceMaintenance Clone(int? year, int? value)
        {
            DataSetResourceMaintenance clone = (DataSetResourceMaintenance)this.Clone();
            clone.Year = year;
            clone.Value = value;
            return clone;
        }

        public object Clone()
        {
            object clone = new DataSetResourceMaintenance
            {
                DocType = DocType,
                SubProgram = SubProgram,
                FinanceSource = FinanceSource,
                Year = Year,
                Value = Value
            };
            return clone;
        }
    }
}
