using System.Collections.Generic;
using System.Linq;
using BaseApp.Environment;
using BaseApp.ReportProfile;
using BaseApp.Rights;
using BaseApp.Rights.Functional.Decorators;
using BaseApp.Rights.Organizational;
using BaseApp.Rights.Organizational.Decorators;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.DataAccess.ClientFilters;
using Platform.BusinessLogic.Decorators;
using Platform.BusinessLogic.Denormalizer.Decorator;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.PrimaryEntities.Factoring;
namespace BaseApp.DataAccess
{
    /// <summary>
    /// Временное решение. Цель вынести декораторы из проекта Platform.Web
    /// </summary>
    public static class DecoratorsManager
    {
        /// <summary>
        /// Возвращает список sql-декораторов, применяемых при получении списка элементов для грида
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static void RegisterDecorators(GridParams param)
        {
            var decorators = BaseAppEnvironment.Instance.RequestStorage.Decorators;

            RegisterDenormalizer(decorators, param);
            decorators.Add(new AddSysDimensionsFilter(param.FieldId));
            if (!string.IsNullOrEmpty(param.Search))
            {
                decorators.Add(new AddGridSearch(param.Search, param.VisibleColumns.ToList()));
            }
            if (param.FieldId.HasValue && param.DocId.HasValue)
            {
                decorators.Add(new AddFilterOnTablepartOwnerFieldForParentLink(param.FieldId.Value, param.DocId.Value));
            }
            if (param.FieldId.HasValue && param.ItemId.HasValue)
            {
                decorators.Add(new AddPreventCircles(param.FieldId.Value, param.ItemId.Value));
            }
            decorators.Add(new ServerFiltersDecorator()
            {
                FieldValues = param.FieldValues,
                IdEntityField = param.FieldId
            });
            if (!string.IsNullOrWhiteSpace(param.HierarchyFieldName))
            {
                if (param.HierarchyFieldValues != null && param.HierarchyFieldValues.Count > 0)
                {
                    decorators.Add(new AddHierarcyFilter(param.HierarchyFieldName, param.HierarchyFieldValues));
                }
                else if (param.HierarchyFieldValue.HasValue)
                {
                    decorators.Add(new AddHierarcyFilter(param.HierarchyFieldName, new List<int?> { param.HierarchyFieldValue }));
                }
                else
                {
                    decorators.Add(new AddHierarcyFilter(param.HierarchyFieldName, null));
                }

            }
            decorators.Add(new AddActualItemForVersioning(param.ActualDate));
            decorators.Add(new OrganizationRightsDecorator(OrganizationRights.For(param.EntityId)));
            RegisterFilterForControls(param, decorators);
            if (param.FieldId.HasValue)
            {
                decorators.Add(new AddFilterByStatusForReference(param.FieldId.Value, param.EntityId));
                decorators.Add(new AddFilterEntitiesForGenericLink(param.FieldId.Value));

                decorators.Add(new AddFilterByDateForVersioning(param.FieldId.Value, param.EntityId, param.FieldValues, param.DocId));

                decorators.Add(new AddFilterNoParentForHierarchyReference(param.FieldId.Value));
            }

            if (param.Filters.Any())
            {
                decorators.Add(new ClientFiltersForExtendedFieldsDecorator(param.Filters));
            }
        }

        public static void RegisterMultilinkDecorators(GridParams param)
        {
            // ToDo{CORE-1014} копипаст с методом RegisterDecorators. 
            var decorators = BaseAppEnvironment.Instance.RequestStorage.Decorators;
            if (param.Filters.Any())
            {
                decorators.Add(new ClientFiltersForExtendedFieldsDecorator(param.Filters));
            }
        }

        public static void RegisterUserDecorators()
        {
            RegisterFunctionalRightsDecorators();
			RegisterFilterEntityDecorator();
        }

