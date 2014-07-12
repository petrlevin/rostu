using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Interfaces;
using Sbor.Document;

namespace Sbor.CommonControls
{
    [ControlInitialFor(typeof(TestDocument) ,InitialSkippable = true)]
    public class TestFreeCommonControl : IFreeCommonControl<ITestDocument,DataContext>
    {
        public void Execute(DataContext dataContext, ITestDocument element)
        {
            Controls.Throw("Сообщение свободного общего контроля");
        }
    }
}
