using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Logic
{
    public class BlankVersioningProperties
    {
        public BlankVersioningProperties()
        {
            MandatoryVersioning = new List<string>();
            OptionalVersioning = new List<string>();
            MandatoryNonVersioned = new List<string>();
            OptionalNonVersioned = new List<string>();
        }

        /// <summary>
        /// Обязательные версионные поля
        /// </summary>
        public List<string> MandatoryVersioning { get; set; }

        /// <summary>
        /// Необязательные версионные поля
        /// </summary>
        public List<string> OptionalVersioning { get; set; }
        
        /// <summary>
        /// Обязательные неверсионные поля
        /// </summary>
        public List<string> MandatoryNonVersioned { get; set; }

        /// <summary>
        /// Необязательные неверсионные поля
        /// </summary>
        public List<string> OptionalNonVersioned { get; set; }
    }
}

