using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.Dal.Common.Interfaces.QueryParts;

namespace Platform.Dal.QueryBuilders.QueryParts
{
	/// <summary>
	/// Пэйджинг
	/// </summary>
	public class Paging : IPaging
	{
		/// <summary>
		/// Начальный элемент выборки
		/// </summary>
		public int Start { get; set; }

		/// <summary>
		/// Количество отбираемых элементов
		/// </summary>
		public int Count { get; set; }
	}
}
