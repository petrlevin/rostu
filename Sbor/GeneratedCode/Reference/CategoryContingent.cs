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
	/// Категории контингента
	/// </summary>
	public partial class CategoryContingent : ReferenceEntity, IHasRefStatus    , IHierarhy  
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
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Вышестоящая группа контингента
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Вышестоящая группа контингента
	    /// </summary>
		public virtual Sbor.Reference.CategoryContingent Parent{get; set;}
		private ICollection<Sbor.Reference.CategoryContingent> _idParent; 
        /// <summary>
        /// Вышестоящая группа контингента
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.CategoryContingent> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<Sbor.Reference.CategoryContingent>()); } 
			set{ _idParent = value; }
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

	
private ICollection<Sbor.Reference.Contingent> _mlContingent_CategoryContingent; 
        /// <summary>
        /// Категория контингента
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reference.Contingent> Contingent_CategoryContingent 
		{
			get{ return _mlContingent_CategoryContingent ?? (_mlContingent_CategoryContingent = new List<Sbor.Reference.Contingent>()); } 
			set{ _mlContingent_CategoryContingent = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public CategoryContingent()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503836; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503836; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Категории контингента"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503836);
			}
		}


	}
}