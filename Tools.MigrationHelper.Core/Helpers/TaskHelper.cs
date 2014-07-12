using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NAnt.Core;

namespace Tools.MigrationHelper.Core.Helpers
{
	/// <summary>
	/// Общие методы тасков
	/// </summary>
	public static class TaskHelper
	{
        private static readonly string PathPlatformDbScripts = AppDomain.CurrentDomain.BaseDirectory + @"\PlatformDb\Scripts";

	    private const string PostUpdatePath = @"\PostUpdateScripts";
        private const string PostUpdateDbPath = @"\PostUpdateDbScripts";
        private const string PreUpdateDbPath = @"\PreUpdateDbScripts";
        private const string PreUpdatePath = @"\PreUpdateScripts";

	    public static void ExecutePostUpdateDbScript(SqlConnection connection)
		{
            var directoryInfo = new DirectoryInfo(PathPlatformDbScripts + PostUpdateDbPath);
            ExecuteScriptFiles(connection, directoryInfo.GetFiles());
		}

        public static void ExecutePostUpdateScript(SqlConnection connection)
        {
            var directoryInfo = new DirectoryInfo(PathPlatformDbScripts + PostUpdatePath);
            ExecuteScriptFiles(connection, directoryInfo.GetFiles());
        }

        public static void ExecutePreUpdateScript(SqlConnection connection)
        {
            var directoryInfo = new DirectoryInfo(PathPlatformDbScripts + PreUpdatePath);
            ExecuteScriptFiles(connection, directoryInfo.GetFiles());
        }

	    public static void ExecutePreUpdateDbScript(SqlConnection connection)
	    {
            var directoryInfo = new DirectoryInfo(PathPlatformDbScripts + PreUpdateDbPath);
	        ExecuteScriptFiles(connection, directoryInfo.GetFiles());
	    }

	    private static void ExecuteScriptFiles(SqlConnection connection, IEnumerable<FileInfo> files)
	    {
            foreach (var fileInfo in files.OrderBy(f=>f.Name))
            {
                ExecuteScriptFile(connection, fileInfo);
            }
	    }

	    public static void ExucuteDeleteFailDate(SqlConnection connection)
	    {
            var fileInfo = new FileInfo(PathPlatformDbScripts + @"\DeleteFailData.sql");
	        try
	        {
                ExecuteScriptFile(connection, fileInfo);
	        }
	        catch(Exception e)
	        {
	            throw new Exception("Ошибка удаления невалидных данных!" + e.Message);
	        }
	    }

	    private static void ExecuteScriptFile(SqlConnection connection, FileInfo file)
	    {
            string textCommands = file.OpenText().ReadToEnd();
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);
            string dbName = sqlConnectionStringBuilder.InitialCatalog;

            if (string.IsNullOrEmpty(textCommands))
                return;

            using (var command = new SqlCommand(String.Format(textCommands, dbName), connection))
            {
                command.CommandTimeout = sqlConnectionStringBuilder.ConnectTimeout;
                command.ExecuteNonQuery();
            }
	    }

	    public static void DisableIndex(SqlConnection connection)
		{
            const string commandText = "ALTER INDEX [Unique_isCaption] ON [ref].[EntityField] DISABLE";

            ExecuteSQlCommand(connection, commandText);
		}

		public static void EnableIndex(SqlConnection connection)
		{
            const string commandText = "ALTER INDEX [Unique_isCaption] ON [ref].[EntityField] REBUILD PARTITION = ALL WITH ( PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, IGNORE_DUP_KEY  = OFF, ONLINE = OFF, SORT_IN_TEMPDB = OFF )";

            ExecuteSQlCommand(connection, commandText);
		}

	    /// <summary>
	    /// Выключить триггер EntityLogic для сущности Entity
	    /// </summary>
	    public static void DisableEntityLogicTrigger(SqlConnection connection)
	    {
            const string commandText = "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ref].[EntityLogicIUD]') AND [type] IN (N'TA')) DISABLE TRIGGER [ref].[EntityLogicIUD] ON [ref].[Entity];IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ref].[EntityFieldLogicIUD]') AND [type] IN (N'TA')) DISABLE TRIGGER [ref].[EntityFieldLogicIUD] ON [ref].[EntityField];";

            ExecuteSQlCommand(connection, commandText);
	    }

	    /// <summary>
		/// Включить триггер EntityLogic для сущности Entity
		/// </summary>
		public static void EnableEntityLogicTrigger(SqlConnection connection)
		{
            const string commandText = "ENABLE TRIGGER [ref].[EntityLogicIUD] ON [ref].[Entity];ENABLE TRIGGER [ref].[EntityFieldLogicIUD] ON [ref].[EntityField];";

            ExecuteSQlCommand(connection, commandText);
		}

		/// <summary>
		/// Создание таблицы с ревизиями xml файлов
		/// </summary>
		public static void CreateDevDbRevisionTable(SqlConnection connection)
		{
		    const string commandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DevDbRevision]') AND type in (N'U'))" +
		                               " CREATE TABLE [dbo].[DevDbRevision] ([path] nvarchar(400), [revision] int);";

		    ExecuteSQlCommand(connection, commandText);
		}

        public static void CreateUpdateRevisionTable(SqlConnection connection)
        {
            const string commandText =
                "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateRevisions]') AND type in (N'U'))" +
                " CREATE TABLE [dbo].[UpdateRevisions] ([revision] int, [date] DateTime , [devid] int,[File] VARBINARY(MAX));";

            ExecuteSQlCommand(connection, commandText);
        }

        public static void CreateDistrDataTriggers(SqlConnection connection)
        {
            const string commandText = "exec [dbo].[CreateDistributiveDataTriggers]";

            ExecuteSQlCommand(connection, commandText);
        }

	    /// <summary>
        /// Создание бэкапа
        /// </summary>
        public static void CreateBackUp(string сonnectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(сonnectionString);
            var commandString = string.Format("BACKUP DATABASE {0} TO  DISK = N'{1}_{0}.bak' WITH NOFORMAT, INIT", connectionStringBuilder.InitialCatalog, "Update");
            ExecuteSQlCommand(сonnectionString, commandString);
        }

        public static void ExecuteSQlCommand(string connectionString, string commandString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var server = GetServer(connection);
                try
                {
                    server.ConnectionContext.ExecuteNonQuery(commandString);
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                        throw e.InnerException;

                    throw;
                }
            }
        }

        public static void ExecuteSQlCommand(SqlConnection connection, string commandString)
	    {
            var server = GetServer(connection);
            try
            {
                server.ConnectionContext.ExecuteNonQuery(commandString);
            }
            catch (Exception e)
            {
                if(e.InnerException != null)
                    throw e.InnerException;

                throw;
            }
	    }

        public static object ExecuteScalarCommand(SqlConnection connection, string commandString)
        {
            var server = GetServer(connection);
            object result;
            try
            {
                result = server.ConnectionContext.ExecuteScalar(commandString);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw;
            }
            return result;
	    }

        public static Server GetServer(SqlConnection connection)
	    {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);
            var server = new Server(new ServerConnection(connection));
            server.ConnectionContext.StatementTimeout = sqlConnectionStringBuilder.ConnectTimeout;
            return server;
	    }

	    public static byte[] GetZipFileByte(string sourcePath, string comment = null)
        {
            var ms = new MemoryStream();

            using (var zip = new ZipFile())
            {
                zip.AddDirectory(sourcePath);
                zip.Comment = comment;
                zip.Save(ms);
            }

            return ms.ToArray();
        }

        public class NotDelete
        {
            public int Id { get; set; }
            public string TableName { get; set; }
        }
	}
}
