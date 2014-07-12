using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Tools.MigrationHelper.DbManager.DbActions.Interfaces
{
	/// <summary>
	/// Действие над БД.
	/// Объект действия является результатом сравнения двух ОММ для одного элемента БД - строки (или строк) таблицы, UDF, ...
	/// </summary>
	public interface IDbAction
	{
		/// <summary>
		/// Действия, которые должны предшествовать данному
		/// </summary>
		List<IDbAction> DependsOn { get; }

		/// <summary>
		/// Строки, для каждой из которых необходимо совершить однотипное действие. Например вставить или удалить.
		/// </summary>
		IEnumerable<DataRow> Rows { get; }

		/// <summary>
		/// Возвращает SQL команду
		/// </summary>
		List<SqlCommand> GetCommand();

		/// <summary>
		/// Получить текстовое представления выполняемого действия
		/// </summary>
		/// <returns></returns>
		string Verbose();
	}
}
