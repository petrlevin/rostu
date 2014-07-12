using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Platform.DbCmd;
using Tools.MigrationHelper.DbManager.DbActions.Interfaces;

namespace Tools.MigrationHelper.DbManager.DbActions
{
	class DeleteRows : DbActionBase
	{
		public DeleteRows(IEnumerable<DataRow> rows)
		{
			Rows = rows;

			TplAction = "Удаление строк из таблицы {0} ({1}):" + Environment.NewLine;
			TplEmptyAction = "Холостая операция удаления строк";
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
			string tableName = Rows.First().Table.TableName;
			StringBuilder textCommand = new StringBuilder();
			textCommand.AppendFormat("DELETE FROM {0} WHERE [id] IN ({1})", tableName, string.Join(",", Rows.Select(a => a.Field<int>("id").ToString())));
			SqlCmd sqlCmd = new SqlCmd();
			SqlCommand command = sqlCmd.GetSqlCmd(textCommand.ToString(),null);
			result.Add(command);
			return result;
		}

		#endregion
	}
}
