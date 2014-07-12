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
	/// Тестовый отчет по аудиту
	/// </summary>
	public partial class TestAuditReport : ReportEntity      
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
		/// От
		/// </summary>
		private DateTime? _DateFrom; 
        /// <summary>
	    /// От
	    /// </summary>
		public  DateTime? DateFrom 
		{
			get{ return _DateFrom != null ? ((DateTime)_DateFrom).Date : (DateTime?)null; }
			set{ _DateFrom = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// До
		/// </summary>
		private DateTime? _DateTo; 
        /// <summary>
	    /// До
	    /// </summary>
		public  DateTime? DateTo 
		{
			get{ return _DateTo != null ? ((DateTime)_DateTo).Date : (DateTime?)null; }
			set{ _DateTo = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Пользователь
		/// </summary>
		public int? IdUser{get; set;}
        /// <summary>
	    /// Пользователь
	    /// </summary>
		public virtual BaseApp.Reference.User User{get; set;}
		

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
		/// Конструктор по-умолчанию
		/// </summary>
		public TestAuditReport()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1811939279; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1811939279; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Тестовый отчет по аудиту"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1811939279);
			}
		}


	}
}