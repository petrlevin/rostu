using System;
using System.Collections.Generic;
using System.Data;

namespace Platform.PrimaryEntities.Common.Interfaces
{
	/// <summary>
	/// Интерфейс, реализуемый любой сущностью.
	/// Не содержит id.
	/// </summary>
	public interface IBaseEntity : IEquatable<IBaseEntity>
	{
		/// <summary>
		/// Получить объект сущности из строки таблицы
		/// </summary>
		/// <param name="row"></param>
		void FromDataRow(DataRow row);

		/// <summary>
		/// Получить объект из словаря. Поле-Значение
		/// </summary>
		/// <param name="values"></param>
		void FromDictionary(IDictionary<string, object> values);

		/// <summary>
		/// Получить объект из записи SqlDataReader'а
		/// </summary>
		/// <param name="record">Запись</param>
		void FromDataRecord(IDataRecord record);

        /// <summary>
        /// Идентификатор типа сущности
        /// </summary>
        int EntityId { get; }

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
        string EntityCaption { get; }
	}
}
