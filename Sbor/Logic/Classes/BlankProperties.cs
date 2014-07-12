using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Logic
{
    /// <summary>
    /// Поля бланка СБП 
    /// </summary>
    public class BlankProperties
    {
        public BlankProperties()
        {
            Mandatory = new List<string>();
            Denied = new List<string>();
            Optional = new List<string>();
        }

        /// <summary>
        /// Обязательные поля
        /// </summary>
        public List<string> Mandatory { get; set; }

        /// <summary>
        /// Запрещенные поля
        /// </summary>
        public List<string> Denied { get; set; }
        
        /// <summary>
        /// Необязательные поля
        /// </summary>
        public List<string> Optional { get; set; }
    
    }
}
