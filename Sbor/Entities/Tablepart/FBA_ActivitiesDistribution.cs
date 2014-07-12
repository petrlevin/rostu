using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Sbor.DbEnums;

namespace Sbor.Tablepart
{
    public partial class FBA_ActivitiesDistribution
    {
        /// <summary>   
        /// Проверка удаляемых мероприятий
        /// </summary>         
        [ControlInitial(InitialUNK = "0906", InitialCaption = "Проверка метода распределения")]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 1)]
        public void Control_0906(DataContext context)
        {

            var fbaActivity = context.FBA_Activity.FirstOrDefault(f => f.Id == this.IdFBA_Activity);
            var distMethod = context.FBA_DistributionMethods.FirstOrDefault(f => f.Id == this.IdMaster);

            if (fbaActivity.IsOwnActivity && distMethod.IndirectCostsDistributionMethod == IndirectCostsDistributionMethod.M3)
            {
                Controls.Throw("Указанный метод не применим к деятельности, не предусмотренной заданием учредителя.");
            }
        }
    }
}
