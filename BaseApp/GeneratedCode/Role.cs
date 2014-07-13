using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.DbEnums;

namespace BaseApp.GeneratedCode
{

	public class Role : ReferenceEntity 
	{
	
		public Int32 id{get; set;}
		public string Caption{get; set;}
		public string Name{get; set;}
		public string Description{get; set;}
	
		public Role()
        {
             
		}
	}
}

