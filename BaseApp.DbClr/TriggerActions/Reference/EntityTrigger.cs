using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;
using Platform.DbClr.Interfaces;
using System.Collections.Generic;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
namespace BaseApp.DbClr.TriggerActions.Reference
{
	/// <summary>
	/// Триггер для сущности Entity
	/// </summary>
	public class EntityTrigger : ITriggerAction<Entity>
	{
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		/// <summary>
		/// Проверка валидности имени сущности
		/// </summary>
		private static readonly Regex _validName = new Regex(@"^[a-zA-Z]\w*$");
		
		/// <summary>
		/// Команда создания таблицы
		/// </summary>
		private const string _createTable = "CREATE TABLE [{0}].[{1}] ( [tstamp] TIMESTAMP NOT NULL )";
		
		/// <summary>
		/// Команда удаления таблицы
		/// </summary>
		private const string _dropTable = "DROP TABLE [{0}].[{1}]";

		/// <summary>
		/// Команда переименования объекта БД
		/// </summary>
		private const string _renameObject = "EXEC sp_rename @objname='{0}.{1}', @newname='{2}'";
		
		/// <summary>
        /// Удалени строк из регистра по зависимостям объектов
        /// </summary>
		private const string _deleteItemDep =
                "DELETE FROM reg.ItemsDependencies WHERE idObject = '{0}' AND idObjectEntity = {1}";

		/// <summary>
		/// Удалени строк из таблицы поддержки общих ссылок
		/// </summary>
		private const string _deleteGenericLinks =
			"DELETE FROM [dbo].[GenericLinks] WHERE [idReferencedEntity]={0} OR [idReferencesEntity]={0}";

		/// <summary>
		/// Список типов поля - общих ссылок
		/// </summary>
		readonly EntityFieldType[] _genericLink = new[]
				{
					EntityFieldType.ReferenceEntity,
					EntityFieldType.ToolEntity,
					EntityFieldType.TablepartEntity,
					EntityFieldType.DocumentEntity
				};
		
		/// <summary>
		/// Создание таблицы
		/// </summary>
		/// <param name="entity">Сущность</param>
        private void CreateTable(Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(string.Format(_createTable, entity.Schema, entity.Name));
			SqlContext.Pipe.Send(string.Format("Вставлена сущность с именем {0}", entity.Name));
		}

		/// <summary>
		/// Изменение таблицы
		/// </summary>
		/// <param name="insertEntity">Новое значение</param>
		/// <param name="deleteEntity">Старое значение</param>
        private void _renameTable(Entity insertEntity, Entity deleteEntity)
		{
			_renamePk(insertEntity, deleteEntity);
			_renameFk(insertEntity, deleteEntity);
			_renameDefault(insertEntity, deleteEntity);
			_renameCheck(insertEntity, deleteEntity);
			_sqlCmd.ExecuteNonQuery(string.Format(_renameObject, deleteEntity.Schema, deleteEntity.Name, insertEntity.Name));
			if (SqlContext.Pipe != null)
				SqlContext.Pipe.Send(string.Format("Обновлена сущность с именем {0}", insertEntity.Name));
		}

		/// <summary>
		/// Переименовывание Primary Key таблицы
		/// </summary>
		/// <param name="insertEntity">Новое значение</param>
		/// <param name="deleteEntity">Старое значение</param>
		private void _renamePk(Entity insertEntity, Entity deleteEntity)
		{
			_sqlCmd.ExecuteNonQuery(string.Format(_renameObject, deleteEntity.Schema, "PK_" + deleteEntity.Name, "PK_" + insertEntity.Name));
		}

		/// <summary>
		/// Переименовывание Foreign Keys таблицы
		/// </summary>
		/// <param name="insertEntity">Новое значение</param>
		/// <param name="deleteEntity">Старое значение</param>
		private void _renameFk(Entity insertEntity, Entity deleteEntity)
		{
			foreach (IEntityField entityField in deleteEntity.Fields.Where(a=> a.IsFieldWithForeignKey() && (a.IdForeignKeyType==(byte)ForeignKeyType.ForeignKey || a.IdForeignKeyType==(byte)ForeignKeyType.ForeignKeyWithCascadeDelete) && a.IdEntity!=a.IdEntityLink))
			{
				_sqlCmd.ExecuteNonQuery(string.Format(_renameObject, deleteEntity.Schema, "FK_" + entityField.Name + "_" + deleteEntity.Name + "_" + entityField.EntityLink.Name, "FK_" + entityField.Name + "_" + insertEntity.Name + "_" + entityField.EntityLink.Name));
			}
			List<EntityField> listFields = _sqlCmd.Select<EntityField>(
                "SELECT * FROM [ref].[EntityField] WHERE [idEntity]<>{0} AND [idEntityLink]={0} AND ([idForeignKeyType]={1} OR [idForeignKeyType]={2}) AND ( [idEntityFieldType]={3} Or [idEntityFieldType]={4})",
				new object[] {insertEntity.Id, (byte) ForeignKeyType.ForeignKey, (byte) ForeignKeyType.ForeignKeyWithCascadeDelete, (byte)EntityFieldType.Link, (byte)EntityFieldType.FileLink});
			foreach (EntityField entityField in listFields)
			{
				_sqlCmd.ExecuteNonQuery(string.Format(_renameObject, entityField.Entity.Schema, "FK_" + entityField.Name + "_" + entityField.Entity.Name + "_" + deleteEntity.Name, "FK_" + entityField.Name + "_" + entityField.Entity.Name + "_" + insertEntity.Name));
			}
			foreach (IEntityField entityField in deleteEntity.Fields.Where(a=> a.IsFieldWithForeignKey() && (a.IdForeignKeyType==(byte)ForeignKeyType.ForeignKey || a.IdForeignKeyType==(byte)ForeignKeyType.ForeignKeyWithCascadeDelete) && a.IdEntity==a.IdEntityLink))
			{
				_sqlCmd.ExecuteNonQuery(string.Format(_renameObject, deleteEntity.Schema, "FK_" + entityField.Name + "_" + deleteEntity.Name + "_" + deleteEntity.Name, "FK_" + entityField.Name + "_" + insertEntity.Name + "_" + insertEntity.Name));
			}
		}

