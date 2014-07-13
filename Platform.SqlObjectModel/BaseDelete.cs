using System.Collections.Generic;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Абстарктный класс для построения конструкции DELETE
	/// </summary>
	public abstract class BaseDelete : BaseDml
	{
		/// <summary>
		/// Метод, осуществяющий построение запроса
		/// </summary>
		/// <returns></returns>
		public abstract DeleteStatement GetQuery();
	}
}
