namespace Platform.PrimaryEntities.DbEnums
{
	/// <summary>
	/// Тип программируемого объекта
	/// </summary>
	/// <remarks>
    /// Порядок важен! используется в Tools.MigrationHelper.Core.AssemblyDeploy
	/// </remarks>
	public enum ProgrammabilityType
	{
		/// <summary>
		/// Хранимая процедура
		/// </summary>
		StoredProcedure = 1,

		/// <summary>
		/// UDF
		/// </summary>
		Function = 2,

        /// <summary>
        /// Триггер
        /// </summary>
        Trigger = 3 ,

        /// <summary>
        /// Агрегат
        /// </summary>
        Aggreagate = 4,
        
		/// <summary>
        /// Представление
        /// </summary>
        View = 5
	}
}
