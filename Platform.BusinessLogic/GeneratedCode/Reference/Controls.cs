using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	public partial class Controls : ReferenceEntity 
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public override Int32 Id {get; set;}

		/// <summary>
		/// Включен
		/// </summary>
		public bool Enabled{get; set;}

		/// <summary>
		/// Мягкий
		/// </summary>
		public bool Skippable{get; set;}

	
		public Controls()
		{
 
		}
	}
}