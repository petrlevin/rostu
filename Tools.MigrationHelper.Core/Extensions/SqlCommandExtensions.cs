using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Extensions
{
	public static class SqlCommandExtensions
	{
		/// <summary>
		/// SQL-выражение вместе со значениями всех параметров. 
		/// Используется при логировании.
		/// </summary>
		/// <remarks>
		/// Не следует переименовывать данный метод в ToString(), т.к.:
		/// 1. При переносе куска кода, в которм используется данный метод с именем ToString, в класс, где не подключен данный нэймспэйс, 
		/// ошибки не произойдет, т.к. будет использован стандартный ToString(), а вот результат будет заметен не сразу, и не очевидно будет где исправлять.
		/// </remarks>
		/// <returns></returns>
		public static string AsString(this SqlCommand cmd)
		{

			var sb = new StringBuilder();
			sb.AppendLine(cmd.CommandText);
			foreach (SqlParameter parameter in cmd.Parameters)
			{
				sb.AppendLine(string.Format("{0} = {1}", parameter.ParameterName, parameter.Value));
			}
			return sb.ToString();
		}
	}
}
