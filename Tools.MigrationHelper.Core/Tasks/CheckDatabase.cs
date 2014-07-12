using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.DbManager;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("CheckDatabase")]
    public class CheckDatabase : DbDeployTask
    {
        /// <summary>
        /// Сравнение базы данных с набором xml файлов.
        /// Результатом будут записи которые в базе отсутствуют или изменены
        /// </summary>
        protected override void ExecuteTask()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                try
                {
                    var targetDb = new DbDataSet(DevId);
                    DbDataSet targetFs;
                    if (!string.IsNullOrWhiteSpace(SourcePath))
                    {
                        Log(Level.Verbose, "Считываем метаданные из файловой системы...");
                        targetFs = new DbDataSet(DevId);
                        targetFs.FromFs(SourcePath);
                    }
                    else
                    {
                        throw new Exception("Не указан параметр TargetPath!");
                    }
                    Log(Level.Verbose, "Считываем метаданные из БД...");
                    targetDb.FromDb(connection);
                    Log(Level.Verbose, "Сравнение таблиц");
                    CompareMetadataTables(targetFs, targetDb);

                }
                catch (BuildException)
                {
                    throw;
                }
                catch (Exception ex)
                {

                    Fatal("Фатальная ошибка ", ex);
                }
            }
        }

        private void CompareMetadataTables(DbDataSet targetFs, DbDataSet targetDb)
        {
            var list = targetFs.DataSet.Tables.Cast<DataTable>().ToList();
            Parallel.ForEach(list, table =>
                {
                    var resultlist = new List<string>();
                    if (!table.TableName.Contains("ml.") && !table.TableName.Contains("ref.DistributiveData"))
                        resultlist = CompareTable(targetFs, targetDb, table.TableName);
                    if(resultlist.Any())
                        resultlist.Add("---------------------------------------------------------------------------------------");
                    foreach (var res in resultlist)
                    {
                        Log(Level.Verbose, res);
                    }
                });
        }

        /// <summary>
        /// Сравнение таблицы из двух объектов Metadata
        /// </summary>
        /// <param name="targetFs">Метаданные полученные из xml файлов</param>
        /// <param name="targetDb">Метаданные содержащиеся в таблице из базы</param>
        /// <param name="tableName">Наименование сравниваемой таблицы</param>
        /// <returns></returns>
        private List<string> CompareTable(DbDataSet targetFs, DbDataSet targetDb, string tableName)
        {
            var result = new List<string>();
            var rows = new List<DataRow>(targetFs.DataSet.Tables[tableName].AsEnumerable());
            if (targetDb.DataSet.Tables[tableName] == null)
            {
                result.Add(string.Format("Таблица {0} отсутствует в бд.",
                                         tableName));
            }
            else
            {
                foreach (var row in rows)
                {
                    DataRow rowDb =
                            targetDb.DataSet.Tables[tableName].AsEnumerable().SingleOrDefault(
                                a => int.Parse(a[Names.Id].ToString()) == (int) row[Names.Id]);
                        if (rowDb == null)
                        {
                            result.Add(string.Format("В базе в таблице {0} отсутствует запись с идентификатором '{1}'.",
                                                     tableName, row[Names.Id]));
                        }
                        else
                        {
                           result.AddRange(CompareRow(row, rowDb));
                        }
                }
            }
            return result;
        }

        /// <summary>
        /// Сравнение двух строк
        /// </summary>
        /// <param name="rowFs">Строка из таблицы принадлежащей метаданным из xml файлов</param>
        /// <param name="rowDb">Строка из таблицы принадлежащей метаданным из базы</param>
        /// <returns></returns>
        private IEnumerable<string> CompareRow(DataRow rowFs, DataRow rowDb)
        {
            var result = new List<string>();
            if (rowFs == null || rowDb == null)
            {
                result.Add(string.Format("Сравнение пустых строк не имеет смысла."));
                return result;
            }

            string tableName = rowFs.Table.TableName;
            List<DataColumn> columns = rowFs.Table.Columns.Cast<DataColumn>().Where(a => a.ColumnName != Names.Tstamp).ToList();

            foreach (var columnFs in columns)
            {
                string columnName = columnFs.ColumnName;
                if (rowDb.Table.Columns[columnName] == null)
                {
                    result.Add(string.Format("В базе в таблице '{0}' отсутствует колонка '{1}'.",
                        tableName, columnName));
                }
                else
                {
                    if (rowFs[columnName].ToString() != rowDb[columnName].ToString())
                    {
                        result.Add(string.Format("В базе в таблице '{0}' для записи с идентификатором '{1}' в поле {2} изменено значение.",
                            tableName, rowFs[Names.Id], columnName));
                    }
                }
            }
            return result;
        }
    }
}
