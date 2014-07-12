using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using MigrationHelper;
using Platform.DbCmd;
using Tools.MigrationHelper.DbManager.DbActions.Interfaces;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.DbManager.DbActions
{
	/// <summary>
	/// 
	/// </summary>
	public class InsertRows : DbActionBase
	{
		private List<string> _fieldsName;

		/// <summary>
		/// Генератор комнад INSERT
		/// </summary>
		/// <param name="rows">Строки, которые следует вставить в таблицу</param>
		/// <param name="fieldsName">Список наименования полей</param>
		public InsertRows(IEnumerable<DataRow> rows, List<string> fieldsName=null)
		{
			this.Rows = rows;
			_fieldsName = fieldsName;
			TplAction = "Создание строк в таблице {0} ({1}):" + Environment.NewLine;
			TplEmptyAction = "Холостая операция создания строк";
		}

		/// <summary>
		/// Генератор комнад INSERT
		/// </summary>
		/// <param name="rows">Строки, которые следует вставить в таблицу</param>
		public InsertRows(DataRow row)
		{
			this.Rows = new List<DataRow> {row};
		}

		#region Implementation of IDbAction

		/// <summary>
		/// Возвращает SQL команду
		/// </summary>
		public override List<SqlCommand> GetCommand()
		{
			List<SqlCommand> result = new List<SqlCommand>();
			if (!Rows.Any())
				return result;

			const string sqlTpl = "INSERT INTO {0} ({1}) VALUES ({2});";
			//const string sqlTpl = "INSERT INTO {0} ({1}) VALUES {2};";
			DataTable table = Rows.First().Table;
			List<string> fieldsName = _fieldsName ??
			                          (from DataColumn c in table.Columns where c.ColumnName != Names.Tstamp select c.ColumnName).ToList();
			string columnNames = getColumnNames(fieldsName);

			if (IsIdentity) result.Add(getSetIdentityInsertCmd(table.TableName, true));
			//int i = 0;
			//int k = fieldsName.Count;
			//string placeholders = string.Join(",", Rows.Select(a => "(" + getValuePlaceholders2(fieldsName, i++ * k) + ")"));
			//object[] values = Rows.SelectMany(a => getValues(fieldsName, a)).ToArray();
			//SqlCmd sqlCmd = new SqlCmd();
			//SqlCommand cmd = sqlCmd.GetSqlCmd(string.Format(sqlTpl, table.TableName, columnNames, placeholders), values);
			//result.Add(cmd);
			foreach (var row in Rows)
			{
				string placeholders =
					getValuePlaceholders(fieldsName);

				object[] values = getValues(fieldsName, row);
				var sqlCmd = new SqlCmd();
				var cmd = sqlCmd.GetSqlCmd(string.Format(sqlTpl, table.TableName, columnNames, placeholders), values);
				result.Add(cmd);
			}
			if (IsIdentity) result.Add(getSetIdentityInsertCmd(table.TableName, false));

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
				var idColumn = Rows.First().Table.Columns[Names.Id];
				return idColumn != null ? idColumn.AutoIncrement : false;
			}
		}

		#endregion

		#region Private Methods

		private SqlCommand getSetIdentityInsertCmd(string tableName, bool on)
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

		private string getValuePlaceholders2(IEnumerable<string> fieldsName, int i)
		{
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
