using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы значений в регистрах
    /// </summary>
    public enum ValueType : byte
    {
        /// <summary>
        /// План
        /// </summary>
        [EnumCaption("План")]
        Plan = 1,

        /// <summary>
        /// Факт
        /// </summary>
        [EnumCaption("Факт")]
        Fact = 2,

        /// <summary>
        /// Доведено
        /// </summary>
        [EnumCaption("Доведено")]
        Bring = 3,

        /// <summary>
        /// Обосновано
        /// </summary>
        [EnumCaption("Обосновано")]
        Justified = 4,

        /// <summary>
        /// Обосновано ПФХД
        /// </summary>
        [EnumCaption("Обосновано ПФХД")]
        JustifiedFBA = 5,

        /// <summary>
        /// План ПФХД
        /// </summary>
        [EnumCaption("План ПФХД")]
        PlanFBA = 6,

        /// <summary>
        /// Спрос
        /// </summary>
        [EnumCaption("Спрос")]
        Demand = 7,

        /// <summary>
        /// Мощность
        /// </summary>
        [EnumCaption("Мощность")]
        Capacity = 8,   
        
        /// <summary>
        /// Обосновано ГРБС
        /// </summary>
        [EnumCaption("Обосновано ГРБС")]
        JustifiedGRBS = 9,

        /// <summary>
        /// Балансировка расходов, доходов, ИФДБ - расходы (источник Смета)
        /// </summary>
        [EnumCaption("Балансировка расходов, доходов, ИФДБ - расходы (источник Смета)")]
        BalancingIFDB_Estimate = 10,

        /// <summary>
        /// Балансировка расходов, доходов, ИФДБ - расходы (источник ДВ)
        /// </summary>
        [EnumCaption("Балансировка расходов, доходов, ИФДБ - расходы (источник ДВ)")]
        BalancingIFDB_ActivityOfSBP = 11
    }
}
