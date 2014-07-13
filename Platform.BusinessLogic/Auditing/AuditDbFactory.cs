using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Platform.Application.Common;
using Platform.Common;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Auditing
{
	/// <summary>
	/// Создание и обновление БД аудита
	/// </summary>
	public partial class AuditDbFactory: IAfterAplicationStart
	{
		/// <summary>
		/// Строка соединения с основной БД PlatformDBConnectionString
		/// </summary>
		private static SqlConnectionStringBuilder DbConnBuilder
		{
			get { return DbConnectionString.Instance.Builder; }
		}

        /// <summary>
        /// Комманды для создания объектов БД аудита. ключ = имя объекта; значение = комманда
        /// </summary>
	    private Dictionary<string, string> Ddl;

		/// <summary>
		/// Выполняется при старте веб-приложения
		/// </summary>
		public void Execute()
		{
			var config = IoC.Resolve<AuditConfiguration>();
			if (!config.Enabled)
				return;

			if (!config.ConnectionStringBuilder.DataSource.Equals(DbConnBuilder.DataSource, StringComparison.OrdinalIgnoreCase))
				throw new PlatformException("БД аудита не может находиться на другом инстансе sql server'а по отношению к базе с данными, " +
											"потому что БД аудита через view обращается к базе с данными, " +
											"а вьюха не может обратиться к таблице другой БД, находящейся на другом инстансе.");

            Ddl = new SqlTpl().GetDdl();

			using (var connection = new SqlConnection(DbConnBuilder.ToString()))
			{
				connection.Open();
				string auditDbName = config.ConnectionStringBuilder.InitialCatalog;
				using (var com = connection.CreateCommand())
				{
					com.CommandText = @"
						SELECT count(1)
						FROM master.dbo.sysdatabases 
						WHERE (name = '" + auditDbName + "')";
					if (com.ExecuteScalar().Equals(1))
						CreateTables(com, auditDbName);
					else
						CreateDataBase(com, auditDbName);
				}
			}
		}

        /// <summary>
        /// Создание БД Аудита, настройка ее свойств, создание таблиц.
        /// Если БД уже есть, то ничего не делается.
        /// </summary>
        /// <param name="com"></param>
        /// <param name="dbName"></param>
		private void CreateDataBase(SqlCommand com, string dbName)
		{
			com.CommandText = @"
						SELECT TOP(1) f.physical_name 
						FROM sys.master_files f INNER JOIN  sys.databases d ON (d.database_id=f.database_id)
						WHERE d.[name]='" + DbConnBuilder.InitialCatalog + "'";

			var dbPath = com.ExecuteScalar().ToString();
			var dir = Path.GetDirectoryName(dbPath);

			if (string.IsNullOrEmpty(dir))
				return;

			var newDbPath = Path.Combine(dir, dbName) + ".mdf";
			var newDbPathLog = Path.Combine(dir, dbName) + ".ldf";

			using (var ts = new TransactionScope())
			{
				com.CommandText = @"
						CREATE DATABASE [" + dbName + @"] ON  PRIMARY 
						( NAME = N'" + dbName + "', FILENAME = N'" + newDbPath +
									@"' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
						LOG ON 
						( NAME = N'" + dbName + "_log', FILENAME = N'" + newDbPathLog +
									@"', SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%) ";

				com.ExecuteNonQuery();

				com.CommandText = @"

						ALTER DATABASE [" + dbName + @"] SET COMPATIBILITY_LEVEL = 100
						IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
						begin
						EXEC [" + dbName + @"].[dbo].[sp_fulltext_database] @action = 'enable'
						end
						ALTER DATABASE [" + dbName + @"] SET ANSI_NULL_DEFAULT OFF 
						ALTER DATABASE [" + dbName + @"] SET ANSI_NULLS OFF 
						ALTER DATABASE [" + dbName + @"] SET ANSI_PADDING OFF 
						ALTER DATABASE [" + dbName + @"] SET ANSI_WARNINGS OFF 
						ALTER DATABASE [" + dbName + @"] SET ARITHABORT ON 
						ALTER DATABASE [" + dbName + @"] SET AUTO_CLOSE OFF 
						ALTER DATABASE [" + dbName + @"] SET AUTO_CREATE_STATISTICS ON 
						ALTER DATABASE [" + dbName + @"] SET AUTO_SHRINK OFF 
						ALTER DATABASE [" + dbName + @"] SET AUTO_UPDATE_STATISTICS ON 
						ALTER DATABASE [" + dbName + @"] SET CURSOR_CLOSE_ON_COMMIT OFF 
						ALTER DATABASE [" + dbName + @"] SET CURSOR_DEFAULT  GLOBAL 
						ALTER DATABASE [" + dbName + @"] SET CONCAT_NULL_YIELDS_NULL OFF 
						ALTER DATABASE [" + dbName + @"] SET NUMERIC_ROUNDABORT OFF 
						ALTER DATABASE [" + dbName + @"] SET QUOTED_IDENTIFIER OFF 
						ALTER DATABASE [" + dbName + @"] SET RECURSIVE_TRIGGERS OFF 
						ALTER DATABASE [" + dbName + @"] SET DISABLE_BROKER 
						ALTER DATABASE [" + dbName + @"] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
						ALTER DATABASE [" + dbName + @"] SET DATE_CORRELATION_OPTIMIZATION OFF 
						ALTER DATABASE [" + dbName + @"] SET TRUSTWORTHY OFF 
						ALTER DATABASE [" + dbName + @"] SET ALLOW_SNAPSHOT_ISOLATION OFF 
						ALTER DATABASE [" + dbName + @"] SET PARAMETERIZATION SIMPLE 
						ALTER DATABASE [" + dbName + @"] SET READ_COMMITTED_SNAPSHOT OFF 
						ALTER DATABASE [" + dbName + @"] SET HONOR_BROKER_PRIORITY OFF 
						ALTER DATABASE [" + dbName + @"] SET READ_WRITE 
						ALTER DATABASE [" + dbName + @"] SET RECOVERY FULL 
						ALTER DATABASE [" + dbName + @"] SET MULTI_USER 
						ALTER DATABASE [" + dbName + @"] SET PAGE_VERIFY CHECKSUM  
						ALTER DATABASE [" + dbName + @"] SET DB_CHAINING OFF 
					";
				com.ExecuteNonQuery();

                CreateTables(com, dbName);

				ts.Complete();
			}
		}

        /// <summary>
        /// Создает таблицы БД. Перед созданием каждой таблицы проверяется факт ее наличия. 
        /// Если таблица есть, то ничего не предпринимается.
        /// </summary>
        /// <param name="com"></param>
        /// <param name="dbName"></param>
        private void CreateTables(SqlCommand com, string dbName)
        {
            com.CommandText = @"USE " + dbName + ";";
            com.ExecuteNonQuery();

            foreach (string objectName in Ddl.Keys)
            {
                com.CommandText = string.Format(@"SELECT OBJECT_ID('dbo.{0}')", objectName);
                var obj = com.ExecuteScalar();
                if (obj is DBNull)
                {
                    com.CommandText = Ddl[objectName];
                    com.ExecuteNonQuery();
                }
            }
        }
	}
}
