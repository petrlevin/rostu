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



namespace Tests.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// ДТЧ
	/// </summary>
	public partial class testDtp : ReferenceEntity      
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
		/// Родительская ТЧ
		/// </summary>
		

		/// <summary>
		/// Дочерняя ТЧ
		/// </summary>
		private ICollection<Tests.Tablepart.testDtp_Child> _tpChild; 
        /// <summary>
        /// Дочерняя ТЧ
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<Tests.Tablepart.testDtp_Child> Child 
		{
			get{ return _tpChild ?? (_tpChild = new List<Tests.Tablepart.testDtp_Child>()); } 
			set{ _tpChild = value; }
		}

		/// <summary>
		/// Ресурс 1 - Средства
		/// </summary>
		public bool V1{get; set;}

		/// <summary>
		/// Ресурс 2 - Объем
		/// </summary>
		public bool V2{get; set;}

		/// <summary>
		/// Ресурс 3 - Коэффициент
		/// </summary>
		public bool V3{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public testDtp()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1811939287; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1811939287; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "ДТЧ"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1811939287);
			}
		}


	}
}