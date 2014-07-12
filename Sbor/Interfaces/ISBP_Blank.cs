using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Logic
{
    public interface ISBP_Blank
    {
        byte? IdBlankValueType_ExpenseObligationType { get; set; }

        byte IdBlankValueType_FinanceSource { get; set; }

        byte? IdBlankValueType_KFO { get; set; }

        byte? IdBlankValueType_KVSR { get; set; }

        byte? IdBlankValueType_RZPR { get; set; }

        byte? IdBlankValueType_KCSR { get; set; }

        byte? IdBlankValueType_KVR { get; set; }

        byte? IdBlankValueType_KOSGU { get; set; }

        byte? IdBlankValueType_DFK { get; set; }

        byte? IdBlankValueType_DKR { get; set; }

        byte? IdBlankValueType_DEK { get; set; }

        byte? IdBlankValueType_CodeSubsidy { get; set; }

        byte? IdBlankValueType_BranchCode { get; set; }
    }
}
