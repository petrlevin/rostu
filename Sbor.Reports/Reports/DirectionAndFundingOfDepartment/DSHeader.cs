using System;

namespace Sbor.Reports.Reports.DirectionAndFundingOfDepartment
{
    public class DSHeader
    {
        public string PublicLegalFormationName { get; set; }
        public string ProgramName { get; set; }
        public string Contractor { get; set; }
        public string ReportCaption { get; set; }
        public DateTime CurrentDate { get; set; }           //Дата формирования печатной формы
        public bool IsRatingAdditionalNeeds { get; set; }
    }
}
