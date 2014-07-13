using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Registry
{
    /// <summary>
    /// 
    /// </summary>
    public class FilterDecorator : AddWhere
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="regEntityId"></param>
        /// <param name="filterEntityId"></param>
        /// <param name="filterId"></param>
        public FilterDecorator(int regEntityId, int filterEntityId, int filterId)
            : base(BuildConditions(regEntityId, filterEntityId, filterId),LogicOperator.And)
        {

        }

        private static IFilterConditions BuildConditions(int regEntityId, int filterEntityId, int filterId)
        {
            var regEntity = Objects.ById<Entity>(regEntityId);
            if (
                regEntity.Fields.Any(
                    ef =>
                    (ef.Name.ToLower() == "IdRegistrator".ToLower()) &&
                    ((ef.EntityFieldType == EntityFieldType.ToolEntity) ||
                     (ef.EntityFieldType == EntityFieldType.DocumentEntity))))
            {
                return FilterConditions.Equal("IdRegistrator", filterId) &
                       FilterConditions.Equal("IdRegistratorEntity", filterEntityId);
            }
            else
            {
                return FilterConditions.Equal("IdRegistrator", filterId);
            }
        }
    }
}
