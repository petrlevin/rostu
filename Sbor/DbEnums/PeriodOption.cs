using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Варианты периодов
    /// </summary>
    public enum PeriodOption : byte
    {
        /// <summary>
        /// Только ОФГ
        /// </summary>
        [EnumCaption("Только ОФГ")]
        OFG,

        /// <summary>
        /// ОФГ и плановый период
        /// </summary>
        [EnumCaption("ОФГ и плановый период")]
        PlanOFG,

        /// <summary>
        /// Только плановый период
        /// </summary>
        [EnumCaption("Только плановый период")]
        Plan
    }
}
