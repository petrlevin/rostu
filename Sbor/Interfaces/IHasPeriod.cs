using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Interfaces
{
    public interface IHasPeriod
    {
        /// <summary>
        /// Срок реализации с
        /// </summary>
        DateTime DateStart { get; set; }

        /// <summary>
        /// Срок реализации по
        /// </summary>
        DateTime DateEnd { get; set; }
    }
}
