using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.MigrationHelper.DbManager.DbActions.Interfaces;

namespace Tools.MigrationHelper.DbManager.DbActions
{
	public abstract class DbActionBase : IDbAction
	{
		protected string TplEmptyAction;
		protected string TplAction;

		#region Implementation of IDbAction

		/// <summary>
		/// Действия, которые должны предшествовать данному
		/// </summary>
		public List<IDbAction> DependsOn { get; protected set; }
		
		public IEnumerable<DataRow> Rows { get; protected set; }

		/// <summary>
		/// Возвращает SQL команду
		/// </summary>
		public abstract List<SqlCommand> GetCommand();
		
		/// <summary>
		/// Получить текстовое представления выполняемого действия
		/// </summary>
		/// <returns></returns>
		public virtual string Verbose()
		{
			var sb = new StringBuilder();

			if (!this.Rows.Any())
			{
				sb.Append(TplEmptyAction);
			}
			else
			{
				var columnNames = this.Table.Columns.Cast<DataColumn>()
					.Select(col => col.ColumnName)
					.Aggregate((a, b) => string.Format("{0}, {1}", a, b));

				sb.AppendFormat(TplAction, this.Table.TableName, columnNames);
				int n = 0;
				foreach (var dataRow in Rows)
				{
					string row = dataRow.ItemArray
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
				return Rows.Any() ? Rows.First().Table : null;
			}
		}
	}
}
