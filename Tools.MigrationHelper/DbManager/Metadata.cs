using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MigrationHelper;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Tools.MigrationHelper.Extensions;
using Tools.MigrationHelper.Helpers;
using Tools.MigrationHelper.EnumsProcessing;

namespace Tools.MigrationHelper.DbManager
{
	/// <summary>
	/// Объектная модель метаданныех БД. Полностью определяет структуру БД. 
	/// Содержит списки строк таблиц и списки программируемых объектов.
	/// </summary>
	public class Metadata
	{
		#region Templates and Settings

		private const string SqlEntity = "SELECT * FROM [ref].[Entity]";
		private const string SqlEntityField = "SELECT * FROM [ref].[EntityField]";

		#endregion

		#region Private Fields
		
		/// <summary>
		/// Строка подключения с БД
		/// </summary>
		private readonly string _connectionString;

		/// <summary>
		/// Используется при сохранении ОММ в xml
		/// </summary>
		private readonly XmlWriteMode xmlWriteMode = XmlWriteMode.WriteSchema;

		private const SolutionProject defaultProject = 0;

		#endregion

		/// <summary>
		/// DataSet содержащий предзаполненные данные
		/// </summary>
		public DataSet DataSet;

		//по имени получить ид из сущности
		//новый ид из сущности, и из поля сущности

		/// <summary>
		/// Конструктор
		/// </summary>
		public Metadata()
		{
			DataSet = new DataSet();
		}
		
		#region FromDb

		/// <summary>
		/// Получить метаданные из БД
		/// </summary>
		/// <param name="connection">Соединение с БД</param>
		public void FromDb(SqlConnection connection, bool isDeveloper = true)
		{
			FromDbMetadataTables(connection, isDeveloper);
			FromDbOtherTables(connection, isDeveloper);
			SetAllowNullToId();
		}

		/// <summary>
		/// Получить метаданные из БД
		/// </summary>
		/// <param name="connectionString">Строка соединения</param>
		public void FromDb(string connectionString)
		{
			using(var conn = new SqlConnection(connectionString))
			{
				conn.Open();
				FromDb(conn);
				conn.Close();
			}
		}

		/// <summary>
		/// Во всех таблицах датасета устанавливает для поля id признак allow null
		/// </summary>
		private void SetAllowNullToId()
		{
			return;
			foreach (DataTable table in DataSet.Tables)
			{
				if (table.Columns.Contains(Names.Id))
				{
					DataColumn col = table.Columns[Names.Id];
					col.AllowDBNull = true;
					col.AutoIncrement = false;
				}
			}
		}

		/// <summary>
		/// Считывает содержимое двух основных таблиц с метаданными: ref.Entity и ref.EntityField.
		/// Информация о других таблицах (в т.ч. и об оставшихся таблицах метаданных) может быть получена из этих двух таблиц.
		/// </summary>
		private void FromDbMetadataTables(SqlConnection connection, bool isDeveloper = true)
		{
			// http://msdn.microsoft.com/en-us/library/vstudio/bb399340(v=vs.100).aspx Loading Data Into a DataSet
			SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(isDeveloper ? SqlEntity : SqlEntity + " WHERE id<0", connection)
				{MissingSchemaAction = MissingSchemaAction.AddWithKey};
			sqlDataAdapter.Fill(DataSet, Names.RefEntity);

			sqlDataAdapter = new SqlDataAdapter(isDeveloper ? SqlEntityField : SqlEntityField + " WHERE id<0", connection)
				{MissingSchemaAction = MissingSchemaAction.AddWithKey};
			sqlDataAdapter.Fill(DataSet, Names.RefEntityField);
		}

