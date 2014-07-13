using System;
using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.DataAccess;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Rights.Functional
{
    /// <summary>
    /// 
    /// </summary>
    public class RightsHolder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridParams"></param>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public IEntity DefineHolder(GridParams gridParams)
        {
            try
            {
                if (gridParams.SrcEntityId != 0)
                    return Objects.ById<Entity>(gridParams.SrcEntityId);

                var entity = Objects.ById<Entity>(gridParams.EntityId);
                if (String.IsNullOrWhiteSpace(gridParams.OwnerFieldName))
                    return entity;
                return entity.RealFields.Single(f => f.Name == gridParams.OwnerFieldName).EntityLink;

            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка получения держателя прав. Идентификатор сущности : {0} ", gridParams.EntityId), ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="ownerEntityId"></param>
        /// <returns></returns>
        public IEntity DefineHolder(int entityId, int? ownerEntityId)
        {
            try
            {
                if (ownerEntityId.HasValue)
                    return Objects.ById<Entity>(ownerEntityId.Value);
                var entity = Objects.ById<Entity>(entityId);
                if (entity.EntityType != EntityType.Tablepart)
                    return entity;
                var dataContext = IoC.Resolve<DbContext>().Cast<DataContext>();
                try
                {
                    return dataContext.EntityField.First(
                        ef => ((ef.IdEntityLink == entityId) && (ef.IdEntityFieldType == (byte)EntityFieldType.Tablepart)))
                                      .Entity;

                }
                catch (InvalidOperationException)
                {
                    throw new PlatformException(String.Format("Сущность является табличной частью. Однако не найдено ни одного поля типа 'табличная часть' указывающего на эту сущность. ", entityId));
                }

            }
            catch (Exception ex)
            {
                throw new PlatformException(String.Format("Ошибка получения держателя прав. Идентификатор сущности : {0} ", entityId), ex);
            }

        }





        /// <summary>
        /// Определить держателя прав 
        /// </summary>
        /// <param name="gridParams"></param>
        /// <returns></returns>
        public static IEntity Define(GridParams gridParams)
        {
            return new RightsHolder().DefineHolder(gridParams);
        }

        /// <summary>
        /// Определить держателя прав 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="parentEntityId"></param>
        /// <returns></returns>
        public static IEntity Define(int entityId, int? parentEntityId)
        {
            return new RightsHolder().DefineHolder(entityId, parentEntityId);
        }

    }
}
