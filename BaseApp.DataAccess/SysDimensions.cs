using System.Data.Entity;
using System.Linq;
using BaseApp.Reference;
using Platform.BusinessLogic;
using Platform.Common;
using Version = BaseApp.Reference.Version;

namespace BaseApp.DataAccess
{
	/// <summary>
	/// класс реализующий получение системных измерений из датаконтекста по идентификатору
	/// </summary>
	public static class SysDimensions
	{
		/// <summary>
		/// Получение DataContext
		/// </summary>
		/// <returns></returns>
		private static DataContext _getDataContext()
		{
			return IoC.Resolve<DbContext>().Cast<DataContext>();
		}

		/// <summary>
		/// Получение PublicLegalFormation по идентифкатору
		/// </summary>
		/// <param name="id">Идентификатор</param>
		/// <returns></returns>
		public static PublicLegalFormation PublicLegalFormationById(int id)
		{
			var dataContext = _getDataContext();
			return dataContext.PublicLegalFormation.SingleOrDefault(a => a.Id == id);
		}

		/// <summary>
		/// Получение Version по идентифкатору
		/// </summary>
		/// <param name="id">Идентификатор</param>
		/// <returns></returns>
		public static Version VersionById(int id)
		{
			var dataContext = _getDataContext();
			return dataContext.Version.SingleOrDefault(a => a.Id == id);
		}

		/// <summary>
		/// Получение Budget по идентифкатору
		/// </summary>
		/// <param name="id">Идентификатор</param>
		/// <returns></returns>
		public static Budget BudgetById(int id)
		{
			var dataContext = _getDataContext();
			return dataContext.Budget.SingleOrDefault(a => a.Id == id);
		}

	}
}
