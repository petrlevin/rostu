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



namespace Sbor.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Примененные правила индексации
	/// </summary>
	public partial class BalancingIFDB_RuleIndex : TablePartEntity      
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
		public virtual Sbor.Tool.BalancingIFDB Owner{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// `'Индекс ' + ({BudgetYear})`
		/// </summary>
		public decimal? IndexOFG{get; set;}

		/// <summary>
		/// `'Индекс ' + ({BudgetYear} + 1)`
		/// </summary>
		public decimal? IndexPFG1{get; set;}

		/// <summary>
		/// `'Индекс ' + ({BudgetYear} + 2)`
		/// </summary>
		public decimal? IndexPFG2{get; set;}

		/// <summary>
		/// Применено
		/// </summary>
		public bool IsApplied{get; set;}

		/// <summary>
		/// Количество обработанных строк 
		/// </summary>
		public Int32? ChangeCount{get; set;}

		/// <summary>
		/// Номер изменния по порядку
		/// </summary>
		public Int32? ChangeNumber{get; set;}

		/// <summary>
		/// Набор КБК для правила
		/// </summary>
		

		/// <summary>
		/// Включая дополнительные расходы
		/// </summary>
		public bool IsIncludeAdditionalNeed{get; set;}

			private ICollection<Sbor.Tablepart.BalancingIFDB_ChangeHistory> _BalancingIFDB_ChangeHistory; 
        /// <summary>
        /// Правило
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_ChangeHistory> BalancingIFDB_ChangeHistory 
		{
			get{ return _BalancingIFDB_ChangeHistory ?? (_BalancingIFDB_ChangeHistory = new List<Sbor.Tablepart.BalancingIFDB_ChangeHistory>()); } 
			set{ _BalancingIFDB_ChangeHistory = value; }
		}
		private ICollection<Sbor.Tablepart.BalancingIFDB_RuleFilterKBK> _BalancingIFDB_RuleFilterKBK; 
        /// <summary>
        /// Правило
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_RuleFilterKBK> BalancingIFDB_RuleFilterKBK 
		{
			get{ return _BalancingIFDB_RuleFilterKBK ?? (_BalancingIFDB_RuleFilterKBK = new List<Sbor.Tablepart.BalancingIFDB_RuleFilterKBK>()); } 
			set{ _BalancingIFDB_RuleFilterKBK = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalancingIFDB_RuleIndex()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265298; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265298; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Примененные правила индексации"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265298);
			}
		}


	}
}