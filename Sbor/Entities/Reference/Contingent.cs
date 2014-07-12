using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Reference
{

    public partial class Contingent : ReferenceEntity 
	{

        /// <summary>   
        /// Контроль "Проверка уникальности наименования контингента"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 1)]
        public void Control_502001(DataContext context)
        {
            var sfMsgErr = "В справочнике уже имеется контингент с таким же наименованием:<br>{0}";

            var exists = context.Contingent.Where(r => 
                r.IdPublicLegalFormation == IdPublicLegalFormation &&
                r.Caption.Trim() == Caption.Trim() &&
                r.IdRefStatus != (byte)RefStats.Archive &&
                r.Id != Id); 
           
            if (exists.Any())
            {
                var e = exists.FirstOrDefault();
                Controls.Throw(string.Format(sfMsgErr, e.Caption));
            }
        }
    }
}

