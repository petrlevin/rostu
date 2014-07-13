using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;

namespace Platform.BusinessLogic.Reference
{

	/// <summary>
	/// Операции
	/// </summary>
	public partial class Operation :IOperation
	{
	    private const string BeforeNonAtomicOperationPrefix = "Before";
       
        /// <summary>
        /// Имя метода, вызываемого при старте неатомарной операции
        /// </summary>
        public string BeforeOperationName {
            get { return BeforeNonAtomicOperationPrefix + Name; } 
        }

	}
}