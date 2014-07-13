using System;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;



namespace BaseApp.Reference
{
    /// Настройки проекта
    /// </summary>
    public partial class ProjectSettings : ReferenceEntity
    {
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Delete | ControlType.Update, Sequence.Before, ExecutionOrder = 0)]
        public void ControlMain(DataContext context)
        {
            Controls.Throw("Любое изменение данного справочника запрещено.");
        }
    }
}