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
    /// Получение последней версии элемента из общей ссылки
    /// </summary>
    public class GetLastVersion
    {	/// <summary>
        /// Экземпляр SqlCmd для выполненения команд
        /// </summary>
        private static readonly SqlCmd SqlCmd = new SqlCmd(new SqlConnection("context connection = true"), ConnectionType.ConnectionPerCommand);

        /// <summary>
        /// Получение id последней версии по общей ссылке
        /// </summary>
        /// <param name="idEntity">Идентифкатор сущности</param>
        /// <param name="idItem">Идентификатор элемента</param>
        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static int? Get(Int32 idEntity, Int32 idItem)
        {
            IEntity entity = Objects.ById<Entity>(idEntity);
            
            var textCommand = String.Format(
                "WITH DocumentTree(id, idParent) AS " +
                "(SELECT id, idParent AS Document FROM [{0}].[{1}] WHERE id = {2} UNION ALL " +
                "SELECT d.id, d.idParent FROM [{0}].[{1}] d " +
                "INNER JOIN DocumentTree t ON d.idParent = t.id ) SELECT id FROM DocumentTree DT " +
                "WHERE NOT EXISTS (SELECT NULL FROM DocumentTree WHERE idParent = DT.id )", entity.Schema, entity.Name, idItem);

            return SqlCmd.ExecuteScalar<Int32?>(textCommand);
        }
        
    }
}
