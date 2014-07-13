using System.Collections.Generic;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Абстарктный класс для построения конструкции INSERT
	/// </summary>
	public abstract class BaseInsert : BaseDml
	{
		/// <summary>
		/// Поля в которые осуществляется вставка
		/// </summary>
		protected IEnumerable<string> FieldsName;

		/// <summary>
		/// Метод, осуществяющий построение запроса
		/// </summary>
		/// <returns></returns>
		public abstract InsertStatement GetQuery();

	}
}
