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

namespace BaseApp.Registry
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Выполненные операции
	/// </summary>
	public partial class ExecutedOperation : RegistryEntity  , IHasCommonRegistrator    
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		public Int32 Id{get; set;}

		/// <summary>
		/// Время операции
		/// </summary>
		private DateTime? _Date; 
        /// <summary>
	    /// Время операции
	    /// </summary>
		public  DateTime? Date 
		{
			get{ return _Date != null ? ((DateTime)_Date).Date : (DateTime?)null; }
			set{ _Date = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Пользователь
		/// </summary>
		public int IdUser{get; set;}
        /// <summary>
	    /// Пользователь
	    /// </summary>
		public virtual BaseApp.Reference.User User{get; set;}
		

		/// <summary>
		/// Операция
		/// </summary>
		public int IdEntityOperation{get; set;}
        /// <summary>
	    /// Операция
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.EntityOperation EntityOperation{get; set;}
		

		/// <summary>
		/// Исходный статус
		/// </summary>
		public int IdOriginalStatus{get; set;}
        /// <summary>
	    /// Исходный статус
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.DocStatus OriginalStatus{get; set;}
		

		/// <summary>
		/// Конечный статус
		/// </summary>
		public int IdFinalStatus{get; set;}
        /// <summary>
	    /// Конечный статус
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.DocStatus FinalStatus{get; set;}
		

		/// <summary>
		/// Регистратор
		/// </summary>
		public int IdRegistrator{get; set;}

		/// <summary>
		/// Регистратор: тип документа
		/// </summary>
		public int IdRegistratorEntity{get; set;}
        /// <summary>
	    /// Регистратор: тип документа
	    /// </summary>
		public virtual Entity RegistratorEntity{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ExecutedOperation()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2080374745; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2080374745; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Выполненные операции"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2080374745);
			}
		}


	}
}