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

namespace Platform.BusinessLogic.Registry
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Сериализованный элемент сущности
	/// </summary>
	public partial class SerializedEntityItem : RegistryEntity      
	{
	
		/// <summary>
		/// Сериализованные данные
		/// </summary>
		[Column(TypeName="xml")] 
		public string Data{get; set;}
		[NotMapped]
		public XDocument DataWrapper 
		{
			get { return XDocument.Parse(Data); }
			set { Data = value.ToString(); }
		}

		/// <summary>
		/// Документ
		/// </summary>
		public int IdTool{get; set;}

		/// <summary>
		/// Документ: тип документа
		/// </summary>
		public int IdToolEntity{get; set;}
        /// <summary>
	    /// Документ: тип документа
	    /// </summary>
		public virtual Entity ToolEntity{get; set;}
		

		/// <summary>
		/// Идентификатор
		/// </summary>
		public Int32 Id{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public SerializedEntityItem()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2080374746; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2080374746; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Сериализованный элемент сущности"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2080374746);
			}
		}


	}
}