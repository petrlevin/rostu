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
	/// Ресурсное обеспечение подпрограмм
	/// </summary>
	public partial class StateProgram_SubProgramResourceMaintenance : TablePartEntity      
	{
	
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
		/// Подпрограмма
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Подпрограмма
	    /// </summary>
		public virtual Sbor.Tablepart.StateProgram_ListSubProgram Master{get; set;}
		

		/// <summary>
		/// Источник
		/// </summary>
		public int? IdFinanceSource{get; set;}
        /// <summary>
	    /// Источник
	    /// </summary>
		public virtual Sbor.Reference.FinanceSource FinanceSource{get; set;}
		

			private ICollection<Sbor.Tablepart.StateProgram_SubProgramResourceMaintenance_Value> _StateProgram_SubProgramResourceMaintenance_Value; 
        /// <summary>
        /// Ссылка на главную ТЧ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.StateProgram_SubProgramResourceMaintenance_Value> StateProgram_SubProgramResourceMaintenance_Value 
		{
			get{ return _StateProgram_SubProgramResourceMaintenance_Value ?? (_StateProgram_SubProgramResourceMaintenance_Value = new List<Sbor.Tablepart.StateProgram_SubProgramResourceMaintenance_Value>()); } 
			set{ _StateProgram_SubProgramResourceMaintenance_Value = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public StateProgram_SubProgramResourceMaintenance()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503807; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503807; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Ресурсное обеспечение подпрограмм"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503807);
			}
		}


	}
}