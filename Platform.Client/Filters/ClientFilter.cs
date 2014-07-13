using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Client.Filters
{
    /// <summary>
    /// Клиентский фильтр одного поля
    /// </summary>
    public class ClientFilter
    {
        public string Field { get; set; }
        public List<FilterData> Data { get; set; }
    }
}
