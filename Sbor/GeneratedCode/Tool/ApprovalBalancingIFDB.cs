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



namespace Sbor.Tool
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Утверждение балансировки расходов, доходов и ИФДБ
	/// </summary>
	public partial class ApprovalBalancingIFDB : ToolEntity<ApprovalBalancingIFDB>      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

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
		/// Дата
		/// </summary>
		private DateTime _Date; 
        /// <summary>
	    /// Дата
	    /// </summary>
		public  DateTime Date 
		{
			get{ return _Date.Date; }
			set{ _Date = value.Date; }
		}

		/// <summary>
		/// Дата обработки
		/// </summary>
		private DateTime? _DateCommit; 
        /// <summary>
	    /// Дата обработки
	    /// </summary>
		public  DateTime? DateCommit 
		{
			get{ return _DateCommit != null ? ((DateTime)_DateCommit).Date : (DateTime?)null; }
			set{ _DateCommit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Последнее редактирование
		/// </summary>
		private DateTime? _DateLastEdit; 
        /// <summary>
	    /// Последнее редактирование
	    /// </summary>
		public  DateTime? DateLastEdit 
		{
			get{ return _DateLastEdit != null ? ((DateTime)_DateLastEdit).Date : (DateTime?)null; }
			set{ _DateLastEdit = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Бюджет
		/// </summary>
		public int IdBudget{get; set;}
        /// <summary>
	    /// Бюджет
	    /// </summary>
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Статус
		/// </summary>
		public override int IdDocStatus{get; set;}

		/// <summary>
		/// Бланки
		/// </summary>
		private ICollection<Sbor.Tablepart.ApprovalBalancingIFDB_Blank> _tpBlanks; 
        /// <summary>
        /// Бланки
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tablepart.ApprovalBalancingIFDB_Blank> Blanks 
		{
			get{ return _tpBlanks ?? (_tpBlanks = new List<Sbor.Tablepart.ApprovalBalancingIFDB_Blank>()); } 
			set{ _tpBlanks = value; }
		}

		/// <summary>
		/// Источники данных для инструментов
		/// </summary>
		public byte IdSourcesDataTools{get; set;}
                            /// <summary>
                            /// Источники данных для инструментов
                            /// </summary>
							[NotMapped] 
                            public virtual Sbor.DbEnums.SourcesDataTools SourcesDataTools {
								get { return (Sbor.DbEnums.SourcesDataTools)this.IdSourcesDataTools; } 
								set { this.IdSourcesDataTools = (byte) value; }
							}

		/// <summary>
		/// Номер инструмента
		/// </summary>
		public string Number{get; set;}

	
private ICollection<Sbor.Tool.BalancingIFDB> _mlBalancingIFDBs; 
        /// <summary>
        /// Сводный инструмент
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Tool.BalancingIFDB> BalancingIFDBs 
		{
			get{ return _mlBalancingIFDBs ?? (_mlBalancingIFDBs = new List<Sbor.Tool.BalancingIFDB>()); } 
			set{ _mlBalancingIFDBs = value; }
		}
			private ICollection<Sbor.Document.LimitBudgetAllocations> _mlLimitBudgetAllocations; 
        /// <summary>
        /// Сводный инструмент
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.LimitBudgetAllocations> LimitBudgetAllocations 
		{
			get{ return _mlLimitBudgetAllocations ?? (_mlLimitBudgetAllocations = new List<Sbor.Document.LimitBudgetAllocations>()); } 
			set{ _mlLimitBudgetAllocations = value; }
		}
			private ICollection<Sbor.Document.ActivityOfSBP> _mlActivityOfSBPs; 
        /// <summary>
        /// Сводный инструмент
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Document.ActivityOfSBP> ActivityOfSBPs 
		{
			get{ return _mlActivityOfSBPs ?? (_mlActivityOfSBPs = new List<Sbor.Document.ActivityOfSBP>()); } 
			set{ _mlActivityOfSBPs = value; }
		}
			
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public ApprovalBalancingIFDB()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265063; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265063; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Утверждение балансировки расходов, доходов и ИФДБ"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265063);
			}
		}


	}
}