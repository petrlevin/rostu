using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.SummaryAggregates
{
    public class AggregateQueryBuilder : Platform.Dal.QueryBuilders.SelectQueryBuilder
    {
        private AddAggregateDecorator aggregate;
        private string hierarchyFieldName;

        public AggregateQueryBuilder(IEntity entity, string hierarchyFieldName = null)
            : base(entity)
        {
            this.QueryDecorators = IoC.Resolve<List<TSqlStatementDecorator>>("Decorators").Where(d => d is IApplyForAggregate).ToList();
            this.hierarchyFieldName = hierarchyFieldName;
			aggregate = new AddAggregateDecorator(Entity, hierarchyFieldName);
            AfterPrivateDecorators.Add(new AddDbComputedFunctionFields());
			AfterPrivateDecorators.Add(aggregate);
        }

        protected override void InitPrivateDecorators()
        {
            AddWhere condidions = new AddWhere();
            //AddGridSearch gridSearch = new AddGridSearch();
            


            BeforePrivateDecorators.AddRange(new List<TSqlStatementDecorator>
				{
					condidions
				});

        }

        public List<String> AggregateFields
        {
            get { return aggregate.Fields; }
        }

    }
}
