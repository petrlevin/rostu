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
	/// Пользователи
	/// </summary>
	public partial class User : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ф.И.О пользователя
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Логин пользователя
		/// </summary>
		public string Name{get; set;}

		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email{get; set;}

		/// <summary>
		/// Пароль
		/// </summary>
		public string Password{get; set;}

		/// <summary>
		/// Дата последнего входа
		/// </summary>
		private DateTime? _DateofLastEntry; 
        /// <summary>
	    /// Дата последнего входа
	    /// </summary>
		public  DateTime? DateofLastEntry 
		{
			get{ return _DateofLastEntry != null ? ((DateTime)_DateofLastEntry).Date : (DateTime?)null; }
			set{ _DateofLastEntry = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Отдел
		/// </summary>
		public string Department{get; set;}

		/// <summary>
		/// Интернет адрес
		/// </summary>
		public string Site{get; set;}

		/// <summary>
		/// Адрес сервера приложений
		/// </summary>
		public string IISAddress{get; set;}

		/// <summary>
		/// Контактный телефон
		/// </summary>
		public string Telephone{get; set;}

		/// <summary>
		/// Группа доступа
		/// </summary>
		public int IdAccessGroup{get; set;}
        /// <summary>
	    /// Группа доступа
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.AccessGroup AccessGroup{get; set;}
		

		/// <summary>
		/// Организация
		/// </summary>
		public int? IdOrganization{get; set;}
        /// <summary>
	    /// Организация
	    /// </summary>
		public virtual BaseApp.Reference.Organization Organization{get; set;}
		

		/// <summary>
		/// Ответственное лицо
		/// </summary>
		public int? IdResponsiblePerson{get; set;}
        /// <summary>
	    /// Ответственное лицо
	    /// </summary>
		public virtual BaseApp.Reference.ResponsiblePerson ResponsiblePerson{get; set;}
		

		/// <summary>
		/// Сменить пароль при следующем входе
		/// </summary>
		public bool ChangePasswordNextTime{get; set;}

		/// <summary>
		/// Заблокирован
		/// </summary>
		public bool IsBlocked{get; set;}

	
private ICollection<BaseApp.Reference.Role> _mlRoles; 
        /// <summary>
        /// Пользователь
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.Role> Roles 
		{
			get{ return _mlRoles ?? (_mlRoles = new List<BaseApp.Reference.Role>()); } 
			set{ _mlRoles = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public User()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2147483493; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2147483493; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Пользователи"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2147483493);
			}
		}


	}
}