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
    public class DefaultStrategy 
    {
        private readonly IControlInteraction _controlInteraction;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlInteraction"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultStrategy([Dependency] IControlInteraction controlInteraction)
        {
            if (controlInteraction == null) throw new ArgumentNullException("controlInteraction");
            _controlInteraction = controlInteraction;
        }

        internal protected virtual void Invoke(IControlInfo control, Action controlAction, string controlName)
        {
                controlAction();
        }


        internal protected virtual void InvokeSkippable(IControlInfo control, Action controlAction, string controlName)
        {
                Invoke(control, controlAction, controlName);
        }

        internal protected virtual bool MaySkipSkippable(ControlResponseException exception)
        {
            return _controlInteraction.MaySkip(exception);
        }

        internal protected virtual bool MaySkip(IControlInfo controlInfo)
        {
            return ((controlInfo != null) && (controlInfo.Skippable));
        }
    }

    class SomeStrategy: DefaultStrategy
    {
        public SomeStrategy(IControlInteraction controlInteraction) : base(controlInteraction)
        {
        }

        protected internal override void InvokeSkippable(IControlInfo control, Action controlAction, string controlName)
        {

        }

        protected internal override void Invoke(IControlInfo control, Action controlAction, string controlName)
        {
            if (controlName == "SomeControl")
                return;
            base.Invoke(control, controlAction, controlName);

        }
    }
}
