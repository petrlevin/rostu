using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.Dal;

namespace BaseApp.Audit
{
    /// <summary>
    /// Sql-логгер информации о запросах к веб-сервисам
    /// </summary>
    public class RequestSqlLogData : ISqlLogData
    {
        /// <summary>
        /// Имя метода
        /// </summary>
        public string MethodName { get; set; }
        
        /// <summary>
        /// Параметры
        /// </summary>
        public string JsonData { get; set; }

        /// <summary>
        /// Время выполнения метода в миллисекундах
        /// </summary>
        public int? MethodTime { get; set; }

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
                        INSERT INTO [dbo].[request_data]
                        (
                        [MethodName],
                        [JsonData],
                        " + (MethodTime.HasValue ? "[MethodTime]," : "") + @"
                        [UserId],
                        [Date])
                    VALUES
                        (
                        @MethodName,
                        @JsonData,
                        " + (MethodTime.HasValue ? "@MethodTime, " : "") + @"
                        @UserId,
                        @Date)";

            comm.Parameters.Add("@MethodName", SqlDbType.VarChar);
            comm.Parameters.Add("@JsonData", SqlDbType.VarChar);

            if (MethodTime.HasValue)
                comm.Parameters.Add("@MethodTime", SqlDbType.Int );
            
            comm.Parameters.Add("@UserId", SqlDbType.Int);
            comm.Parameters.Add("@Date", SqlDbType.DateTime);

            comm.Parameters["@MethodName"].Value = MethodName;
            comm.Parameters["@JsonData"].Value = JsonData;
            
            if (MethodTime.HasValue)
                comm.Parameters["@MethodTime"].Value = MethodTime.Value;
            
            comm.Parameters["@UserId"].Value = userId;
            comm.Parameters["@Date"].Value = DateTime.Now;
            return comm;
        }
    }
}
