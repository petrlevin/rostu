using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.Utils.Extensions;

namespace Sbor.Tablepart
{
    public partial class FBA_DistributionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(ExcludeFromSetup = true, InitialSkippable = true)]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = -1000)]
        public void BeforeDelete(DataContext context)
        {
            if (context.FBA_CostActivities.Any(w => w.IdFBA_DistributionMethods == Id))
            {
                Controls.Throw("Все строки расходов, распределенные по удаляемому методу, будут удалены из документа");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = -900)]
        public void Delete(DataContext context)
        {
            var toDelete = context.FBA_CostActivities.Where(w => w.IdFBA_DistributionMethods == Id);
            if (toDelete.Any())
            {
                context.FBA_CostActivities.RemoveAll(toDelete);
            }
        }
    }
}
