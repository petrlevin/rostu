namespace Platform.PrimaryEntities.Common.DbEnums
{
	/// <summary>
	/// Тип вычисляемого поля сущности
	/// </summary>
	public enum CalculatedFieldType
	{
		/// <summary>
		/// Присоединенное (бывш. Виртуальная ссылка) (только для формы)
		/// </summary>
		Joined = 1,

		/// <summary>
		/// Клиентская формула (только для формы)
		/// </summary>
		ClientComputed = 2,

		/// <summary>
		/// Вычисляемое (Формула БД)
		/// </summary>
		DbComputed = 3,

		/// <summary>
		/// Расширение SelectQueryBuilder'а
		/// </summary>
		AppComputed = 4,

        /// <summary>
        /// Формула со значениями нумератора
        /// </summary>
        NumeratorExpression = 5,

		/// <summary>
		/// Вычисляемое (функция БД)
		/// </summary>
		DbComputedFunction = 6,

		/// <summary>
		/// Вычисляемое хранимое (Формула БД Persisted)
		/// </summary>
		DbComputedPersisted = 7
	}
}
