using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Типы мероприятий
    /// </summary>
    public enum ActivityType : byte
    {
        /// <summary>
        /// Услуга
        /// </summary>
        [EnumCaption("Услуга")]
        Service = 0,

        /// <summary>
        /// Работа
        /// </summary>
        [EnumCaption("Работа")]
        Work = 1,

        /// <summary>
        /// Административная функция
        /// </summary>
        [EnumCaption("Административная функция")]
        AdministrativeFunction = 2,

        /// <summary>
        /// Публичное обязательство
        /// </summary>
        [EnumCaption("Публичное обязательство")]
        PublicLiability = 3,

        /// <summary>
        /// Объект капитальных вложений
        /// </summary>
        [EnumCaption("Объект капитальных вложений")]
        SubjectCapitalInvestments = 4,

        /// <summary>
        /// Содержание имущества
        /// </summary>
        [EnumCaption("Содержание имущества")]
        ContentAsset = 5,

        /// <summary>
        /// Иная деятельность
        /// </summary>
        [EnumCaption("Иная деятельность")]
        OtherActivity = 6,

        /// <summary>
        /// Публичное нормативное обязательство
        /// </summary>
        [EnumCaption("Публичное нормативное обязательство")]
        PublicNormativeLiability = 7


    }
}
