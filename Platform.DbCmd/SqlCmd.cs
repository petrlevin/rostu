using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Platform.DbCmd
{
	// ToDo: переделать на Unity before, after и try-catch

	/// <summary>
	/// Helper-класс для выполнения команд на SQL Server'е.
	/// Содержит методы-обертки к командам класса <see cref="SqlCommand"/> (для удобства такие методы имеют такие же имена). 
	/// </summary>
	public class SqlCmd : IDisposable
	{
		#region Constructors

		public SqlCmd()
		{
		}

		public SqlCmd(SqlConnection connection)
			: this(connection, ConnectionType.SingleConnection)
		{
		}

		public SqlCmd(SqlConnection connection, ConnectionType connType)
		{
			this.Connection = connection;
			this.ConnectionType = connType;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Текущее соединение
		/// </summary>
		public SqlConnection Connection { get; set; }
        
        /// <summary>
        /// Тип текущего соединения
        /// </summary>
        public ConnectionType ConnectionType { get; set; }

		#endregion

		#region Public API

		/// <summary>
		/// Строка соединения в контексте
		/// </summary>
		public static SqlConnection ContextConnection
		{
			get { return new SqlConnection("context connection = true"); }
		}



		/// <summary>
		/// Выполнение команды SQL, возвращающей скалярное значение
		/// </summary>
		/// <param name="sql">
		/// Текст SQL-запроса
		/// </param>
		/// <param name="parameters">
		/// </param>
		public T ExecuteScalar<T>(string sql, params object[] parameters)
		{
			beforeExec(); 
			
			T result;
			try
			{
				using (var command = GetSqlCmd(sql, parameters))
				{
					var resObj = command.ExecuteScalar(); // разбив на 2 строки стало ясно где ошибка - в выполнении запроса или в преобразовании значения к типу T
					result = (T)resObj;
				}

			}
			catch (Exception exception)
			{
				throw new Exception(getErrorMsg(sql), exception);
			}

			afterExec(); 
			return result;
		}

		/// <summary>
		/// Выполнение команды, не возвращающей значений. Удобно при выполнении DDL.
		/// </summary>
		/// <param name="sql">SQL-запрос</param>
		/// <param name="parameters"> </param>
		/// <returns>Количество затронутых строк</returns>
		public int ExecuteNonQuery(string sql, params object[] parameters)
		{
			beforeExec(); 

			int result;
			try
			{
				using (var command = GetSqlCmd(sql, parameters))
				{
					result = command.ExecuteNonQuery();
				}
			}
				catch(SqlException sqlException)
				{
					throw;
				}
			catch (Exception exception)
			{
				throw new Exception(getErrorMsg(sql), exception);
			}
			
			afterExec(); 
			return result;
		}

		/// <summary>
		/// Выполнение команды, не возвращающей значений. Удобно при выполнении DDL.
		/// </summary>
		/// <param name="sql">SQL-запрос</param>
		/// <param name="parameters"> </param>
		/// <returns>Количество затронутых строк</returns>
		public int ExecuteNonQuery(string sql, SqlTransaction sqlTransaction, params object[] parameters)
		{
			beforeExec();

			int result;
			try
			{
				using (var command = GetSqlCmd(sql, parameters))
				{
					command.Transaction = sqlTransaction;
					result = command.ExecuteNonQuery();
				}
			}
			catch (Exception exception)
			{
				throw new Exception(getErrorMsg(sql), exception);
			}

			afterExec();
			return result;
		}
		
		/// <summary>
		/// Получение
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public List<T> SelectOneColumn<T>(string sql, params object[] parameters)
		{
			List<T> result = new List<T>();
			beforeExec(); 
			try
			{
				using (SqlCommand command = GetSqlCmd(sql, parameters))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						object value;
						while (reader.Read())
						{
							value = reader.GetValue(0);
							if (!(value is DBNull || value==null))
								result.Add((T)value);
						}
					}
				}
			}
			catch (Exception exception)
			{
				throw new Exception(getErrorMsg(sql), exception);
			}

			afterExec();
			return result;
		}

        //todo: сделать возможность использования интерфейсов
		/// <summary>
		/// Получение списка объектов заданного типа
		/// </summary>
		/// <typeparam name="T">Тип</typeparam>
		/// <param name="sql">Запрос</param>
		/// <param name="parameters">Список параметров</param>
		/// <returns></returns>
		public List<T> Select<T>(string sql, params object[] parameters) where T : class, new()
		{
			beforeExec();
			
			List<T> result = new List<T>();
			if (string.IsNullOrEmpty(sql))
				return result;
			Dictionary<string, PropertyInfo> props = typeof (T).GetProperties().ToDictionary(p => p.Name.ToLower());

			try
			{
				using (SqlCommand command = GetSqlCmd(sql, parameters))
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
			}
			catch (Exception exception)
			{
				throw new Exception(getErrorMsg(sql), exception);
			}

			afterExec();
			
			return result;
		}

        /// <summary>
        /// Получение строк
        /// </summary>
        /// <remarks>
        /// Написал свой вариант, т.к. первый падает
        /// </remarks>
        public List<Dictionary<string, object>> Select2(string sql, params object[] parameters)
        {
            beforeExec();
            var result = new List<Dictionary<string, object>>();

            var cmd = GetSqlCmd(sql, parameters);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int col = 0; col <= reader.FieldCount - 1; col++)
                    {
                        row.Add(reader.GetName(col).ToLowerInvariant(), reader[col]);
                    }

                    result.Add(row);
                }
                reader.Close();
            }
            afterExec();
            return result;
        }

		/// <summary>
		/// Получение первого объекта заданного типа
		/// </summary>
		/// <typeparam name="T">Тип</typeparam>
		/// <param name="sql">Запрос</param>
		/// <param name="parameters">Список параметров</param>
		/// <returns></returns>
		public T SelectFirst<T>(string sql, params object[] parameters) where T : new()
		{
			beforeExec();
			T result = new T();
			Dictionary<string, PropertyInfo> props = typeof (T).GetProperties().ToDictionary(p => p.Name.ToLower());
			try
			{
				using (SqlCommand command = GetSqlCmd(sql, parameters))
				{
					using (SqlDataReader reader=command.ExecuteReader(CommandBehavior.SingleRow))
					{
						if (reader.Read())
						{
							int fieldCount = reader.FieldCount;
							for (int i = 0; i < fieldCount; i++)
							{
								string fieldName = reader.GetName(i).ToLower();
								if (!fieldName.Equals("tstamp", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i)
									&& props.ContainsKey(fieldName)
									&& props[fieldName].CanWrite
									&& props[fieldName].GetSetMethod(/*nonPublic*/ true).IsPublic)
								{
									PropertyInfo p = props[fieldName];
									p.SetValue(result, reader.GetValue(i), null);
								}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				throw new Exception(getErrorMsg(sql), exception);
			}

			afterExec();
			
			return result;
		}

		public List<Dictionary<string, object>> Select(string sql, params object[] parameters)
		{
			beforeExec();

			List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

			try
			{
				using (SqlCommand command = GetSqlCmd(sql, parameters))
				{
					SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
					DataTable table = new DataTable();
					sqlDataAdapter.Fill(table);
					result.AddRange(from DataRow row in table.Rows select row.Table.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName, column => row[column.ColumnName]));
				}
			}
			catch (Exception exception)
			{
				throw new Exception(getErrorMsg(sql), exception);
			}
			
			afterExec();

			return result;
		}
		 
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public Dictionary<string, object> ExecuteReaderRow(string sql, params object[] parameters)
		{
			beforeExec();

			var result = new Dictionary<string, object>();
			try
			{
				using (var command = GetSqlCmd(sql, parameters))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						reader.Read();
						for (int i = 0; i < reader.FieldCount; i++)
						{
							result.Add(reader.GetName(i), reader[i]);
						}
						reader.Close();
					}
				}
			}
			catch (Exception exception)
			{
				throw new Exception(getErrorMsg(sql), exception);
			}
			
			afterExec();
			return result;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Получение команды SQL
		/// </summary>
		/// <param name="sql">
		/// Текст запроса
		/// </param>
		/// <param name="objects">
		/// Параметры запроса
		/// </param>
		/// <returns>
		/// Команда SQL
		/// </returns>
		public SqlCommand GetSqlCmd(string sql, object[] objects)
		{
			var sqlCmd = new SqlCommand();
			var paramCounter = 0;
			if (objects != null)
			{
				foreach (var o in objects)
				{
					var paramName = string.Format("@p{0}", paramCounter);
					var p = new SqlParameter(paramName, o);
					sqlCmd.Parameters.Add(p);
					sql = sql.Replace(string.Format("{{{0}}}", paramCounter), string.Format("@p{0}", paramCounter));
					paramCounter++;
				}
			}

			sqlCmd.CommandText = sql;
			sqlCmd.Connection = this.Connection;
			return sqlCmd;
		}

		private void beforeExec()
		{
			if (Connection.State != ConnectionState.Open)
			{
				Connection.Open();
			}
		}

		private void afterExec()
		{
			if (ConnectionType == ConnectionType.ConnectionPerCommand && Connection.State != ConnectionState.Closed)
			{
				Connection.Close();
			}
		}

		private string getErrorMsg(string sql)
		{
			return string.Format("<br/><b>Прозошла ошибка выполнения комадны:</b> <br/>{0}<br/>", sql);
		}
		#endregion

		#region Implements IDisposable

	    /// <summary>
	    /// Закрыть соединение, если оно не закрыто
	    /// </summary>
	    /// <filterpriority>2</filterpriority>
	    public void Dispose()
		{
			if (Connection.State != ConnectionState.Closed)
			{
				Connection.Close();
			}
		}

		#endregion
	}
}
