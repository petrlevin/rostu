using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls.DispatcherStrategies
{
    /// <summary>
    /// 
    /// </summary>
    public class SkipSkippableStrategy : DefaultStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlInteraction"></param>
        public SkipSkippableStrategy([Dependency]IControlInteraction controlInteraction)
            : base(controlInteraction)
        {
        }

        internal protected override bool MaySkipSkippable(ControlResponseException exception)
        {
            return true;
        }
    }
}
