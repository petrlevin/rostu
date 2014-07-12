using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.LongTermGoalProgram
{
    public class DataSetHeader
    {
        public int Id { get; set; }
        public string PublicLegalFormation { get; set; }    //ППО
        public string Caption { get; set; }                 //Наименование учреждения
        public string StateProgram { get; set; }            //Государственная программа, подпрограмма ГП
        public string ResponsibleExecutantType { get; set; }//Тип ответственного исполнителя
        public string ResponsibleExecutant { get; set; }    //Ответственный исполнитель ДЦП <Сноска 1>
        public string ImplementationPeriod { get; set; }    //Сроки реализации программы
        public string SystemGoal { get; set; }              //Цель программы 
        public DateTime CurrentDate { get; set; }           //Дата формирования печатной формы
    }

}
