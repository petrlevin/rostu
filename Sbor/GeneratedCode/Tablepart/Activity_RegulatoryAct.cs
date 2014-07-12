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
	/// НПА
	/// </summary>
	public partial class Activity_RegulatoryAct : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Reference.Activity Owner{get; set;}
		

		/// <summary>
		/// НПА
		/// </summary>
		public int IdRegulatoryAct{get; set;}
        /// <summary>
	    /// НПА
	    /// </summary>
		public virtual Sbor.Reference.RegulatoryAct RegulatoryAct{get; set;}
		

		/// <summary>
		/// Является основанием для предоставления
		/// </summary>
		public bool? IsBasis{get; set;}

		/// <summary>
		/// Устанавливает стандарты качества, регламенты
		/// </summary>
		public bool? IsEstablishQualityStandard{get; set;}

		/// <summary>
		/// Устанавливает предельные цены (тарифы)
		/// </summary>
		public bool? IsSetMaxPrice{get; set;}

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ведомство
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Ведомство
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public Activity_RegulatoryAct()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503831; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503831; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "НПА"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503831);
			}
		}


	}
}