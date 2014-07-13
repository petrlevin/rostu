using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.Rights.Organizational.Decorators
{
    /// <summary>
    /// 
    /// </summary>
    public class ImplementationRevertStrict : ImplementationRevert
    {

        



        protected override void AddRightGroup(IGrouping<IEntityField, IOrganizationRightInfo> rightGroup)
        {
            if ((rightGroup.Key.Name.ToLower()!="id") && (Query.GetSelectColumn(rightGroup.Key.Name) != null))
                base.AddRightGroup(rightGroup);
        }



        protected override SchemaObjectTableSource CrateCurrentGroup(string alias)
        {
            var currentGroup = Helper.CreateSchemaObjectTableSource("", OrganizationRightsSelectQueryBuilder.WithPartName, alias);
            return currentGroup;
        }


    }
}