		/// <summary>
		/// Переименовывание DEFAULT таблицы
		/// </summary>
		/// <param name="insertEntity">Новое значение</param>
		/// <param name="deleteEntity">Старое значение</param>
		private void _renameDefault(Entity insertEntity, Entity deleteEntity)
		{
			foreach (IEntityField entityField in deleteEntity.Fields.Where(a=> a.FieldDefaultValueType==FieldDefaultValueType.Sql && !string.IsNullOrEmpty(a.DefaultValue)))
			{
				_sqlCmd.ExecuteNonQuery(string.Format(_renameObject, deleteEntity.Schema, "DEFAULT_" + deleteEntity.Name + "_" + entityField.Name, "DEFAULT_" + insertEntity.Name + "_" + entityField.Name));
			}
		}

		/// <summary>
		/// Переименовывание CHECK таблицы
		/// </summary>
		/// <param name="insertEntity">Новое значение</param>
		/// <param name="deleteEntity">Старое значение</param>
		private void _renameCheck(Entity insertEntity, Entity deleteEntity)
		{
			foreach (IEntityField entityField in deleteEntity.Fields.Where(a=> _genericLink.Contains(a.EntityFieldType)))
			{
				_sqlCmd.ExecuteNonQuery(string.Format(_renameObject, deleteEntity.Schema, "CK_" + deleteEntity.Name + "_" + entityField.Name + "Entity", "CK_" + insertEntity.Name + "_" + entityField.Name + "Entity"));
			}
		}

		private void _renameGenericLinkTriggers(Entity insertEntity, Entity deleteEntity)
		{
			_sqlCmd.ExecuteNonQuery(string.Format("IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_GenericLink_Delete]')) "+_renameObject, deleteEntity.Schema, deleteEntity.Name + "_GenericLink_Delete", "CK_" + insertEntity.Name + "_GenericLink_Delete"));
			_sqlCmd.ExecuteNonQuery(string.Format("IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_GenericLink_Insert]')) "+_renameObject, deleteEntity.Schema, deleteEntity.Name + "_GenericLink_Insert", "CK_" + insertEntity.Name + "_GenericLink_Insert"));
			_sqlCmd.ExecuteNonQuery(string.Format("IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_GenericLink_Update]')) "+_renameObject, deleteEntity.Schema, deleteEntity.Name + "_GenericLink_Update", "CK_" + insertEntity.Name + "_GenericLink_Update"));
			_sqlCmd.ExecuteNonQuery(string.Format("IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_AllowGenericLink_Delete]')) " + _renameObject, deleteEntity.Schema, deleteEntity.Name + "_AllowGenericLink_Delete", "CK_" + insertEntity.Name + "_AllowGenericLink_Delete"));
			_sqlCmd.ExecuteNonQuery(string.Format("IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_AllowGenericLink_Insert]')) " + _renameObject, deleteEntity.Schema, deleteEntity.Name + "_AllowGenericLink_Insert", "CK_" + insertEntity.Name + "_AllowGenericLink_Insert"));
			_sqlCmd.ExecuteNonQuery(string.Format("IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_AllowGenericLink_Update]')) " + _renameObject, deleteEntity.Schema, deleteEntity.Name + "_AllowGenericLink_Update", "CK_" + insertEntity.Name + "_AllowGenericLink_Update"));
		}

		/// <summary>
		/// Удаление таблицы
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <returns></returns>
        private void DropTable(Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(string.Format(_dropTable, entity.Schema, entity.Name));
			if (SqlContext.Pipe != null) 
				SqlContext.Pipe.Send(string.Format("Удалена сущность с именем {0}", entity.Name));
		}

		#region Implementation of ITriggerAction<Entity>

		/// <summary>
		/// Релизация триггера INSERT
		/// </summary>
        public void ExecInsertCmd(List<Entity> inserted)
		{
			if (inserted.Any(a=> !_validName.IsMatch(a.Name)))
				throw new Exception("В названии сущности использованы недопустимые символы");
			inserted.ForEach(CreateTable);
		}

		/// <summary>
		/// Релизация триггера UPDATE
		/// </summary>
        public void ExecUpdateCmd(List<Entity> inserted, List<Entity> deleted)
		{
			if (inserted.Any(a => !_validName.IsMatch(a.Name)))
				throw new Exception("В названии сущности использованы недопустимые символы");
            foreach (Entity insert in inserted)
			{
                Entity delete = deleted.SingleOrDefault(a => a.Id == insert.Id);
                if(!delete.Name.Equals(insert.Name, StringComparison.OrdinalIgnoreCase))
				    _renameTable(insert, delete);
			}
		}

		/// <summary>
		/// Релизация триггера DELETE
		/// </summary>
		public void ExecDeleteCmd(List<Entity> deleted)
		{
		    foreach (var delete in deleted)
		    {
		        DropTable(delete);
                _sqlCmd.ExecuteNonQuery(string.Format(_deleteItemDep, delete.Id, delete.EntityId));
			    _sqlCmd.ExecuteNonQuery(string.Format(_deleteGenericLinks, delete.Id));
		    }
		}

		#endregion
	}


}
