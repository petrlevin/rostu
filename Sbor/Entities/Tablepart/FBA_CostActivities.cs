using System.Linq;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Attributes;
using Platform.BusinessLogic.Common.Enums;
using Sbor.Interfaces;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    [SelectionWithNoChilds]
    public partial class FBA_CostActivities : ILineCostWithRelations
    {
        /// <summary>   
        /// Проверка удаляемых мероприятий
        /// </summary>         
        [ControlInitial(InitialUNK = "0905", InitialCaption = "Проверка КФО по собственной деятельности")]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 1)]
        public void Control_0905(DataContext context)
        {
            var kfo = context.KFO.FirstOrDefault(f => f.Id == this.IdKFO);
            var fbaActivity = context.FBA_Activity.FirstOrDefault(f => f.Id == this.IdMaster);

            if (fbaActivity.IsOwnActivity && kfo != null && kfo.IsIncludedInBudget)
            {
                Controls.Throw(string.Format("Мероприятие {0} не предусмотрено в плане деятельности учреждения. Не допускается вносить расходы за счет средств, отнесенных к бюджетным, с КФО '{1}-{2}'.",fbaActivity.Activity.Caption, kfo.Code, kfo.Caption));
            }
        }
    }
}
