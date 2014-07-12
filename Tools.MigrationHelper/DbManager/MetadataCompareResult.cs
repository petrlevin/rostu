using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.Utils.Extensions;
using Tools.MigrationHelper.DbManager;
using Tools.MigrationHelper.DbManager.DbActions.Interfaces;
using Tools.MigrationHelper.DbManager.Extensions;
using Tools.MigrationHelper.Extensions;
using Tools.MigrationHelper.Helpers;

namespace MigrationHelper
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
		public MetadataCompareResult(Metadata source, Metadata targetDb, Metadata targetFs)
		{
			TargetDb = targetDb;
			TargetFs = targetFs;
			Source = source;
		}
		#endregion

		#region Private Members
		
		/// <summary>
		/// Неупорядоченный список действий, необходимых для приведения <seealso cref="Target"/> к состоянию <seealso cref="Source"/>.
		/// </summary>
		private List<IDbAction> Actions;

		#endregion

		#region Public Members

		/// <summary>
		/// Целевая ОММ (на которую нацелен процесс обновления), которую необходимо привести в состояние <seealso cref="Source"/>.
		/// </summary>
		public Metadata TargetDb { get; private set; }

		/// <summary>
		/// Целевая ОММ, полученная из файлов поставки
		/// </summary>
		public Metadata TargetFs { get; private set; }

		/// <summary>
		/// Исходная (эталонная) ОММ, к состоянию которой необходимо привести <seealso cref="TargetDb"/> с учетом <seealso cref="TargetFs"/>.
		/// </summary>
		public Metadata Source { get; private set; }

		/// <summary>
		/// Упорядоченный список действий, необходимых для приведения <seealso cref="Target"/> к состоянию <seealso cref="Source"/>.
		/// </summary>
		/// <remarks>
		/// * Удаление полей и udf/sp/view.
		/// * Обновление полей
		/// * Удаление сущностей
		/// * Обновление сущностей
		/// * Создание сущностей
		/// * Ссоздание полей и udf/sp/view.
		/// Отключение триггеров
		/// * Состав предзаполненных справочников и ref.EntityItem
		/// Включение триггеров
		/// </remarks>
		public List<List<IDbAction>> OrderedActions;

		private IDbAction OrderEntityForDelete(List<DataRow> entityRows)
		{
			List<DataRow> entityFieldRows =
				TargetDb.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
					a => entityRows.Select(k => k.Field<int>(Names.Id)).Contains(a.Field<int>(Names.IdEntity))).ToList();
			var test =
				entityFieldRows.Where(a => a.Field<int?>(Names.IdEntityLink).HasValue).ToDictionary(
					a => a.Field<int>(Names.IdEntity), b => b.Field<int?>(Names.IdEntityLink));

			List<DataRow> orderList = new List<DataRow>();
			while (orderList.Count != entityRows.Count())
			{
				foreach (DataRow row in entityRows)
				{
					if (orderList.All(a => a.Field<int>(Names.Id) != row.Field<int>(Names.Id)))
					{
						if (!entityFieldRows.Any(a => a.Field<int>(Names.IdEntity) != row.Field<int>(Names.Id)
						                              && a.Field<int?>(Names.IdEntityLink) == row.Field<int>(Names.Id)
						                              && !orderList.Select(k => k.Field<int>(Names.Id)).Contains(a.Field<int>(Names.IdEntity))))
							orderList.Add(row);
					}
				}
			}
			return orderList.GetDelete();
			//entityRows.Where(a=> )
		}

		private List<int> OrderEntityForInsert(Metadata metadata)
		{
			List<int> result = metadata.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
					a => 
					metadata.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
						b => b.Field<int>(Names.IdEntity) == a.Field<int>(Names.Id) && b.Field<byte>(Names.IdEntityFieldType) == (byte)EntityFieldType.Link).All(c => !c.Field<int?>(Names.IdEntityLink).HasValue))
					.Select(a => a.Field<int>(Names.Id)).Distinct().ToList();
			string tmp = string.Join(",", result);

			result = GetLinkedEntity(metadata, result).ToList();
			return result;
		}

		private IEnumerable<int> GetLinkedEntity(Metadata metadata, List<int> listEntityId)
		{
			List<DataRow> entitis =
				metadata.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(a => !listEntityId.Contains(a.Field<int>(Names.Id))).
					ToList();
			List<int> result=new List<int>();
			List<int> added = (from entity in entitis
			                   let exist = metadata.DataSet.Tables[Names.RefEntityField].AsEnumerable().Count(b => b.Field<int>(Names.IdEntity) == entity.Field<int>(Names.Id) && b.Field<byte>(Names.IdEntityFieldType) == (byte) EntityFieldType.Link && b.Field<int?>(Names.IdEntityLink).HasValue && (listEntityId.Contains(b.Field<int>(Names.IdEntityLink)) || b.Field<int>(Names.IdEntityLink) == entity.Field<int>(Names.Id)))
			                   let notExist = metadata.DataSet.Tables[Names.RefEntityField].AsEnumerable().Count(b => b.Field<int>(Names.IdEntity) == entity.Field<int>(Names.Id) && b.Field<byte>(Names.IdEntityFieldType) == (byte) EntityFieldType.Link && b.Field<int?>(Names.IdEntityLink).HasValue && b.Field<int>(Names.IdEntityLink) != entity.Field<int>(Names.Id) && !listEntityId.Contains(b.Field<int>(Names.IdEntityLink)))
			                   where exist > 0 && notExist == 0
			                   select entity.Field<int>(Names.Id)).ToList();
			string tmp = string.Join(",", result);

			result.AddRange(listEntityId);
			result.AddRange(added);
			if (added.Count > 0)
			{
				result = GetLinkedEntity(metadata, result).ToList();
			}
			return result;
		}
		
		/// <summary>
		/// Сравнивает две ОММ: <seealso cref="Source"/>, <seealso cref="TargetDb"/> и <seealso cref="TargetFs"/>.
		/// Результатом является заполненная коллекция <seealso cref="OrderedActions"/>.
		/// </summary>
		public void Compare(SqlConnection connection)
		{
			Actions = new List<IDbAction>();
			CheckMetadataTables();
			List<string> eef = new List<string> { Names.RefEntity, Names.RefEntityField };

			Predicate<DataRow> ids = row => row.Field<string>(Names.Name) == Names.Id;
			Predicate<DataRow> notIdsNotDbCalculated = row => row.Field<string>(Names.Name) != Names.Id && row.Field<byte?>(Names.IdCalculatedFieldType) != 3;
			Predicate<DataRow> notIdsDbCalculated = row => row.Field<string>(Names.Name) != Names.Id && row.Field<byte?>(Names.IdCalculatedFieldType) == 3;

			Predicate<DataRow> notExistEntityInTargetById =
				row => TargetDb.DataSet.Tables[Names.RefEntity].AsEnumerable().All(rowDb => rowDb.Field<int>("id") != row.Field<int>("id"))
					   && TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().All(rowFs => rowFs.Field<int>("id") != row.Field<int>("id"));
			
			Predicate<DataTable> isEnum = table => table.TableName.StartsWith("enm.");
			Predicate<DataTable> isMultiLink = table => table.TableName.StartsWith("ml.");

			if (TargetFs == null)
			{
				// Сущности
				Actions.Add(getCreateAction(Source.DataSet.Tables[Names.RefEntity], TargetDb.DataSet.Tables[Names.RefEntity]));
				// Поля с именем "id"
				Actions.Add(getCreateAction(Source.DataSet.Tables[Names.RefEntityField], TargetDb.DataSet.Tables[Names.RefEntityField], ids));
				// Остальные поля
				Actions.Add(getCreateAction(Source.DataSet.Tables[Names.RefEntityField], TargetDb.DataSet.Tables[Names.RefEntityField], notIdsNotDbCalculated));
				Actions.Add(getCreateAction(Source.DataSet.Tables[Names.RefEntityField], TargetDb.DataSet.Tables[Names.RefEntityField], notIdsDbCalculated));
				List<int> orderEntity = OrderEntityForInsert(Source);

				// Элементы перечислений
				Actions.AddRange(createNewTablesOnTarget(orderEntity, eef));
				// Элементы остальных сущностей
				//Actions.AddRange(createNewTablesOnTarget(srcTable => !isEnum(srcTable) && !isMultiLink(srcTable) && !eef.Contains(srcTable.TableName)));
				//Элементы мультилинков
				//Actions.AddRange(createNewTablesOnTarget(srcTable => isMultiLink(srcTable)));
			}
			else
			{
				/*
				 * Правила для изменения строк в таблице Entity
				 * 1. Нельзя менять поля Name,idEntityType
				 */
				/*
				 * удалить лишние поля сущности Entity
				 * вставить нессылочные поля сущности Entity
				 * вставить ссылочные поля сущшости Entity ссылающиеся на существующую сущность
				 * обновить нессылочные поля сущности Entity
				 * обновить ссылочные поля сущшости Entity ссылающиеся на существующую сущность
				*/

				/*
				1.вставить,апдейтить сущности
				2.вставить поля id сущностей
				3.вставить нессылочные поля сущностей
				4.вставить ссылочные поля сущностей
				5.удалить поля сущностей
				6.изменить поля сущностей
				7.удалить сущности
				*/

				/*
				1.получить удаляемые сущности и удалить в нужном порядке, поля удалятся каскадно
				 * 
				 * имеем "список1" и бежим по нему
				 * а) на сущность никто из сущностей "списка1" не ссылается, переносим в "список2"
				 * б) на сущность ссылается сущность из "списка1", берем ссылающуюся сущность и проверяем ее на шаг а), и на шаг б) - получаем рекурсию
				 * возникает вопрос, как бежать
				 * 
				 
				*/
				//(операция 1)Получаем сущности которые есть в TargetFs и отсутствуют в Source, для удаления
				List<DataRow> deleteEntity =
					TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
						rowFs =>
						Source.DataSet.Tables[Names.RefEntity].AsEnumerable().All(
							rowSrc => rowSrc.Field<int>(Names.Id) != rowFs.Field<int>(Names.Id))).ToList();
				//Удаляем сущности, автоматом удаляются поля этих сущностей
				//TODO сделать удаление индексов и прочих привязанных к сущности и ее полям данных
				Actions.Add(OrderEntityForDelete(deleteEntity));

				//(операция 2)Получаем поля сущностей которые (есть в TargetFs и отсутствуют в Source) и не были удалены в результате (операции 1), для удаления
				List<DataRow> deleteEntityField =
					TargetFs.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
						rowFs =>
						Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().All(
							rowSrc => rowSrc.Field<int>(Names.Id) != rowFs.Field<int>(Names.Id)) && !deleteEntity.Select(k=> k.Field<int>(Names.Id)).Contains(rowFs.Field<int>(Names.IdEntity))).ToList();
				//Команды для удаления полей удалемых сущностей
				Actions.Add(deleteEntityField.GetDelete());

				//(операция 3)новые сущности
				List<DataRow> newEntity = Source.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
					rowSrc =>
					TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().All(rowFs => rowFs.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))
					&& TargetDb.DataSet.Tables[Names.RefEntity].AsEnumerable().All(rowDb => rowDb.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))).
					ToList();
				Actions.Add(newEntity.GetInsert());

				//(операция 3)Добавить поля id для новых сущностей
				List<DataRow> newEntityFieldId =
					Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
						rowSrc => rowSrc.Field<string>(Names.Name) == Names.Id &&
						newEntity.Select(a => a.Field<int>(Names.Id)).Contains(rowSrc.Field<int>(Names.IdEntity))).ToList();
				Actions.Add(newEntityFieldId.GetInsert());

				//(операция 4)Добавить поля, имя которых не равно id
				List<DataRow> newEntityFieldNotId =
					Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
						rowSrc => rowSrc.Field<string>(Names.Name) != Names.Id &&
						TargetFs.DataSet.Tables[Names.RefEntityField].AsEnumerable().All(rowFs => rowFs.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))
						&& TargetDb.DataSet.Tables[Names.RefEntityField].AsEnumerable().All(rowDb => rowDb.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))).ToList();
				Actions.Add(newEntityFieldId.GetInsert());

				//(операция 5)Получаем сущности которые есть в Source и в TargetFs чтобы проверить в дальнейшем, были ли изменения
				List<DataRow> updateEntity =
					Source.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(
						rowSrc =>
						TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().Any(rowFs => rowFs.Field<int>(Names.Id) == rowSrc.Field<int>(Names.Id))).
						ToList();
				Actions.Add(updateEntity.GetUpdate(
					TargetFs.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(a => updateEntity.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id))), 
					TargetDb.DataSet.Tables[Names.RefEntity].AsEnumerable().Where(a => updateEntity.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id)))));

				//(операция 6)Получаем поля сущностей которые есть в Source и в TargetFs чтобы проверить в дальнейшем, были ли изменения
				List<DataRow> updateEntityField = Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
					rowSrc =>
					TargetFs.DataSet.Tables[Names.RefEntityField].AsEnumerable().Any(rowFs => rowFs.Field<int>(Names.Id) == rowSrc.Field<int>(Names.Id))).
					ToList();
				Actions.Add(updateEntityField.GetUpdate(
					TargetFs.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(a => updateEntityField.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id))),
					TargetDb.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(a => updateEntityField.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id)))));

				
				//Теперь надо сделать обновление остальных сущностей а для этого определить очередность действий
				/*
				 * варианты:
				 * 1. есть данные в сущности с обеих сторон
				 * 2. есть данные в сущности только с одной стороны
				 * 3. нюанс (а сущность только была создана) - и чего делать то ???
				 * 4. надо как то определить очередность отдельно для удаления, изменения и добавления данных
				 * а) порядок - вставляем
				 * б) порядок - обновляем
				 * в) порядок - удаляем
				 * делать все это по TargetFs, если упало в связи с различиями с TargetDb - значит упало
				 * саначала удалим все лишнее
				*/

				foreach (DataTable tableSrc in Source.DataSet.Tables.Cast<DataTable>().Where(a => a.TableName != Names.RefEntity && a.TableName != Names.RefEntityField))
				{
					string tableName = tableSrc.TableName;
					if (TargetFs.DataSet.Tables[tableName] == null && TargetDb.DataSet.Tables[tableName] == null)
					{
						Actions.Add(tableSrc.AsEnumerable().GetInsert());
					} else
					{
						//вставка новых записей
						List<DataRow> insertRows =
							tableSrc.AsEnumerable().Where(
								rowSrc =>
								TargetFs.DataSet.Tables[tableName].AsEnumerable().All(
									rowFs => rowFs.Field<int>(Names.Id) != rowSrc.Field<int>(Names.Id))).ToList();
						Actions.Add(insertRows.GetInsert());
						
						//обновление
						List<DataRow> updateRows = tableSrc.AsEnumerable().Where(
							rowSrc =>
							TargetFs.DataSet.Tables[tableName].AsEnumerable().Any(
								rowFs => rowFs.Field<int>(Names.Id) == rowSrc.Field<int>(Names.Id))
							&&
							TargetDb.DataSet.Tables[tableName].AsEnumerable().Any(
								rowDb => rowDb.Field<int>(Names.Id) == rowSrc.Field<int>(Names.Id))).ToList();
						Actions.Add(updateRows.GetUpdate(
							TargetFs.DataSet.Tables[tableName].AsEnumerable().Where(a => updateRows.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id))), 
							TargetDb.DataSet.Tables[tableName].AsEnumerable().Where(a => updateRows.Select(b => b.Field<int>(Names.Id)).Contains(a.Field<int>(Names.Id)))));
						
						//удаление
						List<DataRow> deleteRows =
							TargetFs.DataSet.Tables[tableName].AsEnumerable().Where(
								rowFs =>
								Source.DataSet.Tables[tableName].AsEnumerable().All(
									rowSrc => rowSrc.Field<int>(Names.Id) != rowFs.Field<int>(Names.Id))).ToList();
						Actions.Add(deleteRows.GetDelete());
					}
				}
			}
			// Сущности (таблицы), отсутствующие в эталонной ОММ. Их следует удалить в целевой БД.
			// Таблицы не рассматриваем, т.к. они целиком будут удалены при удалении соответствующей строки из ref.Entity
			// ...
			
			// Таблицы Списки элементов (таблицы, список udf, список sp, список view), отсутствующие в целевой ОММ.
			// Они будут созданы.
			// Здесь таблицы не исключаем, ввиду возможности наличия предзаполненных данных
			//IEnumerable<IDbElement> elementsForInsert = Source.DbElements.Where(src => !Target.DbElements.Any(target => isEqual(src, target)));
			//foreach (var missInTarget in elementsForInsert)
			//{
			//	Actions.Add(missInTarget.GetCreate());
			//}

			// Списки, которые присутствуют в обоих сравниваемых ОММ. Сравниваем их состав
			// Здесь таблицы не исключаем, т.к. в эталонной ОММ может присутствовать созданный предзаполненный справочник с данными.
			//IEnumerable<IDbEntity> listsToCompare = this.Target.Where(target => this.Source.Any(src => isEqual(src, target)));
			//foreach (var targetList in listsToCompare)
			//{
			//	this.Actions.AddRange(targetList.CompareWithSource(this.Source.Single(src => isEqual(src, targetList))));
			//}

			//Prioritize();
			computeOrder();
			//UpdateViews();
		}

		/// <summary>
		/// Обновление View после изменений в таблицах Entity и EntityField
		/// </summary>
		private void UpdateViews()
		{
			const string entity = Names.EntitySchema + "." + Names.Entity;
			const string entityField = Names.EntityFieldSchema + "." + Names.EntityField;
			Func<DataRow, bool> forUpdateViews = row => row.Field<string>(Names.Name) == "forUpdateViews";

			Source.DataSet.Tables[entityField].Columns["id"].AutoIncrement = false;
			Source.DataSet.Tables[entityField].Columns["id"].ReadOnly = false;
			int countColumn = Source.DataSet.Tables[entityField].Columns.Count;
			DataRow baseRow = Source.DataSet.Tables[entityField].AsEnumerable().SingleOrDefault(a=> a.Field<string>(Names.Name)==Names.Id && a.Field<int>(Names.IdEntity)==1);
			if (baseRow==null)
				throw new Exception("Не получена строка EntityField, где Name=='id' && idEntity==1");
			int i = 1;
			List<int> idsEntity =
				Source.DataSet.Tables[entity].AsEnumerable().Where(a => a.Field<byte>("idEntityType") != 1).Select(
					a => a.Field<int>("id")).Union(
						TargetDb.DataSet.Tables[entity].AsEnumerable().Where(a => a.Field<byte>("idEntityType") != 1).Select(
							a => a.Field<int>("id"))).ToList();
			foreach (int idEntity in idsEntity)
			{
				DataRow insertRow = Source.DataSet.Tables[entityField].NewRow();
				for (int j = 0; j < countColumn; j++)
				{
					insertRow[j] = baseRow[j];
				}
				insertRow["id"] = int.MaxValue - i;
				insertRow["idEntity"] = idEntity;
				insertRow["Name"] = "forUpdateViews";
				insertRow["AllowNull"] = true;
				i++;
				Source.DataSet.Tables[entityField].Rows.Add(insertRow);
			}
			Source.DataSet.Tables[entityField].Columns["id"].AutoIncrement = true;
			Source.DataSet.Tables[entityField].Columns["id"].ReadOnly = true;

			//Actions.Add(getCreateAction(Source.DataSet.Tables[entityField], TargetDb.DataSet.Tables[entityField], forUpdateViews));
			Actions.Add(Source.DataSet.Tables[entityField].AsEnumerable().Where(forUpdateViews).ToList().GetInsert());
			Actions.Add(Source.DataSet.Tables[entityField].AsEnumerable().Where(forUpdateViews).ToList().GetDelete());
		}

		/// <summary>
		/// Возвращает упорядоченный список действий, сгенерированных для обновления БД
		/// </summary>
		/// <returns></returns>
		public string Verbose()
		{
			StringBuilder result = new StringBuilder();
			int queueNum = 0; // номер очереди
			int actionNum;    // номер действия в очереди.
			foreach (var orderedAction in OrderedActions)
			{
				actionNum = 0;
				result.AppendLine(string.Format("Очередь действий № {0}.", ++queueNum));

				foreach (var dbAction in orderedAction)
				{
					result.AppendLine(string.Format("{0}. {1}", ++actionNum, dbAction.Verbose()));
				}
			}
			return result.ToString();
		}

		/// <summary>
		/// Выполняет действия <see cref="OrderedActions"/> над указанной БД
		/// </summary>
		public void Execute(SqlConnection connection)
		{
			string errTpl = "Произошла ошибка при выполнении команды.{0}Очередь №{1}, действие №{2}, комманда №{3}.{0}Sql:\n{4}{0}{0}Текст ошибки:{0}{5}";
			int i = 0, j, k;
			foreach (var list in OrderedActions)
			{
				++i; 
				j = 0; 
				foreach (var action in list)
				{
					++j; 
					k = 0;
					foreach (var command in action.GetCommand())
					{
						++k;
						command.Connection = connection;
						try
						{
							Debug.WriteLine(command.AsString() + Environment.NewLine + "===");
							command.ExecuteNonQuery();
						}
						catch(SqlException ex)
						{
							var msg = string.Format(errTpl, Environment.NewLine, i, j, k, command.AsString(), ex.Message);
							throw new Exception(msg, ex);
						}
					}
				}
			}
		}

		#endregion

		#region Private Methods

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
		private IEnumerable<IDbAction> createNewTablesOnTarget(IEnumerable<int> orderEntity, List<string> exclidedTable)
		{
			List<IDbAction> result = new List<IDbAction>();

			foreach (int idEntity in orderEntity)
			{
				DataRow rowEntity =
					Source.DataSet.Tables[Names.RefEntity].AsEnumerable().Single(a => a.Field<int>(Names.Id) == idEntity);
				bool isHierarchyEntity =
					Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Any(
						a =>
						a.Field<int>(Names.IdEntity) == idEntity && a.Field<int?>(Names.IdEntityLink).HasValue &&
						a.Field<int>(Names.IdEntityLink) == idEntity);
				string tableName = Utils.getSchemaByEntityType(rowEntity.Field<byte>(Names.IdEntityType)) + "." +
				                   rowEntity.Field<string>(Names.Name);
				DataTable srcTable = Source.DataSet.Tables[tableName];
				if (srcTable != null)
				{
					if (!TargetDb.DataSet.Tables.Contains(srcTable.TableName)) // целиком отсутствующие таблицы
					{
						TargetDb.DataSet.Tables.Add(srcTable.Clone());
					}
					if (srcTable.Rows.Count > 0)
					{
						if (!exclidedTable.Contains(tableName) && !isHierarchyEntity)
						{
							result.Add(getCreateAction(srcTable, TargetDb.DataSet.Tables[srcTable.TableName]));
						}
						if (!exclidedTable.Contains(tableName) && isHierarchyEntity)
						{
							result.AddRange(getHierarcyCreateAction(idEntity, srcTable, TargetDb.DataSet.Tables[srcTable.TableName], null));
						}
					}
				}
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
		/// <param name="isUpdate">Обновление БД/или деплой</param>
		/// <param name="filter">Дополнительный фильтр</param>
		/// <returns></returns>
		private List<IDbAction> getHierarcyCreateAction(int idEntity, DataTable source, DataTable target, int? idParent = null, bool isUpdate = false, Predicate<DataRow> filter = null)
		{
			List<IDbAction> result=new List<IDbAction>();
			if (isUpdate)
			{

			}
			else
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

				string hFieldName = Source.DataSet.Tables[Names.RefEntityField].AsEnumerable().Where(
					a =>
					a.Field<int>(Names.IdEntity) == idEntity && a.Field<int?>(Names.IdEntityLink).HasValue &&
					a.Field<int>(Names.IdEntityLink) == idEntity).Select(a => a.Field<string>(Names.Name)).FirstOrDefault();

				List<DataRow> dataRows = source.Rows.Cast<DataRow>().Where(row => filter(row) && filterById(row) && (row.Field<int?>(hFieldName) ?? 0) == (idParent ?? 0)).ToList();

				result.Add(dataRows.GetInsert());
				if (dataRows.Count>0)
				{
					foreach (DataRow row in dataRows)
					{
						result.AddRange(getHierarcyCreateAction(idEntity, source, target, row.Field<int?>(Names.Id)));
					}
				}

			}
			return result;
		}

		private IDbAction getCreateAction(DataTable source, DataTable target, Predicate<DataRow> filter = null)
		{
			if (filter ==  null)
				filter = row => true;
			Func<DataRow, bool> filterById;

			if (source.TableName.StartsWith("enm."))
				filterById = row => target.AsEnumerable().All(a => Convert.ToInt32(a[Names.Id]) != Convert.ToInt32(row[Names.Id]));
			else
				filterById = row => target.Rows.Find(row[Names.Id]) == null;
			if (source.Columns["id"]==null)
				filterById = row => true;

			List<DataRow> result = source.Rows.Cast<DataRow>().Where(row => filter(row) && filterById(row)).ToList();
			return result.GetInsert();
		}

		/// <summary>
		/// Устанавливает приоритет между действиями над БД.
		/// Обрабатывается коллекция <seealso cref="Actions"/> заполняется свойство DependsOn каждого экшена.
		/// См. <seealso cref="OrderedActions"/>
		/// </summary>
		private void prioritize()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// На основании информации о приоритете выполняемых действий над БД определяется точный порядок.
		/// Заполняется коллекция <seealso cref="OrderedActions"/>
		/// </summary>
		private void computeOrder()
		{
			//throw new NotImplementedException();
			this.OrderedActions = new List<List<IDbAction>>() { this.Actions };
		}

		/// <summary>
		/// Определяет порядок, в котором должны следовать таблицы при загрузке данных.
		/// </summary>
		/// <param name="tableNames">Имена таблиц, в которые будут загружаться данные</param>
		/// <returns>Упорядоченный список в соответствии с зависимостями между таблицами</returns>
		private List<string> orderTables(List<string> tableNames)
		{
			/*
			 * В данный момент в методе жестко закодирован порядок загрузки.
			 * В дальнейшем следует реализовать алгоритм, вычисляющий порядок на основе информации о ссылочных полях.
			 */

			var order = new List<string>
				{
					"Operation",
					"EntityOperation"
				};

			var sourceList = tableNames.Clone().ToList();

			// удаляем из списка, определяющего порядок, элементы отсутствующие в tableNames
			order.RemoveAll(order.Where(i => !sourceList.Contains(i)).Contains);
			// удаляем из tableNames элементы, оставшиеся в order
			sourceList.RemoveAll(order.Contains);

			return order.Concat(sourceList).ToList();
		}

		#endregion
	}
}
