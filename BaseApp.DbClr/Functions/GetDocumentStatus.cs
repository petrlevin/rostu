using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Functions
{
    /// <summary>
    /// Получение статуса документа из общей ссылки
    /// </summary>
    public class GetDocumentStatus
    {	/// <summary>
        /// Экземпляр SqlCmd для выполненения команд
        /// </summary>
        private static readonly SqlCmd SqlCmd = new SqlCmd(new SqlConnection("context connection = true"), ConnectionType.ConnectionPerCommand);

        /// <summary>
        /// Получение id статуса документа из общей ссылки
        /// </summary>
        /// <param name="idEntity">Идентифкатор сущности</param>
        /// <param name="idItem">Идентификатор элемента</param>
        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static int Get(Int32 idEntity, Int32 idItem)
        {
            IEntity entity = Objects.ById<Entity>(idEntity);

            var textCommand = String.Format(
                "SELECT idDocStatus FROM [{0}].[{1}] WHERE id = {2}", entity.Schema, entity.Name, idItem);

            return SqlCmd.ExecuteScalar<Int32>(textCommand);
        }

    }
}
