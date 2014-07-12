using System;

namespace Sbor.Interfaces
{
    /// <summary>
    /// Сущность с полем-наименованием
    /// </summary>
    public interface IHasCaption
    {
        /// <summary>
        /// Наименование
        /// </summary>
        String Caption { get; set; }
    }
}
