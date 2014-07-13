using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.DbEnums;

namespace BaseApp.GeneratedCode
{

	public class Budget : ReferenceEntity 
	{
	
		public Int32 id{get; set;}
		public int idPublicLegalFormation{get; set;} public virtual PublicLegalFormation _idPublicLegalFormation{get; set;}
		public DateTime Year{get; set;}
		public string Caption{get; set;}
	
		public Budget()
        {
             
		}
	}
}

