using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Platform.Caching.Common;

namespace Platform.PrimaryEntities.Factoring
{
    public class CacheStub: ICache
    {
        public void PutByKeys(object value, params object[] keys)
        {
            
        }

        public object GetByKeys(object[] keys)
        {
            return null;
        }

        public void Put(SqlCommand dependencyCommand, object value, params object[] keys)
        {
           
        }
    }
}
