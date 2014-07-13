using System.Collections.Generic;

namespace Platform.DbClr.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// О значении таблиц INSERTED и DELETED: http://msdn.microsoft.com/ru-ru/library/ms191300.aspx
    /// </remarks>
    /// <typeparam name="T"></typeparam>
	public interface ITriggerAction<T> 
	{
		/// <summary>
        /// Релизация триггера INSERT
		/// </summary>
		/// <param name="inserted">Список вставленных строк</param>
        void ExecInsertCmd(List<T> inserted);

		/// <summary>
        /// Релизация триггера UPDATE
		/// </summary>
		/// <param name="inserted">Список измененных строк с новыми значениями после изменений</param>
		/// <param name="deleted">Список измененных строк с оригинальными значениями до изменений</param>
        void ExecUpdateCmd(List<T> inserted, List<T> deleted);

        /// <summary>
        /// Релизация триггера DELETE
        /// </summary>
        /// <param name="deleted">Список удаленых экземпляров сущности</param>
        void ExecDeleteCmd(List<T> deleted);
	}
}
