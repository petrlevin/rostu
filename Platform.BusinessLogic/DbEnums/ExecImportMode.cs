namespace Platform.BusinessLogic.DbEnums
{
    /// <summary>
    /// Способ выполнения импорта
    /// </summary>
    public enum ExecImportMode
    {
        /// <summary>
        /// Выдавать исключение, прерывать импорт
        /// </summary>
        ReportErrorAndStop = 1,

        /// <summary>
        /// Использовать первый элемент из набора с одинаковым наименованием
        /// </summary>
        UseFirstElementOfTheSame = 2

    }
}
