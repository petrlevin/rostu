using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.Utils.Common;
using Sbor.Interfaces;
using Sbor.Logic;


namespace Sbor.Tablepart
{
	/// <summary>
	/// Ресурсное обеспечение
	/// </summary>
    public partial class ActivityOfSBP_ActivityResourceMaintenance : TablePartEntity, ITpResourceMaintenance, ILineCostWithRelations
	{
	    /// <summary>   
	    /// Контроль "Проверка уникальности сметной строки"
	    /// </summary>         
	    [ControlInitial(InitialUNK = "0352", InitialCaption = "Проверка уникальности сметной строки",
	        InitialSkippable = false)]
	    public void Control_0352(DataContext context)
	    {
            Owner.Control0352(context);
	    }
	}
}