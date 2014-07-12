using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Platform.DbCmd;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.DbManager.DbActions
{
    public class DeleteRows : DbActionBatch
	{
		public DeleteRows(IEnumerable<DataRow> rows)
		{
            Actions = new List<DbAction>();
            foreach (var dataRow in rows)
            {
                var action = new DbAction() { Row = dataRow, ActionBatch = this, TableName = dataRow.Table.TableName };
                Actions.Add(action);
            }

			TplAction = "Удаление строк из таблицы {0} ({1}):" + Environment.NewLine;
			TplEmptyAction = "Холостая операция удаления строк";
		}

		#region Implementation of IDbAction

//		/// <summary>
//		/// Возвращает SQL команду
//		/// </summary>
//		public override List<SqlCommand> GetCommand()
//		{
//			var result = new List<SqlCommand>();
//            if (!Actions.Any())
//				return result;
//            string tableName = Actions.First().Row.Table.TableName;
//            string validTableName = "[" + tableName.Replace(".", "].[") + "]";
//			var textCommand = new StringBuilder();
//
//		    if (tableName.StartsWith("ml."))
//		    {
//                var columns = Actions.First().Row.Table.Columns.Cast<DataColumn>().Where(a => !a.ColumnName.EndsWith("Order") && a.ColumnName != Names.Tstamp).ToList();
//                var column1 = columns[0].ColumnName;
//                var column2 = columns[1].ColumnName;
//                foreach (var action in Actions)
//		        {
//                    textCommand.AppendFormat("DELETE FROM {0} WHERE [{1}] = {2} AND [{3}] = {4}", validTableName, column1, action.Row[column1], column2, action.Row[column2]);
//		        }
//		    }
//		    else
//		    {
//                textCommand.AppendFormat("DELETE FROM {0} WHERE [id] IN ({1})", validTableName, string.Join(",", Actions.Select(a => a.Row.Field<int>("id").ToString())));    
//		    }
//		    
//			SqlCmd sqlCmd = new SqlCmd();
//			SqlCommand command = sqlCmd.GetSqlCmd(textCommand.ToString(),null);
//			result.Add(command);
//			return result;
//		}

        /// <summary>
        /// Возвращает SQL команду
        /// </summary>
        public override List<SqlCommand> GetCommand(DataRow row)
        {
            var result = new List<SqlCommand>();
            if (row == null)
                return result;
            string tableName = row.Table.TableName;
            string validTableName = "[" + tableName.Replace(".", "].[") + "]";
            var textCommand = new StringBuilder();

            if (tableName.StartsWith("ml."))
            {
                var columns = row.Table.Columns.Cast<DataColumn>().Where(a => !a.ColumnName.EndsWith("Order") && a.ColumnName != Names.Tstamp).ToList();
                var column1 = columns[0].ColumnName;
                var column2 = columns[1].ColumnName;
                textCommand.AppendFormat("DELETE FROM {0} WHERE [{1}] = {2} AND [{3}] = {4}", validTableName, column1,
                                         row[column1], column2, row[column2]);
            }
            else
            {
                textCommand.AppendFormat("DELETE FROM {0} WHERE [id] IN ({1})", validTableName, string.Join(",", row.Field<int>("id").ToString()));
            }

            SqlCmd sqlCmd = new SqlCmd();
            SqlCommand command = sqlCmd.GetSqlCmd(textCommand.ToString(), null);
            result.Add(command);
            return result;
        }

        #endregion
	}
}
