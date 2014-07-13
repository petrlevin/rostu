using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Security;
using System.Text;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

[assembly: AllowPartiallyTrustedCallers]
namespace BaseApp.DbClr.Functions
{
	/// <summary>
	/// Получение наименований для всей сущности
	/// </summary>
	public class GetCaptions
	{
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		/// <summary>
		/// Точка входа для CLR функции
		/// </summary>
		/// <param name="idEntity"></param>
		/// <returns></returns>
		[SqlFunction(SystemDataAccess = SystemDataAccessKind.Read, DataAccess = DataAccessKind.Read, FillRowMethodName = "FillRow", TableDefinition = "Id int, IdEntity int, Caption nvarchar(MAX)")]
		public static IEnumerable Get(Int32 idEntity)
		{
			IEntity entity = Objects.ById<Entity>(idEntity);
			IEntityField captionField = entity.CaptionField;
			return GetResult(entity, captionField);
		}

		[SqlFunction(SystemDataAccess = SystemDataAccessKind.Read, DataAccess = DataAccessKind.Read, FillRowMethodName = "FillRow", TableDefinition = "Id int, IdEntity int, Caption nvarchar(MAX)")]
		public static IEnumerable GetByField(Int32 idEntity, Int32 idEntityField)
		{
			IEntity entity = Objects.ById<Entity>(idEntity);
			IEntityField captionField = entity.Fields.Single(a => a.Id == idEntityField);
			return GetResult(entity, captionField);
		}

		private static IEnumerable GetResult(IEntity entity, IEntityField entityField)
		{
			GetSelectByParam getSelect;
			if (entityField.EntityFieldType == EntityFieldType.ReferenceEntity || entityField.EntityFieldType == EntityFieldType.DocumentEntity || entityField.EntityFieldType == EntityFieldType.TablepartEntity || entityField.EntityFieldType == EntityFieldType.ToolEntity)
			{
				List<int> idEntities = getEntitiesGenericLinks(entity, entityField);
				getSelect = new GetSelectByParam(entity, "caption", entityField, idEntities);
			}
			else
			{
				getSelect = new GetSelectByParam(entity, "caption", entityField);
			}
			string textCommand = getSelect.GetResult();
			List<Result> result = _sqlCmd.Select<Result>(textCommand);
			return result;
		}

		/// <summary>
		/// Получения уникальных идентификаторов сущностей из общей ссылки
		/// </summary>
		/// <param name="entity">Сущности</param>
		/// <param name="captionField">Поле Caption сущности</param>
		/// <returns></returns>
		private static List<int> getEntitiesGenericLinks(IEntity entity, IEntityField captionField)
		{
			return 
				_sqlCmd.SelectOneColumn<int>(string.Format("SELECT DISTINCT [{0}Entity] FROM [{1}].[{2}]", captionField.Name, entity.Schema,
				                                  entity.Name));
		}

		/// <summary>
		/// Класс описывающий строку результирующего набора
		/// </summary>
		public class Result
		{
			/// <summary>
			/// Идентификатор элемента сущности
			/// </summary>
			public Int32 Id { get; set; }

			/// <summary>
			/// Идентификатр сущности
			/// </summary>
			public Int32 IdEntity { get; set; }
			
			/// <summary>
			/// Наименование элемента сущности
			/// </summary>
			public string Caption { get; set; }
		}

		/// <summary>
		/// Заполнение строки результата
		/// </summary>
		/// <param name="value">Значение в виде object</param>
		/// <param name="id">Значение в виде SqlInt32</param>
		/// <param name="idEntity"></param>
		/// <param name="caption"></param>
		public static void FillRow(object value, out SqlInt32 id, out SqlInt32 idEntity, out SqlString caption)
		{
			Result result = (Result)value;
			id = result.Id;
			idEntity = result.IdEntity;
			caption = result.Caption;
		}
	}
}
