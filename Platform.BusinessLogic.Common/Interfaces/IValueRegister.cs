using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IValueRegister : ICommonRegister
    {
        /// <summary>
        /// Тип значения
        /// </summary>
        byte IdValueType { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        decimal Value { get; set; }
    }
}
