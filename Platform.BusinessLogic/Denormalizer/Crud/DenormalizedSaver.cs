using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Platform.BusinessLogic.DataAccess;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.Utils.Collections;

namespace Platform.BusinessLogic.Denormalizer.Crud
{
	public class DenormalizedSaver : DenormalizedCrud
	{
		private const string columnNameMask = @"^(.+)\{(-?\d+)\}$";

		private readonly Regex columnNameRegex;
        
        /// <summary>
        /// Словарь вида [имя поля = значение], 
        /// в который из словаря всех отправленных с клиента значений отбираются только 
        /// значения полей родительской сущности ДТЧ.
        /// </summary>
        private IgnoreCaseDictionary<object> masterTpValues { get; set; }
        
        /// <summary>
        /// Словарь вида [id периода = [имя поля дочерней сущности ДТЧ = значение]], 
        /// в который из словаря всех отправленных с клиента значений отбираются только 
        /// значимые поля ДТЧ, сгруппированные по периодам
        /// </summary>
        private Dictionary<int,IgnoreCaseDictionary<object>> periodValues { get; set; }

        /// <summary>
        /// Данные из БД.
        /// Словарь вида [id периода = [имя поля дочерней сущности ДТЧ = значение]], 
        /// в который записываются все строки дочерней сущности, относящиеся к периодам, затронутыми при сохранении.
        /// </summary>
        private Dictionary<int, IgnoreCaseDictionary<object>> existingData { get; set; }

	    /// <summary>
	    /// Соединение с БД, полученное из контейнера
	    /// </summary>
        private SqlConnection connection
	    {
	        get { return IoC.Resolve<SqlConnection>("DbConnection"); }
	    }

	    private List<TSqlStatementDecorator> decorators
	    {
	        get { return new List<TSqlStatementDecorator>() { new AddWhere() }; }
	    }

		public IgnoreCaseDictionary<object> Values { get; set; }

		public int? ItemId { get; set; }

		private DataManager childDm { get; set; }

		public DenormalizedSaver(int entityId)
			: base(entityId)
		{
			columnNameRegex = new Regex(columnNameMask, RegexOptions.Multiline);
		}

        public int? SaveElement(int? itemId, IgnoreCaseDictionary<object> values)
		{
            Check();
            ItemId = itemId;
			Values = values;
            masterTpValues = new IgnoreCaseDictionary<object>(values.Where(kvp => !columnNameRegex.IsMatch(kvp.Key)));
            periodValues = getPeriodValues(values);
            childDm = createChildDataManager();

            if (!IsMasterTablepart)
            {
                throw new PlatformException("Сущность, указанная в конструкторе, не является денормализованной ТЧ.");
            }

			masterDm = DataManagerFactory.Create(TargetEntity);

			if (!ItemId.HasValue)
			{
				return create();
			}
			return update();
		}

		#region Private Methods

		/// <summary>
		/// Пока принудительно создается DalDataManager.
		/// </summary>
		/// <remarks>
		/// Пока принудительно создается DalDataManager. Использование EfDataManager пока не реализовано.
		/// До того как будет реализована поддержка EfDataManager следует добавить проверку на наличие контролей. 
		/// Если в сущностном классе есть контроли - выдавать исключение, т.к. пользователь будет ожидать, что они отработают.
		/// Для поддержки EfDataManager необходимо чтобы инструкции update и delete могли отрабатывать по некоторому фильтру 
		/// (как это делается данным классом при использовании DalDataManager'а).
		/// </remarks>
		/// <returns></returns>
		private DataManager createChildDataManager()
		{
			//return DataManagerFactory.Create(TargetEntity);
			return new DalDataManager(connection, TpAnalyzer.ChildTp);
		}

        /// <summary>
        /// Считываем строки из дочерней сущности ДТЧ, относящиеся к затронутым при сохранении периодам.
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, IgnoreCaseDictionary<object>> readExistingData()
        {
            var result = new Dictionary<int, IgnoreCaseDictionary<object>>();
            if (!periodValues.Keys.Any())
                return result;

            var select = new SelectQueryBuilder(TpAnalyzer.ChildTp)
                {
                    Conditions = getFilterByMasterAndPeriod(periodValues.Keys.ToArray()),
                    Order = new Order() {{TpAnalyzer.HierarchyPeriodField.Name, true}}
                };
            SqlCommand cmd = select.GetSqlCommand(connection, decorators);

            using (SqlDataReader reader = cmd.ExecuteReaderLog())
            {
                while (reader.Read())
			    {
                    var row = new IgnoreCaseDictionary<object>();

			        foreach (string valueFieldName in TpAnalyzer.ValueFields.Select(vf => vf.Name))
			        {
			            row[valueFieldName] = reader[valueFieldName];
			        }

                    int periodId = reader.GetInt32(reader.GetOrdinal(TpAnalyzer.HierarchyPeriodField.Name));
			        result[periodId] = row;
			    }
			    reader.Close();
            }
            return result;
        }

