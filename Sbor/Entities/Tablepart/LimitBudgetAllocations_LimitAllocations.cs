using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Attributes;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Common.Extensions;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Registry;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Tablepart
{
    [SelectionWithNoChilds]
    public partial class LimitBudgetAllocations_LimitAllocations : ILineCostWithRelations, IDenormilizedExpense
    {
        /// <summary>
        /// Действие при вставке новой строки
        /// </summary>
        /// <param name="context"></param>
        [Control(ControlType.Insert, Sequence.After, ExecutionOrder = -1000)]
        public void AutoInsert(DataContext context)
        {
            //Проверить сметные строки в ТЧ «Предельные объемы бюджетных ассигнований» на уникальность. 
            //Уникальность проверять по всем полям строки кроме «ОФГ», «ПФГ-1», «ПФГ-2».
            var dublicate = context.LimitBudgetAllocations_LimitAllocations.FirstOrDefault(l =>
                                                                l.IdOwner == IdOwner &&
                                                                l.Id != Id &&
                                                                l.IdCodeSubsidy == IdCodeSubsidy &&
                                                                l.IdDEK == IdDEK &&
                                                                l.IdDFK == IdDFK &&
                                                                l.IdDKR == IdDKR &&
                                                                l.IdExpenseObligationType == IdExpenseObligationType &&
                                                                l.IdFinanceSource == IdFinanceSource &&
                                                                l.IdKCSR == IdKCSR &&
                                                                l.IdKFO == IdKFO &&
                                                                l.IdKOSGU == IdKOSGU &&
                                                                l.IdKVR == IdKVR &&
                                                                l.IdKVSR == IdKVSR &&
                                                                l.IdRZPR == IdRZPR);

            //Если найдены неуникальные строки, то действие не выполнять и выдать сообщение
            if (dublicate != null)
            {
                var msg = '-' +
                          String.Format("{0}{1}{2}{3}{4}{5}{6} <br/>",
                                        dublicate.IdExpenseObligationType.HasValue
                                            ? ("Тип РО " + (ExpenseObligationType)dublicate.IdExpenseObligationType)
                                            : String.Empty,
                                        dublicate.FinanceSource != null
                                            ? (", ИФ " + dublicate.FinanceSource.Code)
                                            : String.Empty,
                                        dublicate.KFO != null ? (", КФО " + dublicate.KFO.Code) : String.Empty,
                                        dublicate.KVSR != null ? (", КВСР " + dublicate.KVSR.Caption) : String.Empty,
                                        dublicate.RZPR != null ? (", РзПР " + dublicate.RZPR.Code) : String.Empty,
                                        dublicate.KCSR != null ? (", КЦСР " + dublicate.KCSR.Code) : String.Empty,
                                        dublicate.KVR != null ? (", КВР " + dublicate.KVR.Code) : String.Empty)
                                .TrimStart(',')
                                .Trim();


                Controls.Throw(
                    "В таблице «Предельные объемы бюджетных ассигнований» уже имеется строка с заданными параметрами:<br/>" + msg);
            }
        }

        internal IEnumerable<LimitVolumeAppropriations> GetLimitVolumeAppropriations(DataContext context, SBP_Blank blank, FindParamEstimatedLine findParamEstimatedLine, LimitBudgetAllocations document, byte idValueType, int? idParent)
        {
            var result = new List<LimitVolumeAppropriations>();

            var allocations = new decimal[3];
            allocations[0] = OFG ?? 0;
            allocations[1] = PFG1 ?? 0;
            allocations[2] = PFG2 ?? 0;

            var estimatedLineId = this.GetLineId(context, document.Id, document.EntityId, blank, findParamEstimatedLine);
            if (!estimatedLineId.HasValue)
                throw new Exception("Сметная строка не определенна");

            var budgetYear = document.Budget.Year;
            var hierarchyPeriodIds = new []{budgetYear, budgetYear + 1, budgetYear + 2};
            hierarchyPeriodIds = hierarchyPeriodIds.Select(y => y.GetIdHierarchyPeriodYear(context)).ToArray();

            if (idParent.HasValue)
                allocations = FindDifferencesForReg(context, allocations.ToArray(), hierarchyPeriodIds, estimatedLineId.Value, idValueType, idParent.Value).ToArray();

            var i = -1;
            foreach (var allocation in allocations)
            {
                i++;
              /*  if (allocation == 0)
                    continue;*/

                var dataLine = new LimitVolumeAppropriations()
                {
                    IdPublicLegalFormation = document.IdPublicLegalFormation,
                    IdVersion = document.IdVersion,
                    IdBudget = document.IdBudget,
                    IdEstimatedLine = estimatedLineId.Value,
                    IdAuthorityOfExpenseObligation = null,
                    IdTaskCollection = null,
                    IsIndirectCosts = false,
                    IdHierarchyPeriod = hierarchyPeriodIds[i],
                    IdValueType = idValueType,
                    Value = allocation,
                    IdOKATO = null,
                    IsMeansAUBU = false,
                    IdRegistrator = document.Id,
                    DateCommit = document.DateCommit,
                    IdApproved = document.Id,
                    IdApprovedEntity = document.EntityId,
                    DateCreate = DateTime.Now,
                    IdRegistratorEntity = document.EntityId
                };

                result.Add(dataLine);
            }

            return result;
        }

        private IEnumerable<decimal> FindDifferencesForReg(DataContext context, decimal[] values, int[] periods, int estimatedLineId, int idValueType, int idParent)
        {
            var result = new decimal[3];

            var regValues = context.LimitVolumeAppropriations.Where(
                l => l.IdEstimatedLine == estimatedLineId && l.IdValueType == idValueType && l.IdRegistrator == idParent && l.IdRegistratorEntity == LimitBudgetAllocations.EntityIdStatic )
                                   .GroupBy(l => new {l.IdEstimatedLine, l.IdHierarchyPeriod})
                                   .Select(g => new
                                       {
                                           idPeriod = g.Key.IdHierarchyPeriod,
                                           sumValue = g.Sum(c => (decimal?) c.Value),
                                       })
                                   .ToDictionary(l => l.idPeriod, l => l.sumValue);
            
            for (int i = 0; i < 3; i++)
            {
                result[i] = ( values[i] - ( regValues.ContainsKey(periods[i]) ? regValues[periods[i]] : 0 ) ) ?? 0;
            }

            return result;
        }

        public string GetFullCaption()
        {
            var msg = new StringBuilder();

            if (ExpenseObligationType != null)
                msg.AppendFormat("Тип РО  {0}", ExpenseObligationType.Caption());

            if (FinanceSource != null)
                msg.AppendFormat(", ИФ {0}", FinanceSource.Code);

            if (KFO != null)
                msg.AppendFormat(", КФО {0}", KFO.Code);

            if ( KVSR != null)
                msg.AppendFormat(", КВСР {0}", KVSR.Caption);

            if ( RZPR != null)
                msg.AppendFormat(", РЗПР {0}", RZPR.Code);

            if (KCSR != null)
                msg.AppendFormat(", КЦСР {0}", KCSR.Code);

            if (KVR != null)
                msg.AppendFormat(", КВР {0}", KVR.Code);

            if (KOSGU != null)
                msg.AppendFormat(", КОСГУ {0}", KOSGU.Code);

            if (DFK != null)
                msg.AppendFormat(", ДФК {0}", DFK.Code);

            if (DKR != null)
                msg.AppendFormat(", ДКР {0}", DKR.Code);

            if (DEK != null)
                msg.AppendFormat(", ДЕК {0}", DEK.Code);

            if (CodeSubsidy != null)
                msg.AppendFormat(", Код субсидии {0}", CodeSubsidy.Code);

            if (BranchCode != null)
                msg.AppendFormat(", Отраслевой код {0}", BranchCode.Code);

            return msg.ToString().Trim(',').Trim();
        }


    }
}
