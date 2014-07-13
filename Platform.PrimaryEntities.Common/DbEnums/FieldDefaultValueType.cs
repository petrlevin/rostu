namespace Platform.PrimaryEntities.Common.DbEnums
{
    /// <summary>
    /// Тип выражения, указанного в качестве значения по умолчанию
    /// </summary>
    public enum FieldDefaultValueType
    {
        /// <summary>
        /// Используется механизм default value sql-server'а.
        /// </summary>
        Sql = 1,

        /// <summary>
        /// Используется механизм сервера приложения
        /// </summary>
        Application = 2,

        /// <summary>
        /// Вычисляемое на sql-server'е выражение.
        /// </summary>
        SqlComputed = 3,

    }
}
