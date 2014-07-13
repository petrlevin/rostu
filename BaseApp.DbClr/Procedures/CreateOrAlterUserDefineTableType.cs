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

namespace BaseApp.DbClr.Procedures
{
	/// <summary>
	/// Созадние или пересоздание пользовательского табличного типа (User-Defined Table Types) для сущности или формы
	/// </summary>
	public class CreateOrAlterUserDefineTableType
	{
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(new SqlConnection("context connection = true"), ConnectionType.ConnectionPerCommand);

		/// <summary>
		/// Созадние или пересоздание пользовательского табличного типа (User-Defined Table Types) для сущности
		/// </summary>
		/// <param name="idEntity"></param>
		[SqlProcedure]
		public static void ExecByIdEntity(int idEntity)
		{
			try
			{
				Entity entity = Objects.ById<Entity>(idEntity);
				if (entity.EntityType==EntityType.Enum)
					return;
				_dropUdtt(entity);
				_createUdtt(entity);
			}
			catch
			{
				return;
			}
		}

		/// <summary>
		/// Созадние или пересоздание пользовательского табличного типа (User-Defined Table Types) для сущности
		/// </summary>
		[SqlProcedure]
		public static void ExecByTableName(string tableName)
		{
			if (string.IsNullOrEmpty(tableName))
				return;

			string name = tableName.Contains(".") ? tableName.Split(new [] {'.'})[1] : tableName;
			int? idEntity = _sqlCmd.ExecuteScalar<int?>(string.Format("SELECT id FROM ref.Entity WHERE Name='{0}'", name));
			if (!idEntity.HasValue)
				return;
			try
			{
				Entity entity = Objects.ById<Entity>(idEntity.Value);
				if (entity.EntityType == EntityType.Enum)
					return;
				_dropUdtt(entity);
				_createUdtt(entity);
			}
			catch
			{
				return;
			}
		}

		/// <summary>
		/// Возвращает команду удаления UserDefinedTableType для сущности
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <returns>string</returns>
		private static void _dropUdtt(Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(
				String.Format(
					"IF EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'{0}' AND ss.name = N'gen') DROP TYPE [gen].[{0}];", entity.Name));
		}

		/// <summary>
		/// Возвращает команду создания табличного типа (UDTT - User Defined Table Type).
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <remarks>
		/// UDTT создается для таблиц всех сущностей и позволяет осуществлять пакетную вставку записей.
		/// </remarks>
		/// <returns></returns>
		private static void _createUdtt(Entity entity)
		{
			StringBuilder result = new StringBuilder();
			IEnumerable<IEntityField> fields =
				 entity.Fields.Where(
					a =>
					a.EntityFieldType != EntityFieldType.VirtualTablePart && a.EntityFieldType != EntityFieldType.Multilink &&
					a.EntityFieldType != EntityFieldType.DataEndpoint &&
					a.EntityFieldType != EntityFieldType.Tablepart && !a.IdCalculatedFieldType.HasValue);

			List<string> fieldsName =
				_sqlCmd.SelectOneColumn<string>(
					"select name from sys.columns where object_id=OBJECT_ID(N'" + entity.Schema + "." + entity.Name + "') and is_computed=0 and is_identity=0 order by column_id");
			result.AppendFormat("CREATE TYPE [gen].[{0}] AS TABLE(", entity.Name);
			bool isFirst = true;
			foreach (string fieldName in fieldsName)
			{
				IEntityField entityField = fields.SingleOrDefault(a => a.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
				if (entityField==null)
					continue;
				if (isFirst)
				{
					result.AppendFormat("[{0}] {1} {2}", entityField.Name, entityField.SqlType, entityField.AllowNull ? "NULL" : "NOT NULL");
					isFirst = false;
				}
				else
				{
					result.AppendFormat(",[{0}] {1} {2}", entityField.Name, entityField.SqlType, entityField.AllowNull ? "NULL" : "NOT NULL");
				}
			}
			result.Append(")");
			if (isFirst)
				return;
			_sqlCmd.ExecuteNonQuery(result.ToString());
		}


	}
}
