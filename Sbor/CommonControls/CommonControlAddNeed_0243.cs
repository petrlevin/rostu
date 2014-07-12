using System;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Sbor.Document;
using Sbor.Interfaces;

namespace Sbor.CommonControls
{
    [ControlInitialFor(typeof(ActivityOfSBP), InitialSkippable = false, InitialUNK = "0336", InitialCaption = "Проверка признака «Вести доппотребности»")]
    [ControlInitialFor(typeof(LongTermGoalProgram), InitialSkippable = false, InitialUNK = "0243", InitialCaption = "Проверка признака «Вести доппотребности»")]
    [ControlInitialFor(typeof(StateProgram), InitialSkippable = false, InitialUNK = "0140", InitialCaption = "Проверка признака «Вести доппотребности»")]
    public class CommonControlAddNeed_0243 : IFreeCommonControl<IAddNeed, DataContext>
    {
        public void Execute(DataContext dataContext, IAddNeed element)
        {
            if (!element.HasAdditionalNeed)
            {
                Controls.Throw("В документе отсутствуют значения по доп. потребностям. Воспользуйтесь операцией «Утвердить».");
            }
        }
    }
}