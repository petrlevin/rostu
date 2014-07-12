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

using Platform.PrimaryEntities.DbEnums;using Platform.PrimaryEntities.Common.DbEnums;

namespace Sbor.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// КОСГУ
	/// </summary>
	public partial class KOSGU : ReferenceEntity, IHasRefStatus , IVersioning   , IHierarhy  
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Статус
		/// </summary>
		public byte IdRefStatus{get; set;}
                            /// <summary>
                            /// Статус
                            /// </summary>
							[NotMapped] 
                            public virtual RefStatus RefStatus {
								get { return (RefStatus)this.IdRefStatus; } 
								set { this.IdRefStatus = (byte) value; }
							}

		/// <summary>
		/// Код
		/// </summary>
		public string Code{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Вышестоящий
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящий
	    /// </summary>
		public virtual Sbor.Reference.KOSGU Parent{get; set;}
		private ICollection<Sbor.Reference.KOSGU> _idParent; 
        /// <summary>
        /// Вышестоящий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KOSGU> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Reference.KOSGU>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Дата начала действия
		/// </summary>
		private DateTime? _ValidityFrom; 
        /// <summary>
	    /// Дата начала действия
	    /// </summary>
		public  DateTime? ValidityFrom 
		{
			get{ return _ValidityFrom != null ? ((DateTime)_ValidityFrom).Date : (DateTime?)null; }
			set{ _ValidityFrom = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Дата окончания действия
		/// </summary>
		private DateTime? _ValidityTo; 
        /// <summary>
	    /// Дата окончания действия
	    /// </summary>
		public  DateTime? ValidityTo 
		{
			get{ return _ValidityTo != null ? ((DateTime)_ValidityTo).Date : (DateTime?)null; }
			set{ _ValidityTo = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Cсылка на корень
		/// </summary>
		public int? IdRoot{get; set;}
        /// <summary>
	    /// Cсылка на корень
	    /// </summary>
		public virtual Sbor.Reference.KOSGU Root{get; set;}
		private ICollection<Sbor.Reference.KOSGU> _idRoot; 
        /// <summary>
        /// Cсылка на корень
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KOSGU> ChildrenByidRoot 
		{
			get{ return _idRoot ?? (_idRoot = new List<Sbor.Reference.KOSGU>()); } 
			set{ _idRoot = value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public KOSGU()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503739; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503739; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "КОСГУ"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503739);
			}
		}


	}
}