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
	/// Реестр целей (задач)
	/// </summary>
	public partial class RegistryGoal : ReportEntity      
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
		/// СБП
		/// </summary>
		public int? IdSBP{get; set;}
        /// <summary>
	    /// СБП
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Начало периода
		/// </summary>
		private DateTime? _DateStart; 
        /// <summary>
	    /// Начало периода
	    /// </summary>
		public  DateTime? DateStart 
		{
			get{ return _DateStart != null ? ((DateTime)_DateStart).Date : (DateTime?)null; }
			set{ _DateStart = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Конец периода
		/// </summary>
		private DateTime? _DateEnd; 
        /// <summary>
	    /// Конец периода
	    /// </summary>
		public  DateTime? DateEnd 
		{
			get{ return _DateEnd != null ? ((DateTime)_DateEnd).Date : (DateTime?)null; }
			set{ _DateEnd = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Выводить цели, действующие в период
		/// </summary>
		public bool? OutputGoalOperatingPeriod{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int? IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

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
		public bool? ConstructReportApprovedData{get; set;}

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
		/// Выводить в отчет код элемента СЦ справочника «Система целеполагания»
		/// </summary>
		public bool? DisplayReportCodeS{get; set;}

		/// <summary>
		/// Выводить в отчет показатели цели (задачи)
		/// </summary>
		public bool? DisplayReportDataGoal{get; set;}

		/// <summary>
		/// Выводить выбранные значения параметров в отчет
		/// </summary>
		public bool? DispleySelectedParameterValues{get; set;}

		/// <summary>
		/// Повторять заголовки  таблиц на каждой  странице
		/// </summary>
		public bool? RepeatTableHeader{get; set;}

		/// <summary>
		/// Формировать значения с расшифровкой
		/// </summary>
		public bool? GenerateValuesWithDetails{get; set;}

		/// <summary>
		/// Бюджет
		/// </summary>
		public int? IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Выводить в отчет ресурсное обеспечение целей
		/// </summary>
		public bool? DisplayResourceProvision{get; set;}

		/// <summary>
		/// Источник данных для вывода ресурсного обеспечения в отчет
		/// </summary>
		public byte? DisplayResourceSupport{get; set;}
                            /// <summary>
                            /// Источник данных для вывода ресурсного обеспечения в отчет
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.Reports.DbEnums.SourcesDataReports? SplayResourceSupport {
								get { return (Sbor.Reports.DbEnums.SourcesDataReports?)this.DisplayResourceSupport; } 
								set { this.DisplayResourceSupport = (byte?) value; }
							}

		/// <summary>
		/// Выводить в отчет значения показателей за период планирования бюджета
		/// </summary>
		public bool? DisplayDataBudgetPeriod{get; set;}

	
private ICollection<Sbor.Reference.ElementTypeSystemGoal> _mlElementTypeSystemGoal; 
        /// <summary>
        /// Реестр целей
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.ElementTypeSystemGoal> ElementTypeSystemGoal 
		{
			get{ return _mlElementTypeSystemGoal ?? (_mlElementTypeSystemGoal = new List<Sbor.Reference.ElementTypeSystemGoal>()); } 
			set{ _mlElementTypeSystemGoal = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public RegistryGoal()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1275068394; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1275068394; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Реестр целей (задач)"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1275068394);
			}
		}


	}
}