using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Document;
using Sbor.Logic;

namespace Sbor.Tablepart
{
    public partial class LongTermGoalProgram_Activity 
	{
        // !!! НЕ УДАЛЯТЬ !!!
        // !!! Данный контроль веременно отключен пока не будут налажены вызовы контролей для ДТЧ
        /// <summary>   
        /// Контроль "Наличие объемов у мероприятий"
        /// </summary> 
        //[ControlInitial(InitialUNK = "0224", InitialCaption = "Наличие объемов у мероприятий")]
        //[Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 50)]
        //public void Control_0224(DataContext context)
        //{
        //    var tpActivity0 = context.LongTermGoalProgram_Activity.Where(r => r.Id == this.Id).ToList();
        //    var tpActivity_Value0 = context.LongTermGoalProgram_Activity_Value.Where(r => r.IdMaster == this.Id).ToList();

        //    LongTermGoalProgram.CtrlPart0224(tpActivity0, tpActivity_Value0);
        //}
    }	
}