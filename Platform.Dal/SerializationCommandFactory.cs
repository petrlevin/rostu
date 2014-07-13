using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal
{
    /// <summary>
    /// Фабрика Sql-команд для сериализивания документов/инструментов
    /// </summary>
    public class SerializationCommandFactory : SqlCommandFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText">Sql-запрос</param>
        public SerializationCommandFactory(string commandText) : base(commandText){}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  SqlCommand CreateCommand(int id)
        {
            return CreateCommand(id, CommandText);
        }

        protected SqlCommand CreateCommand(int id , string commandText)
        {
            var parameters = new Dictionary<KeyValuePair<string, SqlDbType>, object>
                {
                    { new KeyValuePair<string, SqlDbType>(GetThisParameterName(), SqlDbType.Int), id }
                };

            return CreateCommand(commandText, parameters);
        }

        /// <summary>
        /// Получить имя параметра -- Id документа
        /// </summary>
        /// <returns></returns>
        public static string GetThisParameterName()
        {
            return "@docId";
        }

        /// <summary>
        /// Получить имя параметра -- Id документа
        /// </summary>
        /// <returns></returns>
        public static Literal GetThisParameter()
        {
            return GetThisParameterName().ToLiteral(LiteralType.Variable);
        }


    }
}