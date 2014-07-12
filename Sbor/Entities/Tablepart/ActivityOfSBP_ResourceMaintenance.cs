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
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Ресурсное обеспечение
	/// </summary>
    public partial class ActivityOfSBP_ResourceMaintenance : TablePartEntity, ITpResourceMaintenance   
	{
        #region Implementation of ITpResourceMaintenanc

        [NotMapped]
        public int IdMaster { get { return 0; } set { ; } }

        #endregion
    }
}