using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using SharpSvn;
using Tools.MigrationHelper.Core.Context;
using Tools.MigrationHelper.Core.EnumsProcessing;
using Tools.MigrationHelper.Core.Helpers;
using Tools.MigrationHelper.EnumsProcessing;
using Tools.MigrationHelper.Extensions;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.DbManager
{
	/// <summary>
	/// Объектная модель метаданныех БД. Полностью определяет структуру БД. 
	/// Содержит списки строк таблиц и списки программируемых объектов.
	/// </summary>
	public class DbDataSet
	{
        /// <summary>
        /// DataSet содержащий предзаполненные данные
        /// </summary>
        public DataSet DataSet;

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

	    private ProgrammabilityContext _context;

	    private int? progEntityId
	    {
	        get { return Platform.PrimaryEntities.Reference.Programmability.EntityIdStatic;}
	    }
	

		#endregion

        #region Constructors

        /// <summary>
		/// Конструктор
		/// </summary>
		public DbDataSet(int devId)
		{
		    _devId = devId;
		    DataSet = new DataSet();
			_isDeveloper = devId > 0;
		}

		public DbDataSet(int devId, TasksEnum task)
		{
			_devId = devId;
			_task = task;
			_isDeveloper = devId > 0;
			DataSet = new DataSet();
		}

		public DbDataSet(int devId, string connectionString)
		{
			_devId = devId;
			_isDeveloper = devId > 0;
			DataSet = new DataSet();
			_connectionString = connectionString;
            _context = new ProgrammabilityContext(_connectionString);
		}

		public DbDataSet(int devId, string connectionString, TasksEnum task)
		{
			_devId = devId;
			_task = task;
			_isDeveloper = devId > 0;
			DataSet = new DataSet();
			_connectionString = connectionString;
		}

        #endregion

	    #region FromDb

	    /// <summary>
	    /// Получить метаданные из БД
	    /// </summary>
	    /// <param name="connection">Соединение с БД</param>
	    /// <param name="isDeveloper"></param>
	    public void FromDb(SqlConnection connection)
		{
			FromDbMetadataTables(connection, _isDeveloper);
			FromDbOtherTables(connection, _isDeveloper);
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
                    selectStmt = !hasId ? sqlAllItems : sqlPredefinedItems;
				}

                var schema = Platform.PrimaryEntities.Schemas.ByEntityType((EntityType)entityType);
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(string.Format(
					selectStmt,
					schema,
					row[Names.Name]
				), connection) { MissingSchemaAction = MissingSchemaAction.AddWithKey };

				var tableName = string.Format("{0}.{1}", schema, row[Names.Name]);
			    try
			    {
                    sqlDataAdapter.Fill(DataSet, tableName);
			    }
			    catch (Exception e)
			    {
			        throw new Exception(string.Format("Ошибка загрузки таблицы '{0}' в Датасет: {1}", tableName, e.Message));
			    }
				
			}
		}
		#endregion

		#region FromFs

		/// <summary>
		/// Считывает метаданные из файловой системы
		/// </summary>
		/// <param name="path">Путь до проекта, от которого начинается поиск директорий DbStructure</param>
		public void FromFs(string path)
		{
			var xmlPath = path + (_task == TasksEnum.DeployPlatformDb ? "\\" + SolutionProject.Tools_MigrationHelper_Core.ToString().Replace("_", ".") : "");
			List<string> listDirectories = SolutionHelper.GetXMLDirectories(xmlPath);
			if (listDirectories.Count==0)
				throw new Exception("Не найдено ни одного файла");
			FromFsTables(listDirectories, path);
			FromFsEnums(path);
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

	    private readonly int _devId;
		private readonly TasksEnum? _task = null;
		private readonly bool _isDeveloper;
	    /// <summary>
		/// Считывает данные из файловой системы для всех таблиц
		/// </summary>
		/// <param name="listDirectories">Список директорий DbStructure и DbEnums</param>
		private void FromFsTables(IEnumerable<string> listDirectories, string path)
	    {
	        var svnExists = true;
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

                            //Записываем свн версию файла для того чтоб при ToFs вернуть на исходную ревизию(для исключения потери данных)
							if (_isDeveloper && _task == TasksEnum.DeployAppDb && svnExists)
							{
                                //пробуем один раз, если неудачно считаем что свн нет и будем работать без него
							    try
							    {
                                    SvnVersionToDataBase(file.FullName, path);
							    }
							    catch
							    {
							        svnExists = false;
							    }
							}
						}
					}
				}
			}
		}

	    private void SvnVersionToDataBase(string fullName, string path)
	    {
            using (var svnClient = new SvnClient())
            {
                SvnInfoEventArgs version;
                svnClient.GetInfo(fullName, out version);
                var context = new DevDbRevisionContext(_connectionString);
                var devDbRevision = new DevDbRevision
                {
                    Path = SolutionHelper.GetFileSolutionInfo(fullName, path),
                    Revision = (int)version.Revision
                };
                context.DevDbRevision.Add(devDbRevision);
                context.SaveChanges();
            }
	    }

	    private void LoadXmlFileToDataSet(string fileDir)
		{
		    var file = new FileInfo(fileDir);
            var tableName = string.Format("{0}.{1}", file.Directory.Name, Path.GetFileNameWithoutExtension(file.Name));
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
			        var errorFlag = false;
			        try
			        {
			            tempDs.ReadXml(fileDir, XmlReadMode.ReadSchema);
			            msg =
			                "Проблема при загрузке файла {0} в датасет, содержащий данные. Это означает, что в пустой датасет этот файл загружается без проблем. {1}";
			        }
			        catch (Exception)
			        {
			            msg = "Проблема при загрузке файла {0} в пустой датасет: {1}";
			            errorFlag = true;
			        }

			        msg += string.Format("{0}============{0}{1}{0}============={0}",
                        Environment.NewLine,
                        errorFlag ? getErrorMessage(tempDs.Tables.Cast<DataTable>().Last()) : getErrorMessage(DataSet.Tables[tableName])
                    );

                    throw new InvalidOperationException(string.Format(
			            "Возможно поможет svn cleanup - удалите файлы, отсутствующие в SVN. " + msg, fileDir, e.Message));
			    }
			}
		}

        private string getErrorMessage(DataTable dt)
        {
            var errors = dt.GetErrors();

            return !errors.Any() ? String.Empty : 
                     errors
                     .Select(err => err.RowError)
                     .Aggregate((a, b) => string.Format("{0}{1}{2}", a, Environment.NewLine, b));
        }

		/// <summary>
		/// Считывает перечисления из файловой системы(.dll .xml) в DataSet
		/// </summary>
		/// <remarks>
		/// В DataSet сначало вставляются перечисления у которых указан идентификатор для сущности
		/// </remarks>
		private void FromFsEnums(string path)
		{
			if (!_isDeveloper) return;

            var enumsParser = new EnumsParser(path, _task == TasksEnum.DeployPlatformDb ? Names.PrimaryEnums : null);
//		    var enumsFetcher = new EnumsFetcher();
			var enumsMerger = new EnumsMerger(this);

//            var check = enumsFetcher.GetEnums(path, _task == TasksEnum.DeployPlatformDb ? Names.PrimaryEnums : null);

            List<DataTable> enumsTables = enumsParser.GetEnums();
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
		public void WhriteDataSetWithStructure(string path)
		{
			var outputDir = new DirectoryInfo(path);
			if (!outputDir.Exists)
				Directory.CreateDirectory(path);

			foreach (SolutionProject project in Enum.GetValues(typeof(SolutionProject)))
			{
				var projectFiles = new List<string>();
				string projectName = project.ToString().Replace("_", ".");
				DirectoryInfo projectDir = new DirectoryInfo(string.Format("{0}\\{1}", outputDir.FullName, projectName));
				ExportTables(projectDir, project, projectFiles, true);
			}
		}

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
            //Возврат исходной ревизии для xml файлов
			UpdateFilesRevision(path);

			if (path.EndsWith(@"\")) path = path.Substring(0, path.Length - 1);
			DirectoryInfo rootDirectory = new DirectoryInfo(path);
			if (!rootDirectory.Exists)
				throw new Exception("Передан неверный путь");
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

			ExportTables(projectDir, project, projectFiles,false);

			if(delete)
				SolutionHelper.CleanDbStructure(new DirectoryInfo(Path.Combine(solutionPath, projectName)), projectFiles);

			SolutionHelper.InsertToProject(projectFiles, projectDir.FullName, projectName, "Content");
		}

		private void ExportTables(DirectoryInfo projectDir, SolutionProject project, List<string> projectFiles, bool enumsToXml)
		{
			foreach (DataTable table in DataSet.Tables)
			{
				string path;
                string filePathInProject;

				if (table.Columns.IndexOf(Names.Tstamp) >= 0)
					table.Columns.Remove(Names.Tstamp);

				string[] names = table.TableName.Split(new[] { '.' });
				string schemaName = names[0];
				string tableName = names[1];

                if (table.TableName == Names.RegUpdateRevision)
                    continue; // не выгружаем таблицу сюда записываются обновления
				if (schemaName == Names.Enm && !enumsToXml)
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
				if (table.TableName == Names.RefIndex || table.TableName == Names.MlIndexEntityField || table.TableName == Names.MlIncludedEntityField)//Справочник Индексы и мультилинк вставляем вместе с сущностями
					continue;
                if (table.TableName == Names.RefProgrammability)
			    {
			        foreach (DataRow dataRow in table.Rows)
			        {
                        if((int)dataRow[Names.IdProject] != (int)project)
                            continue;
			            var rowName = dataRow[Names.Name].ToString();
			            var newTable = table.Clone();
			            newTable.ImportRow(dataRow);

			            filePathInProject = Names.DbStructure + "\\" + Names.Programmability + "\\" + rowName + ".xml";
			            path = Path.Combine(projectDir.FullName, filePathInProject);
			            newTable.WriteXmlToFile(path, xmlWriteMode);
			            projectFiles.Add(filePathInProject);
			        }
			        continue;
			    }
//                if (table.TableName == Names.RegItemsDependencies)
//                    table.Columns.Remove(Names.Id);

			    filePathInProject = Names.DbStructure + "\\" + schemaName + "\\" + tableName + ".xml";
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
			DataTable entityTableToWrite = entityTable.Clone();
			//Расположение файла относительно папки проекта
			string filePathInProject = (isEnums ? Names.DbEnums : Names.DbStructure + "\\" + Names.EntitySchema) + "\\" + Names.Entity + ".xml";
			//Полный путь до файла для его записи
			string path = Path.Combine(projectDir.FullName, filePathInProject);

			// определяем фильтр
			string filter = " idEntityType " + (isEnums ? "=" : "<>") + " 1 "; // перечисления не рассматриваем
			filter += " AND " + getFilterByProject(entityTable, project);

			entityTableToWrite.CloneAndAddRows(entityTable.Select(filter));
			if (entityTableToWrite.Rows.Count > 0)
			{
				entityTableToWrite.WriteXmlToFile(path, xmlWriteMode);
				projectFiles.Add(filePathInProject);
			}

			ExportIndexes(entityTableToWrite, projectFiles, projectDir);
		}

        /// <summary>
        /// Выкладываем индексы соответствующие сущностям в те же проекты
        /// </summary>
        /// <param name="entityTable">Набор сущностей</param>
        /// <param name="projectFiles">Файлы проекта</param>
        /// <param name="projectDir">Папка проекта</param>
		private void ExportIndexes(DataTable entityTable, List<string> projectFiles, DirectoryInfo projectDir)
		{
			DataTable indexes = DataSet.Tables[Names.RefIndex].Clone();

			var str = string.Join(",", entityTable.AsEnumerable().Select(row => row.Field<int>("id")).ToArray());
			DataRow[] rows = DataSet.Tables[Names.RefIndex].Select("idEntity in (" + (string.IsNullOrEmpty(str) ? "NULL" : str) + ")");
			if (rows.Any())
				indexes.CloneAndAddRows(rows);

			if (indexes.Columns.IndexOf(Names.Tstamp) >= 0)
				indexes.Columns.Remove(Names.Tstamp);

			if (indexes.Rows.Count > 0)
			{
				string fileIndexPathInProject = Names.DbStructure + "\\" + Names.Ref + "\\" + Names.Index + ".xml";
				string pathFilter = Path.Combine(projectDir.FullName, fileIndexPathInProject);

				indexes.WriteXmlToFile(pathFilter, xmlWriteMode);
				projectFiles.Add(fileIndexPathInProject);
			}
            ExportIndexMl(indexes, projectFiles, projectDir, Names.IndexEntityField);
            ExportIndexMl(indexes, projectFiles, projectDir, Names.IncludedEntityField);
		}

	    private void ExportIndexMl(DataTable indexTable, List<string> projectFiles, DirectoryInfo projectDir,
	                               string indexMlTable)
	    {
		    string tableName = Names.Ml + "." + indexMlTable;
			if (DataSet.Tables[tableName] == null)
	            return;

			DataTable mlIndex = DataSet.Tables[tableName].Clone();

            var str = string.Join(",", indexTable.AsEnumerable().Select(row => row.Field<int>("id")).ToArray());
			DataRow[] rows = DataSet.Tables[tableName].Select("idIndex in (" + (string.IsNullOrEmpty(str) ? "NULL" : str) + ")");
            if (rows.Any())
                mlIndex.CloneAndAddRows(rows);

            if (mlIndex.Columns.IndexOf(Names.Tstamp) >= 0)
                mlIndex.Columns.Remove(Names.Tstamp);

            if (mlIndex.Rows.Count > 0)
            {
				string fileMlIndexPathInProject = Names.DbStructure + "\\" + Names.Ml + "\\" + indexMlTable + ".xml";
                string pathFilter = Path.Combine(projectDir.FullName, fileMlIndexPathInProject);

                mlIndex.WriteXmlToFile(pathFilter, xmlWriteMode);
                projectFiles.Add(fileMlIndexPathInProject);
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

		private void UpdateFilesRevision(string path)
		{
			if (!_isDeveloper) return;
			try
			{
				using (SvnClient svnClient = new SvnClient())
				{
                    var context = new DevDbRevisionContext(_connectionString);
					var DevDbRevision = context.DevDbRevision.ToList();
					foreach (var file in DevDbRevision)
					{
						var filePath = Path.Combine(path, file.Path);
						if (!File.Exists(filePath))
							throw new Exception(
								string.Format(
									"Файл {0} не найден. Возможно он был переименован или удален. Вам следует вернуть данный файл на ревизию {1} и повторить операцию ToFs",
									filePath, file.Revision));
						SvnInfoEventArgs version;
						svnClient.GetInfo(filePath, out version);
						if (version.Revision != file.Revision)
						{
							svnClient.Update(filePath, new SvnUpdateArgs { Revision = file.Revision});
						}
					}
				}
			}
			catch
			{
			}
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
			get { return -(int.MaxValue / 32) * (32-(_devId - 1)); }
		}

		/// <summary>
		/// Максимальное значение идентификатора
		/// </summary>
		private int MaxDeveloperId
		{
			get { return -(int.MaxValue / 32) * (32-_devId); }
		}

		#endregion

        #region ItemDependencies

        public void FillDependencies()
	    {
            if(progEntityId == null)
                throw new Exception("Не найдена сущность Programmability");

            foreach (var programmability in _context.Programmabilities.ToList())
            {
                InsertDep(programmability.Id, (int) progEntityId, programmability.Schema, programmability.Name);
            }

            var entities = _context.Database.SqlQuery<Platform.PrimaryEntities.Reference.Entity>(string.Format(SelectEntities, (int)CalculatedFieldType.DbComputed, (int)CalculatedFieldType.DbComputedPersisted)).ToList();

	        foreach (var entity in entities)
	        {
	            InsertDep(entity.Id, entity.EntityId, entity.Schema, entity.Name);
	        }
	    }

        private void InsertDep(int idObject, int idObjectEntity, string objectSchema, string objectName)
	    {
            var result = _context.Database.SqlQuery<Sp_dependencies>(string.Format(SelectDependencies, objectSchema, objectName)).ToList();
            
            if(!result.Any())
                _context.Database.ExecuteSqlCommand(string.Format(DeleteRowItemsDependencies, idObject, idObjectEntity));
     
            foreach (var dep in result)
            {
                var itemDep = new ItemsDependency();
                itemDep.IdObject = idObject;
                itemDep.IdObjectEntity = idObjectEntity;
                var entityType = Platform.PrimaryEntities.Schemas.EntityTypeBySchema(dep.Schema);

                if (!string.IsNullOrEmpty(dep.Field))
                {
                    if (entityType == null)
                        continue;

                    var id = _context.Database.SqlQuery<int?>(string.Format(SelectFieldId, dep.Entity, dep.Field,
                                                                      (int)entityType)).FirstOrDefault();
                    if (id != null)
                    {
                        itemDep.IdDependsOn = (int) id;

                        itemDep.IdDependsOnEntity = Platform.PrimaryEntities.Reference.EntityField.EntityIdStatic;

                        if (!_context.ItemsDependencies.Any(a => a.IdObject == itemDep.IdObject
                                                                 && a.IdObjectEntity == itemDep.IdObjectEntity
                                                                 && a.IdDependsOn == itemDep.IdDependsOn
                                                                 && a.IdDependsOnEntity == itemDep.IdDependsOnEntity))
                        {
                            _context.ItemsDependencies.Add(itemDep);
                        }
                    }
                }
                else
                {
                    var id = _context.Database.SqlQuery<int?>(string.Format(SelectObjectId, dep.Schema, dep.Entity, "1,2,3"))
                                .FirstOrDefault();
                    if (id != null)
                    {
                        itemDep.IdDependsOn = (int) id;
                        itemDep.IdDependsOnEntity = (int) progEntityId;

                        if (!_context.ItemsDependencies.Any(a => a.IdObject == itemDep.IdObject
                                                                 && a.IdObjectEntity == itemDep.IdObjectEntity
                                                                 && a.IdDependsOn == itemDep.IdDependsOn
                                                                 && a.IdDependsOnEntity == itemDep.IdDependsOnEntity))
                        {
                            _context.ItemsDependencies.Add(itemDep);
                        }
                    }
                }
                _context.SaveChanges();
            }
	    }

	    public class Sp_dependencies
        {
            public string Schema { get; set; }
            public string Entity { get; set; }
            public string Field { get; set; }
        }

	    public const string SelectEntities =
	        "SELECT * FROM [ref].[Entity] e WHERE EXISTS(SELECT * FROM ref.EntityField ef WHERE ef.[idEntity] = e.id AND (ef.idCalculatedFieldType = {0} OR ef.idCalculatedFieldType = {1}))";

	    public const string SelectFieldId =
	        "SELECT ef.id FROM ref.EntityField ef LEFT JOIN ref.Entity e ON e.id = ef.[idEntity] WHERE e.Name = '{0}' AND e.idEntityType = '{2}' AND ef.Name = '{1}'";

	    public const string SelectObjectId =
	        "SELECT p.id FROM ref.Programmability p WHERE p.[Schema] = '{0}' AND p.Name = '{1}' AND p.idProgrammabilityType IN ({2})";

	    public const string DeleteRowItemsDependencies =
	        "DELETE FROM reg.ItemsDependencies WHERE IDObject = {0} AND IDObjectEntity = {1}";

	    public const string SelectDependencies =
	        "SELECT referenced_schema_name as [schema], referenced_entity_name as [entity], referenced_minor_name as [field] FROM sys.dm_sql_referenced_entities('{0}.{1}', 'OBJECT')";

	    #endregion
	}
}
