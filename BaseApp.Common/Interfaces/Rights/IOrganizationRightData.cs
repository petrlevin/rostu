using System;
using System.Collections.Generic;
using System.Linq;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Common.Interfaces
{
    /// <summary>
    /// Организационные права
    /// </summary>
    public interface IOrganizationRightData
    {
        /// <summary>
        /// Основные права
        /// </summary>
        IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> Rights { get; }
        
        /// <summary>
        /// Расширения орг. прав
        /// </summary>
        IEnumerable<IOrganizationRightExtension> Extensions { get; }
        
        /// <summary>
        /// Доступ к элементу по ключу (имя сущности)
        /// </summary>
        /// <param name="entityId"></param>
        IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> this[Int32 entityId] { get; }
    }
}
