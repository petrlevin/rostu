using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.DbEnums;

namespace Platform.BusinessLogic.FormsLogic
{
	public class FormConfig
	{
        public FormConfig()
        {
            TableFieldAggregates = new Dictionary<string, bool>();
        }

        public int entityId { get; set; }
        public int formId { get; set; }
        public FormType formType { get; set; }
        public string HierarchyViewField { get; set; }
		public List<FormItem> formItems;
        public bool HasAggregates { get; set; }
        public List<FormConfig> Tablefields { get; set; }

	    /// <summary>
        /// Перечень табличных полей формы элемента и признак - имеет ли табличное поле итоговую строку.
        /// </summary>
        public Dictionary<string, bool> TableFieldAggregates { get; set; }
	}
}
