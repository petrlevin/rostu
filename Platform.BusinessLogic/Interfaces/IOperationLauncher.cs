using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.BusinessLogic.Interfaces
{
    /// <summary>
    /// лончер операций
    /// </summary>
    public interface IOperationLauncher
    {

        /// <summary>
        /// Выполнить  операцию для документа 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="operation"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationExecutionException"></exception>
        /// <exception cref="OperationDefinitionException"></exception>
        ClientActionList ProcessOperation(DbContext dbContext, Operation operation, IBaseEntity document);

        /// <summary>
        /// Выполнить  действие при начале неатомарной операции для документом 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="operation"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="OperationExecutionException"></exception>
        /// <exception cref="OperationDefinitionException"></exception>
        ClientActionList StartOperation(DbContext dbContext, Operation operation, IBaseEntity document);
    }
}
