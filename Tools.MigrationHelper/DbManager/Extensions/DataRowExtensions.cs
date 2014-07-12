using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Tools.MigrationHelper.DbManager.DbActions;
using Tools.MigrationHelper.DbManager.DbActions.Interfaces;

namespace Tools.MigrationHelper.DbManager.Extensions
{
	public static class DataRowExtensions
	{
		/// <summary>
		/// Для строки таблицы возвращает действие по ее созданию
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		public static IDbAction GetInsert(this IEnumerable<DataRow> rows, List<string> fieldsName=null)
		{
			return new InsertRows(rows, fieldsName);
		}

		/// <summary>
		/// Для строк двух наборов производит сравнение и в случае необходимости формирует команды на изменение
		/// </summary>
		/// <param name="sourceRows">Набор строк из текущей версии обновления</param>
		/// <param name="targetFsRows">Набор строк из предыдущей версии обновления</param>
		/// <param name="targetDbRows">Набор строк из обновляемой базы</param>
		/// <returns></returns>
		public static IDbAction GetUpdate(this IEnumerable<DataRow> sourceRows, IEnumerable<DataRow> targetFsRows, IEnumerable<DataRow> targetDbRows)
		{
			return new UpdateRows(sourceRows, targetFsRows, targetDbRows);
		}

		/// <summary>
		/// Получение действия удаления строк
		/// </summary>
		/// <param name="rows">Удаляемые строки</param>
		/// <returns></returns>
		public static IDbAction GetDelete(this IEnumerable<DataRow> rows)
		{
			return new DeleteRows(rows);
		}
	}
}
