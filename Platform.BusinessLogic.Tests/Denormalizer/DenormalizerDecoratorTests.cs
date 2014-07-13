using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Decorator;
using Platform.BusinessLogic.ServerFilters;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;
using Platforms.Tests.Common;
using Platform.Dal.Interfaces;

namespace Platform.BusinessLogic.Tests.Denormalizer
{
	/// <summary>
	/// Целью данного теста является помощь в написании денормализатора.
	/// Чтобы тестировать его функционал не через интерфейс (это долго), а через студию, выполняя тесты в режиме debug.
	/// В любом случае тест должен отрабатывать без падений :)
	/// </summary>
	[TestFixture]
	public class DenormalizerDecoratorTests : SqlTestBase
	{
		private DataContext db;

		private IEntity _targetTablepart;

		/// <summary>
		/// ТЧ для денормализации
		/// </summary>
		private IEntity targetTablepart
		{
			get
			{
				if (_targetTablepart == null)
				{
					_targetTablepart = Objects.ByName<Entity>("SystemGoal_GoalIndicatorValue");
				}
				return _targetTablepart;
			}
		}

		[SetUp]
		public void Setup()
		{
			IUnityContainer uc = new UnityContainer();
			DependencyInjection.RegisterIn(uc, true, false, connectionString);
			IoC.InitWith(new DependencyResolverBase(uc));
			db = new DataContext();
		}

		[Test]
		public void GetItem()
		{
			var builder = getBuilder();
			var cmd = builder.GetSqlCommand(this.connection);
			var sql = cmd.CommandText;
		}

		private SelectQueryBuilder getBuilder()
		{
			return new SelectQueryBuilder(targetTablepart)
				{
					QueryDecorators = new List<TSqlStatementDecorator> {getDenormalizer()},
					Conditions = new FilterConditions()
                                        {
                                            Field = "id",
											Value = "-1677721575"
                                        },
					Search = "Версия по-умолчанию",
					Paging = new Paging { Start = 1, Count = 10 },
					Order = new Order {{"idGoalIndicator", false}}
				};
		}

		private DenormalizerDecorator getDenormalizer()
		{
			var factory = new DenormalizerDecoratorFactory((Entity)targetTablepart);
			var periods = new List<int>
				{
					-1879048164,
					-1879048163,
					-1879048162,
					-1879048161,
					-1879048160
				};
			DenormalizerDecorator denormalizer = factory.GetDecorator(periods);
			return denormalizer;
		}
	}
}
