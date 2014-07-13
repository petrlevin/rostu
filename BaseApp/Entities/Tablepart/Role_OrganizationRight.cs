using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.PrimaryEntities.Reference;
namespace BaseApp.Tablepart
{
	/// <summary>
	/// ТЧ Организационные права
	/// </summary>
    public partial class Role_OrganizationRight 
	{

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before)]
        public void CheckElementNullAndParent()
        {
            if ((IdElement==null) && (IdParentField!=null))
                Controls.Throw("Для прав с неуказанным элементом нельзя использовать иерархию");
        }
	}
}