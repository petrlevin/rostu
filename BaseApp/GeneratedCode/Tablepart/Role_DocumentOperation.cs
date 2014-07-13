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



namespace BaseApp.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// ТЧ Операции документов
	/// </summary>
	public partial class Role_DocumentOperation : TablePartEntity      
	{
	
		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual BaseApp.Reference.Role Owner{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Операция
		/// </summary>
		public int IdOperation{get; set;}
        /// <summary>
	    /// Операция
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.Operation Operation{get; set;}
		

		/// <summary>
		/// Включен
		/// </summary>
		public bool SwitchOn{get; set;}

		/// <summary>
		/// Исходный статус
		/// </summary>
		public string InitialStatus{get; set;}

		/// <summary>
		/// Конечный статус
		/// </summary>
		public string FinalStatus{get; set;}

		/// <summary>
		/// Связь с ТЧ Функциональные права
		/// </summary>
		public int IdMaster{get; set;}
        /// <summary>
	    /// Связь с ТЧ Функциональные права
	    /// </summary>
		public virtual BaseApp.Tablepart.Role_FunctionalRight Master{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public Role_DocumentOperation()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1879048140; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1879048140; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ТЧ Операции документов"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1879048140);
			}
		}


	}
}