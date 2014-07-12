using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Tablepart
{
    public partial class Activity_CodeAuthority : TablePartEntity 
	{
        [Control(ControlType.Update | ControlType.Insert , Sequence.After, ExecutionOrder = 30)]
        public void AutoSetMain(DataContext context)
        {
            if (this.IsMain)
            {
                var qd = context.Activity_CodeAuthority.Where(w => w.IdOwner == this.IdOwner && w.Id != this.Id);
                foreach (var line in qd)
                {
                    line.IsMain = false;
                }

                context.SaveChanges();
            }

        }
    }
}

