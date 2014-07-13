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
    /// Sql-логгер событий в сессии
    /// </summary>
    public class SessionSqlLog : ISqlLogData
    {
        /// <summary>
        /// Идентификатор сессии
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// Идентификатор события
        /// </summary>
        public byte EventId { get; set; }
        
        /// <summary>
        /// Время события
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
                        INSERT INTO [dbo].[Sessions]
                        (
                        [SessionId],
                        [EventId],
                        [Time]
                        )
                    VALUES
                        (
                        @SessionId,
                        @EventId, 
                        @Time
                        )";

            comm.Parameters.Add("@SessionId", SqlDbType.NVarChar, 50);
            comm.Parameters.Add("@EventId", SqlDbType.TinyInt);
            comm.Parameters.Add("@Time", SqlDbType.DateTime);

            comm.Parameters["@SessionId"].Value = SessionId;
            comm.Parameters["@EventId"].Value = EventId;
            comm.Parameters["@Time"].Value = Time;

            return comm;
        }
    }
}
