using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.Utils.Common;
using Sbor.Document;
using Sbor.Logic;


namespace Sbor.Tablepart
{
	/// <summary>
	/// Ресурсное обеспечение ВЦП и основных мероприятий
	/// </summary>
    public partial class StateProgram_DGPKAResourceMaintenance : TablePartEntity, ITpResourceMaintenance   
	{
        // !!! НЕ УДАЛЯТЬ !!!
        // !!! Данный контроль веременно отключен пока не будут налажены вызовы контролей для ДТЧ
        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения ВЦП и  ОМ за один период в разрезе ИФ и без разреза по ИФ "
        /// </summary> 
        //[ControlInitial(InitialCaption = "Проверка наличия ресурсного обеспечения ВЦП и  ОМ за один период в разрезе ИФ и без разреза по ИФ ", InitialUNK = "0130")]
        //[Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 50)]
        //public void Control_0130(DataContext context)
        //{
        //    var tpResourceMaintenance0 = context.StateProgram_DGPKAResourceMaintenance.Where(r => r.IdOwner == this.IdOwner && r.IdMaster == this.IdMaster).ToList();
        //    var tpResourceMaintenance_Value0 = context.StateProgram_DGPKAResourceMaintenance_Value.Where(r => r.IdOwner == this.IdOwner).ToList();

        //    StateProgram.CtrlPart0130(tpResourceMaintenance0, tpResourceMaintenance_Value0);
        //}
    }
}