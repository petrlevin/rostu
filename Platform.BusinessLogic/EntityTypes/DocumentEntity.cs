using System;
using Platform.BusinessLogic.EntityTypes.Interfaces;

namespace Platform.BusinessLogic.EntityTypes
{
    /// <summary>
    /// Базовый класс для документов
    /// </summary>

    public abstract class DocumentEntity<TEntity> : ToolEntity<TEntity>, IDocumentEntity
                        where TEntity : ToolEntity
    {
        /// <summary>
        /// Номер документа
        /// </summary>
        public virtual string Number { get; set; }

        /// <summary>
        /// Дата документа
        /// </summary>
        public virtual DateTime Date { get; set; }

        public override string ToString()
        {
            return String.Format("{0} № {1} от {2}", DocumentCaption, Number, Date.ToString("dd.MM.yyyy"));
        }


        /// <summary>
        /// Наименование типа документа
        /// </summary>
        public virtual string DocumentCaption 
        {
            get
            {
                return EntityCaption;
            }
        }
        
    }

}
