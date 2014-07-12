using System;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Sbor.Document;
using Sbor.Interfaces;

namespace Sbor.CommonControls
{
    [ControlInitialFor(typeof(ActivityOfSBP), InitialSkippable = true, InitialUNK = "0337", InitialCaption = "Проверка признака «Вести доппотребности»")]
    [ControlInitialFor(typeof(LongTermGoalProgram), InitialSkippable = true, InitialUNK = "0244", InitialCaption = "Проверка признака «Вести доппотребности»")]
    [ControlInitialFor(typeof(StateProgram), InitialSkippable = true, InitialUNK = "0141", InitialCaption = "Проверка признака «Вести доппотребности»")]
    public class CommonControlAddNeed_0244 : IFreeCommonControl<IAddNeed, DataContext>
    {
        public void Execute(DataContext dataContext, IAddNeed element)
        {
            if (element.HasAdditionalNeed)
            {
                Controls.Throw("Будет создана и утверждена новая редакция документа – данные по доп. потребностям будут суммированы с базовыми значениями.");
            }
        }
    }
}