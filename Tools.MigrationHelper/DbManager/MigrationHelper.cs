using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using MigrationHelper;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Tools.MigrationHelper.EnumsProcessing;
using Tools.MigrationHelper.Helpers;
using Tools.MigrationHelper.Logger;

namespace Tools.MigrationHelper.DbManager
{
	/// <summary>
	/// Утилита для обновления и инсталляции системы БИС-СБОР
	/// http://conf.rostu-comp.ru/display/Platform3/Migration+Helper+3.0
	/// </summary>
	public class MigrationHelper
	{
		private readonly string _pathPlatformDbScripts = AppDomain.CurrentDomain.BaseDirectory + @"\PlatformDb\Scripts";
		private readonly string _pathPlatformDbData = AppDomain.CurrentDomain.BaseDirectory + @"\PlatformDb\Data";
		private readonly string _path = AppDomain.CurrentDomain.BaseDirectory;
		
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="numDeveloper">Номер разработчика</param>
		public MigrationHelper(int numDeveloper)
		{
			NumDeveloper = numDeveloper;
			this.Report = new Report();
		}

		#region Public Properties

		/// <summary>
		/// Номер разработчика
		/// </summary>
		public static int NumDeveloper { get; private set; }
		
		/// <summary>
		/// Признак, определящий с какой БД идет работа (true - разработчик, false - пользователь)
		/// </summary>
		public bool IsDeveloper { get; set; }

		public string SvnLogin { get; set; }
		public string SvnPassword { get; set; }
		public string SvnPath { get; set; }
		
		/// <summary>
		/// Путь к проекту
		/// </summary>
		public string ProjectPathSource { get; set; }

		/// <summary>
		/// Путь к проекту
		/// </summary>
		public string ProjectPathTarget { get; set; }

		/// <summary>
		/// Строка подключения к БД
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// Отчет, полученный после выполнения команд.
		/// Значение будет установлено только в режиме работы Verbose = true.
		/// </summary>
		public Report Report { get; set; }

		#endregion

		#region private Members

		private SqlConnection _conn;
		
		private SqlConnection Connection
		{
			get
			{
				if (_conn == null)
				{
					_conn = new SqlConnection(ConnectionString);
					_conn.Open();
				}
				return _conn;
			}
		}

		#endregion

