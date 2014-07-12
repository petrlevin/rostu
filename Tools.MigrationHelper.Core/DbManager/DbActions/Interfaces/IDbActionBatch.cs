using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Tools.MigrationHelper.Core.DbManager.DbActions.Interfaces
{
	/// <summary>
	/// Действие над БД.
	/// Объект действия является результатом сравнения двух ОММ для одного элемента БД - строки (или строк) таблицы, UDF, ...
	/// </summary>
	public interface IDbActionBatch
	{
		/// <summary>
		/// Действия, которые должны предшествовать данному
		/// </summary>
        List<IDbActionBatch> DependsOn { get; }

		/// <summary>
		/// Строки, для каждой из которых необходимо совершить однотипное действие. Например вставить или удалить.
		/// </summary>
		List<DbAction> Actions { get; }

//		/// <summary>
//		/// Возвращает SQL команду
//		/// </summary>
//		List<SqlCommand> GetCommand();

        /// <summary>
        /// Возвращает SQL команду
        /// </summary>
        List<SqlCommand> GetCommand(DataRow row);

		/// <summary>
		/// Получить текстовое представления выполняемого действия
		/// </summary>
		/// <returns></returns>
		string Verbose();
	}
}
