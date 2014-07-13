
using Microsoft.Practices.Unity;

namespace Platform.Unity.Common.Interfaces
{
	public interface ISharedStorage
	{
		IUnityContainer Container { get; set; }
		void InitUnityContainer(IUnityContainer container);
	}
}
