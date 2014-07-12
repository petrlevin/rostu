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
	/// Показатели финансового состояния учреждений
	/// </summary>
	public partial class FinancialIndicator : ReferenceEntity, IHasRefStatus    , IHierarhy  
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
		/// Вышестоящий
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящий
	    /// </summary>
		public virtual Sbor.Reference.FinancialIndicator Parent{get; set;}
		private ICollection<Sbor.Reference.FinancialIndicator> _idParent; 
        /// <summary>
        /// Вышестоящий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.FinancialIndicator> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Reference.FinancialIndicator>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Код строки
		/// </summary>
		public string RowCode{get; set;}

		/// <summary>
		/// Наименование показателя
		/// </summary>
		public string Caption{get; set;}

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
		public FinancialIndicator()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1275068396; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1275068396; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Показатели финансового состояния учреждений"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1275068396);
			}
		}


	}
}