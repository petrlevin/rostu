using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Разделы реестра мероприятий
    /// </summary>
    public enum RegistryKeyActivity : byte
    {
        /// <summary>
        /// Раздел 1
        /// </summary>
        [EnumCaption("Раздел 1")]
        Key1,

        /// <summary>
        /// Раздел 2
        /// </summary>
        [EnumCaption("Раздел 2")]
        Key2,

        /// <summary>
        /// Раздел 3
        /// </summary>
        [EnumCaption("Раздел 3")]
        Key3
    }
}
