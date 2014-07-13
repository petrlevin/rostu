using System;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference
{

	/// <summary>
	/// ИсключенияКонтролей
	/// </summary>
    public class Control_Exceptions : ReferenceEntity, IControlInfo
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>
		public override Int32 Id{get; set;}

		/// <summary>
		/// Владелец
		/// </summary>
		public int IdOwner{get; set;}
		public virtual Platform.BusinessLogic.Reference.Control Owner{get; set;}
		

		/// <summary>
		/// ППО
		/// </summary>
		public int IdPublicLegalFormation{get; set;}
		public virtual BaseApp.Reference.PublicLegalFormation PublicLegalFormation{get; set;}
		

		/// <summary>
		/// Бюджет
		/// </summary>
		public int? IdBudget{get; set;}
		public virtual BaseApp.Reference.Budget Budget{get; set;}
		

		/// <summary>
		/// Включен
		/// </summary>
		public bool Enabled{get; set;}

		/// <summary>
		/// Мягкий
		/// </summary>
		public bool Skippable{get; set;}

	    public string Caption
	    {
	        get { return Owner.Caption; }
	    }

	    public string UNK
	    {
	        get { return Owner.UNK; }
	    }

	    public Int32? IdEntity
	    {
	        get { return Owner.IdEntity; }
	    }

        [NotMapped]
	    public bool HasDbEntry
	    {
	        get { return true; }
	    }

		/// <summary>
        /// Идентификатор типа сущности
        /// </summary>
		public override int EntityId
        {
            get { return -1744830433; }
        }

		/// <summary>
        /// Идентификатор типа сущности
        /// </summary>
		public new static int EntityIdStatic
        {
            get { return -1744830433; }
        }

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
        {
            get { return "ИсключенияКонтролей"; }
        }

	}
}