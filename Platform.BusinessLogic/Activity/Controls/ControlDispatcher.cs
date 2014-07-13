using System;
using System.Data.Entity;
using System.Linq;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls
{
    
    /// <summary>
    /// Диспетчер выполнения контролей с возможностью выбора стратегии
    /// </summary>
    public class ControlDispatcher : ControlDispatcherBase
    {
        private readonly DataContext _dataContext;

        private readonly DispatcherStrategies.DefaultStrategy _strategy;
        
        /// <summary>
        /// Диспетчер со стратегией по-умолчанию
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="strategy"></param>
        /// <exception cref="PlatformException"></exception>
        public ControlDispatcher([Dependency]DbContext dbContext = null, [Dependency] DispatcherStrategies.DefaultStrategy strategy = null)
        {
            _strategy = strategy ?? IoC.Resolve<DispatcherStrategies.DefaultStrategy>();

            if (dbContext == null)
                dbContext = IoC.Resolve<DbContext>();
            _dataContext = dbContext.Cast<DataContext>();
        }

        /// <summary>
        /// Получить типизованный DataContext
        /// </summary>
        /// <typeparam name="TDataContext"></typeparam>
        /// <returns></returns>
        protected TDataContext DataContext<TDataContext>() where TDataContext: DataContext
        {
            return _dataContext.Cast<TDataContext>(); 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlAction"></param>
        /// <param name="controlName"></param>
        /// <param name="target"></param>
        public override void InvokeControl(Action controlAction, string controlName, IBaseEntity target)
        {
            var control = GetControlInfo(controlName,target);
            ControlContext.GetRequiredCurrent().ControlInfo = control;
            if ((control == null) || ((control.Enabled) && (!control.Skippable)))
                Invoke(control,controlAction, controlName);
            else if (((control.Enabled) && (control.Skippable)))
                InvokeSkippable(control,controlAction, controlName);

        }

        /// <summary>
        /// Получить информацию о контроле
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual IControlInfo GetControlInfo(string controlName, IBaseEntity target)
        {
            var result = ReadControlInfo(controlName, target);
            return result ?? ControlContext.GetRequiredCurrent().InitialControlInfo;
        }

        /// <summary>
        /// Получить информацию о контроле из БД
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual IControlInfo ReadControlInfo(string controlName, IBaseEntity target)
        {
            IControlInfo result =
                _dataContext.Control.FirstOrDefault(c => (c.Name == controlName) && (target.EntityId == c.IdEntity)) ??
                _dataContext.Control.FirstOrDefault(c => (c.Name == controlName) && (c.IdEntity == null));
            return result;
        }

        private void Invoke(IControlInfo control,Action controlAction, string controlName)
        {
            _strategy.Invoke(control,controlAction,controlName);
        }


        private void InvokeSkippable(IControlInfo control,Action controlAction, string controlName)
        {
            _strategy.InvokeSkippable(control,controlAction,controlName);
        }

        /// <summary>
        /// Возможность пропуска контролей
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override bool MaySkipSkippable(ControlResponseException exception)
        {
            return _strategy.MaySkipSkippable(exception);
        }

        /// <summary>
        /// Возможность пропуска конкретного контроля
        /// </summary>
        /// <param name="controlInfo"></param>
        /// <returns></returns>
        public override bool MaySkip(IControlInfo controlInfo)
        {
            return _strategy.MaySkip(controlInfo);
        }

        
    }
}
