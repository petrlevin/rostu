using System;
using Platform.BusinessLogic.Interfaces;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.DataAccess
{
	public static class DataManagerFactory
	{
		public static DataManager Create(IEntity source)
		{
			return IoC.Resolve<IDataManagerFactory>().CreateManager(source);
		}

        public static DataManager Create(IEntity source, Form form)
        {
            return IoC.Resolve<IDataManagerFactory>().CreateManager(source, form);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TDataManager"></typeparam>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public static TDataManager Create<TDataManager>(IEntity source) where TDataManager : DataManager
        {
            return IoC.Resolve<IDataManagerFactory>().CreateManager<TDataManager>(source);
        }

	}
}