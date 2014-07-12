using System;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Sbor.Document;
using Sbor.Interfaces;

namespace Sbor.CommonControls
{
    [ControlInitialFor(typeof(ActivityOfSBP), InitialSkippable = true, InitialUNK = "0335", InitialCaption = "Проверка признака «Вести доппотребности»")]
    [ControlInitialFor(typeof(LongTermGoalProgram), InitialSkippable = true, InitialUNK = "0242", InitialCaption = "Проверка признака «Вести доппотребности»")]
    [ControlInitialFor(typeof(StateProgram), InitialSkippable = true, InitialUNK = "0139", InitialCaption = "Проверка признака «Вести доппотребности»")]
    public class CommonControlAddNeed_0242 : IFreeCommonControl<IAddNeed, DataContext>
    {
        public void Execute(DataContext dataContext, IAddNeed element)
        {
            if (element.HasAdditionalNeed)
            {
                Controls.Throw("Документ ведется с доп.потребностями. Вы запустили операцию утверждения базовых значений. Будет создана и утверждена новая редакция документа с очищенными данными по доп. потребностям.");
            }
        }
    }
}