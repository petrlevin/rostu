using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.Dal;

namespace BaseApp.Audit
{
    /// <summary>
    /// Логирование изменений в мультилинках
    /// </summary>
    public class MultilinkSqlLogData : ISqlLogData
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Идентификатор элемента
        /// </summary>
        public int FirstId { get; set; }
        
        /// <summary>
        /// Идентификатор элемента на который идет сслыка
        /// </summary>
        public int SecondId { get; set; }
        
        /// <summary>
        /// Операция
        /// </summary>
        public MultilinkOperations Operation { get; set; }


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
                        INSERT INTO [dbo].[multilink_data] (
                        [EntityId],
                        [Operation],
                        [FirstId],
                        [SecondId] ,
                        [IdUser] ,
                        [Date])
                    VALUES
                        (
                        @EntityId,
                        @Operation ,
                        @FirstId ,
                        @SecondId ,
                        @IdUser ,
                        @Date)";

            comm.Parameters.Add("@EntityId", SqlDbType.Int);
            comm.Parameters.Add("@Operation", SqlDbType.TinyInt);
            comm.Parameters.Add("@FirstId", SqlDbType.Int);
            comm.Parameters.Add("@SecondId", SqlDbType.Int);
            comm.Parameters.Add("@IdUser", SqlDbType.Int);
            comm.Parameters.Add("@Date", SqlDbType.DateTime);

            comm.Parameters["@EntityId"].Value = EntityId;
            comm.Parameters["@Operation"].Value = (Int16) Operation;
            comm.Parameters["@FirstId"].Value = FirstId;
            comm.Parameters["@SecondId"].Value = SecondId;
            comm.Parameters["@IdUser"].Value = userId;
            comm.Parameters["@Date"].Value = DateTime.Now;
            return comm;


        }

        /// <summary>
        /// Возвращает строку, которая представляет текущий объект.
        /// </summary>
        /// <returns>
        /// Строка, представляющая текущий объект.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }
}
