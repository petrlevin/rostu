using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.EditionsComparision
{
    public class TablepartInfo : ITableInfo
    {
        public TablepartInfo()
        {
        }
        
        public TablepartInfo(IEntityField tablefield)
        {
            Field = tablefield;
            process();
        }

        public IEntityField Field { get; set; }

        #region Информация о табличном поле

        private IEnumerable<IEntityField> _tablepartEntityFields;
        /// <summary>
        /// Поля сущности табличной части.
        /// ЗАЧЕМ: чтобы иметь возможность тестировать и задать поля напрямую без указания сущности. 
        /// При задании через сущность при тестировании - возникают проблемы со стратегиями получения полей
        /// </summary>
        public IEnumerable<IEntityField> Fields
        {
            get
            {
                IEnumerable<IEntityField> fields;
                if (_tablepartEntityFields != null)
                    fields = _tablepartEntityFields;
                else
                    fields = TableEntity.RealFields;
                return fields.ToList().Where(ef => !ignoredFields.Contains(ef.Name));
            }
            set { _tablepartEntityFields = value; }
        }

        /// <summary>
        /// Сущность табличной части
        /// </summary>
        public Entity TableEntity { get; set; }

        /// <summary>
        /// Имя поля табличной части, ссылающейся на родительскую сущность. Как правило idOwner
        /// </summary>
        public string OwnerFieldName { get; set; }

        public bool HasMasterField
        {
            get { return !string.IsNullOrEmpty(MasterFieldName); }
        }

        /// <summary>
        /// Имя поля, ссылающегося родительскую табличную часть. Как правило idMaster. 
        /// Если не содержит значения, следовательно данная ТЧ не является подчиненной.
        /// </summary>
        public string MasterFieldName { get; set; }

        /// <summary>
        /// Имя родительской ТЧ (имя ТЧ как поля, не как сущности)
        /// </summary>
        /// <remarks>
        /// Определяется на основе фильтра. 
        /// ЗАЧЕМ: для построения дерева зависимостей между ТЧ.
        /// </remarks>
        public string MasterTablepartfieldName { get; set; }

        public bool HasCaptionField
        {
            get { return !string.IsNullOrEmpty(CaptionFieldName); }
        }
        
        public string CaptionFieldName { get; set; }

        #endregion

        private List<string> ignoredFields;

        private const string IdName = "id";

        private DataContext db { get; set; }

        private List<EntityFieldType> allowedFieldTypes = new List<EntityFieldType>
            {
                EntityFieldType.Tablepart,
                EntityFieldType.VirtualTablePart
            };

        private void process()
        {
            db = IoC.Resolve<DbContext>().Cast<DataContext>();
            check();
            getInfo();
            setIgnoredFields();
        }

        private void check()
        {
            if (!allowedFieldTypes.Contains(Field.EntityFieldType))
                throw new PlatformException("Передано поле некорректного типа.");
        }

        #region getInfo

        private void getInfo()
        {
            OwnerFieldName = db.EntityField.Single(ef => ef.Id == Field.IdOwnerField).Name;
            TableEntity = db.Entity.Single(e => e.Id == Field.IdEntityLink);

            processCaptionField();
            processFilters();
        }

        private void processCaptionField()
        {
            IEntityField captionField = TableEntity.Fields.SingleOrDefault(f => f.IsCaption.HasValue && f.IsCaption.Value);
            if (captionField != null)
                CaptionFieldName = captionField.Name;
        }

        private void processFilters()
        {
            // Ищем фильтр, наложенный на данное табличное поле. Связь между ТЧ обеспечивается единственным фильтром (если фильтров больше - это уже не связь)
            Filter filter = db.Filter.SingleOrDefault(f =>
                f.IdEntityField == Field.Id
                && !f.Disabled
                && f.IdLogicOperator == (byte)LogicOperator.Simple
                && f.IdComparisionOperator == (byte)ComparisionOperator.Equal
                && f.IdRightEntityField.HasValue
                );

            if (filter != null)
            {
                // правое поле - табличная часть, с которой связана данная.
                EntityField rightEntityField = db.EntityField.SingleOrDefault(ef => ef.Id == filter.IdRightEntityField);
                if (rightEntityField.IdEntityFieldType == (byte)EntityFieldType.Tablepart
                    || rightEntityField.IdEntityFieldType == (byte)EntityFieldType.VirtualTablePart)
                {
                    MasterFieldName = db.EntityField.Single(ef => ef.Id == filter.IdLeftEntityField).Name;
                    MasterTablepartfieldName = rightEntityField.Name;
                }
            }
        }

        public void setIgnoredFields()
        {
            ignoredFields = new List<string>()
                {
                    IdName,
                    OwnerFieldName
                };
            if (HasMasterField)
                ignoredFields.Add(MasterFieldName);
        }

        #endregion
    }
}
