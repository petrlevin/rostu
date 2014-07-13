using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Excel;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.DbEnums;

namespace BaseApp.Import
{
	/// <summary>
	/// Класс, единственный метод которого FillDataSetFromFile заполняет DataSet из xls файла.
	/// </summary>
	class DataSetFiller
	{
		private DataSet _dataSet;
	    private DataTable Table;
		private TemplateImportXLS _importTemplate;
		private int _headersPosition;
		private Dictionary<string, object> _clientValues;

		/// <summary>
		/// Заполнение датасета данными из файла
		/// </summary>
		/// <param name="fileName">Имя файла для чтения</param>
		/// <param name="importTemplate">Темплейт для импорта данных</param>
		/// <param name="clientValues"></param>
		/// <param name="ignored"></param>
		/// <returns>Датасет с данными из файла</returns>
        public DataTable FillDataSetFromFile(string fileName, TemplateImportXLS importTemplate, Dictionary<string, object> clientValues, int ignored, string fileType)
		{
			_importTemplate = importTemplate;
			_headersPosition = ignored;
			_clientValues = clientValues;

            FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);

		    IExcelDataReader excelReader = null;
		    switch (fileType)
		    {
                case "xls": excelReader = ExcelReaderFactory.CreateBinaryReader(stream); break;
                case "xlsx": excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream); break;
                default : throw new Exception("Данный тип файлов не поддерживается!");
		    }

            excelReader.IsFirstRowAsColumnNames = true;
            _dataSet = excelReader.AsDataSet();

		    if (_dataSet == null)
		        throw new Exception("Ошибка загрузки файла! Попробуйте другой файл или пересохраните файл с помощью Excel в формате xls или xlsx.");

		    Table = _dataSet.Tables[0];

			SetColumnTypes();
			ExtractPureData();
			SetColumnNames();
			ApplyRegexAndPredefinedValues();

			return Table;
		}


		/// <summary>
		/// Устанавливает тип колонок.
		/// </summary>
		private void SetColumnTypes()
		{
//            DataTable dtCloned = Table.Clone();
            
			foreach (var mapping in _importTemplate.FieldsMap.Where(a => !string.IsNullOrEmpty(a.NameColumn)))
			{
				var fixedName = mapping.NameColumn.Replace('.', '#');
				if (Table.Columns[fixedName] != null)
				{
                    SetColumnType(mapping, Table.Columns[fixedName]);
				}
			}

//            foreach (DataRow row in Table.Rows)
//            {
//                dtCloned.ImportRow(row);
//            }
//		    Table = dtCloned;
		}

		/// <summary>
		/// Устанавливает тип колонки.
		/// </summary>
		/// <param name="mapping"></param>
		/// <param name="col"></param>
		private void SetColumnType(Platform.BusinessLogic.Tablepart.TemplateImportXLS_FieldsMap mapping, DataColumn col)
		{
		    var field = mapping.EntityField;
			// Если для колонки заданы регулярные выражения, изменяем ее тип на строковый
		    if (!string.IsNullOrEmpty(mapping.MaskFinding) && !string.IsNullOrEmpty(mapping.MaskReplacing))
		    {
		        col.DataType = typeof (string);
		    }
//		    if (field.EntityFieldType == EntityFieldType.Money || field.EntityFieldType == EntityFieldType.Numeric)
//		    {
//                col.DataType = typeof(decimal);
//		    }
		}

	    /// <summary>
		/// Устанавливает имя колонки такое же как имя поля справочника.
		/// Добавляет колонки для полей с предопределенным значением.
		/// </summary>
		private void SetColumnNames()
		{
			var errors = new StringBuilder();
			foreach (var mapping in _importTemplate.FieldsMap)
			{
				// в шаблоне импорта для поля задано имя столбца в xls-файле
				if (!string.IsNullOrEmpty(mapping.NameColumn))
				{
					var oldColName = mapping.NameColumn;//.Replace('.', '#');
					if (Table.Columns[oldColName] == null && !_clientValues.ContainsKey(mapping.EntityField.Name))
					{
						errors.AppendLine(string.Format("Не найдена колонка с именем \"{0}\" для поля сущности \"{1}\"</br>", oldColName, mapping.EntityField.Caption));
					}
					else if (!_clientValues.ContainsKey(mapping.EntityField.Name))
					{
						var col = Table.Columns[oldColName];
						var tmp = col.Caption;
						col.ColumnName = mapping.EntityField.Name;
						col.Caption = tmp;
					}
				}
				// задано предопределенное значение для поля
				else if (!string.IsNullOrEmpty(mapping.ValueColumn))
				{
					// добавляем колонку в таблице и устанавливаем у нее имя, соответствующее полю справочника.
					if (Table.Columns[mapping.ValueColumn] == null)
					{
						Table.Columns.Add(mapping.ValueColumn);
					}
					var col = Table.Columns[mapping.ValueColumn];
					var tmp = col.Caption;
					col.ColumnName = mapping.EntityField.Name;
					col.Caption = tmp;
				}
			}

			if (errors.Length > 0)
			{
				throw new Exception(string.Format("В шаблоне описаны колонки, которые отсутствуют в исходном файле: </br>{0}", errors));
			}
		}

		/// <summary>
		/// Строки, являющиеся заголовками, удаляются из таблицы. Значения заголовков сохраняются в свойствах колонок. 
		/// </summary>
		private void ExtractPureData()
		{
			if (_headersPosition == 0)
				return;

			// Указываем для колонок таблицы заголовки
			var h = GetHeaders(_headersPosition, Table);
			foreach (var c in h)
			{
				if (!string.IsNullOrEmpty(c))
				{
					var column = Table.Columns[h.IndexOf(c)];
					column.Caption = column.ColumnName = c;
				}
			}

			// удаляем строки, являющиеся заголовками
			for (int i = _headersPosition; i > 0; i--)
			{
				Table.Rows.RemoveAt(i - 1);
			}
		}

		private void ApplyRegexAndPredefinedValues()
		{
			//var ruru = new CultureInfo("ru-ru");
			foreach (DataRow dataRow in Table.Rows)
			{
				foreach (var mapping in _importTemplate.FieldsMap)
				{
					if (!string.IsNullOrEmpty(mapping.MaskFinding) && !string.IsNullOrEmpty(mapping.MaskReplacing))
					{
						var refFieldName = mapping.EntityField.Name;
						dataRow[refFieldName] = Regex.Replace(dataRow[refFieldName].ToString().Trim(), mapping.MaskFinding, mapping.MaskReplacing);
					}
					if (!string.IsNullOrEmpty(mapping.ValueColumn))
					{
						dataRow[mapping.EntityField.Name] = mapping.ValueColumn.Trim();
					}
					else if (mapping.EntityField.EntityFieldType == EntityFieldType.Bool)
					{
						var value = dataRow[mapping.EntityField.Name].ToString().ToLower();
						if (value.Contains("ложь"))
							value = "false";
						else if (value.Contains("истина"))
							value = "true";
						dataRow[mapping.EntityField.Name] = value;
					}
				}
			}
		}


		#region Вспомогательные функции, не меняющие состояние объекта
		/// <summary>
		/// Получает заголовки колонок
		/// </summary>
		/// <param name="headersPosition"></param>
		/// <param name="rawSheet"></param>
		/// <returns></returns>
		private static IList<string> GetHeaders(int headersPosition, DataTable rawSheet)
		{
			return (from DataColumn item in rawSheet.Columns select headersPosition == 0 ? item.Caption : rawSheet.Rows[headersPosition - 1][item].ToString()).ToList();
		}

		#endregion

	}



}
