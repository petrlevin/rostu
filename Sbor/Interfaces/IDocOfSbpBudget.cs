using Platform.PrimaryEntities.Common.Interfaces;

namespace Sbor.Interfaces
{
    /// <summary>
    /// Документ требует уточнения
    /// </summary>
    public interface IDocOfSbpBudget : IClarificationDoc
    {
        int IdSBP { get; set; }
        int IdBudget { get; set; }
    }
}
