using System.Collections;
using System.Collections.Generic;

namespace Platform.PrimaryEntities.Common.Interfaces
{
	/// <summary>
	/// Интерфейс Форм
	/// </summary>
	public interface IForm
	{
		/// <summary>
		/// Сущность, на которой основана форма
		/// </summary>
		IEntity Entity { get; set; }

        ///// <summary>
        ///// Признак, того что форма стоится на основе полей сущности
        ///// </summary>
        //bool IsDefault { get; set; }

		/// <summary>
		/// Поля формы
		/// </summary>
		IEnumerable<IFormElement> FormElements { get; set; }
	}
}
