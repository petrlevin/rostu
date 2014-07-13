using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Interfaces
{
    /// <summary>
    /// Диспетчер операций
    /// (http://jira.rostu-comp.ru/browse/CORE-158)
    /// </summary>
    public interface IOperationDispatcher
    {

        /// <summary>
        /// Выполнить атомарную операцию  для документа или инструмента
        /// </summary>
        /// <param name="entityOperationId">идентификатор операции</param>
        /// <param name="document">документ или инструмент</param>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        /// <exception cref="OperationExecutionException"></exception>
        /// <exception cref="OperationDefinitionException"></exception>
        /// <exception cref="Exceptions.ToolStatusException"></exception>
        ClientActionList ProcessOperation(int entityOperationId, ToolEntity document);

        /// <summary>
        /// Выполнить атомарную операцию  для документа или инструмента
        /// </summary>
        /// <param name="operationName">наименование операции</param>
        /// <param name="document">документ или инструмент</param>
        /// <returns></returns>
        ClientActionList ProcessOperation(string operationName, ToolEntity document);


        /// <summary>
        /// Начать неатомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="entityOperationId">идентификатор операции</param>
        /// <param name="document">документ или инструмент</param>
        void BeginOperation(int entityOperationId, ToolEntity document);

        /// <summary> 
        /// Завершить неатомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="document">документ или инструмент</param>
        /// <param name="entityOperation">завершенная операция</param>
        ClientActionList CompleteOperation(ToolEntity document, out EntityOperation entityOperation);

        /// <summary> 
        /// Отменить неатомарную операцию для документа или инструмента
        /// </summary>
        /// <param name="document">документ или инструмент</param>
        EntityOperation CancelOperation(ToolEntity document);




    }
}
