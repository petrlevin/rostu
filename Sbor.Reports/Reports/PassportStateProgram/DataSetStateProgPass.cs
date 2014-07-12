using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.Reports.PassportStateProgram
{
    public class DataSetStateProgPass
    {
        public string AnalyticCode { get; set; } //аналитический код
        public string Ppo { get; set; } //ППО
        public string Viewprog { get; set; } //Вид программы
        public string AdditionalViewProg { get; set; } //дополнительный Вид программы
        public string Typeprog { get; set; } //Тип программы
        public string Captionprog { get; set; } //Наименование программы
        public string Subprog { get; set; } //Наименование подпрограммы
        public string Executer { get; set; } //ответственный исполнитель
        public string Coexecuter { get; set; } //Соисполнители
        public string Participant { get; set; } //Участники (для программ с типом = «Ведомственная целевая программа» или «Основное мероприятие».  которой являеться вышестоящей вывести СПБ.Организацию )
        public string Goal { get; set; } //Цель = из атрибутов программы основная цель
        public string Task { get; set; } //Задачи
        public DateTime? Datestart { get; set; } //дата начала ГП   
        public DateTime? Dateend { get; set; } //дата окончания ГП
        public string Goalindicator { get; set; } //наименование целевых показателей
        public string Subproglist { get; set; } //Подпрограммы и Тип программы = «Подпрограмма ГП» или «Подпрограмма ДЦП»
        public string Dcpproglist { get; set; } //Перечень долгосрочных целевых программ и Тип программы = «Долгосрочная целевая программа»
        public string Vcpproglist { get; set; } //Перечень ВЦП и Тип программы = «Ведомственная целевая программа»
        public string Omproglist { get; set; } //Перечень основных мероприятий и Тип программы = «Основное мероприятие»
        public decimal? AmountOfCash { get; set; } //Ресурсное обеспечение. Общая сумма
        public string AmountOfCashView { get; set; } //Для отобрадения нужного количства знаков после запятой,Ресурсное обеспечение. Общая сумма
        public int? IdProgram { get; set; } //Ресурсное обеспечение. Общая сумма
        public string UnitDimension { get; set; } //Единица измерение
        public decimal? Value { get; set; } //значение
        public string Caption { get; set; } //наименование отчета
        public bool? IndicatedCumulatively { get; set; } //Сумма средств фед. бюджета 
        //public int? RegionalAmount { get; set; } //Сумма средств областного бюджета
        //public int? LocalAmount { get; set; } //Сумма средств местного бюджета
        public int? Year { get; set; } //   год Суммы средств
        public int? ExpectedOutComes { get; set; } //Ожидаемые конечные  результаты реализации программы
        public int? Numb { get; set; } //нумерация

    }
}
