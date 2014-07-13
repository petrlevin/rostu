using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.PrimaryEntities.Reference;
namespace Platform.BusinessLogic.Reference
{

    /// <summary>
    /// Контроли
    /// </summary>
    public class Control : ReferenceEntity, IControlInfo
    {

        /// <summary>
        /// Идентификатор
        /// </summary>
        public override Int32 Id { get; set; }

        /// <summary>
        /// Включен
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Мягкий
        /// </summary>
        public bool Skippable { get; set; }

        /// <summary>
        /// Английское наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Русское наименование
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// УНК
        /// </summary>
        public string UNK { get; set; }

        /// <summary>
        /// Сущность
        /// </summary>
        public int? IdEntity { get; set; }
        public virtual Entity Entity { get; set; }

        [NotMapped]
        public bool HasDbEntry
        {
            get { return true; }
        }

        public bool Managed { get; set; }


        public Control()
        {
        }

        /// <summary>
        /// Идентификатор типа сущности
        /// </summary>
        public override int EntityId
        {
            get { return -1744830437; }
        }

        /// <summary>
        /// Идентификатор типа сущности
        /// </summary>
        public new static int EntityIdStatic
        {
            get { return -1744830437; }
        }

        /// <summary>
        /// Русское наименование типа сущности
        /// </summary>
        public override string EntityCaption
        {
            get { return "Контроли"; }
        }




        //private RelationshipManager _relationships;

        //RelationshipManager IEntityWithRelationships.RelationshipManager
        //{
        //    get
        //    {
        //        if (_relationships == null)
        //        {
        //            _relationships = RelationshipManager.Create(this);
        //        }
        //
        //        return _relationships;
        //    }
        //}


        bool IControlInfo.Enabled
        {
            get { return Enabled; }
        }

        bool IControlInfo.Skippable
        {
            get { return Skippable; }
        }

        string IControlInfo.Caption
        {
            get { return Caption; }
        }

        string IControlInfo.UNK
        {
            get { return UNK; }
        }

        Int32? IControlInfo.IdEntity
        {
            get { return IdEntity; }
        }
    }
}