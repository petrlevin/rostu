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
    /// Логгирование данных по действиям над сущностями (операции/контроли)
    /// </summary>
    public class OperationSqlLogData : ISqlLogData
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Идентификатор элемента
        /// </summary>
        public int ElementId { get; set; }

        /// <summary>
        /// Идентификатор транзакции
        /// </summary>
        public int TransactionScope { get; set; }

        /// <summary>
        /// Тип операции
        /// </summary>
        public ProcessOperationTypes OperationType { get; set; }
        
        /// <summary>
        /// Имя операции
        /// </summary>
        public string OperationName { get; set; }
        
        /// <summary>
        /// Идентификатор операции
        /// </summary>
        public int? OperationId { get; set; }

        /// <summary>
        /// Миллисекунд
        /// </summary>
        public int? OperationTime { get; set; }


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
                        INSERT INTO [dbo].[operation_data]
                        (
                        [EntityId],
                        [ElementId],
                        [TransactionScope],
                        [OperationType],
                        [OperationName],
                        " + (OperationTime.HasValue ? "[OperationTime]," : "") + @"
                        " + (OperationId.HasValue ? "[OperationId]," : "") + @"
                        [UserId],
                        [Date])
                    VALUES
                        (
                        @EntityId,
                        @ElementId,
                        @TransactionScope, 
                        @OperationType,
                        @OperationName,
                        " + ( OperationTime.HasValue ? "@OperationTime, " : "") + @"
                        " + ( OperationId.HasValue ? "@OperationId, " : "") + @"
                        @UserId,
                        @Date)";

            comm.Parameters.Add("@EntityId", SqlDbType.Int);
            comm.Parameters.Add("@ElementId", SqlDbType.Int);
            comm.Parameters.Add("@TransactionScope", SqlDbType.Int);
            
            comm.Parameters.Add("@OperationType", SqlDbType.TinyInt);
            comm.Parameters.Add("@OperationName", SqlDbType.VarChar);
            
            if (OperationTime.HasValue)
                comm.Parameters.Add("@OperationTime", SqlDbType.Int);
            
            if (OperationId.HasValue)
                comm.Parameters.Add("@OperationId", SqlDbType.Int);
            
            comm.Parameters.Add("@UserId", SqlDbType.Int);
            comm.Parameters.Add("@Date", SqlDbType.DateTime);

            comm.Parameters["@EntityId"].Value = EntityId;
            comm.Parameters["@ElementId"].Value = ElementId;
            comm.Parameters["@TransactionScope"].Value = TransactionScope;
            
            comm.Parameters["@OperationType"].Value = OperationType;
            comm.Parameters["@OperationName"].Value = OperationName ?? string.Empty;
            
            if (OperationTime.HasValue)
                comm.Parameters["@OperationTime"].Value = OperationTime.Value;
            
            if (OperationId.HasValue)
                comm.Parameters["@OperationId"].Value = OperationId;

            comm.Parameters["@UserId"].Value = userId;
            comm.Parameters["@Date"].Value = DateTime.Now;
            return comm;
        }
    }
}
