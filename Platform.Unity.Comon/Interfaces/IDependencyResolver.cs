using System;
using System.Collections.Generic;

namespace Platform.Unity.Common.Interfaces
{
	public interface IDependencyResolver:IDisposable
	{
		T Resolve<T>();
	    T Resolve<T>(string name);
		IEnumerable<T> ResolveAll<T>();

	}
}
