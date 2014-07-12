using System;

namespace Sbor.Reports.Reports.BudgetExpenseStructure
{
    /// <summary>
    /// Строка данных для отчета
    /// </summary>
    public class BudgetExpenseLine
    {
        /// <summary>
        /// Наименование строки "года". Для кастомных колонок включает в себя название колонки + год
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Поле для сортировки по годам
        /// </summary>
        public int Sort { get; set; }
        
        /// <summary>
        /// Наименование строки данных
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Выделять строку жЫрным
        /// </summary>
        public int IsBold { get; set; }

        /// <summary>
        /// Тип цели
        /// </summary>
        public string SystemGoalType { get; set; }

        /// <summary>
        /// Уровень программы, нужен только для форматов в отчетной форме
        /// </summary>
        public int? LevelProgram { get; set; }

        /// <summary>
        /// Уровень программы, нужен только для форматов в отчетной форме
        /// </summary>
        public int? IsBoldSystemGoal { get; set; }

        /// <summary>
        /// Идентификатор мероприятия, нужен только для группировки в отчетной форме
        /// </summary>
        public int? IdActivity { get; set; }

        /// <summary>
        /// Идентификатор программы, нужен только для группировки в отчетной форме
        /// </summary>
        public int? IdProgram { get; set; }

        /// <summary>
        /// Идентификатор цели, нужен только для группировки в отчетной форме
        /// </summary>
        public int? IdSystemGoalElement { get; set; }

        public string S1 { get; set; }
        public string S2 { get; set; }
        public string S3 { get; set; }
        public string S4 { get; set; }
        public string S5 { get; set; }
        public string S6 { get; set; }
        public string S7 { get; set; }
        public string S8 { get; set; }
        public string S9 { get; set; }
        public string S10 { get; set; }
        public string S11 { get; set; }
        public string S12 { get; set; }
        public string S13 { get; set; }

    }

    public class HeaderInfo
    {
        public String Caption { get; set; }

        public String Unit { get; set; }

        public String ReportDate { get; set; }
    }

    public class ColumnsInfo
    {
        public String Name { get; set; }

        public short Order { get; set; }

        public String Caption { get; set; }
    }

}