        /// <summary>
        /// Через интерфейс системы создали новый элемент ДТЧ => осуществляется только вставка одного или нескольких элементов
        /// </summary>
        /// <returns></returns>
		private int? create()
		{
			int masterId = (int)masterDm.CreateEntry(masterTpValues); // создание строки в родительской ТЧ

			foreach (int periodId in periodValues.Keys.Where(pid => !periodValues[pid].All(value => isEmptyValue(pid))))
			{
                createChildItem(masterId, periodId);
			}
			return masterId;
		}

        /// <summary>
        /// В интерфейсе системы сохранили существующий элемент ДТЧ => возможно удаление, обновление и создание элементов
        /// </summary>
        /// <returns></returns>
		private int? update()
		{
            existingData = readExistingData();

            int[] shouldBeDeleted = periodValues.Keys
                .Where(pid => periodValues[pid].All(kvp => isEmptyValue(kvp.Value))) // сохраняемые значения перида пусты
                .Where(pid =>
                    periodValues[pid].Count == TpAnalyzer.ValueFields.Count // сохраняем (пустые значения) для всех ресурсных полей периода
                    || isOtherResourcesEmpty(pid) // значения незатронутых при сохранении ресурсных полей из БД пустые
                    )
                .ToArray();

            int[] shouldBeUpdated = periodValues.Keys
                .Where(pid => existingData.ContainsKey(pid) && !shouldBeDeleted.Contains(pid))
                .ToArray();

            int[] shouldBeCreated = periodValues.Keys
                .Where(pid => !existingData.ContainsKey(pid) && !periodValues[pid].Values.All(v => isEmptyValue(v)))
                .ToArray();

			if (childDm is DalDataManager)
			{
				deleteChildItems(shouldBeDeleted);
				
				// Обновляем
				foreach (int periodId in shouldBeUpdated)
				{
					int cnt = updateChildItem(periodId);
					if (cnt == 0)
					{
						throw new PlatformException("При обновлении строки дочерней сущности ДТЧ не было обновлено ни одной записи. Что-то не в порядке.");
					}
				}

                // Вставляем
			    foreach (var periodId in shouldBeCreated)
			    {
                    createChildItem((int)ItemId, periodId); 
			    }
			}
			else if (shouldBeUpdated.Any() || shouldBeDeleted.Any())
			{
				throw new NotImplementedException("Обновление и удаление элементов дочерней ТЧ в денормализованном виде не реализовано для EfDataManager, который используется при наличии сущностного класса");
			}
			
			// обновляем запись в родительской ТЧ
			if (masterTpValues.Any())
			{
				return masterDm.UpdateEntry((int)ItemId, masterTpValues);
			}
			return ItemId;
		}

		/// <summary>
		/// удаляем элементы дочерней таблицы, для которых установили пустое значение
		/// </summary>
		private void deleteChildItems(int[] itemIds)
		{
			if (itemIds.Any())
				((DalDataManager)childDm).DeleteItem(getFilterByMasterAndPeriod(itemIds));
		}

		/// <summary>
		/// Создает строку в дочерней сущности ДТЧ
		/// </summary>
		/// <param name="masterId"></param>
		/// <param name="periodId"></param>
		/// <returns></returns>
        private int? createChildItem(int masterId, int periodId)
		{
		    Dictionary<string, object> childTpValues = periodValues[periodId]
		        .Where(kvp => !isEmptyValue(kvp.Value))
		        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

		    if (!childTpValues.Any())
		        return 0;
            
		    Dictionary<string, object> values = childTpValues
                .Concat(getLinkingValues(masterId, periodId))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return childDm.CreateEntry(values);
		}

