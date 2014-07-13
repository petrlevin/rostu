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

namespace BaseApp.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Организации
	/// </summary>
	public partial class Organization : ReferenceEntity, IHasRefStatus      
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
		/// Краткое наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Полное наименование
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// ИНН
		/// </summary>
		public string INN{get; set;}

		/// <summary>
		/// КПП
		/// </summary>
		public string KPP{get; set;}

		/// <summary>
		/// Код организации
		/// </summary>
		public string CodeOrgBud{get; set;}

		/// <summary>
		/// ОКАТО
		/// </summary>
		public int? IdOKATO{get; set;}
        /// <summary>
	    /// ОКАТО
	    /// </summary>
		public virtual BaseApp.Reference.OKATO OKATO{get; set;}
		

		/// <summary>
		/// Почтовый адрес
		/// </summary>
		public string PostAdress{get; set;}

		/// <summary>
		/// Юридический адрес
		/// </summary>
		public string LegalAddress{get; set;}

		/// <summary>
		/// Ответственные лица
		/// </summary>
		private ICollection<BaseApp.Reference.ResponsiblePerson> _tpResponsiblePerson; 
        /// <summary>
        /// Ответственные лица
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.ResponsiblePerson> ResponsiblePerson 
		{
			get{ return _tpResponsiblePerson ?? (_tpResponsiblePerson = new List<BaseApp.Reference.ResponsiblePerson>()); } 
			set{ _tpResponsiblePerson = value; }
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
		/// ОКПО
		/// </summary>
		public string Okpo{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public Organization()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265880; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265880; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Организации"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265880);
			}
		}


	}
}