using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public class CUDListener<TTarget>
    {
        public virtual void OnAfterUpdate(TTarget target)
        {
            
        }

        public virtual void OnBeforeUpdate(TTarget target)
        {

        }

        public virtual void OnBeforeInsert(TTarget target)
        {

        }

        public virtual void OnAfterInsert(TTarget target)
        {

        }

        public virtual void OnBeforeDelete(TTarget target)
        {

        }

        public virtual void OnAfterDelete(TTarget target)
        {

        }
    }
}
