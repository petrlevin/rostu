using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Functions
{
    /// <summary>
    /// Класс реализующий получение идентификаторов всех версий документа
    /// </summary>
    public class GetDocumentVersionIds
    {
        [SqlFunction(DataAccess = DataAccessKind.Read, FillRowMethodName = "FillRow", TableDefinition = "id int")]
        public static IEnumerable Get(int idItem, int idEntity)
        {
            Entity docEntity;
            try
            {
                docEntity = Objects.ById<Entity>(idEntity);
            }
            catch
            {
                SqlContext.Pipe.Send("Ошибка при получении сущности по идентифкатору '" + idEntity + "'");
                throw;
            }

            if (docEntity.EntityType != EntityType.Document)
            {
                var msg = "Сущность " + docEntity.Name + " не является документом!";
                SqlContext.Pipe.Send(msg);
                throw new Exception(msg);
            }
            

            ArrayList result = new ArrayList();
            using (SqlConnection connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                string textCommand = string.Format(
                    @"With cteParents As 
                    ( 
                      Select [id], [idParent] 
	                    From [{0}].[{1}] 
	                    Where [id] = {2} 
                      Union All 
	                    Select [S].[id], [S].[idParent] 
		                    From [{0}].[{1}] S 
		                    Inner Join cteParents On ([cteParents].[idParent] = [S].[id]) 
                    ), cteChilds as 
                    ( 
                      Select [id], [idParent]  
	                    From [{0}].[{1}] 
	                    Where [idParent] = {2} 
                      Union All 
	                    Select [S].[id], [S].[idParent]  
		                    from [{0}].[{1}] S 
		                    inner join cteChilds on ([cteChilds].[id] = [S].[idParent]) 
                    ) 
                    Select [id] From [cteParents] 
                    Union All 
                        Select [id] From [cteChilds]",
                    docEntity.Schema, docEntity.Name, idItem);

                using (SqlCommand command = new SqlCommand(textCommand, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Result(reader.GetInt32(0)));
                        }
                    }
                }
                connection.Close();
            }
            return result;
        }
        
        /// <summary>
        /// Класс описывающий строку результирующего набора
        /// </summary>
        private class Result
        {
            public SqlInt32 Id;

            public Result(SqlInt32 id)
            {
                Id = id;
            }
        }

        /// <summary>
        /// Заполнение строки результата
        /// </summary>
        /// <param name="value">Значение в виде object</param>
        /// <param name="id">Значение в виде SqlInt32</param>
        public static void FillRow(object value, out SqlInt32 id)
        {
            Result result = (Result)value;
            id = result.Id;
        }
    
    }
}
