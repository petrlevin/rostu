using BaseApp.Environment.Storages;
using Platform.Environment;

namespace BgTasks.Core
{
    public class SimpleEnvironment : EnvironmentBase<ApplicationStorage, SessionStorage, RequestStorage>
    {
    }
}
