using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Denormalizer
{
    /// <summary>
    /// Информация о значимых колонках, выводимых в ДТЧ
    /// </summary>
    public class ColumnsInfo
    {
        /// <summary>
        /// Периоды, которые должны быть доступны для данного экземпляра сущности
        /// </summary>
        public IEnumerable<PeriodIdCaption> Periods { get; set; }

        /// <summary>
        /// Перечень имен ресурсных полей дочерней сущности ДТЧ, которые должны быть доступны.
        /// null означает все поля, пустой список - ни одного поля.
        /// </summary>
        public IEnumerable<string> Resources { get; set; }
    }
}