		/// <summary>
		/// Считывает строки предзаполненных сущностей и неосновных таблиц метаданных (основные - Entity и EntityField).
		/// </summary>
		private void FromDbOtherTables(SqlConnection connection, bool isDeveloper = true)
		{
			const string sqlAllItems ="SELECT * FROM [{0}].[{1}]";
			const string sqlPredefinedItems = sqlAllItems + " WHERE [id]<0";
			DataTable entity = DataSet.Tables[Names.RefEntity];
			DataTable entityField = DataSet.Tables[Names.RefEntityField];

			foreach (DataRow row in entity.Select("Name<>'Entity' AND Name<>'EntityField'")) 
			{
				string selectStmt = string.Empty;
				var entityType = (EntityType)(byte)row[Names.IdEntityType];
				if (entityType == EntityType.Enum || isDeveloper)
				{
					selectStmt = sqlAllItems;
				}
				else
				{
					bool hasId = entityField.Select(string.Format("Name='{0}' AND idEntity='{1}'", Names.Id, row[Names.Id])).Any();
					if (!hasId)
						continue;
					selectStmt = sqlPredefinedItems;
				}
				
				var schema = Utils.getSchemaByEntityType((int)entityType);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(string.Format(
					selectStmt,
					schema,
					row[Names.Name]
				), connection) { MissingSchemaAction = MissingSchemaAction.AddWithKey };

				var tableName = string.Format("{0}.{1}", schema, row[Names.Name]);
				sqlDataAdapter.Fill(DataSet, tableName);
			}
		}
		#endregion

		#region FromFs

		/// <summary>
		/// Считывает метаданные из файловой системы
		/// </summary>
		/// <param name="path">Путь до проекта, от которого начинается поиск директорий DbStructure</param>
		/// <param name="factor">Множитель, 1 или -1</param>
		/// <param name="isPlatformDeploy"></param>
		public void FromFs(string path, int factor = 1, bool isPlatformDeploy = false)
		{
			var xmlPath = path + ( isPlatformDeploy ? "\\" + SolutionProject.Tools_MigrationHelper_Core.ToString().Replace("_", ".") : "");
			List<string> listDirectories = GetDirectories(xmlPath);
			if (listDirectories.Count==0)
				throw new Exception("Не найдено ни одного файла");
			FromFsTables(listDirectories);
			FillEmptyIdsInDataSet();
			FromFsEnums(path, isPlatformDeploy);
			DataSet = MultipleFactor(DataSet, factor);
		}

		public static DataSet MultipleFactor(DataSet dataSet, int factor)
		{
			if (factor == -1)
			{
				foreach (DataTable table in dataSet.Tables)
				{
					if (table.Columns["id"] != null && table.Columns["id"].AutoIncrement)
					{
						table.Columns["id"].AutoIncrement = false;
						table.Columns["id"].ReadOnly = false;
					}
				}
				List<int> validEntity = dataSet.Tables[Names.EntitySchema + "." + Names.Entity].AsEnumerable().Where(
					a => a.Field<byte>("idEntityType") != 1).Select(a => a.Field<int>("id")).ToList();
				foreach (
					DataRow rowEntity in
						dataSet.Tables[Names.EntitySchema + "." + Names.Entity].AsEnumerable().Where(
							a => a.Field<byte>("idEntityType") != 1))
				{
					if (dataSet.Tables[Schemas[rowEntity.Field<byte>("idEntityType")] + "." + rowEntity.Field<string>("Name")] != null)
					{
						List<string> fields =
							dataSet.Tables[Names.EntityFieldSchema + "." + Names.EntityField].AsEnumerable().Where(a =>
																													a.Field<int>("idEntity") ==
																													rowEntity.Field<int>("id") &&
																													((a.Field<int?>(
																														Names.IdEntityLink).
																														  HasValue &&
																													  validEntity.Contains(
																														  a.Field<int>(
																															  Names.IdEntityLink)))
																													 ||
																													 a.Field<string>("Name") ==
																													 "id")).Select(
																														 a =>
																														 a.Field<string>("Name"))
								.ToList();

						foreach (
							DataRow rowTable in
								dataSet.Tables[Schemas[rowEntity.Field<byte>("idEntityType")] + "." + rowEntity.Field<string>("Name")].Rows)
						{
							foreach (string fieldName in fields)
							{
								rowTable[fieldName] = rowTable.Field<int?>(fieldName) * -1;
							}
						}
					}
				}

				foreach (DataTable table in dataSet.Tables)
				{
					if (table.Columns["id"] != null && table.Columns["id"].AutoIncrement)
					{
						table.Columns["id"].AutoIncrement = true;
						table.Columns["id"].ReadOnly = true;
					}
				}
			}
			return dataSet;
		}

