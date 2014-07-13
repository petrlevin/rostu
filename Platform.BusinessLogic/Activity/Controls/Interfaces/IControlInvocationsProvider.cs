using System;
using System.Collections.Generic;
using System.Reflection;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls.Interfaces
{
    public interface IControlInvocationsProvider
    {
        IEnumerable<IControlInvocation> GetInvocations(ControlType controlType, Sequence sequence, Type entityType);

    }
}
