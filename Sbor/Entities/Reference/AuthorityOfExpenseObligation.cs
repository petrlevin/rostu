using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;

namespace Sbor.Reference
{
    public partial class AuthorityOfExpenseObligation : ReferenceEntity 
	{
        /// <summary>   
        /// Контроль "Проверка уникальности кода РО"
        /// </summary>
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_500401(DataContext context)
        {
            if (context.AuthorityOfExpenseObligation.Any(a => a.Caption == Caption && a.Id != Id))
                Controls.Throw(string.Format(
                    "В справочнике уже имеется расходное обязательство с кодом {0}.", Caption
                ));
        }
    }
}

