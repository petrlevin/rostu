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




namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Файлы
	/// </summary>
	public partial class FileLink : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Хранить в базе
		/// </summary>
		public bool IsDbStore{get; set;}

		/// <summary>
		/// Файл в базе
		/// </summary>
		public int? IdFileStore{get; set;}
        /// <summary>
	    /// Файл в базе
	    /// </summary>
		public virtual Platform.BusinessLogic.Reference.FileStore FileStore{get; set;}
		

		/// <summary>
		/// Путь к файлу
		/// </summary>
		public string FilePath{get; set;}

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption{get; set;}

		/// <summary>
		/// Описание
		/// </summary>
		public string Description{get; set;}

		/// <summary>
		/// Дата добавления
		/// </summary>
		private DateTime? _Date; 
        /// <summary>
	    /// Дата добавления
	    /// </summary>
		public  DateTime? Date 
		{
			get{ return _Date != null ? ((DateTime)_Date).Date : (DateTime?)null; }
			set{ _Date = value != null ? ((DateTime)value).Date : value; }
		}

		/// <summary>
		/// Размер файла
		/// </summary>
		public Int32? FileSize{get; set;}

		/// <summary>
		/// Расширение
		/// </summary>
		public string Extension{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public FileLink()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1342177258; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1342177258; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Файлы"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1342177258);
			}
		}


	}
}