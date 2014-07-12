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
	/// Ресурсное обеспечение ВЦП и основных мероприятий - значения
	/// </summary>
	public partial class StateProgram_DGPKAResourceMaintenance_Value : TablePartEntity      
	{
	
		/// <summary>
		/// Год
		/// </summary>
		public int IdHierarchyPeriod{get; set;}
        /// <summary>
	    /// Год
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.HierarchyPeriod HierarchyPeriod{get; set;}
		

		/// <summary>
		/// Значение
		/// </summary>
		public decimal? Value{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.StateProgram Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на главную ТЧ
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Ссылка на главную ТЧ
	    /// </summary>
		public virtual Sbor.Tablepart.StateProgram_DGPKAResourceMaintenance Master{get; set;}
		

		/// <summary>
		/// Доп. потребность
		/// </summary>
		public decimal? AdditionalValue{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public StateProgram_DGPKAResourceMaintenance_Value()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503752; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503752; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Ресурсное обеспечение ВЦП и основных мероприятий - значения"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503752);
			}
		}


	}
}