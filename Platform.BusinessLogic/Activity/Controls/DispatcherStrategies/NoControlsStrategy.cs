using System;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;

namespace Platform.BusinessLogic.Activity.Controls.DispatcherStrategies
{
    /// <summary>
    /// 
    /// </summary>
    public class NoControlsStrategy : DefaultStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlInteraction"></param>
        public NoControlsStrategy([Dependency]IControlInteraction controlInteraction)
            : base(controlInteraction)
        {
        }

        public NoControlsStrategy()
            : this(IoC.Resolve<IControlInteraction>())
        {
        }

        internal protected override void Invoke(IControlInfo control, Action controlAction, string controlName)
        {
        }
    }
}
