using Platform.BusinessLogic.DataAccess;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDataManagerFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		DataManager CreateManager(IEntity source);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        DataManager CreateManager(IEntity source, Form form);

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="source"></param>
	    /// <typeparam name="TDataManager"></typeparam>
	    /// <returns></returns>
	    TDataManager CreateManager<TDataManager>(IEntity source) where TDataManager : DataManager;
	}
}