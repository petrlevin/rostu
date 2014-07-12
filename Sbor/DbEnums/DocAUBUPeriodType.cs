using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы периодов в документах АУ/БУ
    /// </summary>
    public enum DocAUBUPeriodType : byte
    {
        /// <summary>
        /// Пусто
        /// </summary>
        [EnumCaption("Пусто")]
        Empty = 0,

        /// <summary>
        /// Год
        /// </summary>
        [EnumCaption("Год")]
        Year = 1,

        /// <summary>
        /// Кварталы
        /// </summary>
        [EnumCaption("Кварталы")]
        Quarter = 3,

        /// <summary>
        /// Месяцы
        /// </summary>
        [EnumCaption("Месяцы")]
        Month = 4
    }
}
