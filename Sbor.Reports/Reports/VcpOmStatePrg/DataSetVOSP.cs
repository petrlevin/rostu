using System;

namespace Sbor.Reports.VcpOmStatePrg
{
    public class DataSetVOSP
    {
        public int? Id { get; set; } //индетификатор документа
        public string Type { get; set; } //Тип (ВЦП и ОМ)
        public int? Number { get; set; } //номер подпрограммы
        public int? Numberssp { get; set; } //номер подпрограммы
        public int? Numberinprog { get; set; } //номер в подпрограмме 
        public int? Numberintypessp { get; set; } //номер в типе подпрограммы
        public string Substatep { get; set; } //Наименование подпрограммы ГП
        public string Statep { get; set; } //Наименование ГП
        public string Captionvcpom { get; set; } //Наименование ВЦП и ОМ
        public string Executive { get; set; } //ответственный исполнитель
        public DateTime? Datestartsub { get; set; } //дата начала Подпрограммы ГП   
        public DateTime? Dateendsub { get; set; } //дата окончания Подпрограммы ГП
        public string Goalindicator { get; set; } //наименование целевых показателей основной цели 
        public string Unitd { get; set; } //единица измерения целевого показателя
        public decimal? Value { get; set; } //значение целевого показателя с самой поздней датой из всех значений с типом "План" 
        public DateTime? Datestart { get; set; } //дата начала ГП   
        public DateTime? Dateend { get; set; } //дата окончания ГП
        public DateTime? Datestartvcpom { get; set; } //дата начала ВЦП и ОМ   
        public DateTime? Dateendvcpom { get; set; } //дата окончания ВЦП и ОМ 
        public DateTime? Datevalue { get; set; } //Дата периода
        public string SuperiorGoalIndicator { get; set; } //Целевые показатели выше стоящей цели
    }
}