		private int updateChildItem(int periodId)
		{
			var filter = getFilterByMasterAndPeriod(periodId);
			return ((DalDataManager)childDm).UpdateEntries(filter, periodValues[periodId]);
		}

		/// <summary>
		/// Получает фильтр по полям idMaster и idHierarchiPeriod
		/// </summary>
		/// <param name="periodValue">int или int[] - идентификатор(ы) периода</param>
		/// <returns></returns>
		private FilterConditions getFilterByMasterAndPeriod(object periodValue)
		{
			return new FilterConditions
			{
				Type = LogicOperator.And,
				Operands = new List<IFilterConditions>
					{
						new FilterConditions
							{
								Type = LogicOperator.Simple,
								Field = TpAnalyzer.MasterField.Name,
								Operator = ComparisionOperator.Equal,
								Value = (int)ItemId
							},
						new FilterConditions
							{
								Type = LogicOperator.Simple,
								Field = TpAnalyzer.HierarchyPeriodField.Name,
								Operator = periodValue is IEnumerable ? ComparisionOperator.InList : ComparisionOperator.Equal,
								Value = periodValue
							}
					}
			};
		}

		/// <summary>
		/// Получает значения для строки дочерней ТЧ, связывающие ее со строкой в родительской ТЧ
		/// </summary>
		/// <param name="periodId">id периода</param>
		/// <param name="value">Числовое значение</param>
		/// <returns></returns>
        private Dictionary<string,object> getLinkingValues(int masterId, int periodId)
		{
			return new IgnoreCaseDictionary<object>
				{
					{ TpAnalyzer.OwnerField.Name, masterTpValues[TpAnalyzer.OwnerField.Name] },
					{ TpAnalyzer.MasterField.Name, masterId },
					{ TpAnalyzer.HierarchyPeriodField.Name, periodId }
				};
		}

        /// <summary>
        /// Возвращает значения, сгруппированные по периодам
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private Dictionary<int, IgnoreCaseDictionary<object>> getPeriodValues(IgnoreCaseDictionary<object> values)
        {
            var result = new Dictionary<int, IgnoreCaseDictionary<object>>();
            foreach (KeyValuePair<string, object> keyValuePair in values)
            {
                string valueFieldName;
                int periodId;

                if (tryParseColumnName(keyValuePair.Key, out valueFieldName, out periodId))
                {
                    if (!result.ContainsKey(periodId))
                        result.Add(periodId, new IgnoreCaseDictionary<object>());
                    result[periodId][valueFieldName] = keyValuePair.Value;
                }
            }
            return result;
        }

	    /// <summary>
	    /// Попадает ли <paramref name="colName"/> под регулярное выражение значимого поля ДТЧ?
	    /// Если да, то в <paramref name="valueFieldName"/> и <paramref name="periodId"/> возвращаются значения.
	    /// </summary>
	    /// <param name="colName">имя поля, пришедшее с клиента</param>
	    /// <param name="valueFieldName">Имя значимого поля в дочерней сущности ДТЧ</param>
	    /// <param name="periodId">идентификатор периода</param>
	    /// <returns>true в случае, если имя колонки попадает под регулярное выражение, false - в противном случае</returns>
	    private bool tryParseColumnName(string colName, out string valueFieldName, out int periodId)
        {
            var match = columnNameRegex.Match(colName);
            if (match.Success && match.Groups.Count == 3)
            {
                periodId = int.Parse(match.Groups[2].Value);
                valueFieldName = match.Groups[1].Value;
                return true; 
            }
	        periodId = 0;
	        valueFieldName = string.Empty;
	        return false;
        }

        /// <summary>
        /// Ресурсные поля периода <paramref name="periodId"/>, не затронутые при сохранении, в БД имеют пустое значение.
        /// </summary>
        /// <param name="periodId"></param>
        /// <returns></returns>
        bool isOtherResourcesEmpty(int periodId)
        {
            IEnumerable<string> others = TpAnalyzer.ValueFields.Select(vf => vf.Name).Where(vfname => !periodValues[periodId].ContainsKey(vfname));
            if (existingData.ContainsKey(periodId) && others.Any())
            {
                return existingData[periodId].Where(kvp => others.Contains(kvp.Key)).All(kvp => isEmptyValue(kvp.Value));
            }
            return true;
        }

        /// <summary>
        /// value == null || string.IsNullOrEmpty(value.ToString())
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool isEmptyValue(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString());
        }

		#endregion
	}
}
