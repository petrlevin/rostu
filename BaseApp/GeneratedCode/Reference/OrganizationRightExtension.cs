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



namespace BaseApp.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Расширение организационных прав
	/// </summary>
	public partial class OrganizationRightExtension : ReferenceEntity      
	{
	
		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Вид (запись или чтение)
		/// </summary>
		public byte IdKind{get; set;}
                            /// <summary>
                            /// Вид (запись или чтение)
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.OrganizationRightExtensionKind Kind {
								get { return (BaseApp.DbEnums.OrganizationRightExtensionKind)this.IdKind; } 
								set { this.IdKind = (byte) value; }
							}

		/// <summary>
		/// Шаблон Sql
		/// </summary>
		public string SqlTemplate{get; set;}

	
private ICollection<Entity> _Targets; 
        /// <summary>
        /// Расширение орг.прав
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Entity> Targets 
		{
			get{ return _Targets ?? (_Targets = new List<Entity>()); } 
			set{ _Targets = value; }
		}
			private ICollection<Entity> _Results; 
        /// <summary>
        /// Расширение орг. прав
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Entity> Results 
		{
			get{ return _Results ?? (_Results = new List<Entity>()); } 
			set{ _Results = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public OrganizationRightExtension()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1744830298; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1744830298; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Расширение организационных прав"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830298);
			}
		}


	}
}