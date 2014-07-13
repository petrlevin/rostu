using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using Platform.BusinessLogic;
using Platform.Dal;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp
{
	/// <summary>
	/// Класс описывающий расширения для DbSet
	/// </summary>
	public static class DbSetExtensions
	{
		/// <summary>
		/// Строит более оптимальный запрос, по сравнению со стандартным Any().
		/// Понимает пока только простые условия, типа IdOwner==1.
		/// Использовать только в виде context.Entity.FastAny(a=> a.IdOwner==1). 
		/// Конструкции вида context.Entity.Where(условие).FastAny(условие) не понимает.</summary>
		/// <typeparam name="T">Тип сущности</typeparam>
		/// <param name="dbSet">Сущность</param>
		/// <param name="predicate">Условие</param>
		/// <returns></returns>
		public static bool FastAny<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate) where T : class
		{
			IEntity entity = typeof(T).GetEntity();
			string condition = predicate.ToTsql();
			object result =
				_execSqlCommand(string.Format("SELECT TOP 1 1 FROM [{0}].[{1}] WHERE {2}", entity.Schema, entity.Name, condition));
			if (result == null)
				return false;
			return Convert.ToBoolean(result);
		}

		/// <summary>
		/// Выполнение SqlCommand.ExecuteScalar
		/// </summary>
		/// <param name="textCommand">Текст команды</param>
		/// <returns></returns>
		private static object _execSqlCommand(string textCommand)
		{
			var connectionString = DbConnectionString.Get();
			object result = new object();
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommandFactory(textCommand, connection).CreateCommand())
				{
					result = command.ExecuteScalar();
				}
				connection.Close();
			}
			return result;
		}

		/// <summary>
		/// Вставка данных в таблицы использую типизированные переменные
		/// </summary>
		/// <typeparam name="T">Тип сущности</typeparam>
		/// <param name="dbSet">Сущность</param>
		/// <param name="items">Вставляемые элементы</param>
		/// <param name="context">Контекст</param>
		public static void InsertAsTableValue<T>(this DbSet<T> dbSet, IEnumerable<T> items, DbContext context) where T : class
		{
			IEntity entity = typeof(T).GetEntity();
			List<string> fields = _getListFields(entity, context);
			DataTable dataTable = _createDataTable(entity, fields, context);
			items._fillDataTable(dataTable);
			using (SqlCommand command = new SqlCommandFactory(context.Database.Connection).CreateCommand() )
			{
				if (command.Connection.State == ConnectionState.Closed)
					command.Connection.Open();
				string columns = string.Join(",", fields.Select(a => "[" + a + "]"));
				command.CommandText = string.Format("INSERT INTO [{0}].[{1}] ({2}) SELECT {2} FROM @tmp", entity.Schema, entity.Name,
													columns);
				SqlParameter param = command.Parameters.AddWithValue("@tmp", dataTable);
				param.SqlDbType = SqlDbType.Structured;
				param.TypeName = "gen." + entity.Name;
				command.ExecuteNonQuery();
				command.Connection.Close();
			}
		}

		/// <summary>
		/// Заполнение DataTable данными
		/// </summary>
		/// <typeparam name="T">Тип сущности</typeparam>
		/// <param name="items">Элементы для заполнения</param>
		/// <param name="table">Таблица</param>
		private static void _fillDataTable<T>(this IEnumerable<T> items, DataTable table)
		{
			Dictionary<string, PropertyInfo> infos = typeof(T).GetProperties().ToDictionary(a => a.Name.ToLower(), b => b);
			PropertyInfo info;
			foreach (T item in items)
			{
				DataRow row = table.NewRow();
				foreach (DataColumn column in table.Columns)
				{
					string columnName = column.ColumnName.ToLower();
					if (!infos.TryGetValue(columnName, out info))
						throw new ArgumentNullException(column.ColumnName,
														string.Format("У типа ({0}) не найдено поле {1}",
																	  typeof(T).Name, column.ColumnName));

					if (info.PropertyType == typeof(System.Xml.Linq.XElement))
						row[column.ColumnName] =
							new SqlXml(
								new XmlTextReader(
									new StringReader((info.GetValue(item, null) as System.Xml.Linq.XElement).ToString())));
					else
						row[column.ColumnName] = info.GetValue(item, null) ?? DBNull.Value;
				}
				table.Rows.Add(row);
			}
		}

		/// <summary>
		/// Получение списка полей таблицы в нужном порядке
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="context">Контекст</param>
		/// <returns></returns>
		private static List<string> _getListFields(IEntity entity, DbContext context)
		{
			List<string> result = new List<string>();
			using (SqlCommand command = new SqlCommandFactory(context.Database.Connection).CreateCommand())
			{
				if (command.Connection.State == ConnectionState.Closed)
					command.Connection.Open();
				command.CommandText = "select name from sys.columns where object_id=OBJECT_ID(N'" + entity.Schema + "." +
									  entity.Name + "') and is_computed=0 and is_identity=0 and name<>'tstamp' order by column_id";
				using (SqlDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetString(0));
					}
				}
				command.Connection.Close();
			}
			return result;
		}

		/// <summary>
		/// Создание пустой DataTable со структурой, соответствующей Сущности
		/// </summary>
		/// <param name="entity">Сущность</param>
		/// <param name="fields">Спиоск полей</param>
		/// <param name="context">Контекст</param>
		/// <returns></returns>
		private static DataTable _createDataTable(IEntity entity, IEnumerable<string> fields, DbContext context)
		{
			DataTable result = new DataTable(string.Format("{0}.{1}", entity.Schema, entity.Name));
			using ( SqlCommand command = new SqlCommandFactory(context.Database.Connection).CreateCommand() )
			{
				command.CommandText = string.Format("SELECT TOP 0 {0} FROM [{1}].[{2}]",
													string.Join(",", fields.Select(a => "[" + a + "]")), entity.Schema, entity.Name);
				SqlDataAdapter adapter = new SqlDataAdapter(command);
				adapter.Fill(result);
			}
			return result;
		}
	}
}