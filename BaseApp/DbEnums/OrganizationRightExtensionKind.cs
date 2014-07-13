using System;
using Platform.Common;

namespace BaseApp.DbEnums
{
    /// <summary>
    /// Вид расширения организационных прав
    /// </summary>
    [Flags]
    public enum OrganizationRightExtensionKind
    {
        /// <summary>
        /// Чтение
        /// </summary>
        [EnumCaption("Чтение")]
        Read = 1,

        /// <summary>
        /// Запись
        /// </summary>
        [EnumCaption("Запись")]
        Write = 2,


        /// <summary>
        /// Запись и чтение
        /// </summary>
        [EnumCaption("Запись и чтение")]
        Both = 3
    }
}
