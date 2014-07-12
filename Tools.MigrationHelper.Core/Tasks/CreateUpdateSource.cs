using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAnt.Core;
using NAnt.Core.Attributes;
using Platform.PrimaryEntities.DbEnums;
using SharpSvn;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
	/// <summary>
	/// Создание пакета обновления
	/// </summary>
	[TaskName("createupdatesource")]
	public class CreateUpdateSource : DeployBase
	{
		[TaskAttribute("startrevision", Required = true)]
        public long StartRevision
		{
			get;
			set;
		}

		[TaskAttribute("endrevision", Required = true)]
		public long EndRevision
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

	    protected override void ExecuteTask()
	    {
	        long currentRevision;
	        using (SvnClient client = new SvnClient())
	        {
	            SvnInfoEventArgs info;
	            
                client.GetInfo(SourcePath, out info);
	            if (info == null)
	            {
	                client.CreateDirectory(SourcePath);
                    client.GetInfo(SourcePath, out info);
	            }

	            currentRevision = info.Revision;
	        }

	        var scriptsDir =
	            new DirectoryInfo(Path.Combine(SourcePath, SolutionProject.Sbor.ToString().Replace('_', '.'), "Scripts"));

	        List<long> revisions =
                scriptsDir.GetFiles("*.sql").Select(s => long.Parse(Path.GetFileNameWithoutExtension(s.Name))).ToList();

            //если нет последней, добавляем
	        if (!revisions.Contains(EndRevision))
	            revisions.Add(EndRevision);

	        if (!Directory.Exists(DistrData))
	            Directory.CreateDirectory(DistrData);

            var dirs = new DirectoryInfo(DistrData).GetDirectories().Where(w => (w.Attributes & FileAttributes.Hidden) == 0);
            
	        foreach (var revision in revisions)
	        {
	            //Если файл не попадает в заданые рамки ревизий, пропускаем
	            if (revision < StartRevision || revision > EndRevision)
	                continue;
	            Log(Level.Info, "Ревизия №{0}", revision);
	            var revisionPath = Path.Combine(OutputDir, revision.ToString());
	            Directory.CreateDirectory(revisionPath);

                //берем первую 
                var revisionDataDir = dirs.Where(d => int.Parse(d.Name) <= revision).Max(m=> m.Name);
	            var dir = dirs.FirstOrDefault(f => f.Name == revisionDataDir);

                if (dir != null)
	            {
                    Log(Level.Info, "Копирование данных");
                    SolutionHelper.Copy(dir.FullName, revisionPath);
	            }
	            else
	            {
                    Fatal(string.Format("Не найдена папка с данными в {0}", DistrData));
	            }
                Log(Level.Info, "Копирование sql скриптов");
                Directory.CreateDirectory(Path.Combine(revisionPath, "Scripts"));
	            var script = scriptsDir.GetFiles(revision.ToString() + "*").SingleOrDefault();
	            if (script != null)
	                script.CopyTo(Path.Combine(revisionPath, "Scripts", script.Name), true);
	        }

	    }
	}
}
