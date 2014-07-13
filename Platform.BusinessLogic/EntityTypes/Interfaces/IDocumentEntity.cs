using System;
using Platform.BusinessLogic.Interfaces;

namespace Platform.BusinessLogic.EntityTypes.Interfaces
{
	/// <summary>
	/// Документ
	/// </summary>
	public interface IDocumentEntity : IToolEntity
	{
        /// <summary>
        /// Номер документа
        /// </summary>
		string Number { get; set; }

		/// <summary>
		/// Дата
		/// </summary>
		DateTime Date { get; set; }
    }
}
