using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Tools.MigrationHelper.Tasks.CheckTask
{
	/// <summary>
	/// Данный класс представляет собой описание отдельного теста, использующегося при проверке консистентности БД.
	/// </summary>
	[Serializable]
	public class Test
	{
		/// <summary>
		/// Соединение с БД следует передавать открытым
		/// </summary>
		[XmlIgnore]
		public SqlConnection Connection { get; set; }
		public string Title { get; set; }
		public string SqlCommand { get; set; }

		public Test()
		{
		}

		public Test(SqlConnection connection)
		{
			this.Connection = connection;
		}

		/// <summary>
		/// Выполнить тест
		/// </summary>
		/// <returns>true в случае успешного выполнения</returns>
		public bool Check()
		{
			// успешным считается запрос, вернувший 0
			return this.ExecuteScalar<int>() == 0;
		}

		private T ExecuteScalar<T>()
		{
			var sql = string.Format(this.SqlCommand, "COUNT(1)");
			SqlCommand cmd = new SqlCommand(sql, Connection);
			return (T)cmd.ExecuteScalar();
		}
	}
}
