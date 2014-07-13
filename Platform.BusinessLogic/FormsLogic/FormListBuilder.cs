using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.SummaryAggregates;
using Platform.PrimaryEntities.Reference;
using System.Data.Entity;

namespace Platform.BusinessLogic.FormsLogic
{
    public class FormListBuilder : FormBuilderBase
    {
        private List<FormItem> listForm;

        public FormListBuilder(int formId): base(formId)
        {
            listForm = new List<FormItem>();
        }

        /// <summary>
        /// Построить конфигурацию формы списка
        /// </summary>
        /// <returns></returns>
        public FormConfig BuildListForm()
        {
            var items = getItems(null).ToList();

            return new FormConfig()
                {
                    formItems = listForm,
                    HierarchyViewField = form.IdHierarchyViewField.HasValue ? db.EntityField.Single(f => f.Id == form.IdHierarchyViewField.Value).Name : null,
                    HasAggregates = form.IdEntity.HasValue ? AggregatesAnalyzer.Any(form.IdEntity.Value, items) : false
                };
        }

        protected override IEnumerable<FormItem> getItems(int? parent)
        {
            IOrderedEnumerable<FormElement> items = this.SrcElements
                .Where(e => (e.IdParent == parent || (!e.IdParent.HasValue && !parent.HasValue))) 
                .ToList()
                .Where(e => !e.IdEntityField.HasValue || !e.EntityField.IsTableField)
                .OrderBy(e => e.Order);

            listForm.AddRange(items.Where(e => e.IdEntityField.HasValue).Select(e => getItem(e)));
            
            return items.Select(e => getItem(e));
        }
    }
}
