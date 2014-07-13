using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.DbEnums;

namespace BaseApp.GeneratedCode
{

	public class User : ReferenceEntity 
	{
	
		public Int32 id{get; set;}
		public string Caption{get; set;}
		public string Name{get; set;}
		public string Email{get; set;}
		public string Password{get; set;}
		public DateTime DateofLastEntry{get; set;}
		public string Department{get; set;}
		public string Site{get; set;}
		public string IISAddress{get; set;}
		public string Telephone{get; set;}
		public int idAccessGroup{get; set;} public virtual AccessGroup _idAccessGroup{get; set;}
	
		public User()
        {
             
		}
	}
}

