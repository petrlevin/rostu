using System.Data.SqlClient;
using Platform.Dal.Common.Interfaces.QueryBuilders;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Базовый класс менеджера работы с данными
    /// </summary>
	public abstract class ManagerBase
	{
		protected SqlConnection DbConnection { get; set; }

		/// <summary>
		/// Метод получения данных из БД
		/// </summary>
		/// <param name="query">Запрос</param>
		/// <returns></returns>
		protected GridResult GetDataSet(ISelectQueryBuilder query)
		{
			SqlCommand cmd = query.GetSqlCommand(DbConnection);
			var result = SqlHelper.MakeResult(cmd);
		    return result;
		}
	}
}
