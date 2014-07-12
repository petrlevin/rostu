using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	public partial class tpStateProgram_GoalSystemElement : TablePartEntity 
	{
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;} public virtual Sbor.Document.StateProgram Owner{get; set;}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public override Int32 Id {get; set;}

		/// <summary>
		/// Из другого документа СЦ
		/// </summary>
		public bool FromAnotherDocumentSE{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdSystemGoal{get; set;} public virtual Sbor.Reference.SystemGoal SystemGoal{get; set;}

		/// <summary>
		/// Основная цель
		/// </summary>
		public bool IsMainGoal{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public int IdGoalIndicator{get; set;} public virtual Sbor.Reference.GoalIndicator GoalIndicator{get; set;}

		/// <summary>
		/// Код
		/// </summary>
		public string Code{get; set;}

		/// <summary>
		/// Вышестоящий
		/// </summary>
		public int IdParentSystemGoal{get; set;} public virtual Sbor.Reference.SystemGoal ParentSystemGoal{get; set;}

		/// <summary>
		/// Тип
		/// </summary>
		public int IdElementTypeSystemGoal{get; set;} public virtual Sbor.Reference.ElementTypeSystemGoal ElementTypeSystemGoal{get; set;}

		/// <summary>
		/// Срок реализации с
		/// </summary>
		public DateTime DateStart{get; set;}

		/// <summary>
		/// Срок реализации по
		/// </summary>
		public DateTime DateEnd{get; set;}

		/// <summary>
		/// Ответственный исполнитель
		/// </summary>
		public int IdSBP{get; set;} public virtual Sbor.Reference.SBP SBP{get; set;}

	
		public tpStateProgram_GoalSystemElement()
		{
 
		}
	}
}