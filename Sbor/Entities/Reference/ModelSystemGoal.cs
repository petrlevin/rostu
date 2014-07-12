using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Reference
{
    public partial class ModelSystemGoal : ReferenceEntity 
	{
        private void InitMaps2(DataContext context, ModelSystemGoal obj)
        {
            if (obj.Parent == null && obj.IdParent.HasValue)
                obj.Parent = context.ModelSystemGoal.SingleOrDefault(a => a.Id == obj.IdParent);

            if (obj.ElementTypeSystemGoal == null)
                obj.ElementTypeSystemGoal = context.ElementTypeSystemGoal.SingleOrDefault(a => a.Id == obj.IdElementTypeSystemGoal);
        }

        private void InitMaps(DataContext context)
        {
            InitMaps2(context, this);
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_500901(DataContext context)
        {
            InitMaps(context);

            bool fail = context.ModelSystemGoal.Any(a =>
				a.IdPublicLegalFormation == IdPublicLegalFormation
				&& (a.IdParent ?? 0) == (IdParent ?? 0)
				&& a.IdElementTypeSystemGoal == IdElementTypeSystemGoal
                && a.IdRefStatus != (byte)RefStats.Archive
                && a.Id != Id
            );
            if (fail)
                Controls.Throw(string.Format(
                    "В справочнике уже имеется актуальная связь {0} - {1}.",
                    Parent != null ? Parent.ElementTypeSystemGoal.Caption : "пусто",
                    ElementTypeSystemGoal.Caption
                ));
        }

        [Control(ControlType.Update | ControlType.Delete, Sequence.Before, ExecutionOrder = 20)]
        public void Control_500902(DataContext context, ModelSystemGoal oModelSystemGoal, ControlType controlType)
        {
            ModelSystemGoal old = oModelSystemGoal;

            InitMaps(context);

            if (controlType == ControlType.Delete)
            {
                old = this;
            }
            else
            {
                InitMaps2(context, old);
			    if (old.IdElementTypeSystemGoal == IdElementTypeSystemGoal && (old.IdParent ?? 0) == (IdParent ?? 0))
                {
                    return;
                }
            }

            int idParentType = (old.Parent != null ? old.Parent.IdElementTypeSystemGoal : 0);
            List<string> list = context.SystemGoal.Where(a =>
                a.IdRefStatus == (byte) RefStats.Work 
				&& a.IdElementTypeSystemGoal == old.IdElementTypeSystemGoal
                && (a.IdParent.HasValue ? a.Parent.IdElementTypeSystemGoal : 0) == idParentType
            ).Select(s => s.Caption).ToList();

            if (list.Any())
            {
                Controls.Check(list, string.Format(
                    "В справочнике «Система целеполагания» обнаружены элементы с типом «{0}», у которых указан в качестве вышестоящего указан элемент с типом «{1}»:<br>{{0}}",
                    old.ElementTypeSystemGoal.Caption,
                    (old.Parent != null ? old.Parent.ElementTypeSystemGoal.Caption : "пусто")
                ));
            }
        }
    }
}

