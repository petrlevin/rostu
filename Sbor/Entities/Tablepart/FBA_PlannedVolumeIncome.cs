using System.Linq;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Attributes;
using Platform.BusinessLogic.Common.Enums;
using Sbor.DbEnums;

namespace Sbor.Tablepart
{
    [SelectionWithNoChilds]
    public partial class FBA_PlannedVolumeIncome
    {
        /// <summary>   
        /// Создание/обновление в ТЧ «Плановые объемы поступлений»
        /// </summary>         
        [ControlInitial(InitialUNK = "0903", InitialCaption = "Проверка строк поступлений")]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 1)]
        public void Control_0903(DataContext context)
        {
            var kfo = context.KFO.FirstOrDefault(f => f.Id == this.IdKFO);
            var financeSource = context.FinanceSource.FirstOrDefault(f => f.Id == this.IdFinanceSource);

            var fail = (kfo!= null && !kfo.IsIncludedInBudget) || (financeSource != null && financeSource.IdFinanceSourceType == (byte) FinanceSourceType.Remains);

            if (!fail)
            {
                Controls.Throw("Допускается вносить только данные по приносящей доход деятельности и остатки прошлых лет.");
            }
        }

        /// <summary>   
        /// Проверка строк поступлений
        /// </summary>         
        [ControlInitial(InitialUNK = "0923", InitialCaption = "Проверка строк поступлений")]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 2)]
        public void Control_0923(DataContext context)
        {
            var kfo = context.KFO.FirstOrDefault(f => f.Id == this.IdKFO);
            var activity = context.FBA_Activity.FirstOrDefault(f => f.Id == IdFBA_Activity);

            var fail = (kfo != null && kfo.IsIncludedInBudget) && (activity != null && activity.IsOwnActivity);

            if (fail)
            {
                Controls.Throw(string.Format("По собственной деятельности учреждения не допускается вносить поступления за счет средств, относящихся к средствам бюджета: <br> - Мероприятие: {0} <br> - КФО: {1}", activity.Activity.Caption, kfo.Caption));
            }
        }


        /// <summary>   
        /// Проверка остатков по целевым субсидиям
        /// </summary>         
        [ControlInitial(InitialUNK = "0924", InitialCaption = "Проверка остатков по целевым субсидиям")]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 3)]
        public void Control_0924(DataContext context)
        {
            var kfo = context.KFO.FirstOrDefault(f => f.Id == this.IdKFO);

            var fail = (kfo != null && (kfo.Code.Trim() == "5" || kfo.Code.Trim() == "6")) && (IdFBA_Activity == null);

            if (fail)
            {
                Controls.Throw("По остаткам средств от целевых субсидий необходимо указывать мероприятие.");
            }
        }
    }
}
