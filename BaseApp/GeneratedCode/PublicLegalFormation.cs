using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.DbEnums;

namespace BaseApp.GeneratedCode
{

	public class PublicLegalFormation : ReferenceEntity 
	{
	
		public Int32 id{get; set;}
		public int idParent{get; set;} public virtual PublicLegalFormation _idParent{get; set;}
		public string Caption{get; set;}
		public int idBudgetLevel{get; set;} public virtual BudgetLevel _idBudgetLevel{get; set;}
		public int idAccessGroup{get; set;} public virtual AccessGroup _idAccessGroup{get; set;}
		public string Subject{get; set;}
		public int idMethodofFormingCode_GoalSetting{get; set;} public virtual MethodofFormingCode _idMethodofFormingCode_GoalSetting{get; set;}
		public int idMethodofFormingCode_TargetIndex{get; set;} public virtual MethodofFormingCode _idMethodofFormingCode_TargetIndex{get; set;}
		public int idMethodofFormingCode_Activity{get; set;} public virtual MethodofFormingCode _idMethodofFormingCode_Activity{get; set;}
	
		public PublicLegalFormation()
        {
             
		}
	}
}

