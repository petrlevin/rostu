using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;

namespace Sbor.DbEnums
{
    /// <summary>
    /// Виды источников финансирования
    /// </summary>
    public enum FinanceSourceType : byte
    {
        /// <summary>
        /// Федеральный бюджет
        /// </summary>
        [EnumCaption("Федеральный бюджет")]
        FederalBudget = 0,

        /// <summary>
        /// Бюджет субъекта РФ
        /// </summary>
        [EnumCaption("Бюджет субъекта РФ")]
        RegionalBudgetRF = 1,

        /// <summary>
        /// Местный бюджет
        /// </summary>
        [EnumCaption("Местный бюджет")]
        LocalBudget = 2,

        /// <summary>
        /// Внебюджетные средства
        /// </summary>
        [EnumCaption("Внебюджетные средства")]
        ExtrabudgetaryFunds = 3,

        /// <summary>
        /// Территориальный фонд обязательного медицинского страхования
        /// </summary>
        [EnumCaption("Территориальный фонд обязательного медицинского страхования")]
        RegionalHealthInsuranceFund = 4,
        
        /// <summary>
        /// Пенсионный фонд РФ
        /// </summary>
        [EnumCaption("Пенсионный фонд РФ")]
        PensionFundRF = 5,
        
        /// <summary>
        /// Фонд социального страхования РФ
        /// </summary>
        [EnumCaption("Фонд социального страхования РФ")]
        SocialInsuranceFundRF = 6,

        /// <summary>
        /// Остатки
        /// </summary>
        [EnumCaption("Остатки")]
        Remains = 7,

         /// <summary>
        /// Неподтвержденные средства федерального бюджета
        /// </summary>
        [EnumCaption("Неподтвержденные средства федерального бюджета")]
        UnconfirmedFunds = 8,

        /// <summary>
        /// Иные источники
        /// </summary>
        [EnumCaption("Иные источники")]
        OtherFunds = 9
    }
}
