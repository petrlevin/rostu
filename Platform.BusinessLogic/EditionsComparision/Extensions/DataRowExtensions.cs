using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.EditionsComparision.Extensions
{
    public static class DataRowExtensions
    {
        public static int GetId(this DataRow row)
        {
            return (int)row["Id"];
        }
    }
}
