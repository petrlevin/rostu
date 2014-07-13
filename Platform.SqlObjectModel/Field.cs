using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Класс, для описания поля
	/// </summary>
	public class Field
	{
		/// <summary>
		/// Выражение
		/// </summary>
		public Expression Experssion;

		/// <summary>
		/// Алиас поля
		/// </summary>
		public string Alias;
	}
}
