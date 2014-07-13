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

namespace BaseApp.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Разрешения на ввод дополнительных потребностей
	/// </summary>
	public partial class PermissionsInputAdditionalRequirements : ReferenceEntity, IHasRefStatus      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		public override Int32 Id{get; set;}

		/// <summary>
		/// Публично-правовое образование
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Статус
		/// </summary>
		public byte IdRefStatus{get; set;}
							[NotMapped] 
							public virtual RefStatus RefStatus {
								get { return (RefStatus)this.IdRefStatus; } 
								set { this.IdRefStatus = (byte) value; }
							}

		/// <summary>
		/// Ведомство
		/// </summary>
		public int IdSBP{get; set;}
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Разрешено вводить дополнительные потребности
		/// </summary>
		public bool EnterAdditionalRequirements{get; set;}

	

		public PermissionsInputAdditionalRequirements()
		{
		}

		/// <summary>
        /// Идентификатор типа сущности
        /// </summary>
		public override int EntityId
        {
            get { return -1610612495; }
        }

		/// <summary>
        /// Идентификатор типа сущности
        /// </summary>
		public new static int EntityIdStatic
        {
            get { return -1610612495; }
        }

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
        {
            get { return "Разрешения на ввод дополнительных потребностей"; }
        }

		

		

		/// <summary>
		/// Регистрация идентфикатора сущности
		/// </summary>
		public class EntityIdRegistrator:IBeforeAplicationStart
		{
			public void Execute()
			{
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1610612495);
			}
		}


	}
}