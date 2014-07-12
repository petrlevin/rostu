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
	/// TestReg1
	/// </summary>
	public partial class TestReg1 : RegistryEntity  , IHasRegistrator , IHasCommonTerminator  , IRegistryWithOperation , IRegistryWithTermOperation
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		public Int32 Id{get; set;}

		/// <summary>
		/// Регистратор
		/// </summary>
		public int IdRegistrator{get; set;}
        /// <summary>
	    /// Регистратор
	    /// </summary>
		public virtual Sbor.Document.TestDocument Registrator{get; set;}
		

		/// <summary>
		/// Операция
		/// </summary>
		public int? IdExecutedOperation{get; set;}
        /// <summary>
	    /// Операция
	    /// </summary>
		public virtual BaseApp.Registry.ExecutedOperation ExecutedOperation{get; set;}
		

		/// <summary>
		/// Text
		/// </summary>
		public string Text{get; set;}

		/// <summary>
		/// Анулятор
		/// </summary>
		public int? IdTerminator{get; set;}

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
		public int? IdTerminatorEntity{get; set;}
        /// <summary>
	    /// Ссылка на сущность
	    /// </summary>
		public virtual Entity TerminatorEntity{get; set;}
		

		/// <summary>
		/// Операция анулирования
		/// </summary>
		public int? IdTerminateOperation{get; set;}
        /// <summary>
	    /// Операция анулирования
	    /// </summary>
		public virtual BaseApp.Registry.ExecutedOperation TerminateOperation{get; set;}
		

		/// <summary>
		/// Дата прекращения
		/// </summary>
		private DateTime? _DateTerminate; 
        /// <summary>
	    /// Дата прекращения
	    /// </summary>
		public  DateTime? DateTerminate 
		{
			get{ return _DateTerminate != null ? ((DateTime)_DateTerminate).Date : (DateTime?)null; }
			set{ _DateTerminate = value != null ? ((DateTime)value).Date : value; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public TestReg1()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1744830428; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1744830428; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "TestReg1"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1744830428);
			}
		}


	}
}