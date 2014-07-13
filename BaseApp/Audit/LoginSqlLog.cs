using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.Dal;

namespace BaseApp.Audit
{
    /// <summary>
    /// Логгер входов в систему
    /// </summary>
    public class LoginSqlLog : ISqlLogData
    {
        /// <summary>
        /// Идентификатор сессии 
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Время входа в систему
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Команда для записи лога в базу
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public SqlCommand CreateCommand(SqlConnection connection, int userId)
        {
            var comm = new SqlCommandFactory(connection).CreateCommand();

            comm.CommandText = @"
                        INSERT INTO [dbo].[Logins]
                        (
                        [SessionId],
                        [UserId],
                        [Time]
                        )
                    VALUES
                        (
                        @SessionId,
                        @UserId, 
                        @Time
                        )";

            comm.Parameters.Add("@SessionId", SqlDbType.NVarChar, 50);
            comm.Parameters.Add("@UserId", SqlDbType.Int);
            comm.Parameters.Add("@Time", SqlDbType.DateTime);

            comm.Parameters["@SessionId"].Value = SessionId;
            comm.Parameters["@UserId"].Value = userId;
            comm.Parameters["@Time"].Value = Time;

            return comm;
        }
    }
}
