using System.Collections.Generic;

namespace Platform.Dal.Common.Interfaces.QueryBuilders
{
	public interface IUpdateQueryBuilder : IDeleteQueryBuilder
	{
		/// <summary>
		/// Поля и значения.
		/// </summary>
		Dictionary<string, string> FieldValues { get; set; }
	}
}
