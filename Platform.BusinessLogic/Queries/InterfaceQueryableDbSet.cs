using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using EntityFramework.Extensions;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Queries
{
    internal class InterfaceQueryableDbSet<TInterface> : InterfaceQueryable<TInterface>, IQueryableDbSet<TInterface>
    {

        public static Expression<Func<T, bool>> CastExpression<T>(Expression<Func<TInterface, bool>> expression) where T:TInterface
        {
            var parameter = expression.Parameters[0];

            var result = Expression.Lambda<Func<T, bool>>(expression.Body, parameter);
            return result;
        }

        public static Expression<Func<T, T>> CastExpressionT<T>(Expression<Func<TInterface, TInterface>> expression) where T : TInterface
        {
            var parameter = expression.Parameters[0];

            var result = Expression.Lambda<Func<T, T>>(expression.Body, parameter);
            return result;
        }

        private static IQueryable Cast(DbSet dbSet,Type entityType )
        {
            return
                (IQueryable)
                typeof (DbSet).GetMethod("Cast")
                              .MakeGenericMethod(new Type[] {entityType})
                              .Invoke(dbSet, new object[] {});
        }

        private int ListDelete(Expression<Func<TInterface, bool>> filterExpression)
        {
            foreach (var el in this.Where(filterExpression).ToList() )
                Remove(el);
            
            return 1;
        }

        private int GroupDelete(Type entityType,  Expression<Func<TInterface, bool>> filterExpression)
        {
            var deleteMethod = typeof(BatchExtensions)
                        .GetGenericMethod("Delete", new Type[] { typeof(IQueryable<>), typeof(Expression<>) });

            return (int) deleteMethod
                                    .MakeGenericMethod(new Type[] {entityType})
                                    .Invoke(null, new object[] { 
                                        Cast(_inner, entityType), 
                                        typeof (InterfaceQueryableDbSet<TInterface>)
                                                            .GetMethod("CastExpression")
                                                            .MakeGenericMethod(new Type[] {entityType})
                                                            .Invoke(null, new object[] { filterExpression }) 
                                    });
        }

        private int Update(Type entityType, Expression<Func<TInterface, bool>> filterExpression, Expression<Func<TInterface, TInterface>> updateExpression)
        {
            var deleteMethod = typeof(BatchExtensions)
                        .GetGenericMethod("Update", new Type[] { typeof(IQueryable<>), typeof(Expression<>), typeof(Expression<>) });

            return (int)deleteMethod
                                    .MakeGenericMethod(new Type[] { entityType })
                                    .Invoke(null, new object[] { 
                                        Cast(_inner, entityType), 
                                        typeof (InterfaceQueryableDbSet<TInterface>)
                                                            .GetMethod("CastExpression")
                                                            .MakeGenericMethod(new Type[] {entityType})
                                                            .Invoke(null, new object[] { filterExpression }),
                                        typeof (InterfaceQueryableDbSet<TInterface>)
                                                            .GetMethod("CastExpressionT")
                                                            .MakeGenericMethod(new Type[] {entityType})
                                                            .Invoke(null, new object[] { updateExpression }) 
                                    });
        }

        private readonly DbSet _inner;
        public InterfaceQueryableDbSet(DbSet dbSet,Type entityType ) : base(Cast(dbSet,entityType))
        {
            _inner = dbSet;
        }

        public TInterface Add(TInterface entity)
        {
            return (TInterface) _inner.Add(entity);
        }

        public TInterface Attach(TInterface entity)
        {
            return (TInterface)_inner.Attach(entity);
        }

        public TInterface Create()
        {
            return (TInterface)_inner.Create();
        }

        public TInterface Remove(TInterface entity)
        {
            return (TInterface)_inner.Remove(entity);
        }

        public int RemoveAll(int idEntityType, Expression<Func<TInterface, bool>> filterExpression)
        {
            Type entityType;
            
            if (!new TypeLocator().TryGetType(Objects.ById<Entity>(idEntityType), out entityType))
                throw new PlatformException(String.Format("Для сущности с id={0} не удалось обнаружить сущностной класс", idEntityType));

            var controlInvocations = ControlLauncher.GetInvocations(ControlType.Delete, Sequence.Any, entityType);
            if (!controlInvocations.Any())
                return GroupDelete(entityType, filterExpression);

            return ListDelete(filterExpression);
        }

        public int UpdateAll(int idEntityType, Expression<Func<TInterface, bool>> filterExpression, Expression<Func<TInterface, TInterface>> updateExpression)
        {
            Type entityType;

            if (!new TypeLocator().TryGetType(Objects.ById<Entity>(idEntityType), out entityType))
                throw new PlatformException(String.Format("Для сущности с id={0} не удалось обнаружить сущностной класс", idEntityType));

            return Update(entityType, filterExpression, updateExpression);
        }
    }
}
