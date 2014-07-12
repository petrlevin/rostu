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
    public partial class ElementTypeSystemGoal : ReferenceEntity 
	{
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            var q = context.ElementTypeSystemGoal_Document.Where(w => w.IdOwner == Id).Select(s => s.DocType.Caption).Distinct();

            Description = string.Join(", ", q.ToArray());
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_500801(DataContext context)
        {
            bool fail = context.ElementTypeSystemGoal.Any(a =>
				a.IdPublicLegalFormation == IdPublicLegalFormation
                && a.Caption == Caption
                && a.IdRefStatus != (byte)RefStats.Archive
                && a.Id != Id
            );
            if (fail)
                Controls.Throw(string.Format(
                    "В справочнике уже имеется актуальный тип «{0}».", Caption
                ));
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 20)]
        public void Control_500802(DataContext context)
        {
            if (this.IdRefStatus == (byte)RefStats.Work)
            {
                if (!context.ElementTypeSystemGoal_Document.Any(a => a.IdOwner == Id))
                    Controls.Throw("Необходимо указать хотя бы один документ.");
            }
        }
    }
}

