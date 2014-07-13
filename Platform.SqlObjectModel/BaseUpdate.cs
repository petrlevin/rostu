using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Абстарктный класс для построения конструкции UPDATE
	/// </summary>
	public abstract class BaseUpdate : BaseDml
	{
		/// <summary>
		/// Изменяемые поля
		/// </summary>
		protected IEnumerable<string> FieldsName;

		/// <summary>
		/// Метод, осуществяющий построение запроса
		/// </summary>
		/// <returns></returns>
		public abstract UpdateStatement GetQuery();
	}
}
