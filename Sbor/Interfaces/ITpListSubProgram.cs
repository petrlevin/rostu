using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Sbor.Logic
{
    interface ITpListSubProgram : IIdentitied
    {
        /// <summary>
        /// Ссылка на владельца
        /// </summary>
        int IdOwner { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        int IdSystemGoal { get; set; }
        Sbor.Reference.SystemGoal SystemGoal { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        int IdAnalyticalCodeStateProgramValue { get; set; }

        /// <summary>
        /// Тип ответственного исполнителя
        /// </summary>
        int IdResponsibleExecutantType { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        int IdDocType { get; }

        /// <summary>
        /// Ответственный исполнитель
        /// </summary>
        int IdSBP { get; set; }

        /// <summary>
        /// Срок реализации с
        /// </summary>
        DateTime? DateStart { get; set; }

        /// <summary>
        /// Срок реализации по
        /// </summary>
        DateTime? DateEnd { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        string Caption { get; set; }
        
        /// <summary>
        /// Ссылка на порожденный документ
        /// </summary>
        int? IdDocument { get; set; }
        
        /// <summary>
        /// Id сущности порожденного элемента
        /// </summary>
        int? IdDocumentEntity { get; set; }
    }
}
