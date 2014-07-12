using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Interfaces;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;

namespace Sbor.Registry
{
    public partial class GoalTarget : RegistryEntity, ICommonRegister
    {
        public void Terminate(DataContext context, int idTerminator, int entityId, DateTime dateTerminate)
        {
            this.IdTerminator = idTerminator;
            this.IdTerminatorEntity = entityId;
            this.DateTerminate = dateTerminate;

            foreach (var rec in context.ValuesGoalTarget.Where(r => r.IdGoalTarget == this.Id))
            {
                rec.IdTerminator = idTerminator;
                rec.IdTerminatorEntity = entityId;
                rec.DateTerminate = dateTerminate;
            }
        }
    }
}
