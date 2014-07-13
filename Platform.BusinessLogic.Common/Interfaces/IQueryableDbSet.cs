using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IQueryableDbSet<T>:IQueryable<T>
    {
        T Add(T entity);
        T Attach(T entity);
        T Create();
        T Remove(T entity);
        int RemoveAll(int idEntityType, Expression<Func<T, bool>> filterExpression);
        
    }
}
