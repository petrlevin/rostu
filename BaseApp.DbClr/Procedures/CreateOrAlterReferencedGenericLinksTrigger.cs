using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Procedures
{
	/// <summary>
	/// Класс релизующий создание триггеров для сущностей на которые разрешена общая ссылка
	/// </summary>
	public class CreateOrAlterReferencedGenericLinksTrigger
	{
		#region Private Property

		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		private const string _deleteTrigger = @"CREATE TRIGGER [{0}].[{1}_AllowGenericLink_Delete]
   ON  [{0}].[{1}]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;

    IF EXISTS(SELECT 1 FROM DELETED a INNER JOIN [dbo].[GenericLinks] b ON [b].[idReferenced]=[a].[id] and [b].[idReferencedEntity]={2} INNER JOIN [ref].[EntityField] c ON [c].[idEntity]=[b].[idReferencesEntity] AND [c].[id]=[b].[idReferencesEntityField] WHERE [c].[idForeignKeyType]=2)
		RAISERROR(50004, 16, 1);
	
	IF EXISTS(SELECT 1 FROM DELETED a INNER JOIN [dbo].[GenericLinks] b ON [b].[idReferenced]=[a].[id] and [b].[idReferencedEntity]={2} INNER JOIN [ref].[EntityField] c ON [c].[idEntity]=[b].[idReferencesEntity] AND [c].[id]=[b].[idReferencesEntityField] WHERE [c].[idForeignKeyType]=3)
	BEGIN
		DECLARE @str varchar(max);
        SET @str =(SELECT dbo.Concatenate(CAST([a].[id] as varchar),',' ) FROM DELETED as [a] )


		EXECUTE sp_executesql  N'[dbo].[CascadeDeleteForGenericLinks] {2} , @str ' ,N'@str nvarchar(max)',@str 

	END
END";
		#endregion

		#region private Methods
		/// <summary>
		/// Удаление триггера
		/// </summary>
		/// <param name="entity">Сущность</param>
		private static void _dropTrigger(Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(
				string.Format(
					"IF EXISTS(SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_AllowGenericLink_Delete]')) DROP TRIGGER [{0}].[{1}_AllowGenericLink_Delete]",
					entity.Schema, entity.Name));
		}

		/// <summary>
		/// Создание триггера
		/// </summary>
		/// <param name="entity">Сущность</param>
		private static void _createTrigger(Platform.PrimaryEntities.Reference.Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(string.Format(_deleteTrigger, entity.Schema, entity.Name, entity.Id));
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
				_dropTrigger(entity);
				if (!entity.AllowGenericLinks)
					return;
				_createTrigger(entity);
			} catch
			{
				return;
			}
		}

	}
}
