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
	/// Класс релизующий создание триггеров для сущностей с полями типа "Общая ссылка"
	/// </summary>
	public class CreateOrAlterReferencesGenericLinksTrigger
	{
		#region Private Property
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		/// <summary>
		/// Операции
		/// </summary>
		private enum Operation
		{
			Insert,
			Delete,
			Update
		}

		/// <summary>
		/// Список типов поля - общих ссылок
		/// </summary>
		private static readonly EntityFieldType[] _genericLink = new[]
				{
					EntityFieldType.ReferenceEntity,
					EntityFieldType.ToolEntity,
					EntityFieldType.TablepartEntity,
					EntityFieldType.DocumentEntity
				};
		
		/// <summary>
		/// Текст триггера ON DELETE
		/// </summary>
		private const string _deleteTigger = @"CREATE TRIGGER [{0}].[{1}_GenericLink_Delete]
   ON [{0}].[{1}]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	{2}
END";
		/// <summary>
		/// Команда DELETE для триггера ON DELETE
		/// </summary>
		private const string _deleteCommand =
			"DELETE c FROM [dbo].[GenericLinks] c INNER JOIN DELETED a ON a.id=c.idReferences AND c.idReferencesEntity={0} AND c.idReferencesEntityField={1};";

		/// <summary>
		/// Текст триггера ON INSERT
		/// </summary>
		private const string _insertTigger = @"CREATE TRIGGER [{0}].[{1}_GenericLink_Insert]
   ON  [{0}].[{1}]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;

    --Проверка заполнения поля ***Entity для заполненных полей ALLOW NULL
    {2}
    
    {3}
END
";
		/// <summary>
		/// Команда проверки для триггера ON INSERT и ON DELETE
		/// </summary>
		private const string _checkInsertCommand =
			"IF EXISTS(SELECT 1 FROM INSERTED WHERE {0}) RAISERROR(50003, 16, 1);";

		/// <summary>
		/// Текст триггера ON UPDATE
		/// </summary>
		private const string _updateTrigger = @"CREATE TRIGGER [{0}].[{1}_GenericLink_Update]
   ON  [{0}].[{1}]
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

    --Проверка заполнения поля ***Entity для заполненных полей ALLOW NULL
    {2}
	
	--Удалить измененные
	{3}
	
	--Добавить измененные
    {4}
END";

		private const string _deleteCommandforUpdateTiggerAllowNull = @"DELETE c FROM [dbo].[GenericLinks] c
		INNER JOIN DELETED a ON a.id=c.idReferences AND c.idReferencesEntity={0} AND c.idReferencesEntityField={1}
		INNER JOIN INSERTED b on b.id=a.id
	WHERE a.{2} IS NOT NULL AND a.{2}<>ISNULL(b.{2}, 0)";

		private const string _deleteCommandforUpdateTigger = @"DELETE c FROM [dbo].[GenericLinks] c
		INNER JOIN DELETED a ON a.id=c.idReferences AND c.idReferencesEntity={0} AND c.idReferencesEntityField={1}
		INNER JOIN INSERTED b on b.id=a.id
	WHERE a.{2}<>b.{2}";
		#endregion

		#region private Methods
		/// <summary>
		/// Создание триггера ON DELETE
		/// </summary>
		/// <param name="entity">Сущность</param>
		private static void _createDeleteTrigger(Entity entity)
		{
			_dropTrigger(entity, Operation.Delete);
			if (!entity.Fields.Any(a => _genericLink.Contains(a.EntityFieldType)))
				return;
			StringBuilder deleteTextCommands = new StringBuilder();
			foreach (IEntityField entityField in entity.Fields.Where(a => _genericLink.Contains(a.EntityFieldType)))
			{
				deleteTextCommands.AppendFormat(_deleteCommand, entity.Id, entityField.Id);
				deleteTextCommands.AppendLine();
			}
			_sqlCmd.ExecuteNonQuery(string.Format(_deleteTigger, entity.Schema, entity.Name, deleteTextCommands.ToString()));
		}

		/// <summary>
		/// Создание триггера ON INSERT
		/// </summary>
		/// <param name="entity">Сущность</param>
		private static void _createInsertTrigger(Entity entity)
		{
			_dropTrigger(entity, Operation.Insert);
			if (!entity.Fields.Any(a => _genericLink.Contains(a.EntityFieldType)))
				return;
			bool isFirst = true;
			bool isFirst2 = true;
			StringBuilder checkInsertCommand = new StringBuilder("IF EXISTS(SELECT 1 FROM INSERTED WHERE ");
			StringBuilder insertCommand =
				new StringBuilder(
					"INSERT INTO [dbo].[GenericLinks] ([idReferenced],[idReferencedEntity],[idReferences],[idReferencesEntity],[idReferencesEntityField])");
			foreach (IEntityField entityField in entity.Fields.Where(a => _genericLink.Contains(a.EntityFieldType)))
			{
				if (entityField.AllowNull)
				{
					if (isFirst)
					{
						checkInsertCommand.AppendLine(string.Format("({0} IS NOT NULL AND {0}Entity IS NULL)", entityField.Name));
						isFirst = false;
					}
					else
					{
						checkInsertCommand.AppendFormat(" OR ({0} IS NOT NULL AND {0}Entity IS NULL)", entityField.Name);
					}
				}
				if (isFirst2)
				{
					isFirst2 = false;
				} else
				{
					insertCommand.AppendLine("UNION ALL");
				}
				insertCommand.AppendLine(string.Format("SELECT {0}, {0}Entity, id, {1}, {2} FROM INSERTED {3}", entityField.Name,
													   entity.Id, entityField.Id,
													   (entityField as EntityField).ColumnAllowNull
														   ? string.Format("WHERE {0} IS NOT NULL", entityField.Name)
														   : ""));
			}
			if (isFirst)
			{
				checkInsertCommand = new StringBuilder("");
			}
			else
			{
				checkInsertCommand.Append(") RAISERROR(50003, 16, 1);");
			}
			_sqlCmd.ExecuteNonQuery(string.Format(_insertTigger, entity.Schema, entity.Name, checkInsertCommand.ToString(), insertCommand.ToString()));
		}

		/// <summary>
		/// Создание триггера ON DELETE
		/// </summary>
		/// <param name="entity">Сущность</param>
		private static void _createUpdateTrigger(Entity entity)
		{
			_dropTrigger(entity, Operation.Update);
			if (!entity.Fields.Any(a => _genericLink.Contains(a.EntityFieldType)))
				return;
			StringBuilder deleteCommands = new StringBuilder();
			StringBuilder insertCommand =
				new StringBuilder(
					"INSERT INTO [dbo].[GenericLinks] ([idReferenced],[idReferencedEntity],[idReferences],[idReferencesEntity],[idReferencesEntityField])");
			StringBuilder checkInsertCommand = new StringBuilder("IF EXISTS(SELECT 1 FROM INSERTED WHERE ");
			bool isFirst = true;
			bool isFirst2 = true;
			foreach (IEntityField entityField in entity.Fields.Where(a => _genericLink.Contains(a.EntityFieldType)))
			{
				deleteCommands.AppendLine(
					string.Format(entityField.AllowNull ? _deleteCommandforUpdateTiggerAllowNull : _deleteCommandforUpdateTigger,
					              entity.Id, entityField.Id, entityField.Name));
				
				if (entityField.AllowNull)
				{
					if (isFirst)
					{
						checkInsertCommand.AppendLine(string.Format("({0} IS NOT NULL AND {0}Entity IS NULL)", entityField.Name));
						isFirst = false;
					}
					else
					{
						checkInsertCommand.AppendFormat(" OR ({0} IS NOT NULL AND {0}Entity IS NULL)", entityField.Name);
					}
				}
				if (isFirst2)
				{
					isFirst2 = false;
				}
				else
				{
					insertCommand.AppendLine("UNION ALL");
				}
				insertCommand.AppendLine(string.Format("SELECT [a].[{0}], [a].[{0}Entity], [a].[id], {1}, {2} FROM INSERTED a INNER JOIN DELETED b ON [b].[id]=[a].[id] {3}", entityField.Name,
													   entity.Id, entityField.Id,
													   (entityField as EntityField).ColumnAllowNull
														   ? string.Format("WHERE [a].[{0}] IS NOT NULL AND [a].[{0}]<>ISNULL([b].[{0}], 0)", entityField.Name)
														   : string.Format("WHERE [a].[{0}]<>[b].[{0}]", entityField.Name)));
			}
			if (isFirst)
			{
				checkInsertCommand = new StringBuilder("");
			}
			else
			{
				checkInsertCommand.Append(") RAISERROR(50003, 16, 1);");
			}
			_sqlCmd.ExecuteNonQuery(string.Format(_updateTrigger, entity.Schema, entity.Name, checkInsertCommand.ToString(),
			                                      deleteCommands.ToString(), insertCommand.ToString()));
		}


		/// <summary>
		/// Удаление триггера для заданной операции
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="operation">Операция</param>
		private static void _dropTrigger(Entity entity, Operation operation)
		{
			_sqlCmd.ExecuteNonQuery(
				string.Format(
					"IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_GenericLink_{2}]')) DROP TRIGGER [{0}].[{1}_GenericLink_{2}]",
					entity.Schema, entity.Name, operation.ToString()));
		}
		#endregion
		
		/// <summary>
		/// Метод создания триггеров
		/// </summary>
		/// <param name="idEntity">Идентификатор сущности</param>
		[SqlProcedure]
		public static void Exec(int idEntity)
		{
			try
			{
				Entity entity = Objects.ById<Entity>(idEntity);
				_createDeleteTrigger(entity);
				_createInsertTrigger(entity);
				_createUpdateTrigger(entity);
			}
			catch
			{
				return;
			}
		}

	}
}
