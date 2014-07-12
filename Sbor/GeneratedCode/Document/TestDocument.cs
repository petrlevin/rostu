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
	/// TestDocument
	/// </summary>
	public partial class TestDocument : DocumentEntity<TestDocument>      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

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
		/// idDocStatus
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Zumma
		/// </summary>
		public decimal? Zumma{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int? IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormationModule PublicLegalFormation{get; set;}
		

		/// <summary>
		/// tpSomeTablePart
		/// </summary>
		private ICollection<Sbor.Tablepart.TestDocumentTP> _tpSomeTablePart; 
        /// <summary>
        /// tpSomeTablePart
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.TestDocumentTP> SomeTablePart 
		{
			get{ return _tpSomeTablePart ?? (_tpSomeTablePart = new List<Sbor.Tablepart.TestDocumentTP>()); } 
			set{ _tpSomeTablePart = value; }
		}

		/// <summary>
		/// Другая табличная часть
		/// </summary>
		private ICollection<Sbor.Tablepart.TestDocumentTP2> _tpOtherTablePart; 
        /// <summary>
        /// Другая табличная часть
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.TestDocumentTP2> OtherTablePart 
		{
			get{ return _tpOtherTablePart ?? (_tpOtherTablePart = new List<Sbor.Tablepart.TestDocumentTP2>()); } 
			set{ _tpOtherTablePart = value; }
		}

		/// <summary>
		/// SBP
		/// </summary>
		public int? IdSbp{get; set;}
        /// <summary>
	    /// SBP
	    /// </summary>
		public virtual Sbor.Reference.SBP Sbp{get; set;}
		

		/// <summary>
		/// Другой
		/// </summary>
		public int? IdOther{get; set;}
        /// <summary>
	    /// Другой
	    /// </summary>
		public virtual Sbor.Document.TestDocument2 Other{get; set;}
		

		/// <summary>
		/// Этот
		/// </summary>
		public int? IdThis{get; set;}
        /// <summary>
	    /// Этот
	    /// </summary>
		public virtual Sbor.Document.TestDocument This{get; set;}
		private ICollection<Sbor.Document.TestDocument> _idThis; 
        /// <summary>
        /// Этот
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.TestDocument> ChildrenByidThis 
		{
			get{ return _idThis ?? (_idThis = new List<Sbor.Document.TestDocument>()); } 
			set{ _idThis = value; }
		}

		/// <summary>
		/// AAA
		/// </summary>
		public int? AAA{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TestDocument()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1744830429; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1744830429; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "TestDocument"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830429);
			}
		}


	}
}