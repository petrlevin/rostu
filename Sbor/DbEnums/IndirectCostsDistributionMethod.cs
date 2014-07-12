using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Методы распределения косвенных расходов
    /// </summary>
    public enum IndirectCostsDistributionMethod : byte
    {
        /// <summary>
        /// Равное распределение между мероприятиями
        /// </summary>
        [EnumCaption("Равное распределение между мероприятиями")]
        M1 = 1,

        /// <summary>
        /// Пропорционально прямым расходам по мероприятиям
        /// </summary>
        [EnumCaption("Пропорционально прямым расходам по мероприятиям")]
        M2 = 2,

        /// <summary>
        /// Пропорционально объему предоставляемых мероприятий
        /// </summary>
        [EnumCaption("Пропорционально объему предоставляемых мероприятий")]
        M3 = 3,

        /// <summary>
        /// Задаваемый коэффициент распределения
        /// </summary>
        [EnumCaption("Задаваемый коэффициент распределения")]
        M4 = 4,

        /// <summary>
        /// Пропорционально прямым расходам по указанным КОСГУ
        /// </summary>
        [EnumCaption("Пропорционально прямым расходам по указанным КОСГУ")]
        M5 = 5
    }
}
