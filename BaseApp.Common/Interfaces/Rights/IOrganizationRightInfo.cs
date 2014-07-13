using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Common.Interfaces
{
    /// <summary>
    /// Информация о орг. правах
    /// </summary>
    public interface IOrganizationRightInfo
    {
        /// <summary>
        /// Поле сущности
        /// </summary>
        IEntityField Field { get; }

        /// <summary>
        /// Родительское поле
        /// </summary>
        IEntityField ParentField { get; }

        /// <summary>
        /// Идентификатор элемента
        /// </summary>
        int? IdElement { get; }

        /// <summary>
        /// Ссылка на сущность
        /// </summary>
        int IdElementEntity { get; }
        
        /// <summary>
        /// Сущность элменета
        /// </summary>
        IEntity ElementEntity { get; }
    }
}
