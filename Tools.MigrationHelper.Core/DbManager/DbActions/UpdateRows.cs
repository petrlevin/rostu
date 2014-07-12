using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Platform.DbCmd;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.DbManager.DbActions
{
	/// <summary>
	/// Класс получения команд UPDATE после сравнения двух наборов
	/// </summary>
    public class UpdateRows : DbActionBatch
	{
		private readonly IEnumerable<DataRow> _sourceRows;
		private readonly List<DataRow> _targetFsRows;
		private readonly List<DataRow> _targetDbRows;
        private readonly List<DataRow> _finalStateRows;

		#region Overrides of DbAction

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="sourceRows">Набор строк из обновления</param>
		/// <param name="targetFsRows">Набор строк из предыдущего обновления</param>
        public UpdateRows(IEnumerable<DataRow> sourceRows, IEnumerable<DataRow> targetFsRows, IEnumerable<DataRow> targetDbRows, IEnumerable<DataRow> finalStateRows)
		{
            Actions = new List<DbAction>();
		    var sourceRowsList = sourceRows.ToList();
            foreach (var dataRow in sourceRowsList)
            {
                var action = new DbAction() { Row = dataRow, ActionBatch = this, TableName = dataRow.Table.TableName };
                Actions.Add(action);
            }
            _sourceRows = sourceRowsList;
			_targetFsRows = targetFsRows.ToList();
			_targetDbRows = targetDbRows.ToList();
            _finalStateRows = finalStateRows.ToList();
		}

//		/// <summary>
//		/// Возвращает SQL команду
//		/// </summary>
//		public override List<SqlCommand> GetCommand()
//		{
//			List<SqlCommand> result = new List<SqlCommand>();
//		    if (!_sourceRows.Any()) return result;
//
//			string tableName = _sourceRows.First().Table.TableName;
//            string validTableName = "[" + tableName.Replace(".", "].[") + "]";
//			List<DataColumn> sourceColumns = _sourceRows.First().Table.Columns.Cast<DataColumn>().Where(a => a.ColumnName != Names.Id && a.ColumnName!=Names.Tstamp).ToList();
//			List<DataColumn> targetColumns = _targetFsRows.First().Table.Columns.Cast<DataColumn>().Where(a => a.ColumnName != Names.Id && a.ColumnName!=Names.Tstamp).ToList();
//
//			foreach (DataRow sourceRow in _sourceRows)
//			{
//			    string sqlTpl = string.Empty;
//			    DataRow targetFsRow;
//			    DataRow targetDbRow;
//                //для сущностей у которых нет id(пока что это только мультилинк)
//			    if (tableName.StartsWith("ml."))
//			    {
//                    var columns = sourceRow.Table.Columns.Cast<DataColumn>().Where(a => !a.ColumnName.EndsWith("Order") && a.ColumnName != Names.Tstamp).ToList();
//			        var column1 = columns[0].ColumnName;
//			        var column2 = columns[1].ColumnName;
//
//                    targetFsRow = _targetFsRows.SingleOrDefault(a =>
//                        Convert.ToInt32(a[column1]) == Convert.ToInt32(sourceRow[column1])
//                        && Convert.ToInt32(a[column2]) == Convert.ToInt32(sourceRow[column2])
//                        );
//
//                    targetDbRow = _targetDbRows.SingleOrDefault(a =>
//                        Convert.ToInt32(a[column1]) == Convert.ToInt32(sourceRow[column1])
//                        && Convert.ToInt32(a[column2]) == Convert.ToInt32(sourceRow[column2])
//                        );
//
//                    sqlTpl = string.Format("UPDATE {0} SET {{0}} WHERE [{2}]={1} AND [{4}] = {3};", validTableName, Convert.ToInt32(sourceRow[column1]), column1, Convert.ToInt32(sourceRow[column2]), column2);
//			    }
//			    else
//			    {
//                    targetFsRow = _targetFsRows.SingleOrDefault(a => Convert.ToInt32(a[Names.Id]) == Convert.ToInt32(sourceRow[Names.Id]));
//                    targetDbRow = _targetDbRows.SingleOrDefault(a => Convert.ToInt32(a[Names.Id]) == Convert.ToInt32(sourceRow[Names.Id]));
//
//                    sqlTpl = string.Format("UPDATE {0} SET {{0}} WHERE [id]={1};", validTableName, sourceRow.Field<int>(Names.Id));
//			    }
//			    
//				Dictionary<string, object> updateValues = new Dictionary<string, object>();
//
//				if (targetFsRow != null && targetDbRow != null)
//				{
//					foreach (DataColumn sourceColumn in sourceColumns)
//					{
//						string columnName = sourceColumn.ColumnName;
//						DataColumn targetColumn = targetColumns.SingleOrDefault(a => a.ColumnName == sourceColumn.ColumnName);
//						if (targetColumn != null && tableName != Names.RefProgrammability)
//						{
//							if (!sourceRow[columnName].Equals(targetFsRow[columnName]) &&
//								targetFsRow[columnName].Equals(targetDbRow[columnName]))
//							{
//								updateValues.Add(columnName, sourceRow[sourceColumn]);
//							}
//						}
//						else
//						{
//							updateValues.Add(columnName, sourceRow[sourceColumn]);
//						}
//					}
//				}
//				if (updateValues.Count>0)
//				{
//					SqlCmd sqlCmd = new SqlCmd();
//					string placeHolder = getValuePlaceholders(updateValues.Select(a => a.Key));
//					result.Add(sqlCmd.GetSqlCmd(string.Format(sqlTpl, placeHolder), updateValues.Select(a => a.Value).ToArray()));
//				}
//			}
//			return result;
//		}

	    public override List<SqlCommand> GetCommand(DataRow row)
	    {
	        var result = new List<SqlCommand>();

	        if (!_sourceRows.Any()) return result;

	        string tableName = _sourceRows.First().Table.TableName;
	        string validTableName = "[" + tableName.Replace(".", "].[") + "]";
	        List<DataColumn> sourceColumns =
	            _sourceRows.First()
	                       .Table.Columns.Cast<DataColumn>()
	                       .Where(a => a.ColumnName != Names.Id && a.ColumnName != Names.Tstamp && !a.ReadOnly)
	                       .ToList();
	        List<DataColumn> targetColumns =
	            _targetFsRows.First()
	                         .Table.Columns.Cast<DataColumn>()
                             .Where(a => a.ColumnName != Names.Id && a.ColumnName != Names.Tstamp && !a.ReadOnly)
	                         .ToList();

	        string sqlTpl = string.Empty;
	        DataRow targetFsRow;
	        DataRow targetDbRow;
            DataRow finalStateRow;
	        //для сущностей у которых нет id(пока что это только мультилинк)
	        if (tableName.StartsWith("ml."))
	        {
	            var columns =
                    row.Table.Columns.Cast<DataColumn>()
	                         .Where(a => !a.ColumnName.EndsWith("Order") && a.ColumnName != Names.Tstamp)
	                         .ToList();
	            var column1 = columns[0].ColumnName;
	            var column2 = columns[1].ColumnName;

	            targetFsRow = _targetFsRows.SingleOrDefault(a =>
	                                                        Convert.ToInt32(a[column1]) ==
                                                            Convert.ToInt32(row[column1])
	                                                        &&
	                                                        Convert.ToInt32(a[column2]) ==
                                                            Convert.ToInt32(row[column2])
	                );

	            targetDbRow = _targetDbRows.SingleOrDefault(a =>
	                                                        Convert.ToInt32(a[column1]) ==
                                                            Convert.ToInt32(row[column1])
	                                                        &&
	                                                        Convert.ToInt32(a[column2]) ==
                                                            Convert.ToInt32(row[column2])
	                );
	            finalStateRow = _finalStateRows.SingleOrDefault(a =>
	                                                          Convert.ToInt32(a[column1]) ==
	                                                          Convert.ToInt32(row[column1])
	                                                          &&
	                                                          Convert.ToInt32(a[column2]) ==
	                                                          Convert.ToInt32(row[column2])
	                );
	            sqlTpl = string.Format("UPDATE {0} SET {{0}} WHERE [{2}]={1} AND [{4}] = {3};", validTableName,
                                       Convert.ToInt32(row[column1]), column1, Convert.ToInt32(row[column2]),
	                                   column2);
	        }
	        else
	        {
	            targetFsRow =
                    _targetFsRows.SingleOrDefault(a => Convert.ToInt32(a[Names.Id]) == Convert.ToInt32(row[Names.Id]));

	            targetDbRow =
                    _targetDbRows.SingleOrDefault(a => Convert.ToInt32(a[Names.Id]) == Convert.ToInt32(row[Names.Id]));

	            finalStateRow =
	                _finalStateRows.SingleOrDefault(a => Convert.ToInt32(a[Names.Id]) == Convert.ToInt32(row[Names.Id]));

	            sqlTpl = string.Format("UPDATE {0} SET {{0}} WHERE [id]={1};", validTableName,
                                       row.Field<int>(Names.Id));
	        }

	        var updateValues = new Dictionary<string, object>();

	        if (targetFsRow != null && targetDbRow != null)
	        {
                foreach (DataColumn sourceColumn in sourceColumns)
                {
                    string columnName = sourceColumn.ColumnName;
                    DataColumn targetColumn = targetColumns.SingleOrDefault(a => a.ColumnName == sourceColumn.ColumnName);
                    if (targetColumn != null && tableName != Names.RefProgrammability)
                    {
                        if (!row[columnName].Equals(targetFsRow[columnName])
                            && finalStateRow != null
                            && !targetDbRow[columnName].Equals(finalStateRow[columnName]) 
                            && targetFsRow[columnName].Equals(targetDbRow[columnName]))
                        {
                            updateValues.Add(columnName, row[sourceColumn]);
                        }
                    }
                    else
                    {
                        if (finalStateRow != null && finalStateRow.Table.Columns[columnName] != null && !targetDbRow[columnName].Equals(finalStateRow[columnName]))
                            updateValues.Add(columnName, row[sourceColumn]);
                    }
                }
	        }
	        if (updateValues.Count > 0)
	        {
	            var sqlCmd = new SqlCmd();
	            string placeHolder = getValuePlaceholders(updateValues.Select(a => a.Key));
	            result.Add(sqlCmd.GetSqlCmd(string.Format(sqlTpl, placeHolder), updateValues.Select(a => a.Value).ToArray()));
	        }

	        return result;
	    }

	    #endregion

		private string getValuePlaceholders(IEnumerable<string> fieldsName)
		{
			int i = 0;
			return string.Join(",",
			                   (from string c in fieldsName
			                    where c != Names.Tstamp
			                    select string.Format("[{1}] = {{{0}}}", i++, c)));
		}
	}
}
