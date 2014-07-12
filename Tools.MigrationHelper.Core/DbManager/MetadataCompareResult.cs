using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using NAnt.Core;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Tools.MigrationHelper.Core.DbManager.DbActions;
using Tools.MigrationHelper.Core.DbManager.DbActions.Interfaces;
using Tools.MigrationHelper.Core.DbManager.Extensions;
using Tools.MigrationHelper.Extensions;
using Tools.MigrationHelper.Helpers;
using Metadata = Tools.MigrationHelper.Core.DbManager.DbDataSet;

namespace Tools.MigrationHelper.Core.DbManager
{
    /// <summary>
    /// Результатом является внутренняя коллекция <seealso cref="OrderedActions"/>.
    /// </summary>
    public class MetadataCompareResult
    {
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Исходная (эталонная) ОММ, к состоянию которой необходимо привести <seealso cref="Target"/>.</param>
        /// <param name="targetDb">Целевая ОММ (на которую нацелен процесс обновления), которую необходимо привести в состояние полученная из БД</param>
        /// <param name="targetFs">Целевая ОММ (на которую нацелен процесс обновления), которую необходимо привести в состояние полученная из файлов поставки</param>
        /// <param name="finalState">ОММ к которой в итоге нескольких циклов обновления мы должны прийти</param>
        /// <param name="connection"></param>
        public MetadataCompareResult(DbDataSet source, DbDataSet targetDb, DbDataSet targetFs, DbDataSet finalState,
                                     SqlConnection connection)
        {
            _connection = connection;
            TargetDb = targetDb;
            TargetFs = targetFs;
            Source = source;
            FinalState = finalState;
            _entities = Source.DataSet.Tables[Names.RefEntity].AsEnumerable().ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Исходная (эталонная) ОММ, к состоянию которой необходимо привести <seealso cref="Target"/>.</param>
        /// <param name="targetDb">Целевая ОММ (на которую нацелен процесс обновления), которую необходимо привести в состояние полученная из БД</param>
        /// <param name="targetFs">Целевая ОММ (на которую нацелен процесс обновления), которую необходимо привести в состояние полученная из файлов поставки</param>
        /// <param name="connection"></param>
        public MetadataCompareResult(DbDataSet source, DbDataSet targetDb, DbDataSet targetFs, SqlConnection connection)
        {
            _connection = connection;
            TargetDb = targetDb;
            TargetFs = targetFs;
            Source = source;
            _entities = Source.DataSet.Tables[Names.RefEntity].AsEnumerable().ToList();
            FinalState = source;
        }
        #endregion

        #region Private Members

        /// <summary>
        /// Неупорядоченный список действий, необходимых для приведения <seealso cref="Target"/> к состоянию <seealso cref="Source"/>.
        /// </summary>
        private List<IDbActionBatch> _actionBatches;
        private LinkedList<DbAction> _actionList;
        private readonly List<DataRow> _entities;
        private readonly SqlConnection _connection;

        #endregion


        #region Public Members

        /// <summary>
        /// Целевая ОММ (на которую нацелен процесс обновления), которую необходимо привести в состояние <seealso cref="Source"/>.
        /// </summary>
        public DbDataSet TargetDb { get; private set; }

        /// <summary>
        /// Целевая ОММ, полученная из файлов поставки
        /// </summary>
        public DbDataSet TargetFs { get; private set; }

        /// <summary>
        /// Состояние к которому в итоге должны прийти
        /// </summary>
        public DbDataSet FinalState { get; private set; }

        /// <summary>
        /// Исходная (эталонная) ОММ, к состоянию которой необходимо привести <seealso cref="TargetDb"/> с учетом <seealso cref="TargetFs"/>.
        /// </summary>
        public DbDataSet Source { get; private set; }

        /// <summary>
        /// Упорядоченный список действий, необходимых для приведения <seealso cref="Target"/> к состоянию <seealso cref="Source"/>.
        /// </summary>
        /// <remarks>
        /// * Удаление сущностей
        /// * Обновление сущностей
        /// * Создание сущностей
        /// * Ссоздание полей и udf/sp/view.
        /// * Обновление полей
        /// * Удаление полей
        /// </remarks>
        public List<Dictionary<int, List<SqlCommand>>> OrderedCommands;

        /// <summary>
        /// Сравнивает две ОММ: <seealso cref="Source"/>, <seealso cref="TargetDb"/> и <seealso cref="TargetFs"/>.
        /// Результатом является заполненная коллекция <seealso cref="OrderedActions"/>.
        /// </summary>
        public void Compare()
        {
            _actionBatches = new List<IDbActionBatch>();
            CheckMetadataTables();
            var eef = new List<string> { Names.RefEntity, Names.RefEntityField };
            var eefIds = Source.DataSet.Tables[Names.RefEntity]
                .AsEnumerable().Where(w => (new List<string> {Names.Entity, Names.EntityField}).Contains(
                    w.Field<string>(Names.Name))).Select(s => s.Field<int>(Names.Id));


            Predicate<DataRow> ids = row => row.Field<string>(Names.Name) == Names.Id;
            Predicate<DataRow> notIdsNotDbCalculated = row => row.Field<string>(Names.Name) != Names.Id && (row.Field<byte?>(Names.IdCalculatedFieldType) != (byte)CalculatedFieldType.DbComputed && row.Field<byte?>(Names.IdCalculatedFieldType) != (byte)CalculatedFieldType.DbComputedPersisted);
            Predicate<DataRow> notIdsDbCalculated = row => row.Field<string>(Names.Name) != Names.Id && (row.Field<byte?>(Names.IdCalculatedFieldType) == (byte)CalculatedFieldType.DbComputed || row.Field<byte?>(Names.IdCalculatedFieldType) == (byte)CalculatedFieldType.DbComputedPersisted);

            List<int> orderInsertEntity = DbDataSetCompareHelper.OrderEntityForInsert(Source.DataSet);
            List<int> orderDeleteEntity = DbDataSetCompareHelper.OrderEntityForDelete(Source.DataSet);

            if (TargetFs == null)
            {
                // Сущности
                _actionBatches.Add(GetInsertAction(Source.DataSet.Tables[Names.RefEntity],
                                             TargetDb.DataSet.Tables[Names.RefEntity]).GetInsert());
                // Поля с именем "id"
                _actionBatches.Add(GetInsertAction(Source.DataSet.Tables[Names.RefEntityField],
                                             TargetDb.DataSet.Tables[Names.RefEntityField], ids).GetInsert());
                // Остальные поля
                _actionBatches.Add(GetInsertAction(Source.DataSet.Tables[Names.RefEntityField],
                                             TargetDb.DataSet.Tables[Names.RefEntityField], notIdsNotDbCalculated).GetInsert());
                _actionBatches.Add(GetInsertAction(Source.DataSet.Tables[Names.RefEntityField],
                                             TargetDb.DataSet.Tables[Names.RefEntityField], notIdsDbCalculated).GetInsert());
                
                // Элементы остальных сущностей
                _actionBatches.AddRange(InsertActions(orderInsertEntity, eef));
            }
            else
            {
                //удаляем в первую очередь фильтры и индексы, они могут ссылаться на поля или сущности
                var deleteFirst = new List<int>
                    {
                        -2147483602, //элементы формы
                        -2147483603, //формы
                        -2080374748, //фильтры
                        -2013265751, //индексы
                        -1811939300 //настройки сущности
                    };

                //список ид сущностей которые вставляем в первую очередь, на них ссылаются элементы справочника Сущность и Поля сущности
                var insertFirst =
                    Source.DataSet.Tables[Names.RefEntityField].AsEnumerable()
                                                               .Where(w => eefIds.Contains(w.Field<int>(Names.IdEntity))
                                                                      && !eefIds.Contains(w.Field<int>(Names.IdEntityLink))// исключаем ссылки между ref.Entity и ref.EntityField
                                                                      && w.Field<byte>(Names.IdEntityFieldType) == (byte) EntityFieldType.Link)
                                                               .Select(s => s.Field<int>(Names.IdEntityLink)).ToList();

                var action1 = DeleteActions(deleteFirst);
                if(action1 != null)
                    _actionBatches.AddRange(action1);

                var action2 = InsertActions(insertFirst);
                if (action2 != null)
                    _actionBatches.AddRange(action2);

                //ид мультилинков
                List<int?> mlEntities =
                    TargetFs.DataSet.Tables[Names.RefEntity]
                        .AsEnumerable()
                        .Where(w => (byte) w[Names.IdEntityType] == (byte) EntityType.Multilink)
                        .Select(s => (int?) s[Names.Id])
                        .ToList();

                //(операция 1)Получаем сущности которые есть в TargetFs и отсутствуют в Source, для удаления
                List<DataRow> deleteEntity =
                    TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
                        rowFs =>
                        Source.DataSet.Tables[Names.RefEntity].AsEnumerable().All(
                            rowSrc => rowSrc.Field<int>(Names.Id) != rowFs.Field<int>(Names.Id))
                        && FinalState.DataSet.Tables[Names.RefEntity].AsEnumerable().All(a => !DbDataSetCompareHelper.IsEqualId(a[Names.Id], rowFs[Names.Id]))
                        ).ToList();

                //список полей которые ссылаются на сущности которые удаляются
                List<DataRow> deleteLinkEntityField =
                    TargetDb.DataSet.Tables[Names.RefEntityField].
                        AsEnumerable().
                        Where(rowFs =>
                              deleteEntity.Select(k => (int?) k[Names.Id])
                                          .Contains(DbDataSetCompareHelper.ToNullableInt32(rowFs[Names.IdEntityLink].ToString()))
                              && !mlEntities.Contains(DbDataSetCompareHelper.ToNullableInt32(rowFs[Names.IdEntity].ToString())))
                                                                 .ToList();

                if (deleteLinkEntityField.Any())
                    _actionBatches.Add(deleteLinkEntityField.GetDelete());

                if (deleteEntity.Any())
                {
                    //сначала удаляем мультилинки, потом все остальные(потому что мультилинки ссылаются, а поля у них удалить нельзя)
                    _actionBatches.Add(deleteEntity.Where(w => (byte)w[Names.IdEntityType] == (byte)EntityType.Multilink).GetDelete());
                    _actionBatches.Add(deleteEntity.Where(w => (byte)w[Names.IdEntityType] != (byte)EntityType.Multilink).GetDelete());
                }

                //список полей которые отсутсвуют в TargetFs
                List<DataRow> deleteEntityField =
                    TargetFs.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
                        rowFs =>
                        Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().All(
                            rowSrc => rowSrc.Field<int>(Names.Id) != rowFs.Field<int>(Names.Id))
                        && FinalState.DataSet.Tables[Names.RefEntityField].AsEnumerable().All(a => !DbDataSetCompareHelper.IsEqualId(a[Names.Id], rowFs[Names.Id]))
                        ).ToList();

                if (deleteEntityField.Any())
                {
                    _actionBatches.Add(deleteEntityField.GetDelete());
                    DeleteFieldsFromDataSet(deleteEntityField);
                }

                //(операция 3)новые сущности
                List<DataRow> newEntity = Source.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
                    rowSrc =>
                    TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().All(
                        rowFs => rowFs.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))
                    &&
                    TargetDb.DataSet.Tables[Names.RefEntity].AsEnumerable().All(
                        rowDb => rowDb.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))).
                                                                                 ToList();
                if (newEntity.Any())
                    _actionBatches.Add(newEntity.GetInsert());

                //(операция 3)Добавить поля id для новых сущностей
                List<DataRow> newEntityFieldId =
                    Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
                        rowSrc => rowSrc.Field<string>(Names.Name) == Names.Id &&
                                  newEntity.Select(a => a.Field<int>(Names.Id))
                                           .Contains(rowSrc.Field<int>(Names.IdEntity))).ToList();
                if (newEntityFieldId.Any())
                    _actionBatches.Add(newEntityFieldId.GetInsert());

                //(операция 4)Добавить поля, имя которых не равно id
                List<DataRow> newEntityFieldNotId =
                    Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
                        rowSrc => rowSrc.Field<string>(Names.Name) != Names.Id &&
                                  TargetFs.DataSet.Tables[Names.RefEntityField].AsEnumerable().All(
                                      rowFs => rowFs.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))
                                  &&
                                  TargetDb.DataSet.Tables[Names.RefEntityField].AsEnumerable().All(
                                      rowDb => rowDb.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))).ToList();
                if (newEntityFieldNotId.Any())
                {
                    List<string> fieldsName = (from DataColumn c in TargetDb.DataSet.Tables[Names.RefEntityField].Columns
                                       where c.ColumnName == Names.Id || (c.ColumnName != Names.Tstamp && !c.ReadOnly)
                                       select c.ColumnName).ToList();
                    _actionBatches.Add(newEntityFieldNotId.GetInsert(fieldsName));
                    InsertFieldsToDataSet(newEntityFieldNotId);
                }


                //(операция 5)Получаем сущности которые есть в Source и в TargetFs чтобы проверить в дальнейшем, были ли изменения
                List<DataRow> updateEntity =
                    Source.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
                        rowSrc =>
                        TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().Any(
                            rowFs => rowFs.Field<int>(Names.Id) == rowSrc.Field<int>(Names.Id))).
                                                           ToList();
                if (updateEntity.Any())
                    _actionBatches.Add(updateEntity.GetUpdate(
                        TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
                            a => updateEntity.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id))),
                        TargetDb.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
                            a => updateEntity.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id))),
                        FinalState.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
                            a => updateEntity.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id)))));

                //(операция 6)Получаем поля сущностей которые есть в Source и в TargetFs чтобы проверить в дальнейшем, были ли изменения
                List<DataRow> updateEntityField = Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
                    rowSrc =>
                    TargetFs.DataSet.Tables[Names.RefEntityField].AsEnumerable().Any(
                        rowFs => rowFs.Field<int>(Names.Id) == rowSrc.Field<int>(Names.Id))).
                                                                                              ToList();
                if (updateEntityField.Any())
                    _actionBatches.Add(updateEntityField.GetUpdate(
                        TargetFs.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
                            a => updateEntityField.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id))),
                        TargetDb.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
                            a => updateEntityField.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id))),
                        FinalState.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
                            a => updateEntityField.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id)))));

                //Все сущности кроме тех что уже создали в insertFirst и eef
                _actionBatches.AddRange(InsertActions(orderInsertEntity, eef, insertFirst));
                _actionBatches.AddRange(UpdateActions(orderInsertEntity, eef));
                _actionBatches.AddRange(DeleteActions(orderDeleteEntity, eef));
				
			}

            FillDependencies();
            OrderedCommands = new List<Dictionary<int, List<SqlCommand>>>();
            GetOrderedCommands();
        }

        /// <summary>
        /// Выполняет действия <see cref="OrderedActions"/> над указанной БД
        /// </summary>
        public void Execute()
        {
            const string errTpl = "Произошла ошибка при выполнении команды.{0}Очередь №{1}, действие №{2}, комманда №{3}.{0}Sql:\n{4}{0}{0}Текст ошибки:{0}{5}";
            int i = 0;
            foreach (var list in OrderedCommands)
            {
                ++i;
                int j = 0;
                foreach (var action in list.OrderBy(o=> o.Key))
                {
                    ++j;
                    int k = 0;
                    if(action.Value == null) continue;

                    foreach (var command in action.Value)
                    {
                        ++k;
                        command.Connection = _connection;
                        command.CommandTimeout = _connection.ConnectionTimeout;
                        try
                        {
                            Debug.WriteLine(command.AsString() + Environment.NewLine + "===");
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            if (ex is SqlException || ex is SqlTypeException)
                            {
                                var msg = string.Format(errTpl, Environment.NewLine, i, j, k, command.AsString(), ex.Message);
                                throw new Exception(msg, ex);
                            }
                            throw;
                        }
                    }
                }
            }
        }

        #endregion


        #region Private Methods

        private void InsertFieldsToDataSet(IEnumerable<DataRow> newEntityFieldNotId)
        {
            foreach (var dataRow in newEntityFieldNotId)
            {
                var columnName = dataRow[Names.Name].ToString();
                var tableName = GetTableNameByEntityId((int)dataRow[Names.IdEntity]);
                if(tableName == null)
                    continue;
                var fieldType = (byte)dataRow[Names.IdEntityFieldType];

                byte? calcType = null;
                byte outt;
                var result = byte.TryParse(dataRow[Names.IdCalculatedFieldType].ToString(), out outt);

                if (result)
                    calcType = outt;


                //поля которых нет физически в базе пропускаем
                if (fieldType == (byte)EntityFieldType.Multilink || fieldType == (byte)EntityFieldType.Tablepart || fieldType == (byte)EntityFieldType.VirtualTablePart || calcType == (byte)CalculatedFieldType.Joined || calcType == (byte)CalculatedFieldType.ClientComputed)
                    continue;

                if (TargetDb.DataSet.Tables[tableName] != null && Source.DataSet.Tables[tableName] != null)
					if (!TargetDb.DataSet.Tables[tableName].Columns.Contains(columnName) && Source.DataSet.Tables[tableName].Columns[columnName] != null)
                        Source.DataSet.Tables[tableName].Columns[columnName].CopyTo(TargetDb.DataSet.Tables[tableName]);

                if (TargetFs.DataSet.Tables[tableName] != null && Source.DataSet.Tables[tableName] != null)
					if (!TargetFs.DataSet.Tables[tableName].Columns.Contains(columnName) && Source.DataSet.Tables[tableName].Columns[columnName] != null)
                        Source.DataSet.Tables[tableName].Columns[columnName].CopyTo(TargetFs.DataSet.Tables[tableName]);
            }
        }

        private void DeleteFieldsFromDataSet(IEnumerable<DataRow> entityFieldNotId)
        {
            foreach (var dataRow in entityFieldNotId)
            {
                var columnName = dataRow[Names.Name].ToString();
                var tableName = GetTableNameByEntityId((int)dataRow[Names.IdEntity]);
                if (tableName == null)
                    continue;
                var fieldType = (byte)dataRow[Names.IdEntityFieldType];

                byte? calcType = null;
                byte outt;
                var result = byte.TryParse(dataRow[Names.IdCalculatedFieldType].ToString(), out outt);

                if (result)
                    calcType = outt;

                //поля которых нет физически в базе пропускаем
                if (fieldType == (byte)EntityFieldType.Multilink || fieldType == (byte)EntityFieldType.Tablepart || fieldType == (byte)EntityFieldType.VirtualTablePart || calcType == (byte)CalculatedFieldType.Joined || calcType == (byte)CalculatedFieldType.ClientComputed)
                    continue;

                if (TargetDb.DataSet.Tables[tableName] != null)
                    if (TargetDb.DataSet.Tables[tableName].Columns.Contains(columnName))
                        if (TargetDb.DataSet.Tables[tableName].Columns.CanRemove(
                            TargetDb.DataSet.Tables[tableName].Columns[columnName]))
                        {
                            TargetDb.DataSet.Tables[tableName].Columns.Remove(columnName);
                        }

                if (TargetFs.DataSet.Tables[tableName] != null)
                    if (TargetFs.DataSet.Tables[tableName].Columns.Contains(columnName))
                        if (TargetFs.DataSet.Tables[tableName].Columns.CanRemove(
                            TargetFs.DataSet.Tables[tableName].Columns[columnName]))
                        {
                            TargetFs.DataSet.Tables[tableName].Columns.Remove(columnName);
                        }
            }
        }

        /// <summary>
        /// У сравниваемых ООМ должны быть таблицы Entity и EntityField
        /// </summary>
        private void CheckMetadataTables()
        {
            const string errMesg = "ОММ {0} не содержит таблицы {1}.";
            if (Source.DataSet.Tables.IndexOf("ref.Entity") < 0)
            {
                throw new Exception(string.Format(errMesg, "Source", Names.Entity));
            }
            if (TargetDb.DataSet.Tables.IndexOf("ref.Entity") < 0)
            {
                throw new Exception(string.Format(errMesg, "Target", Names.Entity));
            }
            if (Source.DataSet.Tables.IndexOf("ref.EntityField") < 0)
            {
                throw new Exception(string.Format(errMesg, "Source", Names.EntityField));
            }
            if (TargetDb.DataSet.Tables.IndexOf("ref.EntityField") < 0)
            {
                throw new Exception(string.Format(errMesg, "Target", Names.EntityField));
            }
        }
		
		/// <summary>
        /// Для всех таблиц из <see cref="Source"/> (кроме тех, что не удовлетворяют <param name="filter">фильтру</param>)
        /// создает действие на создание insert строк, отсутствующих в <see cref="TargetDb"/>.
        /// Если таблица полностью отсутствует в <see cref="TargetDb"/>, то ее структура будет скопирована из <see cref="Source"/>.
        /// </summary>
        private IEnumerable<IDbActionBatch> InsertActions(IEnumerable<int> orderEntity, List<string> exclidedTable = null, List<int> notInsert = null)
        {
            var result = new List<IDbActionBatch>();

            foreach (int idEntity in orderEntity)
            {
                if (notInsert != null && notInsert.Contains(idEntity))
                    continue;

                DataRow rowEntity =
                    Source.DataSet.Tables[Names.RefEntity].AsEnumerable().Single(a => a.Field<int>(Names.Id) == idEntity);
                bool isHierarchyEntity =
                    Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Any(
                        a =>
                        a.Field<int>(Names.IdEntity) == idEntity && a.Field<int?>(Names.IdEntityLink).HasValue &&
                        a.Field<int>(Names.IdEntityLink) == idEntity);
                string tableName = Schemas.ByEntityType((EntityType)rowEntity.Field<byte>(Names.IdEntityType)) + "." +
                                   rowEntity.Field<string>(Names.Name);
                DataTable srcTable = Source.DataSet.Tables[tableName];
                DataTable finalStateTable = FinalState.DataSet.Tables[tableName];
                if (srcTable != null && finalStateTable != null && (exclidedTable == null || !exclidedTable.Contains(tableName)))
                {
					if ((!TargetDb.DataSet.Tables.Contains(srcTable.TableName) && TargetFs == null) || (!TargetDb.DataSet.Tables.Contains(srcTable.TableName) && TargetFs != null && !TargetFs.DataSet.Tables.Contains(srcTable.TableName))) // целиком отсутствующие таблицы
                    {
                        TargetDb.DataSet.Tables.Add(srcTable.Clone());
                    }
                    if (TargetFs != null && !TargetFs.DataSet.Tables.Contains(srcTable.TableName))
                        TargetFs.DataSet.Tables.Add(srcTable.Clone());
                    if (srcTable.Rows.Count > 0)
                    {
	                    Predicate<DataRow> notExistInFs = row => true;
						if (TargetFs != null)
	                    {
		                    if (srcTable.TableName.StartsWith("enm."))
			                    notExistInFs =
				                    row =>
				                    TargetFs.DataSet.Tables[srcTable.TableName].AsEnumerable().All(
					                    a => Convert.ToInt32(a[Names.Id]) != Convert.ToInt32(row[Names.Id]));
                            else if (srcTable.TableName.StartsWith("ml."))
		                    {
                                var exp = Names.Name + "= '" + TargetFs.DataSet.Tables[srcTable.TableName].TableName.Split('.')[1] + "'";
		                        DataRow entity = Source.DataSet.Tables[Names.RefEntity].Select(exp)[0];
		                        var fields =
		                            Source.DataSet.Tables[Names.RefEntityField].Select(Names.IdEntity + "=" + entity[Names.Id] +
		                                                                               " AND " + Names.IdEntityFieldType + "=" +
		                                                                               (int) EntityFieldType.Link);
		                        var column1 = (string) fields[0][Names.Name];
		                        var column2 = (string) fields[1][Names.Name];
                                notExistInFs =
		                            row =>
                                    TargetFs.DataSet.Tables[srcTable.TableName].Select(string.Format("[{0}]='{1}' AND [{2}]='{3}'", column1, row[column1], column2,
                                                                row[column2])).Length == 0;
		                    }
		                    else
		                    {
		                        notExistInFs =
		                            row => TargetFs.DataSet.Tables[srcTable.TableName] != null && TargetFs.DataSet.Tables[srcTable.TableName].Rows.Find(row[Names.Id]) == null;
		                    }
	                    }

                        var dataRows = isHierarchyEntity
                                               ? GetOrderHierarcyRowsForInsert(idEntity, srcTable,
                                                                                TargetDb.DataSet.Tables[
                                                                                    srcTable.TableName],
                                                                                null, notExistInFs)
                                               : GetInsertAction(srcTable, TargetDb.DataSet.Tables[srcTable.TableName],notExistInFs);

                        
                        if(dataRows.Any())
                            result.Add(dataRows.GetInsert());
                    }
                }
            }
		    return result;
        }

		/// <summary>
		/// Возвращает список действий DELETE для таблиц. При построении учитывается Source, TargetFs и TargetDb
		/// </summary>
		/// <param name="orderEntity">Порядок сущностей</param>
		/// <param name="exclidedTable">Исключаемые таблицы</param>
		/// <returns></returns>
        private IEnumerable<IDbActionBatch> DeleteActions(IEnumerable<int> orderEntity, List<string> exclidedTable = null)
		{
            var result = new List<IDbActionBatch>();
			foreach (int idEntity in orderEntity)
			{
			    bool isHierarchyEntity =
			        Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Any(
			            a =>
			            a.Field<int>(Names.IdEntity) == idEntity && a.Field<int?>(Names.IdEntityLink).HasValue &&
			            a.Field<int>(Names.IdEntityLink) == idEntity);
				DataRow rowEntity = Source.DataSet.Tables[Names.RefEntity].AsEnumerable().Single(a => a.Field<int>(Names.Id) == idEntity);
				if (rowEntity != null)
				{
					string tableName = Schemas.ByEntityType(rowEntity.Field<byte>(Names.IdEntityType)) + "." +
					                   rowEntity.Field<string>(Names.Name);
					DataTable srcTable = Source.DataSet.Tables[tableName];
                    var deleteRows = new List<DataRow>();
					if (srcTable != null && (exclidedTable == null || !exclidedTable.Contains(tableName)))
					{
					    if (tableName.StartsWith("ml."))
					    {
                            var exp = Names.Name + "= '" + tableName.Split('.')[1] + "'";
                            DataRow entity = Source.DataSet.Tables[Names.RefEntity].Select(exp)[0];
                            var fields = Source.DataSet.Tables[Names.RefEntityField].Select(Names.IdEntity + "=" + entity[Names.Id] + " AND " + Names.IdEntityFieldType + "=" + (int)EntityFieldType.Link);
                            var column1 = (string)fields[0][Names.Name];
                            var column2 = (string)fields[1][Names.Name];
					        if (TargetFs.DataSet.Tables[tableName] != null)
					        {
                                deleteRows = 
                                TargetFs.DataSet.Tables[tableName]
                                .AsEnumerable().Where(rowFs => !Source.DataSet.Tables[tableName]
                                    .AsEnumerable().Any(rowSrc =>
                                        DbDataSetCompareHelper.IsEqualId(rowSrc[column1], rowFs[column1])
                                        && DbDataSetCompareHelper.IsEqualId(rowSrc[column2], rowFs[column2])
                                        )).ToList();
					        }
					    }
					    else
					    {
					        deleteRows = TargetFs.DataSet.Tables[tableName] == null
					                         ? deleteRows
					                         : TargetFs.DataSet.Tables[tableName]
					                               .AsEnumerable().Where(rowFs => Source.DataSet.Tables[tableName]
					                                                                  .AsEnumerable()
					                                                                  .All(
					                                                                      rowSrc =>
                                                                                          !DbDataSetCompareHelper.IsEqualId(rowSrc[Names.Id], rowFs[Names.Id])) &&
					                                                              FinalState.DataSet.Tables[tableName]
					                                                                  .AsEnumerable()
					                                                                  .All(
					                                                                      rowSrc =>
                                                                                          !DbDataSetCompareHelper.IsEqualId(rowSrc[Names.Id], rowFs[Names.Id])))
					                               .ToList();
					    }
					}
				    if (srcTable == null)
				    {
                            deleteRows = TargetFs.DataSet.Tables[tableName] == null ? deleteRows :
                                TargetFs.DataSet.Tables[tableName]
                                .AsEnumerable().ToList();

                        
				    }
				    var middleResult = isHierarchyEntity ? GetOrderHierarcyRowsForDelete(idEntity,deleteRows).ToList() : deleteRows;
                    if (middleResult.Any())
                        result.Add(middleResult.GetDelete());
				}
			}
		    return result;
		}

	    /// <summary>
		/// Возвращает список действий UPDATE для таблиц. При построении учитывается Source, TargetFs и TargetDb
		/// </summary>
		/// <param name="orderEntity">Порядок сущностей</param>
		/// <param name="exclidedTable">Исключаемые таблицы</param>
		/// <returns></returns>
        private IEnumerable<IDbActionBatch> UpdateActions(IEnumerable<int> orderEntity, List<string> exclidedTable)
		{
            var result = new List<IDbActionBatch>();

			foreach (int idEntity in orderEntity)
			{
				DataRow rowEntity =
					Source.DataSet.Tables[Names.RefEntity].AsEnumerable().Single(a => a.Field<int>(Names.Id) == idEntity);

				string tableName = Schemas.ByEntityType(rowEntity.Field<byte>(Names.IdEntityType)) + "." +
				                   rowEntity.Field<string>(Names.Name);

				DataTable srcTable = Source.DataSet.Tables[tableName];

				if (srcTable != null && !exclidedTable.Contains(tableName))
				{
                    var targetFsRows = new List<DataRow>();
                    List<DataRow> targetDbRows;
                    List<DataRow> finalStateRows;
					List<DataRow> updateRows;

                    if (TargetFs.DataSet.Tables[tableName] == null || TargetDb.DataSet.Tables[tableName] == null)
                        continue;

				    if (tableName.StartsWith("ml."))
				    {
                        if(srcTable.Columns.Count < 3)
                            continue;
				        var exp = Names.Name + "= '" + tableName.Split('.')[1] + "'";
				        DataRow entity = Source.DataSet.Tables[Names.RefEntity].Select(exp)[0];
				        var fields =
				            Source.DataSet.Tables[Names.RefEntityField].Select(Names.IdEntity + "=" + entity[Names.Id] + " AND " +
				                                                               Names.IdEntityFieldType + "=" +
				                                                               (int) EntityFieldType.Link);
				        var column1 = (string) fields[0][Names.Name];
				        var column2 = (string) fields[1][Names.Name];

				        updateRows = srcTable.AsEnumerable().Where(rowSrc =>
				                                                   TargetFs.DataSet.Tables[tableName].AsEnumerable()
				                                                                                     .Any(
				                                                                                         rowFs =>
				                                                                                         DbDataSetCompareHelper.IsEqualId(rowFs[column1],
				                                                                                                   rowSrc[column1])
				                                                                                         &&
                                                                                                         DbDataSetCompareHelper.IsEqualId(rowFs[column2],
				                                                                                                   rowSrc[column2])
				                                                       ) &&
				                                                   TargetDb.DataSet.Tables[tableName].AsEnumerable()
				                                                                                     .Any(
				                                                                                         rowDb =>
                                                                                                         DbDataSetCompareHelper.IsEqualId(rowDb[column1],
				                                                                                                   rowSrc[column1])
				                                                                                         &&
                                                                                                         DbDataSetCompareHelper.IsEqualId(rowDb[column2],
				                                                                                                   rowSrc[column2])
				                                                       ))
				                             .ToList();

				        targetFsRows =
				            TargetFs.DataSet.Tables[tableName].AsEnumerable()
				                                              .Where(
				                                                  a =>
                                                                  updateRows.Any(b =>
                                                                      DbDataSetCompareHelper.IsEqualId(b[column1], a[column1])
                                                                      && DbDataSetCompareHelper.IsEqualId(b[column2], a[column2])
                                                                      )).ToList();
				        targetDbRows =
				            TargetDb.DataSet.Tables[tableName].AsEnumerable()
				                                              .Where(
				                                                  a =>
                                                                  updateRows.Any(b =>
                                                                      DbDataSetCompareHelper.IsEqualId(b[column1], a[column1])
                                                                      && DbDataSetCompareHelper.IsEqualId(b[column2], a[column2])
                                                                      )).ToList();
				        finalStateRows =
				            FinalState.DataSet.Tables[tableName].AsEnumerable()
				                                              .Where(
				                                                  a =>
				                                                  updateRows.Any(b =>
                                                                      DbDataSetCompareHelper.IsEqualId(b[column1], a[column1])
                                                                      && DbDataSetCompareHelper.IsEqualId(b[column2], a[column2])
				                                                      )).ToList();
				    }
				    else
				    {
				        updateRows = srcTable.AsEnumerable().Where(rowSrc =>
				                                                   TargetFs.DataSet.Tables[tableName].AsEnumerable()
				                                                                                     .Any(
				                                                                                         rowFs =>
                                                                                                        DbDataSetCompareHelper.IsEqualId(rowFs[Names.Id],
				                                                                                             rowSrc[Names.Id]))
                                                                   &&
				                                                   TargetDb.DataSet.Tables[tableName].AsEnumerable()
				                                                                                     .Any(
				                                                                                         rowDb =>
                                                                                                         DbDataSetCompareHelper.IsEqualId(rowDb[Names.Id], rowSrc[Names.Id])))
				                             .ToList();

                        targetFsRows = TargetFs.DataSet.Tables[tableName] == null ? targetFsRows :
				            TargetFs.DataSet.Tables[tableName].AsEnumerable()
				                                              .Where(
				                                                  a =>
				                                                  updateRows.Select(b => Convert.ToInt32(b[Names.Id]))
				                                                            .Contains(Convert.ToInt32(a[Names.Id]))).ToList();
				        targetDbRows =
				            TargetDb.DataSet.Tables[tableName].AsEnumerable()
				                                              .Where(
				                                                  a =>
				                                                  updateRows.Select(b => Convert.ToInt32(b[Names.Id]))
				                                                            .Contains(Convert.ToInt32(a[Names.Id]))).ToList();
				        finalStateRows =
				            FinalState.DataSet.Tables[tableName].AsEnumerable()
				                                              .Where(
				                                                  a =>
				                                                  updateRows.Select(b => Convert.ToInt32(b[Names.Id]))
				                                                            .Contains(Convert.ToInt32(a[Names.Id]))).ToList();
				    }

				    if (updateRows.Any())
                        result.Add(updateRows.GetUpdate(targetFsRows, targetDbRows, finalStateRows));
				}

			}

	        return result;
		}

        /// <summary>
        /// Получение действий для иерархической таблицы (пока реализован вариант с наличием единственного поля, ссылающегося на саму таблицу)
        /// </summary>
        /// <param name="idEntity">Идентификатор сущности</param>
        /// <param name="source">Источник обновления</param>
        /// <returns></returns>
        private IEnumerable<DataRow> GetOrderHierarcyRowsForDelete(int idEntity, List<DataRow> source, List<DataRow> result = null)
		{
            if(result == null)
                result = new List<DataRow>();

			string hFieldName = Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
				a =>
				a.Field<int>(Names.IdEntity) == idEntity && a.Field<int?>(Names.IdEntityLink).HasValue &&
				a.Field<int>(Names.IdEntityLink) == idEntity).Select(a => a.Field<string>(Names.Name)).FirstOrDefault();

            List<DataRow> dataRows;

            if (!result.Any())
            {
                dataRows = source.Where(row => source.All(a => (a.Field<int?>(hFieldName) ?? 0) != (int)row[Names.Id])).ToList();
            }
            else
            {
                var notInResult = source.Where(w => result.All(a => (int) a[Names.Id] != (int) w[Names.Id])).ToList();
                var candidates = notInResult.Where(w => result.Any(a => (a.Field<int?>(hFieldName) ?? 0) == (int)w[Names.Id])).ToList();
                dataRows = candidates.Where(w => notInResult.All(a => (a.Field<int?>(hFieldName) ?? 0) != (int)w[Names.Id])).ToList();
            }

            result.AddRange(dataRows);

			if (dataRows.Any())
			{
                    result.AddRange(GetOrderHierarcyRowsForDelete(idEntity, source, result));
			}

			return result;
		}

		/// <summary>
        /// Получение действий для иерархической таблицы (пока реализован вариант с наличием единственного поля, ссылающегося на саму таблицу)
        /// </summary>
        /// <param name="idEntity">Идентификатор сущности</param>
        /// <param name="source">Источник обновления</param>
        /// <param name="target">Обновляемая БД</param>
        /// <param name="idParent">Идентификатор в иерархическом поле</param>
        /// <param name="filter">Дополнительный фильтр</param>
        /// <returns></returns>
		private List<DataRow> GetOrderHierarcyRowsForInsert(int idEntity, DataTable source, DataTable target, int? idParent = null, Predicate<DataRow> filter = null)
        {
	        var result = new List<DataRow>();
	        if (filter == null)
		        filter = row => true;
	        Func<DataRow, bool> filterById;

	        if (source.TableName.StartsWith("enm."))
		        filterById =
			        row => target.AsEnumerable().All(a => Convert.ToInt32(a[Names.Id]) != Convert.ToInt32(row[Names.Id]));
	        else
		        filterById = row => target.Rows.Find(row[Names.Id]) == null;
	        if (source.Columns["id"] == null)
		        filterById = row => true;

	        string hFieldName = Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
		        a =>
		        a.Field<int>(Names.IdEntity) == idEntity && a.Field<int?>(Names.IdEntityLink).HasValue &&
		        a.Field<int>(Names.IdEntityLink) == idEntity).Select(a => a.Field<string>(Names.Name)).FirstOrDefault();

             var dataRows = new List<DataRow>();

             dataRows =
                     source.Rows.Cast<DataRow>().Where(
                         row => filter(row) && filterById(row)).ToList();

		    if (idParent == null)
		    {
		        dataRows =
                    dataRows.Where(row => (row.Field<int?>(hFieldName) ?? 0) == (idParent ?? 0)
		                         || !dataRows.Any(c =>(c.Field<int?>(Names.Id) ?? 0) == (row.Field<int?>(hFieldName) ?? 0))).ToList();
		    }
		    else
		    {
                dataRows =
                   dataRows.Where(row => filter(row) && filterById(row) && (row.Field<int?>(hFieldName) ?? 0) == (idParent ?? 0)).ToList();
		    }
		     
	        result.AddRange(dataRows);
	        if (dataRows.Count > 0)
	        {
		        foreach (DataRow row in dataRows)
		        {
					result.AddRange(GetOrderHierarcyRowsForInsert(idEntity, source, target, row.Field<int?>(Names.Id), filter));
		        }
	        }

	        return result;
        }

	    /// <summary>
        /// Получение действия INSERT для набора строк одной таблицы
        /// </summary>
        /// <param name="source">Источник обновления</param>
        /// <param name="target">Обновляемая БД</param>
        /// <param name="filter">Фильтр отбора строк</param>
        /// <returns></returns>
        private List<DataRow> GetInsertAction(DataTable source, DataTable target, Predicate<DataRow> filter = null)
        {
            if (filter == null)
                filter = row => true;
            Func<DataRow, bool> filterById;

            if (source.TableName.StartsWith("enm."))
                filterById = row => target.AsEnumerable().All(a => Convert.ToInt32(a[Names.Id]) != Convert.ToInt32(row[Names.Id]));
            else
                filterById = row => target.Rows.Find(row[Names.Id]) == null;
            if (source.Columns["id"] == null)
                filterById = row => true;
		    if (source.TableName.StartsWith("ml."))
		    {
			    var exp = Names.Name + "= '" + source.TableName.Split('.')[1] +"'"; 
				DataRow entity = Source.DataSet.Tables[Names.RefEntity].Select(exp)[0];
			    var fields = Source.DataSet.Tables[Names.RefEntityField].Select(Names.IdEntity + "=" + entity[Names.Id] + " AND " + Names.IdEntityFieldType + "=" + (int)EntityFieldType.Link);
			    var column1 = (string) fields[0][Names.Name];
				var column2 = (string) fields[1][Names.Name];
				filterById = row => target.Select(string.Format("[{0}]='{1}' AND [{2}]='{3}'", column1, row[column1], column2, row[column2])).Length == 0;
		    }
			
		    List<DataRow> result = source.Rows.Cast<DataRow>().Where(row => filter(row) && filterById(row)).ToList();
	        return result;
        }

        /// <summary>
        /// Устанавливает приоритет между действиями над БД.
        /// Обрабатывается коллекция <seealso cref="_actionBatches"/> заполняется свойство DependsOn каждого экшена.
        /// См. <seealso cref="OrderedActions"/>
        /// </summary>
        private void FillDependencies()
        {
            var itemsDependTable = Source.DataSet.Tables["reg.ItemsDependencies"];
            _actionList = new LinkedList<DbAction>();
            foreach (var actionBatch in _actionBatches)
            {
                if (actionBatch.Actions != null)
                    foreach (var action in actionBatch.Actions)
                    {
                        _actionList.AddLast(action);
                    }
            }

            var dic = new Dictionary<DbAction, List<DbAction>>();
            var listActions =
                _actionList.Where(
                    w =>
                    (w.EntityName == Names.EntityField &&
					 (w.Row[Names.IdCalculatedFieldType].ToString() == ((int)CalculatedFieldType.DbComputed).ToString() || w.Row[Names.IdCalculatedFieldType].ToString() == ((int)CalculatedFieldType.DbComputedPersisted).ToString())) ||
                    w.EntityName == Names.Programmability).ToList();

            if(itemsDependTable != null)
                foreach (var action in listActions)
                {
                    var entityName = action.EntityName;
                    var dependencies = new List<DataRow>();
                    if (entityName == Names.EntityField)
                    {
                        var entity = Source.DataSet.Tables[Names.RefEntity].AsEnumerable()
                                                                           .SingleOrDefault(
                                                                               w =>
                                                                               (int) w[Names.Id] ==
                                                                               (int) action.Row[Names.IdEntity]);

                        dependencies =
                        itemsDependTable.AsEnumerable()
                                        .Where(
                                            w =>
                                            (int?)w[Names.IDObject] == (int)entity[Names.Id] &&
                                            (int?)w[Names.IDObjectEntity] == Platform.PrimaryEntities.Reference.Entity.EntityIdStatic)
                                        .ToList();
                    }
                    else
                    {
                        var tableEntity =
                        Source.DataSet.Tables[Names.RefEntity].AsEnumerable()
                                                              .SingleOrDefault(
                                                                  w =>
                                                                  w[Names.Name].ToString() == entityName &&
                                                                  (byte)w[Names.IdEntityType] == action.IDEntityType);


                        dependencies =
                            itemsDependTable.AsEnumerable()
                                            .Where(
                                                w =>
                                                (int?)w[Names.IDObject] == (int?)action.Row[Names.Id] &&
                                                (int?)w[Names.IDObjectEntity] == (int?)tableEntity[Names.Id])
                                            .ToList();
                    }

                    var depActionList = new List<DbAction>();
                    foreach (var dependency in dependencies)
                    {
                        var dep =
                            _actionList.Where(
                                dbAction =>
                                dbAction.TableName == DbDataSetCompareHelper.TableNameById((int)dependency[Names.IDDependsOnEntity]) &&
                                (int?) dbAction.Row[Names.Id] == (int?) dependency[Names.IDDependsOn]).ToList();

                        //записываем в словарь экшены от которых зависит данный экшн(далее будем менять их очередность)
                        depActionList.AddRange(dep);
                    }
                    // проверяем не двигали ли их раньше
                    var conrol = dic.SelectMany(s => s.Value).Any(depActionList.Contains);
                    if(!conrol && depActionList.Any())
                        dic.Add(action, depActionList);
                }

            //меняем местами экшены
            foreach (var action in dic)
            {
                var current = _actionList.Find(action.Key);
                int currentIndex = _actionList.Select((n, i) => n == action.Key ? (int?) i : null).
                                              FirstOrDefault(n => n != null) ?? -1;
                foreach (var dep in action.Value)
                {
                    int depIndex = _actionList.Select((n, i) => n == dep ? (int?)i : null).
                                              FirstOrDefault(n => n != null) ?? -1;
                    if (depIndex > currentIndex)
                    {
                        _actionList.Remove(dep);
                        _actionList.AddBefore(current, dep);
                        FillOrder(dic, dep);
                    }
                }
            }
        }

        private void FillOrder(Dictionary<DbAction, List<DbAction>> actionsDic, DbAction currentAction)
        {
            if (actionsDic.ContainsKey(currentAction))
                foreach (var action in actionsDic[currentAction])
                {
                    var current = _actionList.Find(currentAction);
                    int currentIndex = _actionList.Select((n, i) => n == currentAction ? (int?)i : null).
                              FirstOrDefault(n => n != null) ?? -1;
                    int depIndex = _actionList.Select((n, i) => n == action ? (int?)i : null).
                          FirstOrDefault(n => n != null) ?? -1;
                    if (depIndex > currentIndex)
                    {
                        _actionList.Remove(action);
                        _actionList.AddBefore(current, action);
                        FillOrder(actionsDic, action);
                    }
                }
        }



        private string GetTableNameByEntityId(int id)
        {
            var entity = _entities.SingleOrDefault(w => (int) w[Names.Id] == id) ?? TargetDb.DataSet.Tables[Names.RefEntity].AsEnumerable().ToList().SingleOrDefault(w => (int)w[Names.Id] == id);

            return entity != null ? string.Format("{0}.{1}",Schemas.ByEntityType((byte)entity[Names.IdEntityType]), entity[Names.Name]) : null;
        }

        /// <summary>
        /// На основании очереди экшенов, получаем упорядоченный набор комманд
        /// Заполняется коллекция <seealso cref="OrderedCommands"/>
        /// </summary>
        private void GetOrderedCommands()
        {
            var actions = _actionList.ToList();
            OrderedCommands.Add(GetCommands(actions));
        }

        private Dictionary<int, List<SqlCommand>> GetCommands(List<DbAction> actions)
        {
            var i = 1;
            var dic = new Dictionary<int, DbAction>();
            foreach (var action in actions)
            {
                dic.Add(i,action);
                i++;
            }
            var result = new Dictionary<int, List<SqlCommand>>();

            foreach (var value in dic)
            {
                var commandList = value.Value.GetCommand();
                    if(commandList.Any())
                        result.Add(value.Key, commandList);
            }

            return result;
        }

        #endregion
    }
}
