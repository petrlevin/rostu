namespace Platform.PrimaryEntities.Common.DbEnums
{
	/// <summary>
	/// Тип поля сущности
	/// </summary>
	[ClientEnum]
	public enum EntityFieldType
	{
		/// <summary>
		/// Булево
		/// Бинарное значение true или false
		/// </summary>
		Bool = 1,
		
		/// <summary>
		/// Nvarchar от 1 до 4000
		/// Строка
		/// </summary>
		String = 2,
		
		/// <summary>
		/// Целое
		/// Целое число
		/// </summary>
		Int	= 3,
		
		/// <summary>
		/// Большое целое
		/// Большое целое число
		/// </summary>
		BigInt = 4,
		
		/// <summary>
		/// Вещественное
		/// Вещественное число
		/// </summary>
		Numeric = 5,
		
		/// <summary>
		/// Дата и время
		/// Формат даты
		/// </summary>
		DateTime = 6,
		
		/// <summary>
		/// Ссылка
		/// Ссылка на сущность
		/// </summary>
		Link = 7,
		
		/// <summary>
		/// Мультиссылка
		/// Для установления соответсвия между двумя таблицами в третьей
		/// </summary>
		Multilink = 8,
		
		/// <summary>
		/// Табличная часть
		/// Поле отображаемое в виде таблицы
		/// </summary>
		Tablepart = 9,
		
		/// <summary>
		/// Точка данных.
		/// Аналог табличной части, но с произвольными инструкциями на операции CRUD.
		/// </summary>
		DataEndpoint = 13,

		/// <summary>
		/// Guid
		/// Доп
		/// </summary>
		Guid = 14,

		/// <summary>
		/// Tinyint
		/// Используется для Enumerator
		/// </summary>
		TinyInt = 15,

		/// <summary>
		/// Smallint
		/// Доп
		/// </summary>
		SmallInt = 16,

		/// <summary>
		/// Nvarchar(MAX)
		/// Дополнительное текстовое описание
		/// </summary>
		Text = 17,

		/// <summary>
		/// Виртуальная ТЧ
		/// Виртуальная ТЧ - поле сущности ссылающееся на справочник, в котором есть поле, ссылающееся на данную сущность.
		/// Записи виртуальных ТЧ не копируются при клонировании документов.
		/// </summary>
		VirtualTablePart = 18,

		/// <summary>
		/// XML
		/// XML
		/// </summary>
		Xml = 19,

		/// <summary>
		/// Общая ссылка  
		/// Общая ссылка на справочник, инструмент , документ
		/// </summary>
		ReferenceEntity = 20,

		/// <summary>
		/// Общая ссылка  на инструмент
		/// Общая ссылка на инструмент , документ
		/// </summary>
		ToolEntity = 21,

		/// <summary>
		/// Общая ссылка на табличную часть
		/// Общая ссылка на табличную часть
		/// </summary>
		TablepartEntity = 22,


		/// <summary>
		/// Общая ссылка на документ
		/// Общая ссылка на документ
		/// </summary>
		DocumentEntity = 23,

		/// <summary>
		/// Дата
		/// Формат даты
		/// </summary>
		Date = 24,

		/// <summary>
		/// Деньги
		/// Денежный формат
		/// </summary>
		Money = 25,

		/// <summary>
		/// Файл
		/// Поле для хранения файла
		/// </summary>
		File = 26,

        /// <summary>
		/// Ссылка на общие файлы
		/// Поле для ссылки на общие файлы
		/// </summary>
        FileLink = 27

    }
}
