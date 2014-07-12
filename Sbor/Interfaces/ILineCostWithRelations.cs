using Sbor.DbEnums;
using Sbor.Reference;

namespace Sbor.Interfaces
{
    /// <summary>
    /// Строка с КБК и мапингом
    /// </summary>
    public interface ILineCostWithRelations : ILineCost
    {
        ExpenseObligationType? ExpenseObligationType { get; }

        FinanceSource FinanceSource { get; set; }

        KFO KFO { get; set; }

        KVSR KVSR { get; set; }

        RZPR RZPR { get; set; }

        KCSR KCSR { get; set; }

        KVR KVR { get; set; }

        KOSGU KOSGU { get; set; }

        DFK DFK { get; set; }

        DKR DKR { get; set; }

        DEK DEK { get; set; }

        CodeSubsidy CodeSubsidy { get; set; }

        BranchCode BranchCode { get; set; }
    }
}
