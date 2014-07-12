using System;
using Sbor.Interfaces;

namespace Sbor.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDocSGE: IHasPeriod
    {

        /// <summary>
        /// ППО
        /// </summary>
        int IdPublicLegalFormation { get; set; }
        BaseApp.Reference.PublicLegalFormation PublicLegalFormation { get; set; }


        /// <summary>
        /// Версия
        /// </summary>
        int IdVersion { get; set; }
        BaseApp.Reference.Version Version { get; set; }


        /// <summary>
        /// Тип документа
        /// </summary>
        int IdDocType { get; set; }
        Sbor.Reference.DocType DocType { get; set; }

        /// <summary>
        /// Номер
        /// </summary>
        string Number { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// Системная дата утверждения
        /// </summary>
        DateTime? DateCommit { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        int IdDocStatus { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        Int32 Id { get; set; }

        /// <summary>
        /// Предыдущая редакция
        /// </summary>
        int? IdParent { get; }

        DateTime ParentDate { get; }
        DateTime ParentDateStart { get; }
        DateTime ParentDateEnd { get; }
        DateTime? ParentDateCommit { get; }

        /// <summary>
        /// Вести доп. потребности
        /// </summary>
        bool HasAdditionalNeed { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        string Caption { get; set; }

        /// <summary>
        /// Наименование документа
        /// </summary>
        string Header { get; set; }

        /// <summary>   
        /// Получение массива идентификаторов документов всех версий этого документа, включая его самого
        /// </summary>         
        int[] AllVersionDocIds { get; }
    }
}

