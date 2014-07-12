using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    public partial class LongTermGoalProgram_ActivityResourceMaintenance_Value : IChildDenormalized, ITpAddValue
	{
        int ITpWithHierarchyPeriod.IdHierarchyPeriod
        {
            get
            {
                return IdHierarchyPeriod ?? 0;
            }
            set
            {
            }
        }
    }
}