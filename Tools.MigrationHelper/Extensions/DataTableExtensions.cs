﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Extensions
{
	public static class DataTableExtensions
	{
		/// <summary>
		/// Добавляет в таблицу копии переданных строк.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="rows"></param>
		public static void CloneAndAddRows(this DataTable table, DataRow[] rows)
		{
			foreach (DataRow row in rows)
			{
				var newRow = table.NewRow();
				newRow.ItemArray = row.ItemArray.Clone() as object[];
				table.Rows.Add(newRow);
			}
		}

		/// <summary>
		/// Запись DataTable в xml с созданием пути
		/// </summary>
		/// <param name="table">DataTable</param>
		/// <param name="path">Путь куда записываем DataTable. Пример: C:\example.xml</param>
		/// <param name="mode">Указывает, как записывать XML-данные</param>
		public static void WriteXmlToFile(this DataTable table, string path, XmlWriteMode mode)
		{
			var file = new FileInfo(path);
			if (!Directory.Exists(file.Directory.FullName))
				Directory.CreateDirectory(file.Directory.FullName);
			table.WriteXml(path, mode);
		}
	}
}
