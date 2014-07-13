using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Interfaces;
using Platform.Dal.Common.Interfaces;
using Platform.Environment.Interfaces;

namespace BaseApp.Environment.Storages
{
    /// <summary>
    /// Хранилище уровня запроса
    /// </summary>
    public class RequestStorage : IRequestStorageBase
    {
		/// <summary>
		/// 
		/// </summary>
		public RequestStorage()
		{
			Decorators=new List<TSqlStatementDecorator>();
			Validators=new List<ITSqlStatementValidator>();
		}

        public void ClearDecorators()
        {
            Decorators.Clear();
        }

        /// <summary>
		/// Строка соединения
		/// </summary>
		public SqlConnection DbConnection { get; set; }

		/// <summary>
		/// Активные декораторы
		/// </summary>
		public List<TSqlStatementDecorator> Decorators { get; set; }

        /// <summary>
        /// Активные валидаторы
        /// </summary>
		public List<ITSqlStatementValidator> Validators { get; set; }

        /// <summary>
        /// Текущий дата-контекст
        /// </summary>
        public DbContext DbContext { get; set; }

        /// <summary>
        /// Текущие блокировки
        /// </summary>
        public Locks Locks { get; set; }
        
        /// <summary>
        /// Текущий менеджер контролей
        /// </summary>
        public IControlDispatcher ControlDispatcher { get; set; }



	}
}
