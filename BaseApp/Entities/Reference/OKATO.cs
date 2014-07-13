using System.Linq;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Activity.Controls;

namespace BaseApp.Reference
{
	public partial class OKATO
	{
        /// <summary>
        /// Автоконтроль на уникальность кода ОКАТО
        /// </summary>
        /// <param name="context"></param>
        [Control(ControlType.Update|ControlType.Insert, Sequence.Before, ExecutionOrder=10)]
        public void Control_501201(DataContext context)
        {
           if (context.OKATO.Any(a => a.Caption == Caption && a.Id != Id))
               Controls.Throw("В справочнике уже имеется элемент с таким же кодом ОКАТО.");
        }
	}
}

