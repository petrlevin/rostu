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
	/// ТЧ Функциональные права
	/// </summary>
	public partial class Role_FunctionalRight : TablePartEntity      
	{
	
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
		/// Сущность
		/// </summary>
		public int IdEntity{get; set;}
        /// <summary>
	    /// Сущность
	    /// </summary>
		public virtual Entity Entity{get; set;}
		

		/// <summary>
		/// Редактирование
		/// </summary>
		public bool EditingFlag{get; set;}

			private ICollection<BaseApp.Tablepart.Role_DocumentOperation> _Role_DocumentOperation; 
        /// <summary>
        /// Связь с ТЧ Функциональные права
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Tablepart.Role_DocumentOperation> Role_DocumentOperation 
		{
			get{ return _Role_DocumentOperation ?? (_Role_DocumentOperation = new List<BaseApp.Tablepart.Role_DocumentOperation>()); } 
			set{ _Role_DocumentOperation = value; }
		}
		private ICollection<BaseApp.Tablepart.Role_RefStatus> _Role_RefStatus; 
        /// <summary>
        /// Связь с ТЧ Функциональные права
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Tablepart.Role_RefStatus> Role_RefStatus 
		{
			get{ return _Role_RefStatus ?? (_Role_RefStatus = new List<BaseApp.Tablepart.Role_RefStatus>()); } 
			set{ _Role_RefStatus = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public Role_FunctionalRight()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1879048141; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1879048141; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ Функциональные права"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1879048141);
			}
		}


	}
}