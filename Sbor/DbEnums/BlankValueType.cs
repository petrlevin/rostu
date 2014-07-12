using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы значений в бланках СБП
    /// </summary>
    public enum BlankValueType : byte
    {
        /// <summary>
        /// Пусто
        /// </summary>
        [EnumCaption("Пусто")]
        Empty = 0,

        /// <summary>
        /// Обязательное
        /// </summary>
        [EnumCaption("Обязательное")]
        Mandatory = 1,

        /// <summary>
        /// Необязательное
        /// </summary>
        [EnumCaption("Необязательное")]
        Optional = 2
    }
}
