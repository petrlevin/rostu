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
using Sbor.Reference;


namespace Sbor.Tablepart
{
	/// <summary>
	/// Ресурсное обеспечение
	/// </summary>
    public partial class LongTermGoalProgram_ResourceMaintenance : TablePartEntity, ITpResourceMaintenance
    {

        // !!! НЕ УДАЛЯТЬ !!!
        // !!! Данный контроль веременно отключен пока не будут налажены вызовы контролей для ДТЧ
        /// <summary>   
        /// Контроль "Проверка наличия ресурсного обеспечения документа за один период в разрезе ИФ и без разреза по ИФ"
        /// </summary> 
        //[ControlInitial(InitialCaption = "Проверка наличия ресурсного обеспечения документа за один период в разрезе ИФ и без разреза по ИФ", InitialUNK = "0219")]
        //[Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 50)]
        //public void Control_0219(DataContext context)
        //{
        //    var tpResourceMaintenance0 = context.LongTermGoalProgram_ResourceMaintenance.Where(r => r.IdOwner == this.IdOwner).ToList();
        //    var tpResourceMaintenanceValue0 = context.LongTermGoalProgram_ResourceMaintenance_Value.Where(r => r.IdOwner == this.IdOwner).ToList();

        //    LongTermGoalProgram.CtrlPart0219(tpResourceMaintenance0, tpResourceMaintenanceValue0);
        //}

        #region Implementation of ITpResourceMaintenanc

        [NotMapped]
        public int IdMaster { get { return 0; } set { ; } }

        #endregion
    }
}