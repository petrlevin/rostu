using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.PrimaryEntities.Interfaces
{
    public interface ISelectProvider<TData>
    {
        ISelect<TData> GetSelect(string parameterName, object parameter );
    }
}
