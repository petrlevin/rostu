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
	/// КВСР/КАДБ/КАИФ
	/// </summary>
	public partial class KVSR : ReferenceEntity, IHasRefStatus , IVersioning     
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Код
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Description{get; set;}

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
		public virtual Sbor.Reference.KVSR Root{get; set;}
		private ICollection<Sbor.Reference.KVSR> _idRoot; 
        /// <summary>
        /// Cсылка на корень
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.KVSR> ChildrenByidRoot 
		{
			get{ return _idRoot ?? (_idRoot = new List<Sbor.Reference.KVSR>()); } 
			set{ _idRoot = value; }
		}

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
		/// Конструктор по-умолчанию
		/// </summary>
		public KVSR()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265885; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265885; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "КВСР/КАДБ/КАИФ"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265885);
			}
		}


	}
}