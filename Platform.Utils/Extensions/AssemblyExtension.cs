using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Extensions
{
    public static class AssemblyExtension
    {
        public static IEnumerable<Type> AllTypes(this Assembly assembly)
        {
            var result = new List<Type>();
            try
            {
                result.AddRange(assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException)
            {
            }
            return result;
        }


    


        public static IEnumerable<Type> AllTypes<TBaseType>(this Assembly assembly,TypeOptions options = TypeOptions.All )
        {
            return AllTypes(assembly).WhitchInherit(typeof (TBaseType), options);
            
        }
    }
}
