using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using Platform.Common;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.FormsLogic
{
	public abstract class FormBuilderBase
	{
	    protected DataContext db;
        protected Form form;
        protected IEnumerable<FormElement> SrcElements { get; set; }

	    private Entity _entity;
	    protected Entity entity
	    {
	        get
	        {
	            if (_entity == null)
                    _entity = db.Entity.Single(e => e.Id == form.IdEntity);

	            return _entity;
	        }
	    }

        public FormBuilderBase(int formId)
        {
            db = IoC.Resolve<DbContext>().Cast<DataContext>();
            this.form = db.Form.Single(f => f.Id == formId);
            this.SrcElements = db.FormElement.Where(fe => fe.IdOwner == formId);
        }

	    protected abstract IEnumerable<FormItem> getItems(int? parent);

        protected FormItem getItem(FormElement element)
        {
            var item = new FormItem();

            if (element.EntityField != null)
            {
                item.EntityFieldName = element.EntityField.Name;
            }

            if (element.Control != null)
            {
                item.ControlName = element.Control.ComponentName;
                item.ControlAlias = element.Control.Alias;
                item.DefaultProperties = element.Control.DefaultProperties;
                item.LabelProperty = element.Control.LabelProperty;
            }

            item.Properties = element.Properties;
            item.Label = element.Caption;
            item.Items = getItems(element.Id).ToList();

            return item;
        }
    }

}
