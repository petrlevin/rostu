using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.OrganizationalRoles;
using BaseApp.ServerFilters;
using BaseApp.SystemDimensions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Dal;
using Platform.Dal.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.References;

namespace BaseApp.Tests.Unity
{
	/// <summary>
	/// Автоматическое создание зарегистрированных декораторов
	/// </summary>
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class DecoratorsResolvingTests
	{
		private UnityContainer container;

		[SetUp]
		public void Init()
		{
			container = new UnityContainer();
		}

		/// <summary>
		/// Регистрируем в контейнере значения серверных фильтров.
		/// Используется декоратором серверных фильтров.
		/// </summary>
		private void regServerFilterValues()
		{
			var serverFilterValues = new ServerFilterValues()
				{
					{"FieldName", "Value"}
				};
			container.RegisterInstance(serverFilterValues); // Т.о. необходимо регистрировать серверный фильтр в RequestStorage
		}

		/// <summary>
		/// При создании декоратора через Unity значение серверных фильтров должно резолвится.
		/// </summary>
		[Test]
        
		public void ServerFilterValues_ShouldResolve()
		{
			regServerFilterValues();
			var decorator = container.Resolve<ServerFiltersDecorator>();
			Assert.AreEqual("Value", decorator.FilterValues["FieldName"]);
		}

		[Ignore]
		[Test]
		public void RegisteredDecoratorsShouldResolve()
		{
			regServerFilterValues();

			var registeredDecorators = new List<Type>
				{
					typeof (OrgRolesDecorator),
					typeof (ServerFiltersDecorator),
					typeof (SysDimensionsDecorator)
				};

			var entity = new Entity();
			var query = new QueryFactory(entity).Select();
			query.QueryDecorators = registeredDecorators.Select(type => (ITSqlStatementDecorator)container.Resolve(type)).ToList();

			// ToDo: Проверить корректность созданных объектов декораторов
		}
	}
}
