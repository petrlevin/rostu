using System;
using BaseApp.Common.Interfaces;
using BaseApp.SystemDimensions;

namespace BaseApp.Environment.Storages
{
	/// <summary>
	/// Хранилище уровня сессии
	/// </summary>
	public class SessionStorage 
	{
		/// <summary>
		/// Пользователь
		/// </summary>
		public IUser CurrentUser { get; set; }

        /// <summary>
        /// Системные измерения
        /// </summary>
        public SysDimensionsState CurentDimensions { get; set; }
	    
        /// <summary>
        /// Идентификатор сессии
        /// </summary>
        public Guid Id { get; set; }
	};
}
