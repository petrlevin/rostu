using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.PublicInstitutionEstimate
{
    public class DataSetPIE
    {
        public int Id { get; set; }//индетификатор документа
        public string Activity { get; set; }//Наименование мероприятия
        public string Expense { get; set; }//Наименование расходов
        public string KVSR { get; set; }//КВСР
        public string RZPR { get; set; }//РзПР
        public string KCSR { get; set; }//КЦСР
        public string KVR { get; set; }//КВР
        public string KOSGU { get; set; }//КОСГУ
        public string KFO { get; set; }//КФО
        public string DKR { get; set; }//ДКР
        public string DFK { get; set; }//ДФК
        public string DEK { get; set; }//ДЭК
        public string Codesybs { get; set; }//Код субсидии
        public string Branchcode { get; set; }//Отраслевой код
        public decimal? OfgbvAll { get; set; }//OFGBVAll Очередной финансовый год - Базовый вариант - Всего 
        public decimal? OfgbvIndirectCosts { get; set; }//OFGBVIndirectCosts Очередной финансовый год - Базовый вариант - В т.ч. косвенные расходы
        public decimal? OfgdpAll { get; set; }//OFGDPAll Очередной финансовый год - Доп. потребность - Всего 
        public decimal? OfgdpIndirectCosts { get; set; }//OFGDPIndirectCosts Очередной финансовый год - Доп. потребность - В т.ч. косвенные расходы
        public decimal? Ofg1BvAll { get; set; }//OFG1BVAll Плановый период  - Базовый вариант - Всего 
        public decimal? Ofg1BvIndirectCosts { get; set; }//OFG1BVIndirectCosts Плановый период  - Базовый вариант - В т.ч. косвенные расходы
        public decimal? Ofg1DpAll { get; set; }//OFG1DPAll Плановый период  - Доп. потребность - Всего 
        public decimal? Ofg1DpIndirectCosts { get; set; }//OFG1DPIndirectCosts Плановый период  - Доп. потребность - В т.ч. косвенные расходы
        public decimal? Ofg2BvAll { get; set; }//OFG2BVOMAll Плановый период  - Базовый вариант - Всего 
        public decimal? Ofg2BvIndirectCosts { get; set; }//OFG2BVOMIndirectCosts Плановый период  - Базовый вариант - В т.ч. косвенные расходы
        public decimal? Ofg2DpAll { get; set; }//OFG2DPOMAll Плановый период  - Доп. потребность - Всего 
        public decimal? Ofg2DpIndirectCosts { get; set; }//OFG2DPOMIndirectCosts Плановый период  - Доп. потребность - В т.ч. косвенные расходы
        public int? Budgetyear { get; set; }//год бюджета 
        public int? Idsbpparent { get; set; }//ид сбп родителя 
        public string Numbersmet { get; set; }//Номер сметы из одкумента
        public DateTime? Datesmet { get; set; }//дата сметы из одкумента
        public string institution { get; set; }//Получатель бюджетных средств (учреждение сметы из одкумента)
        public string Mainbk { get; set; }// Глава по БК
        public string Grbs { get; set; }//ГРБС
    }
}
