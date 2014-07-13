namespace Platform.PrimaryEntities.Common.DbEnums
{
	/// <summary>
	/// Логический оператор
	/// </summary>
	/// <remarks>
	/// Перечисление используется классом FilterConditions и Серверным фильтром.
	/// </remarks>
	public enum LogicOperator
	{
		/// <summary>
		/// Простое выражение
		/// Выражение, не содержащее логических операторов "И" и "ИЛИ"
		/// </summary>
		Simple,

		/// <summary>
		/// Группа И
		/// Операнды группы - это выражения, соединенные логическим оператором И
		/// </summary>
		And,

		/// <summary>
		/// Группа Или
		/// Операнды группы - это выражения, соединенные логическим оператором Или
		/// </summary>
		Or
	}
}
