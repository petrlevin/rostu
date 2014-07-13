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

using Platform.PrimaryEntities.DbEnums;using Platform.PrimaryEntities.Common.DbEnums;

namespace BaseApp.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Оперативные отчеты
	/// </summary>
	public partial class TableReport : ReferenceEntity    , IHierarhy  
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Системное имя
		/// </summary>
		public string Name{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Проект
		/// </summary>
		public int? IdSolutionProject{get; set;}
                            /// <summary>
                            /// Проект
                            /// </summary>
							[NotMapped] 
                            public virtual SolutionProject? SolutionProject {
								get { return (SolutionProject?)this.IdSolutionProject; } 
								set { this.IdSolutionProject = (int?) value; }
							}

		/// <summary>
		/// Запрос на выборку
		/// </summary>
		public string Sql{get; set;}

		/// <summary>
		/// Типы колонок (необязательные параметры)
		/// </summary>
		private ICollection<BaseApp.Tablepart.TableReport_ColumnType> _tpColumnTypes; 
        /// <summary>
        /// Типы колонок (необязательные параметры)
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Tablepart.TableReport_ColumnType> ColumnTypes 
		{
			get{ return _tpColumnTypes ?? (_tpColumnTypes = new List<BaseApp.Tablepart.TableReport_ColumnType>()); } 
			set{ _tpColumnTypes = value; }
		}

		/// <summary>
		/// Группа
		/// </summary>
		public int? IdParent{get; set;}
        /// <summary>
	    /// Группа
	    /// </summary>
		public virtual BaseApp.Reference.TableReport Parent{get; set;}
		private ICollection<BaseApp.Reference.TableReport> _idParent; 
        /// <summary>
        /// Группа
        /// </summary>
        [JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.TableReport> ChildrenByidParent 
		{
			get{ return _idParent ?? (_idParent = new List<BaseApp.Reference.TableReport>()); } 
			set{ _idParent = value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TableReport()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1811939281; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1811939281; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Оперативные отчеты"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1811939281);
			}
		}


	}
}