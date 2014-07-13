using System;
using System.Data.Entity;
using System.Reflection;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls.Interfaces
{
    public interface IControlInvocation
    {
        Action<DbContext, ControlType, Sequence, IBaseEntity, IBaseEntity> Action { get; }
        MemberInfo MemberInfo { get; }
        int ExecutionOrder { get; }
        IControlInfo InitialControlInfo { get; }
    }
}