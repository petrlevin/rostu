using Platform.Common;

namespace BaseApp.DbEnums
{
    /// <summary>
    /// Виды ролей
    /// </summary>
    public enum RoleKind
    {
        /// <summary>
        /// Функциональная роль
        /// </summary>
        [EnumCaption("Функциональная роль")]
        FunctionalRole = 1,

        /// <summary>
        /// Организационная роль
        /// </summary>
        [EnumCaption("Организационная роль")]
        OrganizationalRole = 2
    }
}
