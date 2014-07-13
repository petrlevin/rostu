using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Dal.Decorators.Abstract;

namespace Platform.Dal.Decorators
{
    public class WithParentsEventArgs : EventDataList<bool>
    {
        public WithParentsEventArgs(IEnumerable<bool> values): base(values)
        {
        }
    }
}
