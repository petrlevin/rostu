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

namespace Sbor.Registry
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// TestReg2
	/// </summary>
	public partial class TestReg2 : RegistryEntity  , IHasCommonRegistrator    
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		public Int32 Id{get; set;}

		/// <summary>
		/// idRegistrator
		/// </summary>
		public int IdRegistrator{get; set;}

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
		public int IdRegistratorEntity{get; set;}
        /// <summary>
	    /// Ссылка на сущность
	    /// </summary>
		public virtual Entity RegistratorEntity{get; set;}
		

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TestReg2()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1744830427; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1744830427; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "TestReg2"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830427);
			}
		}


	}
}