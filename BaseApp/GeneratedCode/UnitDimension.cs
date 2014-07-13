using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.DbEnums;

namespace BaseApp.GeneratedCode
{

	public class UnitDimension : ReferenceEntity 
	{
	
		public Int32 id{get; set;}
		public string OKEICode{get; set;}
		public string Caption{get; set;}
		public string Symbol{get; set;}
		public string InternationalAbbreviation{get; set;}
	
		public UnitDimension()
        {
             
		}
	}
}

