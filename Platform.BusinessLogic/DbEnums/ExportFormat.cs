using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.DbEnums
{
	/// <summary>
	/// Способ экспорта
	/// </summary>
	public enum ExportFormat
	{
		/// <summary>
		/// Текст
		/// </summary>
		Text,

		/// <summary>
		/// Xml
		/// </summary>
		Xml,

		/// <summary>
		/// Json
		/// </summary>
		Json,

		/// <summary>
		/// Файл
		/// </summary>
		Binary,

		/// <summary>
		/// Архив
		/// </summary>
		Zip
	}
}
