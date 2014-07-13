using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal;
using Platform.Dal.Interfaces;

namespace BaseApp.SystemDimensions
{
	public class SysDimensionsDecorator : ITSqlStatementDecorator
	{
		#region Implementation of IQueryDecorator

		public TSqlStatement Decorate(TSqlStatement source)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
