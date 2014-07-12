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



namespace Sbor.Tablepart
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// История применения правил
	/// </summary>
	public partial class BalancingIFDB_ChangeHistory : TablePartEntity      
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
		public virtual Sbor.Tool.BalancingIFDB Owner{get; set;}
		

		/// <summary>
		/// Сметная строка
		/// </summary>
		public int? IdMaster{get; set;}
        /// <summary>
	    /// Сметная строка
	    /// </summary>
		public virtual Sbor.Tablepart.BalancingIFDB_EstimatedLine Master{get; set;}
		

		/// <summary>
		/// Правило
		/// </summary>
		public int? IdBalancingIFDB_RuleIndex{get; set;}
        /// <summary>
	    /// Правило
	    /// </summary>
		public virtual Sbor.Tablepart.BalancingIFDB_RuleIndex BalancingIFDB_RuleIndex{get; set;}
		

		/// <summary>
		/// Старое значение
		/// </summary>
		public decimal? OldValue{get; set;}

	

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public BalancingIFDB_ChangeHistory()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2013265295; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public static int EntityIdStatic
		{
			get { return -2013265295; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "История применения правил"; }
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
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,-2013265295);
			}
		}


	}
}