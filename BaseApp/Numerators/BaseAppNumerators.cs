using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using BaseApp.Reference;
using BaseApp.SystemDimensions;
using Platform.Common;
using Version = BaseApp.Reference.Version;

namespace BaseApp.Numerators
{
	/// <summary>
	/// Нумераторы системных измерений
	/// </summary>
	public class BaseAppNumerators
	{
		private readonly SysDimensionsState _sysDimensionsState = IoC.Resolve<SysDimensionsState>("CurentDimensions");

		/// <summary>
		/// Получение идентификатора текущего PublicLegalFormation
		/// </summary>
		/// <returns></returns>
		public int IdPublicLegalFormation()
		{
			PublicLegalFormation publicLegalFormation = (_sysDimensionsState.PublicLegalFormation as PublicLegalFormation);
			return publicLegalFormation == null ? 0 : publicLegalFormation.Id;
		}

		/// <summary>
		/// Получение идентификатора текущего Budget
		/// </summary>
		/// <returns></returns>
		public int IdBudget()
		{
			Budget budget = (_sysDimensionsState.Budget as Budget);
			return budget == null ? 0 : budget.Id;
		}

		/// <summary>
		/// Получение идентификатора текущего Version
		/// </summary>
		/// <returns></returns>
		public int IdVersion()
		{
			Version version = (_sysDimensionsState.Version as Version);
			return version == null ? 0 : version.Id;
		}

        /// <summary>
		/// Получение идентификатора текущего AccessGroup
        /// </summary>
        /// <returns></returns>
        public int? IdAccessGroup()
        {
			PublicLegalFormation publicLegalFormation = (_sysDimensionsState.PublicLegalFormation as PublicLegalFormation);
	        return publicLegalFormation == null ? 0 : publicLegalFormation.IdAccessGroup;
        }

        /// <summary>
        /// Получение года текущего бюджета
        /// </summary>
        /// <returns></returns>
        public int BudgetYear()
        {
            Budget budget = (_sysDimensionsState.Budget as Budget);
            return budget == null ? 0 : budget.Year;
        }

	    public int? IdUser()
	    {
	        var user = IoC.Resolve<IUser>("CurrentUser");
            return user == null ? 0 : user.Id;
	    }
	}
}
