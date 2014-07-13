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
    public class CustomFilterGridParams : GridParams
    {
        /// <summary>
        /// Идентификатор сущности по которой накладывается фильтр
        /// </summary>
        public int FilterEntityId { get;set; }

        /// <summary>
        /// Идентификатор значения по которому накладывается фильтр
        /// </summary>
        public int FilterValueId { get; set; }
    }
}
