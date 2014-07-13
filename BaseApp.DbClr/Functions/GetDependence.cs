using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using BaseApp.Common.Interfaces.DbDependecy;
using Microsoft.SqlServer.Server;
using Platform.DbCmd;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;

namespace BaseApp.DbClr.Functions
{
    //todo: Юра, напиши комментарии)
	/// <summary>
	/// Получить зависимости для записи таблицы
	/// </summary>
	public class GetDependence
	{
		private static readonly SqlCmd SqlCmd=new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		private const string TextSqlCommand =
			@"SELECT [a].[Name] as FieldName, [b].[id] as EntityId, [b].[Name] as EntityName, [b].[Caption] as EntityCaption, [c].[id] as EntityIdType, [c].[Caption] as EntityType,
				[d].[Name] as HeadFieldName, [e].[id] as HeadEntityId, [e].[Name] as HeadEntityName, [e].[Caption] as HeadEntityCaption, [f].[id] as HeadEntityIdType, [f].[Caption] as HeadEntityType
			FROM [ref].[EntityField] a
				INNER JOIN [ref].[Entity] b ON [b].[id]=[a].[idEntity]
				INNER JOIN [enm].[EntityType] c ON [c].[id]=[b].[idEntityType]
				LEFT OUTER JOIN [ref].[EntityField] d ON ([b].[idEntityType]={0} AND [d].[idEntity]=[b].[id] AND [d].[Name]='idOwner') OR ([b].[idEntityType]={1} AND [d].[idEntity]=[b].[id] AND [d].[idEntityFieldType]={2} AND ([d].[idCalculatedFieldType] IS NULL OR [d].[idCalculatedFieldType]={4} OR [d].[idCalculatedFieldType]={5}) AND [d].[id]<>[a].[id])
				LEFT OUTER JOIN [ref].[Entity] e ON [e].[id]=[d].[idEntityLink]
				LEFT OUTER JOIN [enm].[EntityType] f ON [f].[id]=[e].[idEntityType]
			WHERE [a].[idEntityLink]={3} AND [a].[idEntityFieldType]={2} AND ([a].[idCalculatedFieldType] IS NULL OR [a].[idCalculatedFieldType]={4} OR [a].[idCalculatedFieldType]={5})
			order by HeadEntityCaption, EntityCaption, FieldName";

		/// <summary>
		/// Точка входя для CLR функции
		/// </summary>
		/// <param name="idItem"></param>
		/// <param name="idEntity"></param>
		/// <returns></returns>
		[SqlFunction(SystemDataAccess = SystemDataAccessKind.Read, DataAccess = DataAccessKind.Read, FillRowMethodName = "FillRow", TableDefinition = "HeadTypeName nvarchar(100), HeadEntityCaption nvarchar(400), HeadId int, HeadCaption nvarchar(MAX), TypeName nvarchar(100), EntityCaption nvarchar(400), Id int, Caption nvarchar(MAX)")]
		public static IEnumerable Get(int idItem, int idEntity)
		{
			List<ResultEntityDependence> resultEntityDependences =
				SqlCmd.Select<ResultEntityDependence>(string.Format(TextSqlCommand, (byte) EntityType.Tablepart,
				                                                     (byte) EntityType.Multilink, (byte) EntityFieldType.Link,
																	 idEntity == -2147483615 ? idItem : idEntity, (byte)CalculatedFieldType.DbComputed, (byte)CalculatedFieldType.DbComputedPersisted));

			StringBuilder textCommand1 = new StringBuilder();
            int currentEntityId = 0;

			foreach (ResultEntityDependence entityDependence in resultEntityDependences)
			{
				if (currentEntityId != 0 && currentEntityId == entityDependence.EntityId)
				{
					textCommand1.AppendFormat(" OR [{0}]={1}", entityDependence.FieldName, idItem);
					continue;
				}


				if (textCommand1.Length != 0)
				{
					textCommand1.AppendLine();
					textCommand1.AppendLine("UNION ALL");
				}
				currentEntityId = entityDependence.EntityId;

				if (entityDependence.HeadEntityId != 0 && entityDependence.EntityIdType == 4)
				{
					textCommand1.AppendFormat(
						"SELECT '{7}' as HeadTypeName, '{8}' as HeadEntityCaption, [a].[{9}] as HeadId, [b].[Caption] as HeadCaption, '{3}' as TypeName, '{4}' as EntityCaption, [a].[id], [c].[Caption] as Caption FROM [{1}].[{2}] a INNER JOIN dbo.GetCaptions({10}) b ON [b].[Id]=[a].[{9}] INNER JOIN dbo.GetCaptions({0}) c ON [c].[Id]=[a].[id] WHERE [a].[{5}]={6}",
						entityDependence.EntityId, Schemas.ByEntityType(entityDependence.EntityIdType), entityDependence.EntityName,
						entityDependence.EntityType, entityDependence.EntityCaption, entityDependence.FieldName, idItem,
						entityDependence.HeadEntityType, entityDependence.HeadEntityCaption, entityDependence.HeadFieldName,
						entityDependence.HeadEntityId);
				}
				else
				{
					if (entityDependence.EntityIdType == 5)
					{
						textCommand1.AppendFormat(
							"SELECT '' as HeadTypeName, '' as HeadEntityCaption, 0 as HeadId, '' as HeadCaption, '{3}' as TypeName, '{4}' as EntityCaption, [a].[{7}] as [id], [b].[Caption] as Caption FROM [{1}].[{2}] a INNER JOIN dbo.GetCaptions({0}) b ON [b].[Id]=[a].[{7}] WHERE [a].[{5}]={6}",
							entityDependence.HeadEntityId, Schemas.ByEntityType(entityDependence.EntityIdType), entityDependence.EntityName,
							entityDependence.HeadEntityType, entityDependence.HeadEntityCaption, entityDependence.FieldName, idItem,
							entityDependence.HeadFieldName);
					}
					else
					{
						textCommand1.AppendFormat(
							"SELECT '' as HeadTypeName, '' as HeadEntityCaption, 0 as HeadId, '' as HeadCaption, '{3}' as TypeName, '{4}' as EntityCaption, [a].[id], [b].[Caption] as Caption FROM [{1}].[{2}] a INNER JOIN dbo.GetCaptions({0}) b ON [b].[Id]=[a].[id] WHERE [a].[{5}]={6}",
							entityDependence.EntityId, Schemas.ByEntityType(entityDependence.EntityIdType), entityDependence.EntityName,
							entityDependence.EntityType, entityDependence.EntityCaption, entityDependence.FieldName, idItem);
					}
				}
			}
			textCommand1.AppendLine();
			List<ResultDependence> result = SqlCmd.Select<ResultDependence>(textCommand1.ToString());
			return result;
		}

