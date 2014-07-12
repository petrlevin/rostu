using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NAnt.Core;

namespace Tools.MigrationHelper.Core.Controls
{
    public static class Assemblies
    {
        [ThreadStatic]
        private static List<Assembly> _assemblies;
        public static List<Assembly> Get(string sourcePath,Action<Exception,String> onLoadException)
        {
            if (_assemblies != null)
                return _assemblies;

            var copiedAss = new List<String>();

            var path = String.Format("{0}\\Platform.Web\\bin\\", sourcePath);

            var tempPath = Path.Combine(System.IO.Path.GetTempPath(), "rostu_all_bins");
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath,true);

            Directory.CreateDirectory(tempPath);

            Directory.EnumerateFiles(path, "*.dll").ToList().ForEach(fn =>
            {
                var name = new FileInfo(fn).Name;
                var dest = Path.Combine(
                    tempPath, name);
                if (File.Exists(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, name)))
                    return;

                File.Copy(fn, dest, false);
                copiedAss.Add(dest);
            });

            //            var la = AppDomain.CurrentDomain.GetAssemblies();


            var asmbls = new List<Assembly>();
            Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").ToList().ForEach(fn =>
            {
                try
                {
                    var curan = AssemblyName.GetAssemblyName(fn);
                    var ass =
                        AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == curan.FullName);
                    if (ass == null)
                        ass = Assembly.Load(curan);

                    asmbls.Add(ass);


                }
                catch (BadImageFormatException)
                {

                }

            });

            Directory.EnumerateFiles(tempPath, "*.dll").ToList().ForEach(fn =>
            {
                //la.FirstOrDefault(a => a.Location == fn);
                try
                {
                    var curan = AssemblyName.GetAssemblyName(fn);
                    //var ass =
                    //    AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == curan.FullName) ??
                    //    Assembly.Load(curan);
                    var ass =
                        AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == curan.FullName);
                    if (ass == null)
                    {
                        ass = Assembly.LoadFrom(fn);
                    }
                    asmbls.Add(ass);

                }
                catch (BadImageFormatException)
                {

                }
                catch (Exception ex)
                {
                    onLoadException(ex,fn);
                    

                    //ErrorFormat(
                    //    "Ошибка при загрузке сборки {0} .  {1} - {2} ", fn, ex.GetType(), ex.Message);
                    //Log(Level.Debug, ex.StackTrace);
                }
            });
            _assemblies = asmbls;
            return asmbls;
        }

    }
}
