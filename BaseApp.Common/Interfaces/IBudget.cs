using System;

namespace BaseApp.Common.Interfaces
{
	/// <summary>
	/// Бюджет для системных измерений
	/// </summary>
	public interface IBudget
	{
        /// <summary>
        /// Идентификатор бюджета
        /// </summary>
        Int32 Id { get; }

        /// <summary>
        /// Публично-правовое образование
        /// </summary>
        int IdPublicLegalFormation { get; }

	    /// <summary>
	    /// Год
	    /// </summary>
	    Int32 Year { get; }

	    /// <summary>
        /// Наименование
        /// </summary>
        string Caption { get; }
	}
}
