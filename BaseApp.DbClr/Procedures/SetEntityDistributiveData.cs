using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Transactions;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Procedures
{
    public class SetEntityDistributiveData
    {
        [SqlProcedure]
        public static void Exec(int idEntity)
        {
            using (var ts = new TransactionScope())
            {
                using (var conn = new SqlConnection("context connection = true"  ))
                {
                    using (Objects.UseConnection(conn))
                    {
                        conn.Open();
                        var entities = new List<Int32>();
                        DoExec(idEntity, entities, conn);
                        ts.Complete();
                    }
                }
                
            }
        }

        private static void DoExec(int idEntity, List<int> entities, SqlConnection conn)
        {
            if (entities.Contains(idEntity))
                return;
            entities.Add(idEntity);
            var sql = String.Format(@"
                        declare @entityEntityId int
	                    declare @entityEntityFieldId int
	                    SET @entityEntityId = (SELECT id FROM [ref].[Entity] WHERE [name] = 'Entity')
	                    SET @entityEntityFieldId = (SELECT id FROM [ref].[Entity] WHERE [name] = 'EntityField')

	                    INSERT INTO [ref].[DistributiveData]
                        (
                            [idElement]
                            ,[idElementEntity]
                        ) 
                        SELECT 
      	                {0}
		                ,@entityEntityId 
                        WHERE (0=(SELECT COUNT(1) FROM [ref].[DistributiveData] dd WHERE dd.idElementEntity=@entityEntityId AND dd.idElement = {0} ))

                        INSERT INTO [ref].[DistributiveData]
                        (
                            [idElement]
                            ,[idElementEntity]
                        ) 
                        SELECT 
      	                [ef].[id]
		                ,@entityEntityFieldId FROM [ref].[EntityField] ef
                        WHERE [ef].[idEntity] = {0} AND 
                        (0=(SELECT COUNT(1) FROM [ref].[DistributiveData] dd WHERE dd.idElementEntity=@entityEntityFieldId AND dd.idElement = [ef].[id] ))

                    ", idEntity);
            
            var comm = conn.CreateCommand();
            comm.CommandText = sql;
            comm.ExecuteNonQuery();
            foreach (IEntityField entityField in Objects.ById<Entity>(idEntity).Fields.Where(ef => ef.IdEntityLink != null))
            {
                DoExec(entityField.IdEntityLink.Value,entities,conn);
            }

        }
    }
}
