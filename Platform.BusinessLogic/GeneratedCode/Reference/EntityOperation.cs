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


using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Операции сущностей
	/// </summary>
	public partial class EntityOperation : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Сущность
		/// </summary>
		public int IdEntity{get; set;}
        /// <summary>
	    /// Сущность
	    /// </summary>
		public virtual Entity Entity{get; set;}
		

		/// <summary>
		/// Операция
		/// </summary>
		public int IdOperation{get; set;}
        /// <summary>
	    /// Операция
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.Operation Operation{get; set;}
		

		/// <summary>
		/// Скрытая
		/// </summary>
		public bool IsHidden{get; set;}

	
private ICollection<Platform.BusinessLogic.Reference.DocStatus> _OriginalStatus; 
        /// <summary>
        /// Операция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Platform.BusinessLogic.Reference.DocStatus> OriginalStatus 
		{
			get{ return _OriginalStatus ?? (_OriginalStatus = new List<Platform.BusinessLogic.Reference.DocStatus>()); } 
			set{ _OriginalStatus = value; }
		}
			private ICollection<Platform.BusinessLogic.Reference.DocStatus> _FinalStatus; 
        /// <summary>
        /// Операция
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Platform.BusinessLogic.Reference.DocStatus> FinalStatus 
		{
			get{ return _FinalStatus ?? (_FinalStatus = new List<Platform.BusinessLogic.Reference.DocStatus>()); } 
			set{ _FinalStatus = value; }
		}
			private ICollection<EntityField> _EditableFields; 
        /// <summary>
        /// Операция сущности
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<EntityField> EditableFields 
		{
			get{ return _EditableFields ?? (_EditableFields = new List<EntityField>()); } 
			set{ _EditableFields = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public EntityOperation()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2147483487; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2147483487; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Операции сущностей"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2147483487);
			}
		}


	}
}