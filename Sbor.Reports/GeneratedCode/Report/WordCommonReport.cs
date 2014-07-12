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



namespace Sbor.Reports.Report
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Универсальный отчет по шаблонам в WORD
	/// </summary>
	public partial class WordCommonReport : ReportEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Имя профиля
		/// </summary>
		public string ReportProfileCaption{get; set;}

		/// <summary>
		/// Тип профиля
		/// </summary>
		public byte? IdReportProfileType{get; set;}
                            /// <summary>
                            /// Тип профиля
                            /// </summary>
							[NotMapped] 
                            public virtual BaseApp.DbEnums.ReportProfileType? ReportProfileType {
								get { return (BaseApp.DbEnums.ReportProfileType?)this.IdReportProfileType; } 
								set { this.IdReportProfileType = (byte?) value; }
							}

		/// <summary>
		/// Автор профиля
		/// </summary>
		public int? IdReportProfileUser{get; set;}
        /// <summary>
	    /// Автор профиля
	    /// </summary>
		public virtual BaseApp.Reference.User ReportProfileUser{get; set;}
		

		/// <summary>
		/// Временный экземпляр
		/// </summary>
		public bool? IsTemporary{get; set;}

		/// <summary>
		/// Файл-шаблон
		/// </summary>
		public Int32? IdTemplateFile{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public WordCommonReport()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1342177255; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1342177255; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Универсальный отчет по шаблонам в WORD"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1342177255);
			}
		}


	}
}