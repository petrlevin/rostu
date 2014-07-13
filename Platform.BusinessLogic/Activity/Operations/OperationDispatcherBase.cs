using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.Common.Exceptions;
using Platform.Utils.Extensions;

namespace Platform.BusinessLogic.Activity.Operations
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class OperationDispatcherBase : IOperationDispatcher
    {
        #region Конструкторы

        protected OperationDispatcherBase(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #endregion

        #region Private

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IOperationLauncher _operationLauncher = new OperationLauncher();
        private readonly DbContext _dbContext;

        private void DoBeginOperation(ToolEntity document, EntityOperation entityOperation)
        {
            BlockDocument(document);
            CheckBeginState(document, entityOperation);
            WriteBeginOperation(document, entityOperation);

            _operationLauncher.StartOperation(_dbContext, entityOperation.Operation, document);

        }

        /// <exception cref="LockEntityException"></exception>
        private ClientActionList DoProcessOperation(ToolEntity document, EntityOperation entityOperation)
        {
            BlockDocument(document);
            CheckBeginState(document, entityOperation);
			WriteBeginOperation(document, entityOperation, false);
	        var result = DoProcess(document, entityOperation);
			WriteEndOperation(document, entityOperation, false);
	        return result;
        }

        #endregion

        #region Abstract
        
        // Блокировка документ
        protected abstract void BlockDocument(ToolEntity document);
        
        // Получение  операцию
        protected abstract EntityOperation GetOperation(int entityOperationId);
        protected abstract EntityOperation GetOperation(string operationName, int toolEntityId);

        //Проверка конеченного статуса документа
        protected abstract void CheckFinalStatus(ToolEntity document, EntityOperation entityOperation);

        //Проверка состояния документа при начале выполнения атомарной и неатомарной операции
        protected abstract void CheckBeginState(ToolEntity document, EntityOperation entityOperation);
        //Проверка состояния документа при завершении выполнения неатомарной операции
        protected abstract EntityOperation CheckCompleteState(ToolEntity document);
        //Проверка состояния документа при отмене выполнения неатомарной операции
        protected abstract EntityOperation CheckCancelState(ToolEntity document);

        //Запись в базу при начале выполения неатомарной операции
		protected abstract void WriteBeginOperation(ToolEntity document, EntityOperation entityOperation, bool withSerialized = true);
        //Запись в базу (удаление записей) при завершении и отмене выполения неатомарной операции
		protected abstract void WriteEndOperation(ToolEntity document, EntityOperation entityOperation, bool withSerialized = true);
        //Восстановление документа при отмене операции
        protected abstract void RestoreDocument(ToolEntity document);

        #endregion

        #region Protected

        protected DbContext DbContext
        {
            get { return _dbContext; }
        }

        protected virtual ClientActionList DoProcess(ToolEntity document, EntityOperation entityOperation)
        {
            var result = _operationLauncher.ProcessOperation(_dbContext, entityOperation.Operation, document);
            CheckFinalStatus(document, entityOperation);
            return result;
        }

        #endregion

        #region IOperationDispatcher Implementation

        /// <summary>
        /// Выполнить атомарную операцию  для документа или инструмента
        /// </summary>
        /// <param name="entityOperationId">идентификатор операции</param>
        /// <param name="document">документ или инструмент</param>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        /// <exception cref="OperationExecutionException"></exception>
        /// <exception cref="OperationDefinitionException"></exception>
        public ClientActionList ProcessOperation(int entityOperationId, ToolEntity document)
        {
            var entityOperation = GetOperation(entityOperationId);
            Logger.Info("Выполняется операция {0}. Id - {1} . Документ - {2}  ", entityOperation, entityOperationId, document);
            return DoProcessOperation(document, entityOperation);
        }

        /// <summary>
        /// Выполнить атомарную операцию  для документа или инструмента
        /// </summary>
        /// <param name="operationName">наименование операции</param>
        /// <param name="document">документ или инструмент</param>
        /// <returns></returns>
        public ClientActionList ProcessOperation(string operationName, ToolEntity document)
        {
            var entityOperation = GetOperation(operationName, document.EntityId);
            return DoProcessOperation(document, entityOperation);
        }

        /// <summary>
        /// Начать неатомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="entityOperationId">идентификатор операции</param>
        /// <param name="document">документ или инструмент</param>
        public void BeginOperation(int entityOperationId, ToolEntity document)
        {
            var entityOperation = GetOperation(entityOperationId);
            DoBeginOperation(document, entityOperation);
        }

        /// <summary> 
        /// Завершить неатомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="document">документ или инструмент</param>
        /// <param name="entityOperation">завершенная операция</param>
        public ClientActionList CompleteOperation(ToolEntity document, out EntityOperation entityOperation)
        {
            BlockDocument(document);
            entityOperation = CheckCompleteState(document);
            OnBeforeCancelComplete(document,entityOperation);
            var result = DoProcess(document, entityOperation);
            WriteEndOperation(document, entityOperation);
            return result;
        }

        /// <summary> 
        /// Отменить неатомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="document">документ или инструмент</param>
        public EntityOperation CancelOperation(ToolEntity document)
        {
            BlockDocument(document);
            var entityOperation = CheckCancelState(document);
            OnBeforeCancelComplete(document, entityOperation);
            RestoreDocument(document);
            WriteEndOperation(document, entityOperation);
            return entityOperation;
        }

        private void OnBeforeCancelComplete(ToolEntity document, EntityOperation entityOperation)
        {
            if (BeforeCancelCompleteEvents == null)
                return;
            foreach (var beforeCancelCompleteEvent in BeforeCancelCompleteEvents)
            {
                beforeCancelCompleteEvent(entityOperation,document);
            }
        }

        #endregion

        [ThreadStatic]
        internal static List<OperationsHandler> BeforeCancelCompleteEvents;

        static OperationDispatcherBase()
        {
            Application.Application.EndRequest += () => { BeforeCancelCompleteEvents = null; };
        }
    }
}
