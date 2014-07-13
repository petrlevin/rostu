using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic
{
    public abstract class ConnectionString<T> where T : ConnectionString<T>, new()
    {
        protected ConnectionString()
        {
            Builder = get();
        }

        #region Singleton

        private static T instance;

        public static T Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }

        #endregion


        protected abstract SqlConnectionStringBuilder get();

        public SqlConnectionStringBuilder Builder { get; private set; }
    }
}
