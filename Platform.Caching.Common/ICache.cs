using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace Platform.Caching.Common
{



    public interface ICache : ISimpleCache
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyCommand">
        /// Ограничения на команду  dependencyCommand
        /// All the columnnames must be explicitly stated in the query. So use Select Col1, Col2 from Table rather than select * from Table. Hence selection cannot include * and TableName.* in the query.
        /// Table must use two part name, use dbo.TableName rather than TableName
        /// Unnamed or duplicate columns are not allowed
        /// Reference to a table with ComputedColumns are not allowed
        /// When you need aggregate column subscription, you must use a GROUPBY. Cube, Rollup or Having is not allowed
        /// Statement must not contain Pivot or Unpivot operators
        /// Union, Intersect and except is not allowed
        /// Statement should not contain a reference of a View
        /// It should not contain Distinct, Compute or Into
        /// NText, Text, Image Type in the query for notification is not allowed
        /// Rowset functions like OpenRowset or OpenQuery are not allowed
        /// Statement must not refer to a service broker Queue
        /// Top expression is also not allowed in the query.
        /// Set NoCount ON will invalidate the usage of Query Notification in a stored procedure
        /// Reference to server global variables (@@variableName) must also be excluded from the queries        
        /// !!! Если команда  не корректная то АСИНХРОННО выкидывается исключение InvalidCommandException
        /// </param>
        /// <param name="value"></param>
        /// <param name="keys"></param>
        /// <exception cref="ArgumentNullException">dependencyCommand</exception>
        
        /// <exception cref="ArgumentException"> "В кэш не передано ни одного ключа. Не возможно закэшировать данные"</exception>
        void Put(SqlCommand dependencyCommand,Object value, params Object[] keys);
        

    }
}
