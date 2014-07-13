using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;



namespace BaseApp.Reference
{
	/// <summary>
	/// Расширение организационных прав
	/// </summary>
    public partial class OrganizationRightExtension : IOrganizationRightExtension 
	{


        IEnumerable<Platform.PrimaryEntities.Common.Interfaces.IEntity> IOrganizationRightExtension.Entities
        {
            get { return Results; }
        }

        string IOrganizationRightExtension.SqlTemplate
        {
            get { return SqlTemplate; }
        }
    }
}