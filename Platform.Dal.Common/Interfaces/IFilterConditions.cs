using System.Collections.Generic;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.Dal.Common.Interfaces
{
	public interface IFilterConditions
	{
		/// <summary>
		/// Тип выражения
		/// </summary>
		LogicOperator Type { get; set; }

		/// <summary>
		/// Отрицание выражения.
		/// </summary>
		bool Not { get; set; }

		/// <summary>
		/// Поле - левый операнд условного выражения
		/// </summary>
		string Field { get; set; }

		/// <summary>
		/// Условие заданное текстом sql-запроса.
		/// </summary>
		string Sql { get; set; }

		/// <summary>
		/// Бинарный оператор
		/// </summary>
		ComparisionOperator Operator { get; set; }

		/// <summary>
		/// Значение. Тип значения вычисляется из типа поля. Значение может быть списком (если Operator = InList).
		/// </summary>
		object Value { get; set; }

		/// <summary>
		/// Список операндов одной из логической операции И, Или, Не. 
		/// </summary>
		List<IFilterConditions> Operands { get; set; }
	}
}
