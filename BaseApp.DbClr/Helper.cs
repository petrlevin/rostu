using System;
using System.Data.SqlClient;
using System.Linq;
using Platform.DbClr;
using Platform.PrimaryEntities.DbEnums;

namespace BaseApp.DbClr
{
	/// <summary>
	/// Класс содержащий методы, общие для CRL объектов (функции, процедуры, триггеры)
	/// </summary>
	public static class Helper
	{
	    /// <summary>
	    /// 
	    /// </summary>
	    public const string LeftOuterJoin = "LEFT OUTER JOIN";
	  
        /// <summary>
	    /// 
	    /// </summary>
	    public const string InnerJoin = "INNER JOIN";

	    /// <summary>
		/// Выполнение SQL команды без получения результата и передачи параметров
		/// </summary>
		/// <param name="textCommand">SQL команда</param>
		public static void ExecCommand(string textCommand)
		{
			using (SqlConnection connection = new SqlConnection("context connection=true"))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(textCommand, connection))
				{
					command.ExecuteNonQuery();
				}
				connection.Close();
			}
		}

		/// <summary>
		/// Выполнение SQL команды с получением результата из первой строки первого столбца
		/// </summary>
		/// <param name="textCommand">SQL команда</param>
		public static object ExecCommandOurScalar(string textCommand)
		{
			object result;
			using (SqlConnection connection = new SqlConnection("context connection=true"))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(textCommand, connection))
				{
					result = command.ExecuteScalar();
				}
				connection.Close();
			}
			return result;
		}
	}
}
