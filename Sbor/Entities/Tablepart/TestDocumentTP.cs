using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;

namespace Sbor.Tablepart
{
    /// <summary>
	/// Таб часть тестового документа
	/// </summary>
	public partial class TestDocumentTP 
	{
	
            [Control(ControlType.Update, Sequence.Any)]
            public void ControlOfTabPart(DataContext dataContext,Sequence sequence
                )
            {
                Value = Value + ((sequence == Sequence.Before )? "b" : "a");
                Controls.Throw("Уходите от меня . Я люблю другого человека!!");
            }



	}
}