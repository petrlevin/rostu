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
	/// TestDocument2
	/// </summary>
	public partial class TestDocument2 : DocumentEntity<TestDocument2>    , IHierarhy  
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
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Родитель
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Родитель
	    /// </summary>
		public virtual Sbor.Document.TestDocument2 Parent{get; set;}
		private ICollection<Sbor.Document.TestDocument2> _idParent; 
        /// <summary>
        /// Родитель
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.TestDocument2> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Document.TestDocument2>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// SSS
		/// </summary>
		public System.Int64 SSS{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TestDocument2()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1744830423; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1744830423; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "TestDocument2"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830423);
			}
		}


	}
}