using System;
using System.Collections.Generic;
using System.Linq;
using BaseApp.Common.Interfaces;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Interfaces;
using Platform.Dal.Requirements;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.Rights.Organizational.Decorators
{
    /// <summary>
    /// Декоратор добавляющий фильтрацию по организационным правам
    /// </summary>
    public class OrganizationRightsDecorator : OrganizationRightsDecorator<Implementation>, IHasRequirements, IApplyForAggregate
    {



        /// <summary>
        /// 
        /// </summary>
        /// <param name="rightsData"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public OrganizationRightsDecorator(IOrganizationRightData rightsData)
            : base(rightsData)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IRequirement> GetRequirements()
        {
            return new List<IRequirement> {new SourceFields(){Fields =RightsData.Rights.Where(r=>(r.Key.Name.ToLower()!="id")).Select(r=>r.Key.Name).ToList() }}


            ;
            
        }

    }
}