		/// <summary>
		/// Зависимость элементов в базе
		/// </summary>
		public class ResultEntityDependence
		{
			/// <summary>
			/// Системное имя поля
			/// </summary>
			public string FieldName { get; set; }
		
            /// <summary>
			/// Идентификатор сущности
			/// </summary>
			public int EntityId { get; set; }
			
            /// <summary>
            /// Системное имя сущности
            /// </summary>
            public string EntityName { get; set; }
			
            /// <summary>
            /// Наименование сущности
            /// </summary>
            public string EntityCaption { get; set; }
			
            /// <summary>
            /// Идентификатор типа сущности
            /// </summary>
            public byte EntityIdType { get; set; }
			
            /// <summary>
            /// Системное имя типа сущности
            /// </summary>
            public string EntityType { get; set; }
			
            public string HeadFieldName { get; set; }
			
            public int HeadEntityId { get; set; }
			
            public string HeadEntityName { get; set; }
			
            public string HeadEntityCaption { get; set; }
			
            public byte HeadEntityIdType { get; set; }
			
            public string HeadEntityType { get; set; }
		}

		/// <summary>
		/// Класс описывающий строку результирующего набора
		/// </summary>
        public class ResultDependence : IResultDbDependence
		{
			public string HeadTypeName { get; set; }
			public string HeadEntityCaption { get; set; }
			public Int32 HeadId { get; set; }
			public string HeadCaption { get; set; }

			public string TypeName { get; set; }
			public string EntityCaption { get; set; }
			public Int32 Id { get; set; }
			public string Caption { get; set; }
		}

	    /// <summary>
	    /// Заполнение строки результата
	    /// </summary>
	    /// <param name="value">Значение в виде object</param>
	    /// <param name="entityCaption"></param>
	    /// <param name="id">Значение в виде SqlInt32</param>
	    /// <param name="headTypeName"></param>
	    /// <param name="headEntityCaption"></param>
	    /// <param name="headId"></param>
	    /// <param name="headCaption"></param>
	    /// <param name="typeName"></param>
	    /// <param name="caption"></param>
	    public static void FillRow(object value, out SqlString headTypeName, out SqlString headEntityCaption, out SqlInt32 headId, out SqlString headCaption, out SqlString typeName, out SqlString entityCaption, out SqlInt32 id, out SqlString caption)
		{
			ResultDependence result = (ResultDependence) value;
			headTypeName = result.HeadTypeName;
			headEntityCaption = result.HeadEntityCaption;
			headId = result.HeadId;
			headCaption = result.HeadCaption;
			typeName = result.TypeName;
			entityCaption = result.EntityCaption;
			id = result.Id;
			caption = result.Caption;
		}


	}
}