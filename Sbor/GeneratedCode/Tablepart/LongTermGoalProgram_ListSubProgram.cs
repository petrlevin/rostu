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

using Platform.PrimaryEntities.Reference;

namespace Sbor.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Перечень подпрограмм
	/// </summary>
	public partial class LongTermGoalProgram_ListSubProgram : TablePartEntity      
	{
	
		/// <summary>
		/// Статус
		/// </summary>
		public int? IdActualDocStatus{get; set;}

		/// <summary>
		/// Актуальный документ
		/// </summary>
		public int? IdActualDocument{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.LongTermGoalProgram Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Код
		/// </summary>
		public int? IdAnalyticalCodeStateProgram{get; set;}
        /// <summary>
	    /// Код
	    /// </summary>
		public virtual Sbor.Reference.AnalyticalCodeStateProgram AnalyticalCodeStateProgram{get; set;}
		

		/// <summary>
		/// Ответственный исполнитель
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// Ответственный исполнитель
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Тип ответственного исполнителя
		/// </summary>
		public int IdResponsibleExecutantType{get; set;}
        /// <summary>
	    /// Тип ответственного исполнителя
	    /// </summary>
		public virtual Sbor.Reference.ResponsibleExecutantType ResponsibleExecutantType{get; set;}
		

		/// <summary>
		/// Основная цель
		/// </summary>
		public int IdSystemGoal{get; set;}
        /// <summary>
	    /// Основная цель
	    /// </summary>
		public virtual Sbor.Reference.SystemGoal SystemGoal{get; set;}
		

		/// <summary>
		/// Срок реализации с
		/// </summary>
		private DateTime? _DateStart; 
        /// <summary>
	    /// Срок реализации с
	    /// </summary>
		public  DateTime? DateStart 
		{
			get{ return _DateStart != null ? ((DateTime)_DateStart).Date : (DateTime?)null; }
			set{ _DateStart = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Срок реализации по
		/// </summary>
		private DateTime? _DateEnd; 
        /// <summary>
	    /// Срок реализации по
	    /// </summary>
		public  DateTime? DateEnd 
		{
			get{ return _DateEnd != null ? ((DateTime)_DateEnd).Date : (DateTime?)null; }
			set{ _DateEnd = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Документ
		/// </summary>
		public int? IdDocument{get; set;}

		/// <summary>
		/// Документ: тип документа
		/// </summary>
		public int? IdDocumentEntity{get; set;}
        /// <summary>
	    /// Документ: тип документа
	    /// </summary>
		public virtual Entity DocumentEntity{get; set;}
		

			private ICollection<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance> _LongTermGoalProgram_SubProgramResourceMaintenance; 
        /// <summary>
        /// Подпрограмма
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance> LongTermGoalProgram_SubProgramResourceMaintenance 
		{
			get{ return _LongTermGoalProgram_SubProgramResourceMaintenance ?? (_LongTermGoalProgram_SubProgramResourceMaintenance = new List<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance>()); } 
			set{ _LongTermGoalProgram_SubProgramResourceMaintenance = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public LongTermGoalProgram_ListSubProgram()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503784; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503784; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Перечень подпрограмм"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503784);
			}
		}


	}
}