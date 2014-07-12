using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.DbManager;
using Tools.MigrationHelper.Core.Helpers;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
	[TaskName("updatedb")]
	public class UpdateDb : DeployAppDb
	{
		 [TaskAttribute("targetpath", Required = false)]
		 public string TargetPath
		 {
			 get;
			 set;
		 }

         [TaskAttribute("finalstatepath", Required = false)]
         public string FinalStatePath
         {
             get;
             set;
         }

	    protected override void ExecuteTask()
		 {
			 using (var connection = new SqlConnection(ConnectionString))
			 {
				 connection.Open();
				 try
				 {
                     Log(Level.Verbose, "Выполнение PreUpdateDb скрипта...");
                     TaskHelper.ExecutePreUpdateDbScript(connection);
					 TaskHelper.DisableIndex(connection);
					 DisableVersionongTriggers(connection);
                     TaskHelper.DisableEntityLogicTrigger(connection);

					 if (IsDeveloper())
					 {
						 Log(Level.Verbose, "Создаем таблицу для хранения ревизий xml файлов...");
						 TaskHelper.CreateDevDbRevisionTable(connection);
					 }

					 var source = new DbDataSet(DevId, ConnectionString);
					 var targetDb = new DbDataSet(DevId);
					 DbDataSet targetFs = null;
                     DbDataSet finalState;//состояние к которому стремимся

					 targetDb.FromDb(connection);
					 if (!string.IsNullOrWhiteSpace(TargetPath))
					 {
						 targetFs = new DbDataSet(DevId);
						 targetFs.FromFs(TargetPath);
					 }

                     if (!string.IsNullOrWhiteSpace(FinalStatePath))
                     {
                         finalState = new DbDataSet(DevId);
                         finalState.FromFs(FinalStatePath);
                     }
                     else
                     {
                         finalState = new DbDataSet(DevId);
                         finalState.FromFs(SourcePath);
                     }

					 if (targetFs != null)
						 CompareMetadataTables(targetFs, targetDb, new List<string> { "ref.Entity", "ref.Entityfield" });

					 Log(Level.Verbose, "Считываем метаданные из файловой системы...");
					 source.FromFs(SourcePath);
                     
                     var cmpRes = new MetadataCompareResult(source, targetDb, targetFs, finalState, connection);
                     //Сравнение и получение команд
                     cmpRes.Compare();

					 Log(Level.Verbose, "Применение метаданных к базе ...");
					 cmpRes.Execute();

//					 Log(Level.Verbose, cmpRes.Verbose());
					 SetStartIdentity();
                     Log(Level.Verbose, "Включение индексов и версионных триггеров");
					 TaskHelper.EnableIndex(connection);
					 EnableVersionongTriggers(connection);
                     Log(Level.Verbose, "Выполнение PostUpdateDb скрипта...");
					 TaskHelper.ExecutePostUpdateDbScript(connection);
				 }
				 catch (BuildException)
				 {
					 throw;
				 }

				 catch (Exception ex)
				 {

					 Fatal("Фатальная ошибка при развертывании базы приложения", ex);
				 }
			 }

		 }

		 private void DisableVersionongTriggers(SqlConnection connection)
		 {
		     const string commandText = "declare @str varchar(max); " +
		                                "set @str=(select 'ALTER TABLE ['+c.name+'].['+b.name+'] DISABLE TRIGGER ['+a.name+'];' AS [text()] " +
		                                "from sys.triggers a inner join sys.tables b on b.object_id=a.parent_id " +
		                                "inner join sys.schemas c on c.schema_id=b.schema_id " +
		                                "where a.name like '%_IsVersioning' FOR XML PATH ('')); " +
		                                "exec (@str)";
             TaskHelper.ExecuteSQlCommand(connection,commandText);
		 }

		 private void EnableVersionongTriggers(SqlConnection connection)
		 {


		     const string commandText = "declare @str varchar(max); " +
		                                "set @str=(select 'ALTER TABLE ['+c.name+'].['+b.name+'] ENABLE TRIGGER ['+a.name+'];' AS [text()] " +
		                                "from sys.triggers a inner join sys.tables b on b.object_id=a.parent_id " +
		                                "inner join sys.schemas c on c.schema_id=b.schema_id " +
		                                "where a.name like '%_IsVersioning' FOR XML PATH ('')); " +
		                                "exec (@str)";
             TaskHelper.ExecuteSQlCommand(connection, commandText);
		 }

		 /// <summary>
		 /// Сравнение таблиц из двух объектов Metadata
		 /// </summary>
		 /// <param name="targetFs">Метаданные полученные из предыдущего обновления</param>
		 /// <param name="targetDb">Метаданные содержащиеся в обновляемой таблице</param>
		 /// <param name="tablesName">Наименование сравниваемых таблиц</param>
		 /// <returns></returns>
		 private bool CompareMetadataTables(DbDataSet targetFs, DbDataSet targetDb, IEnumerable<string> tablesName)
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
		 private bool CompareTable(DbDataSet targetFs, DbDataSet targetDb, string tableName)
		 {
			 bool result = true;
		     List<DataRow> rows = new List<DataRow>(targetFs.DataSet.Tables[tableName].AsEnumerable());
		     Parallel.ForEach(rows, row =>
		         {
                     DataRow rowDb =
                     targetDb.DataSet.Tables[tableName].AsEnumerable().SingleOrDefault(
                         a => a.Field<int>(Names.Id) == row.Field<int>(Names.Id));
                     if (rowDb == null)
                     {

                         Log(Level.Verbose, "В обновляемой базе в таблице {0} отсутствует запись с наименованием '{1}' и идентификатором '{2}'.",
                                                             tableName, row.Field<string>(Names.Name), row.Field<int>(Names.Id));
                         result = false;
                     }
                     else
                     {
                         result = result && CompareRow(row, rowDb);
                     }
		         });
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
			 if (rowFs == null || rowDb == null)
			 {
                 Log(Level.Verbose, "Сравнение пустых строк не имеет смысла.");
				 return false;
			 }

			 string tableName = rowFs.Table.TableName;
		     List<DataColumn> columns = rowFs.Table.Columns.Cast<DataColumn>().Where(a => a.ColumnName != Names.Tstamp).ToList();

			 Parallel.ForEach(columns, columnFs =>
             {
				 string columnName = columnFs.ColumnName;
				 if (rowDb.Table.Columns[columnName] == null)
				 {
                     Log(Level.Verbose,
						 "В обновляемой базе в таблице '{0}' отсутствует колонка '{1}'.",
						 tableName, columnName);
					 result = false;
				 }
				 else
				 {
                     if (!rowFs[columnName].Equals(rowDb[columnName]))
					 {
                         Log(Level.Verbose,
							 "В обновляемой базе в таблице '{0}' для записи с идентификатором '{1}' в поле {2} изменено значение.",
							 tableName, rowFs.Field<int>(Names.Id), columnName);
						 result = false;
					 }
				 }
			 });

			 return result;
		 }
	}
}
