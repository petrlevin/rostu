using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls.DispatcherStrategies;
using Platform.BusinessLogic.Interfaces;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Unity;
using Microsoft.Practices.Unity;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// Контекст выполнения контролей. 
    /// Внутри контекста действует определенная стратегия выполнения контролей.
    /// <a href="http://conf.rostu-comp.ru/pages/viewpage.action?pageId=13599219">Контроли</a>.
    /// </summary>
    /// <remarks>
    /// Я не стал применять терми "Область видимости выполнения контролей", 
    /// т.к. считаю, что по семантике термин 'контекст' в данном случае более точно отражает смысл класса.
    /// </remarks>
    public class ControlScope : ScopeBase
    {
        /// <summary>
        /// Создает новую область видимости выполнения контролей если не была установлена раньше
        /// </summary>
        /// <returns></returns>
        public static ControlScope IfNotSetYet()
        {
            IControlLauncher cl;
            try
            {
                cl = ControlLauncher.Current;
            }
            catch (Exception)
            {

                return new ControlScope();
            }

            return new ControlScope(()=>(cl == null) || (cl is Striker));
        }

        public static ControlScope IfNotSetYet(IBaseEntity element)
        {
            IControlLauncher cl;
            try
            {
                cl = ControlLauncher.Current;
            }
            catch (Exception)
            {
                return new ControlScope(element);
            }

            return new ControlScope(element,() => (cl == null) || (cl is Striker));
        }

        /// <summary>
        /// Вcе CUD операции будут выполнятся с контролями
        /// </summary>
        public ControlScope():this(()=>true)
        {
        }

        /// <summary>
        /// Вcе CUD операции будут выполнятся с контролями если условие <paramref name="condition"/> возвращает истину
        /// </summary>
        public ControlScope(Func<bool> condition):this(condition,true)
        {
        }

        protected ControlScope(Func<bool> condition, bool registerLauncher)
            : base(condition)
        {
            if (registerLauncher && condition())
                UnityContainer.RegisterType(typeof(IControlLauncher), typeof(ControlLauncher), ControlLauncher.NameForCUDControlRegistration);
        }

        private ControlScope(bool registerLauncher)
            : this(() => true, registerLauncher)
        {
        }

        /// <summary>
        /// CUD операции будут выполняться с контролями если условие <paramref name="condition"/> возвращает истину. Какие контроли и над какими сущностями 
        /// определяется настройкам <paramref name="settings"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="settings"></param>
        public ControlScope(Func<bool> condition ,SpecificControlLauncher.Settings settings)
        {
            if (condition())
            {
                UnityContainer.RegisterType(typeof (IControlLauncher), typeof (SpecificControlLauncher) ,ControlLauncher.NameForCUDControlRegistration);
                UnityContainer.RegisterInstance<SpecificControlLauncher.Settings>(settings);
            }
        }

        /// <summary>
        /// CUD операции будут выполняться с контролями. Какие контроли и над какими сущностями 
        /// определяется настройкам <paramref name="settings"/>
        /// </summary>
        /// <param name="settings"></param>
        public ControlScope(SpecificControlLauncher.Settings settings) :this(()=>true,settings)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlDispatcher"></param>
        public ControlScope(IControlDispatcher controlDispatcher, ScopeOptions options)
            : this(options == ScopeOptions.ApplyExecutingAndDispatching)
        {
            if (controlDispatcher == null) throw new ArgumentNullException("controlDispatcher");
            UnityContainer.RegisterInstance(controlDispatcher);
        }

        public ControlScope(DispatcherStrategies.DefaultStrategy strategy, ScopeOptions options)
            : this(options==ScopeOptions.ApplyExecutingAndDispatching)
        {
            if (strategy == null) throw new ArgumentNullException("strategy");
            UnityContainer.RegisterInstance(strategy);
        }

        public ControlScope(DispatcherStrategies.DefaultStrategy strategy, bool registerLauncher)
            : this(registerLauncher)
        {
            if (strategy == null) 
                throw new ArgumentNullException("strategy");
            UnityContainer.RegisterInstance(strategy);
        }

        ///  <summary>
        /// CUD операции будут выполняться с контролями для элемента <paramref name="baseEntity"/>.
        ///  </summary>
        ///  <param name="baseEntity">Сущность</param>
        /// <param name="skipSkippable">Не прерываться на мягких контролях</param>
        public ControlScope(IBaseEntity baseEntity, bool skipSkippable = false)
        {
            if (skipSkippable)
                UnityContainer.RegisterType(typeof(DispatcherStrategies.DefaultStrategy), typeof(SkipSkippableStrategy));

            UnityContainer.RegisterInstance("ControlTarget", baseEntity);
            UnityContainer.RegisterType(typeof(IControlLauncher), typeof(ConcreteControlLauncher),ControlLauncher.NameForCUDControlRegistration);
        }

        public ControlScope(IBaseEntity baseEntity ,Func<bool> condition) :base(condition)
        {
            if (condition())
            {
                UnityContainer.RegisterInstance("ControlTarget", baseEntity);
                UnityContainer.RegisterType(typeof (IControlLauncher), typeof (ConcreteControlLauncher),ControlLauncher.NameForCUDControlRegistration);
            }
        }
    }


    public enum ScopeOptions
    {
        /// <summary>
        /// При указании этого значения использование области видимости (Scope) определяет только КАК  будут выполнятся контроли
        /// но определяют  будут ли они выполнятся вообще
        /// </summary>
        ApplyOnlyDispatching,
        /// <summary>
        /// При указании этого значения использование области видимости (Scope) определяет и КАК  будут выполнятся контроли
        /// и будут ли они выполнятся вообще
        /// </summary>
        ApplyExecutingAndDispatching
    }
}
