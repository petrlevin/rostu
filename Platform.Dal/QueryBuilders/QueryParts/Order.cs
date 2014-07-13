using Platform.Dal.Common.Interfaces.QueryParts;
using Platform.Utils.Collections;

namespace Platform.Dal.QueryBuilders.QueryParts
{
	/// <summary>
	/// Порядок сортировки. Ключ : имя поля, значение : true - по возрастанию, false - по убыванию
	/// </summary>
	public class Order : OrderedDictionary<string,bool>, IOrder
	{
	}
}
