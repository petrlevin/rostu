using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Procedures
{
	/// <summary>
	/// Класс описывающий процедуру создания или обновления триггера для версионной сущности
	/// </summary>
	public class CreateOrAlterVersioningTrigger
	{
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(new SqlConnection("context connection = true"), ConnectionType.ConnectionPerCommand);

		/// <summary>
		/// Текст команды создания триггера
		/// </summary>
		private const string _createIsVersioning =
			@"CREATE TRIGGER {0} ON {1}
WITH EXECUTE AS CALLER
INSTEAD OF INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM inserted)
        RETURN;
    IF EXISTS(SELECT 1 FROM inserted WHERE [ValidityFrom]>=[ValidityTo])
    BEGIN
		RAISERROR(50002, 16, 1);
        RETURN;
    END
    IF (UPDATE([ValidityFrom]) OR UPDATE([ValidityTo]) {5})
    BEGIN
        IF EXISTS(SELECT 1 FROM inserted i INNER JOIN {1} t ON ({2}) AND ISNULL(t.[ValidityTo], CONVERT(DATE, '31.12.9999', 104)) > ISNULL(i.[ValidityFrom], CONVERT(DATE, '01.01.0001', 104)) AND ISNULL(t.[ValidityFrom], CONVERT(DATE, '01.01.0001', 104)) < ISNULL(i.[ValidityTo], CONVERT(DATE, '31.12.9999', 104)) AND NOT EXISTS(SELECT 1 FROM deleted WHERE (t.[id]=i.[id])))
            --OR EXISTS(SELECT 1 FROM inserted i INNER JOIN {1} t ON ({2}) AND t.[ValidityFrom] <= i.[ValidityFrom] AND t.[ValidityTo]>= i.[ValidityFrom] AND i.[ValidityTo] IS NULL AND t.[id]<>i.[id])
            --OR EXISTS(SELECT 1 FROM inserted i INNER JOIN {1} t ON ({2}) AND t.[ValidityFrom] <> i.[ValidityFrom] AND t.[ValidityTo] IS NULL AND i.[ValidityTo] IS NULL AND NOT EXISTS(SELECT 1 FROM deleted WHERE {2}))
            --OR EXISTS(SELECT 1 FROM inserted i INNER JOIN {1} t on ({2}) AND t.[ValidityFrom] > i.[ValidityFrom] AND i.[ValidityTo] IS NULL AND NOT EXISTS(SELECT 1 FROM deleted WHERE (t.[id]=i.[id])))
            --OR EXISTS(SELECT 1 FROM inserted i INNER JOIN {1} t on ({2}) AND t.[ValidityFrom] <= i.[ValidityTo] AND i.[ValidityTo] IS NOT NULL AND t.[ValidityTo] IS NULL AND t.[id]<>i.[id])
        BEGIN
			RAISERROR(50001, 16, 1, '{6}');
            RETURN;
        END
    END
    DECLARE @keys TABLE (Id INT);
    IF NOT EXISTS(SELECT * FROM deleted)
        INSERT INTO {1} ({3}) 
        OUTPUT INSERTED.id INTO @keys
        SELECT {3} FROM inserted;
    ELSE
        UPDATE mainTable SET {4}
        OUTPUT INSERTED.id INTO @keys
        FROM {1} mainTable
            INNER JOIN inserted u ON u.id=mainTable.id

    SELECT id FROM @keys
END";

		/// <summary>
		/// Реализация процедуры
		/// </summary>
		/// <param name="idIndex"></param>
		[SqlProcedure]
		public static void Exec(int idIndex)
		{
			Index index;
			Entity entity;
			try
			{
				index = Objects.ById<Index>(idIndex);
				
			} catch
			{
				SqlContext.Pipe.Send(string.Format("Ошибка при получении индекса с идентифкатором '{0}'", idIndex));
				return;
			}
			try
			{
			entity = Objects.ById<Entity>(index.IdEntity);
			}
			catch
			{
				SqlContext.Pipe.Send(string.Format("Ошибка при получении сущности с идентифкатором '{0}'", idIndex));
				return;
			}
			if (entity.IsVersioning && index.IndexType == IndexType.UniqueIndex)
			{
				_dropTrigger(entity, index);
				_createTrigger(entity, index);
			}
		}

		/// <summary>
		/// Удаление триггера
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="index">Индекс</param>
		private static void _dropTrigger(Entity entity, Index index)
		{
			const string textCommand = "IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_{2}_IsVersioning]')) DROP TRIGGER [{0}].[{1}_{2}_IsVersioning]";
			_sqlCmd.ExecuteNonQuery(string.Format(textCommand, entity.Schema, entity.Name, index.Name));
		}

		/// <summary>
		/// Создание триггера
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="index">Индекс</param>
		private static void _createTrigger(Entity entity, Index index)
		{
			List<string> listFields = _sqlCmd.SelectOneColumn<string>(
				"select b.Name from ml.Index_EntityField_Indexable a inner join ref.EntityField b on b.id=a.idEntityField WHERE idIndex={0} order by a.idEntityFieldOrder",
				index.Id);
			if (listFields.Count == 0)
			{
				listFields.Add("id");
			}
			string uniqueWhere = "(";
			string isUpdate = "";
			bool first = true;
			foreach (string item in listFields)
			{
				if (first)
				{
					uniqueWhere += string.Format("t.[{0}]=i.[{0}]", item);
					first = false;
				} else
				{
					uniqueWhere += string.Format(" AND t.[{0}]=i.[{0}]", item);
				}
				isUpdate += string.Format("OR UPDATE([{0}]) ", item);
			}
			uniqueWhere += ")";

			string updateFields = "";
			string insertFields = "";
			List<string> entityFields = _sqlCmd.SelectOneColumn<string>(string.Format("SELECT [Name] FROM [ref].[EntityField] WHERE [idEntity]={0} AND [idEntityFieldType] NOT IN (8,9,13,18) AND [idCalculatedFieldType] IS NULL AND [Name]<>'id'", entity.Id));
			first = true;
			foreach (string field in entityFields)
			{
				if (first)
				{
					insertFields += field;
					updateFields += string.Format("mainTable.[{0}]=u.[{0}]", field);
					first = false;
				}
				else
				{
					insertFields += string.Format(", {0}", field);
					updateFields += string.Format(", mainTable.[{0}]=u.[{0}]", field);
				}
			}

			string trigger = string.Format(_createIsVersioning,
										   entity.Name +"_"+index.Name + "_IsVersioning",
										   "[" + entity.Schema + "].[" + entity.Name + "]", uniqueWhere,
										   insertFields, updateFields, isUpdate, index.Name);
			try
			{
				_sqlCmd.ExecuteNonQuery(trigger);
			} catch(SqlException exception)
			{
				throw;
			}
		}
	}
}
