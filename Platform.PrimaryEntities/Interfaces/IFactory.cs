using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Exceptions;

namespace Platform.PrimaryEntities.Interfaces
{
    public interface IFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="DbFactoryException"></exception>
        /// <exception cref="InvalidOperationException">Не верный параметр типа объекта </exception>
        TObject ById<TObject>(int id) where TObject : Metadata;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="DbFactoryException"></exception>
        /// <exception cref="InvalidOperationException">Не верный параметр типа объекта </exception>
        TObject ByName<TObject>(string name) where TObject : Metadata;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TParent"></typeparam>
        /// <param name="parentId"></param>
        /// <returns></returns>
        /// <exception cref="DbFactoryException"></exception>
        /// <exception cref="InvalidOperationException">Не верный параметр типа объекта </exception>
        IList<TObject> ChildsById<TObject, TParent>(int parentId) where TObject : Metadata;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        /// <exception cref="DbFactoryException"></exception>
        /// <exception cref="InvalidOperationException">Не верный параметр типа объекта </exception> 
        TObject Create<TObject>() where TObject : Metadata;
    };


}
