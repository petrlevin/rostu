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

using Platform.PrimaryEntities.Reference;

namespace BaseApp.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// ТЧ Организационные права
	/// </summary>
	public partial class Role_OrganizationRight : TablePartEntity      
	{
	
		/// <summary>
		/// В иерархии по полю
		/// </summary>
		public int? IdParentField{get; set;}
        /// <summary>
	    /// В иерархии по полю
	    /// </summary>
		public virtual EntityField ParentField{get; set;}
		

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual BaseApp.Reference.Role Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Значение поля
		/// </summary>
		public int? IdElement{get; set;}

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
		public int IdElementEntity{get; set;}
        /// <summary>
	    /// Ссылка на сущность
	    /// </summary>
		public virtual Entity ElementEntity{get; set;}
		

		/// <summary>
		/// Редактирование
		/// </summary>
		public bool EditingFlag{get; set;}

		/// <summary>
		/// Отключено
		/// </summary>
		public bool Disabled{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public Role_OrganizationRight()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1879048136; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1879048136; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ Организационные права"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1879048136);
			}
		}


	}
}