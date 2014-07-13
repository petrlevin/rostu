using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Unity.Common.Interfaces;

namespace Platform.Common
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// http://stackoverflow.com/questions/277438/abstracting-ioc-container-behind-a-singleton-doing-it-wrong
	/// </remarks>
	public static class IoC
	{
		private static IDependencyResolver inner;

		public static void InitWith(IDependencyResolver container)
		{
			inner = container;
		}

		/// <exception cref="InvalidOperationException">Container has not been initialized. Please supply an instance if IUnityContainer.</exception>
		public static T Resolve<T>()
		{
			if (inner == null)
				throw new InvalidOperationException("Container has not been initialized. Please supply an instance if IUnityContainer.");

			return inner.Resolve<T>();
		}

        public static T Resolve<T>(string name)
        {
            if (inner == null)
                throw new InvalidOperationException("Container has not been initialized. Please supply an instance if IUnityContainer.");

            return inner.Resolve<T>(name);
        }


		public static IEnumerable<T> ResolveAll<T>()
		{
			return inner.ResolveAll<T>();
		}


        public static void Dispose()
        {
            if (inner!=null)
                inner.Dispose();
        }
	}
}
