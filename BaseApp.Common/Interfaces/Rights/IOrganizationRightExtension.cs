using System.Collections.Generic;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Common.Interfaces
{
    /// <summary>
    /// Расширение орг. прав
    /// </summary>
    public interface IOrganizationRightExtension
    {
        /// <summary>
        /// Типы элементов к которым применяются правила
        /// </summary>
        IEnumerable<IEntity> Entities { get; }
        
        /// <summary>
        /// Шаблон запроса, расширяющий выборку
        /// </summary>
        string SqlTemplate { get; }
    }
}
