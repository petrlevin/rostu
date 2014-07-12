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

namespace Sbor.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Типы колонок
	/// </summary>
	public partial class TableReport_ColumnType : TablePartEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
		public virtual BaseApp.Reference.TableReport Owner{get; set;}
		

		/// <summary>
		/// Имя поля
		/// </summary>
		public string FieldName{get; set;}

		/// <summary>
		/// Тип поля
		/// </summary>
		public byte? IdEntityFieldType{get; set;}
							[NotMapped] 
							public virtual EntityFieldType? EntityFieldType {
								get { return (EntityFieldType?)this.IdEntityFieldType; } 
								set { this.IdEntityFieldType = (byte?) value; }
							}

	

		public TableReport_ColumnType()
		{
		}

		/// <summary>
        /// Идентификатор типа сущности
        /// </summary>
		public override int EntityId
        {
            get { return -1207959506; }
        }

		/// <summary>
        /// Идентификатор типа сущности
        /// </summary>
		public new static int EntityIdStatic
        {
            get { return -1207959506; }
        }

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
        {
            get { return "Типы колонок"; }
        }

		

		

		/// <summary>
		/// Регистрация идентфикатора сущности
		/// </summary>
		public class EntityIdRegistrator:IBeforeAplicationStart
		{
			public void Execute()
			{
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959506);
			}
		}


	}
}