using System.Collections.Generic;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Абстарктный класс для построения конструкции SELECT
	/// </summary>
	public abstract class BaseSelect : BaseDml
	{
		/// <summary>
		/// Поля для результирующего набора
		/// </summary>
		protected IEnumerable<string> FieldsName;
		
		/// <summary>
		/// Метод, осуществяющий построение запроса
		/// </summary>
		/// <returns></returns>
		public abstract SelectStatement GetQuery();
	}
}
