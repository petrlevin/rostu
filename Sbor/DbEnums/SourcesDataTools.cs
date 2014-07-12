using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Источники данных для инструментов
    /// </summary>
    public enum SourcesDataTools
    {
        /// <summary>
        /// ЭД «Смета казенного учреждения»
        /// </summary>
        [EnumCaption("Смета казенного учреждения")]
        PublicInstitutionEstimate = 1,

        /// <summary>
        /// ЭД «Деятельность ведомства»
        /// </summary>
        [EnumCaption("Деятельность ведомства")]
        ActivityOfSBP = 2
    }
}
