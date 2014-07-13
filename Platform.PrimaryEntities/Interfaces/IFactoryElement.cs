using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Exceptions;

namespace Platform.PrimaryEntities.Interfaces
{
    public interface IFactoryElement<out TObject>: IBaseFactoryElement<TObject>
    {

        /// <summary>
        /// Создает объект по имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="DbFactoryException"></exception>
        TObject CreateByName(string name);

        /// <summary>
        /// Создает объект типа <typeparam name="TObject"></typeparam>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectCreationException"></exception>
        TObject CreateObject();


    }
}
