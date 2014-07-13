using System;
using System.Data.Entity;
using BaseApp.Common.Exceptions;
using BaseApp.Common.Interfaces;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Operations;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Rights.Functional
{
    /// <summary>
    /// Менеджер для работы с правами 
    /// </summary>
    public class RightsManager 
    {
        private readonly IEntity _entity;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        /// <param name="dbContext"></param>
        public RightsManager([Dependency("CurrentUser")]IUser user, [Dependency]DbContext dbContext,IEntity entity)

        {
            _entity = entity;
            if (user == null) throw new ArgumentNullException("user");
            if (user.IsSuperUser())
                _strategy = new SuperUserStrategy();
            else
            {
                if (dbContext == null) throw new ArgumentNullException("dbContext");
                _strategy = new DefaultStrategy(user, dbContext.Cast<DataContext>());
            }

        }

        private readonly StrategyBase _strategy;

        /// <summary>
        /// Разрешено ли читать
        /// </summary>
        /// <param name="rightHolder"></param>
        /// <returns></returns>
        public bool AllowedRead(IEntity rightHolder)
        {
            return _strategy.AllowedRead(rightHolder,_entity)==null;
        }


        /// <summary>
        /// Разрешено ли редактировать
        /// </summary>
        /// <param name="rightHolder"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool AllowedEdit(IEntity rightHolder)
        {
            return _strategy.AllowedEdit(rightHolder,_entity, null)==null;
        }


        /// <summary>
        /// Проверка на чтение
        /// </summary>
        /// <param name="rightHolder"></param>
        /// <exception cref="FunctionalRightsException"></exception>
        public void ValidateRead(IEntity rightHolder)
        {
            var error = _strategy.AllowedRead(rightHolder, _entity);
            if (error!=null)
                throw new FunctionalRightsException(error);
        }


        /// <summary>
        /// Проверка на редактирование
        /// </summary>
        /// <param name="rightHolder"></param>
        /// <param name="itemId"></param>
        /// <param name="manager"></param>
        /// <exception cref="FunctionalRightsException"></exception>
        public void ValidateWrite(IEntity rightHolder, int? itemId = null)
        {
            ValidateWrite(rightHolder, itemId.HasValue ? new[] { itemId.Value } : null);
            
        }

        public void ValidateExecute(IEntity rightHolder, EntityOperation operation)
        {
            var error = _strategy.AllowedExecute(rightHolder, operation);
            if (error != null)
                throw new FunctionalRightsException(error);
            
        }

        public void ValidateExecute(IEntity rightHolder, int entityOperationId)
        {
                var error = _strategy.AllowedExecute(rightHolder, entityOperationId);
                if (error != null)
                    throw new FunctionalRightsException(error);

        }


        public void ValidateWrite(IEntity rightHolder, int[] itemId = null)
        {
            var error = _strategy.AllowedEdit(rightHolder, _entity, itemId);
            if (error != null)
                throw new FunctionalRightsException(error);
        }

        
    }
}
