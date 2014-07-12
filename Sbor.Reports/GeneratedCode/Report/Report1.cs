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
	/// Отчет №1
	/// </summary>
	public partial class Report1 : ReportEntity      
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
		/// Текст
		/// </summary>
		public string Text{get; set;}

		/// <summary>
		/// СБП
		/// </summary>
		public int? SBP{get; set;}
        /// <summary>
	    /// СБП
	    /// </summary>
		public virtual Sbor.Reference.SBP P{get; set;}
		

		/// <summary>
		/// Булево
		/// </summary>
		public bool? Boolean{get; set;}

		/// <summary>
		/// ТЧ
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.Report1_Tp> _tpSomeTablepart; 
        /// <summary>
        /// ТЧ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.Report1_Tp> SomeTablepart 
		{
			get{ return _tpSomeTablepart ?? (_tpSomeTablepart = new List<Sbor.Reports.Tablepart.Report1_Tp>()); } 
			set{ _tpSomeTablepart = value; }
		}

		/// <summary>
		/// Дата
		/// </summary>
		private DateTime? _Date; 
        /// <summary>
	    /// Дата
	    /// </summary>
		public  DateTime? Date 
		{
			get{ return _Date != null ? ((DateTime)_Date).Date : (DateTime?)null; }
			set{ _Date = value != null ? ((DateTime)value).Date : value; }
		}

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
		

	
private ICollection<Sbor.Reference.FinanceSource> _mlFinanceSource; 
        /// <summary>
        /// idReport1
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
		public Report1()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1811939298; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1811939298; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Отчет №1"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1811939298);
			}
		}


	}
}