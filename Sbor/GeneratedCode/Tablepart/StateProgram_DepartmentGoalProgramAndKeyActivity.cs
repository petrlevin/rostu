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
	/// ВЦП и основные мероприятия
	/// </summary>
	public partial class StateProgram_DepartmentGoalProgramAndKeyActivity : TablePartEntity      
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
		/// Код
		/// </summary>
		public int IdAnalyticalCodeStateProgram{get; set;}
        /// <summary>
	    /// Код
	    /// </summary>
		public virtual Sbor.Reference.AnalyticalCodeStateProgram AnalyticalCodeStateProgram{get; set;}
		

		/// <summary>
		/// Тип документа
		/// </summary>
		public int IdDocType{get; set;}
        /// <summary>
	    /// Тип документа
	    /// </summary>
		public virtual Sbor.Reference.DocType DocType{get; set;}
		

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
		/// Ссылка на сущность
		/// </summary>
		public int? IdDocumentEntity{get; set;}
        /// <summary>
	    /// Ссылка на сущность
	    /// </summary>
		public virtual Entity DocumentEntity{get; set;}
		

		/// <summary>
		/// Статус
		/// </summary>
		public int? IdActiveDocStatus{get; set;}

		/// <summary>
		/// idActiveDocument
		/// </summary>
		public Int32? IdActiveDocument{get; set;}

		/// <summary>
		/// Актуальный документ
		/// </summary>
		public string ActiveDocumentCaption{get; set;}

			private ICollection<Sbor.Tablepart.StateProgram_DGPKAResourceMaintenance> _StateProgram_DGPKAResourceMaintenance; 
        /// <summary>
        /// ВЦП и основное мероприятие
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.StateProgram_DGPKAResourceMaintenance> StateProgram_DGPKAResourceMaintenance 
		{
			get{ return _StateProgram_DGPKAResourceMaintenance ?? (_StateProgram_DGPKAResourceMaintenance = new List<Sbor.Tablepart.StateProgram_DGPKAResourceMaintenance>()); } 
			set{ _StateProgram_DGPKAResourceMaintenance = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public StateProgram_DepartmentGoalProgramAndKeyActivity()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503806; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503806; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ВЦП и основные мероприятия"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503806);
			}
		}


	}
}