using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.EditionsComparision
{
    public class TableInfo: ITableInfo
    {
        public TableInfo(int entityId)
        {
            var db = IoC.Resolve<DbContext>().Cast<DataContext>();
            this.TableEntity = db.Entity.Single(e => e.Id == entityId);
            process();
        }

        public IEnumerable<IEntityField> Fields
        {
            get { return TableEntity.RealFields; }
        }
        public bool HasCaptionField { get; private set; }
        public string CaptionFieldName { get; set; }

        public Entity TableEntity { get; set; }

        private void process()
        {
            HasCaptionField = Fields.Any(ef => ef.IsCaption.HasValue && ef.IsCaption.Value);
            if (HasCaptionField)
                CaptionFieldName = Fields.First(ef => ef.IsCaption.Value).Name;
        }

    }
}
