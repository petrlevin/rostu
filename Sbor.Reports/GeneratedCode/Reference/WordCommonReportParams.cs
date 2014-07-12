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

namespace Sbor.Reports.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Переменные отчетов
	/// </summary>
	public partial class WordCommonReportParams : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Сущность выходного параметра
		/// </summary>
		public int? IdOutputEntity{get; set;}
        /// <summary>
	    /// Сущность выходного параметра
	    /// </summary>
		public virtual Entity OutputEntity{get; set;}
		

		/// <summary>
		/// Поле сущности
		/// </summary>
		public int? IdOutputEntityField{get; set;}
        /// <summary>
	    /// Поле сущности
	    /// </summary>
		public virtual EntityField OutputEntityField{get; set;}
		

		/// <summary>
		/// SQL запрос
		/// </summary>
		public string SqlQuery{get; set;}

		/// <summary>
		/// Входные параметры
		/// </summary>
		private ICollection<Sbor.Reports.Tablepart.WordCommonReportParams_InputParam> _tpInputParams; 
        /// <summary>
        /// Входные параметры
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Sbor.Reports.Tablepart.WordCommonReportParams_InputParam> InputParams 
		{
			get{ return _tpInputParams ?? (_tpInputParams = new List<Sbor.Reports.Tablepart.WordCommonReportParams_InputParam>()); } 
			set{ _tpInputParams = value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public WordCommonReportParams()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1342177253; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1342177253; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Переменные отчетов"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1342177253);
			}
		}


	}
}