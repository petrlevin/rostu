using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы аналитических кодов государственных программ
    /// </summary>
    public enum TypeOfAnalyticalCodeStateProgram : byte
    {
        /// <summary>
        /// Государственная программа 
        /// </summary>
        [EnumCaption("Государственная программа")]
        StateProgram = 1,

        /// <summary>
        /// Подпрограмма ГП
        /// </summary>
        [EnumCaption("Подпрограмма ГП")]
        StateSubprogram = 2,

        /// <summary>
        /// Основное мероприятие
        /// </summary>
        [EnumCaption("Основное мероприятие")]
        MainAction = 3
    }
}
