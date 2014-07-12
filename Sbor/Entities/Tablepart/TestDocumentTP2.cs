using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;

namespace Sbor.Tablepart
{
    /// <summary>
	/// TestDocumentTP2
	/// </summary>
	public partial class TestDocumentTP2 
	{

        [Control(ControlType.Update, Sequence.Any)]
        public void ControlOfTabPart2(DataContext dataContext, Sequence sequence
            )
        {
            Amount++;
        }


        [Control(ControlType.Update, Sequence.Any)]
        public void ControlOfTabPart2_Throw(DataContext dataContext, Sequence sequence
            )
        {
            Controls.Throw("ВЫ не заплатили за газ!");
        }


	}
}