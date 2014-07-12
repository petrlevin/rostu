using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NAnt.Core;
using NAnt.Core.Attributes;
using SharpSvn;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("createdistibutivedatas")]
    public class CreateDistibutiveDatas : SourceTask
    {
        const long StartWorkRevision = 3308;

        public CreateDistibutiveDatas()
        {
            StartRevision = StartWorkRevision;
        }

        

        [TaskAttribute("targetpath", Required = true)]
        public string TargetPath
        {
            get;
            set;
        }

        [TaskAttribute("targeturi", Required = false)]
        public string TargetUri
        {
            get;
            set;
        }

        [TaskAttribute("startrevision", Required = false)]
        public long StartRevision
        {
            get;
            set;
        }

        [TaskAttribute("basenumber", Required = false)]
        public string BaseNumber
        {
            get;
            set;
        }

        protected override void ExecuteTask()
        {
            if ((!Directory.Exists(TargetPath))&&(String.IsNullOrEmpty(TargetUri)))
                FatalFormat("Директория {0} не существует." ,TargetPath);

            try
            {
                DoExecuteTask();
            }
            catch (BuildException)
            {

                throw;
            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при содании данных для дистрибутива" , ex);
            }
            
        }

        private void DoExecuteTask()
        {
            using (var client = new SvnClient())
            {
                Log(Level.Info, "Получение эталонных данных из репозитория");
                LoadTargetData(client);


                Log(Level.Info, "Получение изменений в источнике из репозитория");
                var changes = GetSourceChanges(client);

                if (changes == null) return;

                foreach (SvnLogEventArgs svnLogEventArgse in changes.Reverse())
                {
                    var paths = svnLogEventArgse.ChangedPaths;
                    if (paths.Select(p => p.Path).Any(p => Regex.IsMatch(p)))
                        CreateDistributiveData(client, svnLogEventArgse.Revision);
                }
                Log(Level.Info, "Коммит эталонных данных в репозиторий");
                client.Commit(TargetPath, new SvnCommitArgs() {Depth = SvnDepth.Infinity, LogMessage = "Обновление"});
            }
        }

        private Collection<SvnLogEventArgs> GetSourceChanges(SvnClient client)
        {
            SvnInfoEventArgs info;
            Collection<SvnLogEventArgs> changes = null;
            var args = new SvnLogArgs();
            var startRevision = GetStartRevision(client);
            args.End = new SvnRevision(startRevision);
            client.GetInfo(SourcePath, out info);
            if (info.Revision >= startRevision)
            {
                client.GetLog(SourcePath, args, out changes);
            }
            return changes;
        }

        private void CheckOut(SvnClient client)
        {
            var svnUri = new SvnUriTarget(TargetUri);
            var target = SvnTarget.FromString(TargetUri);
            Collection<SvnInfoEventArgs> info;
            bool result = client.GetInfo(target, new SvnInfoArgs { ThrowOnError = false }, out info);

            if (!result)
                client.RemoteCreateDirectory(new Uri(TargetUri), new SvnCreateDirectoryArgs() { LogMessage = String.Format("Автоматически созданный репозиторий для версии {0}", BaseNumber) });

            client.CheckOut(svnUri, TargetPath);
        }

        private void LoadTargetData(SvnClient client)
        {
            if (!Directory.Exists(TargetPath))
            {
                Directory.CreateDirectory(TargetPath);
                CheckOut(client);
            }
            else
            {
                var targetUri = client.GetUriFromWorkingCopy(TargetPath);
                if (!String.IsNullOrEmpty(TargetUri))
                {
                    if (targetUri == null)
                    {
                        CheckOut(client);
                    }

                    else if (targetUri.AbsoluteUri != TargetUri)
                        FatalFormat("Директория {0} не является локальной копие svn-хранилища {1}.", TargetPath,
                                    TargetUri);
                    else
                    {
                        client.Update(TargetPath);
                    }
                }
                else
                {
                    SvnUpdateResult result;
                    client.Update(TargetPath, out result);
                    if (!result.HasRevision)
                        FatalFormat("Директория {0} не является локальной копией svn-хранилища.", TargetPath);
                }
            }
        }

        private void CreateDistributiveData(SvnClient client,long revision)
        {
            Log(Level.Info, "Создание эталонных данных для ревизии {0}" ,revision);
            var uri = client.GetUriFromWorkingCopy(SourcePath);
            if (uri==null)
                FatalFormat("Директория {0} не является локальной копией svn-хранилища.", SourcePath);
            var tempPath = GetTempPath();
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath,true);
            Directory.CreateDirectory(tempPath);
            try
            {
                GetSourceData(client, revision, uri, tempPath);
                SerializeEnums(tempPath);
                DeleteTestData(tempPath);
                var path = CreateTargetRevisionPath(revision);
                CopyDataFiles(tempPath, path, client);
            }
            catch (InvalidOperationException)
            {

            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }

        private void SerializeEnums(string tempPath)
        {
            var enumSerialize = new EnumSerialize();
            CopyTo(enumSerialize);
            enumSerialize.SourcePath = tempPath;
            enumSerialize.Execute();
        }

        private string CreateTargetRevisionPath(long revision)
        {
            var path = Path.Combine(TargetPath, revision.ToString());
            if (Directory.Exists(path))
                FatalFormat("Директория {0} уже существует", path);
            return path;
        }

        private void DeleteTestData(string tempPath)
        {
            var deleteTestData = new DeleteTestData();
            CopyTo(deleteTestData);
            deleteTestData.SourcePath = tempPath;
            deleteTestData.SaveAndRestore = false;
            deleteTestData.Execute();
        }

        private static  Regex Regex = new Regex(@"((DbStructure\/[\w]+)|(DbEnums))\/[\w\s]+\.[\w]+$", RegexOptions.IgnoreCase|RegexOptions.Compiled);

        private void GetSourceData(SvnClient client, long revision, Uri uri, string tempPath)
        {
            client.CheckOut(uri, tempPath, new SvnCheckOutArgs() {Revision = new SvnRevision(revision), Depth = SvnDepth.Empty});

            
            Collection<SvnListEventArgs> list;
            var gotList = client.GetList(tempPath,
                                         new SvnListArgs() {Revision = new SvnRevision(revision), Depth = SvnDepth.Infinity},
                                         out list);

            if (gotList)
            {
                var paths = new List<String>();
                foreach (SvnListEventArgs item in list)
                {
                    if (Regex.IsMatch(item.Path))
                        paths.Add(Path.Combine(tempPath, item.Path.Replace('/', '\\')));
                }
                client.Update(paths,
                              new SvnUpdateArgs() {Revision = new SvnRevision(revision), UpdateParents = true, KeepDepth = true});
            }

        }

        private void CopyDataFiles(string sourcePath, string destPath , SvnClient client)
        {

            foreach (string file in Directory.EnumerateFiles(sourcePath,"*.xml",SearchOption.AllDirectories))
            {
                var newFile = CopyFile(file, sourcePath, destPath);
                client.Add(newFile, new SvnAddArgs() { AddParents = true, Depth = SvnDepth.Infinity });
            }
        }

        private string CopyFile(string file, string sourcePath, string destPath)
        {
            var fileDastPath = destPath + @"\" + file.Substring(sourcePath.Length + 1);
            if (!Directory.Exists(Path.GetDirectoryName(fileDastPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileDastPath));
            }
            File.Copy(file, fileDastPath);
            return fileDastPath;

        }

        private static string GetTempPath()
        {
            var tempPath = Path.Combine(System.IO.Path.GetTempPath(), "rostu_distrdata"+Guid.NewGuid().ToString());
            return tempPath;
        }


        private long GetStartRevision(SvnClient client)
        {
            long result = Math.Max(StartRevision,StartWorkRevision);

            if (!string.IsNullOrEmpty(BaseNumber))
            {
                Collection<SvnLogEventArgs> changes;
                var args = new SvnLogArgs {StrictNodeHistory = true};
                client.GetLog(SourcePath, args, out changes);
                result = changes.Last().Revision;//Last это первая ревизия
            }
            foreach (string dir in Directory.EnumerateDirectories(TargetPath,"*",SearchOption.TopDirectoryOnly))
            {
                long revision;
                if (Int64.TryParse(new DirectoryInfo(dir).Name, out revision))
                {
                    if (revision > result)
                        result = revision;
                }
            }
            return result+1;
        }

    }
}
