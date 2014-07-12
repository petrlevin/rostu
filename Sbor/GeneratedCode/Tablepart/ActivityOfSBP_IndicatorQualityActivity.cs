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
	/// Показатели качества мероприятий
	/// </summary>
	public partial class ActivityOfSBP_IndicatorQualityActivity : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.ActivityOfSBP Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Мероприятие
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Мероприятие
	    /// </summary>
		public virtual Sbor.Tablepart.ActivityOfSBP_Activity Master{get; set;}
		

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdIndicatorActivity{get; set;}
        /// <summary>
	    /// Наименование
	    /// </summary>
		public virtual Sbor.Reference.IndicatorActivity IndicatorActivity{get; set;}
		

			private ICollection<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity_Value> _ActivityOfSBP_IndicatorQualityActivity_Value; 
        /// <summary>
        /// Ссылка на главную ТЧ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity_Value> ActivityOfSBP_IndicatorQualityActivity_Value 
		{
			get{ return _ActivityOfSBP_IndicatorQualityActivity_Value ?? (_ActivityOfSBP_IndicatorQualityActivity_Value = new List<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity_Value>()); } 
			set{ _ActivityOfSBP_IndicatorQualityActivity_Value = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ActivityOfSBP_IndicatorQualityActivity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503773; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503773; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Показатели качества мероприятий"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503773);
			}
		}


	}
}