using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Denormalizer.Interfaces
{
	/// <summary>
	/// Индерфейс-метка для дочерних табличных частей, подлежащих денормализации
	/// </summary>
	/// <remarks>
	/// Обращаю вримание на то, что наличие сгенерированного сущностного класса необязательно. 
	/// Достаточно вручную создать класс с именем сущности и пометить его интерсфейсом. В этом случае при работе с ТЧ будет использоваться DAL.
	/// </remarks>
	public interface IChildDenormalized
	{
	}
}
