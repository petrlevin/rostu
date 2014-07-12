using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Platform.PrimaryEntities.Common;
using Platform.Utils;

namespace Platform.Web.Services
{
    /// <summary>
    /// Сервис для получения информации о перечислениях на стороне клиента
    /// </summary>
    public class EnumsService
    {
        /// <summary>
        /// Возвращает значения всех перечислений, помеченных атрибутом <see cref="ClientEnumAttribute"/>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, int>> GetEnums()
        {
            var result = new Dictionary<string, Dictionary<string, int>>();
            foreach (Type type in getTypes().Where(t => t.IsEnum && t.GetCustomAttribute<ClientEnumAttribute>() != null))
            {
                var enumDict = new Dictionary<string, int>();

                foreach (object item in Enum.GetValues(type))
                {
                    enumDict.Add(item.ToString(), (int)item);
                }

                result.Add(type.Name, enumDict);
            }

            return result;
        }

        private List<Type> getTypes()
        {
            var types = new List<Type>();
            foreach (Assembly assembly in Assemblies.All())
            {
                try
                {
                    types.AddRange(assembly.GetTypes());
                }
                catch (Exception)
                {
                }
            }
            return types;
        }
    }
}