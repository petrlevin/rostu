using System;
using System.Data.Entity;
using System.Reflection;
using Platform.BusinessLogic.Activity.Controls.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ControlInvocation : IControlInvocation
    {
        public Action<DbContext, ControlType, Sequence, IBaseEntity, IBaseEntity> Action { get; set; }
        public MemberInfo MemberInfo { get; set; }
        public int ExecutionOrder { get; set; }
        public IControlInfo InitialControlInfo { get; set; }
    }
}