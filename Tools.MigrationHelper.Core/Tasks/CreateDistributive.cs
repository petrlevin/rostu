using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NAnt.Core;
using NAnt.Core.Attributes;
using Platform.PrimaryEntities.DbEnums;
using SharpSvn;
using Tools.MigrationHelper.Core.Tasks;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Tasks
{
    [TaskName("createdistributive")]
    public class CreateDistributive : DeployBase
    {
        [TaskAttribute("startrevision", Required = false)]
        public string StartRevision
        {
            get;
            set;
        }

        [TaskAttribute("endrevision", Required = false)]
        public string EndRevision
        {
            get;
            set;
        }

        [TaskAttribute("outputdir", Required = true)]
        public string OutputDir
        {
            get;
            set;
        }

        [TaskAttribute("distrdata", Required = true)]
        public string DistrData
        {
            get;
            set;
        }

        [TaskAttribute("minrevision", Required = false)]
        public string MinRevision
        {
            get;
            set;
        }

        private string MigrationHelperDir {
            get { return Path.Combine(OutputDir, "Migration"); }
        }

        private string DeployBinDir
        {
            get { return Path.Combine(OutputDir, @"bin\"); }
        }

        private string ReportsDir
        {
            get { return Path.Combine(SourcePath, SolutionProject.Sbor_Reports.ToString().Replace('_','.')); }
        }

        private string UpdateDir
        {
            get { return Path.Combine(OutputDir, string.Format("Update.{0}-{1}", string.IsNullOrEmpty(MinRevision) ? StartRevision : MinRevision, EndRevision)); }
        }

        protected override void ExecuteTask()
        {
	        try
	        {
		        var outDir = new DirectoryInfo(OutputDir);
		        Log(Level.Verbose, "Создании директории {0}", OutputDir);
		        if (!outDir.Exists)
			        Directory.CreateDirectory(OutputDir);
                
		        Log(Level.Verbose, "Копирование файлов dll...", OutputDir);
		        WhriteAssebly();

                //Копирование отчетов и печатных форм
	            CopyReportsAndPF();

                //Копирование самого MigrationHelper
		        CopyExe();

	            CopyWebConfig();

                //Вызывается таск для создания пакетов обновления на несколько ревизий
                CreateUpdateSource();
	        }
	        catch (BuildException)
	        {
		        throw;
	        }
	        catch (Exception ex)
	        {
		        Fatal("Фатальная ошибка при созданиии дистрибутива", ex);
	        }
        }

        private void CopyWebConfig()
        {
            var directory = new DirectoryInfo(Path.Combine(SourcePath, PlatformWeb));

            var file = directory.GetFiles(WebTransformed).SingleOrDefault();

            if (file != null)
            {
                file.CopyTo(Path.Combine(OutputDir,WebConfig), true);
            }
        }

        private void CopyReportsAndPF()
        {
            foreach (var dir in new DirectoryInfo(ReportsDir).GetDirectories())
            {
                //папку bin пропускаем
                if(dir.Name == "bin")
                    continue;
                if (dir.GetFiles(RdclPattern, SearchOption.AllDirectories).Any())
                {
                    SolutionHelper.CopyWithRoot(dir.FullName, DeployBinDir, RdclPattern);
                }
            }
        }

        private void CreateUpdateSource()
        {
            Directory.CreateDirectory(UpdateDir);
            if (!string.IsNullOrEmpty(StartRevision) && !string.IsNullOrEmpty(EndRevision))
            {
                var source = new CreateUpdateSource
                    {
                        DevId = DevId,
                        EndRevision = long.Parse(EndRevision),
                        StartRevision = GetStartRevision(),
                        SourcePath = SourcePath,
                        OutputDir = UpdateDir,
                        DistrData = DistrData
                    };
                CopyTo(source);
                source.Execute();
            }
        }

        private void WhriteAssebly()
	    {
			var directoryInfo2 = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\PlatformDb\Scripts");
			FileInfo fileInfo2 = directoryInfo2.GetFiles("Assemblies.list")[0];
			string[] listAssemblies = fileInfo2.OpenText()
												  .ReadToEnd()
												  .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Directory.CreateDirectory(DeployBinDir);
		    foreach (string assembly in listAssemblies)
		    {
				var directoryInfo = new DirectoryInfo(SourcePath + @"\bin");
				if (!directoryInfo.Exists)
				{
					directoryInfo = new DirectoryInfo(SourcePath + @"\" + assembly.Replace(".dll", @"\bin\debug"));
					if (!directoryInfo.Exists)
						throw new Exception("Не найден путь: '" + SourcePath + @"\" +
											assembly.Replace(".dll", @"\bin\debug") + "'");
				}
				FileInfo file;
				if (directoryInfo.GetFiles(assembly).Length == 1)
				{
					file = directoryInfo.GetFiles(assembly)[0];
				}
				else
				{
					throw new Exception("Не найден файл: '" + directoryInfo.FullName + @"\" + assembly);
				}
                file.CopyTo(DeployBinDir + file.Name, true);
		    }
	    }

	    private void CopyExe()
	    {
            var startPath = Path.Combine(SourcePath, ToolsMigrationHelper);
		    
	        Directory.CreateDirectory(MigrationHelperDir);

		    foreach (var fileName in Names.FilesCopyToDistr)
		    {
			    var file = new FileInfo(Path.Combine(startPath, fileName));
                file.CopyTo(Path.Combine(MigrationHelperDir,file.Name), true);
		    }

			var dirDebug = new DirectoryInfo(Path.Combine(startPath, BinDebug));
			var files = dirDebug.GetFiles();

            var outputBinDebug = Path.Combine(MigrationHelperDir, BinDebug);
	        Directory.CreateDirectory(outputBinDebug);

		    foreach (var file in files)
		    {
				file.CopyTo(Path.Combine(outputBinDebug, file.Name), true);
		    }

            startPath = Path.Combine(SourcePath, ToolsMigrationHelperCore);
			var dirScripts = Path.Combine(startPath, @"PlatformDb");
            var destinationPath = Path.Combine(MigrationHelperDir, BinDebug, @"PlatformDb");

            SolutionHelper.Copy(dirScripts, destinationPath);
	    }

        private long GetStartRevision()
        {
            long result;
            using (var client = new SvnClient())
            {
                Collection<SvnLogEventArgs> changes;
                var args = new SvnLogArgs {StrictNodeHistory = true};
                client.GetLog(SourcePath, args, out changes);
                var svnFirstRevision = changes.Last().Revision; //Last это первая ревизия
                result = Math.Max(long.Parse(StartRevision), svnFirstRevision);
            }
            return result;
        }

        private const string BinDebug = @"bin\Debug";
        private const string RdclPattern = "*.rdlc";
        private const string WebConfig = "Web.config";
        private const string WebTransformed = "Web.Transformed.config";
        private const string PlatformWeb = "Platform.Web";
        private const string ToolsMigrationHelper = "Tools.MigrationHelper";
        private const string ToolsMigrationHelperCore = ToolsMigrationHelper + ".Core";
    }
}