		/// <summary>
		/// Создание БД
		/// </summary>
		private void CreateDb()
		{
			SqlConnectionStringBuilder sqlConnectionStringBuilder=new SqlConnectionStringBuilder(ConnectionString);
			string dbName = sqlConnectionStringBuilder.InitialCatalog;
			sqlConnectionStringBuilder.InitialCatalog = "master";
			DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
			FileInfo fileInfo = directoryInfo.GetFiles("createDb.sql")[0];
			string[] textCommands = fileInfo.OpenText().ReadToEnd().Split(new char[] { '\r', '\n' });
			using (SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
			{
				connection.Open();
				foreach (string textCommand in textCommands)
				{
					if (!string.IsNullOrWhiteSpace(textCommand))
					{
						using (SqlCommand command = new SqlCommand(string.Format(textCommand, dbName), connection))
						{
							command.ExecuteNonQuery();
						}
					}
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Выполнение списка команд перечисленного в файле
		/// </summary>
		/// <param name="file">Файл</param>
		private void ExecListCommandFromFile(FileInfo file)
		{
			string[] textCommands = file.OpenText().ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				foreach (string textCommand in textCommands)
				{
					using (SqlCommand command = new SqlCommand(textCommand, connection))
					{
						command.ExecuteNonQuery();
					}
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Создание схем а БД
		/// </summary>
		private void CreateSchemas()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
			FileInfo fileInfo = directoryInfo.GetFiles("CreateSchemas.list")[0];
			if (!fileInfo.Exists)
				throw new Exception("Не найден файл CreateSchemas.list");
			ExecListCommandFromFile(fileInfo);
		}

		/// <summary>
		/// Создание базовых таблиц
		/// </summary>
		private void CreateBaseTable()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
			FileInfo fileInfo = directoryInfo.GetFiles("createBaseTable.sql")[0];
			string textCommand = fileInfo.OpenText().ReadToEnd();
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(textCommand, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Загружает данные в DataSet из Tools.MigrationHelper/PlatformDb/Data
		/// </summary>
		/// <returns></returns>
		private DataSet LoadPlatformDbData()
		{
			var targetFs = new Metadata();

			targetFs.FromFs(ProjectPathSource, 1,true);

			return targetFs.DataSet;
		}

		/// <summary>
		/// Вставляет данные из DataSet в БД
		/// </summary>
		/// <param name="dataSet"></param>
		private void FillPlatformDbData(DataSet dataSet)
		{
			foreach (DataTable table in dataSet.Tables)
			{
				bool isAutoincrement = table.Columns[Names.Id].AutoIncrement;
				StringBuilder textCommand=new StringBuilder();
				List<string> fieldsName = (from DataColumn column in table.Columns where column.ColumnName != Names.Tstamp select column.ColumnName).ToList();
				List<string> fieldsName2 = (from DataColumn column in table.Columns where column.ColumnName != Names.Tstamp select "[" + column.ColumnName + "]").ToList();
				List<string> paramsName = (from DataColumn column in table.Columns where column.ColumnName != Names.Tstamp select "@" + column.ColumnName).ToList();
				textCommand.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", table.TableName, string.Join(",", fieldsName2), string.Join(",", paramsName));
				using (SqlConnection connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand("", connection))
					{
						if (isAutoincrement)
						{
							command.CommandText = string.Format("SET IDENTITY_INSERT {0} ON", table.TableName);
							command.ExecuteNonQuery();
						}
						command.CommandText = textCommand.ToString();
						foreach (DataRow row in table.Rows)
						{
							command.Parameters.Clear();
							foreach (string fieldName in fieldsName)
							{
								command.Parameters.AddWithValue("@" + fieldName, row[fieldName]);
							}
							command.ExecuteNonQuery();
						}
						if (isAutoincrement)
						{
							command.CommandText = string.Format("SET IDENTITY_INSERT {0} OFF", table.TableName);
							command.ExecuteNonQuery();
						}
					}
					connection.Close();
				}
			}
		}

		/// <summary>
		/// Создание View для базовых таблиц
		/// </summary>
		private void CreateBaseView()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
			FileInfo fileInfo = directoryInfo.GetFiles("CreateBaseView.list")[0];
			if (!fileInfo.Exists)
				throw new Exception("Не найден файл CreateBaseView.list");
			ExecListCommandFromFile(fileInfo);
		}


		private string GetAssemblyBits(string assemblyPath)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("0x");

			using (FileStream stream = new FileStream(assemblyPath,
			                                          FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				int currentByte = stream.ReadByte();
				while (currentByte > -1)
				{
					builder.Append(currentByte.ToString("X2", CultureInfo.InvariantCulture));
					currentByte = stream.ReadByte();
				}
			}

			return builder.ToString();
		}

		/// <summary>
		/// Публикация сборок (*.dll) в БД
		/// </summary>
		private void CreateAssemblies()
		{
			DirectoryInfo directoryInfo2 = new DirectoryInfo(_pathPlatformDbScripts);
			FileInfo fileInfo2 = directoryInfo2.GetFiles("Assemblies.list")[0];
			string[] listAssemblies = fileInfo2.OpenText().ReadToEnd().Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
			const string textCommand = "CREATE ASSEMBLY [{0}] AUTHORIZATION [dbo] FROM {1} WITH PERMISSION_SET = SAFE";
			foreach (string assembly in listAssemblies)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(ProjectPathSource + @"\bin");
				if (!directoryInfo.Exists)
				{
					directoryInfo = new DirectoryInfo(ProjectPathSource + @"\" + assembly.Replace(".dll", @"\bin\debug"));
					if (!directoryInfo.Exists)
						throw new Exception("Не найден путь: '" + ProjectPathSource + @"\" + assembly.Replace(".dll", @"\bin\debug") + "'");
				}
				FileInfo file;
				if (directoryInfo.GetFiles(assembly).Length == 1)
				{
					file = directoryInfo.GetFiles(assembly)[0];
				}
				else
				{
					throw new Exception("Не найден файл: '" + directoryInfo.FullName + @"\" + assembly);
				}
				using (SqlConnection connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand("", connection))
					{
						command.CommandText = string.Format(textCommand, file.Name.Substring(0, file.Name.Length - 4), GetAssemblyBits(file.FullName));
						command.ExecuteNonQuery();
					}
					connection.Close();
				}
			}
		}

		/// <summary>
		/// Создание триггеров для таблиц ref.Entity и ref.EntityField
		/// </summary>
		private void CreateTriggers()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
			FileInfo fileInfo = directoryInfo.GetFiles("CreateTriggers.list")[0];
			if (!fileInfo.Exists)
				throw new Exception("Не найден файл CreateTriggers.list");
			ExecListCommandFromFile(fileInfo);
		}

		/// <summary>
		/// Изменение стартового значение IDENTITY для таблиц
		/// </summary>
		private void SetStartIdentity()
		{
			if (!IsDeveloper)
				return;
			int no = NumDeveloper - 1;
			const int step = int.MaxValue/32;

			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				List<string> tablesName = new List<string>();
				using (SqlCommand command = new SqlCommand("SELECT [a].[Name], [a].[idEntityType] FROM [ref].[Entity] AS [a] WHERE [idEntityType]<>1 AND exists(select 1 from ref.EntityField where [idEntity]=a.id and Name='id')", connection))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							tablesName.Add(string.Format("{0}.{1}", Utils.getSchemaByEntityType(reader.GetByte(1)), reader.GetString(0)));
						}
					}
				}
				foreach (string tableName in tablesName)
				{
					StringBuilder textCommand = new StringBuilder();
					textCommand.AppendLine("declare @newMax int;");
					textCommand.AppendFormat(
						"select @newMax=CASE WHEN ISNULL(MAX(id)+1,1)<({0}*{1}) THEN ({0}*{1}) ELSE ISNULL(MAX(id)+1,1) END from [{2}] where id<({0}+1)*{1};",
						no, step, tableName.Replace(".", "].["));
					textCommand.AppendLine();
					textCommand.AppendFormat("DBCC CHECKIDENT ('{0}', RESEED, @newMax);", tableName);
					using (SqlCommand command = new SqlCommand(textCommand.ToString(), connection))
					{
						command.ExecuteNonQuery();
					}
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Вставка данных (в БД) в таблицы первичных перечислений
		/// </summary>
		private void FillPrimaryEnums()
		{
			var enumsFetcher = new EnumsFetcher();

			const string insertCmdTpl = "INSERT INTO {0} ({1}) SELECT {1} FROM @enumValues";
			DataSet dataSet = enumsFetcher.GetPrimaryEnums(ProjectPathSource);
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				foreach (var enumName in Names.PrimaryEnums)
				{
					string tableName = string.Format("{0}.{1}", Utils.getSchemaByEntityType((int) EntityType.Enum), enumName);
					DataTable table = dataSet.Tables[tableName];
					
					StringBuilder fields = new StringBuilder();
					foreach (DataColumn column in table.Columns)
					{
						fields.AppendFormat(",[{0}]", column.ColumnName);
					}
					fields.Remove(0, 1);

					using (SqlCommand command = new SqlCommand(string.Format(insertCmdTpl, table.TableName, fields), connection))
					{
						SqlParameter param = command.Parameters.AddWithValue("@enumValues", table);
						param.SqlDbType = SqlDbType.Structured;
						param.TypeName = "[gen].[Enums]";
						command.ExecuteNonQuery();
					}
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Создание ссылочных ограничений
		/// </summary>
		private void CreateConstreints()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
			FileInfo fileInfo = directoryInfo.GetFiles("CreateConstraints.list")[0];
			if (!fileInfo.Exists)
				throw new Exception("Не найден файл CreateConstraints.list");
			ExecListCommandFromFile(fileInfo);
		}

		/// <summary>
		/// Подготовка СУБД для корректной работы уведомлений об изменениях SqlDependency
		/// </summary>
		private void EnableSqlNotification()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
			FileInfo fileInfo = directoryInfo.GetFiles("EnableSQLNotification.sql")[0];
			string textCommand = fileInfo.OpenText().ReadToEnd();
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(textCommand, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}
		}


		/// <summary>
		/// Развертывание БД платформы
		/// </summary>
		public void DeployPlatformDb()
		{
			// БД платформы разворачивается путем выполнения sql-инструкций, сгенерированных на основе анализа первичных сущностных классов (см. http://jira.rostu-comp.ru/browse/CORE-12).
			CreateDb();
			CreateSchemas();
			CreateBaseTable();
			DataSet dataSet = LoadPlatformDbData();
			if (!IsDeveloper)
			{
				dataSet = Metadata.MultipleFactor(dataSet, -1);
			}
			FillPlatformDbData(dataSet);
			CreateBaseView();
			CreateAssemblies();
			CreateTriggers();
//			FillPrimaryEnums();
			CreateConstreints();
			SetStartIdentity();
			EnableSqlNotification();
		}


		/// <summary>
		/// Загружает в эталонную БД метаданные прикладного решения
		/// </summary>
		public void DeployAppDb()
		{
			disableEntityLogicTrigger();
			Metadata source = new Metadata();
			Metadata targetDb = new Metadata();
			Metadata targetFs = null;

			int factor = IsDeveloper ? 1 : -1;
			targetDb.FromDb(Connection);
			if (!string.IsNullOrWhiteSpace(ProjectPathTarget))
			{
				targetFs = new Metadata();
				targetFs.FromFs(ProjectPathTarget, factor);
			}
			if (targetFs != null && !CompareMetadataTables(targetFs, targetDb, new List<string> {"ref.Entity", "ref.Entityfield"}))
				throw new Exception("Не совпадают метаданные");

			try
			{
				source.FromFs(ProjectPathSource, factor);
			}
			catch (Exception ex)
			{
				this.Report.Messages.Add(new Message(MethodBase.GetCurrentMethod(), MessageType.Error, ex.Message));
			}
			
			MetadataCompareResult cmpRes = new MetadataCompareResult(source, targetDb, targetFs);
			try
			{
				cmpRes.Compare(this.Connection);
			}
			catch (Exception ex)
			{
				this.Report.Messages.Add(new Message(MethodBase.GetCurrentMethod(), MessageType.Error, ex.Message));
			}
			
			try
			{
				cmpRes.Execute(this.Connection);
			}
			catch(Exception ex)
			{
				this.Report.Messages.Add(new Message(MethodBase.GetCurrentMethod(), MessageType.Error, ex.Message));
			}

			if (Verbose)
				this.Report.Messages.Add(new Message(MethodBase.GetCurrentMethod(), MessageType.Info, cmpRes.Verbose()));

			SetStartIdentity();
			enableEntityLogicTrigger();
		}

		/// <summary>
		/// Включить триггер EntityLogic для сущности Entity
		/// </summary>
		private void enableEntityLogicTrigger()
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("ENABLE TRIGGER [ref].[EntityLogicIUD] ON [ref].[Entity];", connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Выключить триггер EntityLogic для сущности Entity
		/// </summary>
		private void disableEntityLogicTrigger()
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand("DISABLE TRIGGER [ref].[EntityLogicIUD] ON [ref].[Entity];", connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Сравнение таблиц из двух объектов Metadata
		/// </summary>
		/// <param name="targetFs">Метаданные полученные из предыдущего обновления</param>
		/// <param name="targetDb">Метаданные содержащиеся в обновляемой таблице</param>
		/// <param name="tablesName">Наименование сравниваемых таблиц</param>
		/// <returns></returns>
		private bool CompareMetadataTables(Metadata targetFs, Metadata targetDb, IEnumerable<string> tablesName)
		{
			return tablesName.Aggregate(true, (current, tableName) => current && CompareTable(targetFs, targetDb, tableName));
		}

		/// <summary>
		/// Сравнение таблицы из двух объектов Metadata
		/// </summary>
		/// <param name="targetFs">Метаданные полученные из предыдущего обновления</param>
		/// <param name="targetDb">Метаданные содержащиеся в обновляемой таблице</param>
		/// <param name="tableName">Наименование сравниваемой таблицы</param>
		/// <returns></returns>
		private bool CompareTable(Metadata targetFs, Metadata targetDb, string tableName)
		{
			bool result = true;
			foreach (DataRow rowFs in targetFs.DataSet.Tables[tableName].Rows)
			{
				DataRow rowDb =
					targetDb.DataSet.Tables[tableName].AsEnumerable().SingleOrDefault(
						a => a.Field<int>(Names.Id) == rowFs.Field<int>(Names.Id));
				if (rowDb == null)
				{
					Report.Messages.Add(new Message(MethodBase.GetCurrentMethod(), MessageType.Error,
													string.Format(
														"В обновляемой базе в таблице {0} отсутствует запись с наименованием '{1}' и идентификатором '{2}'.",
														tableName, rowFs.Field<string>(Names.Name), rowFs.Field<int>(Names.Id))));
					result = false;
				}
				else
				{
					result = result && CompareRow(rowFs, rowDb);
				}
			}
			return result;
		}

		/// <summary>
		/// Сравнение двух строк
		/// </summary>
		/// <param name="rowFs">Строка из таблицы принадлежащей метаданным из предыдущего обновления</param>
		/// <param name="rowDb">Строка из таблицы принадлежащей метаданным из обновляемой базы</param>
		/// <returns></returns>
		private bool CompareRow(DataRow rowFs, DataRow rowDb)
		{
			bool result = true;
			if (rowFs==null || rowDb==null)
			{
				Report.Messages.Add(new Message(MethodBase.GetCurrentMethod(), MessageType.Error,
				                                "Сравнение пустых строк не имеет смысла."));
				return false;
			}
			string tableName = rowFs.Table.TableName;
			foreach (DataColumn columnFs in rowFs.Table.Columns.Cast<DataColumn>().Where(a=> a.ColumnName!=Names.Tstamp))
			{
				string columnName = columnFs.ColumnName;
				if (rowDb.Table.Columns[columnName] == null)
				{
					Report.Messages.Add(new Message(MethodBase.GetCurrentMethod(), MessageType.Error, string.Format(
						"В обновляемой базе в таблице '{0}' отсутствует колонка '{1}'.",
						tableName, columnName)));
					result = false;
				}
				else
				{
					if (!rowFs[columnFs].Equals(rowDb[columnFs]))
					{
						Report.Messages.Add(new Message(MethodBase.GetCurrentMethod(), MessageType.Error, string.Format(
							"В обновляемой базе в таблице '{0}' для записи с идентификатором '{1}' в поле {2} изменено значение.",
							tableName, rowFs.Field<int>(Names.Id), columnName)));
						result = false;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Разворачивает БД платформы и загружает в нее метаданные прикладного решения
		/// </summary>
		public void DeployDb()
		{
			DeployPlatformDb();
			DeployAppDb();
		}

		/// <summary>
		/// Выполняет экспорт таблиц метаданных в xml
		/// </summary>
		/// <param name="path">
		/// Путь, по которому следует разместить директорию с метаданными
		/// </param>
		public void ExportMetadata(string path)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Выполняет обновление указанной БД до состояния, определяемого указанным набором метаданных.
		/// </summary>
		public void UpdateDb()
		{
			throw new NotImplementedException();
			/*var target = new Metadata();
			var source = new Metadata();

			target.FromDb(Connection);
			source.FromFs();

			var result = new MetadataCompareResult(source, target);
			result.Compare();
			result.Execute();
			*/
		}

		/// <summary>
		/// Создает пакет метаданных для обновления
		/// </summary>
		public void CreateUpdatePackage(int revisionFrom, int revisionTo)
		{
			throw new NotImplementedException();
			/* Обращение только к SVN
			 * Пакет обновления создается там же, откуда запущен MH, в поддиректории UpdatePackages
			 */
		}

		/// <summary>
		/// Сравнивает БД с метаданными. 
		/// Проверка соответствия системных таблиц исходным кофигурационным файлам, из котрорых было произведено последнее обновление системы. 
		/// Позволит выяснить - менялось ли что-либо в структуре в принципе.
		/// </summary>
		public void ValidateDbMetadata()
		{
			throw new NotImplementedException();
			/*
			 * - БД -> OMM1
			 * - xml -> OMM2
			 * - сравнить два набора OMM
			 */
		}

		/// <summary>
		/// Проверка соответствия структуры БД стандартным объектам, генерируемыми для соответствующих метаданных из системных таблиц. 
		/// Данная проверка даст понять - менялось ли что-нибудь в структуре БД без использования конфигуратора (т.е. не через интерфейс).
		/// </summary>
		public void ValidateDbStructure()
		{
			throw new NotImplementedException();
		}

		#region Private Methods
		

		#endregion

		public bool Verbose { get; set; }
	}
}
