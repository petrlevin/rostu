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



namespace Sbor.Document
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Реестр мероприятий
	/// </summary>
	public partial class RegisterActivity : DocumentEntity<RegisterActivity>    , IHierarhy  
	{
	
		/// <summary>
		/// Показатели качества 
		/// </summary>
		private ICollection<Sbor.Tablepart.RegisterActivity_IndicatorActivity> _tpIndicatorActivity; 
        /// <summary>
        /// Показатели качества 
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.RegisterActivity_IndicatorActivity> IndicatorActivity 
		{
			get{ return _tpIndicatorActivity ?? (_tpIndicatorActivity = new List<Sbor.Tablepart.RegisterActivity_IndicatorActivity>()); } 
			set{ _tpIndicatorActivity = value; }
		}

		/// <summary>
		/// Исполнители 
		/// </summary>
		private ICollection<Sbor.Tablepart.RegisterActivity_Performers> _tpPerformers; 
        /// <summary>
        /// Исполнители 
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.RegisterActivity_Performers> Performers 
		{
			get{ return _tpPerformers ?? (_tpPerformers = new List<Sbor.Tablepart.RegisterActivity_Performers>()); } 
			set{ _tpPerformers = value; }
		}

		/// <summary>
		/// Заголовок
		/// </summary>
		public string Header{get; set;}

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Тип документа
		/// </summary>
		public int IdDocType{get; set;}
        /// <summary>
	    /// Тип документа
	    /// </summary>
		public virtual Sbor.Reference.DocType DocType{get; set;}
		

		/// <summary>
		/// Номер
		/// </summary>
		public override string Number{get; set;}

		/// <summary>
		/// Дата
		/// </summary>
		private DateTime _Date; 
        /// <summary>
	    /// Дата
	    /// </summary>
		public override  DateTime Date 
		{
			get{ return _Date.Date; }
			set{ _Date = value.Date; }
		}

		/// <summary>
		/// Утвержден
		/// </summary>
		public bool IsApproved{get; set;}

		/// <summary>
		/// Системная дата утверждения
		/// </summary>
		private DateTime? _DateCommit; 
        /// <summary>
	    /// Системная дата утверждения
	    /// </summary>
		public  DateTime? DateCommit 
		{
			get{ return _DateCommit != null ? ((DateTime)_DateCommit).Date : (DateTime?)null; }
			set{ _DateCommit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Ведомство
		/// </summary>
		public int? IdSBP{get; set;}
        /// <summary>
	    /// Ведомство
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

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
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Предыдущая редакция
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Предыдущая редакция
	    /// </summary>
		public virtual Sbor.Document.RegisterActivity Parent{get; set;}
		private ICollection<Sbor.Document.RegisterActivity> _idParent; 
        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.RegisterActivity> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.RegisterActivity>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Причина отказа
		/// </summary>
		public string ReasonCancel{get; set;}

		/// <summary>
		/// Мероприятия
		/// </summary>
		private ICollection<Sbor.Tablepart.RegisterActivity_Activity> _tpActivity; 
        /// <summary>
        /// Мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.RegisterActivity_Activity> Activity 
		{
			get{ return _tpActivity ?? (_tpActivity = new List<Sbor.Tablepart.RegisterActivity_Activity>()); } 
			set{ _tpActivity = value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public RegisterActivity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503828; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503828; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Реестр мероприятий"; }
		}

		
        /// <summary>
	    /// Наименование типа документа
	    /// </summary>
        public override string DocumentCaption 
        {
            get
            {
                if (DocType==null)
                    return base.DocumentCaption;
                else
                    return DocType.Caption;
                    
            }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503828);
			}
		}


	}
}