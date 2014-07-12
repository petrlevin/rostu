using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.PrimaryEntities;

namespace Sbor.CommonControls
{
    //[Control(ControlType.Update, Sequence.After, ExecutionOrder = 1)]
    [ControlInitial(InitialSkippable = true, InitialUNK = "__90156" ,ExcludeFromSetup = true)]
    public class TestAllCommonControl : ICommonControl<BaseEntity,DataContext>
    {

        
    
    
        public void Execute(DataContext dataContext, ControlType controlType, Sequence sequence, BaseEntity element,
                            BaseEntity oldElement)
        {
            Controls.Throw("Сообщение контрола AllCommonControl ");
        }
    

        
    }
}
