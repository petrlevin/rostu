using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Условия восприятия
    /// </summary>
    public enum TermsOfPerception : byte
    {
        /// <summary>
        /// Больше – лучше
        /// </summary>
        [EnumCaption("Больше – лучше")]
        MoreBetter = 1,

        /// <summary>
        /// Меньше – лучше
        /// </summary>
        [EnumCaption("Меньше – лучше")]
        LessBetter = 2,

        /// <summary>
        /// Отсутствие отклонений
        /// </summary>
        [EnumCaption("Отсутствие отклонений")]
        NoDeviations = 3
    }
}
