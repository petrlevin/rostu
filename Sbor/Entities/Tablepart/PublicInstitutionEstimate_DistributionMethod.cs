using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.Utils.Extensions;
using Sbor.Interfaces;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    partial class PublicInstitutionEstimate_DistributionMethod
    {
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Delete, Sequence.Before, 10)]
        public void AutoDelete(DataContext context)
        {
            using (new ControlScope())
            {
                context.PublicInstitutionEstimate_Expense.RemoveAll(
                    context.PublicInstitutionEstimate_Expense.Where(e => e.IdOwner == IdOwner && 
                                                                         e.IdIndirectCostsDistributionMethod.HasValue && 
                                                                         e.IdIndirectCostsDistributionMethod.Value == IdIndirectCostsDistributionMethod ).ToList());

                context.SaveChanges();
            }

            
            
        }
    }
}
