using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Activity.Controls.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.ClientInteraction;
using Platform.Common;
using Platform.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// менеджер контролей на вставку удаление и обновление объекта сущностного класса в базу данных.
    /// (класс НЕ потокобезапасный)
    /// 
    /// </summary>

    public class ControlLauncher : IControlLauncher
    {

        private static readonly ISimpleCache Cache = new SimpleCache();





        private readonly Action<DbContext, IBaseEntity> _detatchEntity;
        private readonly DbContext _dbContext;
        private readonly IControlDispatcher _controlDispatcher;

        /// <summary>
        /// Используется в тестах
        /// </summary>
        internal ControlLauncher(Action<DbContext, IBaseEntity> detatchEntity, DbContext dbContext, IControlDispatcher controlDispatcher)
        {
            _detatchEntity = detatchEntity;
            this._dbContext = dbContext;
            this._controlDispatcher = controlDispatcher;
        }

        static private readonly Action<DbContext, IBaseEntity> DefaultDetatchEntity = (dc, be) =>
                                 {
                                     try
                                     {
                                         dc.Entry(be).State = EntityState.Detached;
                                     }
                                     catch (Exception)
                                     {
                                     }
                                     
                                 };

        /// <summary>
        /// 
        /// </summary>
        /// 
        [InjectionConstructor]
        public ControlLauncher([Dependency]DbContext dbContext, [Dependency]IControlDispatcher controlDispatcher)
        {
            this._dbContext = dbContext;
            this._controlDispatcher = controlDispatcher;
            _detatchEntity = DefaultDetatchEntity;
        }




        public ControlLauncher(IControlDispatcher controlDispatcher, bool resolveContext = true)
            : this(resolveContext ? DefaultDetatchEntity : (c, e) => { }, resolveContext ? IoC.Resolve<DbContext>() : null, controlDispatcher)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="entity"></param>
        /// <param name="memberInfo"></param>
        /// <param name="initialControlInfo"></param>
        /// <exception cref="ControlExecutionException"></exception>
        public void InvokeControl(Action<IBaseEntity> control, IBaseEntity entity, MemberInfo memberInfo , IControlInfo initialControlInfo)
        {

            using (new ControlContext(_controlDispatcher, memberInfo, entity, () => _detatchEntity(_dbContext, entity), initialControlInfo))
            {
                _controlDispatcher.InvokeControl(() =>
                                                     {
                                                         try
                                                         {
                                                             control(entity);

                                                         }
                                                         catch (ControlDefinitionException)
                                                         {
                                                             throw;
                                                         }
                                                         catch (ControlExecutionException)
                                                         {
                                                             throw;
                                                         }
                                                         catch (Exception ex)
                                                         {

                                                             if (ex is IHandledException)
                                                                 throw;
                                                             var newEx = new ControlExecutionException(
                                                                 "Ошибка при выполнении контроля ", ex, memberInfo,
                                                                 entity);
                                                             newEx.OnComplete +=
                                                                 () => _detatchEntity(_dbContext, entity);
                                                             throw newEx;


                                                         }
                                                     }, memberInfo.Name, entity);
            }


        }


        public virtual void ProcessControls(ControlType controlType, Sequence sequence, IBaseEntity entityValue,
                                    IBaseEntity oldEntityValue)
        {
            if (entityValue == null) throw new ArgumentNullException("entityValue");



            foreach (var inv in GetInvocations(controlType, sequence, entityValue.GetType()))
            {
                IControlInvocation curInv = inv;
                InvokeControl(ent => curInv.Action(_dbContext, controlType, sequence, ent, oldEntityValue), entityValue, inv.MemberInfo , curInv.InitialControlInfo);
            }
        }

        public static IEnumerable<IControlInvocation> GetInvocations(
            ControlType controlType, Sequence sequence, Type entityType)
        {
            entityType = ObjectContext.GetObjectType(entityType);
            var result = Cache.Get<List<IControlInvocation>>("ProcessControll",
                                                                                                   controlType, sequence,
                                                                                                   entityType);

            if (result != null)
                return result;

            result = new List<IControlInvocation>();
            foreach (IControlInvocationsProvider controlInvocationsProvider in IoC.ResolveAll<IControlInvocationsProvider>())
            {
                result.AddRange(controlInvocationsProvider.GetInvocations(controlType, sequence, entityType));
            }
            result = result.OrderBy(ci => ci.ExecutionOrder).ToList();
            Cache.Put(result, "ProcessControll", controlType, sequence, entityType);
            return result;

        }






        public class ForFreeControlsRegistration : IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                unityContainer.RegisterType(typeof(IControlLauncher), typeof(ControlLauncher), NameForFreeControlRegistration);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static IControlLauncher Current
        {
            get { return IoC.Resolve<IControlLauncher>(NameForCUDControlRegistration); }
        }


        public const string NameForFreeControlRegistration = "ForFreeControl";
        public const string NameForCUDControlRegistration = "ForCUDControl";

    }
}
