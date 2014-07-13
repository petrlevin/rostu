using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Operations;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.EntityTypes.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.Common;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.EntityTypes
{
	/// <summary>
	/// Базовый класс для инструментов
	/// </summary>
	public abstract class ToolEntity : ReferenceEntity, IToolEntity
	{
		public virtual int IdDocStatus { get; set; }
		public virtual DocStatus DocStatus { get; set; }

		public IQueryable<EntityOperation> GetAvailableOperations()
		{
            return EntityOperations
                .Include(e => e.EditableFields)
                .Include(e => e.Entity)
                .Include(e => e.Operation)
                .Where(eo => eo.IdEntity == EntityId && eo.OriginalStatus.Any(a => a.Id == IdDocStatus) && !eo.IsHidden);
		}



        internal void InvokeControl<T>(Expression<Action<T>> controlLambda) where T:ToolEntity
        {
            var methodName = String.Empty;
            {
                var body = controlLambda.Body;
                var call = body as MethodCallExpression;
                if (call != null)
                    methodName = typeof(T).Name + "." + call.Method.Name;
            }

            var audit = Audit<OperationAuditor>.Start(new OperationAuditor()
                {
                    EntityId = EntityId,
                    ElementId = Id,
                    OperationName = methodName,
                    OperationType = ProcessOperationTypes.Control
                });

            new ControlInvoker<T>((T)this).InvokeControl(controlLambda);
            
            audit.Complete();
        }


        internal void InvokeControl<T>(Type commonControl) where T : ToolEntity
        {
            new ControlInvoker<T>((T)this).InvokeControl(commonControl,(T)this);
        }


        internal ClientActionList InvokeOperation<T>(Expression<Action<T>> operationLambda) where T : ToolEntity
        {
            return new OperationInvoker<T>((T)this).InvokeOperation(operationLambda);
        }

        /// <summary>
        /// Выполнить операцию по имени
        /// </summary>
        /// <param name="operationName"></param>
        /// <returns></returns>
        public ClientActionList ExecuteOperation(string operationName)
        {
            return new OperationInvoker(this).InvokeOperation(operationName);
        }


	    protected IQueryable<EntityOperation> EntityOperations
	    {
	        get
	        {
	            var dbContext = (DataContext) IoC.Resolve<DbContext>();
	            return dbContext.EntityOperation.Where(op=>op.IdEntity == EntityId);

	        }
	    }


	}

    public abstract class ToolEntity<TEntity> : ToolEntity where TEntity : ToolEntity
    {

        protected void ExecuteControl(Expression<Action<TEntity>> controlLambda)
        {
            InvokeControl(controlLambda);
        }

        protected void ExecuteControl(Type controlType)
        {
            InvokeControl<TEntity>(controlType);
        }

        protected void ExecuteControl<TControl>()
        {
            ExecuteControl(typeof(TControl));
        }


        /// <summary>
        /// Выполнить операцию
        /// </summary>
        /// <param name="operationLambda"></param>
        /// <returns></returns>
        public ClientActionList ExecuteOperation(Expression<Action<TEntity>> operationLambda)
        {
            return InvokeOperation(operationLambda);
        }

    }

}
