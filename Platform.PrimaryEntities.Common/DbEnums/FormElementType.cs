using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.PrimaryEntities.Common.DbEnums
{
	/// <summary>
	/// Тип элемента формы
	/// </summary>
	public enum FormElementType
	{
		/// <summary>
		/// Поле сущности
		/// </summary>
		ReferenceField=1,

		/// <summary>
		/// Присоединенное
		/// </summary>
		Joined=2,

		/// <summary>
		/// Клиентская формула
		/// </summary>
		ClientFormula = 3,

		/// <summary>
		/// Вычисляемое (Формула БД)
		/// </summary>
		Computed = 4
	}
}
