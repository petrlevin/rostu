using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Caching.Common;

namespace Platform.Environment.Interfaces
{
    public interface IApplicationStorageBase
    {
        IManagedCache Cache { get; set; }
    }
}
