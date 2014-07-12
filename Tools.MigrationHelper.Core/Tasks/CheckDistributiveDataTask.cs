using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAnt.Core.Attributes;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;

using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;

namespace Tools.MigrationHelper.Core.Tasks
{

    [TaskName("checkdistributivedata")]
    public class CheckDistributiveDataTask : MhTask
    {
        [TaskAttribute("connectionstring", Required = true)]
        public string ConnectionString { get; set; }


        

        protected override void ExecuteTask()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var query = BuildQuery(connection);
                using (var comm = connection.CreateCommand())
                {

                    comm.CommandText = query;
                    comm.CommandType = CommandType.Text;
                    using (var r = comm.ExecuteReader())
                    {
                        if (!r.HasRows)
                            return;
                        var errors = r.AsEnumerable(
                            dr => new ErrorInfo()
                                      {
                                          Id = dr.GetInt32("id"),
                                          Caption = dr.GetString("Caption"),
                                          EntityName = dr.GetString("entityName"),
                                          IdElementEntity = dr.GetInt32("idElementEntity"),
                                          IdElement = dr.GetInt32("idElement"),
                                          FieldName = dr.GetString("fieldName"),
                                          FieldValue = dr.GetInt32("fieldValue"),
                                          FieldEntity = dr.GetInt32("fieldEntity"),
                                          FieldEntityName = dr.GetString("fieldEntityName")
                                      });
                        var error = new StringBuilder();
                        error.AppendLine(
                            "Эталонные данные не консистенты. Ссылочные поля эталонных элементов не могут ссылаться на тестовые элементы.");
                        error.AppendLine();

                        error.AppendLine(
                            "------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        error.AppendLine();
                        error.AppendFormat("{0,-12}|{1,-16}|{2,-12}|{3,-23}|{4,-23}|{5,-23}|{6,-12}|{7,-12}|{8,-20}", "Id", "IdElementEntity",
                                           "IdElement", "EntityName", "Caption", "FieldName", "FieldValue",
                                           "FieldEntity", "FieldEntityName");
                        error.AppendLine(
                            "------------------------------------------------------------------------------------------------------------------------------------------------------------");

                        foreach (ErrorInfo errorInfo in errors)
                        {
                        error.AppendFormat("{0,-12}|{1,-16}|{2,-12}|{3,-23}|{4,-23}|{5,-23}|{6,-12}|{7,-12}|{8,-20}", errorInfo.Id.ToString(), errorInfo.IdElementEntity.ToString(),
                                           errorInfo.IdElement.ToString(), errorInfo.EntityName, errorInfo.Caption, errorInfo.FieldName, errorInfo.FieldValue.ToString(),
                                           errorInfo.FieldEntity.ToString(), errorInfo.FieldEntityName);
                        error.AppendLine();
                            
                        }

                        error.AppendLine(
                            "------------------------------------------------------------------------------------------------------------------------------------------------------------");

                        error.AppendLine();
                        error.AppendLine();
                        error.AppendLine(
                            " 1) Id - Идентификатор в таблице 'DistributivaData' ");
                        error.AppendLine(
                            " 2) IdElementEntity - Идентификатор сущности в таблице 'DistributivaData' ");
                        error.AppendLine(
                            " 3) IdElement - Идентификатор элемента в таблице 'DistributivaData' ");
                        error.AppendLine(
                            " 4) EntityName - Наименование сущности для которой нарушена консистентность (соответствует IdElementEntity) ");
                        error.AppendLine(
                            " 5) Caption - Русское наименование элемента для которого нарушена консистентность , если соответсвующее поле определено или идентификатор (соответствует IdElement) ");
                        error.AppendLine(
                            " 6) FieldName - Наименование поля сущности, для которого нарушена консистентность  ");
                        error.AppendLine(
                            " 7) FieldValue - Значение поля  (идентификатор )");
                        error.AppendLine(
                            " 8) FieldEntity - Идентификатор сущности поля , для которого нарушена консистентность  ");
                        error.AppendLine(
                            " 9) FieldEntityName - Наименование сущности поля , для которого нарушена консистентность  ");

                        error.AppendLine();
                        error.AppendLine();
                        error.AppendLine();

                        Error(error.ToString());
                    }



                }
            }

        }

        struct ErrorInfo
        {
            public Int32 Id { get; set; }
            public String EntityName { get; set; }
            public String Caption { get; set; }
            public Int32 IdElementEntity { get; set; }
            public Int32 IdElement { get; set; }
            public String FieldName { get; set; }
            public Int32 FieldValue { get; set; }
            public String FieldNameCaption { get; set; }
            public Int32 FieldEntity { get; set; }
            public String FieldEntityName { get; set; }
            public String FieldEntityCaption { get; set; }
        }

