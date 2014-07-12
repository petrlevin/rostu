using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Platform.PrimaryEntities.DbEnums;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.EnumsProcessing
{
	/// <summary>
	/// Класс реализующий извлечение информации о всех перечислениях (enum) через Reflection.
	/// В xml, хранящиеся в DbEnums данный класс не лезет.
	/// </summary>
    [System.Obsolete("use class EnumsParser")]
	public class EnumsFetcher
	{
		private class InnerObject
		{
			public string Caption;
			public string Description;
		}

		#region Public Members
		/// <summary>
		/// Считывает Перечисления из файловой системы и возвращает list DataTable
		/// </summary>
		/// <param name="path">Путь до проекта</param>
		public List<DataTable> GetEnums(string path)
		{
			return GetEnums(path, null, true, null);
		}

		/// <summary>
		/// Считывает Перечисления из файловой системы и возвращает list DataTable
		/// </summary>
		/// <param name="path">Путь до проекта</param>
		/// <param name="namesEnums">Наименование необходимых енумераторов</param>
		public List<DataTable> GetEnums(string path, string[] namesEnums)
		{
			return GetEnums(path, namesEnums, true, null);
		}

		/// <summary>
		/// Считывает Перечисления из файловой системы и возвращает list DataTable без данных по первичным перечислениям
		/// </summary>
		/// <param name="path">Путь до проекта</param>
		public List<DataTable> GetEnumsWithoutPrimary(string path)
		{
			return GetEnums(path, null, true, Names.PrimaryEnums);
		}

		/// <summary>
		/// Считывает Перечисления из файловой системы и возвращает list DataTable
		/// </summary>
		/// <param name="path">Путь до проекта</param>
		/// <param name="namesEnums">Наименование необходимых енумераторов</param>
		/// <param name="entityTables">Заполнение таблицы сущности</param>
		/// <param name="ingnoredEnums">Наименование енумераторов которые игнорируются</param>
		/// <returns></returns>
		public List<DataTable> GetEnums(string path, string[] namesEnums, bool entityTables, string[] ingnoredEnums)
		{
			var dataTables = new List<DataTable>();
			
			// получаем все типы проекта с базовым типом enum
			var types = GetTypes(path);

			//фильтр по наименованию
			if (namesEnums != null && namesEnums.Any())
				types = types.Where(w => namesEnums.Contains(w.Name)).ToList();

			//фильтр по наименованию
			if (ingnoredEnums != null && ingnoredEnums.Any())
				types = types.Where(w => !ingnoredEnums.Contains(w.Name)).ToList();

			var entityTable = GetEntityTable();

			//формируем сущности
			foreach (var type in types)
			{
				//получаем поля типа
				var fields = type.GetFields();
				// создаем табличку
				var table = GetEnumTable(type.Name);
				foreach (var field in fields)
				{
					// пропускаем не нужные нам поля
					if (field.Name.Equals("value__")) continue;
					// получаем из xml наименование и описание
					XmlElement documentation = DocsByReflection.XmlFromMember(type.GetMember(field.Name)[0]);
					var inObj = InnerText(documentation != null ? documentation.ChildNodes[0].InnerText : "");
					// заполняем табличку
					DataRow newRow = table.NewRow();

					newRow[Names.Id] = field.GetRawConstantValue();
					newRow[Names.Name] = field.Name;
					newRow[Names.Caption] = inObj.Caption;
					newRow[Names.Description] = inObj.Description;

					table.Rows.Add(newRow);
				}
				// добавляем в датасет
				dataTables.Add(table);
				// если надо заполняем таблицы Entity
				if (entityTables)
				{
					XmlElement documentation = DocsByReflection.XMLFromType(type);
					var inObj = InnerText(documentation != null ? documentation.ChildNodes[0].InnerText : "");
					var projectName = type.Assembly.GetName().Name.Replace('.','_');

					DataRow newRow = entityTable.NewRow();

					newRow[Names.Name] = type.Name;
					newRow[Names.Caption] = inObj.Caption ?? type.Name;
					newRow[Names.Description] = inObj.Description;
					newRow[Names.IdEntityType] = 1;
					newRow[Names.IdProject] = Enum.Parse(typeof (SolutionProject), projectName);

					entityTable.Rows.Add(newRow);
				}
			}

			if(entityTable.Rows.Count > 0)
				dataTables.Add(entityTable);

			#if DEBUG
			Debug.WriteLine("Таблицы перечислений:");
			Debug.WriteLine("---------------------");
			foreach (var dataTable in dataTables.OrderBy(t => t.TableName))
			{
				Debug.WriteLine(dataTable.TableName);
			}
			Debug.WriteLine("Содержание таблицы entityTable:");
			Debug.WriteLine("---------------------");
			if (dataTables.SingleOrDefault(t => t.TableName == Names.RefEntity) != null)
			foreach (DataRow row in dataTables.Single(t => t.TableName == Names.RefEntity).Rows.Cast<DataRow>().OrderBy(r => r[Names.Name]))
			{
				Debug.WriteLine(row[Names.Name]);
			}
			#endif

			return dataTables;
		}

		/// <summary>
		/// Возвращает DataSet с данными первичных перечислений
		/// </summary>
		/// <param name="path">Путь до папки проекта</param>
		/// <returns></returns>
		public DataSet GetPrimaryEnums(string path)
		{
			var dataSet = new DataSet();
			dataSet.Tables.AddRange(GetEnums(path, Names.PrimaryEnums, false, null).ToArray());
			dataSet.AcceptChanges();
			return dataSet;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Возвращает коллекцию типа Type c базовым типом enum
		/// </summary>
		/// <param name="path">Путь до проекта</param>
		/// <returns></returns>
		private List<Type> GetTypes(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new Exception("Не указан путь до проекта");

			var types = new List<Type>();
			//получаем дочерние каталоги проекта без тестов
			var listDirectories = Directory.EnumerateDirectories(path).Where(w=>!w.EndsWith("Tests")).ToList();
			//Добавляем сам путь, в случае если указан один проект
			listDirectories.Add(path);

			Dictionary<string,string> dirWithDLL = new Dictionary<string, string>();
			foreach (var listDirectory in listDirectories)
			{
				if (Directory.Exists(Path.Combine(listDirectory, @"bin\Debug")))
				{
					dirWithDLL.Add(listDirectory,Path.Combine(listDirectory, @"bin\Debug"));
				}
				else if (Directory.Exists(Path.Combine(listDirectory, @"bin\Release")))
					dirWithDLL.Add(listDirectory,Path.Combine(listDirectory, @"bin\Release"));
			}


			if(!dirWithDLL.Any())
				throw new Exception("Не найдено ни одной директории со сборками");

			//находим и подгружаем сборки
			foreach (var assembly in dirWithDLL.Select(listDirectory => Directory.GetFiles(listDirectory.Value, new DirectoryInfo(listDirectory.Key).Name + ".dll")).SelectMany(files => files.Select(Assembly.LoadFile)))
			{
				try
				{
					types.AddRange(assembly.GetTypes().Where(w => w.BaseType == typeof(Enum) && (w.Namespace != null && w.Namespace.EndsWith("DbEnums"))));
				}
				catch (ReflectionTypeLoadException ex)
				{
					types.AddRange(ex.Types.Where(w => w != null && w.BaseType == typeof(Enum) && (w.Namespace != null && w.Namespace.EndsWith("Enums"))));
				}
			}
			return types;
		}

		private DataTable GetEntityTable()
		{
			// Create table.
			var table = new DataTable {TableName = Names.RefEntity};

			// Create columns.
			table.Columns.Add(Names.Name, typeof(string));
			table.Columns.Add(Names.Caption, typeof(string));
			table.Columns.Add(Names.Description, typeof(string));
			table.Columns.Add(Names.IdEntityType, typeof(byte));
			table.Columns.Add(Names.IdProject, typeof(int));

			table.AcceptChanges();
			return table;
		}

		private DataTable GetEnumTable(string tableName)
		{
			// Create table.
			var table = new DataTable {TableName = "enm." + tableName};

			// Create columns.
			DataColumn idColumn = table.Columns.Add(Names.Id, typeof(int));
			table.Columns.Add(Names.Name, typeof(string));
			table.Columns.Add(Names.Caption, typeof(string));
			table.Columns.Add(Names.Description, typeof(string));

			// Set the ID column as the primary key column.
			table.PrimaryKey = new[] { idColumn };
			
			table.AcceptChanges();
			return table;
		}

		/// <summary>
		/// Разбор текста
		/// </summary>
		/// <param name="innerText"></param>
		/// <returns></returns>
		private InnerObject InnerText(string innerText)
		{
			var inObj = new InnerObject();
			// Разделяем на 2 строки по переносу каретки
			string[] strings = innerText.Split(new[] { '\r', '\n' }, 2, StringSplitOptions.RemoveEmptyEntries);

			if (strings.Any())
			{
				var str = strings[0].Trim();
				inObj.Caption = string.IsNullOrWhiteSpace(str) ? null : str ;
				if (strings.Count() > 1)
				{
					inObj.Description = strings[1].Trim();
				}
			}
			else
			{
				inObj.Caption = null;
				inObj.Description = null;
			}

			return inObj;
		}

		#endregion
	}
}
