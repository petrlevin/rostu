using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы СБП
    /// </summary>
    public enum SBPType : byte
    {
        /// <summary>
        /// Главный распорядитель БС
        /// </summary>
        [EnumCaption("Главный распорядитель БС")]
        GeneralManager = 1,

        /// <summary>
        /// Распорядитель БС
        /// </summary>
        [EnumCaption("Распорядитель БС")]
        Manager = 2,

        /// <summary>
        /// Казенное учреждение, ОГВ (ОМСУ), структурное подразделение
        /// </summary>
        [EnumCaption("Казенное учреждение, ОГВ (ОМСУ), структурное подразделение")]
        TreasuryEstablishment = 3,

        /// <summary>
        /// Бюджетное учреждение
        /// </summary>
        [EnumCaption("Бюджетное учреждение")]
        BudgetEstablishment = 4,

        /// <summary>
        /// Автономное учреждение
        /// </summary>
        [EnumCaption("Автономное учреждение")]
        IndependentEstablishment = 5
    }
}
