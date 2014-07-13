using System;
using System.Linq;
using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.PrimaryEntities.DbEnums;

namespace BaseApp.Reference
{
    public partial class PublicLegalFormationModule : ReferenceEntity
    {
        [Control(ControlType.Update | ControlType.Insert, Sequence.After, ExecutionOrder = 10)]
        public void UpdatePublicLegalFormationModule(DataContext context)
        {
            PublicLegalFormation.UsedGMZ = !(IncludeModule.Any(a => a.Id == -1610612703));
            context.SaveChanges();
        }
    }
}
