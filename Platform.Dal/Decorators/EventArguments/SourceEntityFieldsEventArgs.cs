using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.Dal.Decorators.EventArguments
{
    /// <summary>
    /// Поля сущности, для анализа
    /// </summary>
    public class SourceEntityFieldsEventArgs : EventDataList<IEntityField>
	{
        public SourceEntityFieldsEventArgs(IEnumerable<IEntityField> values): base(values)
        {
        }
	}
}
