using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public static class MiltilinkHelper
    {

        /// <summary>
        /// Возвращет левое поле (по отношению к сущности <paramref name="idOwnerEntity"/>) мультиссылки
        /// </summary>
        /// <param name="multilink">Сущность самой мультиссылки</param>
        /// <param name="idOwnerEntity">id сущности, содержащей мультиссылку</param>
        /// <returns>Поле мультиссылки</returns>
        public static IEntityField GetLeftMultilinkField(IEntity multilink, int idOwnerEntity)
        {
            return multilink.Fields.Single(f => f.IdEntityLink == idOwnerEntity);
        }



        /// <summary>
        /// Возвращет правое поле (по отношению к сущности <paramref name="idOwnerEntity"/>) мультиссылки
        /// </summary>
        /// <param name="multilink">Сущность самой мультиссылки</param>
        /// <param name="idOwnerEntity">id сущности, содержащей мультиссылку</param>
        /// <returns>Поле мультиссылки</returns>
        public static IEntityField GetRightMultilinkField(IEntity multilink, int idOwnerEntity)
        {
            return multilink.Fields.Single(f => f.EntityFieldType == EntityFieldType.Link && f.IdEntityLink != idOwnerEntity);
        }


        /// <summary>
        /// Возвращет сущность правого поля (по отношению к сущности <paramref name="idOwnerEntity"/>) мультиссылки
        /// </summary>
        /// <param name="multilink">Сущность самой мультиссылки</param>
        /// <param name="idOwnerEntity">id сущности, содержащей мультиссылку</param>
        /// <returns>Поле мультиссылки</returns>
        public static IEntity GetRightMultilinkEntity(IEntity multilink, int idOwnerEntity)
        {
            return multilink.Fields.Single(f => f.EntityFieldType == EntityFieldType.Link && f.IdEntityLink != idOwnerEntity).EntityLink;
        }

    }
}
