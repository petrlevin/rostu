namespace Platform.Dal.Common.Interfaces.QueryBuilders
{
	public interface IDeleteQueryBuilder : IQueryBuilder
	{
		/// <summary>
		/// Условия отбора. Как для выборки, так и для удаления. 
		/// При Вставке (Insert) игнорируется.
		/// </summary>
		IFilterConditions Conditions { get; set; }
	}
}
