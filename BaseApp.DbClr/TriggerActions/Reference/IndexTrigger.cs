using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Platform.DbClr;
using Platform.DbClr.Interfaces;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.TriggerActions.Reference
{
	/// <summary>
	/// Триггеры для сущности Index
	/// </summary>
	public class IndexTrigger : ITriggerAction<Index>
	{
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		/// <summary>
		/// Включение/выключение использования индекса
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="use">Включить/Выключить</param>
		/// <param name="index">Индекс</param>
		private void _changeUseIndex(Index index, Entity entity, bool use)
		{
			_sqlCmd.ExecuteNonQuery(use
				                        ? string.Format("EXECUTE [dbo].[CreateOrAlterIndex] {0}", index.Id)
				                        : string.Format(
					                        "IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{1}].[{2}]') AND name = N'{0}') ALTER INDEX [{0}] ON [{1}].[{2}] DISABLE",
					                        index.Name, entity.Schema, entity.Name));
		}

		/// <summary>
		/// Удаление триггера
		/// </summary>
		/// <param name="indexName">Наименование индекса</param>
		/// <param name="entity">Сущность</param>
		private void _dropTrigger(Entity entity, string indexName)
		{
			const string textCommand = "IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[{0}].[{1}_{2}_IsVersioning]')) DROP TRIGGER [{0}].[{1}_{2}_IsVersioning]";
			_sqlCmd.ExecuteNonQuery(string.Format(textCommand, entity.Schema, entity.Name, indexName));
		}

		/// <summary>
		/// Удаление индекса
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="index">Индекс</param>
		private void _dropIndex(Index index, Entity entity)
		{
			if (index.IndexType == IndexType.UniqueIndex && entity.IsVersioning)
			{
				_dropTrigger(entity, index.Name);
			}
			else
			{
				_sqlCmd.ExecuteNonQuery(
					string.Format(
						"IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{1}].[{2}]') AND name = N'{0}') DROP INDEX [{0}] ON [{1}].[{2}] WITH ( ONLINE = OFF )",
						index.Name, entity.Schema,
						entity.Name));
			}
		}

		/// <summary>
		/// Переименование индекса
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="newName">Новое имя</param>
		/// <param name="index">Индекс</param>
		private void _renameIndex(Index index, Entity entity, string newName)
		{
			if (index.IndexType == IndexType.UniqueIndex && entity.IsVersioning)
			{
				_dropTrigger(entity, index.Name);
				_sqlCmd.ExecuteNonQuery(string.Format("EXECUTE [dbo].[CreateOrAlterVersioningTrigger] {0}", index.Id));
			}
			if ((index.IndexType != IndexType.UniqueIndex && entity.IsVersioning) || (index.IndexType == IndexType.UniqueIndex && !entity.IsVersioning))
			{
				_sqlCmd.ExecuteNonQuery(string.Format("EXEC sp_rename N'[{0}].[{1}].[{2}]', N'{3}', N'INDEX'", entity.Schema,
				                                      entity.Name, index.Name, newName));
			}
		}

		#region Implementation of ITriggerAction<Index>

		/// <summary>
		/// Релизация триггера INSERT
		/// </summary>
		public void ExecInsertCmd(List<Index> inserted)
		{
		}

		/// <summary>
		/// Релизация триггера UPDATE
		/// </summary>
		public void ExecUpdateCmd(List<Index> inserted, List<Index> deleted)
		{
			foreach (Index delete in deleted)
			{
				Index insert = inserted.Single(a => a.Id == delete.Id);
				if (insert == null)
					continue;
				Entity entity = delete.Entity;
				if (insert.IdRefStatus != delete.IdRefStatus)
				{
					_changeUseIndex(insert, entity, insert.Status == RefStatus.Work);
				}
				if (!insert.Name.Equals(delete.Name, StringComparison.OrdinalIgnoreCase))
				{
					_renameIndex(delete, entity, insert.Name);
				}
				if (insert.IndexType == IndexType.UniqueIndex && entity.IsVersioning)
				{
					_sqlCmd.ExecuteNonQuery(string.Format("EXECUTE [dbo].[CreateOrAlterVersioningTrigger] {0}", insert.Id));
				}
				else
				{
					_sqlCmd.ExecuteNonQuery(string.Format("EXECUTE [dbo].[CreateOrAlterIndex] {0}", insert.Id));
				}
			}
		}

		/// <summary>
		/// Релизация триггера DELETE
		/// </summary>
		public void ExecDeleteCmd(List<Index> deleted)
		{
			foreach (Index delete in deleted)
			{
                try
                {
                    Entity entity = delete.Entity;
                    _dropIndex(delete, entity);
                }
                catch
                {
                    continue;
                }
			}
		}

		#endregion
	}
}
