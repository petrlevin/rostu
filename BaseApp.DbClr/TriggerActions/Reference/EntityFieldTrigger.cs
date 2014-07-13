using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Platform.DbClr.Interfaces;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.TriggerActions.Reference
{
	/// <summary>
	/// Триггеры для сущности EntityField
	/// </summary>
	public partial class EntityFieldTrigger : ITriggerAction<EntityField>
    {
        #region Private Fields

        /// <summary>
		/// Проверка валидности имени поля сущности
		/// </summary>
		private static readonly Regex _validName = new Regex(@"^[a-zA-Z]\w*$");

		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

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
        /// Типы полей для которых не создается соответствующая колонка в таблице
        /// </summary>
        private static readonly EntityFieldType[] _withoutColumn = new[]
				{
					EntityFieldType.Multilink,
					EntityFieldType.Tablepart,
					EntityFieldType.VirtualTablePart,
					EntityFieldType.DataEndpoint,
				};

        #endregion

        #region Private Methods

        /// <summary>
		/// Удалени строк из таблицы поддержки общих ссылок
		/// </summary>
		private void _deleteGenericLinks(Platform.PrimaryEntities.Reference.EntityField entityField)
		{
			if (_genericLink.Contains(entityField.EntityFieldType))
			{
				_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DeleteGenericLinks, entityField.Id));
			}
		}
		/// <summary>
		/// Создание столбца в таблице
		/// </summary>
		/// <param name="entityField">Поле сущности</param>
		/// <param name="entity">Сущность в которую добавляется поле</param>
		private void _createColumn(Platform.PrimaryEntities.Reference.EntityField entityField, Platform.PrimaryEntities.Reference.Entity entity)
		{
			if (entityField.IdCalculatedFieldType.HasValue && entityField.CalculatedFieldType == CalculatedFieldType.DbComputedFunction)
				return;
			if (entityField.IdCalculatedFieldType.HasValue && (entityField.CalculatedFieldType == CalculatedFieldType.DbComputed || entityField.CalculatedFieldType == CalculatedFieldType.DbComputedPersisted))
			{
				if (String.IsNullOrEmpty(entityField.Expression))
					throw new Exception("Не заполнено поле Expression");

				string persisted = entityField.CalculatedFieldType == CalculatedFieldType.DbComputedPersisted ? "PERSISTED" : "";
				_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.CreateColumnComputed,
									 new object[] { entity.Schema, entity.Name, entityField.Name, entityField.Expression, persisted}));
			} 
            else if (entityField.Name.Equals("id", StringComparison.OrdinalIgnoreCase) && entity.EntityType != EntityType.Enum)
			{
				_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.CreateColumnId,
				new object[]
					{
						entity.Schema, entity.Name, entityField.Name, entityField.SqlType,
						entityField.ColumnAllowNull ? "NULL" : "NOT NULL"
					}));

			} 
            else if (String.IsNullOrEmpty(entityField.DefaultValue) || entityField.FieldDefaultValueType != FieldDefaultValueType.Sql)
			{
				_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.CreateColumnWithoutDefault,
				new object[]
					{
						entity.Schema, entity.Name, entityField.Name, entityField.SqlType,
						entityField.ColumnAllowNull ? "NULL" : "NOT NULL"
					}));
			}
			else
			{
				if (String.IsNullOrEmpty(entityField.DefaultValue))
					throw new Exception("CreateColumn: не заполнено поле DefaultValue");
				_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.CreateColumnWithDefault,
				new object[]
						{
							entity.Schema, entity.Name, entityField.Name, entityField.SqlType,
							entityField.ColumnAllowNull ? "NULL" : "NOT NULL", entityField.DefaultValue
						}));
			}

			if (entity.EntityType != EntityType.Multilink && entityField.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
			{
				_createPk(entity, "[id]");
			}

			if (entity.EntityType == EntityType.Multilink)
			{
				List<string> listFields = _sqlCmd.SelectOneColumn<string>(
					string.Format("SELECT '['+[Name]+']' FROM [ref].[EntityField] WHERE [idEntity]={0} AND [idEntityFieldType]=7",
					              entity.Id));
				if (listFields.Count == 2)
				{
					_createPk(entity, string.Join(",", listFields.ToArray()));
				}
			}
		}

		/// <summary>
		/// Создание Primary Key
		/// </summary>
		private void _createPk(Entity entity, string listFields)
		{
			_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.CreatePk, entity.Schema, entity.Name, listFields));
		}

		/// <summary>
		/// Удаление Primary Key
		/// </summary>
		/// <param name="entityField">Поле сущности</param>
		/// <param name="entity">Сущность</param>
		private void _dropPk(EntityField entityField, Entity entity)
		{
			if (entityField.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
			{
				_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DropPk, entity.Schema, entity.Name));
			}
		}

		/// <summary>
		/// Создание Foregn Key
		/// </summary>
		/// <param name="entityField">Поле сущности</param>
		/// <param name="entity">Сущность</param>
		/// <returns>string</returns>
		private void _createForeignKey(EntityField entityField, Entity entity)
		{
			if (entityField.ForeignKeyType == ForeignKeyType.WithOutForeignKey || !entityField.IsFieldWithForeignKey() )
				return;
			if (entityField.IdEntityLink == null && entityField.Name.Equals("idowner", StringComparison.OrdinalIgnoreCase))
				return;
			if (entityField.IdEntityLink == null)
				throw new Exception("Не заполнено поле IdEntityLink");

			Entity entityLink = _sqlCmd.SelectFirst<Entity>("SELECT * FROM [ref].[Entity] WHERE [id]='" + entityField.IdEntityLink + "'");

			string fkName = string.Format("{0}_{1}_{2}", entityField.Name, entity.Name, entityField.EntityLink.Name);
			if (fkName.Length > 124)
				fkName = fkName.Substring(0, 124);
			switch (entityField.ForeignKeyType)
			{
				case ForeignKeyType.ForeignKey:
					{
						_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.CreateFk,
													 entity.Schema, entity.Name, entityLink.Schema, entityLink.Name, entityField.Name,
													 "NO ACTION", fkName
												 ));
						_createIndex(entityField, entity);
						break;
					}
				case ForeignKeyType.ForeignKeyWithCascadeDelete:
					{
						_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.CreateFk,
						                                      entity.Schema, entity.Name, entityLink.Schema, entityLink.Name,
						                                      entityField.Name, "CASCADE", fkName));
						_createIndex(entityField, entity);
						break;
					}
				default:
					return;
			}
		}

		/// <summary>
		/// Удаления Foregn Key
		/// </summary>
		/// <param name="entityField">Поле сущности</param>
		/// <param name="entity">Сущность</param>
		/// <returns>string</returns>
		private void _dropForeignKey(EntityField entityField, Entity entity)
		{
			if (entityField.ForeignKeyType != ForeignKeyType.WithOutForeignKey && entityField.IsFieldWithForeignKey() )
			{
				if (entityField.IdEntityLink == null)
					return;
                
				string fkName = string.Format("{0}_{1}_{2}", entityField.Name, entity.Name, entityField.EntityLink.Name);
				if (fkName.Length > 124)
					fkName = fkName.Substring(0, 124);
				_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.DropFk,
													  entity.Schema, entity.Name, fkName));
				_dropIndex(entityField, entity);
			}
		}

		/// <summary>
		/// Создание индекса для ссылочного поля
		/// </summary>
		/// <param name="entityField"></param>
		/// <param name="entity"></param>
		private void _createIndex(EntityField entityField, Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.CreateIndex, entityField.Name, entity.Schema, entity.Name));
		}

		/// <summary>
		/// Удаление индекса для ссылочного поля
		/// </summary>
		/// <param name="entityField"></param>
		/// <param name="entity"></param>
		private void _dropIndex(EntityField entityField, Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DropIndex, entity.Schema, entity.Name, entityField.Name));
		}

		/// <summary>
		/// Обновление версионных триггеров после изменения состава полей
		/// </summary>
		private void _updateVersioningTriggers(Entity entity)
		{
			if (entity.IsVersioning)
			{
				List<int> listIdUniqueIndexes =
					_sqlCmd.SelectOneColumn<int>(string.Format(
						"SELECT [id] FROM [ref].[Index] WHERE [idEntity]={0} AND idIndexType={1}",
						entity.Id, (byte)IndexType.UniqueIndex));
				foreach (int idIndex in listIdUniqueIndexes)
				{
					_sqlCmd.ExecuteNonQuery(string.Format("EXECUTE [dbo].[CreateOrAlterVersioningTrigger] {0}", idIndex));
				}
			}
		}

		/// <summary>
		/// Переименование столбца таблицы
		/// </summary>
		/// <param name="insert">Новое значение</param>
		/// <param name="delete">Старое значение</param>
		/// <param name="entity">Сущность</param>
		private void _renameColumn(EntityField insert, EntityField delete, Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.RenameColumn, entity.Schema, entity.Name, delete.Name, insert.Name));
            if (delete.IsFieldWithForeignKey() && delete.ForeignKeyType != ForeignKeyType.WithOutForeignKey)
				_dropForeignKey(delete, entity);
            if (insert.IsFieldWithForeignKey() && insert.ForeignKeyType != ForeignKeyType.WithOutForeignKey)
				_createForeignKey(insert, entity);
		}

		/// <summary>
		/// Изменение типа поля
		/// </summary>
		/// <param name="insert">Новое значение</param>
		/// <param name="delete">Старое значение</param>
		/// <param name="entity">Сущность</param>
		private void _alterColumnChangeType(EntityField insert, EntityField delete, Entity entity)
		{
			if (!string.IsNullOrEmpty(delete.DefaultValue) && delete.FieldDefaultValueType == FieldDefaultValueType.Sql)
			{
				_dropDefault(delete, entity);
			}
            if (delete.IsFieldWithForeignKey() && delete.ForeignKeyType != ForeignKeyType.WithOutForeignKey)
			{
				_dropForeignKey(delete, entity);
			}
			_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.AlterColumnChangeType, entity.Schema, entity.Name, insert.Name,
                                   insert.SqlType, insert.ColumnAllowNull ? "NULL" : "NOT NULL"));
			if (!string.IsNullOrEmpty(insert.DefaultValue) && insert.FieldDefaultValueType == FieldDefaultValueType.Sql)
			{
				_createDefault(insert, entity);
			}
            if (insert.IsFieldWithForeignKey() && insert.ForeignKeyType != ForeignKeyType.WithOutForeignKey)
			{
				_createForeignKey(insert, entity);
			}
		}

		/// <summary>
		/// Удаление значения по умолчанию для столбца
		/// </summary>
		/// <param name="entityField">Поле сущности</param>
		/// <param name="entity">Сущность</param>
		/// <returns>string</returns>
		private void _dropDefault(EntityField entityField, Entity entity)
		{
			if (!String.IsNullOrEmpty(entityField.DefaultValue) && entityField.FieldDefaultValueType == FieldDefaultValueType.Sql)
			{
				_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.DropDefault, entity.Schema, entity.Name, entityField.Name));
			}
		}

		/// <summary>
		/// Создание значения по умолчанию для столбца
		/// </summary>
		/// <param name="entityField">Поле сущности</param>
		/// <param name="entity">Сущность</param>
		/// <returns>string</returns>
		private void _createDefault(EntityField entityField, Entity entity)
		{
			if (!String.IsNullOrEmpty(entityField.DefaultValue) && entityField.FieldDefaultValueType == FieldDefaultValueType.Sql)
			{
				_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.CreateDefault, entity.Schema, entity.Name, entityField.Name, entityField.DefaultValue));
			}
		}

		/// <summary>
		/// Удаление столбца в таблице
		/// </summary>
		/// <param name="entityField">Поле сущности</param>
		/// <param name="entity">Сущность</param>
		/// <returns>string</returns>
		private void _dropColumn(EntityField entityField, Entity entity)
		{
			_sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.StrDropColumn, entity.Schema, entity.Name, entityField.Name));
		}

        #endregion

        #region Implementation ITriggerAction

        /// <summary>
        /// Релизация триггера INSERT
        /// </summary>
        public void ExecInsertCmd(List<EntityField> inserted)
        {
            if (inserted.Any(a => !_validName.IsMatch(a.Name)))
                throw new Exception("В названии поля сущности использованы недопустимые символы");


            Entity entity = null;
            List<Entity> listEntity = new List<Entity>();
            foreach (EntityField entityField in inserted)
            {
                if (_withoutColumn.Contains(entityField.EntityFieldType) ||
                    (entityField.IdCalculatedFieldType.HasValue &&
                     (entityField.CalculatedFieldType == CalculatedFieldType.AppComputed ||
                      entityField.CalculatedFieldType == CalculatedFieldType.ClientComputed ||
                      entityField.CalculatedFieldType == CalculatedFieldType.NumeratorExpression ||
                      entityField.CalculatedFieldType == CalculatedFieldType.Joined)))
                    continue;

                if (entity == null || entity.Id != entityField.IdEntity)
                {
                    entity = entityField.Entity;
                    listEntity.Add(entity);
                }

                if (entity.Name.Equals("entityfield", StringComparison.OrdinalIgnoreCase) || entity.Name.Equals("entity", StringComparison.OrdinalIgnoreCase))
                    throw new Exception(
                        @"Ядром запрещено изменение состава полей сущностей Entity и EntityField через добавление/удаление записей в таблице ref.EntityField. Для решения данной задачи пользуйтесь ручным sql-скриптом с отключением соответствующего триггера. Не забудьте также внести изменения в Tools.MigrationHelper\PlatformDb\");

                _createColumn(entityField, entity);
                _createForeignKey(entityField, entity);

            }
            listEntity.ForEach(_updateVersioningTriggers);
        }

        /// <summary>
        /// Релизация триггера UPDATE
        /// </summary>
        public void ExecUpdateCmd(List<EntityField> inserted, List<EntityField> deleted)
        {
            if (inserted.Any(a => !_validName.IsMatch(a.Name)))
                throw new Exception("В названии поля сущности использованы недопустимые символы");

            Entity entity = null;
            List<Entity> listEntity = new List<Entity>();
            foreach (EntityField delete in deleted)
            {
                EntityField insert = inserted.SingleOrDefault(a => a.Id == delete.Id);
                if (insert == null)
                    continue;

                if (delete.IdEntity != insert.IdEntity)
                    throw new Exception("Нельзя менять идентификатор сущности у поля");

                if (_withoutColumn.Contains(insert.EntityFieldType) || _withoutColumn.Contains(delete.EntityFieldType))
                    return;
                if ((delete.IdEntityFieldType != insert.IdEntityFieldType || delete.IdCalculatedFieldType != insert.IdCalculatedFieldType) && _withoutColumn.Contains(insert.EntityFieldType) || (insert.IdCalculatedFieldType.HasValue && (insert.CalculatedFieldType == CalculatedFieldType.AppComputed || insert.CalculatedFieldType == CalculatedFieldType.NumeratorExpression || insert.CalculatedFieldType == CalculatedFieldType.ClientComputed || insert.CalculatedFieldType == CalculatedFieldType.Joined)))
                    return;

                if (entity == null || entity.Id != delete.IdEntity)
                {
                    entity = delete.Entity;// _sqlCmd.SelectFirst<Platform.PrimaryEntities.Reference.Entity>("SELECT * FROM [ref].[Entity] WHERE [id]=" + delete.IdEntity);
                    listEntity.Add(entity);
                }

                if (entity.Name.Equals("entityfield", StringComparison.OrdinalIgnoreCase) || entity.Name.Equals("entity", StringComparison.OrdinalIgnoreCase))
                {
                    if (delete.IdEntityFieldType != insert.IdEntityFieldType
						|| (delete.IdCalculatedFieldType != insert.IdCalculatedFieldType && ((delete.IdCalculatedFieldType.HasValue && (delete.CalculatedFieldType == CalculatedFieldType.DbComputed || delete.CalculatedFieldType == CalculatedFieldType.DbComputedPersisted)) || (insert.IdCalculatedFieldType.HasValue && (insert.CalculatedFieldType == CalculatedFieldType.DbComputed || insert.CalculatedFieldType == CalculatedFieldType.DbComputedPersisted))))
                        || delete.Size != insert.Size
                        || delete.Precision != insert.Precision
                        || !delete.Name.Equals(insert.Name, StringComparison.OrdinalIgnoreCase)
                        || delete.ColumnAllowNull != insert.ColumnAllowNull
						|| (((delete.IdCalculatedFieldType.HasValue && (delete.CalculatedFieldType == CalculatedFieldType.DbComputed || delete.CalculatedFieldType == CalculatedFieldType.DbComputedPersisted)) || (insert.IdCalculatedFieldType.HasValue && (insert.CalculatedFieldType == CalculatedFieldType.DbComputed || insert.CalculatedFieldType == CalculatedFieldType.DbComputedPersisted))) && !delete.Expression.Equals(insert.Expression, StringComparison.OrdinalIgnoreCase))
                        || (string.IsNullOrEmpty(delete.DefaultValue) && !string.IsNullOrEmpty(insert.DefaultValue))
                        || (!string.IsNullOrEmpty(delete.DefaultValue) && string.IsNullOrEmpty(insert.DefaultValue))
                        || (!string.IsNullOrEmpty(delete.DefaultValue) && !string.IsNullOrEmpty(insert.DefaultValue) && !delete.DefaultValue.Equals(insert.DefaultValue, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new Exception(
                            @"Ядром запрещено изменение состава полей сущностей Entity и EntityField через добавление/удаление записей в таблице ref.EntityField. Для решения данной задачи пользуйтесь ручным sql-скриптом с отключением соответствующего триггера. Не забудьте также внести изменения в Tools.MigrationHelper\PlatformDb\");
                    }
                }


                //if (entity.Name.Equals("entityfield", StringComparison.OrdinalIgnoreCase) || entity.Name.Equals("entity", StringComparison.OrdinalIgnoreCase))
                //throw new Exception(@"Ядром запрещено изменение состава полей сущностей Entity и EntityField через добавление/удаление записей в таблице ref.EntityField. Для решения данной задачи пользуйтесь ручным sql-скриптом с отключением соответствующего триггера. Не забудьте также внести изменения в Tools.MigrationHelper\PlatformDb\");

				if (!delete.Name.Equals(insert.Name, StringComparison.OrdinalIgnoreCase) && (delete.CalculatedFieldType == CalculatedFieldType.DbComputed || delete.CalculatedFieldType == CalculatedFieldType.DbComputedPersisted))
                {
                    _dropColumn(delete, entity);
                    _createColumn(insert, entity);
                }
                if (!delete.Name.Equals(insert.Name, StringComparison.OrdinalIgnoreCase) && delete.CalculatedFieldType == null && !_withoutColumn.Contains(delete.EntityFieldType) && !_withoutColumn.Contains(insert.EntityFieldType))
                {
                    _renameColumn(insert, delete, entity);
                }

                //Изменяем столбец только если размер увеличивается, чтобы не терять данные при обновлении
                if (delete.IdEntityFieldType != insert.IdEntityFieldType || ((delete.Size ?? -10) < (insert.Size ?? -10)) || (delete.Precision ?? -10) != (insert.Precision ?? -10) || delete.ColumnAllowNull != insert.ColumnAllowNull)
                {
                    switch (delete.CalculatedFieldType)
                    {
                        case CalculatedFieldType.DbComputed:
						case CalculatedFieldType.DbComputedPersisted:
                            _dropColumn(delete, entity);
                            _createColumn(insert, entity);
                            break;
                        case null:
                            _alterColumnChangeType(insert, delete, entity);
                            break;
                        default:
                            {
                                insert.AllowNull = true; //костыль
                                _createColumn(insert, entity);
                            }
                            break;
                    }

                }

                if (!string.IsNullOrEmpty(delete.DefaultValue) && !delete.DefaultValue.Equals(insert.DefaultValue, StringComparison.OrdinalIgnoreCase) && delete.IdEntityFieldType == insert.IdEntityFieldType && delete.IdCalculatedFieldType == insert.IdCalculatedFieldType)
                {
                    if (delete.FieldDefaultValueType == FieldDefaultValueType.Sql)
                    {
                        _dropDefault(insert, entity);
                    }
                    if (insert.FieldDefaultValueType == FieldDefaultValueType.Sql)
                    {
                        _createDefault(insert, entity);
                    }
                }

                if (delete.IdCalculatedFieldType != insert.IdCalculatedFieldType)
                {
                    _dropColumn(delete, entity);
                    _createColumn(insert, entity);
                }

                if (!string.IsNullOrEmpty(delete.Expression) && !delete.Expression.Equals(insert.Expression, StringComparison.OrdinalIgnoreCase))
                {
					if (insert.CalculatedFieldType == CalculatedFieldType.DbComputed || insert.CalculatedFieldType == CalculatedFieldType.DbComputedPersisted)
                    {
                        _dropColumn(delete, entity);
                        _createColumn(insert, entity);
                    }
                }
                if ((delete.IdForeignKeyType != insert.IdForeignKeyType) || (delete.IsFieldWithForeignKey() && insert.IsFieldWithForeignKey() && (delete.IdEntityLink ?? 0) != (insert.IdEntityLink ?? 0)))
                {
                    _dropForeignKey(delete, entity);
                    _createForeignKey(insert, entity);
                }
            }
            listEntity.ForEach(_updateVersioningTriggers);
        }

        /// <summary>
        /// Релизация триггера DELETE
        /// </summary>
        public void ExecDeleteCmd(List<EntityField> deleted)
        {
            Entity entity = null;
            List<Entity> listEntity = new List<Entity>();
            foreach (EntityField delete in deleted)
            {
                if (entity == null || entity.Id != delete.IdEntity)
                {
                    try
                    {
                        entity = delete.Entity;
                        // _sqlCmd.SelectFirst<Platform.PrimaryEntities.Reference.Entity>("SELECT * FROM [ref].[Entity] WHERE [id]=" + delete.IdEntity);
                        listEntity.Add(entity);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (_withoutColumn.Contains(delete.EntityFieldType) ||
                    (delete.IdCalculatedFieldType.HasValue &&
                     (delete.CalculatedFieldType == CalculatedFieldType.AppComputed ||
                      delete.CalculatedFieldType == CalculatedFieldType.NumeratorExpression ||
                      delete.CalculatedFieldType == CalculatedFieldType.ClientComputed ||
                      delete.CalculatedFieldType == CalculatedFieldType.Joined)))
                    return;

                if (entity.Name.Equals("entityfield", StringComparison.OrdinalIgnoreCase) ||
                    entity.Name.Equals("entity", StringComparison.OrdinalIgnoreCase))
                    throw new Exception(
                        @"Ядром запрещено изменение состава полей сущностей Entity и EntityField через добавление/удаление записей в таблице ref.EntityField. Для решения данной задачи пользуйтесь ручным sql-скриптом с отключением соответствующего триггера. Не забудьте также внести изменения в Tools.MigrationHelper\PlatformDb\");

                _dropDefault(delete, entity);
                _dropForeignKey(delete, entity);
                _dropPk(delete, entity);
                _dropColumn(delete, entity);
                _deleteGenericLinks(delete);
            }
            listEntity.ForEach(_updateVersioningTriggers);
        }

        #endregion
    }
}
