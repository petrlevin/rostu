namespace Platform.PrimaryEntities.DbEnums
{
	/// <summary>
	/// Тип поддержки ссылочного поля
	/// </summary>
	public enum ForeignKeyType
	{
		/// <summary>
		/// Без обеспечения ссылочной целостности
		/// </summary>
		WithOutForeignKey =1,

		/// <summary>
		/// С обеспечением поддержки ссылочной целостности
		/// </summary>
		ForeignKey =2,

		/// <summary>
		/// С обеспечением поддержки ссылочной целостности и каскадным удалением
		/// </summary>
		ForeignKeyWithCascadeDelete=3
	}
}
