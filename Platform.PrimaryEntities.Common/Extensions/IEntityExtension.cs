using System;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.PrimaryEntities.Common.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityExtension
    {



        /// <summary>
        /// Полное имя класса сущности
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="baseNamespace"> пространство имен сборки где находится класс сущности</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Для перечеслений метод не реализован</exception>
        public static string GetClassName (this IEntity entity ,string baseNamespace)
        {
            if (entity.EntityType==EntityType.Enum)
            {
                    throw new NotImplementedException("Для перечеслений метод не реализован");
            }
            return String.Format("{0}.{1}.{2}",baseNamespace, entity.EntityType, entity.Name);
         }


        /// <summary>
        /// Определяет является ли данная сущность справочником со статусами
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsRefernceWithStatus(this IEntity entity)
        {
            return (entity.EntityType == EntityType.Reference) && (entity.RealFields.Any(ef => ef.EntityLink != null && ef.EntityLink.Id == Constants.RefStatusId && ef.Name.ToLower() == "idrefstatus"));
        }

        /// <summary>
        /// Имя класса сущности без базового пространства имен
        /// например (Reference.User)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static string GetClassName(this IEntity entity)
        {
            if (entity.EntityType == EntityType.Enum)
            {
                throw new NotImplementedException("Для перечеслений метод не реализован");
            }
            return String.Format("{0}.{1}", entity.EntityType, entity.Name);
        }



    }
}
