using System;

namespace Sbor.Interfaces
{
    /// <summary>
    /// Документ, имеющий поля с информацей о статусе  "Прекращён"
    /// </summary>
    public interface IDocStatusTerminate
    {
		/// <summary>
		/// Дата прекращения
		/// </summary>
        DateTime? DateTerminate { get; set; }

		/// <summary>
		/// Причина прекращения
		/// </summary>
        string ReasonTerminate { get; set; }
    }
}
