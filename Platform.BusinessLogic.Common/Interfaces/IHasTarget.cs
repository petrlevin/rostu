using System;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHasTarget
    {
        Type Target { get; }
    }
}