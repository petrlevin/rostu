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



namespace Sbor.Tool
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Настройка балансировки
	/// </summary>
	public partial class BalanceConfig : ToolEntity<BalanceConfig>    , IHierarhy  
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
		/// Дата
		/// </summary>
		private DateTime _Date; 
        /// <summary>
	    /// Дата
	    /// </summary>
		public  DateTime Date 
		{
			get{ return _Date.Date; }
			set{ _Date = value.Date; }
		}

		/// <summary>
		/// Правила фильтрации
		/// </summary>
		private ICollection<Sbor.Tablepart.BalanceConfig_FilterRule> _tpFilterRules; 
        /// <summary>
        /// Правила фильтрации
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalanceConfig_FilterRule> FilterRules 
		{
			get{ return _tpFilterRules ?? (_tpFilterRules = new List<Sbor.Tablepart.BalanceConfig_FilterRule>()); } 
			set{ _tpFilterRules = value; }
		}

		/// <summary>
		/// Пользователи
		/// </summary>
		private ICollection<Sbor.Tablepart.BalanceConfig_User> _tpUsers; 
        /// <summary>
        /// Пользователи
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalanceConfig_User> Users 
		{
			get{ return _tpUsers ?? (_tpUsers = new List<Sbor.Tablepart.BalanceConfig_User>()); } 
			set{ _tpUsers = value; }
		}

		/// <summary>
		/// Фильтры КБК
		/// </summary>
		private ICollection<Sbor.Tablepart.BalanceConfig_FilterKBK> _tpFilterKBKs; 
        /// <summary>
        /// Фильтры КБК
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.BalanceConfig_FilterKBK> FilterKBKs 
		{
			get{ return _tpFilterKBKs ?? (_tpFilterKBKs = new List<Sbor.Tablepart.BalanceConfig_FilterKBK>()); } 
			set{ _tpFilterKBKs = value; }
		}

		/// <summary>
		/// Источник данных для инструмента
		/// </summary>
		public byte IdSourcesDataTools{get; set;}
                            /// <summary>
                            /// Источник данных для инструмента
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.SourcesDataTools SourcesDataTools {
								get { return (Sbor.DbEnums.SourcesDataTools)this.IdSourcesDataTools; } 
								set { this.IdSourcesDataTools = (byte) value; }
							}

		/// <summary>
		/// Предыдущая версия
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Предыдущая версия
	    /// </summary>
		public virtual Sbor.Tool.BalanceConfig Parent{get; set;}
		private ICollection<Sbor.Tool.BalanceConfig> _idParent; 
        /// <summary>
        /// Предыдущая версия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tool.BalanceConfig> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Tool.BalanceConfig>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Последнее редактирование
		/// </summary>
		private DateTime? _DateLastEdit; 
        /// <summary>
	    /// Последнее редактирование
	    /// </summary>
		public  DateTime? DateLastEdit 
		{
			get{ return _DateLastEdit != null ? ((DateTime)_DateLastEdit).Date : (DateTime?)null; }
			set{ _DateLastEdit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Тип формируемого инструмента
		/// </summary>
		public byte IdBalancingIFDBType{get; set;}
                            /// <summary>
                            /// Тип формируемого инструмента
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.BalancingIFDBType BalancingIFDBType {
								get { return (Sbor.DbEnums.BalancingIFDBType)this.IdBalancingIFDBType; } 
								set { this.IdBalancingIFDBType = (byte) value; }
							}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalanceConfig()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265304; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265304; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Настройка балансировки"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265304);
			}
		}


	}
}