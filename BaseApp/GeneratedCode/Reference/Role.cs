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



namespace BaseApp.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Роли
	/// </summary>
	public partial class Role : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Группа доступа
		/// </summary>
		public int? IdAccessGroup{get; set;}
        /// <summary>
	    /// Группа доступа
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.AccessGroup AccessGroup{get; set;}
		

		/// <summary>
		/// Тип роли
		/// </summary>
		public byte IdRoleType{get; set;}
                            /// <summary>
                            /// Тип роли
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.RoleType RoleType {
								get { return (BaseApp.DbEnums.RoleType)this.IdRoleType; } 
								set { this.IdRoleType = (byte) value; }
							}

		/// <summary>
		/// Функциональные права
		/// </summary>
		private ICollection<BaseApp.Tablepart.Role_FunctionalRight> _tpFunctionalRights; 
        /// <summary>
        /// Функциональные права
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Tablepart.Role_FunctionalRight> FunctionalRights 
		{
			get{ return _tpFunctionalRights ?? (_tpFunctionalRights = new List<BaseApp.Tablepart.Role_FunctionalRight>()); } 
			set{ _tpFunctionalRights = value; }
		}

		/// <summary>
		/// Операции документов
		/// </summary>
		private ICollection<BaseApp.Tablepart.Role_DocumentOperation> _tpRole_DocumentOperation; 
        /// <summary>
        /// Операции документов
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Tablepart.Role_DocumentOperation> Role_DocumentOperation 
		{
			get{ return _tpRole_DocumentOperation ?? (_tpRole_DocumentOperation = new List<BaseApp.Tablepart.Role_DocumentOperation>()); } 
			set{ _tpRole_DocumentOperation = value; }
		}

		/// <summary>
		/// Статусы справочников
		/// </summary>
		private ICollection<BaseApp.Tablepart.Role_RefStatus> _tpRole_RefStatus; 
        /// <summary>
        /// Статусы справочников
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Tablepart.Role_RefStatus> Role_RefStatus 
		{
			get{ return _tpRole_RefStatus ?? (_tpRole_RefStatus = new List<BaseApp.Tablepart.Role_RefStatus>()); } 
			set{ _tpRole_RefStatus = value; }
		}

		/// <summary>
		/// Организационные права
		/// </summary>
		private ICollection<BaseApp.Tablepart.Role_OrganizationRight> _tpRole_OrganizationRight; 
        /// <summary>
        /// Организационные права
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Tablepart.Role_OrganizationRight> Role_OrganizationRight 
		{
			get{ return _tpRole_OrganizationRight ?? (_tpRole_OrganizationRight = new List<BaseApp.Tablepart.Role_OrganizationRight>()); } 
			set{ _tpRole_OrganizationRight = value; }
		}

		/// <summary>
		/// Вид роли
		/// </summary>
		public byte IdRoleKind{get; set;}
                            /// <summary>
                            /// Вид роли
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.RoleKind RoleKind {
								get { return (BaseApp.DbEnums.RoleKind)this.IdRoleKind; } 
								set { this.IdRoleKind = (byte) value; }
							}

	
private ICollection<BaseApp.Reference.User> _mlUsers; 
        /// <summary>
        /// Роль
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.User> Users 
		{
			get{ return _mlUsers ?? (_mlUsers = new List<BaseApp.Reference.User>()); } 
			set{ _mlUsers = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public Role()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2147483492; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2147483492; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Роли"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2147483492);
			}
		}


	}
}