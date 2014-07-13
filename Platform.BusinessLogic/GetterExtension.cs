using System;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Values;

namespace Platform.BusinessLogic
{
    /// <summary>
    /// получение  значений свойств  объекта
    /// </summary>
    static public class GetterExtension
    {

        /// <summary>
        /// получить словарь скалярных свойств объекта
        /// </summary>

        /// <param name="source"></param>
        /// <param name="notDefault">признак получения значений отличных от дефолтных (Default(T))</param>
        /// <returns></returns>
        static public Dictionary<String, Object> GetScalarValues(this object source,
                                                 bool notDefault = false)
        {

            return GetValues(source, Helper.IsScalar, notDefault);

        }

        /// <summary>
        /// Получить значение свойства или результата вызова метода <seealso cref="Options"/>  для объекта 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="valueName"></param>
        /// <param name="options">IgnoreCase , UseProperty , UseMethod</param>
        /// <returns></returns>
        static public Object GetValue(this object source, string valueName, Options options = Options.Default)
        {
            return new Getter().Get(source, valueName, options);
        }

        /// <summary>
        /// Получить  значенния всех свойств объекта тип которых удовлетворят фильтру <paramref name="propertyTypeMatch"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyTypeMatch">тип свойства (не значения)</param>
        /// <param name="notDefault">признак получения значений отличных от дефолтных (Default(T))</param>
        /// <returns></returns>
        static public Dictionary<String, Object> GetValues(object source, Func<Type, bool> propertyTypeMatch, bool notDefault = false)
        {
            return new Getter().GetAll(source, propertyTypeMatch, notDefault);
        }


    }
}
