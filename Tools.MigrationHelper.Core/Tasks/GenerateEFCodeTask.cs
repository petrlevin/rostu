using NAnt.Core;
using NAnt.Core.Attributes;
using Tools.MigrationHelper.Core.DbManager;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("generatecode")]
    public class GenerateEfCodeTask : DbDeployTask
    {
        protected override void ExecuteTask()
        {
            var metadata = new DbDataSet(DevId, ConnectionString);
            Log(Level.Verbose, "Получение метаданных из БД");
            metadata.FillDependencies();
            metadata.FromDb(ConnectionString);
            var generator = new Generators.EntityClasses.Generator(SourcePath, metadata.DataSet);
            Log(Level.Verbose, "Генерация кода.");
            generator.Generate();
        }
    }
}
