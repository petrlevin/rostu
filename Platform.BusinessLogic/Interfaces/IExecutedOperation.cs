using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Interfaces
{
    public interface IExecutedOperation : IBaseEntity, IIdentitied
    {

        /// <summary>
        /// Время  операции
        /// </summary>
        DateTime? Date{ get; set; }


        /// <summary>
        /// Пользователь
        /// </summary>
        int IdUser { get; set; }


        /// <summary>
        /// Операция
        /// </summary>
        int IdEntityOperation { get; set; }

        Reference.EntityOperation EntityOperation { get; set; }


        /// <summary>
        /// Исходный статус
        /// </summary>
        int IdOriginalStatus { get; set; }

        Reference.DocStatus OriginalStatus { get; set; }

        /// <summary>
        /// Конечный статус
        /// </summary>
        int IdFinalStatus { get; set; }

        Reference.DocStatus FinalStatus { get; set; }
    }
}
