using BaseApp.Environment.Storages;
using Platform.Environment.Interfaces;

namespace BaseApp.Environment.Interfaces
{
    //todo: нигде не используется. можно попробовать выпилить
    /// <summary>
    /// Системное окружение
    /// </summary>
    public interface IEnvironment : IEnvironment<ApplicationStorage,SessionStorage,RequestStorage>
	{
	}
}
