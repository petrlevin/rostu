using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Common.Interfaces
{
    public interface IOperation :IBaseEntity ,IIdentitied
    {
	
		/// <summary>
		/// Английское наименование метода операции
		/// </summary>
		string Name{get; set;}

		/// <summary>
		/// Название операции
		/// </summary>
		string Caption{get; set;}

		/// <summary>
		/// Описание операции
		/// </summary>
		string Description{get; set;}
    }
}
