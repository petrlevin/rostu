using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.FormsLogic;
using Platform.BusinessLogic.SummaryAggregates;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace Platform.Web.Services
{
    /// <summary>
    /// Сервис получения форм элементов
    /// </summary>
    public class FormService
    {
        #region private
        private DataContext _db;
        private DataContext Db
        {
            get
            {
                if (_db == null)
                {
                    _db = IoC.Resolve<DbContext>().Cast<DataContext>();
                }
                return _db;
            }
        }

        private FormConfig _formConfig;

        private void CreateListForm(int entityId)
        {
            var entitySettings = Db.EntitySetting.SingleOrDefault(f => f.IdEntity == entityId && f.IdListForm.HasValue);
            if (entitySettings == null)
            {
                _formConfig.HasAggregates = AggregatesAnalyzer.Any(entityId);
                return;
            }

            _formConfig = new FormListBuilder(entitySettings.IdListForm.Value).BuildListForm();
            _formConfig.formId = entitySettings.IdListForm.Value;
        }

        private void CreateSelectionForm(int entityId)
        {
            var entitySettings = Db.EntitySetting.SingleOrDefault(f => f.IdEntity == entityId && f.IdSelectionForm.HasValue);
            if (entitySettings == null)
                return;

            _formConfig = new FormListBuilder(entitySettings.IdSelectionForm.Value).BuildListForm();
            _formConfig.formId = entitySettings.IdSelectionForm.Value;
        }

        private void CreateItemForm(int entityId)
        {
            Entity entity = Db.Entity.Single(e => e.Id == entityId);
            var entitySettings = Db.EntitySetting.SingleOrDefault(f => f.IdEntity == entityId && f.IdItemForm.HasValue);

            Dictionary<string, bool> tableFieldAggregates = GetTablefieldsAggregates(entityId);

            if (entitySettings == null /*&& entity.EntityType != EntityType.Report*/)
            {
                _formConfig.TableFieldAggregates = tableFieldAggregates;
                return;
            }

            _formConfig = new FormItemBuilder(entitySettings.IdItemForm.Value).BuildItemForm();
            _formConfig.TableFieldAggregates = tableFieldAggregates;
            _formConfig.formId = entitySettings.IdItemForm.Value;
        }

        private Dictionary<string, bool> GetTablefieldsAggregates(int entityId)
        {
            var result = new Dictionary<string, bool>();

            List<EntityField> tableFields = Db.EntityField.Where(ef => ef.IdEntity == entityId && (
                ef.IdEntityFieldType == (byte)EntityFieldType.Tablepart
                || ef.IdEntityFieldType == (byte)EntityFieldType.VirtualTablePart)).ToList();
            foreach (EntityField tableField in tableFields)
            {
                result[tableField.Name] = AggregatesAnalyzer.Any((int)tableField.IdEntityLink);
            }
            return result;
        }
        #endregion
        
        /// <summary>
        /// Получить форму
        /// </summary>
        /// <param name="entityId">Идентификатор сущности</param>
        /// <param name="context">Тип формы</param>
        /// <returns></returns>
        public FormConfig GetForm(int entityId, FormType context)
        {
            _formConfig = new FormConfig();

            switch (context)
            {
                case FormType.Item:
                    CreateItemForm(entityId);
                    break;
                case FormType.List:
                    CreateListForm(entityId);
                    break;
                case FormType.Selection:
                    CreateSelectionForm(entityId);
                    break;
                default:
                    throw new PlatformException("Передан неизвестный тип формы.");
            }

            _formConfig.entityId = entityId;
            _formConfig.formType = context;
            return _formConfig;
        }

        /// <summary>
        /// Получить форму, используя системное наименование формы
        /// </summary>
        /// <param name="name">Системное имя формы</param>
        /// <param name="context">Тип формы</param>
        /// <returns></returns>
        public FormConfig GetFormByName(string name, FormType context)
        {
            Form form = Db.Form.SingleOrDefault(f => f.Name == name);
            if (form == null)
                throw new PlatformException(string.Format("Не найдена форма с именем {0} (или их более одной)", name));

            switch (context)
            {
                case FormType.Item:
                    _formConfig = new FormItemBuilder(form.Id).BuildItemForm();
                    break;
                case FormType.List:
                    _formConfig = new FormListBuilder(form.Id).BuildListForm();
                    break;
                case FormType.Selection:
                    _formConfig = new FormListBuilder(form.Id).BuildListForm();
                    break;
                default:
                    throw new PlatformException("Передан неизвестный тип формы.");
            }

            _formConfig.formId = form.Id;
            _formConfig.formType = context;
            return _formConfig;
        }

      
    }
}