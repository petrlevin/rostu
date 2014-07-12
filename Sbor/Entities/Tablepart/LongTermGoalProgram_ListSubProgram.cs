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
using Sbor.Reference;


namespace Sbor.Tablepart
{
	/// <summary>
	/// Перечень подпрограмм
	/// </summary>
    public partial class LongTermGoalProgram_ListSubProgram : TablePartEntity, ITpListSubProgram 
	{
        #region Implementation of IDocSGE

        [NotMapped]
        public int IdDocType { get { return DocType.SubProgramDGP; } }

        [NotMapped]
        public int IdAnalyticalCodeStateProgramValue { get { return IdAnalyticalCodeStateProgram ?? 0; } set { IdAnalyticalCodeStateProgram = value; } }

        #endregion
    }
}