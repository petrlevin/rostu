using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.SqlObjectModel
{
	/// <summary>
	/// Абстарктный класс для построения конструкции SELECT, INSERT, UPDATE, DELETE, MERGE
	/// </summary>
	public abstract class BaseDml
	{
		/// <summary>
		/// Имя таблицы
		/// </summary>
		protected string TableName;

		/// <summary>
		/// Имя схемы, которой принадлежит таблица
		/// </summary>
		protected string SchemaName;

		/// <summary>
		/// Алиас присваиваемый таблице
		/// </summary>
		protected string AliasName;
	}
}
