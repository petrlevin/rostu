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
    partial class PublicInstitutionEstimate_FounderAUBUExpense : ILineCostWithRelations, IDenormilizedExpense 
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, 1)]
        [ControlInitial(ExcludeFromSetup = true)]
        public void AutoSet(DataContext context)
        {
            var doc = context.PublicInstitutionEstimate.First(d => d.Id == IdOwner);
            doc.InvokeControl(e => e.Control_0716(context, this) );
        }

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, 5)]
        public void FillAOE(DataContext context)
        {
            if (!this.IdAuthorityOfExpenseObligation.HasValue)
            {
                Owner.calculateRo_FounderAUBUExpenses(context, new int[Id]);
            }
        }

    }
}
