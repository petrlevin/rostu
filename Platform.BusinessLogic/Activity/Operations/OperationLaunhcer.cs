using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Interfaces;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Actions;
using Platform.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.BusinessLogic.Activity.Operations
{
    /// <summary>
    /// 
    /// </summary>
    public class OperationLauncher : IOperationLauncher
    {
        private static readonly ISimpleCache Cache = new SimpleCache();


        private readonly ParameterExpression _instanceEntityExpression = Expression.Parameter(typeof(IBaseEntity), "i");
        private readonly ParameterExpression _dbContextExpression = Expression.Parameter(typeof(DbContext), "dbc");

        /// <summary>
        /// Выполнить операцию для документа 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="operation"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationExecutionException"></exception>
        public ClientActionList ProcessOperation(DbContext dbContext, Operation operation, IBaseEntity document)
        {
            if (document == null) throw new ArgumentNullException("document");
            Invocation inv = GetInvocation(operation, document.GetType());
            
            try
            {
                return inv.Func(dbContext, document);
            }
            catch (OperationDefinitionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex is IHandledException)
                    throw;
                throw new OperationExecutionException("Ошибка при выполнении операции",ex,inv.MemberInfo,document,inv.Operation);
            }
            
            

        }

        /// <summary>
        /// Обработать начало неатомарной операции для документа 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="operation"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationExecutionException"></exception>
        public ClientActionList StartOperation(DbContext dbContext, Operation operation, IBaseEntity document)
        {
            if (document == null) throw new ArgumentNullException("document");
            Invocation inv = GetInvocation(operation, document.GetType(), true);

            try
            {
                return inv.Func(dbContext, document);
            }
            catch (OperationDefinitionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex is IHandledException)
                    throw;
                throw new OperationExecutionException("Ошибка при запуске неатомарной операции", ex, inv.MemberInfo, document, inv.Operation);
            }



        }


        private Invocation GetInvocation(Operation operation, Type entityType, bool isOnStart = false)
        {
            entityType = ObjectContext.GetObjectType(entityType);
            var result = Cache.Get<Invocation>("ProcessOperation", operation.Id, entityType, isOnStart);
            if (result != null)
                return result;

            result = BuildInvocation(operation, entityType, isOnStart);
            Cache.Put(result, "ProcessOperation", operation.Id, entityType, isOnStart);
            return result;

        }

        private Invocation BuildInvocation(Operation operation, Type entityType, bool isOnStart)
        {
            var operationName = isOnStart ? operation.BeforeOperationName : operation.Name;

            var methods =
                entityType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.Name == operationName).ToList();

            if (!methods.Any())
            {
                return new Invocation()
                           {
                               Func = (a, b) => null
                           };
            }

            var matchMethods = methods.Where(
                m =>
                (((m.ReturnType == typeof (void)) ||
                  (m.ReturnType == typeof (ClientActionBase) || (m.ReturnType == typeof (ClientActionList)))) &&
                 (!m.GetParameters().Any() ||
                  ((m.GetParameters().Count() == 1) &&
                   (typeof (DbContext).IsAssignableFrom(m.GetParameters()[0].ParameterType)))))).ToList();
            if (!matchMethods.Any())
                throw new OperationDefinitionException(
                    String.Format("Операция '{0}' не правильно определена для сущности '{1}.'", operationName, entityType.Name),
                    methods.First(), operation);
            if (matchMethods.Count() > 1)
                throw new OperationDefinitionException(
                    String.Format("Двойственное определение  операции '{0}'  для сущности '{1}.'", operationName,
                                  entityType.Name), null, operation);

            var methodInfo = matchMethods.First();

            var inv = new Invocation()
                          {
                              Func =
                                  Expression.Lambda<Func<DbContext, IBaseEntity, ClientActionList>>(
                                      BuildCall(methodInfo, entityType,
                                                operation)
                                      , _dbContextExpression, _instanceEntityExpression).Compile(),
                              MemberInfo = methodInfo
                          };
            return inv;
        }

        private Expression BuildCall(MethodInfo methodInfo, Type entityType, Operation operation)
        {
            if (methodInfo.ReturnType == typeof (ClientActionList))
                return DoBuildCall(methodInfo, entityType, operation);
            else if (methodInfo.ReturnType == typeof (ClientActionList))
            {
                return  Expression.New(typeof(ClientActionList).GetConstructor(new Type[] { typeof(ClientActionList) }), DoBuildCall(methodInfo, entityType, operation));
            }
            else
            {
                var exprs = new List<Expression>();
                exprs.Add(DoBuildCall(methodInfo, entityType, operation));
                exprs.Add(Expression.New(typeof(ClientActionList)));
                return Expression.Block(exprs);

            }
        }


        private MethodCallExpression DoBuildCall(MethodInfo methodInfo, Type entityType, Operation operation)
        {
            try
            {
                if (methodInfo.GetParameters().Any())
                {
                    ParameterInfo parameterInfo = methodInfo.GetParameters().First();

                    return Expression.Call(
                        Expression.Convert(_instanceEntityExpression, entityType),
                        methodInfo,
// ReSharper disable PossiblyMistakenUseOfParamsMethod
                        Expression.TryCatch(

                            Expression.Convert(_dbContextExpression, parameterInfo.ParameterType),
                            Expression.Catch(typeof (Exception),
                                             Expression.Throw(
                                                 Expression.Constant(
                                                     new OperationDefinitionException(
                                                         String.Format(
                                                             "Операция {0} не правильно определена для сущности '{1}.'",
                                                             operation.Name, entityType.Name), methodInfo, operation))
                                                 , parameterInfo.ParameterType)
                                )));
// ReSharper restore PossiblyMistakenUseOfParamsMethod
                }

                else

                    return Expression.Call(Expression.Convert(_instanceEntityExpression, entityType), methodInfo
                        );
            
            }
            catch (ArgumentException ex)
            {

                throw new OperationDefinitionException(String.Format("Операция {0} не правильно определена для сущности '{1}.'", operation.Name, entityType.Name), ex, methodInfo, operation);
            }

        }




        private class Invocation
        {
            public Func<DbContext, IBaseEntity, ClientActionList> Func { get; set; }
            public MemberInfo MemberInfo { get; set; }
            public Operation Operation { get; set; }

        }

    }
}
