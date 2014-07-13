using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using BaseApp.Rights;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.DbEnums;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common;
using Platform.Dal;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;
using IsolationLevel = System.Transactions.IsolationLevel;
using TransactionScope = Platform.BusinessLogic.DataAccess.TransactionScope;

namespace BaseApp.Import
{
	/// <summary>
	/// 
	/// </summary>
	public class Import
	{
		private readonly TemplateImportXLS _template;
		private readonly byte[] _file;
		private readonly int? _idOwner;
		private readonly FieldValues _dependencies;
		private readonly int _ignor;
		private readonly DataManager _dataManager;

		/// <summary>
		/// Сущность в которую загружаем 
		/// </summary>
		private Entity Entity
		{
			get { return _template.Entity; }
		}

        private readonly List<string> _sysDimention = new List<string>() { "idPublicLegalFormation", "idBudget", "idVersion" };
        private readonly SysDimensionsState _sysDimensionsState = IoC.Resolve<SysDimensionsState>("CurentDimensions");

		/// <summary>
		/// Набор данных прочитанных из файла
		/// </summary>
		private readonly DataTable _table;

        private DataTable GetDataTable(string fileType)
		{
			var fileName = Path.GetTempFileName();
			ByteArrayToFile(fileName, _file);
			var filler = new DataSetFiller();
            return filler.FillDataSetFromFile(fileName, _template, CustomFieldValues, _ignor, fileType);
		}

		private readonly Platform.BusinessLogic.DataContext _dataContext;
	    private string _filetype;

