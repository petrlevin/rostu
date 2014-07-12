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
	/// Целевые показатели
	/// </summary>
	public partial class GoalTarget : RegistryEntity  , IHasCommonRegistrator , IHasCommonTerminator  , IRegistryWithOperation , IRegistryWithTermOperation
	{
	
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
		/// Элемент СЦ
		/// </summary>
		public int IdSystemGoalElement{get; set;}
        /// <summary>
	    /// Элемент СЦ
	    /// </summary>
		public virtual Sbor.Registry.SystemGoalElement SystemGoalElement{get; set;}
		

		/// <summary>
		/// Показатель
		/// </summary>
		public int IdGoalIndicator{get; set;}
        /// <summary>
	    /// Показатель
	    /// </summary>
		public virtual Sbor.Reference.GoalIndicator GoalIndicator{get; set;}
		

		/// <summary>
		/// Регистратор
		/// </summary>
		public int IdRegistrator{get; set;}

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
		/// Утверждающий документ
		/// </summary>
		public int? IdApproved{get; set;}

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
		/// Аннулятор
		/// </summary>
		public int? IdTerminator{get; set;}

		/// <summary>
		/// Регистратор: тип документа
		/// </summary>
		public int IdRegistratorEntity{get; set;}
        /// <summary>
	    /// Регистратор: тип документа
	    /// </summary>
		public virtual Entity RegistratorEntity{get; set;}
		

		/// <summary>
		/// Аннулятор: тип документа
		/// </summary>
		public int? IdTerminatorEntity{get; set;}
        /// <summary>
	    /// Аннулятор: тип документа
	    /// </summary>
		public virtual Entity TerminatorEntity{get; set;}
		

		/// <summary>
		/// Утверждающий документ: тип документа
		/// </summary>
		public int? IdApprovedEntity{get; set;}
        /// <summary>
	    /// Утверждающий документ: тип документа
	    /// </summary>
		public virtual Entity ApprovedEntity{get; set;}
		

		/// <summary>
		/// Дата создания записи
		/// </summary>
		private DateTime? _DateCreate; 
        /// <summary>
	    /// Дата создания записи
	    /// </summary>
		public  DateTime? DateCreate 
		{
			get{ return _DateCreate != null ? ((DateTime)_DateCreate).Date : (DateTime?)null; }
			set{ _DateCreate = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Операция анулирования
		/// </summary>
		public int? IdTerminateOperation{get; set;}
        /// <summary>
	    /// Операция анулирования
	    /// </summary>
		public virtual BaseApp.Registry.ExecutedOperation TerminateOperation{get; set;}
		

		/// <summary>
		/// Операция
		/// </summary>
		public int? IdExecutedOperation{get; set;}
        /// <summary>
	    /// Операция
	    /// </summary>
		public virtual BaseApp.Registry.ExecutedOperation ExecutedOperation{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public GoalTarget()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265855; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265855; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Целевые показатели"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265855);
			}
		}


	}
}