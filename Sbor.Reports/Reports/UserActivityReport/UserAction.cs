using System;
using System.Linq;
using System.Xml.Linq;
using Platform.BusinessLogic.Common.Enums;
using Platform.Common.Extensions;

namespace Sbor.Reports.UserActivityReport
{
    public class UserAction
    {
        public DateTime Date { get; set; }

        public byte Operation { get; set; }

        public string OperationCaption
        {
            get { return ((Operations)Operation).Caption(); }
        }

        public int EntityId { get; set; }

        public string EntityCaption { get; set; }

        public int ElementId { get; set; }

        public string ElementCaption { get; set; }

        public string Before { get; set; }

        public string After { get; set; }

        public int IdUser { get; set; }

        public string UserCaption { get; set; }
    }

    public class CommonInfo
    {
        public string UserCaption { get; set; }
        
        public string ReportElementCaption { get; set; }
        
        public string DateFrom { get; set; }
        
        public string DateTo { get; set; }
    }
}

    