        private string BuildQuery(SqlConnection connection)
        {
            //throw new NotImplementedException();
            var selectEntity = "SELECT DISTINCT [idElementEntity] FROM [ref].[DistributiveData]";
            using (var comm = connection.CreateCommand())
            {

                comm.CommandText = selectEntity;
                comm.CommandType = CommandType.Text;
                using (var reader = comm.ExecuteReader())
                {
                    var word = "";
                    var sql = new StringBuilder();
                    var types = new[]
				        {
					        EntityType.Reference,
					        EntityType.Tablepart,
					        EntityType.Tool,
				        	EntityType.Document
				        };

                    var entityEntityId = Objects.ByName<Entity>("Entity").Id;
                    foreach (int entityId in reader.AsEnumerable<Int32>(r => r.GetInt32(0)))
                    {

                        var entity = Objects.ById<Entity>(entityId);



                        foreach (IEntityField entityField in entity.RealFields.Where(f => f.EntityFieldType == EntityFieldType.Link).Where(f => types.Contains(f.EntityLink.EntityType)))
                        {
                            sql.AppendLine();
                            sql.Append(word);
                            sql.AppendFormat("SELECT [{0}].[id] as id, {1} as entityId ,'{2}' as fieldName , [{0}].[{2}] as fieldValue ,[dbo].[GetCaption]({3},[{0}].[{2}] ) as fieldValueCaption ,  {3} as fieldEntity, [dbo].[GetCaption]({4},{3}) as fieldEntityCaption  FROM [{5}].[{0}] as [{0}] ", entity.Name, entityId, entityField.Name, entityField.EntityLink.Id, entityEntityId, entity.Schema);
                            sql.AppendFormat(
                                @"
                                    WHERE ([{0}].[{1}] IS NOT NULL) " + ((entityField.EntityLink.Id == entityEntityId) ? @" AND ([{0}].[{1}] IN (SELECT [id] FROM [ref].[Entity] WHERE [idEntityType]=3 OR [idEntityType]=4 OR [idEntityType]=6 OR [idEntityType]=7))  " : "") + " AND ([{0}].[{1}] NOT IN (SELECT [idElement] FROM [ref].[DistributiveData] WHERE [idElement]= [{0}].[{1}] AND [idElementEntity]={2} ))",
                                entity.Name, entityField.Name, entityField.EntityLink.Id);

                            word = " UNION ";
                        }

                        sql.AppendLine("--Общие ссылки");

                        foreach (IEntityField entityField in entity.RealFields.Where(f => f.IsCommonReference()))
                        {
                            sql.AppendLine();
                            sql.Append(word);
                            sql.AppendFormat("SELECT [{0}].[id] as id, {1} as entityId ,'{2}' as fieldName ,[{0}].[{2}] as fieldValue ,[dbo].[GetCaption]([{0}].[{2}Entity],[{0}].[{2}] ) as fieldValueCaption , [{0}].[{2}Entity] as fieldEntity, [dbo].[GetCaption]({3},[{0}].[{2}Entity]) as fieldEntityCaption  FROM [{4}].[{0}] as [{0}] ", entity.Name, entityId, entityField.Name, entityEntityId, entity.Schema);
                            sql.AppendFormat(
                                @"
                                    WHERE ([{0}].[{1}] IS NOT NULL) AND  ([{0}].[{2}]!={3} OR [{0}].[{1}] IN (SELECT [id] FROM [ref].[Entity] WHERE [idEntityType]=3 OR [idEntityType]=4 OR [idEntityType]=6 OR [idEntityType]=7)) AND ([{0}].[{2}] IN (SELECT [id] FROM [ref].[Entity] WHERE [idEntityType]=3 OR [idEntityType]=4 OR [idEntityType]=6 OR [idEntityType]=7)) AND ([{0}].[{1}] NOT IN (SELECT [idElement] FROM [ref].[DistributiveData] WHERE [idElement]= [{0}].[{1}] AND [idElementEntity]=[{0}].[{2}]  ))",
                                entity.Name, entityField.Name, entityField.Name + "Entity" ,entityEntityId);

                            word = " UNION ";
                        }
                        sql.AppendLine("--Общие ссылки КОНЕЦ");


                    }

                    return
                        @"SELECT [dd].[id] , [e].Name as [entityName]  ,  [dbo].[GetCaption]([dd].[idElementEntity] , [dd].[idElement]) as [Caption] , [dd].[idElementEntity] , [dd].[idElement] ,[b].[fieldName] ,[b].[fieldValue] ,[b].[fieldValueCaption] , [b].[fieldEntity] , [e2].Name as [fieldEntityName], [b].[fieldEntityCaption]  FROM [ref].[DistributiveData] dd INNER JOIN 
                            (" +
                        sql.ToString() + @")  b ON
                            [dd].[idElementEntity] =[b].[entityId] AND [dd].[idElement] = [b].[id] JOIN [ref].[Entity] e ON ([e].[id]=[dd].[idElementEntity]) JOIN [ref].[Entity] e2 ON ([e2].[id]=[b].[fieldEntity])";

                }




            }
        }
    }
}
