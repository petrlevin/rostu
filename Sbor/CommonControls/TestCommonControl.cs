using System;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Sbor.Document;

namespace Sbor.CommonControls
{
    [Control(ControlType.Update, Sequence.After, ExecutionOrder = 10000)]
    [ControlOrderFor(typeof(TestDocument),1)]
    [ControlInitial(ExcludeFromSetup = true, InitialSkippable = true,InitialCaption = "yuuu")]
    [ControlInitialFor(typeof(TestDocument3),InitialSkippable = false,InitialUNK = "10909")]
    [ControlInitialFor(typeof(TestDocument),  InitialUNK = "210909" ,InitialCaption = "жесть")]
    public class TestCommonControl : ICommonControl<ITestDocument,DataContext>
    {
        public void Execute(DataContext dataContext, ControlType controlType, Sequence sequence, ITestDocument element,
                            ITestDocument oldElement)
        {
            Controls.Throw(String.Format("Общий контрол {0} , {1} " ,element.Date ,element.Number));
        }
    }
}