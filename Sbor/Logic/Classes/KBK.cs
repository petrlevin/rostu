using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sbor.Interfaces;

namespace Sbor.Logic
{
    public abstract class KBK : ILineCost
    {
        public byte? IdExpenseObligationType { get; set; }
        public int? IdFinanceSource { get; set; }
        public int? IdKFO { get; set; }
        public int? IdKVSR { get; set; }
        public int? IdRZPR { get; set; }
        public int? IdKCSR { get; set; }
        public int? IdKVR { get; set; }
        public int? IdKOSGU { get; set; }
        public int? IdDFK { get; set; }
        public int? IdDKR { get; set; }
        public int? IdDEK { get; set; }
        public int? IdCodeSubsidy { get; set; }
        public int? IdBranchCode { get; set; }
    }
}
