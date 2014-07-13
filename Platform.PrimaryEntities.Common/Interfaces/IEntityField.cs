using System;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.PrimaryEntities.Common.Interfaces
{
	public interface IEntityField
	{
		/// <summary>
		/// Идентификатор элемента
		/// </summary>
		int Id { get; set; }

		/// <summary>
		/// Системное наименование
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
		string Caption { get; set; }

		/// <summary>
		/// Тип поля
		/// </summary>
		EntityFieldType EntityFieldType { get; }

		/// <summary>
		/// Признак, что поле является полем "Наименование"
		/// </summary>
		bool? IsCaption { get; set; }

		/// <summary>
		/// Признак, что поле является полем "Описание"
		/// </summary>
		bool? IsDescription { get; set; }

        bool? IsSystem { get; set; }

		/// <summary>
		/// Идентификатор типа вычисляемого поля
		/// </summary>
		byte? IdCalculatedFieldType { get; }

		/// <summary>
		/// Тип вычисляемого поля
		/// </summary>
		CalculatedFieldType? CalculatedFieldType { get; }

		/// <summary>
		/// Идентификатор типа поля
		/// </summary>
		byte IdEntityFieldType { get; set; }

		/// <summary>
		/// Идентификатор сущности на которую ссылается поле
		/// </summary>
		int? IdEntityLink { get; set; }

		/// <summary>
		/// Идентификатор сущности, которой принадлежит поле
		/// </summary>
		int IdEntity { get; set; }

        /// <summary>
        /// Системное наименование сущности
        /// </summary>
		string EnityName { get; }

	    /// <summary>
		/// Допускается ли NULL значение
		/// </summary>
		bool AllowNull { get; set; }

        /// <summary>
        /// Признак "Только для чтения"
        /// </summary>
        bool? ReadOnly { get; set; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
		string DefaultValue { get; set; }

		/// <summary>
		/// Идентификатор поля которое ссылается на данную сущность
		/// </summary>
		int? IdOwnerField { get; set; }

        /// <summary>
        /// Сущность на которую ссылается поле
        /// </summary>
		IEntity EntityLink { get; }

		/// <summary>
		/// Размер поля
		/// </summary>
		Int16? Size { get; set; }

        /// <summary>
        /// Точность
        /// </summary>
        byte? Precision { get; set; }

        /// <summary>
        /// Валидатор
        /// </summary>
        string RegExpValidator { get; set; }

		/// <summary>
		/// Идентификатор типа значения по умолчанию
		/// </summary>
		byte IdFieldDefaultValueType { get; set; }

		/// <summary>
		/// Тип значения по умолчанию
		/// </summary>
		FieldDefaultValueType FieldDefaultValueType { get; }

		/// <summary>
		/// Выражение
		/// </summary>
		string Expression { get; set; }

        /// <summary>
        /// Тип поля в формате MS SQL
        /// </summary>
		string SqlType { get; }

        /// <summary>
        /// Всплывающая подсказка
        /// </summary>
		string Tooltip { get; set; }

        /// <summary>
        /// Сущность которой принадлежит поле
        /// </summary>
		IEntity Entity { get; }

		/// <summary>
		/// Идентификатор типа поддержки ссылочного поля
		/// </summary>
		byte? IdForeignKeyType { get; set; }

		/// <summary>
		/// Поле является вычисляемым на стороне БД
		/// </summary>
		bool IsDbComputed { get; }
	}
}
