using System;
using System.Collections;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Functions
{
    /// <summary>
    /// Класс реализующий получение идентификаторов родителей и расстояния до родителя для указанного элемента при построении иерархии по указанному полю
    /// </summary>
    public class GetParents
    {
        /// <summary>
        /// Получение идентификаторов предков и растояния до предков для указанного элемента
        /// </summary>
        /// <param name="idItem">Элемент, для которого получаются предки</param>
        /// <param name="idParentField">id поля по которому строится иерархия, если NULL, поле определяется по entityName и parentFieldName </param>
        /// <param name="entityName">имя сущности, импользуется только при idParentField = NULL</param>
        /// <param name="parentFieldName">имя поля, по которому строится иерархия, импользуется только при idParentField = NULL</param>
        /// <param name="includeThis">Включать в результат сам элемент да/нет</param>
        /// <returns></returns>
        [SqlFunction(DataAccess = DataAccessKind.Read, FillRowMethodName = "FillRow", TableDefinition = "id int, distance int")]
        public static IEnumerable Get(int? idItem, int? idParentField=null, string entityName=null, string parentFieldName=null, bool includeThis=false)
        {

            EntityField parentEntityField;
            Entity entity;
            ArrayList result = new ArrayList();

            if (idItem != null)
            {
                if (idParentField != null)
                {
                    try
                    {
                        parentEntityField = Objects.ById<EntityField>(idParentField ?? 0);
                    }
                    catch
                    {
                        SqlContext.Pipe.Send("Ошибка при получении поля сущности по идентифкатору '" + idParentField + "'");
                        throw;
                    }
                    parentFieldName = parentEntityField.Name;
                    entity = parentEntityField.Entity;
                }
                else
                {
                    try
                    {
                        entity = Objects.ByName<Entity>(entityName);
                    }

                    catch
                    {
                        SqlContext.Pipe.Send("Ошибка при получении поля сущности по имени '" + entityName + "'");
                        throw;
                    }
                }


                using (SqlConnection connection = new SqlConnection("context connection=true"))
                {
                    connection.Open();
                    string textCommand = string.Format(
                        "WITH allParents ([id], [idParent], [distance]) AS " +
                        "(SELECT [id], [{2}], {4} as dist " +
                        "FROM [{0}].[{1}] " +
                        "WHERE [id]={3} " +
                        "UNION ALL " +
                        "SELECT [b].[id], [b].[{2}], [a].[distance]+1 as dist " +
                        "FROM [allParents] a " +
                        "INNER JOIN [{0}].[{1}] b ON [a].[{2}]=[b].[id]) " +
                        "SELECT [id], [distance] FROM [allParents]",
                        entity.Schema, entity.Name, parentFieldName,
                        includeThis
                            ? idItem.ToString()
                            : string.Format("(select [{0}] from [{1}].[{2}] p where p.[id]={3})", parentFieldName, entity.Schema, entity.Name, idItem),
                        includeThis ? "0" : "1");

                    using (SqlCommand command = new SqlCommand(textCommand, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new Result(reader.GetInt32(0), reader.GetInt32(1)));
                            }
                        }
                    }
                    connection.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Класс описывающий строку результирующего набора
        /// </summary>
        private class Result
        {
            public SqlInt32 Id;
            public SqlInt32 Distance;

            public Result(SqlInt32 id, SqlInt32 distance)
            {
                Id = id;
                Distance = distance;
            }
        }

        /// <summary>
        /// Заполнение строки результата
        /// </summary>
        /// <param name="value">Значение в виде object</param>
        /// <param name="id">Значение в виде SqlInt32</param>
        public static void FillRow(object value, out SqlInt32 id, out SqlInt32 distance)
        {
            Result result = (Result)value;
            id = result.Id;
            distance = result.Distance;
        }
    }
}
