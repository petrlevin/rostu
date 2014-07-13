using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Interfaces;
using Platform.Unity;
using Microsoft.Practices.Unity;



namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ControlScope<T> : ControlScope where T : DispatcherStrategies.DefaultStrategy
    {
        /// <summary>
        /// 
        /// </summary>
        public ControlScope()
        {
            Register();
        }

        private void Register()
        {
            UnityContainer.RegisterType(typeof (DispatcherStrategies.DefaultStrategy), typeof (T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        public ControlScope(Func<bool> condition , ScopeOptions options):base(condition , options ==ScopeOptions.ApplyExecutingAndDispatching)
        {
            if (condition())
                Register();


        }

        public ControlScope(Func<bool> condition,SpecificControlLauncher.Settings settings)
            : base(condition, settings)
        {
            if (condition())
                Register();


        }

        public ControlScope(SpecificControlLauncher.Settings settings)
            : this(()=>true, settings)
        {


        }




    }
}
