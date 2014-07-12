using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.DbCmd;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.DbManager.DbActions
{
	/// <summary>
	/// Класс получения команд UPDATE после сравнения двух наборов
	/// </summary>
	public class UpdateRows : DbActionBase
	{
		private IEnumerable<DataRow> _sourceRows;
		private IEnumerable<DataRow> _targetFsRows;
		private IEnumerable<DataRow> _targetDbRows;

		#region Overrides of DbAction

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="sourceRows">Набор строк из обновления</param>
		/// <param name="targetFsRows">Набор строк из предыдущего обновления</param>
		public UpdateRows(IEnumerable<DataRow> sourceRows, IEnumerable<DataRow> targetFsRows, IEnumerable<DataRow> targetDbRows)
		{
			_sourceRows = sourceRows;
			_targetFsRows = targetFsRows;
			_targetDbRows = targetDbRows;
		}

		/// <summary>
		/// Возвращает SQL команду
		/// </summary>
		public override List<SqlCommand> GetCommand()
		{
			List<SqlCommand> result = new List<SqlCommand>();
			string tableName = _sourceRows.First().Table.TableName;
			List<DataColumn> sourceColumns = _sourceRows.First().Table.Columns.Cast<DataColumn>().Where(a => a.ColumnName != Names.Id && a.ColumnName!=Names.Tstamp).ToList();
			List<DataColumn> targetColumns = _targetFsRows.First().Table.Columns.Cast<DataColumn>().Where(a => a.ColumnName != Names.Id && a.ColumnName!=Names.Tstamp).ToList();
			foreach (DataRow sourceRow in _sourceRows)
			{
				DataRow targetFsRow = _targetFsRows.SingleOrDefault(a => a.Field<int>(Names.Id) == sourceRow.Field<int>(Names.Id));
				DataRow targetDbRow = _targetDbRows.SingleOrDefault(a => a.Field<int>(Names.Id) == sourceRow.Field<int>(Names.Id));
				Dictionary<string, object> updateValues = new Dictionary<string, object>();
				string sqlTpl = string.Format("UPDATE {0} SET {{0}} WHERE [id]={1};", tableName, sourceRow.Field<int>(Names.Id));
				if (targetFsRow != null && targetDbRow != null)
				{
					foreach (DataColumn sourceColumn in sourceColumns)
					{
						string columnName = sourceColumn.ColumnName;
						DataColumn targetColumn = targetColumns.SingleOrDefault(a => a.ColumnName == sourceColumn.ColumnName);
						if (targetColumn != null)
						{
							if (!sourceRow[columnName].Equals(targetFsRow[columnName]) &&
								targetFsRow[columnName].Equals(targetDbRow[columnName]))
							{
								updateValues.Add(columnName, sourceRow[sourceColumn]);
							}
						}
						else
						{
							updateValues.Add(columnName, sourceRow[sourceColumn]);
						}
					}
				}
				if (updateValues.Count>0)
				{
					SqlCmd sqlCmd = new SqlCmd();
					string placeHolder = getValuePlaceholders(updateValues.Select(a => a.Key));
					result.Add(sqlCmd.GetSqlCmd(string.Format(sqlTpl, placeHolder), updateValues.Select(a => a.Value).ToArray()));
				}
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
			                    select string.Format("{{{0}}}", i++)));

		}
	}
}
