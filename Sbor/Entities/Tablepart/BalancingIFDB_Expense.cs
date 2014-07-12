using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    /// <summary>
    /// Расходы
    /// </summary>
    public partial class BalancingIFDB_Expense
    {
        private class Distexp
        {
            public BalancingIFDB_EstimatedLine Line { get; set; }

            public int Num { get; set; }
        }

        private decimal DistExpence(IEnumerable<Distexp> details, int nYear, bool isAdd, decimal? distValue, decimal? oldValue, decimal? origValue)
        {
            if (distValue != oldValue)
            {
                bool isOrigBase = (oldValue ?? 0) == 0;

                decimal val = distValue ?? 0;

                var q = details.Where(w => w.Num == nYear && w.Line.IsAdditionalNeed == isAdd).OrderByDescending(o => isOrigBase ? o.Line.OldValue : o.Line.NewValue).ToList();
                foreach (var s in q)
                {
                    s.Line.NewValue = Math.Round((distValue ?? 0)*(isOrigBase ? (s.Line.OldValue ?? 0)/origValue.Value : (s.Line.NewValue ?? 0)/oldValue.Value), 2);
                    val -= s.Line.NewValue ?? 0;
                }

                if (val != 0 && q.Any())
                {
                    var rec = q.First();
                    rec.Line.NewValue = (rec.Line.NewValue ?? 0) + val;
                }
            }

            return (distValue ?? 0) - (oldValue ?? 0);
        }

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Update, Sequence.After, ExecutionOrder = -1500)]
        public void AutoSet(DataContext context, BalancingIFDB_Expense old)
        {
            List<Distexp> details =
                context.BalancingIFDB_EstimatedLine.Where(w => w.IdMaster == Id)
                    .Select(s => new Distexp
                    {
                        Line = s,
                        Num = s.HierarchyPeriod.Year - s.Owner.Budget.Year
                    }).ToList();

            decimal deltaOFG  = DistExpence(details, 0, false, ChangeOFG,  old.ChangeOFG,  OFG );
            decimal deltaPFG1 = DistExpence(details, 1, false, ChangePFG1, old.ChangePFG1, PFG1);
            decimal deltaPFG2 = DistExpence(details, 2, false, ChangePFG2, old.ChangePFG2, PFG2);
            decimal deltaAdditionalOFG  = DistExpence(details, 0, true, ChangeAdditionalOFG,  old.ChangeAdditionalOFG,  AdditionalOFG );
            decimal deltaAdditionalPFG1 = DistExpence(details, 1, true, ChangeAdditionalPFG1, old.ChangeAdditionalPFG1, AdditionalPFG1);
            decimal deltaAdditionalPFG2 = DistExpence(details, 2, true, ChangeAdditionalPFG2, old.ChangeAdditionalPFG2, AdditionalPFG2);

            var next = context.BalancingIFDB_Program.SingleOrDefault(w => w.Id == IdMaster);
            while (next != null)
            {
                next.ChangeOFG  = (next.ChangeOFG  ?? 0) + deltaOFG;  if (next.ChangeOFG  == 0) next.ChangeOFG  = null;
                next.ChangePFG1 = (next.ChangePFG1 ?? 0) + deltaPFG1; if (next.ChangePFG1 == 0) next.ChangePFG1 = null;
                next.ChangePFG2 = (next.ChangePFG2 ?? 0) + deltaPFG2; if (next.ChangePFG2 == 0) next.ChangePFG2 = null;
                next.ChangeAdditionalOFG  = (next.ChangeAdditionalOFG  ?? 0) + deltaAdditionalOFG;  if (next.ChangeAdditionalOFG  == 0) next.ChangeAdditionalOFG  = null;
                next.ChangeAdditionalPFG1 = (next.ChangeAdditionalPFG1 ?? 0) + deltaAdditionalPFG1; if (next.ChangeAdditionalPFG1 == 0) next.ChangeAdditionalPFG1 = null;
                next.ChangeAdditionalPFG2 = (next.ChangeAdditionalPFG2 ?? 0) + deltaAdditionalPFG2; if (next.ChangeAdditionalPFG2 == 0) next.ChangeAdditionalPFG2 = null;

                next = context.BalancingIFDB_Program.SingleOrDefault(w => w.Id == next.IdParent);
            }
        }
    }
}
