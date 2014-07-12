using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.Utils.Common;
using Sbor.Logic;


namespace Sbor.Tablepart
{
	/// <summary>
	/// ВЦП и основные мероприятия
	/// </summary>
	public partial class StateProgram_DepartmentGoalProgramAndKeyActivity : TablePartEntity, ITpListSubProgram   
	{
        #region Implementation of IDocSGE

        [NotMapped]
        public int IdAnalyticalCodeStateProgramValue { get { return IdAnalyticalCodeStateProgram; } set { IdAnalyticalCodeStateProgram = value; } }

        #endregion
    }
}