using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Sbor.Logic
{
    public interface ITpSystemGoalElement : IIdentitied
    {
        /// <summary>
        /// Ссылка на владельца
        /// </summary>
        int IdOwner { get; set;  }

        /// <summary>
        /// Вышестоящий
        /// </summary>
        int? IdParent { get; set; }

        /// <summary>
        /// Из другого документа СЦ
        /// </summary>
        bool FromAnotherDocumentSE { get; set;  }

        /// <summary>
        /// Наименование
        /// </summary>
        int IdSystemGoal { get; set;  }
        Sbor.Reference.SystemGoal SystemGoal { get; }


        /// <summary>
        /// Основная цель
        /// </summary>
        bool IsMainGoal { get; set; }

        /// <summary>
        /// Тип
        /// </summary>
        int? IdElementTypeSystemGoal { get; set; }
        Sbor.Reference.ElementTypeSystemGoal ElementTypeSystemGoal { get; }


        /// <summary>
        /// Срок реализации с
        /// </summary>
        DateTime? DateStart { get; set; }

        /// <summary>
        /// Срок реализации по
        /// </summary>
        DateTime? DateEnd { get; set; }

        /// <summary>
        /// Ответственный исполнитель
        /// </summary>
        int? IdSBP { get; set; }
        Sbor.Reference.SBP SBP { get; }

        DateTime ? ParentDateStart { get; }
        DateTime ? ParentDateEnd { get; }
        
        int ? ParentIdElementTypeSystemGoal { get; }
        Sbor.Reference.ElementTypeSystemGoal ParentElementTypeSystemGoal { get; }
    }
}
