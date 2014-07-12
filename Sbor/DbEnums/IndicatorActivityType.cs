using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы показателей мероприятий
    /// </summary>
    public enum IndicatorActivityType : byte
    {
        /// <summary>
        /// Показатель качества
        /// </summary>
        [EnumCaption("Показатель качества")]
        QualityIndicator,

        /// <summary>
        /// Показатель объема
        /// </summary>
        [EnumCaption("Показатель объема")]
        VolumeIndicator
    }
}
