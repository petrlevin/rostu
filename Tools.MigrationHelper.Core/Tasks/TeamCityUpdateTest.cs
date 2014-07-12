using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Smo;
using NAnt.Core;
using NAnt.Core.Attributes;
using SevenZSharp;
using Tools.MigrationHelper.Core.Context;
using Tools.MigrationHelper.Core.Helpers;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
    /// <summary>
    /// Тестирование выпущенных поставок на предмет обновления последний версии БД клиента
    /// </summary>
    [TaskName("TeamCityUpdateTest")]
    public class TeamCityUpdateTest : DbDeployTask
    {
        /// <summary>
        /// Директория откуда берется бэкап
        /// </summary>
        [TaskAttribute("BackupDir", Required = false)]
        public string BackupDir { get; set; }

        /// <summary>
        /// Директория где ищются поставки(которые тестируются)
        /// </summary>
        [TaskAttribute("DistrDir", Required = false)]
        public string DistrDir { get; set; }

        /// <summary>
        /// Путь до файла бэкапа
        /// </summary>
        private string BackUpFilePath { get; set; }

        private Server Server { get; set; }

        private SqlConnection SqlConnection { get; set; }

        private int Revision { get; set; }

        private const string PathForFreshBackUp = "C:\\FreshBackUp";

        private readonly string _tempDir;

        public TeamCityUpdateTest()
        {
            _tempDir = Path.Combine(Path.GetTempPath(),@"UpdTest");
        }

        protected override void ExecuteTask()
        {
            Init();

            Log(Level.Verbose, "Копируем бэкап...");
            GetBackUpFile();
            Log(Level.Verbose, "Восстанавливаем БД...");
            RestoreDB();
            GetDataBaseInfo();
            Log(Level.Verbose, "Получаем директории...");
            GetDistrPackagesToLocal();

            foreach (var dir in Directory.GetDirectories(_tempDir))
            {
                try
                {
                    Log(Level.Verbose, "Выполняем обновление...");
                    Update(dir);
                    Log(Level.Verbose, string.Format("Обновление на {0} выполнено успешно", dir));
                    RenameDistrDir(dir,true);
                }
                catch (Exception )
                {
                    Log(Level.Verbose, string.Format("Обновление на {0} провалилось", dir));
                    RenameDistrDir(dir,false);
                }
                finally
                {

                    Log(Level.Verbose, "Восстанавливаем БД...");
                    RestoreDB();
                }
            }
            Log(Level.Verbose, "Удаляем все");
            CleanUp();
        }

        /// <summary>
        /// Переименовывание папки в соответсвии с успешностью обновления
        /// </summary>
        /// <param name="localDir">Локальная папка(на основе ее имени ищется папка на сервере)</param>
        /// <param name="succesfull">Успех или провал обновления</param>
        private void RenameDistrDir(string localDir, bool succesfull)
        {
            var postfix = succesfull ? "work" : "bad";
            var localDirectory = new DirectoryInfo(localDir);
            var fileForRename = new DirectoryInfo(DistrDir).GetFiles(string.Format("{0}*", localDirectory.Name), SearchOption.AllDirectories);
            if (fileForRename.Count() == 1)
            {
                File.Move(fileForRename.First().FullName, fileForRename.First().FullName.Replace("raw", postfix));
            }
        }

        /// <summary>
        /// Инизиализация
        /// </summary>
        /// <remarks>
        /// Nant задает значения свойствам после вызова конструктора, поэтому это тут
        /// </remarks>
        private void Init()
        {
            if (string.IsNullOrEmpty(BackupDir))
                BackupDir = @"\\fs\Разработка\_tmp";
            if(string.IsNullOrEmpty(DistrDir))
                DistrDir = @"\\fs\TeamCity_builds\BIS3";

            SqlConnection = new SqlConnection(ConnectionString);
            var str = new SqlConnectionStringBuilder(ConnectionString) {InitialCatalog = ""};
            var sqlConnection = new SqlConnection(str.ToString());
            Server = TaskHelper.GetServer(sqlConnection);
        }

        /// <summary>
        /// Получение по условию и распаковка поставок на локальную машину
        /// </summary>
        private void GetDistrPackagesToLocal()
        {
            foreach (var dir in new DirectoryInfo(DistrDir).GetDirectories())
            {
                var distrFiles = dir.GetFiles("*_raw*", SearchOption.AllDirectories);
                foreach (var distrFile in distrFiles)
                {
                    var matches = Regex.Match(distrFile.Name, @".(?<key>[\d]+)_raw");
                    var revision = matches.Groups["key"].Value;
                    if (int.Parse(revision) > Revision)
                    {
                        if(distrFile.Name.Contains("trunk"))
                        CopyAndDecode(distrFile, _tempDir);
                    }
                }
            }
        }

        /// <summary>
        /// Получение информации о БД
        /// </summary>
        private void GetDataBaseInfo()
        {
            var context = new UpdateRevisionContext(ConnectionString);
            Revision = context.UpdateRevisions.Max(m => m.Revision);
        }

        /// <summary>
        /// Удаляем все что создали
        /// </summary>
        private void CleanUp()
        {
            foreach (var dir in Directory.GetDirectories(_tempDir))
            {
                SolutionHelper.DeleteDirectory(dir);
            }
            DropDb(SqlConnection.Database);
        }

        /// <summary>
        /// Выполение таски Update MigrationHelper
        /// </summary>
        /// <param name="dir"></param>
        private void Update(string dir)
        {
            var update = new UpdateTask
            {
                DevId = 0,
                SourcePath = dir,
                ConnectionString = ConnectionString
            };
            CopyTo(update);
            update.Execute();
        }

        /// <summary>
        /// Востановление БД из бэкапа
        /// </summary>
        private void RestoreDB()
        {
            string databaseName = SqlConnection.Database;
            try
            {
                DropDb(databaseName);

                var res = new Restore();
                res.Devices.AddDevice(BackUpFilePath, DeviceType.File);


                string mdf = res.ReadFileList(Server).Rows[0][1].ToString();
                var file = new FileInfo(mdf);
                var fileLocation = Path.Combine(Server.MasterDBPath, file.Name);

                var dataFile = new RelocateFile
                    {
                        LogicalFileName = res.ReadFileList(Server).Rows[0][0].ToString(),
                        PhysicalFileName =
                            Server.Databases[databaseName] != null
                                ? Server.Databases[databaseName].FileGroups[0].Files[0].FileName
                                : fileLocation
                    };

                string ldf = res.ReadFileList(Server).Rows[1][1].ToString();
                file = new FileInfo(ldf);
                fileLocation = Path.Combine(Server.MasterDBPath, file.Name);
                var logFile = new RelocateFile
                    {
                        LogicalFileName = res.ReadFileList(Server).Rows[1][0].ToString(),
                        PhysicalFileName =
                            Server.Databases[databaseName] != null
                                ? Server.Databases[databaseName].LogFiles[0].FileName
                                : fileLocation
                    };

                res.RelocateFiles.Add(dataFile);
                res.RelocateFiles.Add(logFile);

                res.Database = databaseName;
                res.NoRecovery = false;
                res.ReplaceDatabase = true;
                res.SqlRestore(Server);
            }
            catch (SmoException ex)
            {
                throw new SmoException(ex.Message, ex.InnerException);
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Дроп базы данных
        /// </summary>
        /// <param name="databaseName">Имя базы данных</param>
        private void DropDb(string databaseName)
        {
            var db = Server.Databases[databaseName];
            if (db != null)
                db.Drop();
        }

        /// <summary>
        /// Копирование файла и его распаковка
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destinationDirPath"></param>
        private void CopyAndDecode(FileInfo file, string destinationDirPath)
        {
            Directory.CreateDirectory(destinationDirPath);
            var copyFile = file.CopyTo(Path.Combine(destinationDirPath, file.Name));
            CompressionEngine.Current.Decoder.DecodeIntoDirectory(copyFile.FullName, destinationDirPath);
            copyFile.Delete();
        }

        /// <summary>
        /// Копирование и распоковка файла бэкапа
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string CopyAndDecodeBackUp(FileInfo file)
        {
            if (file.Extension == "bak")
            {
                file.CopyTo(Path.Combine(PathForFreshBackUp, file.Name));
            }
            else
            {
                CopyAndDecode(file, PathForFreshBackUp);
            }
            var backup = new DirectoryInfo(PathForFreshBackUp).GetFiles("*.bak", SearchOption.AllDirectories).SingleOrDefault();
            if (backup == null)
                throw new Exception("Не найден файл бэкапа в папке " + PathForFreshBackUp + "!");
            return backup.FullName;
        }

        /// <summary>
        /// Копирование и распаковка свежего бэкапа в локальную папку
        /// </summary>
        /// <remarks>
        /// Скопированный файл бэкапа хранится в папке, если на сервере нет более нового бэкапа берется тот который есть
        /// </remarks>
        private void GetBackUpFile()
        {
            var directory = new DirectoryInfo(BackupDir);
            //последний файл бэкапа который был скопирован
            var localDir = new DirectoryInfo(PathForFreshBackUp);
            if (!localDir.Exists)
                Directory.CreateDirectory(PathForFreshBackUp);
            var localFile = localDir.GetFiles("*.bak").SingleOrDefault();
            var newFile = (directory.GetFiles()).ToList().OrderByDescending(o => o.CreationTime).First();
            if (localFile != null)
            {
                var localName = Path.GetFileNameWithoutExtension(localFile.FullName);
                var newFileName = Path.GetFileNameWithoutExtension(newFile.FullName);

                if (localName == newFileName && localFile.CreationTime == newFile.CreationTime)
                {
                    BackUpFilePath = localFile.FullName;
                }
                else
                {
                    localFile.Delete();
                    BackUpFilePath = CopyAndDecodeBackUp(newFile);
                }
            }
            else
            {
                BackUpFilePath = CopyAndDecodeBackUp(newFile);
            }
        }
    }
}
