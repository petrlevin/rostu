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
	/// Мероприятия
	/// </summary>
	public partial class RegisterActivity_Activity : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.RegisterActivity Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdActivity{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Reference.Activity Activity{get; set;}
		

		/// <summary>
		/// Показатель объема
		/// </summary>
		public int IdIndicatorActivity_Volume{get; set;}
        /// <summary>
	    /// Показатель объема
	    /// </summary>
		public virtual Sbor.Reference.IndicatorActivity IndicatorActivity_Volume{get; set;}
		

		/// <summary>
		/// Контингент
		/// </summary>
		public int? IdContingent{get; set;}
        /// <summary>
	    /// Контингент
	    /// </summary>
		public virtual Sbor.Reference.Contingent Contingent{get; set;}
		

		/// <summary>
		/// Раздел реестра
		/// </summary>
		public byte? IdRegistryKeyActivity{get; set;}
                            /// <summary>
                            /// Раздел реестра
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.RegistryKeyActivity? RegistryKeyActivity {
								get { return (Sbor.DbEnums.RegistryKeyActivity?)this.IdRegistryKeyActivity; } 
								set { this.IdRegistryKeyActivity = (byte?) value; }
							}

		/// <summary>
		/// Основная услуга
		/// </summary>
		public int? IdRegystryActivity_ActivityMain{get; set;}
        /// <summary>
	    /// Основная услуга
	    /// </summary>
		public virtual Sbor.Tablepart.RegisterActivity_Activity RegystryActivity_ActivityMain{get; set;}
		private ICollection<Sbor.Tablepart.RegisterActivity_Activity> _idRegystryActivity_ActivityMain; 
        /// <summary>
        /// Основная услуга
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.RegisterActivity_Activity> ChildrenByidRegystryActivity_ActivityMain 
		{
			get{ return _idRegystryActivity_ActivityMain ?? (_idRegystryActivity_ActivityMain = new List<Sbor.Tablepart.RegisterActivity_Activity>()); } 
			set{ _idRegystryActivity_ActivityMain = value; }
		}

			private ICollection<Sbor.Tablepart.RegisterActivity_IndicatorActivity> _RegisterActivity_IndicatorActivity; 
        /// <summary>
        /// ссылка на Мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.RegisterActivity_IndicatorActivity> RegisterActivity_IndicatorActivity 
		{
			get{ return _RegisterActivity_IndicatorActivity ?? (_RegisterActivity_IndicatorActivity = new List<Sbor.Tablepart.RegisterActivity_IndicatorActivity>()); } 
			set{ _RegisterActivity_IndicatorActivity = value; }
		}
		private ICollection<Sbor.Tablepart.RegisterActivity_Performers> _RegisterActivity_Performers; 
        /// <summary>
        /// ссылка на Мероприятия
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.RegisterActivity_Performers> RegisterActivity_Performers 
		{
			get{ return _RegisterActivity_Performers ?? (_RegisterActivity_Performers = new List<Sbor.Tablepart.RegisterActivity_Performers>()); } 
			set{ _RegisterActivity_Performers = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public RegisterActivity_Activity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503820; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503820; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Мероприятия"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503820);
			}
		}


	}
}