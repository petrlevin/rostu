using Platform.Common;

namespace Sbor.Reports.DbEnums
{

    /// <summary>
    /// Источники данных для отчетов
    /// </summary>
    public enum SourcesDataReports
    {
        /// <summary>
        /// Смета казенного учреждения
        /// </summary>
        [EnumCaption("Смета казенного учреждения")]
        BudgetEstimates = 0,

        /// <summary>
        /// Документы
        /// </summary>
        [EnumCaption("Документы")]
        Document = 1,

        /// <summary>
        /// Деятельность ведомства
        /// </summary>
        [EnumCaption("Деятельность ведомства")]
        JustificationBudget = 2,

        /// <summary>
        /// Инструмент балансировки доходов, расходов, ИФДБ (источник - Смета казенного учреждения)
        /// </summary>
        [EnumCaption("Инструмент балансировки доходов, расходов, ИФДБ (источник - Смета казенного учреждения)")]
        InstrumentBalancingSourceEstimates = 3,

        /// <summary>
        /// Инструмент балансировки доходов, расходов, ИФДБ (источник - Деятельность ведомства)
        /// </summary>
        [EnumCaption("Инструмент балансировки доходов, расходов, ИФДБ (источник - Деятельность ведомства)")]
        InstrumentBalancingSourceActivityOfSBP = 4,

        /// <summary>
        /// Ресурсное обеспечение мероприятий
        /// </summary>
        [EnumCaption("Ресурсное обеспечение мероприятий")]
        ResourceMaintenanceActivities = 6,

        /// <summary>
        /// Справочник «Система целеполагания»
        /// </summary>
        [EnumCaption("Справочник «Система целеполагания»")]
        RefSystemGoal = 5,

        /// <summary>
        /// Предельные объемы БА (ГРБС)
        /// </summary>
        [EnumCaption("Предельные объемы БА (ГРБС)")]
        LimitBudgetAllocations = 7
    }
}
