namespace Platform.BusinessLogic.DbEnums
{
    /// <summary>
    /// Тип импорта
    /// </summary>
    public enum ImportType
    {
        /// <summary>
        /// Создавать новые и обновлять существующие
        /// </summary>
        CreateNewAndUpdateExist,

        /// <summary>
        /// Только обновлять существующие, новые не создавать
        /// </summary>
        UpdateExist,

        /// <summary>
        /// Только создавать, игнорировать существующие
        /// </summary>
        CreateNew,

        /// <summary>
        /// Только создавать, предупреждать о существующих
        /// </summary>
        CreateNewAndConfirmExist

    }
}
