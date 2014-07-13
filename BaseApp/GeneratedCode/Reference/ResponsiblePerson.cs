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
	/// Ответственные лица
	/// </summary>
	public partial class ResponsiblePerson : ReferenceEntity, IHasRefStatus      
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
		/// Организация
		/// </summary>
		public int IdOrganization{get; set;}
        /// <summary>
	    /// Организация
	    /// </summary>
		public virtual BaseApp.Reference.Organization Organization{get; set;}
		

		/// <summary>
		/// Ф.И.О
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Должность
		/// </summary>
		public int IdOfficialCapacity{get; set;}
        /// <summary>
	    /// Должность
	    /// </summary>
		public virtual BaseApp.Reference.OfficialCapacity OfficialCapacity{get; set;}
		

		/// <summary>
		/// Роль
		/// </summary>
		public byte? IdRoleResponsiblePerson{get; set;}
                            /// <summary>
                            /// Роль
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.RoleResponsiblePerson? RoleResponsiblePerson {
								get { return (BaseApp.DbEnums.RoleResponsiblePerson?)this.IdRoleResponsiblePerson; } 
								set { this.IdRoleResponsiblePerson = (byte?) value; }
							}

		/// <summary>
		/// Телефон
		/// </summary>
		public string Phone{get; set;}

		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email{get; set;}

		/// <summary>
		/// Дополнительные сведения
		/// </summary>
		public string MoreInformation{get; set;}

		/// <summary>
		/// Полномочия прекращены
		/// </summary>
		private DateTime? _DateEnd; 
        /// <summary>
	    /// Полномочия прекращены
	    /// </summary>
		public  DateTime? DateEnd 
		{
			get{ return _DateEnd != null ? ((DateTime)_DateEnd).Date : (DateTime?)null; }
			set{ _DateEnd = value != null ? ((DateTime)value).Date : value; }
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
		public ResponsiblePerson()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265881; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265881; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Ответственные лица"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265881);
			}
		}


	}
}