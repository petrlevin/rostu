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
	/// Требования к заданию
	/// </summary>
	public partial class PlanActivity_RequirementsForTheTask : TablePartEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Ссылка на владельца
		/// </summary>
		public int IdOwner{get; set;}
        /// <summary>
	    /// Ссылка на владельца
	    /// </summary>
		public virtual Sbor.Document.PlanActivity Owner{get; set;}
		

		/// <summary>
		/// Тип
		/// </summary>
		public byte IdActivityType{get; set;}
                            /// <summary>
                            /// Тип
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.ActivityType ActivityType {
								get { return (Sbor.DbEnums.ActivityType)this.IdActivityType; } 
								set { this.IdActivityType = (byte) value; }
							}

		/// <summary>
		/// Основание для досрочного прекращения исполнения задания
		/// </summary>
		public string ReasonTerminationTask{get; set;}

		/// <summary>
		/// Сроки предоставления отчетов об исполнении задания
		/// </summary>
		public string DatesReportingOnExecutionTask{get; set;}

		/// <summary>
		/// Иные требования к отчетности об исполнении задания
		/// </summary>
		public string OtherRequirementsOnExecutionTask{get; set;}

		/// <summary>
		/// Иная информация, необходимая для исполнения (контроля за исполнением) задания
		/// </summary>
		public string AnyOtherInformationOnExecutionTask{get; set;}

		/// <summary>
		/// Основания для приостановления задания
		/// </summary>
		public string GroundsSuspendTasks{get; set;}

			private ICollection<Sbor.Tablepart.PlanActivity_OrderOfControlTheExecutionTasks> _PlanActivity_OrderOfControlTheExecutionTasks; 
        /// <summary>
        /// Требование к заданию 
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.PlanActivity_OrderOfControlTheExecutionTasks> PlanActivity_OrderOfControlTheExecutionTasks 
		{
			get{ return _PlanActivity_OrderOfControlTheExecutionTasks ?? (_PlanActivity_OrderOfControlTheExecutionTasks = new List<Sbor.Tablepart.PlanActivity_OrderOfControlTheExecutionTasks>()); } 
			set{ _PlanActivity_OrderOfControlTheExecutionTasks = value; }
		}


		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public PlanActivity_RequirementsForTheTask()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265423; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265423; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Требования к заданию"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265423);
			}
		}


	}
}