using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic
{
    /// <summary>
    /// Методы расширения для IEntity
    /// </summary>
    public static class EntityExtension
    {

        /// <summary>
        /// Для сущности <paramref name="entity"/> проверяет наличие сущностного класса.
        /// Класс считается сущностным, если он:
        /// - назван так же как сущность
        /// - находится в пространстве имен ИмяПроектаСолюшена.ИмяТипа, где 
        ///   - ИмяПроектаСолюшена соответствуюет значению idSolutionProject
        ///   - ИмяТипа - имя типа сущности, например, Report.
        /// - реализует интерфейс IBaseEntity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>true - если сущностной класс найден</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool HasEntityClass(this IEntity entity)
        {
            if (entity.EntityType == EntityType.Enum)
                return false;
            Type result;
            new TypeLocator().TryGetType(entity, out result);
            return result != null && typeof(IBaseEntity).IsAssignableFrom(result);
        }

        /// <summary>
        /// Получить тип сущностного класса для сущности
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Type GetEntityType(this IEntity entity)
        {
            if (entity.EntityType == EntityType.Enum)
                return null;
            Type result;
            new TypeLocator().TryGetType(entity, out result);
            return result;
        }

        /// <summary>
        /// Является ли сущность наследником типа <paramref name="type"/> (включая интерфейсы)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Is(this IEntity entity , Type type)
        {
            var entityType = entity.GetEntityType();
            if (entityType == null)
                return false;
            return type.IsAssignableFrom(entityType);
        }

        /// <summary>
        /// Является ли сущность наследником типа <typeparamref name="T"/>p (включая интерфейсы)
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Is<T>(this IEntity entity)
        {
            return entity.Is(typeof (T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool UseEntityFrameworkByDefault(this IEntity entity)
        {
            //Чтобы использовать EF нужно добавить все поля в эти классы и промапить их
            var dalEntities = new string[]
                {
                    "Entity",
                    "EntityField",
                    "Index",
					"Form",
                    "Programmability"
                };

            if (dalEntities.Contains(entity.Name))
                return false;
            return HasEntityClass(entity);
        }
    }
}