        public static void RegisterReportProfileDecorators()
        {
            var decorators = BaseAppEnvironment.Instance.RequestStorage.Decorators;
            decorators.Add(new ReportProfileDecorator());
        }

        public static void RegisterSysDimensionDecorators(GridParams param)
        {
            var decorators = BaseAppEnvironment.Instance.RequestStorage.Decorators;

            var entity = Objects.ById<Entity>(param.EntityId);
            decorators.Add(new AddFilterByStatusForReference(entity));
        }

        /// <summary>
        /// Проверяет потребность в денормализации сущности <paramref name="entityId"/> 
        /// и в случае ее необходимости регистрирует соответствующий декоратор.
        /// </summary>
        /// <param name="decorators"></param>
        /// <param name="param"></param>
        private static void RegisterDenormalizer(List<TSqlStatementDecorator> decorators, GridParams param)
        {
            var source = Objects.ById<Entity>(param.EntityId);
            var factory = new DenormalizerDecoratorFactory(source);
            if (factory.IsMasterTablepart)
            {
                decorators.Add(factory.GetDecorator(param.DenormalizedPeriods));
            }
        }

        private static void RegisterFilterForControls(GridParams param, List<TSqlStatementDecorator> decorators)
        {
            var user = BaseAppEnvironment.Instance.SessionStorage.CurrentUser;

            if ((!user.IsSuperUser()) && (param.EntityId == Platform.PrimaryEntities.Factoring.Objects.ByName<Entity>("Control").Id))
                decorators.Add(
                    new AddWhere(new FilterConditions() { Field = "managed", Type = LogicOperator.Simple, Value = true }, LogicOperator.And));
        }

        private static void RegisterFunctionalRightsDecorators()
        {
            var user = BaseAppEnvironment.Instance.SessionStorage.CurrentUser;
            var decorators = BaseAppEnvironment.Instance.RequestStorage.Decorators;

            decorators.Add(new FunctionalRightsDecorator(user));
        }

        private static void RegisterFilterEntityDecorator()
        {
            var decorators = BaseAppEnvironment.Instance.RequestStorage.Decorators;
            decorators.Add(new AddFilterEnitiesByModule());
        }

        public static void RegisterRegInfoDecorators(CustomFilterGridParams param)
        {
            var decorators = BaseAppEnvironment.Instance.RequestStorage.Decorators;
            decorators.Add(new Platform.BusinessLogic.Registry.FilterDecorator(param.EntityId, param.FilterEntityId, param.FilterValueId));

        }

        public static void RegisterDenormalizerDecorators(int entityId, int? ownerEntityId, int? ownerItemId)
        {
            var decorators = BaseAppEnvironment.Instance.RequestStorage.Decorators;
            var source = Objects.ById<Entity>(entityId);
            var factory = new DenormalizerDecoratorFactory(source);
            if (factory.IsMasterTablepart)
            {
                if (!ownerEntityId.HasValue)
                    throw new PlatformException("При открытии элемента денормализованной ТЧ не передан OwnerEntityId");
                if (!ownerItemId.HasValue)
                    throw new PlatformException("При открытии элемента денормализованной ТЧ не передан OwnerItemId");

                var entityManager = new EntityManager(Objects.ById<Entity>((int)ownerEntityId));
                IBaseEntity obj = entityManager.Find((int)ownerItemId);
                if (obj is IColumnFactoryForDenormalizedTablepart)
                {
                    IColumnFactoryForDenormalizedTablepart item = (IColumnFactoryForDenormalizedTablepart)obj;
                    IEnumerable<int> periods = item.GetColumns(source.Name).Periods.Select(a => a.PeriodId);
                    decorators.Add(factory.GetDecorator(periods));
                }
                else
                {
                    throw new PlatformException("При открытии элемента денормализованной ТЧ передан OwnerEntityId не реализующий интерфейс IColumnFactoryForDenormalizedTablepart");
                }
            }
        }

    }
}
