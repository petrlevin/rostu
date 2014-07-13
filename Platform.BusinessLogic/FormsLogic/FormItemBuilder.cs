using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Platform.BusinessLogic.SummaryAggregates;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using System.Xml.Serialization;

namespace Platform.BusinessLogic.FormsLogic
{
    public class FormItemBuilder : FormBuilderBase
    {
        public FormItemBuilder(int formId): base(formId)
        {
        }

        /// <summary>
        /// Построить конфигурацию формы элемента
        /// </summary>
        /// <returns></returns>
        public FormConfig BuildItemForm()
        {
            FormConfig result = new FormConfig
                {
                    formItems = getItems(null).ToList()
                };

            //serializeFormItem(result.formItems[0]); см. getReportProfilePanel
            
            if (entity.EntityType == EntityType.Report)
                insertReportProfilePanel(result);

            //CORE-802,803,808,809: отключение кеша форм решает данные проблемы.
            //result.Tablefields = SrcElements
            //    .Where(fe => fe.EntityField != null && fe.EntityField.IsTableField)
            //    .Select(fe => (int)fe.EntityField.IdEntityLink)
            //    .ToList() // данный промежуточный ToList предотвращает возникновение ошибки "There is already an open DataReader associated with this Command which must be closed first"
            //    .Select(entityId => createListForm(entityId))
            //    .ToList();
            return result;
        }

        private void serializeFormItem(FormItem item)
        {
            var serializer = new XmlSerializer(item.GetType());

            var writer = new XmlTextWriter(@"z:/tmp/1/1.xml", Encoding.Unicode);
            serializer.Serialize(writer, item);
        }

        protected override IEnumerable<FormItem> getItems(int? parent)
        {
            return this.SrcElements
                .Where(e => e.IdParent == parent || (!e.IdParent.HasValue && !parent.HasValue))
                .OrderBy(e => e.Order)
                .Select(e => getItem(e));
        }

        /// <summary>
        /// ToDo: копипаст из FormService
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private FormConfig createListForm(int entityId)
        {
            var entitySettings = db.EntitySetting.SingleOrDefault(f => f.IdEntity == entityId && f.IdListForm.HasValue);
            if (entitySettings == null)
            {
                return new FormConfig()
                    {
                        entityId = entityId,
                        formType = FormType.List,
                        HasAggregates = AggregatesAnalyzer.Any(entityId)
                    };
            }

            var formConfig = new FormListBuilder(entitySettings.IdListForm.Value).BuildListForm();
            formConfig.entityId = entityId;
            formConfig.formType = FormType.List;
            formConfig.formId = entitySettings.IdListForm.Value;
            return formConfig;
        }

        private void insertReportProfilePanel(FormConfig result)
        {
            result.formItems.Insert(0, getReportProfilePanel());
        }

        /// <summary>
        /// Верхняя панель профиля отчета. Поля ReportProfileCaption, idReportProfileType, idReportProfileUser
        /// </summary>
        /// <remarks>
        /// Метод написан вручную на основании xml, полученного в результате <see cref="serializeFormItem">сериализации</see> объекта FormItem, 
        /// представляющего собой верхнюю панель профиля отчета. 
        /// Данная панель была добавлена в некоторый отчет (в любой), далее был включен метод сериализации и получен xml.
        /// </remarks>
        /// <returns></returns>
        private FormItem getReportProfilePanel()
        {
            return new FormItem()
                {
                    Label = "Профиль отчета",
                    ControlAlias = "fieldset",
                    LabelProperty = "title",
                    DefaultProperties = "{ collapsible: true }",
                    Items = new List<FormItem>()
                        {
                            new FormItem()
                                {
                                    ControlAlias = "fieldcontainer",
                                    ControlName = "Ext.form.FieldContainer",
                                    LabelProperty = "title",
                                    DefaultProperties = "{ layout: 'column', border: 0 }",
                                    Items = new List<FormItem>()
                                        {
                                            new FormItem()
                                                {
                                                    Properties = "{ columnWidth: 0.6 }",
                                                    LabelProperty = "title",
                                                    DefaultProperties = "{ layout: 'anchor', border: 0, margin: '5 5 5 5' }",
                                                    Items = new List<FormItem>() { new FormItem() { EntityFieldName = "ReportProfileCaption"} }
                                                },
                                            new FormItem()
                                                {
                                                    Properties = "{ columnWidth: 0.2 }",
                                                    LabelProperty = "title",
                                                    DefaultProperties = "{ layout: 'anchor', border: 0, margin: '5 5 5 5' }",
                                                    Items = new List<FormItem>() { new FormItem() { EntityFieldName = "idReportProfileType" } }
                                                },
                                            new FormItem()
                                                {
                                                    Properties = "{ columnWidth: 0.2 }",
                                                    LabelProperty = "title",
                                                    DefaultProperties = "{ layout: 'anchor', border: 0, margin: '5 5 5 5' }",
                                                    Items = new List<FormItem>()
                                                        {
                                                            new FormItem() { EntityFieldName = "idReportProfileUser" },
                                                            new FormItem() { EntityFieldName = "isTemporary" }
                                                        }
                                                }
                                        }
                                }
                        }
                };
        }

    }
}
