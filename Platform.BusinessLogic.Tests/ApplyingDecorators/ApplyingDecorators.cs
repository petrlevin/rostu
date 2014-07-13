using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BaseApp.Rights;
using BaseApp.Rights.Functional.Decorators;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.DataAccess.ClientFilters;
using Platform.BusinessLogic.Decorators;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Unity;
using Platforms.Tests.Common;

namespace Platform.BusinessLogic.Tests.ApplyingDecorators
{
	[TestFixture]
	class ApplyingDecorators : SqlTests
	{
		[SetUp]
		public void SetUp()
		{
			IUnityContainer uc = new UnityContainer();
			DependencyInjection.RegisterIn(uc, true, false, connectionString);
			IoC.InitWith(new DependencyResolverBase(uc));
			_entityReference = Objects.ByName<Entity>("Entity");
		}

		private static readonly Paging _paging = new Paging { Start = 1, Count = 25 };

		private static readonly Order _order = new Order() {{"Caption", true}};

		private static Entity _entityReference;

		private List<TSqlStatementDecorator> decorators;

		private ISelectQueryBuilder _getBaseISelectQueryBuilder(Entity entity, Paging paging, Order order, string search, IFilterConditions conditions)
		{
			ISelectQueryBuilder queryBuilder = new SelectQueryBuilder(entity);
			queryBuilder.Paging = paging;
			queryBuilder.Order = order;
			queryBuilder.Search = search;
			queryBuilder.Conditions = conditions;
			return queryBuilder;
		}

		[Test]
		public void SimpleReference()
		{
			ISelectQueryBuilder queryBuilder = _getBaseISelectQueryBuilder(_entityReference, _paging, null, "", null);
			int result = 0;
			using (SqlCommand command= queryBuilder.GetSqlCommand(connection))
			{
				result = command.ExecuteNonQuery();
			}
			Assert.AreNotEqual(0, result);
		}

		[Test]
		public void SimpleReferenceWithOrder()
		{
			var queryBuilder = _getBaseISelectQueryBuilder(_entityReference, _paging, _order, "", null);
			var result = 0;
			using (SqlCommand command = queryBuilder.GetSqlCommand(connection))
			{
				result = command.ExecuteNonQuery();
			}
			Assert.AreNotEqual(0, result);
		}

		public void SimpleReferenceWithOrderAsHierarchy()
		{
			var queryBuilder = _getBaseISelectQueryBuilder(_entityReference, _paging, _order, "", null);
		}

		private void _registerDecorators(GridParams param)
		{
			//registerDenormalizer(decorators, param);
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
			//decorators.Add(new OrganizationRightsDecorator(OrganizationRights.For(param.EntityId)));
			//registerFilterForControls(param, decorators);
			if (param.FieldId.HasValue)
			{
				decorators.Add(new AddFilterByStatusForReference(param.FieldId.Value, param.EntityId));
				decorators.Add(new AddFilterEntitiesForGenericLink(param.FieldId.Value));

				decorators.Add(new AddFilterByDateForVersioning(param.FieldId.Value, param.EntityId, param.FieldValues, param.DocId));

				decorators.Add(new AddFilterNoParentForHierarchyReference(param.FieldId.Value));
			}
			/*
			if (param.Filters.Any())
			{
				decorators.Add(new ClientFiltersForExtendedFieldsDecorator(param.Filters));
			}*/
		}

	}
}
