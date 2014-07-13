using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities;

namespace BaseApp.DbClr.Procedures
{
    /// <summary>
    /// Класс реализующий каскадное удаление для общих ссылок
    /// </summary>
    public class CascadeDeleteForGenericLinks
    {
        #region Private Property
        /// <summary>
        /// Экземпляр SqlCmd для выполненения команд
        /// </summary>

        #endregion

        private static readonly SqlCmd _sqlCmd = new SqlCmd(new SqlConnection("context connection = true"), ConnectionType.ConnectionPerCommand);

        #region Private Methods
        private static void _cascadeDelete(int idEntity, string listId)
        {
            try
            {
                var sql =
                    _sqlCmd.ExecuteScalar<String>(
                        @"SELECT dbo.Concatenate([outer].sql ,';') FROM (SELECT  'DELETE FROM ['+dbo.getSchema(e.idEntityType)+'].['+e.[Name]+'] WHERE [id] IN ('+r.[deleteIds]+') ' as sql FROM  [ref].[Entity] e INNER JOIN (SELECT[a].[idReferencesEntity],dbo.Concatenate([a].[idReferences] ,',') as deleteIds FROM [dbo].[GenericLinks] a INNER JOIN [ref].[EntityField] c ON [c].[idEntity]=[a].[idReferencesEntity] AND [c].[id]=[a].[idReferencesEntityField]  WHERE  [c].[idForeignKeyType]=3 AND [a].[idReferencedEntity]=" +
                        idEntity.ToString() + " AND [a].[idReferenced] IN (" + listId +
                        ") GROUP BY [a].[idReferencesEntity] ) r ON (e.[id] =[r].[idReferencesEntity])) [outer]");
                var tmpl = @"declare @str nvarchar(max);
                                        SET @str = '{0}'
                                        EXECUTE sp_executesql @str ";

                _sqlCmd.ExecuteNonQuery(String.Format(tmpl, sql));

            }
            catch (Exception ex)
            {
                SqlContext.Pipe.Send(ex.Message);
                throw;
            }
        }
        #endregion

        /// <summary>
        /// Метод каскадного удаления
        /// </summary>
        /// <param name="idEntity"></param>
        /// <param name="listId"></param>
        [SqlProcedure]
        public static void Exec(int idEntity, string listId)
        {
            _cascadeDelete(idEntity, listId);
        }
    }
}