using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Sbor.Interfaces;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    partial class PublicInstitutionEstimate_Activity
    {
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Delete, Sequence.Before, 10)]
        public void AutoDelete(DataContext context)
        {
            foreach (var activity in context.PublicInstitutionEstimate_DistributionActivity.Where(e => e.IdOwner == IdOwner && e.IdPublicInstitutionEstimate_Activity == Id).ToList())
            {
                context.PublicInstitutionEstimate_DistributionActivity.Remove(activity);
            }
        }
    }
}
