using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.DbEnums;

namespace BaseApp.GeneratedCode
{

	public class BudgetLevel : ReferenceEntity 
	{
	
		public Int32 id{get; set;}
		public int idParent{get; set;} public virtual BudgetLevel _idParent{get; set;}
		public string Caption{get; set;}
	
		public BudgetLevel()
        {
             
		}
	}
}

