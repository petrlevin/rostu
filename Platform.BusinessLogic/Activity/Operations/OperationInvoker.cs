using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Interfaces;
using Platform.ClientInteraction;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Operations
{

    /// <summary>
    /// 
    /// </summary>
    public class OperationInvoker
    {
        protected readonly ToolEntity Document;
        protected readonly IOperationDispatcher OperationDispatcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="operationDispatcher"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public OperationInvoker(ToolEntity document, IOperationDispatcher operationDispatcher=null)
        {
            if (document == null) throw new ArgumentNullException("document");
            Document = document;

            OperationDispatcher = operationDispatcher ?? IoC.Resolve<IOperationDispatcher>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationName"></param>
        /// <returns></returns>
        public ClientActionList InvokeOperation(string operationName)
        {
            return OperationDispatcher.ProcessOperation(operationName, Document);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OperationInvoker<T> :OperationInvoker    
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="operationDispatcher"></param>
        public OperationInvoker(T document, [Dependency] IOperationDispatcher operationDispatcher) :base(Cast(document),operationDispatcher)
        {
        }

        private static ToolEntity Cast(T document)
        {
           if (!(document is ToolEntity))
               throw new ArgumentException(String.Format("Переданный элемент '{0}' не является документом или инструментом.", document), "document");
            return document as ToolEntity;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>

        public OperationInvoker(T document)
            : base(Cast(document), null)
        {
        }


        private void CheckDbContextParameter(Expression argument, Exception exception
            )
        {
            var ce = argument as ConstantExpression;

            if (ce != null)
            {
                if (!typeof(DataContext).IsAssignableFrom(ce.Type))
                    throw exception;
                return ;
            }
            var me = argument as MemberExpression;
            if (me != null)
            {
                var target = me.Expression as ConstantExpression;
                if (target == null)
                {
                    throw exception;
                }
                if (me.Member is FieldInfo)
                    return ;
                else if (me.Member is PropertyInfo)
                    return ;
            }
            throw exception;
        }

        /// <summary>
        /// 
        /// </summary>

        /// <param name="operationLambda"></param>
        /// <exception cref="Exception"></exception>
        public ClientActionList InvokeOperation(Expression<Action<T>> operationLambda)
        {
            if ((operationLambda.NodeType != ExpressionType.Lambda))
                throw Exception(operationLambda);

            var body = operationLambda.Body;
            var call = body as MethodCallExpression;
            if (call == null)
                throw Exception(operationLambda);
            if ((call.Object == null) || (call.Object.NodeType != ExpressionType.Parameter) || (call.Object.Type != typeof(T)))
                throw Exception(operationLambda);
            if (call.Arguments.Count>1)
                throw Exception(operationLambda);
            if (call.Arguments.Count == 1)
                CheckDbContextParameter(call.Arguments[0], Exception(operationLambda));
            return OperationDispatcher.ProcessOperation(call.Method.Name, Document);
        }




        private Exception Exception(Expression<Action<T>> operationLambda)
        {
            return new ArgumentException(String.Format("Для вызова операции нужно использовать ламбда выражение (entity=>entity.SomeOperationMethod(context) тип передаваемого аргумента должен быть DataContext  либо entity=>entity.SomeOperationMethod()). Передано выражение - {0} ", operationLambda), "operationLambda");
        }

    }
}
