using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Utils.Extensions
{
	public static class ListExtensions
	{
		public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
		{
			return listToClone.Select(item => (T)item.Clone()).ToList();
		}

		public static void AddUnique<T>(this List<T> list, T item) where T : class
		{
			if (list.All(a => a != item))
			{
				list.Add(item);
			}
		}
	}
}
