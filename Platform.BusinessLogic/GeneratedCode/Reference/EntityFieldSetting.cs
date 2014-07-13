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


using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Настройки полей сущности
	/// </summary>
	public partial class EntityFieldSetting : ReferenceEntity      
	{
	
		/// <summary>
		/// Игнорировать фильтр по PublicLegalFormation
		/// </summary>
		public bool? IgnoreFilterByPublicLegalFormation{get; set;}

		/// <summary>
		/// Игнорировать фильтр по Budget
		/// </summary>
		public bool? IgnoreFilterByBudget{get; set;}

		/// <summary>
		/// Игнорировать фильтр по Version
		/// </summary>
		public bool? IgnoreFilterByVersion{get; set;}

		/// <summary>
		/// Агрегатная функция
		/// </summary>
		public byte? IdAggregateFunction{get; set;}
                            /// <summary>
                            /// Агрегатная функция
                            /// </summary>
							[NotMapped] 
                            public virtual Platform.BusinessLogic.DbEnums.AggregateFunction? AggregateFunction {
								get { return (Platform.BusinessLogic.DbEnums.AggregateFunction?)this.IdAggregateFunction; } 
								set { this.IdAggregateFunction = (byte?) value; }
							}

		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Поле
		/// </summary>
		public int IdEntityField{get; set;}
        /// <summary>
	    /// Поле
	    /// </summary>
		public virtual EntityField EntityField{get; set;}
		

		/// <summary>
		/// Игнорировать организационные права
		/// </summary>
		public bool IgnoreOrganizationRights{get; set;}

		/// <summary>
		/// Вышестоящая сущность
		/// </summary>
		public int? IdEntity_Owner{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public EntityFieldSetting()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1744830431; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1744830431; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Настройки полей сущности"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830431);
			}
		}


	}
}