using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using Platform.PrimaryEntities.Factoring;

namespace Tools.MigrationHelper.Helpers
{
	/// <summary>
	/// Преобразование коллекций в DataTable и обратно
	/// http://lozanotek.com/blog/archive/2007/05/09/Converting_Custom_Collections_To_and_From_DataTable.aspx
	/// </summary>
	public class CollectionHelper
	{
		private CollectionHelper()
		{
		}

		public static DataTable ConvertTo<T>(IList<T> list)
		{
			DataTable table = CreateTable<T>();
			Type entityType = typeof(T);
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

			foreach (T item in list)
			{
				DataRow row = table.NewRow();

				foreach (PropertyDescriptor prop in properties)
				{
					row[prop.Name] = prop.GetValue(item);
				}

				table.Rows.Add(row);
			}

			return table;
		}

		public static IList<T> ConvertTo<T>(IList<DataRow> rows)
		{
			IList<T> list = null;

			if (rows != null)
			{
				list = new List<T>();

				foreach (DataRow row in rows)
				{
					T item = CreateItem<T>(row);
					list.Add(item);
				}
			}

			return list;
		}

		public static IList<T> ConvertTo<T>(DataTable table)
		{
			if (table == null)
			{
				return null;
			}

			List<DataRow> rows = new List<DataRow>();

			foreach (DataRow row in table.Rows)
			{
				rows.Add(row);
			}

			return ConvertTo<T>(rows);
		}

		public static T CreateItem<T>(DataRow row)
		{
			T obj = default(T);
			if (row != null)
			{
				obj = (T)Activator.CreateInstance(typeof(T), true);

				foreach (DataColumn column in row.Table.Columns)
				{
					var props = obj.GetType().GetProperties().ToList();
					var prop = props.FirstOrDefault(w => w.Name.ToLower() == column.ColumnName.ToLower());
					
					if(prop != null)
					try
					{
						object value = row[column.ColumnName];
						if(!string.IsNullOrEmpty(value.ToString()))
							prop.SetValue(obj, value, null);
						else
							prop.SetValue(obj, null, null);
					}
					catch(Exception ex)
					{
						throw new Exception("Ошибка преобразования DataRow To Object: " + ex.Message);
					}
				}
			}

			return obj;
		}

		public static DataTable CreateTable<T>()
		{
			Type entityType = typeof(T);
			DataTable table = new DataTable(entityType.Name);
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);

			foreach (PropertyDescriptor prop in properties)
			{
				table.Columns.Add(prop.Name, prop.PropertyType);
			}

			return table;
		}
	}
}
