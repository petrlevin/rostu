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
	/// Обоснование бюджетных ассигнований
	/// </summary>
	public partial class JustificationOfBudget : ReportEntity      
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
		/// Строить отчет по утвержденным данным
		/// </summary>
		public bool? ByApproved{get; set;}

		/// <summary>
		/// Версия
		/// </summary>
		public int? IdVersion{get; set;}
        /// <summary>
	    /// Версия
	    /// </summary>
		public virtual BaseApp.Reference.Version Version{get; set;}
		

		/// <summary>
		/// Повторять заголовки таблиц на каждой странице
		/// </summary>
		public bool? RepeatTableHeader{get; set;}

		/// <summary>
		/// Оценка дополнительной потребности
		/// </summary>
		public bool? HasAdditionalNeed{get; set;}

		/// <summary>
		/// Главный распорядитель БС
		/// </summary>
		public int? IdSBP{get; set;}
        /// <summary>
	    /// Главный распорядитель БС
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

	
private ICollection<BaseApp.Reference.ListRemovedFields> _mlListRemovedField; 
        /// <summary>
        /// Обоснование бюджетных ассигнований
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.ListRemovedFields> ListRemovedField 
		{
			get{ return _mlListRemovedField ?? (_mlListRemovedField = new List<BaseApp.Reference.ListRemovedFields>()); } 
			set{ _mlListRemovedField = value; }
		}
			private ICollection<Sbor.Reference.RZPR> _mlRZPR; 
        /// <summary>
        /// Обоснование бюджетных ассигнований
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.RZPR> RZPR 
		{
			get{ return _mlRZPR ?? (_mlRZPR = new List<Sbor.Reference.RZPR>()); } 
			set{ _mlRZPR = value; }
		}
			private ICollection<Sbor.Reference.KCSR> _mlKCSR; 
        /// <summary>
        /// Обоснование бюджетных ассигнований
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KCSR> KCSR 
		{
			get{ return _mlKCSR ?? (_mlKCSR = new List<Sbor.Reference.KCSR>()); } 
			set{ _mlKCSR = value; }
		}
			private ICollection<Sbor.Reference.KVR> _mlKVR; 
        /// <summary>
        /// Обоснование бюджетных ассигнований
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KVR> KVR 
		{
			get{ return _mlKVR ?? (_mlKVR = new List<Sbor.Reference.KVR>()); } 
			set{ _mlKVR = value; }
		}
			private ICollection<Sbor.Reference.KOSGU> _mlKOSGU; 
        /// <summary>
        /// Обоснование бюджетных ассигнований
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KOSGU> KOSGU 
		{
			get{ return _mlKOSGU ?? (_mlKOSGU = new List<Sbor.Reference.KOSGU>()); } 
			set{ _mlKOSGU = value; }
		}
			private ICollection<Sbor.Reference.AuthorityOfExpenseObligation> _mlAuthorityOfExpenseObligation; 
        /// <summary>
        /// Обоснование бюджетных ассигнований
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.AuthorityOfExpenseObligation> AuthorityOfExpenseObligation 
		{
			get{ return _mlAuthorityOfExpenseObligation ?? (_mlAuthorityOfExpenseObligation = new List<Sbor.Reference.AuthorityOfExpenseObligation>()); } 
			set{ _mlAuthorityOfExpenseObligation = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public JustificationOfBudget()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503520; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503520; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Обоснование бюджетных ассигнований"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503520);
			}
		}


	}
}