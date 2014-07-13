using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Activity
{

    /// <summary>
    /// 
    /// </summary>
    public class TypeLocator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool  TryGetType(IEntity entity, out Type entityType )
        {
            return DoGetType(entity, out entityType);
        }

        private bool DoGetType(IEntity entity, out Type entityType,bool @throw=false)
        {
            entityType = null;
            if (entity.IdProject == 0)
                if (!@throw)
                    return false;
                else
                    throw new InvalidOperationException(String.Format("Для сущности '{0}' не указан проект. Невозможно определить тип. ",entity.Name));
                

            var entityTypeInfo = Cache.Get<EntityTypeInfo>(entity.Id);
            if (entityTypeInfo == null)
            {
                entityTypeInfo = FindType(entity);
                Cache.Put(entityTypeInfo, entity.Id);
                entityType = entityTypeInfo.Value;
            }
            else
            {
                entityType = entityTypeInfo.Value;
            }
            if (@throw && (entityType == null))
                throw new TypeLocationException(String.Format("Невозможно определить тип для сущности '{0}'. Не найден сущностной класс '{1}' в сборке '{2}' .", entity.Name, entityTypeInfo.TypeFullName, entityTypeInfo.Assembly.FullName), entityTypeInfo.TypeFullName);
            return entityType != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TypeLocationException"></exception>
        public Type GetType(IEntity entity )
        {
            Type result;
            DoGetType(entity,out result, true);
            return result;
        }

        class EntityTypeInfo
        {
            public Type Value { get; set; }
            public Assembly Assembly { get; set; }
            public String TypeFullName { get; set; }
        }

        private EntityTypeInfo FindType(IEntity entity)
        {
            try
            {
                var projectName = Solution.EntityClassProjectName(entity.IdProject);
                var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(ass => ass.GetName().Name == projectName);
                Type entityType = assembly.GetType(entity.GetClassName(projectName), false, false);
                return new EntityTypeInfo() { Assembly = assembly, Value = entityType, TypeFullName = entity.GetClassName(projectName) };
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    String.Format(
                        "Для сущности '{0}' указан проект '{1}'. Однако соответствующую сборку найти не удалось.",
                        entity.Name, Solution.EntityClassProjectName(entity.IdProject)), ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidOperationException(
                    String.Format(
                        "Для сущности '{0}' некорректно указан проект . Не возможно определить тип сущности.",
                        entity.Name), ex);
                
            }
        }

        private ISimpleCache Cache
        {
            get { return _cache ?? _Cache; }
        }

        private readonly static ISimpleCache  _Cache = new SimpleCache();
        private readonly ISimpleCache _cache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        public TypeLocator(ISimpleCache cache=null)
        {
            _cache = cache;
        }
    }
}
