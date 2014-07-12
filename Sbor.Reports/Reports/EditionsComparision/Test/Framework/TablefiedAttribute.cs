using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.EditionsComparision.Test.Framework
{
    public class TablefieldAttribute: Attribute
    {
        /// <summary>
        /// Системное имя табличной части как поля
        /// </summary>
        public string FieldName { get; set; }
        public string MasterField { get; set; }
        public string CaptionField { get; set; }
    }
}
