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



namespace Sbor.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Пользователи
	/// </summary>
	public partial class BalanceConfig_User : TablePartEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Tool.BalanceConfig Owner{get; set;}
		

		/// <summary>
		/// Пользователь
		/// </summary>
		public int IdUser{get; set;}
        /// <summary>
	    /// Пользователь
	    /// </summary>
		public virtual BaseApp.Reference.User User{get; set;}
		

		/// <summary>
		/// Правило фильтрации
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Правило фильтрации
	    /// </summary>
		public virtual Sbor.Tablepart.BalanceConfig_FilterRule Master{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalanceConfig_User()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265302; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265302; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265302);
			}
		}


	}
}