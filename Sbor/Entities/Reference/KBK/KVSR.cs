using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;

namespace Sbor.Reference
{
    public partial class KVSR : ReferenceEntity 
	{
        public override string ToString()
        {
            return "КВСР " + Caption;
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_500601(DataContext context)
        {
            /* проверка на уровне триггера есть
			KVSR obj = context.KVSR.FirstOrDefault(a =>
                a.Caption == Caption 
                && a.Id != Id
                && (
                   ((a.ValidityFrom ?? DateTime.MinValue) <= (ValidityFrom ?? DateTime.MinValue) 
                        && (a.ValidityTo ?? DateTime.MaxValue) >= (ValidityTo ?? DateTime.MaxValue))
                || ((ValidityFrom ?? DateTime.MinValue) <= (a.ValidityFrom ?? DateTime.MinValue)
                        && (ValidityTo ?? DateTime.MaxValue) >= (a.ValidityTo ?? DateTime.MaxValue))
                ) && true
            );
            if (obj != null)
                Controls.Throw(string.Format(
                    "В справочнике на указанный период действия уже имеется элемент с таким же кодом:<br>{0}{1}{2}{3}",
                    obj.Caption,
                    obj.ValidityFrom.HasValue || obj.ValidityTo.HasValue ? " -" : "",
                    obj.ValidityFrom.HasValue ? obj.ValidityFrom.Value.ToString(" с dd.MM.yyyy")  : "",
                    obj.ValidityTo.HasValue   ? obj.ValidityTo.  Value.ToString(" по dd.MM.yyyy") : ""
                ));*/
        }
    }
}

