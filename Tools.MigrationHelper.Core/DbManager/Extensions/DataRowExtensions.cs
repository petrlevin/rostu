using System.Collections.Generic;
using System.Data;
using Tools.MigrationHelper.Core.DbManager.DbActions;
using Tools.MigrationHelper.Core.DbManager.DbActions.Interfaces;

namespace Tools.MigrationHelper.Core.DbManager.Extensions
{
	public static class DataRowExtensions
	{
	    /// <summary>
	    /// Для строки таблицы возвращает действие по ее созданию
	    /// </summary>
	    /// <param name="rows"></param>
	    /// <param name="fieldsName"></param>
	    /// <returns></returns>
	    public static IDbActionBatch GetInsert(this IEnumerable<DataRow> rows, List<string> fieldsName = null)
		{
			return new InsertRows(rows, fieldsName);
		}

	    /// <summary>
	    /// Для строк двух наборов производит сравнение и в случае необходимости формирует команды на изменение
	    /// </summary>
	    /// <param name="sourceRows">Набор строк из текущей версии обновления</param>
	    /// <param name="targetFsRows">Набор строк из предыдущей версии обновления</param>
	    /// <param name="targetDbRows">Набор строк из обновляемой базы</param>
	    /// <param name="finalStateRows">Набор строк из финального состояния</param>
	    /// <returns></returns>
	    public static IDbActionBatch GetUpdate(this IEnumerable<DataRow> sourceRows, IEnumerable<DataRow> targetFsRows, IEnumerable<DataRow> targetDbRows, IEnumerable<DataRow> finalStateRows)
		{
			return new UpdateRows(sourceRows, targetFsRows, targetDbRows, finalStateRows);
		}

		/// <summary>
		/// Получение действия удаления строк
		/// </summary>
		/// <param name="rows">Удаляемые строки</param>
		/// <returns></returns>
        public static IDbActionBatch GetDelete(this IEnumerable<DataRow> rows)
		{
			return new DeleteRows(rows);
		}


	}
}
