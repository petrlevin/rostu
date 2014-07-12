using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы значений показателей расчета
    /// </summary>
    public enum IndicatorCalculationValueType : byte
    {
        /// <summary>
        /// Константа
        /// </summary>
        [EnumCaption("Константа")]
        Constant,

        /// <summary>
        /// Переменная
        /// </summary>
        [EnumCaption("Переменная")]
        Variant
    }
}
