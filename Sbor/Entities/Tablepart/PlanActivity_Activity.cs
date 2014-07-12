using System.Linq;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Interfaces;


namespace Sbor.Tablepart
{
	/// <summary>
	/// Мероприятия
	/// </summary>
    public partial class PlanActivity_Activity : TablePartEntity, IPlanActivity_ActivityTps
	{
        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.PlanActivity.SingleOrDefault(a => a.Id == IdOwner);

            if (Activity == null)
                Activity = context.Activity.SingleOrDefault(a => a.Id == IdActivity);

            if (Contingent == null)
                Contingent = context.Contingent.SingleOrDefault(a => a.Id == IdContingent);
        }

        /// <summary>   
        /// Контроль "Наличие удаляемого мероприятия в смете учреждения или в ПФХД"
        /// </summary>         
        [ControlInitial(InitialUNK = "0628/1", InitialCaption = "Наличие удаляемого мероприятия в смете учреждения или в ПФХД", InitialManaged = true)]
        [Control(ControlType.Delete | ControlType.Update, Sequence.Before, ExecutionOrder = 10)]
        public void Control_0628(DataContext context, ControlType ct, PlanActivity_Activity old)
        {
            var oval = (ct == ControlType.Delete ? this : old);

            if (ct == ControlType.Update && oval.IdActivity == IdActivity && oval.IdContingent == IdContingent)
                return;

            InitMaps(context);

            var vtyp = (Owner.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment
                            ? DbEnums.ValueType.Justified
                            : DbEnums.ValueType.JustifiedFBA);

            bool fail = (context.LimitVolumeAppropriations.Where(w =>
                w.IdVersion == Owner.IdVersion
                && w.IdBudget == Owner.IdBudget
                && w.TaskCollection.IdActivity == oval.IdActivity
                && (w.TaskCollection.IdContingent ?? 0) == (oval.IdContingent ?? 0)
                && !w.IsMeansAUBU
                && w.EstimatedLine.IdSBP == Owner.IdSBP
                && w.IdValueType == (byte)vtyp
            ).Sum(c => (decimal?)c.Value) ?? 0) != 0;

            if (fail)
            {
                Controls.Throw(string.Format(
                    "Следующее мероприятие невозможно удалить, так как оно входит в документ «{0}» по учреждению:<br>{1}{2}",
                    (Owner.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment ? "Смета казенного учреждения" : "ПФХД"),
                    Activity.Caption,
                    (Contingent == null ? "" : " - " + Contingent.Caption)
                ));
            }
        }
    }
}