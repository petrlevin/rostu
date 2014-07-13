using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.DbEnums;

namespace BaseApp.GeneratedCode
{

	public class UserRole : MultilinkEntity 
	{
	
		public int idUser{get; set;} public virtual User _idUser{get; set;}
		public int idRole{get; set;} public virtual Role _idRole{get; set;}
	
		public UserRole()
        {
             
		}
	}
}

