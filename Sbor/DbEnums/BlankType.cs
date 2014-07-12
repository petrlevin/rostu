using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы бланков СБП
    /// </summary>
    public enum BlankType : byte
    {
        /// <summary>
        /// Доведение ГРБС
        /// </summary>
        [EnumCaption("Доведение ГРБС")]
        BringingGRBS = 1,

        /// <summary>
        /// Доведение РБС
        /// </summary>
        [EnumCaption("Доведение РБС")]
        BringingRBS = 2,

        /// <summary>
        /// Доведение АУ/БУ
        /// </summary>
        [EnumCaption("Доведение АУ/БУ")]
        BringingAUBU = 3,

        /// <summary>
        /// Доведение КУ
        /// </summary>
        [EnumCaption("Доведение КУ")]
        BringingKU = 4,

        /// <summary>
        /// Формирование АУ/БУ
        /// </summary>
        [EnumCaption("Формирование АУ/БУ")]
        FormationAUBU = 5,

        /// <summary>
        /// Формирование КУ
        /// </summary>
        [EnumCaption("Формирование КУ")]
        FormationKU = 6,

        /// <summary>
        /// Формирование ГРБС
        /// </summary>
        [EnumCaption("Формирование ГРБС")]
        FormationGRBS = 7
    }
}
