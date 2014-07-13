using Platform.Caching.Common;
using Platform.Environment.Interfaces;

namespace BaseApp.Environment.Storages
{
    /// <summary>
    /// Хранилище уровня приложения
    /// </summary>
    public class ApplicationStorage : IApplicationStorageBase
	{
        /// <summary>
        /// Кэш
        /// </summary>
        public IManagedCache Cache { get; set; }
        
        /// <summary>
        /// Строка соединения
        /// </summary>
        public string ConnectionString { get; set; }
	}
}
