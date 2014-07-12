using System;

namespace Sbor.Reports.StateProgramGoalIndicatorValue
{
    public class DSHeader
    {
        public string StateProgramName { get; set; }
        public int ImplementationPeriodStart { get; set; }
        public int ImplementationPeriodEnd { get; set; }
        public string Header { get; set; }
        public string Executive { get; set; }
        public DateTime CurrentDate { get; set; }           //Дата формирования печатной формы
    }
}
