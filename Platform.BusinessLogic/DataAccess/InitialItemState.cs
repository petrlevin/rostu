using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.DataAccess
{
	/// <summary>
	/// Начальное состояние формы нового элемента.
	/// Дефолты, Видимость и редактируемость полей.
	/// </summary>
	public class InitialItemState
	{
		/// <summary>
		/// Значения по умолчанию
		/// </summary>
		public Dictionary<string, object> Defaults;

		/// <summary>
		/// Редактируемые поля. Актуально только при создании документов/инструментов.
		/// Если null - считаем, что все поля разрешены редактировать.
		/// Если коллекция пустая, то ни одно поле нельзя редактировать, элемент открыт только для чтения.
		/// </summary>
		public List<string> EditableFields;
	}
}
