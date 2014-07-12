using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Виды полномочий
    /// </summary>
    public enum AuthorityType : byte
    {
        /// <summary>
        /// Расходные обязательства субъекта РФ
        /// </summary>
        [EnumCaption("Расходные обязательства субъекта РФ")]
        SubjectOfRF = 1,

        /// <summary>
        /// Расходные обязательства муниципальных районов
        /// </summary>
        [EnumCaption("Расходные обязательства муниципальных районов")]
        Municipalities = 2,

        /// <summary>
        /// Расходные обязательства городских округов
        /// </summary>
        [EnumCaption("Расходные обязательства городских округов")]
        UrbanDistricts = 3,

        /// <summary>
        /// Расходные обязательства поселений
        /// </summary>
        [EnumCaption("Расходные обязательства поселений")]
        Settlements = 4
    }
}
