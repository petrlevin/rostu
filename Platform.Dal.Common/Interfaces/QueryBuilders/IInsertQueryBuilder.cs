namespace Platform.Dal.Common.Interfaces.QueryBuilders
{
	public interface IInsertQueryBuilder : IUpdateQueryBuilder
	{
		/// <summary>
		/// Возвращать значение идентификатора. После команды INSERT будет выполнена команда SELECT SCOPE_IDENTITY().
		/// </summary>
		bool ReturnIdentityValue { get; set; }
	}
}
