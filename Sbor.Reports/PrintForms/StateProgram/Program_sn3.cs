using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.StateProgram
{
    public class Program_sn3
    {
        public string StProgram { get; set; }//Наименование государственной программы
        public string StateUndProgram { get; set; }//Наименование подпрограммы государственной программы
        public string TypeExecuter { get; set; }//Тип ответственного исполнителя 
        public string Executer { get; set; }//наименование организации-владельца ведомства, указанного в поле «Ответственный исполнитель» 
        public int DateStart { get; set; }//«Срок реализации с»
        public int DateEnd { get; set; }//«Срок реализации по»
        public string MainGoal { get; set; }//Наименование основной цели
        public string SubjectPPO { get; set; }//Государственная программа.ППО
        public string SubProgramExecuter { get; set; }//наименование организации-владельца ведомства, указанного в поле «Ответственный исполнитель»
    }
}
