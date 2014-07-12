using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;
using SBPTyp = Sbor.DbEnums.SBPType;
using BlankTyp = Sbor.DbEnums.BlankType;
using BlankValTyp = Sbor.DbEnums.BlankValueType;
using Platform.Utils.Extensions;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Tablepart
{
    public partial class SBP_BlankHistory : ISBP_Blank
	{
        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.SBP.SingleOrDefault(a => a.Id == IdOwner);

            if (Budget == null)
                Budget = context.Budget.SingleOrDefault(a => a.Id == IdBudget);
        }

	}
}