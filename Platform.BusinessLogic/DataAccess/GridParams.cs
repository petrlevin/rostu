using System;
using System.Collections.Generic;
using Platform.BusinessLogic.ServerFilters;
using Platform.Client.Filters;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Параметры для отображения списка элементов
    /// </summary>
    public class GridParams
    {
        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public GridParams()
        {
            Filters = new List<ClientFilter>();
        }

        /// <summary>
        /// Информация о сортировки колонки
        /// </summary>
        public class SortInfo
		{
			/// <summary>
			/// Имя колонки
			/// </summary>
			public string Property;
			
            /// <summary>
			/// Порядок сортировки
			/// </summary>
			public string Direction;
		}

	    /// <summary>
		/// Идентификатор сущности, которой принадлежит поле
	    /// </summary>
		public int SrcEntityId;

	    /// <summary>
	    /// Идентификатор поля, которое инициировало отправку запроса за данными. 
	    /// Данное свойство не получает значения только в случае, когда запрос данных инициирован формой списка элементов сущности.
	    /// Во всех остальных случаях запрос за данными к методу gridSource инициируется полем:
	    /// - форма выбора ссылочного поля
	    /// - мультиссылка
	    /// - табличная часть
	    /// </summary>
		public int? FieldId;
        
        /// <summary>
        /// Идентификатор сущности, на которое ссылается поле, инициировавшее отправку запроса
        /// </summary>
        public int EntityId;
        
        /// <summary>
        /// Количество элементов на странице
        /// </summary>
        public int Limit;
        
        
        /// <summary>
        /// Текущий номер страницы
        /// </summary>
        public int Page;

        /// <summary>
        /// Текущие сортировки в списке
        /// </summary>
        public SortInfo[] Sort;
        
        //todo: выпилить?
        public int Start;

        /// <summary>
        /// Системное имя 
        /// </summary>
        public string OwnerFieldName;
        
		/// <summary>
		/// Идентификатор элемента сущности-владельца. 
		/// При запросе данных для: 
        /// * (М)ТЧ
        /// * формы выбора ссылочного поля, находящегося в форме элемента ТЧ
        /// docid содержит идентификатор элемента сущности-владельца.
		/// </summary>
		public int? DocId;

        /// <summary>
        /// Идентификатор элемента.
        /// При запросе данных для: 
        /// * (М)ТЧ
        /// * формы выбора ссылочного поля, находящегося в форме элемента ТЧ
        /// ItemId содержит идентификатор элемента сущности, где находится поле, для которого поступил запрос за данными.
        /// </summary>
        public int? ItemId;

	    /// <summary>
		/// Значения для полей, от которых зависит поле, данные (форма выбора или грид) для которого запрошены с клиента
	    /// </summary>
		public FieldValues FieldValues;

		/// <summary>
	    /// Дата актуальности (для версионных сущностей)
	    /// </summary>
		public DateTime? ActualDate;

	    /// <summary>
	    /// Имя поля, по которому выстраивается иерархия
	    /// </summary>
		public string HierarchyFieldName;

		/// <summary>
		/// Значение родителя
		/// </summary>
	    public int? HierarchyFieldValue;

		/// <summary>
		/// Список значений родителей
		/// </summary>
		public List<int?> HierarchyFieldValues;
		
		/// <summary>
	    /// Подстрока поиска.
	    /// Если в данном поле приходит значение, то ищется во всех отбираемых столбцах
	    /// </summary>
		public string Search;


        /// <summary>
        /// Видимые колонки
        /// </summary>
        public string[] VisibleColumns;

		/// <summary>
		/// Идентификаторы периодов, соответствующие значимым колонкам денормализованной ТЧ
		/// </summary>
	    public List<int> DenormalizedPeriods;

        /// <summary>
        /// Признак того, что запрос идет от формы выбора.
        /// Для формы выбора будет включена дополнительная фильтрация по статусу (только элементы на статусе В Работе).
        /// </summary>
        public bool IsSelectionFormRequest;

        /// <summary>
        /// Клиентские фильтры
        /// </summary>
        public IEnumerable<ClientFilter> Filters { get; set; }
    }
}