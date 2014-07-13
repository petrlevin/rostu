using System;
using System.Collections.Generic;
using System.Linq;
using Platform.DbClr.Interfaces;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.TriggerActions.Reference
{
    /// <summary>
    /// Дополнительный триггер для сущности Entity.
    /// Данный триггер отключается при развертывании/обновлении БД утилитой Migration Helper, 
    /// чтобы не создать повторно поле id и другие автоматически создаваемые поля при создании сущностей
    /// </summary>
    public class EntityLogicTrigger : ITriggerAction<Entity>
    {
        /// <summary>
        /// Экземпляр SqlCmd для выполненения команд
        /// </summary>
        private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

        /// <summary>
        /// Класс для формирования SQL команд
        /// </summary>
        private static class SqlTpl
        {
            /// <summary>
            /// Команда создания поля id для сущности
            /// </summary>
            internal const string InsertIdField = "insert into [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [ReadOnly], [isHidden]) values ('id', 'Идентификатор', {0}, 3, 0, 1, 1)";

            /// <summary>
            /// Команда создания поля idOwner для сущности с типом EntityType.Tablepart
            /// </summary>
            internal const string InsertIdOwnerField = "insert into [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [isSystem]) values ('idOwner', 'Ссылка на владельца', {0}, 7, 0, 1)";

            /// <summary>
            /// Команда создания булево поля isTemporary для сущности типа "Отчет"
            /// </summary>
            internal const string InsertIsTemporaryField = "insert into [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [isSystem], [isHidden], [DefaultValue], [idFieldDefaultValueType]) values ('isTemporary', 'Временный экземпляр', {0}, 1, 0, 1, 1, '1', 1)";

            internal const string InsertReportProfileFields = @"
                INSERT [ref].[EntityField] ([Name], [Caption], [Description], [idEntity], [idEntityFieldType], [idCalculatedFieldType], [Size], [Precision], [Expression], [idEntityLink], [idOwnerField], [idForeignKeyType], [AllowNull], [isCaption], [isDescription], [DefaultValue], [idFieldDefaultValueType], [isSystem], [ReadOnly], [isHidden], [RegExpValidator], [Tooltip]) VALUES (N'ReportProfileCaption', N'Имя профиля', N'', {0}, 2, NULL, 500, 0, N'', NULL, NULL, NULL, 1, 1, 0, N'', NULL, 1, 0, 0, N'', N'Наименование профиля отчета')
                INSERT [ref].[EntityField] ([Name], [Caption], [Description], [idEntity], [idEntityFieldType], [idCalculatedFieldType], [Size], [Precision], [Expression], [idEntityLink], [idOwnerField], [idForeignKeyType], [AllowNull], [isCaption], [isDescription], [DefaultValue], [idFieldDefaultValueType], [isSystem], [ReadOnly], [isHidden], [RegExpValidator], [Tooltip]) VALUES (N'idReportProfileType', N'Тип профиля', N'', {0}, 7, NULL, 0, 0, N'', -1811939077, NULL, 2, 0, 0, 0, N'1', 1, 1, 0, 0, N'', N'Тип профиля отчета (общий или личный). Общие профили доступны всем, личные - только автору.')
                INSERT [ref].[EntityField] ([Name], [Caption], [Description], [idEntity], [idEntityFieldType], [idCalculatedFieldType], [Size], [Precision], [Expression], [idEntityLink], [idOwnerField], [idForeignKeyType], [AllowNull], [isCaption], [isDescription], [DefaultValue], [idFieldDefaultValueType], [isSystem], [ReadOnly], [isHidden], [RegExpValidator], [Tooltip]) VALUES (N'idReportProfileUser', N'Автор профиля', N'', {0}, 7, NULL, 0, 0, N'', -2147483493, NULL, 2, 1, 0, 0, N'{{IdUser}}', 2, 1, 1, 0, N'', N'')
            ";

            /// <summary>
            /// Команда добавления полей ValidityFrom, ValidityTo, idRoot для сущности с признаком isVersioning==true
            /// </summary>
            internal const string InsertValidityFromFields =
                "INSERT INTO [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [isSystem], [isHidden]) " +
                "VALUES ('ValidityFrom', 'Дата начала действия', {0}, 24, 1, 1, 0)";
            internal const string InsertValidityToFields =
                "INSERT INTO [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [isSystem], [isHidden]) " +
                "VALUES ('ValidityTo', 'Дата окончания действия', {0}, 24, 1, 1, 0)";
            internal const string InsertidRootFields =
                "INSERT INTO [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [isSystem], [isHidden], [idEntityLink]) " +
                "VALUES ('idRoot', 'Cсылка на корень', {0}, 7, 1, 1, 1, {0})";
            internal const string InsertidActualItemFields =
                "INSERT INTO [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [isSystem], [isHidden], [idEntityLink], [idCalculatedFieldType]) " +
                "VALUES ('idActualItem', 'Актуальный элемент', {0}, 7, 1, 1, 1, {0}, {1})";

            internal static readonly List<EntityType> WithId = new List<EntityType> { EntityType.Report, EntityType.Reference, EntityType.Document, EntityType.Tool, EntityType.Registry, EntityType.Tablepart };

            private const string _insertOrderedField =
                "INSERT INTO [ref].[EntityField] ([Name], [Caption], [idEntity], [idEntityFieldType], [AllowNull], [isSystem], [isHidden]) " +
                "VALUES ('{0}', 'Порядок', {1}, {2}, 1, 1, 0)";

            internal const string DeleteFieldByName =
                "DELETE FROM [ref].[EntityField] WHERE [idEntity]={0} AND Name='{1}'";

            static internal string InsertOrderedField(string fieldName, int idEntity)
            {
                return string.Format(_insertOrderedField, fieldName, idEntity, (byte)EntityFieldType.Int);
            }
        }


        /// <summary>
        /// Автоматическое создание необходимых для сущности полей
        /// </summary>
        /// <param name="entity">Сущность</param>
        private void _createDefaultFields(Entity entity)
        {
            if (SqlTpl.WithId.Contains(entity.EntityType))
            {
                _sqlCmd.ExecuteNonQuery(String.Format(SqlTpl.InsertIdField, entity.Id));
            }
            if (entity.EntityType == EntityType.Tablepart)
            {
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.InsertIdOwnerField, entity.Id));
            }
            if (entity.EntityType == EntityType.Report)
            {
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.InsertReportProfileFields, entity.Id));
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.InsertIsTemporaryField, entity.Id));
            }
        }

        /// <summary>
        /// Создание версионного триггера
        /// </summary>
        /// <param name="entity">Сущность</param>
        private void _createVersioningTrigger(Entity entity)
        {
            List<int> listIdUniqueIndexes =
                _sqlCmd.SelectOneColumn<int>(string.Format("SELECT [id] FROM [ref].[Index] WHERE [idEntity]={0} AND idIndexType={1}",
                                                          entity.Id, (byte)IndexType.UniqueIndex));
            foreach (int idIndex in listIdUniqueIndexes)
            {
                _sqlCmd.ExecuteNonQuery(string.Format("EXECUTE [dbo].[CreateOrAlterVersioningTrigger] {0}", idIndex));
            }
        }

        /// <summary>
        /// Автоматическое создание полей для версионной сущности
        /// </summary>
        /// <param name="entity">Сущность</param>
        private void _setIsVersioning(Entity entity)
        {
            if (entity.IsVersioning)
            {
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.InsertValidityFromFields, entity.Id));
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.InsertValidityToFields, entity.Id));
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.InsertidRootFields, entity.Id));
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.InsertidActualItemFields, entity.Id, (byte)CalculatedFieldType.AppComputed));
                _createVersioningTrigger(entity);
            }
        }

        private void _applyOrderedFlagForMultilinkEntity(Entity entity, bool ordered)
        {
            // лямбда на добавление поля
            Action<EntityField> add = entityField =>
            {
                string orderFieldName = entityField.Name + "Order";
                int count =
                    _sqlCmd.ExecuteScalar<int>(
                        string.Format(
                            "select COUNT(1) from [ref].[EntityField] WHERE [idEntity]={0} AND [idEntityFieldType]=3 AND Name<>'{1}'",
                            entity.Id, orderFieldName));
                if (count == 0)
                {
                    _sqlCmd.ExecuteNonQuery(SqlTpl.InsertOrderedField(orderFieldName, entity.Id));
                }
            };

            // лямбда на удаление поля
            Action<EntityField> delete = entityField =>
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DeleteFieldByName, entity.Id, entityField.Name + "Order"));

            List<EntityField> linkfields = _sqlCmd.Select<EntityField>("SELECT * FROM [ref].[EntityField] WHERE [idEntity]=" + entity.Id + " AND [idEntityFieldType]=7");
            linkfields.ForEach(ordered ? add : delete);
        }

        private void _applyOrderedFlagForReference(Entity entity, bool ordered)
        {
            const string orderFieldName = "Order";
            if (ordered)
                _sqlCmd.ExecuteNonQuery(SqlTpl.InsertOrderedField(orderFieldName, entity.Id));
            else
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DeleteFieldByName, entity.Id, orderFieldName));
        }

        private void _applyOrderedFlag(Entity entity, bool ordered)
        {
            switch (entity.EntityType)
            {
                case EntityType.Multilink:
                    _applyOrderedFlagForMultilinkEntity(entity, ordered);
                    return;
                default:
                    _applyOrderedFlagForReference(entity, ordered);
                    break;
            }
        }

        /// <summary>
        /// Создание триггеров для поддержки ссылочной целостности "Общих ссылок"
        /// </summary>
        /// <param name="entity">Сущность</param>
        private void _createGenericLinkTriggers(Entity entity)
        {
            _sqlCmd.ExecuteNonQuery("EXEC [dbo].[CreateOrAlterReferencedGenericLinksTrigger] " + entity.Id);
            _sqlCmd.ExecuteNonQuery("EXEC [dbo].[CreateOrAlterReferencesGenericLinksTrigger] " + entity.Id);
        }

        private void _createCheckCycleHierarchy(Entity entity)
        {
        }


        private void _changeGenericLinkConstraints()
        {
            _sqlCmd.ExecuteNonQuery(@"
                IF (select @@TRANCOUNT)=0
                BEGIN
                    BEGIN TRANSACTION
                        EXEC  [dbo].[DropEntityFieldConstraintsForReferenceLinks]
                        EXEC  [dbo].[CreateOrAlterAllowGenericLinks]
                        EXEC  [dbo].[CreateOrEnableEntityFieldConstraintsForReferenceLinks]
                    COMMIT TRANSACTION
                END
                ELSE
                BEGIN
                        EXEC  [dbo].[DropEntityFieldConstraintsForReferenceLinks]
                        EXEC  [dbo].[CreateOrAlterAllowGenericLinks]
                        EXEC  [dbo].[CreateOrEnableEntityFieldConstraintsForReferenceLinks]
                END
                ");
        }


        #region Implementation of ITriggerAction<Entity>

        /// <summary>
        /// Релизация триггера INSERT
        /// </summary>
        public void ExecInsertCmd(List<Entity> inserted)
        {
            foreach (Entity insert in inserted)
            {
                if ((insert.EntityType == EntityType.Document || insert.EntityType == EntityType.Tool) && (!insert.AllowGenericLinks))
                    _sqlCmd.ExecuteNonQuery(string.Format("UPDATE [ref].[Entity] SET AllowGenericLinks=1 WHERE [id]={0}", insert.Id));
                _createDefaultFields(insert);
                _setIsVersioning(insert);
                _applyOrderedFlag(insert, insert.Ordered ?? false);
                _createGenericLinkTriggers(insert);
                _createCheckCycleHierarchy(insert);
            }

            if (inserted.Any(i=>i.AllowGenericLinks))
                _changeGenericLinkConstraints();
        }

        /// <summary>
        /// Релизация триггера UPDATE
        /// </summary>
        public void ExecUpdateCmd(List<Entity> inserted, List<Entity> deleted)
        {
            foreach (Entity delete in deleted)
            {
                Entity insert = inserted.SingleOrDefault(a => a.Id == delete.Id);
                if (insert == null)
                    continue;

                if (!insert.IsVersioning && delete.IsVersioning)
                    throw new Exception("Отключение версионности не реализовано");
                if (insert.IsVersioning && !delete.IsVersioning)
                    _setIsVersioning(insert);

                if (insert.Ordered != delete.Ordered)
                    _applyOrderedFlag(insert, insert.Ordered ?? false);
                if (!insert.AllowGenericLinks && delete.AllowGenericLinks)
                    throw new Exception("Отключение признака AllowGenericLinks невозможно");
                if (insert.AllowGenericLinks && (!delete.AllowGenericLinks))
                    _createGenericLinkTriggers(insert);
            }
            if (inserted.Any(i => i.AllowGenericLinks!= deleted.Single(d=>d.Id==i.Id).AllowGenericLinks))
                _changeGenericLinkConstraints();
        }

        /// <summary>
        /// Релизация триггера DELETE
        /// </summary>
        public void ExecDeleteCmd(List<Entity> deleted)
        {
            if (deleted.Any(d=>d.AllowGenericLinks))
                _changeGenericLinkConstraints();
        }

        #endregion
    }
}
