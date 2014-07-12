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



namespace Sbor.Reports.Report
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Межбюджетные трансферты
	/// </summary>
	public partial class InterBudgetaryTransfers : ReportEntity      
	{
	
		/// <summary>
		/// Имя профиля
		/// </summary>
		public string ReportProfileCaption{get; set;}

		/// <summary>
		/// Тип профиля
		/// </summary>
		public byte? IdReportProfileType{get; set;}
                            /// <summary>
                            /// Тип профиля
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.ReportProfileType? ReportProfileType {
								get { return (BaseApp.DbEnums.ReportProfileType?)this.IdReportProfileType; } 
								set { this.IdReportProfileType = (byte?) value; }
							}

		/// <summary>
		/// Автор профиля
		/// </summary>
		public int? IdReportProfileUser{get; set;}
        /// <summary>
	    /// Автор профиля
	    /// </summary>
		public virtual BaseApp.Reference.User ReportProfileUser{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Временный экземпляр
		/// </summary>
		public bool? IsTemporary{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int? IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Бюджет
		/// </summary>
		public int? IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Версия
		/// </summary>
		public int? IdVersion{get; set;}
        /// <summary>
	    /// Версия
	    /// </summary>
		public virtual BaseApp.Reference.Version Version{get; set;}
		

		/// <summary>
		/// Строить отчет по утвержденным данным
		/// </summary>
		public bool? ByApproved{get; set;}

		/// <summary>
		/// Дата отчета
		/// </summary>
		private DateTime? _DateReport; 
        /// <summary>
	    /// Дата отчета
	    /// </summary>
		public  DateTime? DateReport 
		{
			get{ return _DateReport != null ? ((DateTime)_DateReport).Date : (DateTime?)null; }
			set{ _DateReport = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Единица измерения
		/// </summary>
		public int? IdUnitDimension{get; set;}
        /// <summary>
	    /// Единица измерения
	    /// </summary>
		public virtual Sbor.Reference.UnitDimension UnitDimension{get; set;}
		

		/// <summary>
		/// Источник данных для отчета
		/// </summary>
		public byte? IdSourcesDataReports{get; set;}
                            /// <summary>
                            /// Источник данных для отчета
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.Reports.DbEnums.SourcesDataReports? SourcesDataReports {
								get { return (Sbor.Reports.DbEnums.SourcesDataReports?)this.IdSourcesDataReports; } 
								set { this.IdSourcesDataReports = (byte?) value; }
							}

		/// <summary>
		/// Повторять заголовки таблиц на каждой странице
		/// </summary>
		public bool? RepeatTableHeader{get; set;}

		/// <summary>
		/// Наименование отчета
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// `'' + ({BudgetYear}) + ' год'`
		/// </summary>
		public bool? IsShowYear{get; set;}

		/// <summary>
		/// `'' + ({BudgetYear} + 1) + ' год'`
		/// </summary>
		public bool? IsShowYear1{get; set;}

		/// <summary>
		/// `'' + ({BudgetYear} + 2) + ' год'`
		/// </summary>
		public bool? IsShowYear2{get; set;}

		/// <summary>
		/// Выводить нераспределенный резерв
		/// </summary>
		public bool? IsShowUnallocatedReserve{get; set;}

		/// <summary>
		/// Настраиваемые колонки
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_CustomizableColumns> _tpCustomizableColumns; 
        /// <summary>
        /// Настраиваемые колонки
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_CustomizableColumns> CustomizableColumns 
		{
			get{ return _tpCustomizableColumns ?? (_tpCustomizableColumns = new List<Sbor.Reports.Tablepart.InterBudgetaryTransfers_CustomizableColumns>()); } 
			set{ _tpCustomizableColumns = value; }
		}

		/// <summary>
		/// Правила фильтрации колонок
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK> _tpRuleFilterKBKs; 
        /// <summary>
        /// Правила фильтрации колонок
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK> RuleFilterKBKs 
		{
			get{ return _tpRuleFilterKBKs ?? (_tpRuleFilterKBKs = new List<Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK>()); } 
			set{ _tpRuleFilterKBKs = value; }
		}

		/// <summary>
		/// Шапка отчета
		/// </summary>
		public string Header{get; set;}

	
private ICollection<BaseApp.Reference.BudgetLevel> _mlBudgetLevel; 
        /// <summary>
        /// Межбюджетные трансферты
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.BudgetLevel> BudgetLevel 
		{
			get{ return _mlBudgetLevel ?? (_mlBudgetLevel = new List<BaseApp.Reference.BudgetLevel>()); } 
			set{ _mlBudgetLevel = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public InterBudgetaryTransfers()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503504; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503504; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Межбюджетные трансферты"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503504);
			}
		}


	}
}