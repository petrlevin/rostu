using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Registry
{
    /// <summary>
    /// Информация о записях в регисте
    /// </summary>
    public class RecordsInfo
    {
        /// <summary>
        /// Русское наименование регистра
        /// </summary>
        public string Caption { get; set; }

        public int Id { get; set; }


        /// <summary>
        /// Количество записей
        /// </summary>
        public int Count { get; set; }


    }
}
