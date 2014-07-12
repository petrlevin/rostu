using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sbor.DbEnums;

namespace Sbor.Logic
{
    /// <summary>
    /// Параметры для поиска сметной строки
    /// </summary>
    public class FindParamEstimatedLine
    {
        /// <summary>
        /// создавать, если не нашли
        /// </summary>
        public bool IsCreate;

        /// <summary>
        /// тип бланка
        /// </summary>
        public ActivityBudgetaryType TypeLine;

        /// <summary>
        /// только обязательные коды КБК
        /// </summary>
        public bool IsRequired;

        /// <summary>
        /// 
        /// </summary>
        public bool IsKosgu000;

        /// <summary>
        /// идентификатор бюджета
        /// </summary>
        public int IdBudget;

        /// <summary>
        /// идентификатор СБП
        /// </summary>
        public int IdSbp;

        /// <summary>
        /// идентификатор ППО
        /// </summary>
        public int IdPublicLegalFormation;

        /// <summary>
        /// Создать параметры для поиска сметных строк
        /// </summary>
        public FindParamEstimatedLine()
        {
        }

        /// <summary>
        /// Создать параметры для поиска сметных строк
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="isCreate">создавать, если не нашли</param>
        /// <param name="type">тип бланка</param>
        /// <param name="isRequired">только обязательные коды КБК</param>
        /// <param name="isKOSGU000">искать сметные строки с КОСГУ 000, если не найдено с указанным в строке кодом</param>
        /// <param name="idBudget">идентификатор бюджета</param>
        /// <param name="idSBP">идентификатор СБП</param>
        /// <returns></returns>
        public FindParamEstimatedLine(DataContext context, bool isCreate, ActivityBudgetaryType type, bool isRequired, bool isKOSGU000, int idBudget, int idSBP)
            : this(isCreate, type, isRequired, isKOSGU000, idBudget, idSBP)
        {
            IdPublicLegalFormation = context.SBP.Single(s => s.Id == idSBP).IdPublicLegalFormation;
        }

        /// <summary>
        /// Создать параметры для поиска сметных строк
        /// </summary>
        /// <param name="isCreate">создавать, если не нашли</param>
        /// <param name="type">тип бланка</param>
        /// <param name="isRequired">только обязательные коды КБК</param>
        /// <param name="isKOSGU000">искать сметные строки с КОСГУ 000, если не найдено с указанным в строке кодом</param>
        /// <param name="idBudget">идентификатор бюджета</param>
        /// <param name="idSBP">идентификатор СБП</param>
        /// <param name="idPublicLegalFormation">идентификатор ППО</param>
        /// <returns></returns>
        public FindParamEstimatedLine(bool isCreate, ActivityBudgetaryType type, bool isRequired, bool isKOSGU000, int idBudget, int idSBP, int idPublicLegalFormation) : this(isCreate, type, isRequired, isKOSGU000, idBudget, idSBP)
        {
            IdPublicLegalFormation = idPublicLegalFormation;
        }

        private FindParamEstimatedLine(bool isCreate, ActivityBudgetaryType type, bool isRequired, bool isKOSGU000, int idBudget, int idSBP)
        {
            IsCreate = isCreate;
            TypeLine = type;
            IsRequired = isRequired;
            IsKosgu000 = isKOSGU000;
            IdBudget = idBudget;
            IdSbp = idSBP;
        }
    }
}
