using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BaseApp.Environment;
using BaseApp.Rights;
using BaseApp.Rights.Organizational;
using BaseApp.Rights.Organizational.Decorators;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Analyzers;
using Platform.BusinessLogic.Denormalizer.Decorator;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Collections;


namespace BaseApp.Export
{
    /// <summary>
    /// Экспорт списка элементов сущности в excel
    /// </summary>
    public class EntitiesListExport : ExcelTableWriter
    {
        #region Public Properties

        /// <summary>
        /// Сущность, из которой будут экспортиорваны элементы
        /// </summary>
        public Entity Entity { get; set; }

        public int? IdTemplate;

        /// <summary>
        /// Значение поля с именем <see cref="OwnerFieldName"/>
        /// </summary>
        public int? IdOwner;

        /// <summary>
        /// Имя ownerField - поля, ссылающегося на родителя
        /// </summary>
        public string OwnerFieldName;

        /// <summary>
        /// Только эти колонки следует экспортировать
        /// </summary>
        public List<string> VisibleColumns { get; set; }

        #endregion

        /// <summary>
        /// Получение документа exel 
        /// </summary>
        public string BuildReport()
        {
            List<Dictionary<string, object>> result = GetExportingData();

            if (IsHierarchy)
                result = Order(result);

            Title = Entity.Caption;
            return BuildReport(result);
        }

        public EntitiesListExport(string fieldValues, int? fieldId, Entity entity, string searchStr,
                               List<string> visibleColumnsList, int? ownerItemId)
        {
            Entity = entity;
            Fields = Entity.Fields;

            if (Entity.IsVersioning)
            {
                HierarchyField =
                    (EntityField)
                    Entity.Fields.FirstOrDefault(f => f.IdEntityLink == Entity.Id && !_versioningFields.Contains(f.Name));
            }
            else
            {
                HierarchyField = (EntityField)Entity.Fields.FirstOrDefault(f => f.IdEntityLink == Entity.Id);
            }

            IsHierarchy = HierarchyField != null;
            CaptionField = (EntityField)Entity.Fields.FirstOrDefault(f => f.IsCaption == true);

            decorators.Add(new OrganizationRightsDecorator(OrganizationRights.For(Entity.Id)));

            registerServerFilterDecorator(fieldValues, fieldId);

            if (!string.IsNullOrEmpty(searchStr))
            {
                decorators.Add(new AddGridSearch(searchStr, visibleColumnsList));
            }

            checkDenormalizedTp(ownerItemId); // ! может изменить состав Fields

            // отсеиваем из полей скрытые колонки
            Fields = Fields.Where(ef => visibleColumnsList.Contains(ef.Name, StringComparer.InvariantCultureIgnoreCase));
        }


        #region Private Methods

        private List<Dictionary<string, object>> Order(List<Dictionary<string, object>> collection)
        {
            var result = new List<Dictionary<string, object>>();
            var parents = collection.Where(w => string.IsNullOrEmpty(w[HierarchyField.Name].ToString()));

            if (CaptionField != null)
                parents = parents.OrderBy(o => o[CaptionField.Name]);

            foreach (var row in parents)
            {
                result.Add(row);
                AddAllChildrens(result,collection,row);
            }

            return result;
        }

        private void AddAllChildrens(List<Dictionary<string, object>> result, List<Dictionary<string, object>> collection, Dictionary<string, object> row)
        {
            var childrens = collection.Where(w => w[HierarchyField.Name].ToString() == row[Id].ToString());

            if (CaptionField != null)
                childrens = childrens.OrderBy(o => o[CaptionField.Name]);

            foreach (var childRow in childrens)
            {
                result.Add(childRow);
                AddAllChildrens(result, collection, childRow);
            }
        }

        /// <summary>
        /// Получение значений запросом
        /// </summary>
        /// <returns></returns>
        private List<Dictionary<string, object>> GetExportingData()
        {
            //Получаем значения в список словарей
            var result = new List<Dictionary<string, object>>();

            var cmd = getCommand();
            using (SqlDataReader reader = cmd.ExecuteReaderLog())
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                    for (int col = 0; col <= reader.FieldCount - 1; col++)
                    {
                        row.Add(reader.GetName(col).ToLowerInvariant(), reader[col]);
                    }

                    result.Add(row);
                }
                reader.Close();
            }
            return result;
        }

        private SqlCommand getCommand()
        {
            ISelectQueryBuilder selectBuilder = new IoCQueryFactory(Entity).Select();
            selectBuilder.QueryDecorators.Add(new AddSysDimensionsFilter(null));

            if (!string.IsNullOrWhiteSpace(OwnerFieldName) && IdOwner.HasValue)
                selectBuilder.Conditions = getFilterByOwner();
            if(CaptionField != null)
                selectBuilder.Order = new Order {{CaptionField.Name, true}};

            var connection = IoC.Resolve<SqlConnection>("DbConnection");
            return selectBuilder.GetSqlCommand(connection);
        }

        private FilterConditions getFilterByOwner()
        {
            return new FilterConditions()
                {
                    Field = OwnerFieldName,
                    Operator = ComparisionOperator.Equal,
                    Value = IdOwner
                };
        }

        private void registerServerFilterDecorator(string fieldValues, int? fieldId)
        {
            if (!string.IsNullOrWhiteSpace(fieldValues) && fieldId.HasValue)
            {
                decorators.Add(new ServerFiltersDecorator()
                {
                    FieldValues = FieldValues.FromString(fieldValues),
                    IdEntityField = fieldId
                });
            }
        }

        /// <summary>
        /// Проверяет - является ли экспортируемая ТЧ денормализованной.
        /// Если да, то регистрируется декоратор и добавляются поля значимых колонок.
        /// </summary>
        /// <param name="ownerItemId"></param>
        private void checkDenormalizedTp(int? ownerItemId)
        {
            var denormInfo = IoC.Resolve<DenornalizedEntitiesInfo>();
            if (denormInfo.IsMasterTablepart(Entity.Id))
            {
                var analyzer = denormInfo.GetAnalyzerByMaster(Entity.Id);

                if (!ownerItemId.HasValue)
                    throw new PlatformException("При экспорте ДТЧ с клиента должен передаваться ownerItemId");

                var provider = new PeriodsProvider(analyzer.OwnerEntity.Id, (int)ownerItemId, analyzer.MasterTp.Id);

                decorators.Add(new DenormalizerDecorator()
                {
                    TpAnalyzer = analyzer,
                    DenormalizedPeriods = provider.PeriodIds
                });

                Fields = Fields.Concat(provider.PeriodFields);
            }
        }
        #endregion

        #region Конструктор

        public EntitiesListExport()
        {
            IsHierarchy = false;
        }

        #endregion

        #region private properties

        private List<TSqlStatementDecorator> decorators
        {
            get
            {
                return BaseAppEnvironment.Instance.RequestStorage.Decorators;
            }
        }


        /// <summary>
        /// Является ли поле иерархичным
        /// </summary>
        protected bool IsHierarchy { get; set; }

        /// <summary>
        /// Поле иерархии
        /// </summary>
        protected EntityField HierarchyField { get; set; }

        /// <summary>
        /// Поле кэпшэн
        /// </summary>
        protected EntityField CaptionField { get; set; }

        #endregion

        private const string Id = "Id";


        private readonly List<string> _versioningFields = new List<string>() { "idActualItem", "idRoot" };

       
     
     
    }
}
