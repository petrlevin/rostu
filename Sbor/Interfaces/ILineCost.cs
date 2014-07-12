using Sbor.Logic;

namespace Sbor.Interfaces
{
    public interface ILineCost : ILine
    {
        byte? IdExpenseObligationType { get; set; }

        int? IdFinanceSource { get; set; }

        int? IdKFO { get; set; }

        int? IdKVSR { get; set; }

        int? IdRZPR { get; set; }

        int? IdKCSR { get; set; }

        int? IdKVR { get; set; }

        int? IdKOSGU { get; set; }

        int? IdDFK { get; set; }

        int? IdDKR { get; set; }

        int? IdDEK { get; set; }

        int? IdCodeSubsidy { get; set; }

        int? IdBranchCode { get; set; }
    }
}
