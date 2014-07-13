using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;

namespace Platform.Dal
{
    /// <summary>
    /// Фабрика Sql-команд
    /// </summary>
    public class SqlCommandFactory
    {
        protected readonly string CommandText;

        private readonly DbConnection _connection;

        private static int CommandTimeout
        {
            get { return int.Parse(WebConfigurationManager.AppSettings["ExecuteSqlCommandTimeout"]); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText">Sql-запрос</param>
        public SqlCommandFactory(string commandText)
        {
            CommandText = commandText;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public SqlCommandFactory(DbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="connection"></param>
        public SqlCommandFactory(string commandText, DbConnection connection)
        {
            CommandText = commandText;
            _connection = connection;
        }

        /// <summary>
        /// Вернуть команду, заданную в конструкторе
        /// </summary>
        /// <returns></returns>
        public SqlCommand CreateCommand()
        {
            return CreateCommand(CommandText);
        }

        public SqlCommand CreateCommand(IDictionary<string, object> parameters)
        {
            var sqlCommand = GetBaseCommand(CommandText);

            if (parameters != null && parameters.Any())
                foreach (var param in parameters)
                    sqlCommand.Parameters.AddWithValue(param.Key, param.Value);

            return sqlCommand;
        }

        protected SqlCommand CreateCommand(string commandText)
        {
            return GetBaseCommand(commandText);
        }

        protected SqlCommand CreateCommand(string commandText, IDictionary<KeyValuePair<string, SqlDbType>, object> parameters)
        {
            var sqlCommand = GetBaseCommand(commandText);

            if (parameters != null && parameters.Any())
            {
                var counter = 0;
                foreach (var param in parameters.Keys)
                {
                    sqlCommand.Parameters.Add(param.Key, param.Value);
                    sqlCommand.Parameters[counter++].Value = parameters[param];
                }
            }

            return sqlCommand;
        }

        private SqlCommand GetBaseCommand(string commandText)
        {
            SqlCommand command = _connection != null ? (SqlCommand)_connection.CreateCommand() : new SqlCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            return command;
        }
    }
}