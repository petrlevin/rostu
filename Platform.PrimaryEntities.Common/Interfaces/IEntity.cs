using System.Collections.Generic;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.PrimaryEntities.Common.Interfaces
{
    /// <summary>
    /// Класс сущности
    /// </summary>

    public interface IEntity :  IIdentitied ,IBaseEntity
    {
        /// <summary>
        /// Соедиение с базой данных
        /// </summary>

        #region Данные из БД

        /// <summary>
        /// Системное наименование
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        string Caption { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Идентификатор типа сущности
        /// </summary>
        
        byte IdEntityType { get; set; }

        /// <summary>
        /// Признак генерации сущностного класса для сущности
        /// </summary>
        bool GenerateEntityClass { get; set; }

        /// <summary>
        /// Системные сущности нельзя удалять через интерфейс системы. 
        /// Системные сущности создаются системой (триггерами, контролями и т.д.) и ею же удаляются.
        /// </summary>
        /// <remarks>
        /// Пример системных сущностей: 
        /// * Автоматически создаваемый справочник для каждого поля типа "ссылка в иерархии".
        /// </remarks>
        bool? IsSystem { get; set; }

        /// <summary>
        /// Идентификатор проекта к которому относится сущность
        /// </summary>
		int IdProject { get; set; }

		/// <summary>
		/// Признак версионности сущности
		/// </summary>
		bool IsVersioning { get; set; }

        #endregion

        #region Вычисляемая информация, не требующая ображения к БД
        
		/// <summary>
        /// Тип сущности
        /// </summary>
        EntityType EntityType { get; set; }

        /// <summary>
        /// Наименование схемы в БД которой принадлежит сущность
        /// </summary>
        string Schema { get; }


        #endregion

        

        #region Вычисляемая информация, требующая обращение к БД (кэшу)

        
        /// <summary>
		/// Список полей сущности
        /// </summary>
		IEnumerable<IEntityField> Fields
        {
            get; set;
        }

		/// <summary>
		/// Список полей сущности реально существующих в таблице
		/// </summary>
		IEnumerable<IEntityField> RealFields
		{
			get;
			//set;
		}

		/// <summary>
        /// Поле наименования сущности
        /// </summary>
        IEntityField CaptionField { get; }

        /// <summary>
        /// Поле описания сущности
        /// </summary>
        IEntityField DescriptionField { get; }

        #endregion








    }
}
