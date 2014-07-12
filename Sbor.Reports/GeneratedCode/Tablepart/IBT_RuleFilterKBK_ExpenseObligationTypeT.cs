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
	/// Межбюджетные трансферты ТЧ Правила фильтрации колонок - Типы РО (ТЧ)
	/// </summary>
	public partial class IBT_RuleFilterKBK_ExpenseObligationTypeT : TablePartEntity      
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
		public virtual Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK Owner{get; set;}
		

		/// <summary>
		/// Типы РО
		/// </summary>
		public byte IdExpenseObligationType{get; set;}
                            /// <summary>
                            /// Типы РО
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.ExpenseObligationType ExpenseObligationType {
								get { return (Sbor.DbEnums.ExpenseObligationType)this.IdExpenseObligationType; } 
								set { this.IdExpenseObligationType = (byte) value; }
							}

			private ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK> _InterBudgetaryTransfers_RuleFilterKBK; 
        /// <summary>
        /// Типы РО
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK> InterBudgetaryTransfers_RuleFilterKBK 
		{
			get{ return _InterBudgetaryTransfers_RuleFilterKBK ?? (_InterBudgetaryTransfers_RuleFilterKBK = new List<Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK>()); } 
			set{ _InterBudgetaryTransfers_RuleFilterKBK = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public IBT_RuleFilterKBK_ExpenseObligationTypeT()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503485; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503485; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Межбюджетные трансферты ТЧ Правила фильтрации колонок - Типы РО (ТЧ)"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503485);
			}
		}


	}
}