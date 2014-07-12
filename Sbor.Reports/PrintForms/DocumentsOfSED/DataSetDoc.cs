using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.DocumentsOfSED
{
    public class DataSetDoc
    {
        public int Id { get; set; }
        public string Caption { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public DateTime CurrentDate { get; set; }
        public int IdPublicLegalFormation { get; set; }
        public string CapPublicLegalFormation { get; set; }
        public string Header { get; set; }
    }
}
