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

using Platform.PrimaryEntities.DbEnums;using Platform.PrimaryEntities.Common.DbEnums;

namespace Sbor.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Нормативно-правовые акты
	/// </summary>
	public partial class RegulatoryAct : ReferenceEntity, IHasRefStatus      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Уровень
		/// </summary>
		public int IdBudgetLevel{get; set;}
        /// <summary>
	    /// Уровень
	    /// </summary>
		public virtual BaseApp.Reference.BudgetLevel BudgetLevel{get; set;}
		

		/// <summary>
		/// Вид НПА
		/// </summary>
		public int IdTypeRegulatoryAct{get; set;}
        /// <summary>
	    /// Вид НПА
	    /// </summary>
		public virtual Sbor.Reference.TypeRegulatoryAct TypeRegulatoryAct{get; set;}
		

		/// <summary>
		/// Номер
		/// </summary>
		public string Number{get; set;}

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
		/// Дата вступления в силу
		/// </summary>
		private DateTime _DateStart; 
        /// <summary>
	    /// Дата вступления в силу
	    /// </summary>
		public  DateTime DateStart 
		{
			get{ return _DateStart.Date; }
			set{ _DateStart = value.Date; }
		}

		/// <summary>
		/// Срок действия до
		/// </summary>
		private DateTime? _DateEnd; 
        /// <summary>
	    /// Срок действия до
	    /// </summary>
		public  DateTime? DateEnd 
		{
			get{ return _DateEnd != null ? ((DateTime)_DateEnd).Date : (DateTime?)null; }
			set{ _DateEnd = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Орган, принявший НПА
		/// </summary>
		public string AuthorityRegulatoryAct{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Структурные единицы НПА
		/// </summary>
		private ICollection<Sbor.Tablepart.RegulatoryAct_StructuralUnit> _StructuralUnit; 
        /// <summary>
        /// Структурные единицы НПА
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.RegulatoryAct_StructuralUnit> StructuralUnit 
		{
			get{ return _StructuralUnit ?? (_StructuralUnit = new List<Sbor.Tablepart.RegulatoryAct_StructuralUnit>()); } 
			set{ _StructuralUnit = value; }
		}

		/// <summary>
		/// Статус
		/// </summary>
		public byte IdRefStatus{get; set;}
                            /// <summary>
                            /// Статус
                            /// </summary>
							[NotMapped] 
                            public virtual RefStatus RefStatus {
								get { return (RefStatus)this.IdRefStatus; } 
								set { this.IdRefStatus = (byte) value; }
							}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public RegulatoryAct()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265886; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265886; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Нормативно-правовые акты"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265886);
			}
		}


	}
}