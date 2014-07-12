using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;

namespace Sbor.GeneratedCode
{

	public class RegulatoryAct : ReferenceEntity 
	{
	
		public Int32 id{get; set;}
		public int idPublicLegalFormation{get; set;} public virtual PublicLegalFormation _idPublicLegalFormation{get; set;}
		public int idBudgetLevel{get; set;} public virtual BudgetLevel _idBudgetLevel{get; set;}
		public int idTypeRegulatoryAct{get; set;} public virtual TypeRegulatoryAct _idTypeRegulatoryAct{get; set;}
		public string Number{get; set;}
		public DateTime Date{get; set;}
		public DateTime DateStart{get; set;}
		public DateTime DateEnd{get; set;}
		public string AuthorityRegulatoryAct{get; set;}
		public string Caption{get; set;}
	
		public RegulatoryAct()
        {
             
		}
	}
}

