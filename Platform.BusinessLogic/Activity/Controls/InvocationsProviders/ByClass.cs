using System;
using System.Collections.Generic;
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
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Unity.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls.InvocationsProviders
{
    public class ByClass : Base, IControlInvocationsProvider
    {
        protected readonly ParameterExpression _instanceEntityExpression = Expression.Parameter(typeof(IBaseEntity), "i");


        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlType"></param>
        /// <param name="sequence"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public IEnumerable<IControlInvocation> GetInvocations(ControlType controlType, Sequence sequence, Type entityType)
        {


            IEnumerable<MemberInfo> methods = entityType.GetMethods().Where(m => m.GetCustomAttributes(typeof(ControlAttribute), true).Any());

            methods = GetControls(methods, controlType, sequence);



            //(dbc,ct,s,i,ne)=>i.<ControlMethod>(...?);
            var result = methods.Select(mi => new ControlInvocation()
            {
                Action = Expression.Lambda<Action<DbContext, ControlType, Sequence, IBaseEntity, IBaseEntity>>(
                                                                  BuildCall((MethodInfo)mi, entityType),
                                                                  _dbContextExpression,
                                                                  _controlTypeExpression,
                                                                  _sequenceExpression,
                                                                  _instanceEntityExpression,
                                                                  _oldEntityExpression

                                                                  ).Compile(),

                MemberInfo = mi,
                ExecutionOrder = GetExecutionOrder(mi) ,
                InitialControlInfo = GetInitilaControlInfo(entityType,mi)
                
            }
                                    ).ToList();



            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        private IControlInfo GetInitilaControlInfo(Type entityType, MemberInfo control)
        {
            return ControlInfo.GetInitial(entityType, control);

        }


        private MethodCallExpression BuildCall(MethodInfo control, Type entityType)
        {
            try
            {
                var parameters = new List<Expression>();
                foreach (var parameterInfo in control.GetParameters())
                {
                    if (parameterInfo.ParameterType == typeof(ControlType))
                        parameters.Add(_controlTypeExpression);
                    else if (parameterInfo.ParameterType == typeof(Sequence))
                        parameters.Add(_sequenceExpression);
                    else if ((typeof(IBaseEntity)).IsAssignableFrom(parameterInfo.ParameterType) && (parameterInfo.ParameterType.IsAssignableFrom(entityType)))
                    {
                        parameters.Add(Expression.Convert(_oldEntityExpression, parameterInfo.ParameterType));
                    }
                    else if ((typeof(DbContext)).IsAssignableFrom(parameterInfo.ParameterType))
                    {
                        parameters.Add(BuildDataContextParameter(parameterInfo.ParameterType, parameterInfo.Name,
                                                                 control));
                    }
                    else
                        throw new ControlDefinitionException(
                            String.Format(
                                "Контроль не правильно определен. Параметр {0}  типа '{1}' не может быть инициализирован ",
                                parameterInfo.Name, parameterInfo.ParameterType), control);


                }
                return Expression.Call(Expression.Convert(_instanceEntityExpression, entityType), control,
                                       parameters);
            }
            catch (ArgumentException ex)
            {

                throw new ControlDefinitionException("Контроль не правильно определен.  ", ex, control);
            }
        }

        public class DefaultRegistration : IDefaultRegistration
        {
            public void Register(IUnityContainer unityContainer)
            {
                unityContainer.RegisterType<IControlInvocationsProvider, ByClass>();
            }
        }

    }
}
