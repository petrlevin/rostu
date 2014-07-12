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
	/// Полномочия расходных обязательств
	/// </summary>
	public partial class AuthorityOfExpenseObligation : ReferenceEntity, IHasRefStatus    , IHierarhy  
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Вышестоящий
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящий
	    /// </summary>
		public virtual Sbor.Reference.AuthorityOfExpenseObligation Parent{get; set;}
		private ICollection<Sbor.Reference.AuthorityOfExpenseObligation> _idParent; 
        /// <summary>
        /// Вышестоящий
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.AuthorityOfExpenseObligation> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Reference.AuthorityOfExpenseObligation>()); } 
			set{ _idParent = value; }
		}

		/// <summary>
		/// Код
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Вид полномочия
		/// </summary>
		public byte IdAuthorityType{get; set;}
                            /// <summary>
                            /// Вид полномочия
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.AuthorityType AuthorityType {
								get { return (Sbor.DbEnums.AuthorityType)this.IdAuthorityType; } 
								set { this.IdAuthorityType = (byte) value; }
							}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Номер строки
		/// </summary>
		public string LineNumber{get; set;}

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
		public AuthorityOfExpenseObligation()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265876; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265876; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Полномочия расходных обязательств"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265876);
			}
		}


	}
}