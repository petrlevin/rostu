using System.Collections.Generic;
using Platform.Dal.Common.Interfaces.QueryParts;

namespace Platform.Dal.Common.Interfaces.QueryBuilders
{
	public interface ISelectQueryBuilder : IDeleteQueryBuilder
	{
		/// <summary>
		/// Алиас для тела запроса
		/// </summary>
		string AliasName { get; set; }

		/// <summary>
		/// Список полей для отбора. Данные поля будут включены в выборку.
		/// Если список пуст, то будут отобраны все поля.
		/// </summary>
		/// <remarks>
		/// - List&lt;string&gt; вместо List&lt;EntityField&gt; используется для возможности (де)сериализации.
		/// - Точка данных тоже имеет поля. Некоторые поля могут быть вычисляемыми и здесь перечисляются только те, которые следует включить в выборку.
		/// </remarks>
		List<string> Fields { get; set; }

		/// <summary>
		/// Пэйджинг - номер первой записи в выборке и количество записей.
		/// </summary>
		IPaging Paging { get; set; }

		/// <summary>
		/// Информация о сортировке
		/// </summary>
		IOrder Order { get; set; }

		/// <summary>
		/// Строка поиска в гриде
		/// </summary>
		string Search { get; set; }
	}
}
