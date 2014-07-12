using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Тип фильтра по полю
    /// </summary>
    public enum FilterFieldType
    {
        /// <summary>
        /// В списке
        /// В списке или список пустой
        /// </summary>
        [EnumCaption("В списке")]
        InList = 1,

        /// <summary>
        /// Кроме
        /// Кроме указанных
        /// </summary>
        [EnumCaption("Кроме")]
        Except = 2
    }
}
