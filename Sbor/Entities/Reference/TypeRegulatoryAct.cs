using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Reference;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;

namespace Sbor.Reference
{
    public partial class TypeRegulatoryAct : ReferenceEntity 
	{
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_501401(DataContext context)
        {
            bool fail = context.TypeRegulatoryAct.Any(a =>
				(a.IdAccessGroup ?? 0) == (IdAccessGroup ?? 0)
                && a.Caption == Caption
                && a.Id != Id
            );

            if (fail)
            {
                AccessGroup accessGroup = context.AccessGroup.SingleOrDefault(a => a.Id == this.IdAccessGroup);

                Controls.Throw(string.Format(
                    "В группе доступа «{0}» уже имеется вид НПА «{1}»",
                    IdAccessGroup.HasValue ? accessGroup.Caption : string.Empty,
                    Caption
                ));

            }
        }
    }
}

