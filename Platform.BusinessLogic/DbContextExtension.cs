using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.Common.Exceptions;
using Platform.Dal;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using EntityFramework.Extensions;
using Platform.Utils.Extensions;

namespace Platform.BusinessLogic
{
    /// <summary>
    ///
    /// </summary>
    public static class DbContextExtension
    {
        /// <summary>
        /// Возвращает экземпляр DbSet  для переданной сущности <paramref name="entity"/>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Перечисления не являются сущностью</exception>
        /// <exception cref="TypeLocationException"></exception>
        public static DbSet Set(this DbContext dbContext, IEntity entity)
        {
            return new DbSetLocator().Set(dbContext, entity);
        }


        /// <summary>
        /// Удалить элементы. Если нет контролей -- будет вызван групповой метод.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="filterExpression"></param>
        /// <typeparam name="TEntity"></typeparam>
        public static void RemoveAll<TEntity>(this DbSet<TEntity> set, Expression<Func<TEntity, bool>> filterExpression) where TEntity : class
        {
            var entityType = typeof (TEntity);
            var controlInvocations = ControlLauncher.GetInvocations(ControlType.Delete, Sequence.Any, entityType);
            if (!controlInvocations.Any())
                set.Delete(filterExpression);
            else
                set.RemoveAll(set.Where(filterExpression).ToList());
        }

        /// <summary>
        /// Удалить элементы. Если нет контролей -- будет вызван групповой метод.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="entities"></param>
        /// <typeparam name="TEntity"></typeparam>
        public static void RemoveAll<TEntity>(this DbSet<TEntity> set, IQueryable<TEntity> entities) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var controlInvocations = ControlLauncher.GetInvocations(ControlType.Delete, Sequence.Any, entityType);
            if (!controlInvocations.Any())
                entities.Delete();
            else
                set.RemoveAll(entities.ToList());
        }


        /// <summary>
        /// Возвращает экземпляр типизированного DbSet  для переданной сущности <paramref name="entity"/>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IQueryableDbSet<TInterface> Set<TInterface>(this DbContext dbContext, IEntity entity)
        {

            return new DbSetLocator().Set<TInterface>(dbContext, entity);
        }

        public static IQueryableDbSet<TInterface> Set<TInterface>(this DbContext dbContext, int entityId)
        {

            return new DbSetLocator().Set<TInterface>(dbContext, Objects.ById<Entity>(entityId));
        }


        /// <summary>
        /// Возвращает экземпляр типизированного DbSet  для переданного сущностного элемента<paramref name="element"/>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="element"></param>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IQueryableDbSet<TInterface> SetFor<TInterface>(this DbContext dbContext, object element)
        {
            var baseEntity = element as IBaseEntity;
            if (baseEntity==null)
                throw new ArgumentException(String.Format("Объект {0} не является сущностным." ,element),"element");

            return Set<TInterface>(dbContext, baseEntity.EntityId);
        }

        /// <summary>
        /// Возвращает экземпляр типизированного DbSet  для переданного сущностного элемента<paramref name="element"/>
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="element"></param>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IQueryableDbSet<TInterface> SetFor<TInterface>(this DbContext dbContext, TInterface element)
        {
            return SetFor<TInterface>(dbContext, (Object)element);
        }


        /// <summary>
        /// привести контест к типу
        /// </summary>
        /// <param name="dbContext"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public static T Cast<T>(this DbContext dbContext) where T : DbContext
        {
            var result = dbContext as T;
            if (result != null)
                return result;
            string russianNameGenetivus;
            if (ContextNames.TryGetValue(typeof(T), out russianNameGenetivus))
                throw new PlatformException(
                        String.Format(
                            "Приложение не корректно сконфигурировано. Зарегистрированный  контекст данных {0} не является контекстом {1} ('{2}').",
                            dbContext.GetType(), russianNameGenetivus, typeof(T).FullName));
            else
                throw new PlatformException(
                        String.Format(
                            "Приложение не корректно сконфигурировано. Зарегистрированный  контекст данных {0} не является контекстом {1}.",
                            dbContext.GetType(), typeof(T).FullName));


        }

        /// <summary>
        /// Зарегистрировать имя контекста данных в родительном падеже
        /// </summary>
        /// <param name="contextType"></param>
        /// <param name="russianNameGenetivus"></param>
        public static void RegisterContextName(Type contextType, string russianNameGenetivus)
        {
            ContextNames.Add(contextType, russianNameGenetivus);
        }

        /// <summary>
        /// Получить sql-команду из текущего контекста
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static SqlCommand GetCommand(this DbContext dbContext)
        {
            var connection = dbContext.Database.Connection;
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            return new SqlCommandFactory(connection).CreateCommand();
        }

        private static readonly Dictionary<Type, String> ContextNames = new Dictionary<Type, string>();



        private static readonly ISimpleCache Cache = new SimpleCache();
    }
}
