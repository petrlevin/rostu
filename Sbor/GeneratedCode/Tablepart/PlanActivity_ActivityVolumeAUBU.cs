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
	/// Объемы мероприятий АУ/БУ
	/// </summary>
	public partial class PlanActivity_ActivityVolumeAUBU : TablePartEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.PlanActivity Owner{get; set;}
		

		/// <summary>
		/// Мероприятие АУ/БУ
		/// </summary>
		public int? IdMaster{get; set;}
        /// <summary>
	    /// Мероприятие АУ/БУ
	    /// </summary>
		public virtual Sbor.Tablepart.PlanActivity_ActivityAUBU Master{get; set;}
		

		/// <summary>
		/// Период
		/// </summary>
		public int IdHierarchyPeriod{get; set;}
        /// <summary>
	    /// Период
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.HierarchyPeriod HierarchyPeriod{get; set;}
		

		/// <summary>
		/// Объем
		/// </summary>
		public decimal Volume{get; set;}

		/// <summary>
		/// Доп. потребность
		/// </summary>
		public decimal? AdditionalVolume{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PlanActivity_ActivityVolumeAUBU()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265427; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265427; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Объемы мероприятий АУ/БУ"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265427);
			}
		}


	}
}