		public static Dictionary<byte, string> Schemas = new Dictionary<byte, string>()
			{
				{1, "enm"},
				{3, "ref"},
				{5, "ml"},
				{4, "tp"},
				{8, "reg"},
				{7, "tool"},
				{6, "doc"},
				{9, "rep"}
			};

		/// <summary>
		/// Считывает данные из файловой системы для всех таблиц
		/// </summary>
		/// <param name="listDirectories">Список директорий DbStructure и DbEnums</param>
		private void FromFsTables(IEnumerable<string> listDirectories)
		{
			foreach (string directoryName in listDirectories)
			{
				DirectoryInfo dir = new DirectoryInfo(directoryName);
				
				if (dir.Name == Names.DbEnums)
				{
					var files = new[] {Names.Entity, Names.EntityField};
					foreach (var file in files)
					{
						LoadXmlFileToDataSet(dir.FullName + @"\" + file + ".xml" );
					}
				}
				else
				{
					foreach (DirectoryInfo schemas in dir.GetDirectories()) // цикл по схемам (doc, ml, ref, ...)
					{
						foreach (FileInfo file in schemas.GetFiles("*.xml"))
						{
							LoadXmlFileToDataSet(file.FullName);
						}
					}
				}
			}
		}

		private void LoadXmlFileToDataSet(string fileDir)
		{
			if (File.Exists(fileDir))
			{
				Debug.WriteLine(fileDir);
				try
				{
					DataSet.ReadXml(fileDir, XmlReadMode.ReadSchema);
				}
				catch (Exception e)
				{
					string msg = string.Empty;
					var tempDs = new DataSet();

					try
					{
						tempDs.ReadXml(fileDir, XmlReadMode.ReadSchema);
						msg = "Проблема при загрузке файла {0} в датасет, содержащий данные. Это означает, что в пустой датасет этот файл загружается без проблем. {1}";
					}
					catch (Exception e2)
					{
						msg = "Проблема при загрузке файла {0} в пустой датасет: {1}";
					}

					throw new Exception(string.Format("Возможно поможет svn cleanup - удалите файлы, отсутствующие в SVN. " + msg, fileDir, e.Message));
				}
			}
		}

		/// <summary>
		/// В каждой таблице набора данных строки с пустыми идентификаторами получают новое значение Id
		/// </summary>
		private void FillEmptyIdsInDataSet()
		{
			return;
			Func<string, int> comparator = name => name == Names.RefEntity ? 1 : name == Names.RefEntityField ? 2 : 3;
			List<DataTable> tables = DataSet.Tables.Cast<DataTable>()
				.Where(t => t.Columns.Contains(Names.Id)) // только таблицы, содержащие колонку id
				.OrderBy(t => comparator(t.TableName))    // в порядке: Entity, EntityField, остальные
				.ToList();
			
			foreach (DataTable table in tables)
			{
				int id = GetNewId(table.TableName);

				foreach (var row in table.AsEnumerable().Where(r => r.Field<int?>(Names.Id) == null))
				{
					row[Names.Id] = id++;
				}
			}
		}

		/// <summary>
		/// Возвращает список директорий DbStructure и DbEnums
		/// </summary>
		/// <param name="startPath">Путь, от которого начинается поиск</param>
		/// <returns></returns>
		private List<string> GetDirectories(string startPath)
		{
			List<string> result = new List<string>();
			DirectoryInfo directoryInfo = new DirectoryInfo(startPath);
			foreach (DirectoryInfo nameDirectory in directoryInfo.GetDirectories())
			{
				if (nameDirectory.Name == "DbStructure" || nameDirectory.Name == "DbEnums")
					result.Add(startPath + @"\" + nameDirectory);
				else
					result.AddRange(GetDirectories(startPath + @"\" + nameDirectory));
			}
			return result;
		}

		/// <summary>
		/// Считывает перечисления из файловой системы(.dll .xml) в DataSet
		/// </summary>
		/// <remarks>
		/// В DataSet сначало вставляются перечисления у которых указан идентификатор для сущности
		/// </remarks>
		private void FromFsEnums(string path, bool isPlatformDeploy)
		{
			var enumsFetcher = new EnumsFetcher();
			var enumsMerger = new EnumsMerger(this);

			List<DataTable> enumsTables = enumsFetcher.GetEnums(path, isPlatformDeploy ? Names.PrimaryEnums : null);
			if (enumsTables.Any())
				enumsMerger.Merge(enumsTables);
		}

		#endregion

		#region ToFs

		/// <summary>
		/// Выгрузить метаданные (из ОММ, не из БД!) в папку.
		/// </summary>
		/// <param name="path">Путь к папке</param>
		/// <remarks>
		/// Выгрузка осуществляется в указанную папку. Выгружаются все сущности(Перечисления выгружаются в DbStructure\enm).
		/// </remarks>
		public void WhriteDataSet(string path)
		{
			var outputDir = new DirectoryInfo(path);
			if (!outputDir.Exists)
				Directory.CreateDirectory(path);

			foreach (DataTable table in DataSet.Tables)
			{
				if (table.Columns.IndexOf(Names.Tstamp) >= 0)
					table.Columns.Remove(Names.Tstamp);

				string[] names = table.TableName.Split(new[] { '.' });
				string schemaName = names[0];
				string tableName = names[1];

				if (table.Rows.Count == 0)// не выгружаем если табличка пустая
					continue;

				string filePathInProject = Names.DbStructure + "\\" + schemaName + "\\" + tableName + ".xml";
				var outputPath = Path.Combine(outputDir.FullName, filePathInProject);

				table.WriteXmlToFile(outputPath, xmlWriteMode);
			}
		}

		/// <summary>
		/// Выгрузить метаданные (из ОММ, не из БД!) в папку.
		/// </summary>
		/// <param name="path">Путь к папке проект</param>
		/// <param name="delete">Признак удаления файлов xml, если true то файлы удаляются</param>
		/// <remarks>
		/// Выгрузка осуществляется в папку DbStructure соответствующих проектов.
		/// </remarks>
		public void ToFs(string path, bool delete)
		{
			if (path.EndsWith(@"\")) path = path.Substring(0, path.Length - 1);
			DirectoryInfo rootDirectory = new DirectoryInfo(path);
			if (!rootDirectory.Exists)
				throw new Exception("Передан неверный путь");
			SetAllowNullToId();
			DataTable entityTable = DataSet.Tables[string.Format("{0}.{1}", Names.EntitySchema, Names.Entity)];
			if (entityTable.Columns.IndexOf(Names.IdProject) == -1)
			{
				exportEntitiesForProject(path, defaultProject, delete);
			}
			else
			{
				foreach (SolutionProject project in Enum.GetValues(typeof (SolutionProject)))
				{
					exportEntitiesForProject(path, project, delete);
				}
			}
		}

		private void exportEntitiesForProject(string solutionPath, SolutionProject project, bool delete)
		{
			var projectFiles = new List<string>();

			string projectName = project.ToString().Replace("_", ".");
			DirectoryInfo projectDir = new DirectoryInfo(string.Format("{0}\\{1}", solutionPath, projectName));

			ExportTables(projectDir, project, projectFiles);

			if(delete)
				SolutionHelper.CleanDbStructure(new DirectoryInfo(Path.Combine(solutionPath, projectName)), projectFiles);

			SolutionHelper.InsertToProject(projectFiles, projectDir.FullName, projectName, "Content");
		}

		private void ExportTables(DirectoryInfo projectDir, SolutionProject project, List<string> projectFiles)
		{
			foreach (DataTable table in DataSet.Tables)
			{
				string path;

				if (table.Columns.IndexOf(Names.Tstamp) >= 0)
					table.Columns.Remove(Names.Tstamp);

				string[] names = table.TableName.Split(new[] { '.' });
				string schemaName = names[0];
				string tableName = names[1];

				if (schemaName == "enm")
					continue; // не выгружаем таблицы перечислений (они уже описаны в виде enum'ов)
				if (table.Rows.Count == 0)// не выгружаем если табличка пустая
					continue;
				if (table.TableName == Names.RefEntity) // Выгружаем ref.Entity
				{
					if(table.Select(Names.IdProject + " is null").Any())
						throw new Exception("У сущности не заполнено поле idProject");
					ExportEntities(project, projectDir, table, projectFiles, false);// Без enums в папку DbStructure\ref
					ExportEntities(project, projectDir, table, projectFiles, true);// Enums в папку DbEnums
					continue;
				}
				if (table.TableName == Names.RefEntityField) //Выгружаем ref.EntityFields
				{
					ExportEntityFields(project, projectDir, table, projectFiles, false);// Без enums в папку DbStructure\ref
					ExportEntityFields(project, projectDir, table, projectFiles, true);// Enums в папку DbEnums
					continue;
				}
				if(table.TableName == Names.RefFilter)//фильтры вставляем вместе с EntityField
					continue;

				string filePathInProject = Names.DbStructure + "\\" + schemaName + "\\" + tableName + ".xml";
				path = Path.Combine(projectDir.FullName, filePathInProject);

				//Проверяем что таблица относиться к данному проекту
				if (GetProjectIdByEntity(tableName) == (int) project)
				{
					table.WriteXmlToFile(path, xmlWriteMode);
					projectFiles.Add(filePathInProject);
				}
			}
		}

		/// <summary>
		/// Экспорт полей сущностей проекта.
		/// </summary>
		/// <param name="project">Проект</param>
		/// <param name="projectDir">Папка проекта</param>
		/// <param name="entityFieldTable">Таблица с полями всех сущностей</param>
		/// <param name="projectFiles">Файлы которые необходимо вставить в проект</param>
		/// <param name="isEnums">Признак перечисления</param>
		/// <remarks>Поля сущности фильтруются по проекту указаному в сущности</remarks>
		private void ExportEntityFields(SolutionProject project, DirectoryInfo projectDir, DataTable entityFieldTable, List<string> projectFiles, bool isEnums)
		{
			//Расположение файла относительно папки проекта
			string filePathInProject = (isEnums ? Names.DbEnums : Names.DbStructure + "\\" + Names.EntityFieldSchema) + "\\" + Names.EntityField + ".xml";
			//Полный путь до файла для его записи
			string path = Path.Combine(projectDir.FullName, filePathInProject);

			DataTable projectEntityFields = entityFieldTable.Clone();
			//получаем нужные поля сущности
			FilterEntityFields((int) project, projectEntityFields, isEnums);

			if (projectEntityFields.Rows.Count > 0)
			{
				projectEntityFields.WriteXmlToFile(path, xmlWriteMode);
				projectFiles.Add(filePathInProject);
			}
			//Вставка фильтров относящихся к данным полям
			ExportFilters(projectEntityFields, projectFiles, projectDir);
		}

		private void ExportFilters(DataTable projectEntityFields, List<string> projectFiles, DirectoryInfo projectDir)
		{
			DataTable filters = DataSet.Tables[Names.RefFilter].Clone();
			
			var str = string.Join(",", projectEntityFields.AsEnumerable().Select(row => row.Field<int>("id")).ToArray());
			DataRow[] rows = DataSet.Tables[Names.RefFilter].Select("idEntityField in (" + (string.IsNullOrEmpty(str) ? "NULL" : str) + ")");
			if (rows.Any())
				filters.CloneAndAddRows(rows);

			if (filters.Columns.IndexOf(Names.Tstamp) >= 0)
				filters.Columns.Remove(Names.Tstamp);

			if (filters.Rows.Count > 0)
			{
				string fileFilterPathInProject = Names.DbStructure + "\\" + Names.FilterSchema + "\\" + Names.Filter + ".xml";
				string pathFilter = Path.Combine(projectDir.FullName, fileFilterPathInProject);

				filters.WriteXmlToFile(pathFilter, xmlWriteMode);
				projectFiles.Add(fileFilterPathInProject);
			}
		}

		/// <summary>
		/// Экспорт записей из справочника Enitty
		/// </summary>
		/// <param name="project">
		/// Проект, для которого будет выполнен экспорт. 
		/// Если указан проект по умолчанию (Platform.PrimaryEntities), 
		/// то в него будут также выгружены все сущности, не привязанные к какому-либо проекту (строки, у которых не заполнено значение поля idProject).
		/// </param>
		/// <param name="projectDir">Папка проекта</param>
		/// <param name="entityTable">Таблица Entity со списком строк</param>
		/// <param name="isEnums">Признак перечисления</param>
		/// <remarks>
		/// Если у таблицы Entity отсутствует колонка idProject, то будут экспортированы все сущности.
		/// </remarks>
		/// <returns>Таблица со списком сущностей, относящиеся к проекту <paramref name="project"/></returns>
		private void ExportEntities(SolutionProject project, DirectoryInfo projectDir, DataTable entityTable, List<string> projectFiles, bool isEnums)
		{
			DataTable table = entityTable.Clone();
			//Расположение файла относительно папки проекта
			string filePathInProject = (isEnums ? Names.DbEnums : Names.DbStructure + "\\" + Names.EntitySchema) + "\\" + Names.Entity + ".xml";
			//Полный путь до файла для его записи
			string path = Path.Combine(projectDir.FullName, filePathInProject);

			// определяем фильтр
			string filter = " idEntityType " + (isEnums ? "=" : "<>") + " 1 "; // перечисления не рассматриваем
			filter += " AND " + getFilterByProject(entityTable, project);

			table.CloneAndAddRows(entityTable.Select(filter));
			if (table.Rows.Count > 0)
			{
				table.WriteXmlToFile(path, xmlWriteMode);
				projectFiles.Add(filePathInProject);
			}
		}

		/// <summary>
		/// Если в <paramref name="table"/> есть колонка idProject, то возвращает выражение фильтра
		/// Примеры:
		/// 1. AND (idProject = 1 OR idProject IS NULL) // если проект является проектом по умолчанию
		/// 2. AND (idProject = 2) // для остальных проектов
		/// Если же колонки нет, то возвращается пустая строка.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="project"></param>
		/// <returns></returns>
		private string getFilterByProject(DataTable table, SolutionProject project)
		{
			int projectNum = (int)project;
			string filter = string.Empty;
			if (table.Columns.Contains(Names.IdProject))
			{
				if (project == defaultProject)
					filter = string.Format("({0} = {1} OR {0} IS NULL)", Names.IdProject, projectNum);
				else
					filter = string.Format("{0} = {1}", Names.IdProject, projectNum);
			}
			return filter;
		}

		/// <summary>
		/// Получение идентификатора проекта по имени сущности
		/// </summary>
		/// <param name="entityName">Системное имя сущности</param>
		/// <returns></returns>
		private int GetProjectIdByEntity(string entityName)
		{
			int? result = null;
			DataRow row = DataSet.Tables[Names.RefEntity].AsEnumerable().SingleOrDefault(a => a.Field<string>(Names.Name) == entityName);
			if (row != null)
				result = Convert.ToInt32(row[Names.IdProject]);

			return result ?? (int) defaultProject;
		}

		private void FilterEntityFields(int projectId, DataTable entityFields, bool isEnum)
		{
			var fields = from entityField in DataSet.Tables[Names.RefEntityField].AsEnumerable()
			             join entity in DataSet.Tables[Names.RefEntity].AsEnumerable() on (int) entityField[Names.IdEntity]
				             equals (int) entity[Names.Id]
			             where (int?) entity[Names.IdProject] == projectId
			                   && (isEnum ? 1 == (byte) entity[Names.IdEntityType] : 1 != (byte) entity[Names.IdEntityType])
			             select entityField;

			entityFields.CloneAndAddRows(fields.ToArray());
		}

		#endregion

		#region Other Public Members

		/// <summary>
		/// Возвращает новый идентифкатор для указанной таблицы и данного разработчика (devId)
		/// </summary>
		/// <param name="tableName">Имя таблицы (вместе со схемой), для которой нужно получить значение нового идентификатора</param>
		/// <returns>Значение идентификатора</returns>
		public int GetNewId(string tableName)
		{
			if (!DataSet.Tables.Contains(tableName))
				throw new Exception(string.Format("В DataSet не загружена таблица {0}", tableName));
			int result = (int)DataSet.Tables[tableName].Compute("max(id)+1", string.Format("id<{0}", MaxDeveloperId));
			if (result < MinDeveloperId)
				result = MinDeveloperId + 1;
			return result;
		}

		#endregion

		#region Other Private Methods

		/// <summary>
		/// Минимальное значение идентификатора
		/// </summary>
		private int MinDeveloperId
		{
			get { return int.MaxValue / 32 * (MigrationHelper.NumDeveloper - 1); }
		}

		/// <summary>
		/// Максимальное значение идентификатора
		/// </summary>
		private int MaxDeveloperId
		{
			get { return int.MaxValue / 32 * (MigrationHelper.NumDeveloper); }
		}

		#endregion
	}
}
