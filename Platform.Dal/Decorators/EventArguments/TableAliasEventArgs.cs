using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Dal.Decorators.Abstract;

namespace Platform.Dal.Decorators.EventArguments
{
	public class TableAliasEventArgs : EventData
	{
		/// <summary>
		/// Алиас таблицы, в которой располагаются поля
		/// </summary>
		public string TableAlias { get; set; }
	}
}
