using System;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Functions
{
    /// <summary>
    /// Преобразователь мультилинков в строки
    /// </summary>
    public class MultilinkAsString
    {

        private static readonly SqlCmd SqlCmd = new SqlCmd(new SqlConnection("context connection = true"), ConnectionType.ConnectionPerCommand);

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static String Get(Int32 idMultilinkEntity, Int32 idOwnerEntity , Int32 idOwner)
        {
            var multilinkEntity = Objects.ById<Entity>(idMultilinkEntity);

            var textCommand = String.Format(
                "SELECT  dbo.Concatenate(dbo.GetCaption( {0},t.[{1}]), ', ')  FROM [ml].[{2}] t WHERE  t.[{3}] = {4}",
                MultilinkHelper.GetRightMultilinkEntity(multilinkEntity, idOwnerEntity).Id,
                MultilinkHelper.GetRightMultilinkField(multilinkEntity,idOwnerEntity).Name,
                multilinkEntity.Name,
                MultilinkHelper.GetLeftMultilinkField(multilinkEntity, idOwnerEntity).Name,
                idOwner
                );

            return SqlCmd.ExecuteScalar<string>(textCommand);
        }

    }
}
