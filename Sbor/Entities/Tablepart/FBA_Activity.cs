using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.Utils.Extensions;
using Sbor.DbEnums;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Tablepart
{
    public partial class FBA_Activity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(ExcludeFromSetup = true, InitialSkippable = true)]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 2)]
        public void BeforeDelete(DataContext context)
        {
            if (context.FBA_ActivitiesDistribution.Any(w => w.IdFBA_Activity == Id) || context.FBA_PlannedVolumeIncome.Any(w => w.IdFBA_Activity == Id))
            {
                Controls.Throw("Строки ТЧ \"Мероприятия для распределения\" и ТЧ \"Плановые объемы поступлений\" относящиеся к данному мероприятию, будут удалены из документа");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 3)]
        public void Delete(DataContext context)
        {
            var toDelete = context.FBA_ActivitiesDistribution.Where(w => w.IdFBA_Activity == Id);
            var toDelete2 = context.FBA_PlannedVolumeIncome.Where(w => w.IdFBA_Activity == Id);
            if (toDelete.Any())
            {
                context.FBA_ActivitiesDistribution.RemoveAll(toDelete);
                context.FBA_PlannedVolumeIncome.RemoveAll(toDelete2);
            }
        }

        /// <summary>   
        /// Проверка удаляемых мероприятий
        /// </summary>         
        [ControlInitial(InitialUNK = "0904", InitialCaption = "Проверка удаляемых мероприятий")]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 1)]
        public void Control_0904(DataContext context)
        {
            if (!this.IsOwnActivity)
            {
                var doc = context.FinancialAndBusinessActivities.First(d => d.Id == IdOwner);
                if (context.TaskVolume.Any(
                    tv => tv.IdSBP == doc.IdSBP  
                          && tv.IdVersion == doc.IdVersion
                          && tv.IdBudget == doc.IdBudget
                          && !tv.IdTerminator.HasValue
                          && tv.IdValueType == (byte)ValueType.Plan
                          && tv.TaskCollection.IdActivity == IdActivity
                          && tv.TaskCollection.IdContingent == IdContingent
                          && tv.IdPublicLegalFormation == doc.IdPublicLegalFormation

                    ))
                    Controls.Throw("Не допускается удаление мероприятий, предусмотренных заданием учредителя.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ctType"></param>
        /// <param name="old"></param>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1000)]
        public void AutoSet(DataContext context, ControlType ctType, FBA_Activity old)
        {
            var ownerDoc = context.FinancialAndBusinessActivities.First(f => f.Id == IdOwner);

            var budget = context.Budget.FirstOrDefault(f => f.Id == ownerDoc.IdBudget);

            if (budget == null)
                throw new Exception(String.Format("Отсутствует Бюджет с id = {0}", ownerDoc.IdBudget));
            var budgetYear = budget.Year;

            var taskVolumes = context.TaskVolume.Where(w => w.IdPublicLegalFormation == ownerDoc.IdPublicLegalFormation
                                                && w.IdBudget == ownerDoc.IdBudget
                                                && w.IdVersion == ownerDoc.IdVersion
                                                && w.IdSBP == ownerDoc.IdSBP
                                                && w.IdValueType == (byte)ValueType.Plan
                                                && w.IdTerminator == null
                                                && w.HierarchyPeriod.Year >= budgetYear
                                                && w.HierarchyPeriod.Year <= budgetYear + 2)
                         .Select(s => s.IdTaskCollection).Distinct().ToList();

            var taskCollections = context.TaskCollection.Any(w => taskVolumes.Contains(w.Id) && w.IdActivity == IdActivity && w.IdContingent == IdContingent);

            if (taskCollections)
                IsOwnActivity = false;
        }
    }
}
