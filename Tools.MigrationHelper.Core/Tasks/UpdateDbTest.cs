using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NAnt.Core;
using SharpSvn;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.DbManager;
using Tools.MigrationHelper.Core.Helpers;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
	[TaskName("updatedbtest")]
	public class UpdateDbTest : DbDeployTask
	{
		[TaskAttribute("startrevision", Required = false)]
		public int StartRevision
		{
			get;
			set;
		}

		[TaskAttribute("endrevision", Required = false)]
		public int? EndRevision
		{
			get;
			set;
		}

		private const string Prefix = "test";
		private readonly List<string> _dbNames;
		private static readonly DateTime DateTime = DateTime.Now;

		public UpdateDbTest()
		{
			_dbNames = new List<string>();
		}

		protected override void ExecuteTask()
		{
			// Create and execute a <sysinfo /> task
			Log(Level.Verbose, "Start update test...");
			RunTest(IsDeveloper() && StartRevision > 0);
		}

		/// <summary>
		/// Запуск теста
		/// </summary>
		/// <param name="flag">Проходимся по ревизиям свн, иначе берем xml из указаной папки</param>
		public void RunTest(bool flag)
		{
            CreateBackUp();
            RestoreBackUp();
			if (flag)
			{
				foreach (var revision in GetRevisions())
				{
					SvnUpdate(revision);
					DeployDb(revision);
					UpdateDb();
					Compare(GetDbName(revision));
					RestoreBackUp();
				}
			}
			else
			{
				UpdateDb();
				var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
				var dbName = Prefix + "_" + connectionStringBuilder.InitialCatalog;
				connectionStringBuilder.InitialCatalog = dbName;
				DeployDb(connectionStringBuilder.ConnectionString);
				Compare(dbName);
			}
			Clear();
		}

		/// <summary>
		/// Создание БД
		/// </summary>
		private void DeployDb(int revision)
		{
			var dbName = GetDbName(revision);
			_dbNames.Add(dbName);
			DeployDb(GetConnectionString(dbName, ConnectionString));
		}

		/// <summary>
		/// Вызов Tasks по деблою базы
		/// </summary>
		/// <param name="connectionString"></param>
		private void DeployDb(string connectionString)
		{
			var deployPlatformDb = new DeployPlatformDb
			{
				ConnectionString = connectionString,
				DevId = DevId,
				SourcePath = SourcePath
			};
			CopyTo(deployPlatformDb);
			deployPlatformDb.Execute();

			var deployAppDb = new DeployAppDb
			{
				ConnectionString = connectionString,
				DevId = DevId,
				SourcePath = SourcePath
			};
			CopyTo(deployAppDb);
			deployAppDb.Execute();
		}

		/// <summary>
		/// Вызов Task обновления
		/// </summary>
		private void UpdateDb()
		{
			var updateDb = new UpdateDb
				{
					ConnectionString = ConnectionString, 
					DevId = DevId, 
					SourcePath = SourcePath
				};
			CopyTo(updateDb);
			updateDb.Execute();
		}

		/// <summary>
		/// Создание бэкапа
		/// </summary>
		private void CreateBackUp()
		{
			Log(Level.Verbose, "Создание бэкапа для исходной базы...");
			var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
			var commandString = string.Format("BACKUP DATABASE {0} TO  DISK = N'{1}_{0}_src.bak' WITH NOFORMAT, INIT", connectionStringBuilder.InitialCatalog, Prefix);
            TaskHelper.ExecuteSQlCommand(ConnectionString, commandString);
		}

		/// <summary>
		/// Восстановление бэкапа в новую базу
		/// </summary>
		private void RestoreBackUp()
		{
			Log(Level.Verbose, "Востановление исходной базы из бэкапа во временную...");
			var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
			var commandString = string.Format("USE MASTER " +
											  "DECLARE @data NVARCHAR(400), @log NVARCHAR(400); " +
											  "SELECT @data = SUBSTRING(physical_name, 1, CHARINDEX(N'master.mdf', LOWER(physical_name)) - 1) + '{1}_{0}_src.mdf' " +
											  "FROM master.sys.master_files WHERE database_id = 1 AND file_id = 1 " +
											  "SELECT @log = SUBSTRING(physical_name, 1, CHARINDEX(N'master.mdf', LOWER(physical_name)) - 1) + '{1}_{0}_src.ldf' " +
											  "FROM master.sys.master_files WHERE database_id = 1 AND file_id = 1 " +
											  "RESTORE FILELISTONLY FROM DISK = '{1}_{0}_src.bak' " +
											  "RESTORE DATABASE {1}_{0}_src FROM DISK = '{1}_{0}_src.bak' WITH REPLACE, MOVE '{0}' TO @data, MOVE '{0}_log' TO @log;"
				, connectionStringBuilder.InitialCatalog
			, Prefix);

            TaskHelper.ExecuteSQlCommand(ConnectionString, commandString);
		}

		/// <summary>
		/// Обновление файлов xml на определенную ревизию свн
		/// </summary>
		/// <param name="revision"></param>
		private void SvnUpdate(int revision)
		{
			Log(Level.Verbose, "Обновление xml файлов до нужной ревизии...");
			List<string> directories = SolutionHelper.GetXMLDirectories(SourcePath);
			using (var svnClient = new SvnClient())
			{
				svnClient.Update(directories, new SvnUpdateArgs { Revision = revision });
			}
		}

		/// <summary>
		/// Сравнение двух баз
		/// </summary>
		/// <param name="dbName">Имя базы с которой сравниваем обновленную</param>
		private void Compare(string dbName)
		{
			Log(Level.Verbose, "Сравнение двух баз данных...");
			var source = new DbDataSet(1);
			var targetDb = new DbDataSet(1);

			var connectionBuilder = new SqlConnectionStringBuilder(ConnectionString);
			targetDb.FromDb(GetConnectionString(Prefix + "_" + connectionBuilder.InitialCatalog + "_" + "src", ConnectionString));

			source.FromDb(GetConnectionString(dbName,ConnectionString));

			CompareDataSet(source.DataSet, targetDb.DataSet);
		}

		/// <summary>
		/// Удаление баз данных с сервера с префиксом в названии
		/// </summary>
		private void Clear()
		{
			Log(Level.Verbose, "Удаление временных баз данных...");
			var commandString = string.Format("USE master " +
			                                  "declare @cmd varchar(4000) " +
											  "declare cmds cursor for select 'ALTER DATABASE [' + name + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE drop database [' + name + ']' " +
											  "FROM master..sysdatabases where name like '{0}%' " +
											  "open cmds while 1=1 " +
											  "begin " +
											  "fetch cmds into @cmd " +
											  "if @@fetch_status != 0 " +
											  "break " +
											  "exec(@cmd) " +
											  "end " +
											  "close cmds " +
											  "deallocate cmds;", Prefix);
            TaskHelper.ExecuteSQlCommand(ConnectionString, commandString);
		}

		/// <summary>
		/// получение строки соединения
		/// </summary>
		/// <param name="dbName"></param>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		private static string GetConnectionString(string dbName, string connectionString)
		{
			var connectionBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = dbName };
			return connectionBuilder.ConnectionString;
		}

		/// <summary>
		/// Получение номеров ревизий в заданом диапазоне в которых изменялись xml
		/// </summary>
		/// <returns></returns>
		private List<int> GetRevisions()
		{
			var list = new List<int>();
		    var endRevision = EndRevision == null ? GetLatestRevision() : (long) EndRevision;

            for (var i = StartRevision; i <= endRevision; i++)
			{
				using (var svnClient = new SvnClient())
				{
					Collection<SvnLogEventArgs> logEventArgs;
					svnClient.GetLog(SourcePath, new SvnLogArgs { Start = i, End = i }, out logEventArgs);
					if (logEventArgs.Any(a => a.ChangedPaths.Any(an => an.Path.EndsWith(".xml"))))
					{
						list.Add(i);
					}
				}
			}
			return list;
		}

        /// <summary>
        /// Получение номера последний ревизии свн
        /// </summary>
        /// <returns></returns>
        private int GetLatestRevision()
        {
            using (SvnClient svnClient = new SvnClient())
            {
                SvnInfoEventArgs version;
                svnClient.GetInfo(SourcePath, out version);
                return (int)version.Revision;
            }
        }

		private string GetDbName(int revision)
		{
			return Prefix + "_" + DateTime.Date.ToString("yyMMDD") + "_" + DateTime.TimeOfDay.ToString("HHMMSS") + "_" +
				   revision;
		}

		/// <summary>
		/// Сравнение DataSet
		/// </summary>
		/// <param name="ds1">DataSet c обновлением</param>
		/// <param name="ds2">DataSet c обновленной базой</param>
		private static void CompareDataSet(DataSet ds1, DataSet ds2)
		{
			foreach (DataTable ds1Table in ds1.Tables)
			{
				DataTable ds2Table = ds2.Tables[ds1Table.TableName];
				if (ds1Table.Columns.IndexOf(Names.Tstamp) >= 0)
					ds1Table.Columns.Remove(Names.Tstamp);
				if (ds2Table.Columns.IndexOf(Names.Tstamp) >= 0)
					ds2Table.Columns.Remove(Names.Tstamp);

				var names = ds1Table.TableName.Split(new[] { '.' });
				var schemaName = names[0];

				if (schemaName == "reg")
					continue;
				if (schemaName == "ml")
				{
					foreach (DataRow ds1Row in ds1Table.Rows)
					{
						string filter = string.Empty;
						foreach (DataColumn column in ds1Table.Columns)
						{
							filter += string.Format("{1} = '{0}' AND", ds1Row[column.ColumnName],column.ColumnName);
						}
						//Вырезаем последний AND
						filter = filter.Substring(0, filter.Length - 3);

						DataRow[] ds2Rows = ds2Table.Select(filter);
						if(ds2Rows.Count() > 1)
							throw new Exception(string.Format("У мультилинка '{0}' паре значений соотвествуют неск элементов",ds1Table.TableName));
						if(!ds2Rows.Any())
							throw new Exception(string.Format("У мультилинка '{0}' паре значений отсутствует соотвествие", ds1Table.TableName));
					}
				}
				if(schemaName == "dbo")
					continue;

				
				foreach (DataRow ds1Row in ds1Table.Rows)
				{
					
					string filter = string.Format("Id = '{0}'", ds1Row["Id"]);
					DataRow[] ds2Rows = ds2Table.Select(filter);
					if (ds2Rows.Count() == 1)
					{
						var array1 = ds1Row.ItemArray;
						var array2 = ds2Rows[0].ItemArray;
						for (var i = 0; i < array1.Count(); i++)
						{
							if (array1[i].ToString() != array2[i].ToString())
								throw new Exception(string.Format("Ошибка сравнения! Записи с Id = '{0}' в таблице '{1}' в базах отличаются ", ds1Row["Id"], ds1Table.TableName));
						}
					}
					if (ds2Rows.Count() > 1)
					{
						throw new Exception(string.Format("Ошибка сравнения! В обновляймой базе в таблице '{1}' найдено неск элементов c Id = '{0}'", ds1Row["Id"], ds1Table.TableName));
					}
					if (!ds2Rows.Any())
					{
						throw new Exception(string.Format("Ошибка сравнения! В обновляймой базе не найден элемент таблицы '{1}' c Id = '{0}'", ds1Row["Id"], ds1Table.TableName));
					}
				}
			}
		}
	}
}
