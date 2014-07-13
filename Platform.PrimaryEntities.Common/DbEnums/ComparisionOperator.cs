namespace Platform.PrimaryEntities.Common.DbEnums
{
    /// <summary>
	/// Бинарный оператор сравнения для фильтров.
	/// </summary>
	/// <remarks>
	/// Перечисление используется классом FilterConditions и Серверным фильтром
	/// </remarks>
    [ClientEnum]
    public enum ComparisionOperator
	{
		/// <summary>
		/// Равно
		/// </summary>
		Equal = 0,

		/// <summary>
		/// Больше
		/// </summary>
		Greater = 1,

		/// <summary>
		/// Больше или равно
		/// </summary>
		GreaterOrEqual = 2,

		/// <summary>
		/// Меньше
		/// </summary>
		Less = 3,

		/// <summary>
		/// Меньше или равно
		/// </summary>
		LessOrEqual = 4,
		
		/// <summary>
		/// Содержит. Сопоставляет значение поля шаблону. Только для строковых операндов. Например '%234%'.
		/// </summary>
		Like = 5,

		/// <summary>
		/// В списке
		/// </summary>
		InList = 6,

		/// <summary>
		/// IS NULL
		/// </summary>
		IsNull = 7,

		/// <summary>
		/// IS NOT NULL
		/// </summary>
		IsNotNull = 8,

        /// <summary>
        /// В дату
        /// Только для полей типа Дата или ДатаВремя. 
        /// Данный тип сравнения применяет к операндам следующую t-sql функцию: DATEDIFF(day, op1, op2) = 0.
        /// </summary>
        InSameDate = 9
	}
}
