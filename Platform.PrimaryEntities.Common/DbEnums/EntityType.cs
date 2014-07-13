namespace Platform.PrimaryEntities.Common.DbEnums
{
	/// <summary>
	/// Тип сущности
	/// </summary>
	[ClientEnum]
    public enum EntityType
	{
		/// <summary>
		/// Перечисление
		/// </summary>
		Enum = 1,
		
		/// <summary>
		/// Точка данных
		/// </summary>
		DataEndpoint = 2,

		/// <summary>
		/// Справочник
		/// </summary>
		Reference = 3,
		
		/// <summary>
		/// Табличная часть
		/// </summary>
		Tablepart = 4,
		
		/// <summary>
		/// Мультиссылка
		/// </summary>
		Multilink = 5,
		
		/// <summary>
		/// Документ
		/// </summary>
		Document = 6,
		
		/// <summary>
		/// Инструмент
		/// </summary>
		Tool = 7,
		
		/// <summary>
		/// Регистр
		/// </summary>
		Registry = 8,
	
		/// <summary>
		/// Отчет
		/// </summary>
		Report = 9
	}
}
