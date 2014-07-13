using System;

namespace Platform.PrimaryEntities.Common.Interfaces
{
	/// <summary>
	/// Таблица метаданных.
	/// </summary>
	public interface IMetadata : IBaseEntity
	{
		Guid Id { get; set; }
		string Name { get; set; }
		string Caption { get; set; }
	}
}
