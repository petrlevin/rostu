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



namespace Sbor.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Скорость соединения
	/// </summary>
	public partial class UserBandWidth : ReferenceEntity      
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		
        public override Int32 Id{get; set;}

		/// <summary>
		/// Пользователь
		/// </summary>
		public int IdUser{get; set;}
        /// <summary>
	    /// Пользователь
	    /// </summary>
		public virtual BaseApp.Reference.User User{get; set;}
		

		/// <summary>
		/// Ping, мс
		/// </summary>
		public Int32 Ping{get; set;}

		/// <summary>
		/// Скорость скачивания, Мбайт/сек
		/// </summary>
		public decimal DownloadSpeed{get; set;}

		/// <summary>
		/// Дата измерения
		/// </summary>
		private DateTime _Date; 
        /// <summary>
	    /// Дата измерения
	    /// </summary>
		public  DateTime Date 
		{
			get{ return _Date.Date; }
			set{ _Date = value.Date; }
		}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public UserBandWidth()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -1207959510; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -1207959510; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Скорость соединения"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-1207959510);
			}
		}


	}
}