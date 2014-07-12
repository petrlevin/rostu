using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.Context;
using Tools.MigrationHelper.Core.DbManager;
using Tools.MigrationHelper.Core.Helpers;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("deployappdb")]
    public class DeployAppDb : DbDeployTask
    {

       protected override void ExecuteTask()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                try
                {
                    string sourcePath = SourcePath;
					Log(Level.Verbose, "Выключаем триггер EntityLogic для сущности Entity...");
					TaskHelper.DisableEntityLogicTrigger(connection);
					TaskHelper.DisableIndex(connection);
                    var bytes = new byte[] {};
                    if (IsDeveloper())
					{
						Log(Level.Verbose, "Создаем таблицу для хранения ревизий xml файлов...");
						TaskHelper.CreateDevDbRevisionTable(connection);
					}
					else
					{
                        //Получаем папку с xml файлами на послденюю ревизию которая есть в папке Update*
                        var updateDir =
                            new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.GetDirectories("Update*")
                                                                              .SingleOrDefault();
                        var dirs = updateDir.GetDirectories();
                        var needPath = dirs.Max(m => int.Parse(m.Name)).ToString();
                        var dir = dirs.FirstOrDefault(f => f.Name == needPath);
                        sourcePath = dir.FullName;
                        bytes = TaskHelper.GetZipFileByte(sourcePath);
					}

                    //Создаем таблицу с информацией об апдейтах
                    TaskHelper.CreateUpdateRevisionTable(connection);

                    //Записываем информацию о версии в базу
                    AddInfoToUpdateRevision(bytes);

                    var source = new DbDataSet(DevId, ConnectionString, TasksEnum.DeployAppDb);
					var targetDb = new DbDataSet(DevId);

                    targetDb.FromDb(connection);

                    Log(Level.Verbose, "Считываем метаданные из файловой системы...");
                    source.FromFs(sourcePath);
                    var cmpRes = new MetadataCompareResult(source, targetDb, null, connection);

                    cmpRes.Compare();

                    Log(Level.Verbose, "Применение метаданных к базе ...");
                    cmpRes.Execute();

//                    Log(Level.Verbose, cmpRes.Verbose());
                    SetStartIdentity();
					Log(Level.Verbose, "Включаем триггер EntityLogic для сущности Entity...");
					TaskHelper.EnableEntityLogicTrigger(connection);
					TaskHelper.EnableIndex(connection);
                    if(IsDeveloper())
                        TaskHelper.CreateDistrDataTriggers(connection);
					TaskHelper.ExecutePostUpdateScript(connection);
                }
                catch (BuildException)
                {
                    throw;
                }

                catch (Exception ex)
                {
                    
                    Fatal("Фатальная ошибка при развертывании базы приложения" ,ex);
                }
            }

        }

        private void AddInfoToUpdateRevision(byte[] bytes)
        {
            var context = new UpdateRevisionContext(ConnectionString);
            var ver = GetVersion();
            var newRevision = new UpdateRevision
            {
                Date = DateTime.Now,
                Revision = ver.Revision,
                MajorVersionNumber = 3,
                MinorVersionNumber = ver.Minor,
                BuildNumber = ver.Build,
                File = bytes
            };
            context.UpdateRevisions.Add(newRevision);
            context.SaveChanges();
        }

        private Version GetVersion()
        {
            var path = !IsDeveloper() ? Path.Combine(SourcePath, "bin", "Platform.Web.dll") : Path.Combine(SourcePath,"Platform.Web", "bin", "Platform.Web.dll");
            Assembly assembly = Assembly.LoadFrom(path);
            return assembly.GetName().Version;
        }
    }
}
