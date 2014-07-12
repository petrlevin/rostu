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
	/// Структура расходов бюджета
	/// </summary>
	public partial class BudgetExpenseStructure : ReportEntity      
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
		/// Шапка отчета
		/// </summary>
		public string ReportCap{get; set;}

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
		/// Наименование отчета
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Строить отчет по утвержденным данным
		/// </summary>
		public bool? IsApprovedOnly{get; set;}

		/// <summary>
		/// Дата отчета
		/// </summary>
		private DateTime? _ReportDate; 
        /// <summary>
	    /// Дата отчета
	    /// </summary>
		public  DateTime? ReportDate 
		{
			get{ return _ReportDate != null ? ((DateTime)_ReportDate).Date : (DateTime?)null; }
			set{ _ReportDate = value != null ? ((DateTime)value).Date : value; }
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
		/// Варианты периодов
		/// </summary>
		public byte? IdPeriodOption{get; set;}
                            /// <summary>
                            /// Варианты периодов
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.PeriodOption? PeriodOption {
								get { return (Sbor.DbEnums.PeriodOption?)this.IdPeriodOption; } 
								set { this.IdPeriodOption = (byte?) value; }
							}

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
		/// Перечень выводимых колонок
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_Columns> _tpColumns; 
        /// <summary>
        /// Перечень выводимых колонок
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_Columns> Columns 
		{
			get{ return _tpColumns ?? (_tpColumns = new List<Sbor.Reports.Tablepart.BudgetExpenseStructure_Columns>()); } 
			set{ _tpColumns = value; }
		}

		/// <summary>
		/// Повторять заголовки таблиц на каждой странице
		/// </summary>
		public bool? RepeatTableHeader{get; set;}

		/// <summary>
		/// Правила фильтрации
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_BaseFilter> _tpBaseFilter; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_BaseFilter> BaseFilter 
		{
			get{ return _tpBaseFilter ?? (_tpBaseFilter = new List<Sbor.Reports.Tablepart.BudgetExpenseStructure_BaseFilter>()); } 
			set{ _tpBaseFilter = value; }
		}

		/// <summary>
		/// Наименование колонок
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomColumn> _tpCustomColumns; 
        /// <summary>
        /// Наименование колонок
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomColumn> CustomColumns 
		{
			get{ return _tpCustomColumns ?? (_tpCustomColumns = new List<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomColumn>()); } 
			set{ _tpCustomColumns = value; }
		}

		/// <summary>
		/// Правила фильтрации колонок
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomFilter> _tpCustomFilters; 
        /// <summary>
        /// Правила фильтрации колонок
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomFilter> CustomFilters 
		{
			get{ return _tpCustomFilters ?? (_tpCustomFilters = new List<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomFilter>()); } 
			set{ _tpCustomFilters = value; }
		}

		/// <summary>
		/// Выводить цели
		/// </summary>
		public bool? ShowGoals{get; set;}

		/// <summary>
		/// Выводить программы
		/// </summary>
		public bool? ShowProgram{get; set;}

		/// <summary>
		/// Выводить мероприятия
		/// </summary>
		public bool? ShowActivities{get; set;}

		/// <summary>
		/// Уровень программ
		/// </summary>
		public int? IdDocType{get; set;}
        /// <summary>
	    /// Уровень программ
	    /// </summary>
		public virtual Sbor.Reference.DocType DocType{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BudgetExpenseStructure()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959403; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959403; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Структура расходов бюджета"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959403);
			}
		}


	}
}