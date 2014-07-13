using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Dal.Common.Interfaces.QueryParts
{
	public interface IPaging
	{
		/// <summary>
		/// Начальный элемент выборки
		/// </summary>
		int Start { get; set; }

		/// <summary>
		/// Количество отбираемых элементов
		/// </summary>
		int Count { get; set; }
	}
}
