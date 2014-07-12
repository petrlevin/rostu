using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.Report
{
    public interface IBudgetExpenseStructureFilter
    {
        byte? IdFilterFieldType_ExpenseObligationType { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_ExpenseObligationType { get; }

        /// <summary>
        /// Тип фильтра по полю Источники финансирования
        /// </summary>
        byte? IdFilterFieldType_FinanceSource { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_FinanceSource { get; }

        /// <summary>
        /// Тип фильтра по полю КФО
        /// </summary>
        byte? IdFilterFieldType_KFO { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_KFO { get; }

        /// <summary>
        /// Тип фильтра по полю КВСР/КАДБ/КАИФ
        /// </summary>
        byte? IdFilterFieldType_KVSR { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_KVSR { get; }

        /// <summary>
        /// Тип фильтра по полю РзПР
        /// </summary>
        byte? IdFilterFieldType_RZPR { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_RZPR { get; }

        /// <summary>
        /// Тип фильтра по полю КЦСР
        /// </summary>
        byte? IdFilterFieldType_KCSR { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_KCSR { get; }

        /// <summary>
        /// Тип фильтра по полю КВР
        /// </summary>
        byte? IdFilterFieldType_KVR { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_KVR { get; }

        /// <summary>
        /// Тип фильтра по полю КОСГУ
        /// </summary>
        byte? IdFilterFieldType_KOSGU { get; }

        Sbor.DbEnums.FilterFieldType? FilterFieldType_KOSGU { get; }

        /// <summary>
        /// Тип фильтра по полю ДФК
        /// </summary>
        byte? IdFilterFieldType_DFK { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_DFK { get; }

        /// <summary>
        /// Тип фильтра по полю ДКР
        /// </summary>
        byte? IdFilterFieldType_DKR { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_DKR { get; }

        /// <summary>
        /// Тип фильтра по полю ДЭК
        /// </summary>
        byte? IdFilterFieldType_DEK { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_DEK { get; }

        /// <summary>
        /// Тип фильтра по полю Коды субсидий
        /// </summary>
        byte? IdFilterFieldType_CodeSubsidy { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_CodeSubsidy { get; }

        /// <summary>
        /// Тип фильтра по полю Отраслевые коды
        /// </summary>
        byte? IdFilterFieldType_BranchCode { get; }
        Sbor.DbEnums.FilterFieldType? FilterFieldType_BranchCode { get; }

        /// <summary>
        /// Типы РО
        /// </summary>
        int[] ExpenseObligationTypeIds { get; }

        ICollection<Sbor.Reference.FinanceSource> FinanceSource { get; }

        ICollection<Sbor.Reference.KFO> KFO { get; }
        ICollection<Sbor.Reference.KVSR> KVSR { get; }
        ICollection<Sbor.Reference.RZPR> RZPR { get; }
        ICollection<Sbor.Reference.KCSR> KCSR { get; }
        ICollection<Sbor.Reference.KVR> KVR { get; }
        ICollection<Sbor.Reference.KOSGU> KOSGU { get; }
        ICollection<Sbor.Reference.DFK> DFK { get; }

        ICollection<Sbor.Reference.DKR> DKR { get; }

        ICollection<Sbor.Reference.DEK> DEK { get; }

        ICollection<Sbor.Reference.CodeSubsidy> CodeSubsidy { get; }

        ICollection<Sbor.Reference.BranchCode> BranchCode { get; }
    }
}
