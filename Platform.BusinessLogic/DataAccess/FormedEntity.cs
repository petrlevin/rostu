using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public class FormedEntity:Entity
    {
        public IForm Form { get; set; }

		public IEnumerable<IFormElement> Elements
		{
			get { return Form.FormElements; }
			set { Form.FormElements = value; }
		}    

    }

}
