using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.DbManager;
using Tools.MigrationHelper.Core.Helpers;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{

    [TaskName("deployplatformdb")]
    public class DeployPlatformDb : DbDeployTask
    {
        [TaskAttribute("pathplatformdbscripts", Required = false)]
        public string PathPlatformDbScripts
        {
            get { return _pathPlatformDbScripts; }
            set { _pathPlatformDbScripts = value; }
        }

        protected override void ExecuteTask()
        {
            // БД платформы разворачивается путем выполнения sql-инструкций, сгенерированных на основе анализа первичных сущностных классов (см. http://jira.rostu-comp.ru/browse/CORE-12).
            CreateDb();
            CreateSchemas();
            CreateBaseTable();
            DataSet dataSet = LoadPlatformDbData();
            FillPlatformDbData(dataSet);
            CreateBaseView();
            DeployAssembly();
            CreateConstreints();
            SetStartIdentity();
            EnableSqlNotification();
        }

        private void DeployAssembly()
        {
            var deployAssembly = new AssemblyTask
            {
                ConnectionString = ConnectionString,
                DevId = DevId,
                SourcePath = SourcePath,
                Task = (int) TasksEnum.DeployPlatformDb,
                Action = (int)ActionEnum.Create
            };
            CopyTo(deployAssembly);
            deployAssembly.Execute();
        }

        /// <summary>
        /// Создание БД
        /// </summary>
        private void CreateDb()
        {
            Log(Level.Verbose, "Создание базы данных ...");

            try
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
                string dbName = sqlConnectionStringBuilder.InitialCatalog;
                sqlConnectionStringBuilder.InitialCatalog = "master";
                DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                FileInfo fileInfo = directoryInfo.GetFiles("createDb.sql")[0];
                string[] textCommands = fileInfo.OpenText().ReadToEnd().Split(new[] { '\r', '\n' });
                using (SqlConnection connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    foreach (string textCommand in textCommands)
                    {
	                    if (string.IsNullOrWhiteSpace(textCommand)) continue;
						if (string.IsNullOrWhiteSpace(dbName)) throw new Exception("Не указано имя базы");

                        TaskHelper.ExecuteSQlCommand(connection, string.Format(textCommand, dbName));
                    }
	                connection.Close();
                }
            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при создании базы данных ", ex);
            }
        }


        /// <summary>
        /// Создание View для базовых таблиц
        /// </summary>
        private void CreateBaseView()
        {
            Log(Level.Verbose, "Создание View для базовых таблиц ...");
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                FileInfo fileInfo = directoryInfo.GetFiles("CreateBaseView.list")[0];
                if (!fileInfo.Exists)
                    Fatal("Фатальная ошибка при создании View для базовых таблиц. Не найден файл CreateBaseView.list");
                ExecListCommandFromFile(fileInfo);
            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при создании View для базовых таблиц  ", ex);
            }

        }

        /// <summary>
        /// Подготовка СУБД для корректной работы уведомлений об изменениях SqlDependency
        /// </summary>
        private void EnableSqlNotification()
        {
            Log(Level.Verbose, "Подготовка СУБД для корректной работы уведомлений об изменениях SqlDependency...");
            try
            {

                DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                FileInfo fileInfo = directoryInfo.GetFiles("EnableSQLNotification.sql")[0];
                string textCommand = fileInfo.OpenText().ReadToEnd();
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    TaskHelper.ExecuteSQlCommand(connection, textCommand);
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при подготовка СУБД для корректной работы уведомлений об изменениях SqlDependency", ex);
            }

        }

        /// <summary>
        /// Создание ссылочных ограничений
        /// </summary>
        private void CreateConstreints()
        {
            Log(Level.Verbose, "Создание ссылочных ограничений...");
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                FileInfo fileInfo = directoryInfo.GetFiles("CreateConstraints.list")[0];
                if (!fileInfo.Exists)
                    Fatal("Фатальная ошибка при создании ссылочных ограничений.Не найден файл CreateConstraints.list");
                ExecListCommandFromFile(fileInfo);
            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при создании ссылочных ограничений", ex);
            }

        }

        /// <summary>
        /// Вставляет данные из DataSet в БД
        /// </summary>
        /// <param name="dataSet"></param>
        private void FillPlatformDbData(DataSet dataSet)
        {
            Log(Level.Verbose, "Вставляем данные из DataSet в БД ...");
            try
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    bool isAutoincrement = table.Columns[Names.Id].AutoIncrement;
                    StringBuilder textCommand = new StringBuilder();
                    List<string> fieldsName = (from DataColumn column in table.Columns where column.ColumnName != Names.Tstamp select column.ColumnName).ToList();
                    List<string> fieldsName2 = (from DataColumn column in table.Columns where column.ColumnName != Names.Tstamp select "[" + column.ColumnName + "]").ToList();
                    List<string> paramsName = (from DataColumn column in table.Columns where column.ColumnName != Names.Tstamp select "@" + column.ColumnName).ToList();
                    textCommand.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", table.TableName, string.Join(",", fieldsName2), string.Join(",", paramsName));
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("", connection))
                        {
                            command.CommandTimeout = new SqlConnectionStringBuilder(connection.ConnectionString).ConnectTimeout;
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
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при вставке из DataSet в БД ", ex);
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
                    TaskHelper.ExecuteSQlCommand(connection,textCommand);
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Создание схем а БД
        /// </summary>
        private void CreateSchemas()
        {
            Log(Level.Verbose, "Создание схемы...");
            try
            {
                var directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                FileInfo fileInfo = directoryInfo.GetFiles("CreateSchemas.list")[0];
                if (!fileInfo.Exists)
                    throw new Exception("Не найден файл CreateSchemas.list");
                ExecListCommandFromFile(fileInfo);

            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при создании схемы :" + ex.Message);
            }
        }

        /// <summary>
        /// Создание базовых таблиц
        /// </summary>
        private void CreateBaseTable()
        {
            Log(Level.Verbose, "Создание базовых таблиц ...");
            try
            {
                var directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                var fileInfo = directoryInfo.GetFiles("createBaseTable.sql")[0];
                var textCommand = fileInfo.OpenText().ReadToEnd();
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    TaskHelper.ExecuteSQlCommand(connection, textCommand);
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при создании базовых таблиц", ex);
            }
        }

        /// <summary>
        /// Загружает данные в DataSet из Tools.MigrationHelper/PlatformDb/Data
        /// </summary>
        /// <returns></returns>
        private DataSet LoadPlatformDbData()
        {
            Log(Level.Verbose, "Загрузка данных ...");
            try
            {
                var sourcePath = GetSourcePath();
	            var targetFs = new DbDataSet(DevId, TasksEnum.DeployPlatformDb);
                targetFs.FromFs(sourcePath);
				return targetFs.DataSet;
            }
            catch (Exception ex)
            {
                return Fatal<DataSet>("Фатальная ошибка при загрузке данных", ex);
            }
        }

        private string GetSourcePath()
        {
            var sourcePath = SourcePath;

            if (!IsDeveloper() && PathIsSolution(sourcePath))
                Fatal("DevId равен 0 и в папке источника находится проект! Необходимо установить параментр DevId или указать папку с xml");
            if (!IsDeveloper())
            {
                DirectoryInfo updateDir = null;
                try
                {
                    updateDir = new DirectoryInfo(sourcePath).GetDirectories("Update*").Single();
                }
                catch (Exception e)
                {
                    Fatal("Не найдена папка Update c исходными данными. Ожидалась тут : " + sourcePath, e);
                }

                var dirs = updateDir.GetDirectories();
                var needPath = dirs.Max(m => int.Parse(m.Name)).ToString();
                var dir = dirs.FirstOrDefault(f => f.Name == needPath);
                if(dir == null)
                    Fatal("В папке Update отсутсвуют папочки.");
                sourcePath = dir.FullName;
            }
            return sourcePath;
        }

        private bool PathIsSolution(string sourcePath)
        {
            var dir = new DirectoryInfo(sourcePath);
            var files = dir.GetFiles("*.sln",SearchOption.TopDirectoryOnly);
            return files.Any();
        }

        private  string _pathPlatformDbScripts = AppDomain.CurrentDomain.BaseDirectory + @"\PlatformDb\Scripts";
    }
}
