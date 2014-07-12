using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    public partial class ActivityOfSBP_Activity 
	{
        /// <summary>   
        /// Контроль "Проверка наличия удаляемого мероприятия на вкладке «Спрос и мощность»"
        /// </summary> 
        [ControlInitial(InitialSkippable = true, InitialCaption = "Проверка наличия удаляемого мероприятия на вкладке «Спрос и мощность»", InitialUNK = "0338")]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 50)]
        public void Control_0338(DataContext context)
        {
            var sMsg = "Данное мероприятие также будет удалено из таблицы «Спрос и мощность».";

            if (context.ActivityOfSBP_ActivityDemandAndCapacity.Where(r => r.IdOwner == this.IdOwner && r.IdActivity == this.Id).Any())
            {
                Controls.Throw(sMsg);
            }
        }

        /// <summary>   
        /// обработка при добавлении Мероприятия - заполнять ТЧ Показатели качества мероприятий
        /// </summary> 
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert, Sequence.After, ExecutionOrder = 50)]
        public void FillIndicatorQualityActivity(DataContext context)
        {
            var doc = context.ActivityOfSBP.Where(d => d.Id == this.IdOwner).FirstOrDefault();

            var ia =
                context.Activity_Indicator.Where(w =>
                                                 w.IdSBP == doc.IdSBP &&
                                                 w.IdOwner == this.IdActivity &&
                                                 w.IndicatorActivity.IdIndicatorActivityType == (int) IndicatorActivityType.QualityIndicator).ToList();

            var year = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Year;
            var dateStart = new DateTime(year, 1, 1);

            var hp = context.HierarchyPeriod.Where(h => h.DateStart == dateStart && !h.IdParent.HasValue).FirstOrDefault();

            foreach (var activityIndicator in ia)
            {
                var newIndicatorQualityActivity = new ActivityOfSBP_IndicatorQualityActivity()
                    {
                        Master = this,
                        Owner = this.Owner,
                        IndicatorActivity = activityIndicator.IndicatorActivity
                    };

                context.ActivityOfSBP_IndicatorQualityActivity.Add(newIndicatorQualityActivity);
                context.SaveChanges();

                var newActivityOfSBP_IndicatorQualityActivity_Value = new ActivityOfSBP_IndicatorQualityActivity_Value()
                    {
                        Owner = this.Owner,
                        Master = newIndicatorQualityActivity,
                        HierarchyPeriod = hp
                    };

                context.ActivityOfSBP_IndicatorQualityActivity_Value.Add(newActivityOfSBP_IndicatorQualityActivity_Value);
                context.SaveChanges();
            }

            context.SaveChanges();

        }
	}
}