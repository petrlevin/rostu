using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Platform.Utils.Extensions;

namespace Platform.Utils
{
    public static class Assemblies
    {
        private static  bool _loaded = false;
        private static Object _lock = new object();

        private static void LoadAllAssemblies()
        {

            lock (_lock)
            {

                if (_loaded)
                    return;
                var path = Directory.GetParent(new Uri(Assembly.GetCallingAssembly().CodeBase).LocalPath);

                path.EnumerateFiles("*.dll").ToList().ForEach(s =>
                                                                  {

                                                                      try
                                                                      {
                                                                          var assName =
                                                                              AssemblyName.GetAssemblyName(s.FullName);
                                                                          if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assName.FullName) == null)  
                                                                              Assembly.Load(assName);
                                                                      }
                                                                      catch
                                                                      {

                                                                      }

                                                                  }
                    );
                _loaded = true;
            }
        }


        public static Assembly[] All()
        {
            LoadAllAssemblies();
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public static IEnumerable<Type> AllTypes()
        {
            var result = new List<Type>();
            foreach (Assembly assembly in All())
            {
                  result.AddRange(assembly.AllTypes());
            }
            return result;
        }


        public static IEnumerable<Type> AllTypes<TBaseType>(TypeOptions options = TypeOptions.All)
        {
            var result = new List<Type>();
            foreach (Assembly assembly in All())
            {
                result.AddRange(assembly.AllTypes<TBaseType>(options));
            }
            return result;
        }

    }

}
