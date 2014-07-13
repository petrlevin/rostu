using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.Client.Filters
{
    /// <summary>
    /// { field:ИмяПоля, data:[{...}, {...}, ...] }, где {...} - это FilterData
    /// </summary>
    public class FilterData
    {
        public ComparisionOperator Comparison { get; set; }
        public object Value { get; set; }
    }
}
