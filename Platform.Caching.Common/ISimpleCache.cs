using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace Platform.Caching.Common
{



    public interface ISimpleCache
    {
        void PutByKeys(Object value, Object[] keys);

        Object GetByKeys(Object[] keys);

    }
}
