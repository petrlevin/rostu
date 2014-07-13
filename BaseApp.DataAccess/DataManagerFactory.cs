using System;
using System.Data.SqlClient;
using Platform.BusinessLogic;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Interfaces;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DataAccess
{
	/// <summary>
	/// 
	/// </summary>
	public class DataManagerFactory :IDataManagerFactory
	{
	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="source"></param>
	    /// <returns></returns>
	    public DataManager CreateManager(IEntity source)
	    {
	        if (!source.UseEntityFrameworkByDefault())
	            return new DalDataManager(IoC.Resolve<SqlConnection>("DbConnection"), source);
	        else
	            return CreateEfDataManager(source);
	        
	    }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public DataManager CreateManager(IEntity source, Form form)
        {
            if (!source.UseEntityFrameworkByDefault())
                return new DalDataManager(IoC.Resolve<SqlConnection>("DbConnection"), source, form);
            else
                return CreateEfDataManager(source);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TDataManager"></typeparam>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public TDataManager CreateManager<TDataManager>(IEntity source) where TDataManager : DataManager
        {
            if ((!source.HasEntityClass()) && (typeof(TDataManager) != typeof(DalDataManager)))
                throw new PlatformException(String.Format("Для сущностей не имеющих сущностных классов возможно использование только \"DalDataManager\". Сущность  - {0} (id - {1}) ", source.Name, source.Id));
            if (typeof(TDataManager)==typeof(DalDataManager))
                return new DalDataManager(IoC.Resolve<SqlConnection>("DbConnection"), source) as TDataManager;

            var result = CreateEfDataManager(source) as TDataManager;
            if (result == null)
                throw new PlatformException(String.Format("Для сущности {0} (id - {1} , тип сущности -{2}) использование  {3} невозможно ) ", source.Name, source.Id, source.EntityType, typeof(TDataManager).Name));
            return result;
        }


        private static DataManager CreateEfDataManager(IEntity source)
        {
            if ((source.EntityType == EntityType.Document) || (source.EntityType == EntityType.Tool))
                return new BaseApp.DataAccess.ToolsDataManager(IoC.Resolve<SqlConnection>("DbConnection"), source);
            else
                return new EFDataManager(IoC.Resolve<SqlConnection>("DbConnection"), source);
        }


	}
}
