using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.Reports.ListProgram
{
    public class DataSetListProgram
    {
        public string AnalyticalCode { get; set; } //аналитический код
        public string Ppo { get; set; } //ППО
        public string Viewprog { get; set; } //Вид программы

        public string Typeprog { get; set; } //Тип программы
        public string CaptionProg { get; set; } //Наименование программы

        public string CaptionSBP { get; set; } //ответственный исполнитель


        public string Level { get; set; } //Уровень
        public string sort { get; set; } //Поле для сортировки
        public DateTime? Datestart { get; set; } //дата начала ГП   
        public DateTime? Dateend { get; set; } //дата окончания ГП
        public decimal? AmountOfCash { get; set; } //Ресурсное обеспечение. Общая сумма
        public string AmountOfCashView { get; set; } //Для отображения нужного количства знаков после запятой,Ресурсное обеспечение. Общая сумма
        public int? IdProgram { get; set; } //индетификатор программы
        public string UnitDimension { get; set; } //Единица измерение
        public decimal? Value { get; set; } //значение
        public string Caption { get; set; } //наименование отчета
        //public int? RegionalAmount { get; set; } //Сумма средств областного бюджета
        //public int? LocalAmount { get; set; } //Сумма средств местного бюджета
        public int? Year { get; set; } //   год Суммы средств
        public int? SBPid { get; set; } //Ожидаемые конечные  результаты реализации программы
        public int? Numb { get; set; } //нумерация
    }
}
