using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ionic.Zip;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.Context;
using Tools.MigrationHelper.Core.DbManager;
using Tools.MigrationHelper.Core.Helpers;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("update")]
    public class UpdateTask : DbDeployTask
    {
        [TaskAttribute("targetpath", Required = false)]
        public string TargetPath
        {
            get;
            set;
        }

        private UpdateRevisionContext _context;
        private DateTime _updateTime;
        private DirectoryInfo _updateDir;
        private readonly List<string> _tempPaths;

        public UpdateTask()
        {
            _tempPaths = new List<string>();
        }

        protected override void ExecuteTask()
        {
            Log(Level.Info, "Начало обновления...");
            Log(Level.Info, "Строка соединения: "+ ConnectionString);
            string targetPath;
            var sourceDir = new DirectoryInfo(SourcePath);
            _context = new UpdateRevisionContext(ConnectionString);
            GetUpdateDir(sourceDir);
            var connection = new SqlConnection(ConnectionString); connection.Open();

            //Выполнение предварительного скрипта
            TaskHelper.ExecutePreUpdateScript(connection);

            TaskHelper.ExucuteDeleteFailDate(connection);
            
            var lastRevision = _context.UpdateRevisions.OrderByDescending(o=> new { o.Date, o.Revision}).First().Revision;
            var firstRevision = _context.UpdateRevisions.Min(m => m.Revision);
            var revisions = _context.UpdateRevisions.Select(m => m.Revision).ToList();
            //Проверка подходит ли этот пакет для базы
            CheckDatabase(lastRevision);

            if (string.IsNullOrEmpty(TargetPath))
            {
                targetPath = GetTargetPath(lastRevision.ToString());
                FromDbToTempPath(targetPath, _context.UpdateRevisions.First(f => f.Revision == lastRevision).File);
            }
            else
            {
                targetPath = TargetPath;
            }
            
            Log(Level.Verbose, "Выключаем триггер EntityLogic для сущности Entity...");
            TaskHelper.DisableEntityLogicTrigger(connection);

            //Обновление сборок
            UpdateAssemblies();
            
            TaskHelper.DisableEntityLogicTrigger(connection);

            var directories = _updateDir.GetDirectories();
            var finalStatePath = directories.OrderByDescending(m => m.Name).First();

            //Проходимся по всем ревизиям что есть в поставке
            foreach (var revisionDir in directories)
            {
                _updateTime = DateTime.Now;
                var revisionNumber = Convert.ToInt32(revisionDir.Name);
                if (revisions.Contains(revisionNumber) || firstRevision > revisionNumber)
                    continue;
                Log(Level.Info, "Применяется обновление: {0}", revisionNumber);
                
                try
                {
                    UpdateDb(revisionDir.FullName, targetPath, finalStatePath.FullName);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

                //Выполняем скрипты если они есть
                ExecuteScripts(revisionDir.FullName);
                Log(Level.Info, "Скрипты успешно применены");
                
                //Копируем xml(которые только что накатили на базу) во временную папку, они становятся targetPath
                targetPath = GetTargetPath(revisionDir.Name);
                CopyRevisionToTargetPath(targetPath, revisionDir);
                
                //Записываем в контекст информацию об обновлении
                AddToContext(revisionDir.FullName, revisionNumber);

                Log(Level.Info, "Обновление {0} завершено успешно.", revisionNumber);
            }

            //Выполнение скрипта
            Log(Level.Info, "Выполнение PostUpdate скрипта...");
            TaskHelper.ExecutePostUpdateScript(connection);

            Log(Level.Verbose, "Включаем триггер EntityLogic для сущности Entity...");
            TaskHelper.EnableEntityLogicTrigger(connection);
            connection.Close();

            Log(Level.Verbose, "Удаляем временные файлы...");
            DeleteTempPaths();

        }

        private void DeleteTempPaths()
        {
            foreach (var tempPath in _tempPaths)
            {
                SolutionHelper.DeleteDirectory(tempPath);
            }
        }

        private void GetUpdateDir(DirectoryInfo sourceDir)
        {
            if (sourceDir.GetDirectories("Update*").Count() != 1)
                Fatal("Папка с пакетом обновления отсутствует или их больше одной! Ожидалась тут: " + Path.Combine(SourcePath, "Update"));
            _updateDir = sourceDir.GetDirectories("Update*").SingleOrDefault();
        }

        private void AddToContext(string path, int revisionNumber)
        {
            var bytes = TaskHelper.GetZipFileByte(path);
            Version ver = GetVersion();
            _context.UpdateRevisions.Add(new UpdateRevision
                {
                    Date = _updateTime,
                    Revision = revisionNumber,
                    File = bytes,
                    MajorVersionNumber = 3,
                    MinorVersionNumber = ver.Minor,
                    BuildNumber = ver.Build
                });
            _context.SaveChanges();
        }

        private Version GetVersion()
        {
            Assembly assembly = Assembly.LoadFrom(Path.Combine(SourcePath, @"bin\", "Platform.Web.dll"));
            return assembly.GetName().Version;
        }

        private void FromDbToTempPath(string targetPath, byte[] file)
        {
            Stream stream = new MemoryStream(file);
            using (ZipFile zip1 = ZipFile.Read(stream))
            {
                foreach (ZipEntry e in zip1)
                {
                    e.Extract(targetPath, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        private void CheckDatabase(int lastRevision)
        {
            var strings = _updateDir.Name.Split('.');
            var minRevision = Convert.ToInt32(strings[1].Split('-')[0]);
            if (lastRevision < minRevision)
                Fatal("Ошибка! Данный пакет обновлений не подходит для этой базы, необходим более ранний пакет обновлений");
        }

        private void UpdateAssemblies()
        {
            var deployAssembly = new AssemblyTask
            {
                ConnectionString = ConnectionString,
                DevId = DevId,
                SourcePath = SourcePath,
                Task = (int)TasksEnum.Update,
                Action = (int)ActionEnum.Update
            };
            CopyTo(deployAssembly);
            deployAssembly.Execute();
        }

        /// <summary>
        /// Вызов Task обновления
        /// </summary>
        private void UpdateDb(string sourcePath, string targetPath, string finalStatePath)
        {
            var updateDb = new UpdateDb
            {
                ConnectionString = ConnectionString,
                DevId = DevId,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                FinalStatePath = finalStatePath
            };
            CopyTo(updateDb);
            updateDb.Execute();
        }

        private void CopyRevisionToTargetPath(string targetPath, DirectoryInfo revisionDir)
        {
            var copiedDir = revisionDir.FullName;

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(copiedDir, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(copiedDir, targetPath));

            //Copy all the files
            foreach (string newPath in Directory.GetFiles(copiedDir, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(copiedDir, targetPath), true);
        }

        private string GetTargetPath(string name)
        {
            var tempPath = Path.GetTempPath();
            var revisionTempPath = Path.Combine(tempPath,Guid.NewGuid().ToString(), name);
            _tempPaths.Add(revisionTempPath);//запоминаем, чтобы в конце удалить их
            if (Directory.Exists(revisionTempPath))
                SolutionHelper.DeleteDirectory(revisionTempPath);
            Directory.CreateDirectory(revisionTempPath);
            return revisionTempPath;
        }

        private void ExecuteScripts(string revisionPath)
        {
            var scriptDir = new DirectoryInfo(Path.Combine(revisionPath, "Scripts"));
            foreach (var file in scriptDir.GetFiles())
            {
                Log(Level.Info, "Применяется скрипт № {0}", file.Name);
                var textCommand = file.OpenText().ReadToEnd();
                TaskHelper.ExecuteSQlCommand(ConnectionString, textCommand);
            }
        }
    }
}
