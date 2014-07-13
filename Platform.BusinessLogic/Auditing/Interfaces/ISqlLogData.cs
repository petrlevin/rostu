using System.Data.SqlClient;

namespace Platform.BusinessLogic.Auditing.Interfaces
{
    /// <summary>
    /// Sql - логгер
    /// </summary>
    public interface ISqlLogData
    {
        /// <summary>
        /// Команда для записи лога в базу
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        SqlCommand CreateCommand(SqlConnection connection, int userId);
    }
}