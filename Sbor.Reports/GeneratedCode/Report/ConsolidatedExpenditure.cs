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
	/// Консолидированные расходы
	/// </summary>
	public partial class ConsolidatedExpenditure : ReportEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

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
		/// Временный экземпляр
		/// </summary>
		public bool? IsTemporary{get; set;}

		/// <summary>
		/// Публично-правовое образование
		/// </summary>
		public int? IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// Публично-правовое образование
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Год 
		/// </summary>
		public int? IdHierarchyPeriod{get; set;}
        /// <summary>
	    /// Год 
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.HierarchyPeriod HierarchyPeriod{get; set;}
		

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
		/// Наименование отчета.
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Повторять заголовки таблиц на каждой странице
		/// </summary>
		public bool? RepeatTableHeader{get; set;}

		/// <summary>
		/// Текущее Публично-правовое образование
		/// </summary>
		public int? IdCurrentPPO{get; set;}
        /// <summary>
	    /// Текущее Публично-правовое образование
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation CurrentPPO{get; set;}
		

		/// <summary>
		/// Публично-правовые образования
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_PPO> _tpPublicLegalFormations; 
        /// <summary>
        /// Публично-правовые образования
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_PPO> PublicLegalFormations 
		{
			get{ return _tpPublicLegalFormations ?? (_tpPublicLegalFormations = new List<Sbor.Reports.Tablepart.ConsolidatedExpenditure_PPO>()); } 
			set{ _tpPublicLegalFormations = value; }
		}

		/// <summary>
		/// Правила фильтрации
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter> _tpBaseFilter; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter> BaseFilter 
		{
			get{ return _tpBaseFilter ?? (_tpBaseFilter = new List<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter>()); } 
			set{ _tpBaseFilter = value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ConsolidatedExpenditure()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1610612488; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1610612488; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Консолидированные расходы"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1610612488);
			}
		}


	}
}