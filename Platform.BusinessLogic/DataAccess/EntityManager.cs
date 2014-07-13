using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Менеджер для работы с объектами Entity Framework
    /// Следует использовать когда типы объектов не известны
    /// 
    /// </summary>
    public class EntityManager
    {
        private readonly IEntity _source
            ;

        private readonly DbContext _dbContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Сущность для которорой производятся операции</param>
        /// <param name="dbContext"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public EntityManager(IEntity source, DbContext dbContext)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            _source = source;
            _dbContext = dbContext;
        }


        /// <summary>
        /// Является ли сущность типа <paramref name="TType"/>
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public bool Is<TType>()
        {
            return typeof (TType).IsAssignableFrom(new TypeLocator().GetType(_source));
        }

        public EntityManager(IEntity source) : this (source,IoC.Resolve<DbContext>())
        {
        }

        

        /// <summary>
        /// Создать новый элемент  
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public IBaseEntity Create(EntityState state)
        {
            var set = GetDbSet(_dbContext);
            var businessEntity = (IBaseEntity)set.Create();
            _dbContext.Entry(businessEntity).State = state;
            return businessEntity;
        }

        /// <summary>
        /// Найти элемент по идентификатору
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public IBaseEntity Find(int itemId)
        {
            var dbSet = GetDbSet(_dbContext);

            //Expression<Func<EntityOperation,bool>>  
                //dbSet.Cast<Operation>().Where()

            var obj = dbSet.Find(itemId);
            if (obj == null)
                throw new PlatformException(String.Format("Объект типа  '{0}' не найден по идентификатору '{1}'.",
                                                          _source.GetClassName(), itemId));
            var businessEntity = (IBaseEntity)obj;
            return businessEntity;
        }


         /// <summary>
         /// 
         /// </summary>
         /// <typeparam name="TInterface"></typeparam>
         /// <returns></returns>
         public IQueryable<TInterface> AsQueryable<TInterface>()
         {
             return _dbContext.Set<TInterface>(_source);
         }

        /// <summary>
        /// Удалить элемент
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="element"></param>
        public void Remove(object element)
        {
            GetDbSet(_dbContext).Remove(element);
        }

        protected virtual DbSet GetDbSet(DbContext dbContext)
        {
            return dbContext.Set(_source);
        }


    }
}
