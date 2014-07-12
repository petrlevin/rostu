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
	/// Структурные единицы НПА
	/// </summary>
	public partial class RegulatoryAct_StructuralUnit : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Reference.RegulatoryAct Owner{get; set;}
		

		/// <summary>
		/// Статья
		/// </summary>
		public string Article{get; set;}

		/// <summary>
		/// Часть
		/// </summary>
		public string Part{get; set;}

		/// <summary>
		/// Пункт
		/// </summary>
		public string Item{get; set;}

		/// <summary>
		/// Подпункт
		/// </summary>
		public string SubItem{get; set;}

		/// <summary>
		/// Абзац
		/// </summary>
		public string Paragraph{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

			private ICollection<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit> _ExpenditureObligations_RegulatoryAct_StructuralUnit; 
        /// <summary>
        /// Наименование
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit> ExpenditureObligations_RegulatoryAct_StructuralUnit 
		{
			get{ return _ExpenditureObligations_RegulatoryAct_StructuralUnit ?? (_ExpenditureObligations_RegulatoryAct_StructuralUnit = new List<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit>()); } 
			set{ _ExpenditureObligations_RegulatoryAct_StructuralUnit = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public RegulatoryAct_StructuralUnit()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265873; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265873; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Структурные единицы НПА"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265873);
			}
		}


	}
}