	    /// <summary>
	    /// конструктор
	    /// </summary>
	    /// <param name="file"></param>
	    /// <param name="template"></param>
	    /// <param name="idOwner"></param>
	    /// <param name="dependencies"></param>
	    /// <param name="ignor"></param>
	    /// <param name="dataManager"></param>
	    /// <param name="fileType"></param>
	    public Import(byte[] file, TemplateImportXLS template, int? idOwner, FieldValues dependencies, int ignor, DataManager dataManager, string fileType)
		{
			_template = template;
			_file = file;
			_idOwner = idOwner;
			_dependencies = dependencies;
			_ignor = ignor;
			_dataManager = dataManager;
            _dataContext = IoC.Resolve<DbContext>().Cast<Platform.BusinessLogic.DataContext>();
		    _filetype = fileType;
            _table = GetDataTable(fileType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string Process()
		{
			var sb = new StringBuilder();

			var fields = _template.FieldsMap.Select(a => a.EntityField).ToList();
			var fieldsDictionary = fields.ToDictionary(a => a.Name);
			var linkFields = _template.FieldsMap
				.Where(mapping => mapping.EntityField.IdEntityLink != null)
				.ToDictionary(mapping => mapping.EntityField.Name, mapping => (int)mapping.EntityField.IdEntityLink);

			var parentFields = Entity.Fields.Where(w => w.IdEntityLink == Entity.Id).ToList();

			Dictionary<string, List<string>> nonDistinctCaptions;
			Dictionary<string, List<string>> notFoundCaptions;

			// Словарь соответствия кэпшенов элементам справочника
			var foundCaptions = GetCaptions(linkFields, fieldsDictionary, out nonDistinctCaptions, out notFoundCaptions);
			if (notFoundCaptions.Count > 0)
			{
                sb.AppendLine("<b>Для некоторых импортируемых значений отсутствуют соответствующие элементы справочника.</b></br>");
				foreach (var item in notFoundCaptions)
				{
					sb.AppendLine(string.Format("<b>Колонка:</b> {0}; <b>значения:</b> {1}</br>", item.Key, item.Value.ToString(", ")));
				}
			}
			if (nonDistinctCaptions.Count > 0 && _template.ExecImportMode != ExecImportMode.UseFirstElementOfTheSame) // Использовать первый элемент из набора с одинаковым наименованием
			{
                sb.AppendLine("</br><b>Для некоторых импортируемых значений существует более одного элемента справочника с таким наименованием.</b></br>");
				foreach (var item in nonDistinctCaptions)
				{
					sb.AppendLine(string.Format("<b>Колонка:</b> {0}; <b>значения:</b> {1}</br>", item.Key, item.Value.ToString(", ")));
				}
			}

			if (sb.Length == 0)
			{
				var lst = GetDictionaryLoadList(fields, foundCaptions);
				var hasNotFound = CheckNotFound(fieldsDictionary, foundCaptions, sb, parentFields);

				if (!hasNotFound)
				{
					LoadData(lst, sb, parentFields);
				}
			}

			return sb.ToString().Replace(@"\0xd\0xa", String.Empty).Replace(">", "&gt;").Replace("<", "&lt;");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="byteArray"></param>
		/// <returns></returns>
		public bool ByteArrayToFile(string fileName, byte[] byteArray)
		{
			try
			{
				// Open file for reading
				var fileStream =
					new FileStream(fileName, FileMode.Create,
					               FileAccess.Write);
				// Writes a block of bytes to this stream using data from
				// a byte array.
				fileStream.Write(byteArray, 0, byteArray.Length);

				// close file stream
				fileStream.Close();

				return true;
			}
			catch (Exception)
			{
				// Error
			}

			// error occured, return false
			return false;
		}

		/// <summary>
		/// Значения для idOwner и idParent
		/// Возвращает словарь "имя поля" -> "значение", в котором заполняет значение для поля Владелец и/или Родитель, 
		/// если таковые значения были переданы в параметрах запроса к Import.aspx. Словарь будет использован при импорте, 
		/// для выставления значений полей Владелец и/или Родитель справочника, в который производится импорт данных.
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, object> CustomFieldValues
		{
			get
			{
				var customFieldValues = new Dictionary<string, object>();
				if (_idOwner != null)
					customFieldValues.Add("idOwner", _idOwner);

				// Далее определяется есть ли у сущности, для которой выполняется импорт, ссылка на мастер-сущность. 
				// Например: Есть 2 ТЧ: master и details. ТЧ с детальными данными всегда имеет ссылку на мастер-ТЧ. 
				// Если в качестве мастер-таблицы используется мультиссылка, то детальная ТЧ будет ссылаться на справочник, 
				// на который ссылается мастер-мультиссылка.

				if (_dependencies != null)
				{
					foreach (var fieldDepend in _dependencies)
					{
                        //Поле ссылка на сущность от которой зависит
						var field = _dataContext.EntityField.SingleOrDefault(s => s.Id == fieldDepend.Key);
                        //Имя поля данной сущности которое ссылается на другую сущность
					    var name = Entity.Fields.First(w => w.IdEntityLink == field.IdEntityLink).Name;
						if (field != null)
						{
                            customFieldValues.Add(name, fieldDepend.Value);
						}
					}
				}

				return customFieldValues;
			}
		}

		/// <summary>
		/// Проверяем наличие кэпшенов для всех элементов
		/// </summary>
		/// <param name="linkFields">Словарь ссылочных полей. 
		/// Ключ - имя поля (EntityField.Name), значение - id сущности (EntityField.idEntityFieldLink), на которую ссылается данное поле.</param>
		/// <param name="fldsDic">Словарь всех полей</param>
		/// <param name="nonDistinctCaptions">имя колонки => список неуникальных наименований</param>
		/// <param name="notFoundCaptions">импортируемые значения, не найденные в справочниках.</param>
		/// <returns>Словарь вида (ключ -имя поля) : ( Словарь (ключ-кэпшен:идентификатор))</returns>
		private Dictionary<string, Dictionary<string, int?>> GetCaptions(
			Dictionary<string, int> linkFields,
			Dictionary<string, EntityField> fldsDic,
			out Dictionary<string, List<string>> nonDistinctCaptions,
			out Dictionary<string, List<string>> notFoundCaptions)
		{
			var foundCaptions = new Dictionary<string, Dictionary<string, int?>>();
			// Ключ = имя колонки
			notFoundCaptions = new Dictionary<string, List<string>>();
			nonDistinctCaptions = new Dictionary<string, List<string>>();

			foreach (var column in _table.Columns.Cast<DataColumn>().Where(column => linkFields.ContainsKey(column.ColumnName)))
			{
				List<string> nonDistinctCaptionsList;
				if (fldsDic[column.ColumnName].EntityFieldType == EntityFieldType.Multilink)
				{
					var rows = _table.AsEnumerable().Select(a => a[column].ToString()).Distinct();
					var capsList = new List<string>();
					foreach (var row in rows)
					{
						capsList.AddRange(row.Split(','));
					}

					capsList = capsList.Where(a => a != string.Empty).ToList();
					List<string> notFound;
					var capts = СheckCaptions(capsList.Distinct().ToArray(), linkFields[column.ColumnName], out nonDistinctCaptionsList, out notFound);
					foundCaptions.Add(column.ColumnName, capts);
					if (notFound.Count > 0)
					{
						notFoundCaptions.Add(column.Caption, notFound);
					}
				}
				else
				{
					var notFound = new List<string>();
					var distinctRows =
						_table.AsEnumerable().Select(a => a[column].ToString()).Where(a => a != string.Empty).Distinct().ToArray();
					var capts = СheckCaptions(distinctRows, linkFields[column.ColumnName], out nonDistinctCaptionsList, out notFound);
					foundCaptions.Add(column.ColumnName, capts);
					if (notFound.Count > 0)
					{
						notFoundCaptions.Add(column.Caption, notFound);
					}
				}

				if (nonDistinctCaptionsList.Count > 0)
				{
					nonDistinctCaptions.Add(column.Caption, nonDistinctCaptionsList);
				}
			}

			return foundCaptions;
		}

		///<summary>
		/// Проверка наличия кэпшенов для сущности
		///</summary>
		///<param name="captions">Массив с кэпшенами. Массив содежит уникальные значения из одной колонки исходного файла (откуда осуществляется импорт).</param>
		///<param name="idEntity">Идентификатор сущности, на которую ссылается поле, соответствующее колонке, из которой мы имеем массив уникальных значений в @captions</param>
		///<param name="nonDistinctCaptions">Список наименований элементов справочника, соответствующий тем импортируемым значениям, для которых в справочнике сущесвует более одного элемента с таким наименованием</param>
		///<param name="notFoundInReference">Возвращается список значений, указанных в файле и отсутствующих в справочнике</param>
		///<returns>Словарь кэпшен - идентификатор, в случае если найти элемент по кэпшену не удалось - то идентификатор null</returns>
		private Dictionary<string, int?> СheckCaptions(string[] captions, int idEntity, out List<string> nonDistinctCaptions, out List<string> notFoundInReference)
		{
			Entity entity = _dataContext.Entity.SingleOrDefault(s=> s.Id == idEntity);
			if(entity == null) throw new Exception("Отсутствует сущность на которую ссылается справочник. Id сущности:" + idEntity);

//			if (captions.Count() > 2000)
//			{
//				throw new Exception(
//					string.Format("Файл содержит слишком много ({0}) разных заголовков справочника {1}. Допустимое количество разных заголовков 2000",
//					captions.Count(),
//					entity.Caption));
//			}

			var list = captions.Select(t => new FilterConditions
				{
                    Operator = ComparisionOperator.Equal,
                    Field = entity.CaptionField.Name,
                    Value = t.Replace("\n", "").Replace("\r", "").Trim() 
				}).Cast<IFilterConditions>().ToList();

            var operands = new List<IFilterConditions>();


		    ISelectQueryBuilder selectBuilder = new QueryFactory(entity).Select();
		    {
		        selectBuilder.QueryDecorators.Add(new AddSysDimensionsFilter(null));
		        selectBuilder.Fields = new List<string>(new[] {entity.CaptionField.Name, "id"});


		        var condition = new FilterConditions();
		        if (list.Count > 1)
		        {
		            condition = new FilterConditions
		                            {
		                                Type = LogicOperator.Or,
		                                Operands = list
		                            };
		            operands.Add(condition);
		        }
		        if (list.Count == 1)
		        {
		            condition = list.First() as FilterConditions;
		            operands.Add(condition);
		        }

		        if (operands.Count > 1)
		        {
		            selectBuilder.Conditions = new FilterConditions
		                                           {
		                                               Type = LogicOperator.And,
		                                               Operands = operands
		                                           };
		        }
		        if (operands.Count == 1)
		        {
		            selectBuilder.Conditions = operands.First();
		        }

		        var result = Select(selectBuilder);

		        List<MyClass> caps = result.Select(row => new MyClass
		                                                      {
		                                                          Id = int.Parse(row["id"].ToString()),
		                                                          Caption =
		                                                              row[entity.CaptionField.Name.ToLowerInvariant()]
		                                                              .ToString()
		                                                      }).ToList();

		        //Если поле ссылка на самого себя(иерархия) ищем кэпшены так же и в файле exel
		        if (entity.Id == Entity.Id)
		        {
		            foreach (DataRow row in _table.Rows)
		            {
                        if (!string.IsNullOrEmpty(row[entity.CaptionField.Name].ToString()))
                            if (caps.All(a => a.Caption != row[entity.CaptionField.Name].ToString()))
                                caps.Add(new MyClass
                                    {
                                        Id = null,
                                        Caption = row[entity.CaptionField.Name].ToString()
                                    });
		            }
		        }

		        var groupedCaps =
		            caps.GroupBy(a => a.Caption)
		                .Select(a => new {Caption = a.Key, Id = a.First().Id, Count = a.Count()})
		                .ToList();
		        nonDistinctCaptions = groupedCaps.Where(a => a.Count > 1).Select(a => a.Caption).ToList();
		        var distinctCaps = groupedCaps.Where(a => a.Count == 1).ToDictionary(a => a.Caption, a => a.Id);

		        // Если в колонке исходного файла, соответствующей справочнику idEntity, 
		        // присутствуют значения, которых нет среди наименований элементов этого справочника.
		        // Останавливаем импорт SBOR-9893 (см. *доработка* в описании задачи).
		        notFoundInReference =
		            captions.Where(
		                a =>
		                !distinctCaps.Select(b => b.Key).Contains(a, StringComparer.OrdinalIgnoreCase) &&
		                !groupedCaps.Where(c => c.Count > 1)
		                            .Select(c => c.Caption)
		                            .Contains(a, StringComparer.OrdinalIgnoreCase)).ToList();

		        return distinctCaps;
		    }
		}

		private class MyClass
		{
			public int? Id { get; set; }
			public int EntityId { get; set; }
			public string Caption { get; set; }
		}

	    private FilterConditions GetSysDimentionFilter(Entity entity)
	    {
            var dimentionList = new List<IFilterConditions>();
            foreach (var entityField in entity.Fields)
            {
                if (_sysDimention.Contains(entityField.Name))
                {
                    string value = string.Empty;
                    if (entityField.Name == "idPublicLegalFormation")
                        value = _sysDimensionsState.PublicLegalFormation.Id.ToString();
                    if (entityField.Name == "idBudget")
                        value = _sysDimensionsState.Budget.Id.ToString();
                    if (entityField.Name == "idVersion")
                        value = _sysDimensionsState.Version.Id.ToString();
                    dimentionList.Add(new FilterConditions
                    {
                        Operator = ComparisionOperator.Equal,
                        Field = entityField.Name,
                        Value = value
                    });
                }
            }

            var dimentionCondition = new FilterConditions();

            if (dimentionList.Count > 1)
            {
                dimentionCondition = new FilterConditions
                {
                    Type = LogicOperator.And,
                    Operands = dimentionList
                };
            }
            if (dimentionList.Count == 1)
            {
                dimentionCondition = dimentionList.First() as FilterConditions;
            }

            return dimentionList.Count == 0  ? null : dimentionCondition;
	    }

	    ///  <summary>
		///  Получение перечисления элементов для загрузки в систему
		///  </summary>
		/// <param name="fields">Поля сущности</param>
		/// <param name="captions">Соответсвие кэпшенов</param>
		/// <returns>Перечисление словарей поле:значение</returns>
		private IDictionary<IDictionary<string, object>, IDictionary<string, object>> GetDictionaryLoadList(
			List<EntityField> fields, IDictionary<string, Dictionary<string, int?>> captions)
		{
			var lst = new Dictionary<IDictionary<string, object>, IDictionary<string, object>>();

			foreach (DataRow row in _table.Rows)
			{
				var friendly = new Dictionary<string, object>(CustomFieldValues);
				var systemData = new Dictionary<string, object>(CustomFieldValues);
				var hasValues = false;
				foreach (var field in fields)
				{
					friendly[field.Name] = row[field.Name];

					if (field.EntityFieldType == EntityFieldType.Multilink)
					{
						var idList = new List<object>();
						var items = row[field.Name].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

						if (items.Length > 0)
						{
							idList.AddRange(items.Select(itm => captions[field.Name][itm].ToString()));

							systemData[field.Name] = idList.ToArray();
						}
					}
					else
					{
						if (field.IdEntityLink != null)
						{
                            
						    // если хотим импортировать значение null
							if (field.AllowNull && row[field.Name].NullableToString() == string.Empty)
							{
								systemData[field.Name] = null;
								//hasValues = true;
							}
							else if (captions.ContainsKey(field.Name))
							{
								var captionDict = captions[field.Name];
                                var cap = row[field.Name].ToString().Replace("\n", "").Replace("\r", "").Trim();
                                if (captionDict.ContainsKey(cap))
								{
                                    var value = captionDict[cap];
									if (value == null)//если значение содержиться в словаре, но value пусто, значит что эти значения получим при вставке строк в базу(для новых записей id негде взять(это все для родителей))
									{
                                        systemData[field.Name] = "$%_$" + cap;// $%_$ для того чтоб понимать что это надо подменить в будущем
									}
									else
									{
                                        systemData[field.Name] = captionDict[cap];
									}

									if (row[field.Name].NullableToString() != string.Empty)
										hasValues = true;
								}
							}
						}
						else
						{
                            if (_filetype == "xls" && (field.EntityFieldType == EntityFieldType.DateTime || field.EntityFieldType == EntityFieldType.Date))
                            {
                                try
                                {
                                    systemData[field.Name] = DateTime.FromOADate((double)row[field.Name]);
                                }
                                catch (Exception)
                                {
                                    systemData[field.Name] = row[field.Name] is string ? row[field.Name].ToString().Replace("\n", "").Replace("\r", "").Trim() : row[field.Name];
                                    if (row[field.Name].NullableToString() != string.Empty)
                                        hasValues = true;
                                }
                            }
                            else
                            {
                                if ((field.EntityFieldType == EntityFieldType.Money || field.EntityFieldType == EntityFieldType.Numeric) && row[field.Name].NullableToString() != string.Empty)
                                {
                                    systemData[field.Name] = decimal.Parse(row[field.Name].ToString(), new NumberFormatInfo { NumberDecimalSeparator = "." });
                                }
                                else
                                {
                                    systemData[field.Name] = row[field.Name] is string
                                                                 ? row[field.Name].ToString()
                                                                                  .Replace("\n", "")
                                                                                  .Replace("\r", "")
                                                                                  .Trim()
                                                                 : row[field.Name];
                                }

                                if (row[field.Name].NullableToString() != string.Empty)
                                    hasValues = true;
                            }
						}
					}
				}

				if (systemData.Keys.ToString("", a => a ?? "").Length <= 2)
				{
					continue;
				}

				if (hasValues)
					lst.Add(systemData, friendly);
			}

			return lst;
		}

		/// <summary>
		/// Обработка ненайденных элеметов
		/// </summary>
		/// <param name="fldsDic">Словарь полей сущности</param>
		/// <param name="notFoundCaptions">Список кэпшенов импортируемых элементов</param>
		/// <param name="sb">StringBuilder для записи инфо об ошибки</param>
		/// <param name="parentFields">Список полей иерархии</param>
		/// <returns>Истина если есть элементы которые невозможно импортировать</returns>
		private static bool CheckNotFound(IDictionary<string, EntityField> fldsDic, Dictionary<string, Dictionary<string, int?>> notFoundCaptions, StringBuilder sb, List<IEntityField> parentFields)
		{
			var hasNotFound = false;

			if (notFoundCaptions.Any(a => a.Value.ContainsValue(null) && a.Key != "idParent"))
			{
				sb.Append("<h2>Невозможно продолжить импорт, т.к. имеются следующие ошибки:</h2>");
			}

			foreach (KeyValuePair<string, Dictionary<string, int?>> pair in notFoundCaptions)
			{
				if (pair.Value.Any(a => a.Value == null) && parentFields.All(a=>a.Name != pair.Key))
				{
					hasNotFound = true;
					sb.AppendFormat("Поле <b>{0}({1})</b> содержит следующие элементы не найденные в системе:", fldsDic[pair.Key], pair.Key);
					sb.AppendFormat("<br/> <ul><li>{0}</li></ul>", pair.Value.Where(a => a.Value == null).ToString("</li><li>", a => string.Format("\"{0}\"", a.Key)));
				}
			}

			return hasNotFound;
		}

		/// <summary>
		/// Загрузка данный в систему
		/// </summary>
		/// <param name="itemsList">Список элементов для загрузки</param>
		/// <param name="resultStringBuilder">StringBuilder для вывода результатов загрузки</param>
		/// <param name="parentFields">Список полей иерархии</param>
		private void LoadData(IDictionary<IDictionary<string, object>, IDictionary<string, object>> itemsList, StringBuilder resultStringBuilder, List<IEntityField> parentFields)
		{
//			if (itemsList.Count() > 15000)
//				throw new Exception("Запрещено импортировать более 15000 записей одним файлом!");

			var reslt = LoadItems(
				_template.KeyField.Select(a => a.EntityField).ToList(),
				itemsList.Keys,
				parentFields,
				_template.ImportType,
				true);

			GenerateMsg(resultStringBuilder, reslt, itemsList);
		}

		/// <summary>
		/// Загрузка элементов в справочник
		/// </summary>
		/// <param name="keyFields">
		/// Ключевые поля для контроля существующих элементов
		/// </param>
		/// <param name="loadItems">
		/// Элементы для загрузки
		/// </param>
		/// <param name="parentFields">
		/// Список полей иерархии
		/// </param>
		/// <param name="importType">
		/// Тип импорта
		/// </param>
		/// <param name="continueOnError">
		/// Признак что будут игнорироваться ошибки
		/// </param>
		/// <returns>Информация о том как загрузились элементы</returns>
		private LoadResult LoadItems(
			List<EntityField> keyFields,
			IEnumerable<IDictionary<string, object>> loadItems,
			List<IEntityField> parentFields,
			ImportType importType,
			bool continueOnError)
		{
			var loadRes = new LoadResult { CreateCount = 0, UpdatesCount = 0 };
			var exceptions = new Dictionary<int?, Exception>();
			var errorItems = new Dictionary<int, IDictionary<string, object>>();
			var parentDictionary = new Dictionary<string, int>();
		    var i = 0;
            TransactionScope ts = null;
            if (_template.IsPerformSingleTransaction)
            {
                ts = new TransactionScope(TransactionScopeOption.RequiresNew,
                                          new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }); //, Timeout = new TimeSpan(1, 0, 0)
            }

		    try
		    {
		        if (importType == ImportType.CreateNew || importType == ImportType.CreateNewAndConfirmExist ||
		            importType == ImportType.CreateNewAndUpdateExist)
		            //создание строк
		            foreach (var item in loadItems)
		            {
		                if (exceptions.Count > 0 && _template.IsPerformSingleTransaction)
		                {
		                    break;
		                }

		                bool filter = true;
		                //Строим фильтр для поиска такой же записи в справочнике
		                var filterCollection = new List<IFilterConditions>();
		                //Если табличная часть и поле Владельца не указано в ключевых полях, добавляем фильтр сами
		                if (Entity.EntityType == EntityType.Tablepart && keyFields.All(a => a.Name != "idOwner"))
		                {
		                    filterCollection.Add(new FilterConditions
		                        {
		                            Field = "idOwner",
		                            Value = item["idOwner"],
		                            Operator = ComparisionOperator.Equal
		                        });
		                }
		                //Проходимся по списку ключевых полей и добавляем к фильтру
		                foreach (EntityField field in keyFields)
		                {
		                    if (!item.ContainsKey(field.Name))
		                    {
                                throw new Exception("В файле отсутствует поле '" + field.Caption + "' указаное как ключевое.");
		                    }
		                    var fieldValue = item[field.Name];
		                    fieldValue = field.EntityFieldType == EntityFieldType.String ? fieldValue.ToString() : fieldValue;

		                    if (fieldValue.NullableToString() == string.Empty) filter = false;

		                    filterCollection.Add(new FilterConditions
		                        {
		                            Field = field.Name,
		                            Value = fieldValue,
		                            Operator = ComparisionOperator.Equal
		                        });
		                }

                        //Системные измерения

		                //Если с фильтром все хорошо, делаем запрос
		                List<Dictionary<string, object>> result = null;
		                if (filter)
		                {
		                    ISelectQueryBuilder selectBuilder = new QueryFactory(Entity).Select();
		                    {
                                if (!keyFields.Any(f => f.Name == "idPublicLegalFormation" || f.Name == "idVersion" || f.Name == "idBudget"))
		                            selectBuilder.QueryDecorators.Add(new AddSysDimensionsFilter(null));

		                        if (filterCollection.Count >= 2)
		                        {
		                            selectBuilder.Conditions = new FilterConditions
		                                                           {
		                                                               Type = LogicOperator.And,
		                                                               Operands = filterCollection
		                                                           };
		                        }
		                        if (filterCollection.Count == 1)
		                        {
		                            selectBuilder.Conditions = filterCollection.First();
		                        }
		                        result = Select(selectBuilder);
		                    }
		                }
		                // Новым элемент считаем, если не указано ни одно ключевое поле или результат запроса пусто
		                var itemIsNew = !keyFields.Any() || result == null || !result.Any();

		                // Если элемент не новый пропускаем
		                if (!itemIsNew) continue;

		                // Если у сущности подрузомевается статус а поля нет, присваиваем статус новый
		                if (Entity.IsRefernceWithStatus())
		                {
		                    if (!item.ContainsKey("idRefStatus"))
		                    {
		                        item.Add("idRefStatus", RefStatus.New);
		                    }
		                }

                        //Проверям что все поля родитель имеют значения
		                bool allFieldsHaveValue = true;
                        foreach (var field in parentFields)
                        {
                            if (!item.ContainsKey(field.Name)) continue;

                            //$%_$ помечены кэпшэны которые не найдены в базе, ожидаем их в файле excel
                            allFieldsHaveValue = !item[field.Name].NullableToString().StartsWith("$%_$");
                        }

		                // Значения для вставки без полей иерархии(только если не были найдены значения)
		                var items = new Dictionary<string, object>();
                        if (allFieldsHaveValue)
		                {
                            items = item.ToDictionary(d => d.Key, d => d.Value);
		                }
		                else
		                {
                            items = item.Where(w => parentFields.All(a => a.Name != w.Key))
                                        .ToDictionary(d => d.Key, d => d.Value);
		                }

		                //Пытаемся создать
		                try
		                {
                            var id = _dataManager.CreateEntry(items, ts==null);

		                    if (parentFields.Any())
		                    {
		                        if (parentDictionary.ContainsKey(item[Entity.CaptionField.Name].ToString()) &&
		                            _template.ExecImportMode == ExecImportMode.ReportErrorAndStop)
		                            throw new Exception(
		                                "Ошибка импорта! Было обнаруженно что имеются два одинаковых Наименования, при этом есть поле родитель и Способ выполнения импорта \"Выдавать исключние, прерывать импорт\"! Наименование: " +
		                                item[Entity.CaptionField.Name]);

                                if (!parentDictionary.ContainsKey(item[Entity.CaptionField.Name].ToString()))
		                            parentDictionary.Add(item[Entity.CaptionField.Name].ToString(), (int) id);
		                    }
		                    loadRes.CreateCount++;
		                    loadRes.CreatedItems.Add((int) id);
		                }
		                catch (Exception exception)
		                {
		                    exceptions[exception.HResult] = exception;
		                    errorItems[exception.HResult] = item;

		                    if (!continueOnError)
		                    {
		                        throw;
		                    }
		                }
		            }

		        //обновление строк
		        if (importType == ImportType.CreateNewAndUpdateExist || importType == ImportType.UpdateExist)
		            foreach (var item in loadItems)
		            {
                        if (exceptions.Count > 0 && _template.IsPerformSingleTransaction)
                        {
                            break;
                        }

		                bool filter = true;
		                //Строим фильтр для поиска такой же записи в справочнике
		                var filterCollection = new List<IFilterConditions>();
		                //Если табличная часть и поле Владельца не указано в ключевых полях, добавляем фильтр сами
		                if (Entity.EntityType == EntityType.Tablepart && keyFields.All(a => a.Name != "idOwner"))
		                {
		                    filterCollection.Add(new FilterConditions
		                        {
		                            Field = "idOwner",
		                            Value = item["idOwner"],
		                            Operator = ComparisionOperator.Equal
		                        });
		                }
		                //Проходимся по списку ключевых полей и добавляем к фильтру
		                foreach (EntityField field in keyFields)
		                {
		                    var fieldValue = item[field.Name];
		                    fieldValue = field.EntityFieldType == EntityFieldType.String ? fieldValue.ToString() : fieldValue;

		                    if (fieldValue.NullableToString() == string.Empty) filter = false;

		                    filterCollection.Add(new FilterConditions
		                        {
		                            Field = field.Name,
		                            Value = fieldValue,
		                            Operator = ComparisionOperator.Equal
		                        });
		                }

		                //Если с фильтром все хорошо, делаем запрос
		                List<Dictionary<string, object>> result = null;
		                if (filter)
		                {
		                    ISelectQueryBuilder selectBuilder = new QueryFactory(Entity).Select();
		                    {
		                        selectBuilder.QueryDecorators.Add(new AddSysDimensionsFilter(null));

		                        if (filterCollection.Count >= 2)
		                        {
		                            selectBuilder.Conditions = new FilterConditions
		                                                           {
		                                                               Type = LogicOperator.And,
		                                                               Operands = filterCollection
		                                                           };
		                        }
		                        if (filterCollection.Count == 1)
		                        {
		                            selectBuilder.Conditions = filterCollection.First();
		                        }
		                        result = Select(selectBuilder);
		                    }
		                }
		                // Новым элемент считаем, если не указано ни одно ключевое поле или результат запроса пусто
		                var itemIsNew = !keyFields.Any() || result == null || !result.Any();
		                // Если новый создвать нельзя, записываем исключение
//		                if (itemIsNew && importType == ImportType.UpdateExist)
//		                {
//
////		                    exceptions.Add(i,
////		                                   new Exception(
////		                                       "Невозможно создать новый элемент, т.к. это запрещено настройками импорта!"));
////		                    errorItems.Add(i, item);
////		                    i++;
//		                    break;
//		                }
		                // Если элемент новый, пропускаем
		                if (itemIsNew) continue;

		                if (exceptions.Count > 0 && (_template.IsPerformSingleTransaction || !continueOnError))
		                {
		                    break;
		                }

		                int? id = null;
		                foreach (Dictionary<string, object> nestedItem in result)
		                {
		                    id = nestedItem["id"] as int?;
		                }

		                //Если есть ид элемента который надо обновить, продолжаем
		                if (id == null) continue;

		                foreach (var field in parentFields)
		                {
		                    if (!item.ContainsKey(field.Name)) continue;

                            //$%_$ помечены кэпшэны которые не найдены в базе, ожидаем их в файле excel
		                    bool needReplace = item[field.Name].NullableToString().StartsWith("$%_$");

		                    if (!needReplace) continue;

                            //получаем строку для поиска ее в словаре
		                    var key = item[field.Name].ToString().Replace("$%_$", "");

		                    if (parentDictionary.ContainsKey(key))
		                        item[field.Name] = parentDictionary[key];
		                    else
		                    {
		                        var exeption = new Exception("Не найден родитель для элемента");
		                        exceptions.Add(id, exeption);
		                        errorItems.Add((int) id, item);
		                        if (!continueOnError)
		                        {
		                            throw exeption;
		                        }
		                    }
		                }

		                try
		                {
		                    _dataManager.UpdateEntry((int) id, (Dictionary<string, object>) item);

		                    if (loadRes.CreatedItems.All(a => a != id))
		                    {
		                        loadRes.UpdatesCount++;
		                        loadRes.UpdatedItems.Add((int) id);
		                    }
		                }
		                catch (Exception exception)
		                {
		                    if (!exceptions.ContainsKey(id))
		                    {
		                        exceptions.Add(id, exception);
		                        errorItems.Add((int) id, item);
		                    }
		                    if (!continueOnError)
		                    {
		                        throw;
		                    }
		                }
		            }
		        if (_template.IsPerformSingleTransaction && exceptions.Count == 0)
		        {
		            ts.Complete();
		        }
		    }
            finally
            {
                if (ts != null)
                {
                    ts.Dispose();
                }
            }
		    loadRes.Exceptions = exceptions;
			loadRes.ErrorItems = errorItems;
			return loadRes;
		}

		private void GenerateMsg(StringBuilder sb, LoadResult reslt, IDictionary<IDictionary<string, object>, IDictionary<string, object>> rawData)
		{
			if (reslt.Exceptions.Count == 0 || !_template.IsPerformSingleTransaction)
			{
				sb.AppendFormat(
					"<h3>Импорт завершен:</h3>Создано: <b>{0}</b> <br/>Изменено:<b>{1}</b> <br/>Ошибок:{2}<br/>",
					reslt.CreateCount,
					reslt.UpdatesCount,
					reslt.Exceptions.Count());
			}
			else
			{
				sb.AppendFormat(
					"<h3>Импорт остановлен:</h3>Правильно загруженных новых элементов: <b>{0}</b> <br/>Правильно загруженных измененных элементов: <b>{1}</b> <br/>Ошибок: <b>{2}</b><br/>",
					reslt.CreateCount,
					reslt.UpdatesCount,
					reslt.Exceptions.Count());
			}

			foreach (KeyValuePair<int?, Exception> exception in reslt.Exceptions)
			{
//				var referenceException = exception.Value;
//				if (referenceException != null)
//				{
//					var allKeys =
//						new IDictionary<string, object>[]
//							{
//								reslt.ErrorItems[exception.Key ?? 0],
//								rawData.ContainsKey(reslt.ErrorItems[exception.Key ?? 0])
//									? rawData[reslt.ErrorItems[exception.Key ?? 0]]
//									: new Dictionary<string, object>()
//							}
//							.SelectMany(p => p).Select(p => p.Key).Distinct();
//					var errorInfo = new Dictionary<string, object>();
//					foreach (var field in allKeys)
//					{
//						var friendly = rawData.ContainsKey(reslt.ErrorItems[exception.Key ?? 0])
//							               ? rawData[reslt.ErrorItems[exception.Key ?? 0]].ContainsKey(field)
//								                 ? rawData[reslt.ErrorItems[exception.Key ?? 0]][field]
//								                 : null
//							               : null;
//						var sys = reslt.ErrorItems[exception.Key ?? 0].ContainsKey(field) ? reslt.ErrorItems[exception.Key ?? 0][field] : null;
//						errorInfo[field] = sys != null
//							                   ? string.Format("<b>{0}</b>[{1}]", friendly, sys)
//							                   : string.Format("<b>{0}</b>", friendly);
//					}
//					sb.Append(errorInfo);
//				}
//				else
//				{
					sb.Append(exception.Value.Message);
//				}
			}
		}

		/// <summary>
		/// Получение строк сущности
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public List<Dictionary<string, object>> Select(ISelectQueryBuilder query)
		{
			var result = new List<Dictionary<string, object>>();
			
			var connection = IoC.Resolve<SqlConnection>("DbConnection");
			var cmd = query.GetSqlCommand(connection);
			using (SqlDataReader reader = cmd.ExecuteReaderLog())
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
			return result;
		}

		/// <summary>
		/// The load result.
		/// </summary>
		private class LoadResult
		{
			public Entity EntityType
			{
				get;
				protected set;
			}

			/// <summary>
			/// Gets or sets CreateCount.
			/// </summary>
			public int CreateCount { get; set; }

			/// <summary>
			/// Gets or sets CreatedItems.
			/// </summary>
			public List<int> CreatedItems { get; set; }

			/// <summary>
			/// Gets or sets ErrorItems.
			/// </summary>
			public IDictionary<int, IDictionary<string, object>> ErrorItems { get; set; }

			/// <summary>
			/// Gets or sets Exceptions.
			/// </summary>
			public IDictionary<int?, Exception> Exceptions { get; set; }

			/// <summary>
			/// Gets or sets UpdatedItems.
			/// </summary>
			public List<int> UpdatedItems { get; set; }

			/// <summary>
			/// Gets or sets UpdatesCount.
			/// </summary>
			public int UpdatesCount { get; set; }

			public LoadResult()
			{
				CreatedItems = new List<int>();
				UpdatedItems = new List<int>();
			}

			public LoadResult(Entity type)
			{
				EntityType = type;
			}
		}
	}

}
	