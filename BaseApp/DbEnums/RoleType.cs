using Platform.Common;

namespace BaseApp.DbEnums
{
    /// <summary>
    /// Типы ролей
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// Системная
        /// </summary>
        [EnumCaption("Системная")]
        System = 0,

        /// <summary>
        /// Предустановленная
        /// </summary>
        [EnumCaption("Предустановленная")]
        Preset = 1,

        /// <summary>
        /// Пользовательская
        /// </summary>
        [EnumCaption("Пользовательская")]
        User = 2
    }
}
