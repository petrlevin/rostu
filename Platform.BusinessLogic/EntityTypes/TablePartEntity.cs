using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.BusinessLogic.EntityTypes.Interfaces;

namespace Platform.BusinessLogic.EntityTypes
{
	/// <summary>
	/// Базовый класс для табличных частей
	/// </summary>
	public abstract class TablePartEntity : ReferenceEntity, ITablePartEntity
	{
	}
}
