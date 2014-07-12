using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.Utils.Common;



namespace Sbor.Tablepart
{
	/// <summary>
	/// Объемы мероприятий
	/// </summary>
	public partial class PlanActivity_ActivityVolume : TablePartEntity   
	{
        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.PlanActivity.SingleOrDefault(a => a.Id == IdOwner);

            if (HierarchyPeriod == null)
                HierarchyPeriod = context.HierarchyPeriod.SingleOrDefault(a => a.Id == IdHierarchyPeriod);
        }

        /// <summary>   
        /// Контроль "Проверка  периодов у объемов мероприятий"
        /// </summary>         
        [ControlInitial(InitialUNK = "0603/1", InitialCaption = "Проверка  периодов у объемов мероприятий")]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 10)]
        public void Control_0603(DataContext context)
        {
            InitMaps(context);

            List<string> list = context.PlanActivity_ActivityVolume.Where(w =>
                w.IdOwner == IdOwner && w.Id != Id && w.HierarchyPeriod.Year == HierarchyPeriod.Year
                && (w.HierarchyPeriod.DateEnd.Month - w.HierarchyPeriod.DateStart.Month) != (HierarchyPeriod.DateEnd.Month - HierarchyPeriod.DateStart.Month)
            ).Select(s => s.HierarchyPeriod.Caption).ToList();

            Controls.Check(list, string.Format(
                "Несоответствие периодов. Ранее были введены объемы с другими типами периодов на {0} год:<br>{{0}}",
                HierarchyPeriod.Year
            ));
        }
    }
}