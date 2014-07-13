using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.DbEnums;

namespace BaseApp.SystemDimensions
{
	public class SysDimensionsState : Dictionary<SysDimension, Guid>
	{
		public SysDimensionsState() : base()
		{
		}
	}
}
