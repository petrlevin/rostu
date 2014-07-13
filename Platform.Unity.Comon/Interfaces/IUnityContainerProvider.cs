
using Microsoft.Practices.Unity;

namespace Platform.Unity.Common.Interfaces
{
	/// <summary>
	/// Поставщик контейнера Unity.
	/// Класс, реализующий данный интерфейс, предоставляет доступ к контейнеру.
	/// </summary>
	public interface IUnityContainerProvider
	{
		IUnityContainer Container { get; }
	}
}
