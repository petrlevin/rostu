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



namespace Tests.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Дочерняя ТЧ [ТЧ справочника "ДТЧ"]
	/// </summary>
	public partial class testDtp_Child : TablePartEntity      
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
		public virtual Tests.Reference.testDtp Owner{get; set;}
		

		/// <summary>
		/// Ссылка на родительскую ТЧ
		/// </summary>
		public int? IdMaster{get; set;}

		/// <summary>
		/// Период
		/// </summary>
		public int? IdHierarchyPeriod{get; set;}
        /// <summary>
	    /// Период
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.HierarchyPeriod HierarchyPeriod{get; set;}
		

		/// <summary>
		/// Средства
		/// </summary>
		public decimal? Value1{get; set;}

		/// <summary>
		/// Объем
		/// </summary>
		public Int32? Value2{get; set;}

		/// <summary>
		/// Коэффициент
		/// </summary>
		public decimal? Value3{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public testDtp_Child()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1811939285; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1811939285; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Дочерняя ТЧ [ТЧ справочника \"ДТЧ\"]"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1811939285);
			}
		}


	}
}