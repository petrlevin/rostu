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



namespace Sbor.Tool
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Балансировка доходов, расходов и ИФДБ
	/// </summary>
	public partial class BalancingIFDB : ToolEntity<BalancingIFDB>      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Версия
		/// </summary>
		public int IdVersion{get; set;}
        /// <summary>
	    /// Версия
	    /// </summary>
		public virtual BaseApp.Reference.Version Version{get; set;}
		

		/// <summary>
		/// Номер
		/// </summary>
		public string Number{get; set;}

		/// <summary>
		/// Дата
		/// </summary>
		private DateTime _Date; 
        /// <summary>
	    /// Дата
	    /// </summary>
		public  DateTime Date 
		{
			get{ return _Date.Date; }
			set{ _Date = value.Date; }
		}

		/// <summary>
		/// Дата обработки
		/// </summary>
		private DateTime? _DateCommit; 
        /// <summary>
	    /// Дата обработки
	    /// </summary>
		public  DateTime? DateCommit 
		{
			get{ return _DateCommit != null ? ((DateTime)_DateCommit).Date : (DateTime?)null; }
			set{ _DateCommit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Последнее редактирование
		/// </summary>
		private DateTime? _DateLastEdit; 
        /// <summary>
	    /// Последнее редактирование
	    /// </summary>
		public  DateTime? DateLastEdit 
		{
			get{ return _DateLastEdit != null ? ((DateTime)_DateLastEdit).Date : (DateTime?)null; }
			set{ _DateLastEdit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Программы/мероприятия
		/// </summary>
		private ICollection<Sbor.Tablepart.BalancingIFDB_Program> _tpPrograms; 
        /// <summary>
        /// Программы/мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_Program> Programs 
		{
			get{ return _tpPrograms ?? (_tpPrograms = new List<Sbor.Tablepart.BalancingIFDB_Program>()); } 
			set{ _tpPrograms = value; }
		}

		/// <summary>
		/// Расходы
		/// </summary>
		private ICollection<Sbor.Tablepart.BalancingIFDB_Expense> _tpExpenses; 
        /// <summary>
        /// Расходы
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_Expense> Expenses 
		{
			get{ return _tpExpenses ?? (_tpExpenses = new List<Sbor.Tablepart.BalancingIFDB_Expense>()); } 
			set{ _tpExpenses = value; }
		}

		/// <summary>
		/// Источник данных
		/// </summary>
		public byte? IdSourcesDataTools{get; set;}
                            /// <summary>
                            /// Источник данных
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.SourcesDataTools? SourcesDataTools {
								get { return (Sbor.DbEnums.SourcesDataTools?)this.IdSourcesDataTools; } 
								set { this.IdSourcesDataTools = (byte?) value; }
							}

		/// <summary>
		/// Бюджет
		/// </summary>
		public int IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Сметные строки
		/// </summary>
		private ICollection<Sbor.Tablepart.BalancingIFDB_EstimatedLine> _tpEstimatedLines; 
        /// <summary>
        /// Сметные строки
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_EstimatedLine> EstimatedLines 
		{
			get{ return _tpEstimatedLines ?? (_tpEstimatedLines = new List<Sbor.Tablepart.BalancingIFDB_EstimatedLine>()); } 
			set{ _tpEstimatedLines = value; }
		}

		/// <summary>
		/// Правила индексации
		/// </summary>
		private ICollection<Sbor.Tablepart.BalancingIFDB_RuleIndex> _tpRuleIndexs; 
        /// <summary>
        /// Правила индексации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_RuleIndex> RuleIndexs 
		{
			get{ return _tpRuleIndexs ?? (_tpRuleIndexs = new List<Sbor.Tablepart.BalancingIFDB_RuleIndex>()); } 
			set{ _tpRuleIndexs = value; }
		}

		/// <summary>
		/// Настройка отображения КБК
		/// </summary>
		private ICollection<Sbor.Tablepart.BalancingIFDB_SetShowKBK> _tpSetShowKBKs; 
        /// <summary>
        /// Настройка отображения КБК
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_SetShowKBK> SetShowKBKs 
		{
			get{ return _tpSetShowKBKs ?? (_tpSetShowKBKs = new List<Sbor.Tablepart.BalancingIFDB_SetShowKBK>()); } 
			set{ _tpSetShowKBKs = value; }
		}

		/// <summary>
		/// История применения правил
		/// </summary>
		private ICollection<Sbor.Tablepart.BalancingIFDB_ChangeHistory> _tpChangeHistory; 
        /// <summary>
        /// История применения правил
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_ChangeHistory> ChangeHistory 
		{
			get{ return _tpChangeHistory ?? (_tpChangeHistory = new List<Sbor.Tablepart.BalancingIFDB_ChangeHistory>()); } 
			set{ _tpChangeHistory = value; }
		}

		/// <summary>
		/// Набор КБК для правила
		/// </summary>
		private ICollection<Sbor.Tablepart.BalancingIFDB_RuleFilterKBK> _tpRuleFilterKBKs; 
        /// <summary>
        /// Набор КБК для правила
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalancingIFDB_RuleFilterKBK> RuleFilterKBKs 
		{
			get{ return _tpRuleFilterKBKs ?? (_tpRuleFilterKBKs = new List<Sbor.Tablepart.BalancingIFDB_RuleFilterKBK>()); } 
			set{ _tpRuleFilterKBKs = value; }
		}

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Тип формируемого инструмента
		/// </summary>
		public byte? IdBalancingIFDBType{get; set;}
                            /// <summary>
                            /// Тип формируемого инструмента
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BalancingIFDBType? BalancingIFDBType {
								get { return (Sbor.DbEnums.BalancingIFDBType?)this.IdBalancingIFDBType; } 
								set { this.IdBalancingIFDBType = (byte?) value; }
							}

		/// <summary>
		/// Правило фильтрации
		/// </summary>
		public int IdBalanceConfig_FilterRule{get; set;}
        /// <summary>
	    /// Правило фильтрации
	    /// </summary>
		public virtual Sbor.Tablepart.BalanceConfig_FilterRule BalanceConfig_FilterRule{get; set;}
		

		/// <summary>
		/// Пользователь создавший инструмент
		/// </summary>
		public int IdUser{get; set;}
        /// <summary>
	    /// Пользователь создавший инструмент
	    /// </summary>
		public virtual BaseApp.Reference.User User{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalancingIFDB()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265407; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265407; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Балансировка доходов, расходов и ИФДБ"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265407);
			}
		}


	}
}