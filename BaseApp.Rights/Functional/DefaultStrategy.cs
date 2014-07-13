using System;
using System.Collections.Generic;
using System.Linq;
using BaseApp.Common.Interfaces;
using BaseApp.Tablepart;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.Common.Exceptions;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;

namespace BaseApp.Rights.Functional
{
    internal class DefaultStrategy : StrategyBase
    {
        private readonly IUser _user;
        private readonly DataContext _dataContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dataContext"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultStrategy(IUser user, DataContext dataContext)
        {
            _user = user;
            _dataContext = dataContext;
        }


        private string ErrorMessage(IEntity entity, string readOrEdit)
        {
            return String.Format("Отсутствуют функциональные права на {0} элементов сущности '{1}'", readOrEdit, entity.Caption);
        }

        private string ErrorMessageOperation(IEntity entity, string operationName)
        {
            return String.Format("Отсутствуют функциональные права на выполнение операции {0} для  '{1}'", operationName, entity.Caption);
        }


        private string ErrorMessage(IEntity entity, RefStatus refStatus)
        {
            return
                String.Format(
                    "Отсутствуют функциональные права на редактирование элемента справочника '{0}' в статусе '{1}'",
                    entity.Caption, refStatus.Caption());
        }


        public override string AllowedRead(IEntity rightHolder, IEntity entity)
        {
            if ((entity.EntityType == EntityType.Enum) || (entity.EntityType == EntityType.Registry))
                return null;
            return GetRights(rightHolder).Any() ? null : ErrorMessage(entity, "чтение");
        }


        public override string AllowedEdit(IEntity rightHolder, IEntity entity, int[] itemIds = null)
        {
            var result = GetRights(rightHolder).Any(rfr => rfr.EditingFlag);
            if (!result)
                return ErrorMessage(entity, "редактирование");
            if (entity.EntityType == EntityType.Reference)
                return AllowEditForReffernece(rightHolder, entity, itemIds);
            return null;
        }

        public override string AllowedExecute(IEntity rightHolder, int entityOperationId)
        {
            var operation = _dataContext.EntityOperation.First(eo => eo.Id == entityOperationId) ;
            return AllowedExecute(rightHolder, operation);
        }

        public override string AllowedExecute(IEntity rightHolder, EntityOperation operation)
        {
            if (!_dataContext.Role_DocumentOperation.Any(
                rdo =>
                (rdo.Master.IdEntity == rightHolder.Id) && (rdo.Owner.Users.Any(u => u.Id == _user.Id)) &&
                (rdo.IdOperation == operation.IdOperation) && (rdo.SwitchOn))
                )
                return ErrorMessageOperation(rightHolder, operation.Operation.Caption);
            return null;
        }

        private string AllowEditForReffernece(IEntity rightHolder, IEntity entity, int[] itemIds)
        {

            if (!entity.IsRefernceWithStatus())
                return null;
            if (!entity.GenerateEntityClass)
                return ErrorMessage(entity, "редактирование");
            if (itemIds == null)
                return null;
            var referrences = LoadReferences(entity, itemIds);


            foreach (var @ref in referrences.ToList())
            {
                if (!_dataContext.Role_RefStatus.Any(
                    rrf =>
                    (rrf.Master.IdEntity == rightHolder.Id) && (rrf.Owner.Users.Any(u => u.Id == _user.Id)) &&
                    (rrf.IdRefStatus == @ref.IdRefStatus) && (rrf.SwitchOn))
                    )
                    return ErrorMessage(entity, @ref.RefStatus);
            }

            return null;

        }



        private IQueryable<IHasRefStatus> LoadReferences(IEntity entity, IEnumerable<int> itemIds)
        {
            var result = new EntityManager(entity, _dataContext).AsQueryable<IHasRefStatus>();
            return result.Where(rf => itemIds.Contains(rf.Id));
        }


        private IQueryable<Role_FunctionalRight> GetRights(IEntity rightHolder)
        {
            rightHolder = GetRightHolder(rightHolder);
            return _dataContext.Role_FunctionalRight.Where(fr => (fr.IdEntity == rightHolder.Id) && (fr.Owner.Users.Any(u => u.Id == _user.Id)));
        }

        private IEntity GetRightHolder(IEntity rightHolder)
        {
            if (rightHolder.EntityType == EntityType.Tablepart)
            {
                var masterEntity = _dataContext.Entity.Where(w=>  w.IdEntityType != (int) EntityType.Tablepart && w.IdEntityType != (int)EntityType.Multilink).ToList().SingleOrDefault(w => w.Fields.Any(f => f.IdEntityLink == rightHolder.Id) && rightHolder.Fields.Any(f=> f.IdEntityLink == w.Id));
                if (masterEntity != null)
                {
                    return masterEntity;
                }
                else
                {
                    masterEntity = _dataContext.Entity.Where(w => w.IdEntityType == (int)EntityType.Tablepart).ToList().FirstOrDefault(w => rightHolder.Fields.Any(f => f.IdEntityLink == w.Id));
                    return GetRightHolder(masterEntity);
                }
            }
            return rightHolder;
        }

    }
}
