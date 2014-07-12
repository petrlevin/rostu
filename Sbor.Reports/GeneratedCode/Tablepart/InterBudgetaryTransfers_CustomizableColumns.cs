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
	/// Настраиваемые колонки
	/// </summary>
	public partial class InterBudgetaryTransfers_CustomizableColumns : TablePartEntity    , IHierarhy  
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
		public virtual Sbor.Reports.Report.InterBudgetaryTransfers Owner{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Результирующая колонка
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Результирующая колонка
	    /// </summary>
		public virtual Sbor.Reports.Tablepart.InterBudgetaryTransfers_CustomizableColumns Parent{get; set;}
		private ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_CustomizableColumns> _idParent; 
        /// <summary>
        /// Результирующая колонка
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_CustomizableColumns> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Reports.Tablepart.InterBudgetaryTransfers_CustomizableColumns>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Порядок в отчете
		/// </summary>
		public Int32 Order{get; set;}

			private ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK> _InterBudgetaryTransfers_RuleFilterKBK; 
        /// <summary>
        /// Настраиваемая колонка
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
		public InterBudgetaryTransfers_CustomizableColumns()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503502; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503502; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Настраиваемые колонки"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503502);
			}
		}


	}
}