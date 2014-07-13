using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Platform.PrimaryEntities.Interfaces
{
    public interface ISelect<TData>
    {
        IEnumerable<TData> Execute();
    }

 }
