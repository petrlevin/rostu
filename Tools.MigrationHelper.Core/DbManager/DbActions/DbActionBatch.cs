using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Tools.MigrationHelper.Core.DbManager.DbActions.Interfaces;

namespace Tools.MigrationHelper.Core.DbManager.DbActions
{
    public abstract class DbActionBatch : IDbActionBatch
	{
		protected string TplEmptyAction;
		protected string TplAction;

		#region Implementation of IDbAction

		/// <summary>
		/// Действия, которые должны предшествовать данному
		/// </summary>
        public List<IDbActionBatch> DependsOn { get; protected set; }
		
		public List<DbAction> Actions { get; protected set; }

//		/// <summary>
//		/// Возвращает SQL команду
//		/// </summary>
//		public abstract List<SqlCommand> GetCommand();

        /// <summary>
        /// Возвращает SQL команду
        /// </summary>
        public abstract List<SqlCommand> GetCommand(DataRow row);

        /// <summary>
		/// Получить текстовое представления выполняемого действия
		/// </summary>
		/// <returns></returns>
		public virtual string Verbose()
		{
			var sb = new StringBuilder();

            if (Actions == null || !Actions.Any())
			{
				sb.Append(TplEmptyAction);
			}
			else
			{
				var columnNames = this.Table.Columns.Cast<DataColumn>()
					.Select(col => col.ColumnName)
					.Aggregate((a, b) => string.Format("{0}, {1}", a, b));

				sb.AppendFormat(TplAction, this.Table.TableName, columnNames);
                foreach (var action in Actions)
				{
                    string row = action.Row.ItemArray
						.Select(o => o == null || o == DBNull.Value ? String.Empty : o.ToString())
						.Aggregate((a, b) => string.Format("{0}, {1}", a, b));

					sb.AppendLine(row/*string.Format("{0}. {1}", ++n, row)*/);
				}
			}
			return sb.ToString();
		}

		#endregion

		public DataTable Table
		{
			get
			{
                return Actions.Any() ? Actions.First().Row.Table : null;
			}
		}
	}
}
