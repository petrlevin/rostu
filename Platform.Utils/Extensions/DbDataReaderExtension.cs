using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;

namespace Platform.Utils.Extensions
{
    public static class DbDataReaderExtension
    {

        public static IEnumerable<T> AsEnumerable<T>(this IDataReader dbReader, Func<IDataRecord, T> fabrica)
        {
            if (dbReader == null)
                throw new ArgumentNullException("dbReader");
            while (dbReader.Read())
            {
                yield return fabrica(dbReader);
            }
 
        }
    }
}
