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
	/// Анализ объема бюджетных ассигнований в разрезе прямых и косвенных расходов
	/// </summary>
	public partial class AnalysisBAofDirectAndIndirectCost : ReportEntity      
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
		/// Повторять заголовки  таблиц на каждой  странице
		/// </summary>
		public bool? RepeatTableHeader{get; set;}

		/// <summary>
		/// Единицы измерения
		/// </summary>
		public int? IdUnitDimension{get; set;}
        /// <summary>
	    /// Единицы измерения
	    /// </summary>
		public virtual Sbor.Reference.UnitDimension UnitDimension{get; set;}
		

		/// <summary>
		/// Группировать по мероприятиям
		/// </summary>
		public bool? ByActivity{get; set;}

		/// <summary>
		/// Группировать по подведомственным учреждениям
		/// </summary>
		public bool? BySBP{get; set;}

	
private ICollection<Sbor.Reference.SBP> _mlSBPs; 
        /// <summary>
        /// Анализ объема бюджетных ассигнований в разрезе прямых и косвенных расходов
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.SBP> SBPs 
		{
			get{ return _mlSBPs ?? (_mlSBPs = new List<Sbor.Reference.SBP>()); } 
			set{ _mlSBPs = value; }
		}
			
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public AnalysisBAofDirectAndIndirectCost()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013264951; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013264951; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Анализ объема бюджетных ассигнований в разрезе прямых и косвенных расходов"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013264951);
			}
		}


	}
}