using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.PlanActivity
{
    public class DSHeader
    {
        public int Id { get; set; }
        public string OrganizDescript { get; set; }         //Наименование учреждения
        public int OFG { get; set; }                        //ОФГ
        public int PFG1 { get; set; }                       //ПФГ1
        public int PFG2 { get; set; }                       //ПФГ2
        public bool IsAdditionalNeed { get; set; }
        public DateTime CurrentDate { get; set; }           //Дата формирования печатной формы
    }
}
