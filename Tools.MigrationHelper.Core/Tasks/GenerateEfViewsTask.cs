using NAnt.Core;
using NAnt.Core.Attributes;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("generateview")]
    public class GenerateEfViewsTask : MhTask
    {
        [TaskAttribute("sourcepath", Required = true)]
        public string SourcePath
        {
            get;
            set;
        }

        [TaskAttribute("connectionstring", Required = true)]
        public string ConnectionString
        {
            get;
            set;
        }

        protected override void ExecuteTask()
        {
            Log(Level.Verbose, "Генерация вьюх.");
            var generator = new Generators.PreGenerateEfViews.Generator(SourcePath, ConnectionString);
            generator.Generate();
        }
    }
}
