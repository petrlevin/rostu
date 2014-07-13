using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Exceptions;

namespace Platform.PrimaryEntities.Interfaces
{
    public interface IBaseFactoryElement<out TObject>
    {
        /// <summary>
        /// Создает объект по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="DbFactoryException"></exception>
        TObject CreateById(int id);

    }


}
