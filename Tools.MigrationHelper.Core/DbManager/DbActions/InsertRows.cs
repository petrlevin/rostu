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
	/// 
	/// </summary>
    public class InsertRows : DbActionBatch
	{
		private readonly List<string> _fieldsName;

		/// <summary>
		/// Генератор комнад INSERT
		/// </summary>
		/// <param name="rows">Строки, которые следует вставить в таблицу</param>
		/// <param name="fieldsName">Список наименования полей</param>
        public InsertRows(IEnumerable<DataRow> rows, List<string> fieldsName = null)
		{
            Actions = new List<DbAction>();
		    foreach (var dataRow in rows)
		    {
                var action = new DbAction { Row = dataRow, ActionBatch = this, TableName = dataRow.Table.TableName };
                Actions.Add(action);
		    }
			_fieldsName = fieldsName;
			TplAction = "Создание строк в таблице {0} ({1}):" + Environment.NewLine;
			TplEmptyAction = "Холостая операция создания строк";
		}

		/// <summary>
		/// Генератор комнад INSERT
		/// </summary>
		/// <param name="row">Строки, которые следует вставить в таблицу</param>
		public InsertRows(DataRow row)
		{
            var action = new DbAction { Row = row, ActionBatch = this, TableName = row.Table.TableName };
            Actions.Add(action);
		}

		#region Implementation of IDbAction

//		/// <summary>
//		/// Возвращает SQL команду
//		/// </summary>
//		public override List<SqlCommand> GetCommand()
//		{
//			var result = new List<SqlCommand>();
//            if (Actions == null || !Actions.Any())
//				return result;
//
//			const string sqlTpl = "INSERT INTO {0} ({1}) VALUES ({2});";
//			//const string sqlTpl = "INSERT INTO {0} ({1}) VALUES {2};";
//            DataTable table = Actions.First().Row.Table;
//			List<string> fieldsName = _fieldsName ??
//			                          (from DataColumn c in table.Columns where c.ColumnName==Names.Id || (c.ColumnName != Names.Tstamp && !c.ReadOnly) select c.ColumnName).ToList();
//			string columnNames = getColumnNames(fieldsName);
//			string validTableName = "[" + table.TableName.Replace(".", "].[") + "]";
//			
//			if (IsIdentity) result.Add(GetSetIdentityInsertCmd(validTableName, true));
//
//            foreach (var action in Actions)
//			{
//				string placeholders =
//					getValuePlaceholders(fieldsName);
//
//                object[] values = getValues(fieldsName, action.Row);
//				var sqlCmd = new SqlCmd();
//				var cmd = sqlCmd.GetSqlCmd(string.Format(sqlTpl, validTableName, columnNames, placeholders), values);
//				result.Add(cmd);
//			}
//			if (IsIdentity) result.Add(GetSetIdentityInsertCmd(validTableName, false));
//
//			return result;
//		}

	    /// <summary>
	    /// Возвращает SQL команду
	    /// </summary>
        public override List<SqlCommand> GetCommand(DataRow row)
	    {
	        List<SqlCommand> result = new List<SqlCommand>();

	        const string sqlTpl = "INSERT INTO {0} ({1}) VALUES ({2});";

	        DataTable table = row.Table;
	        List<string> fieldsName = _fieldsName ??
	                                  (from DataColumn c in table.Columns
	                                   where c.ColumnName == Names.Id || (c.ColumnName != Names.Tstamp && !c.ReadOnly)
	                                   select c.ColumnName).ToList();

	        string columnNames = getColumnNames(fieldsName);
	        string validTableName = "[" + table.TableName.Replace(".", "].[") + "]";

	        if (IsIdentity) result.Add(GetSetIdentityInsertCmd(validTableName, true));

	        string placeholders =
	            getValuePlaceholders(fieldsName);

	        object[] values = getValues(fieldsName, row);
	        var sqlCmd = new SqlCmd();
	        var cmd = sqlCmd.GetSqlCmd(string.Format(sqlTpl, validTableName, columnNames, placeholders), values);
	        result.Add(cmd);

	        if (IsIdentity) result.Add(GetSetIdentityInsertCmd(validTableName, false));

	        return result;
	    }

	    /// <summary>
		/// Признак того, что таблица, в которую осуществляется вставка записей имеет IDENTITY-колонку.
		/// </summary>
		/// <returns></returns>
		private bool IsIdentity
		{
			get
			{
                var idColumn = Actions.First().Row.Table.Columns[Names.Id];
				return idColumn != null && idColumn.AutoIncrement;
			}
		}

		#endregion

		#region Private Methods

		private SqlCommand GetSetIdentityInsertCmd(string tableName, bool on)
		{
			return new SqlCommand(getSetIdentityInsertCmdTxt(tableName, on));
		}

		private string getSetIdentityInsertCmdTxt(string tableName, bool on)
		{
			return string.Format("SET IDENTITY_INSERT {0} {1}", tableName, on ? "ON" : "OFF");
		}

		private string getColumnNames(IEnumerable<string> fieldsName)
		{
			return string.Join(",",
			                   (from string c in fieldsName where c != Names.Tstamp select "[" + c + "]"));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fieldsName">Список имен полей</param>
		/// <returns>Пример: "{0},{1},{2}"</returns>
		private string getValuePlaceholders(IEnumerable<string> fieldsName)
		{
			int i = 0;
			return string.Join(",",
			                       (from string c in fieldsName
			                        where c != Names.Tstamp
			                        select string.Format("{{{0}}}", i++)));
		}

	    private object[] getValues(IEnumerable<string> fieldsName, DataRow row)
		{
			return (from c in fieldsName where c != Names.Tstamp select row[c]).ToArray();
		}

		#endregion
	}
}
