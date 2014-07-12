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
	/// Ресурсное обеспечение реализации государственной программы
	/// </summary>
	public partial class ResourceMaintenanceOfTheStateProgram : ReportEntity      
	{
	
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
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Государственная программа/Подпрограмма государственной программы
		/// </summary>
		public int? IdProgram{get; set;}
        /// <summary>
	    /// Государственная программа/Подпрограмма государственной программы
	    /// </summary>
		public virtual Sbor.Registry.Program Program{get; set;}
		

		/// <summary>
		/// Строить отчет по утвержденным данным
		/// </summary>
		public bool? ByApproved{get; set;}

		/// <summary>
		/// Оценка дополнительной потребности в средствах
		/// </summary>
		public bool? IsRatingAdditionalNeeds{get; set;}

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
		/// Повторять заголовки  таблиц на каждой  странице
		/// </summary>
		public bool? RepeatTableHeader{get; set;}

		/// <summary>
		/// Выводить мероприятия
		/// </summary>
		public bool? ShowActivities{get; set;}

		/// <summary>
		/// Строить отчет в разрезе источников
		/// </summary>
		public bool? BySource{get; set;}

		/// <summary>
		/// Источник данных для вывода ресурсного обеспечения в отчет
		/// </summary>
		public byte? IdSourcesDataReports{get; set;}
                            /// <summary>
                            /// Источник данных для вывода ресурсного обеспечения в отчет
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.Reports.DbEnums.SourcesDataReports? SourcesDataReports {
								get { return (Sbor.Reports.DbEnums.SourcesDataReports?)this.IdSourcesDataReports; } 
								set { this.IdSourcesDataReports = (byte?) value; }
							}

		/// <summary>
		/// Выводить мероприятия не имеющие финансирования
		/// </summary>
		public bool? HasNoFunds{get; set;}

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
		/// Имя профиля
		/// </summary>
		public string ReportProfileCaption{get; set;}

	
private ICollection<Sbor.Reference.FinanceSource> _mlFinanceSource; 
        /// <summary>
        /// Ресурсное обеспечение реализации государственной программы
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.FinanceSource> FinanceSource 
		{
			get{ return _mlFinanceSource ?? (_mlFinanceSource = new List<Sbor.Reference.FinanceSource>()); } 
			set{ _mlFinanceSource = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ResourceMaintenanceOfTheStateProgram()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265420; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265420; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Ресурсное обеспечение реализации государственной программы"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265420);
			}
		}


	}
}