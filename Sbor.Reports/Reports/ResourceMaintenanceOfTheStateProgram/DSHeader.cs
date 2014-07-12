using System;

namespace Sbor.Reports.ResourceMaintenanceOfTheStateProgram
{
    public class DSHeader
    {
        public string StateProgramName { get; set; }
        public int ImplementationPeriodStart { get; set; }
        public int ImplementationPeriodEnd { get; set; }
        public string Header { get; set; }
        public string Executive { get; set; }
        public DateTime CurrentDate { get; set; }           //Дата формирования печатной формы
        public string ReportCaption { get; set; }
        public bool BySource { get; set; }
        public string YearCaption { get; set; }
        public string TypeCaption { get; set; }
        public bool HasNoFunds { get; set; }
    }
}
