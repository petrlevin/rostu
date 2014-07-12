using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;



namespace Sbor.Reports.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Типы РО
	/// </summary>
	public partial class ConsolidatedExpenditure_BaseFilter_ExpenseObligationType : TablePartEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter Owner{get; set;}
		

		/// <summary>
		/// Типы РО
		/// </summary>
		public byte? IdExpenseObligationType{get; set;}
                            /// <summary>
                            /// Типы РО
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.ExpenseObligationType? ExpenseObligationType {
								get { return (Sbor.DbEnums.ExpenseObligationType?)this.IdExpenseObligationType; } 
								set { this.IdExpenseObligationType = (byte?) value; }
							}

			private ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter> _ConsolidatedExpenditure_BaseFilter; 
        /// <summary>
        /// Типы РО
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter> ConsolidatedExpenditure_BaseFilter 
		{
			get{ return _ConsolidatedExpenditure_BaseFilter ?? (_ConsolidatedExpenditure_BaseFilter = new List<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter>()); } 
			set{ _ConsolidatedExpenditure_BaseFilter = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ConsolidatedExpenditure_BaseFilter_ExpenseObligationType()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1610612470; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1610612470; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Типы РО"; }
		}

		

		

		/// <summary>
		/// Регистрация идентфикатора сущности
		/// </summary>
		public class EntityIdRegistrator:IBeforeAplicationStart
		{
			/// <summary>
			/// Зарегистрировать
			/// </summary>
			public void Execute()
			{
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1610612470);
			}
		}


	}
}