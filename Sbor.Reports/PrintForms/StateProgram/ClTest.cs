using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.StateProgram
{
    public class ClTest
    {
        public string NumberName { get; set; }
        public string GoalIndicator { get; set; }//цель задачи
        public string Measure { get; set; }//Единица измерения
        public int? NumYear { get; set; }//Год фиксации показателя
        public decimal? GIValue { get; set; }//Значение показателя
    }
}
