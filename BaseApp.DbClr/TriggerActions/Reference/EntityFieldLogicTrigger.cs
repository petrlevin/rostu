using System.Collections.Generic;
using System.Linq;
using Platform.DbClr;
using Platform.DbClr.Interfaces;
using Platform.DbCmd;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.TriggerActions.Reference
{
	/// <summary>
	/// Реализация триггера для [ref].[EntityField]
	/// </summary>
	public class EntityFieldLogicTrigger : ITriggerAction<EntityField>
	{
		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

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
		/// Команда добавления поля сущности для упорядочивания
		/// </summary>
		private const string _insertOrderedFieldForMultilink =
			"INSERT INTO [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [isSystem], [isHidden]) " +
			"VALUES ('{0}', 'Поле для упорядочивания', {1}, {2}, 0, 1, 1)";

		/// <summary>
		/// Команда удаления поля сущности для упорядочивания
		/// </summary>
		private const string _deleteOrderedFieldForMultilink =
			"DELETE FROM [ref].[EntityField] WHERE [idEntity]={0} AND Name='{1}'";

		/// <summary>
		/// Создание поля ссылающегося на сущность, для общей ссылки
		/// </summary>
		private void _createEntityFieldForReferenceLink(EntityField entityField)
		{
			if (!_genericLink.Contains(entityField.EntityFieldType))
				return;
			const int idEntityEntity = -2147483615;
			_sqlCmd.ExecuteNonQuery(string.Format(
				"INSERT INTO [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [idEntityLink], [idForeignKeyType], [AllowNull], [isSystem]) VALUES ('{0}', 'Ссылка на сущность', {1}, {2}, {3}, {4}, {5}, 1)",
				entityField.Name + "Entity", entityField.IdEntity, (byte) EntityFieldType.Link, idEntityEntity,
				(byte) ForeignKeyType.ForeignKey, entityField.ColumnAllowNull ? 1 : 0));
		}

        private void _createEntityFieldConstraintForReferenceLink(EntityField entityField)
        {

            if (!_genericLink.Contains(entityField.EntityFieldType))
                return;
            _sqlCmd.ExecuteNonQuery(string.Format(@"ALTER TABLE [{0}].[{1}]
                        ADD CONSTRAINT CK_{1}_{2}
                        CHECK (dbo.AllowGenericLinks({2})=1)" , Schemas.ByEntityType(entityField.Entity.EntityType) ,entityField.Entity.Name ,entityField.Name + "Entity"));
            
        }

        private void _deleteEntityFieldConstraintForReferenceLink(EntityField entityField)
        {

            if (!_genericLink.Contains(entityField.EntityFieldType))
                return;
            _sqlCmd.ExecuteNonQuery(string.Format(@"ALTER TABLE [{0}].[{1}]
                        DROP CONSTRAINT CK_{1}_{2}
                        ", Schemas.ByEntityType(entityField.Entity.EntityType), entityField.Entity.Name, entityField.Name + "Entity"));

        }



		/// <summary>
		/// Создание полей упорядочивания для мультиссылки
		/// </summary>
		private void _createOrderedFieldForMultilink(EntityField entityField)
		{
			if (entityField.Entity.EntityType == EntityType.Multilink)
			{
				if (entityField.Entity.Ordered.HasValue && entityField.Entity.Ordered.Value)
				{
					if (!entityField.Entity.Fields.Any(a => a.EntityFieldType == EntityFieldType.Int && a.Name == entityField.Name))
					{
						_sqlCmd.ExecuteNonQuery(string.Format(_insertOrderedFieldForMultilink, entityField.Name + "Order", entityField.IdEntity,
															 (byte)EntityFieldType.Int));
					}
				}
			}
		}

		/// <summary>
		/// Удаление поля ссылающегося на сущность, для общей ссылки
		/// </summary>
		private void _deleteEntityFieldForReferenceLink(EntityField entityField)
		{
			if (!_genericLink.Contains(entityField.EntityFieldType))
				return;
			_sqlCmd.ExecuteNonQuery(string.Format("DELETE FROM [ref].[EntityField] WHERE [idEntity]={0} AND [Name]='{1}'", entityField.IdEntity, entityField.Name + "Entity"));
		}

		/// <summary>
		/// Удаление полей упорядочивания для мультиссылки
		/// </summary>
		private void _delOrderedFieldForMultilink(EntityField entityField)
		{
			if (entityField.Entity.EntityType == EntityType.Multilink)
			{
				if (entityField.Entity.Ordered.HasValue && !entityField.Entity.Ordered.Value)
				{
					_sqlCmd.ExecuteNonQuery(string.Format(_deleteOrderedFieldForMultilink, entityField.IdEntity, entityField.Name + "Order"));
				}
			}
		}

		#region Implementation of ITriggerAction<EntityField>

		/// <summary>
		/// Релизация триггера INSERT
		/// </summary>
		public void ExecInsertCmd(List<EntityField> inserted)
		{
			List<int> idsEntity = inserted.Select(a => a.IdEntity).Distinct().ToList();
			foreach (EntityField insert in inserted)
			{
				_createEntityFieldForReferenceLink(insert);
                _createEntityFieldConstraintForReferenceLink(insert);
				_createOrderedFieldForMultilink(insert);
			}
			foreach (int idEntity in idsEntity)
			{
				_sqlCmd.ExecuteNonQuery("EXEC CreateOrAlterUserDefineTableTypeByIdEntity " + idEntity);
			}
		}


	    /// <summary>
		/// Релизация триггера UPDATE
		/// </summary>
		public void ExecUpdateCmd(List<EntityField> inserted, List<EntityField> deleted)
		{
			inserted.Select(a => a.IdEntity).Distinct().ToList().ForEach(a => _sqlCmd.ExecuteNonQuery("EXEC CreateOrAlterUserDefineTableTypeByIdEntity " + a));
		}

		/// <summary>
		/// Релизация триггера DELETE
		/// </summary>
		public void ExecDeleteCmd(List<EntityField> deleted)
		{
			foreach (EntityField delete in deleted)
			{
				try
				{
					Objects.ById<Entity>(delete.IdEntity);
				}
				catch
				{
					return;
				}
				_deleteEntityFieldForReferenceLink(delete);
                _deleteEntityFieldConstraintForReferenceLink(delete);
				_delOrderedFieldForMultilink(delete);
			}
			deleted.Select(a => a.IdEntity).Distinct().ToList().ForEach(a => _sqlCmd.ExecuteNonQuery("EXEC CreateOrAlterUserDefineTableTypeByIdEntity " + a));
		}

		#endregion
	}
}
