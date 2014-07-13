using System.Collections.Generic;
using System.Reflection;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Common.Exceptions
{
    /// <summary>
    /// Тип не  найден в сборке
    /// </summary>
    public class TypeLocationException :PlatformException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="assemblies"></param>
        /// <param name="typeFullName"></param>
        public TypeLocationException(string message,string typeFullName):base(message)
        {
            TypeFullName = typeFullName;


        }

        /// <summary>
        /// тип который не был найден в сборке
        /// </summary>
        public string TypeFullName { get; private set; }

    }
}
