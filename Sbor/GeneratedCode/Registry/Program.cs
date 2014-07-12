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

namespace Sbor.Registry
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Программы
	/// </summary>
	public partial class Program : RegistryEntity  , IHasCommonRegistrator , IHasCommonTerminator  , IRegistryWithOperation , IRegistryWithTermOperation
	{
	
		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Аннулятор
		/// </summary>
		public int? IdTerminator{get; set;}

		/// <summary>
		/// Аннулятор: тип документа
		/// </summary>
		public int? IdTerminatorEntity{get; set;}
        /// <summary>
	    /// Аннулятор: тип документа
	    /// </summary>
		public virtual Entity TerminatorEntity{get; set;}
		

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
		/// Утверждающий документ
		/// </summary>
		public int? IdApproved{get; set;}

		/// <summary>
		/// Утверждающий документ: тип документа
		/// </summary>
		public int? IdApprovedEntity{get; set;}
        /// <summary>
	    /// Утверждающий документ: тип документа
	    /// </summary>
		public virtual Entity ApprovedEntity{get; set;}
		

		/// <summary>
		/// Операция
		/// </summary>
		public int? IdExecutedOperation{get; set;}
        /// <summary>
	    /// Операция
	    /// </summary>
		public virtual BaseApp.Registry.ExecutedOperation ExecutedOperation{get; set;}
		

		/// <summary>
		/// Операция анулирования
		/// </summary>
		public int? IdTerminateOperation{get; set;}
        /// <summary>
	    /// Операция анулирования
	    /// </summary>
		public virtual BaseApp.Registry.ExecutedOperation TerminateOperation{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		public Int32 Id{get; set;}

		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
        /// <summary>
	    /// ППО
	    /// </summary>
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Версия
		/// </summary>
		public int IdVersion{get; set;}
        /// <summary>
	    /// Версия
	    /// </summary>
		public virtual BaseApp.Reference.Version Version{get; set;}
		

		/// <summary>
		/// Дата утверждения
		/// </summary>
		private DateTime? _DateCommit; 
        /// <summary>
	    /// Дата утверждения
	    /// </summary>
		public  DateTime? DateCommit 
		{
			get{ return _DateCommit != null ? ((DateTime)_DateCommit).Date : (DateTime?)null; }
			set{ _DateCommit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Дата аннулирования
		/// </summary>
		private DateTime? _DateTerminate; 
        /// <summary>
	    /// Дата аннулирования
	    /// </summary>
		public  DateTime? DateTerminate 
		{
			get{ return _DateTerminate != null ? ((DateTime)_DateTerminate).Date : (DateTime?)null; }
			set{ _DateTerminate = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Дата создания записи
		/// </summary>
		private DateTime _DateCreate; 
        /// <summary>
	    /// Дата создания записи
	    /// </summary>
		public  DateTime DateCreate 
		{
			get{ return _DateCreate.Date; }
			set{ _DateCreate = value.Date; }
		}

		/// <summary>
		/// Тип
		/// </summary>
		public int IdDocType{get; set;}
        /// <summary>
	    /// Тип
	    /// </summary>
		public virtual Sbor.Reference.DocType DocType{get; set;}
		

		/// <summary>
		/// СБП
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// СБП
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public Program()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503802; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503802; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Программы"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503802);
			}
		}


	}
}