using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;

namespace Platform.Dal
{
	public static class SqlCommandExtensions
	{
		private static void _beforeExec(SqlCommand command)
		{
			if (command.Connection.State!=ConnectionState.Open)
				command.Connection.Open();
		}

		private static void _afterExec(SqlCommand command)
		{
			command.Connection.Close();
		}

		private static string _getErrorMsg(string sql)
		{
			return string.Format("<br/><b>Прозошла ошибка выполнения комадны:</b> <br/>{0}<br/>", sql);
		}

		
		public static List<T> Select<T>(this SqlCommand command) where T : class, new()
		{
			_beforeExec(command);

			List<T> result = new List<T>();
			Dictionary<string, PropertyInfo> props = typeof(T).GetProperties().ToDictionary(p => p.Name.ToLower());

			try
			{
				SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
				DataTable table = new DataTable();
				sqlDataAdapter.Fill(table);
				foreach (DataRow row in table.Rows)
				{
					T item = new T();
					foreach (DataColumn column in row.Table.Columns)
					{
						string name = column.ColumnName.ToLower();
						if (row[name] != DBNull.Value
							&& props.ContainsKey(name)
							&& props[name].CanWrite
							&& props[name].GetSetMethod(/*nonPublic*/ true).IsPublic
							)
						{
							object value = row[name];
							PropertyInfo p = props[name];
							p.SetValue(item, value, null);
						}
					}
					result.Add(item);
				}
			}
			catch (Exception exception)
			{
				throw new PlatformException(_getErrorMsg(command.CommandText), exception);
			} finally
			{
				_afterExec(command);
			}

			return result;
		}

	}
}
