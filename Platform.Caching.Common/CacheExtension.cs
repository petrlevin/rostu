using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Platform.Caching.Common
{
    static public class CacheExtension
    {

        static public TObject Get<TObject>(this ISimpleCache cache, params Object[] keys) where TObject : class
        {
            return cache.GetByKeys(keys) as TObject;
        }


        static public Object Get(this ISimpleCache cache, params Object[] keys) 
        {
            return cache.GetByKeys(keys);
        }

        static public void DisableAndClear(this IManagedCache cache )
        {
            cache.Enabled = false;
            cache.Clear();
        }

        static public void Put(this ISimpleCache cache, Object value, params Object[] keys)
        {
            cache.PutByKeys(value,keys);
        }

    }
}
