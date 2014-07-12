using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;

namespace Sbor.GeneratedCode
{

	public class TypeRegulatoryAct : ReferenceEntity 
	{
	
		public Int32 id{get; set;}
		public int idAccessGroup{get; set;} public virtual AccessGroup _idAccessGroup{get; set;}
		public string Caption{get; set;}
	
		public TypeRegulatoryAct()
        {
             
		}
	}
}

