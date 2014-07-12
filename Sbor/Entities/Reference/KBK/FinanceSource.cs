using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;

namespace Sbor.Reference
{

    public partial class FinanceSource
	{
        /// <summary>   
        /// Контроль "Проверка уникальности кода"
        /// </summary>         
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 1)]
        public void Control_500701(DataContext context)
        {
            var sCode = Code.Trim();
            var MsgErr = string.Format("В справочнике уже имеется элемент с кодом {0}.", sCode);
    
            var exists = context.FinanceSource.Where(r => r.Code.Trim() == sCode && r.Id != Id);

            if (exists.Any())
            {
                Controls.Throw(MsgErr);
            }
        }

        public override string ToString()
        {
            return "ИФ " + Code;
        }
    }
}

