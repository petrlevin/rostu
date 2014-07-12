using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Attributes;
using Platform.BusinessLogic.Common.Enums;
using Sbor.Interfaces;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    [SelectionWithNoChilds]
    partial class PublicInstitutionEstimate_Expense : ILineCostWithRelations, IDenormilizedExpense
    {
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, 5)]
        public void AutoSet(DataContext context)
        {
            if ( IdOwner == 0)
                return;
 
            if (!IsIndirectCosts.HasValue || !IsIndirectCosts.Value)
                IndirectCostsDistributionMethod = null;
            
            var owner = context.PublicInstitutionEstimate.Single(d => d.Id == IdOwner);
            owner.InvokeControl( c => c.Control_0710(context, Id) );
            owner.InvokeControl(c => c.Control_0727(context, this));            
        }

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, 5)]
        public void FillAOE(DataContext context)
        {
            if (!this.IdAuthorityOfExpenseObligation.HasValue)
            {
                Owner.CalculateRo_Expenses(context, new int[]{Id});
            }
        }

    }
}
