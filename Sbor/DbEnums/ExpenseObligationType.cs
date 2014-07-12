using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы РО
    /// </summary>
    public enum ExpenseObligationType : byte
    {
        /// <summary>
        /// Действующие
        /// </summary>
        [EnumCaption("Действующие")]
        Existing = 1,

        /// <summary>
        /// Принимаемые
        /// </summary>
        [EnumCaption("Принимаемые")]
        Accepted = 2
    }
}
