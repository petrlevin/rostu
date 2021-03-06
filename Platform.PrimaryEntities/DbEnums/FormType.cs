﻿namespace Platform.PrimaryEntities.DbEnums
{
	/// <summary>
	/// Тип формы. 
	/// Используется: 
	/// - при открытии автогенерируемой формы (при этом нужно понимать какую форму генерировать - для списка или для элемента).
	/// - для сущности можно назначить 2 формы по-умолчанию - для элемента и для списка.
	/// - при выборе формы списка для пункта меню панели навигации.
	/// </summary>
	public enum FormType
	{
		/// <summary>
		/// Форма элемента
		/// </summary>
		Item = 1,
        
        /// <summary>
		/// Форма списка
		/// </summary>
		List = 2,

        /// <summary>
        /// Форма выбора
        /// </summary>
        Selection = 3 // ToDo{CORE-280}
    }
}
