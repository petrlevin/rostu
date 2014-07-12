using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Платность
    /// </summary>
    public enum PaidType : byte
    {
        /// <summary>
        /// Бесплатная
        /// </summary>
        [EnumCaption("Бесплатная")]
        NotPaid,

        /// <summary>
        /// Платная
        /// </summary>
        [EnumCaption("Платная")]
        FullPaid,

        /// <summary>
        /// Частично платная
        /// </summary>
        [EnumCaption("Частично платная")]
        PartiallyPaid
    }
}
