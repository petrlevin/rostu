using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Unity.Common.Interfaces;
using Platform.Utils;
using Platform.Utils.Extensions;

namespace Platform.BusinessLogic.Activity.Controls.InvocationsProviders
{
    /// <summary>
    /// Провайдер вызовов общих контролей (реализованных как ICommonControl<in TFor, in TDataContext> )
    /// </summary>
    public class Common : Base, IControlInvocationsProvider
    {
        private readonly ParameterExpression _entityExpression = Expression.Parameter(typeof(IBaseEntity), "e");



        public IEnumerable<IControlInvocation> GetInvocations(ControlType controlType, Sequence sequence, Type entityType)
        {
            var result = new List<IControlInvocation>();
            foreach (Type @interface in entityType.GetAllParents())
            {
                if (!CommonControls.ContainsKey(@interface))
                    continue;
                Type currInterface = @interface;
                result.AddRange(GetControls(CommonControls[currInterface], controlType, sequence).Select(
                    ct => new ControlInvocation()
                              {

                                  Action =
                                      Expression
                                      .Lambda<Action<DbContext, ControlType, Sequence, IBaseEntity, IBaseEntity>>(
                                          BuildCall((Type)ct, currInterface),
                                          _dbContextExpression,
                                          _controlTypeExpression,
                                          _sequenceExpression,
                                          _entityExpression,
                                          _oldEntityExpression

                                      ).Compile(),
                                  MemberInfo = ct,
                                  ExecutionOrder = GetExecutionOrder(ct, entityType) ,
                                  InitialControlInfo = GetInitilaControlInfo(entityType, ct)

                              }));
            }
            return result;
        }

        private IControlInfo GetInitilaControlInfo(Type entityType, MemberInfo control)
        {
            return CommonControlInfo.GetInitial(entityType, (Type)control);
        }

        private MethodCallExpression BuildCall(Type control, Type intefaceType)
        {
            var controlInterface = ControlInterface(control, intefaceType);
            return BuildCall(control, control.GetInterfaceMap(controlInterface.ControlInterface).TargetMethods[0], intefaceType,
                             controlInterface.DataContextType);
        }

        protected  int GetExecutionOrder(MemberInfo mi ,Type entityType)
        {
            ControlOrderForAttribute orderAtrrInCntrl;
            ControlOrderForAttribute orderAtrrInEnt;

            ControlAttributeHelper.GetAttributes(mi, entityType, "определен порядок выполнения", out orderAtrrInCntrl, out orderAtrrInEnt);

            if ((orderAtrrInCntrl == null) && (orderAtrrInEnt == null))
                return GetExecutionOrder(mi);
            return orderAtrrInCntrl!=null?orderAtrrInCntrl.ExecutionOrder:orderAtrrInEnt.ExecutionOrder;
        }



        private MethodCallExpression BuildCall(Type control, MethodInfo controlMethod, Type intefaceType,
                                               Type dataContextType)
        {
            //void Control(TDataContext dataContext,ControlType controlType, Sequence sequence , TFor entity , TFor oldEntity);
            var parameters = new List<Expression>();
            parameters.Add(BuildDataContextParameter(dataContextType, "dataContext", controlMethod));
            parameters.Add(_controlTypeExpression);
            parameters.Add(_sequenceExpression);
            parameters.Add(Expression.Convert(_entityExpression, intefaceType));
            parameters.Add(Expression.Convert(_oldEntityExpression, intefaceType));

            return
                Expression.Call(Expression.New(control), controlMethod,
                                parameters);


        }

        #region Static

        static Common()
        {
            CommonControls = new CommonControls();
            Assemblies.AllTypes().Where(IsCommonControl).ToList().ForEach(cct => ControlTypeTargets(cct).ForEach(tt => CommonControls.Add(tt, cct)));

            
        }

        private static List<Type> ControlTypeTargets(Type ctype)
        {
            return ctype.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommonControl<,>)).Select(i => i.GetGenericArguments()[0]).ToList();
        }

        private static ControlInterfaceInfo ControlInterface(Type ctype, Type targetInterface)
        {
            try
            {
                var result = ctype.GetInterfaces()
                                  .Single(
                                      i =>
                                      i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommonControl<,>) &&
                                      i.GetGenericArguments()[0] == targetInterface);
                return new ControlInterfaceInfo()
                {
                    ControlInterface = result,
                    TargetInterface = targetInterface,
                    DataContextType = result.GetGenericArguments()[1]
                };

            }
            catch (InvalidOperationException ex)
            {

                throw new ControlDefinitionException(String.Format(
                    "Контроль  не правильно определен. Тип '{0}'  реализует 'ICommonControl' с одинаковым типом интерфейса цели (TTarget) '{1}' более чем один раз.",
                    ctype, targetInterface
                                                         ), ex, ctype);
            }
        }


        private static bool IsCommonControl(Type type)
        {
            return type.GetCustomAttributes(typeof(ControlAttribute), true).Any() &&
                type.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommonControl<,>));
        }

        private static readonly CommonControls CommonControls;



        #endregion

        class ControlInterfaceInfo
        {
            public Type ControlInterface { get; set; }
            public Type TargetInterface { get; set; }
            public Type DataContextType { get; set; }

        }

        public class DefaultRegistration : IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                unityContainer.RegisterType<IControlInvocationsProvider, Common>();
            }
        }



    }
}
