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
	/// Правила фильтрации
	/// </summary>
	public partial class BalanceConfig_FilterRule : TablePartEntity      
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
		/// Название правила
		/// </summary>
		public string Caption{get; set;}

			private ICollection<Sbor.Tablepart.BalanceConfig_User> _BalanceConfig_User; 
        /// <summary>
        /// Правило фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalanceConfig_User> BalanceConfig_User 
		{
			get{ return _BalanceConfig_User ?? (_BalanceConfig_User = new List<Sbor.Tablepart.BalanceConfig_User>()); } 
			set{ _BalanceConfig_User = value; }
		}
		private ICollection<Sbor.Tablepart.BalanceConfig_FilterKBK> _BalanceConfig_FilterKBK; 
        /// <summary>
        /// Правило фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalanceConfig_FilterKBK> BalanceConfig_FilterKBK 
		{
			get{ return _BalanceConfig_FilterKBK ?? (_BalanceConfig_FilterKBK = new List<Sbor.Tablepart.BalanceConfig_FilterKBK>()); } 
			set{ _BalanceConfig_FilterKBK = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalanceConfig_FilterRule()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265303; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265303; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Правила фильтрации"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265303);
			}
		}


	}
}