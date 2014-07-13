using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Queries;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;


namespace Platform.BusinessLogic.Activity
{
    internal class DbSetLocator :TypeLocator
    {
        public DbSetLocator(ISimpleCache cache) : base(cache)
        {
            
        }

        public DbSetLocator()
            : base(null)
        {

        }


        /// <summary>
        /// Получает DbSet бизнес сущности определенной в <paramref name="entity"/>
        /// Например если entity.Name == "Car"  - DbSetCar 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Перечисления не являются сущностью</exception>
        /// <exception cref="TypeLocationException"></exception>
        public DbSet Set(DbContext dbContext, IEntity entity) 
        {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            if (entity == null) throw new ArgumentNullException("entity");


            return dbContext.Set(GetType(entity));
        }


        public IQueryableDbSet<TInterface> Set<TInterface>(DbContext dbContext, IEntity entity)
        {
            
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            if (entity == null) throw new ArgumentNullException("entity");

            var entityType = GetType(entity);
            if (!typeof(TInterface).IsAssignableFrom(entityType))
                throw new InvalidOperationException(String.Format("Тип '{1}' не реализует интерфейс {0}. Получения DbSet-а не возможно . ", typeof(TInterface),entityType));
            var result = dbContext.Set(entityType);
            return new InterfaceQueryableDbSet<TInterface>(result,entityType);
        }





    }
}
