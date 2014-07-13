using System;
using System.Linq;
using System.Collections.Generic;
using BaseApp;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common.Extensions;

namespace BaseApp.Reference
{

	public partial class ResponsiblePerson : ReferenceEntity 
	{
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_500201(DataContext context)
        {
            //if (IdOrganization.HasValue && RoleResponsiblePerson != null && !DateEnd.HasValue)
            if (RoleResponsiblePerson != null && !DateEnd.HasValue)
            {
                ResponsiblePerson rp = context.ResponsiblePerson.FirstOrDefault(a =>
					a.IdOrganization == IdOrganization
                    && a.IdRoleResponsiblePerson == IdRoleResponsiblePerson
                    && a.Id != Id
                    && !a.DateEnd.HasValue
                    && true
                );
                if (rp != null)
                    Controls.Throw(string.Format(
                        "Уже имеет ответственное лицо с ролью «{0}»<br>"+
                        "Для сохранения текущего элемента требуется предварительно прекратить полномочия у лица:<br>{1}",
                        rp.RoleResponsiblePerson.Caption(), 
                        rp.Caption
                ));
            }
        }
    }
}

