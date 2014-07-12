using System;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Sbor.Document;

namespace Sbor.CommonControls
{
    [Control(ControlType.Update, Sequence.After, ExecutionOrder = 10000)]
    [ControlInitial(InitialSkippable = true ,InitialUNK = "6790" , InitialCaption = "qwertytrewq",ExcludeFromSetup = true)]
    
    public class TestCommonControl2 : ICommonControl<ITestDocument,DataContext>
    {
        public void Execute(DataContext dataContext, ControlType controlType, Sequence sequence, ITestDocument element,
                            ITestDocument oldElement)
        {
            Controls.Throw(String.Format("Общий контрол 2222 {0} , {1} " ,element.Date ,element.Number));
        }
    }
}