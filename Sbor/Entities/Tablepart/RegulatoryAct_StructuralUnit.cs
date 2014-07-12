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
    public partial class RegulatoryAct_StructuralUnit
	{
        private string AddContPart(string str, string hstr, string dstr, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                str += (string.IsNullOrEmpty(str) ? "" : dstr) + hstr + value;
            }

            return str;
        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            var str = string.Empty;

            str = AddContPart(str, "ст.",  " ", Article);
            str = AddContPart(str, "ч.",   " ", Part);
            str = AddContPart(str, "п.",   " ", Item);
            str = AddContPart(str, "п.п.", " ", SubItem);
            str = AddContPart(str, "абз.", " ", Paragraph);

            Caption = str;
        }
	}
}