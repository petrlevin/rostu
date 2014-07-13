using Platform.Common;
using Platform.PrimaryEntities.Common;

namespace Platform.PrimaryEntities.DbEnums
{
    /// <summary>
    /// Статусы справочников
    /// </summary>
    [ClientEnum]
    public enum RefStatus
    {
        /// <summary>
        /// Новый
        /// </summary>
        [EnumCaption("Новый")] New = 1,

        /// <summary>
        /// В работе
        /// </summary>
        [EnumCaption("В работе")] Work = 2,

        /// <summary>
        /// Архив
        /// </summary>
        [EnumCaption("Архив")] Archive = 4

    }
}
