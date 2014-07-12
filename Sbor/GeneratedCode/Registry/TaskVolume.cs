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
	/// Объемы задач
	/// </summary>
	public partial class TaskVolume : RegistryEntity  , IHasCommonRegistrator , IHasCommonTerminator  , IRegistryWithOperation , IRegistryWithTermOperation
	{
	
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
		/// Бюджет
		/// </summary>
		public int? IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// СБП
		/// </summary>
		public int IdSBP{get; set;}
        /// <summary>
	    /// СБП
	    /// </summary>
		public virtual Sbor.Reference.SBP SBP{get; set;}
		

		/// <summary>
		/// Программа
		/// </summary>
		public int? IdProgram{get; set;}
        /// <summary>
	    /// Программа
	    /// </summary>
		public virtual Sbor.Registry.Program Program{get; set;}
		

		/// <summary>
		/// Элемент СЦ
		/// </summary>
		public int? IdSystemGoalElement{get; set;}
        /// <summary>
	    /// Элемент СЦ
	    /// </summary>
		public virtual Sbor.Registry.SystemGoalElement SystemGoalElement{get; set;}
		

		/// <summary>
		/// Набор задач
		/// </summary>
		public int IdTaskCollection{get; set;}
        /// <summary>
	    /// Набор задач
	    /// </summary>
		public virtual Sbor.Registry.TaskCollection TaskCollection{get; set;}
		

		/// <summary>
		/// Показатель объема
		/// </summary>
		public int IdIndicatorActivity_Volume{get; set;}
        /// <summary>
	    /// Показатель объема
	    /// </summary>
		public virtual Sbor.Reference.IndicatorActivity IndicatorActivity_Volume{get; set;}
		

		/// <summary>
		/// Период
		/// </summary>
		public int IdHierarchyPeriod{get; set;}
        /// <summary>
	    /// Период
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.HierarchyPeriod HierarchyPeriod{get; set;}
		

		/// <summary>
		/// Тип значения
		/// </summary>
		public byte IdValueType{get; set;}
                            /// <summary>
                            /// Тип значения
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.ValueType ValueType {
								get { return (Sbor.DbEnums.ValueType)this.IdValueType; } 
								set { this.IdValueType = (byte) value; }
							}

		/// <summary>
		/// Значение
		/// </summary>
		public decimal Value{get; set;}

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
		/// Утверждающий документ: тип документа
		/// </summary>
		public int? IdApprovedEntity{get; set;}
        /// <summary>
	    /// Утверждающий документ: тип документа
	    /// </summary>
		public virtual Entity ApprovedEntity{get; set;}
		

		/// <summary>
		/// Аннулятор: тип документа
		/// </summary>
		public int? IdTerminatorEntity{get; set;}
        /// <summary>
	    /// Аннулятор: тип документа
	    /// </summary>
		public virtual Entity TerminatorEntity{get; set;}
		

		/// <summary>
		/// Доп. потребность
		/// </summary>
		public bool IsAdditionalNeed{get; set;}

		/// <summary>
		/// Деятельность АУ/БУ
		/// </summary>
		public bool? ActivityAUBU{get; set;}

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
		/// Конструктор по-умолчанию
		/// </summary>
		public TaskVolume()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1543503770; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1543503770; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Объемы задач"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1543503770);
			}
		}


	}
}