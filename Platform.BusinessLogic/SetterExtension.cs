using System;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Values;
using Platform.BusinessLogic.Common.Exceptions;

namespace Platform.BusinessLogic
{
    /// <summary>
    /// Установка значений свойств
    /// </summary>
    static public class SetterExtension
    {
        /// <summary>
        /// Установить значение свойства<paramref name="valueName"/>  для объекта <paramref name="target"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        /// <exception cref="ValueExecutionException"></exception>
        /// <exception cref="ValueResolutionException"></exception>
        static public void SetValue(this object target, string valueName, object value)
        {
            new Setter().Set(target, valueName, value);
        }

        /// <summary>
        /// Установить значение свойства<paramref name="valueName"/>  для объекта <paramref name="target"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <exception cref="ValueExecutionException"></exception>
        /// <exception cref="ValueResolutionException"></exception>
        static public void SetValue(this object target, string valueName, object value, Type valueType )
        {
            new Setter().Set(target, valueName, value, valueType);
        }

        /// <summary>
        /// Установить значения свойств для объекта <paramref name="target"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="values"></param>
        /// <param name="ignoreAbsentProperies"></param>
        /// <exception cref="ValueExecutionException"></exception>
        /// <exception cref="ValueResolutionException"></exception>
        static public void SetValues(this object target, Dictionary<string,object> values ,  bool emptyStringAsNullOnConvertError = false ,bool ignoreAbsentProperies=false )
        {
            new Setter(ignoreAbsentProperies, emptyStringAsNullOnConvertError).Set(target, values);
        }

    }
}
