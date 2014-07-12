using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;

namespace Sbor.CommonControls
{

    //[Control(ControlType.Update, Sequence.After, ExecutionOrder = 10000)]
    [ControlInitial(InitialSkippable = true, InitialUNK = "90156" ,ExcludeFromSetup = true)]
    public class TestAllToolCommonControl : ICommonControl<ToolEntity, DataContext>
    {
        public void Execute(DataContext dataContext, ControlType controlType, Sequence sequence, ToolEntity element,
                            ToolEntity oldElement)
        {
            Controls.Throw("Сообщение контрола TestAllToolCommonControl ");
        }
    }
}
