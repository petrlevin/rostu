using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Common.Interfaces
{
    /// <summary>
    /// Версия (Системные измерения)
    /// </summary>
    public interface IVersion : IIdentitied
    {
        /// <summary>
        /// Наименование
        /// </summary>
        string Caption { get; }
    }
}
