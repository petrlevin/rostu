using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Registry;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp
{
    /// <summary>
    /// 
    /// </summary>
    public static class StartedOperationsExtensions
    {
        /// <summary>
        /// Получить начатаю операцию для документа или инструмента
        /// </summary>
        /// <param name="dbSet"></param>
        /// <param name="document"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        /// <exception cref="ToolStateException"></exception>
        static public StartedOperation For(this DbSet<StartedOperation> dbSet, ToolEntity document, StartedOperationInclude include = StartedOperationInclude.None)
        {
            try
            {
                return GetQuery(dbSet, include).SingleOrDefault(so => so.IdRegistratorEntity == document.EntityId && so.IdRegistrator == document.Id);
            }
            catch (InvalidOperationException)
            {
                throw new ToolStateException(String.Format("Обнаружено более чем одна  начатая неатомарная  операция для  {0}. Некорректное состояние. ", document), document);
            }
        }

        /// <summary>
        /// Получить начатаю операцию для документа или инструмента 
        /// </summary>
        /// <param name="dbSet"></param>
        /// <param name="entity"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        /// <exception cref="ToolStateException"></exception>
        static public StartedOperation For(this DbSet<StartedOperation> dbSet, IEntity entity, int documentId, StartedOperationInclude include = StartedOperationInclude.None)
        {
            try
            {
                return GetQuery(dbSet,include).SingleOrDefault(so => so.IdRegistratorEntity == entity.Id && so.IdRegistrator == documentId);
            }
            catch (InvalidOperationException)
            {
                throw new ToolStateException(String.Format("Обнаружено более чем одна  начатая неатомарная операция для {0} {1} id - {2}. Некорректное состояние. ", entity.EntityType ==EntityType.Document?"документа" : "инструмента" ,entity.Caption ,documentId ));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbSet"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static bool Any(this DbSet<StartedOperation> dbSet, ToolEntity document)
        {
            return dbSet.Any(so => so.IdRegistratorEntity == document.EntityId && so.IdRegistrator == document.Id);
        }


        public static bool Any(this DbSet<StartedOperation> dbSet, IEntity entity, int documentId)
        {
            return dbSet.Any(so => so.IdRegistratorEntity == entity.Id && so.IdRegistrator == documentId);
        }

        private static IQueryable<StartedOperation> GetQuery(IQueryable<StartedOperation> query, StartedOperationInclude include)
        {
            var result = query;
            if ((include & StartedOperationInclude.EntityOperation) == StartedOperationInclude.EntityOperation)
                result = result.Include(so => so.EntityOperation);
            if ((include & StartedOperationInclude.ToolEntity) == StartedOperationInclude.ToolEntity)
                result = result.Include(so => so.RegistratorEntity);
            if ((include & StartedOperationInclude.User) == StartedOperationInclude.User)
                result = result.Include(so => so.User);
            if ((include & StartedOperationInclude.Operation) == StartedOperationInclude.Operation)
                result = result.Include(so => so.EntityOperation.Operation);
            if ((include & StartedOperationInclude.EditableFields) == StartedOperationInclude.EditableFields)
                result = result.Include(so => so.EntityOperation.EditableFields);
            return result;
        }
    }
    

    /// <summary>
    /// Опции получения StartedOperation
    /// </summary>
    [Flags]
    public enum StartedOperationInclude
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// 
        /// </summary>
        EntityOperation = 1,
        /// <summary>
        /// 
        /// </summary>
        ToolEntity = 2,
        /// <summary>
        /// 
        /// </summary>
        User = 4,
        /// <summary>
        /// 
        /// </summary>
        EditableFields = EntityOperation | 8,
        /// <summary>
        /// 
        /// </summary>
        Operation = EntityOperation | 16,
    }

}
