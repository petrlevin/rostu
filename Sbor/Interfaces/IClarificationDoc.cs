using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.PrimaryEntities.Common.Interfaces
{
    /// <summary>
    /// Документ требует уточнения
    /// </summary>
	public interface IClarificationDoc : IHierarhy
    {
        /// <summary>
        /// Требует уточнения
        /// </summary>
        bool IsRequireClarification { get; set; }

        /// <summary>
        /// Причина уточнения
        /// </summary>
        string ReasonClarification { get; set; }
    }
}
