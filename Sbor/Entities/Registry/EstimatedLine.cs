using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Sbor.Tablepart;

namespace Sbor.Registry
{
 	/// <summary>
	/// Сметные строки
	/// </summary>
    public partial class EstimatedLine : ILineCostWithRelations
	{
        public override string ToString()
        {
            //Правило формирования поля «Наименование»:
            //Вид бюджетной деятельности [Вид бюджетной деятельности], Тип РО [Тип РО], ИФ [Источник финансирования], КФО [КФО], КВСР [КВСР], РзПР [РзПР], КЦСР [КЦСР], КВР [КВР], КОСГУ [КОСГУ], ДФК [ДФК], ДКР [ДКР], ДЭК [ДЭК], Код субсидии [Код субсидии],Отраслевой код [Отраслевой код].
            //Если код БК не указан (пусто), то в наименовании этот код пропускать.
            //Пример заполнения поля «Наименование»: Вид бюджетной деятельности Расходы, Тип РО Действующее, ИФ 03, КФО 1, КВСР 111, РзПР 07.09, КЦСР 512.97.00, КВР 121

            var texts = new List<string>();

            texts.Add("Вид бюджетной деятельности " + ActivityBudgetaryType.Caption());
            if (ExpenseObligationType.HasValue)
                texts.Add("Тип РО " + ExpenseObligationType.Value.Caption());
            if (IdFinanceSource.HasValue)
                texts.Add(FinanceSource.ToString());
            if (IdKFO.HasValue)
                texts.Add(KFO.ToString());
            if (IdKVSR.HasValue)
                texts.Add(KVSR.ToString());
            if (IdRZPR.HasValue)
                texts.Add( RZPR.ToString());
            if (IdKCSR.HasValue)
                texts.Add( KCSR.ToString() );
            if (IdKVR.HasValue)
                texts.Add(KVR.ToString());
            if (IdKOSGU.HasValue)
                texts.Add(KOSGU.ToString());
            if (IdDFK.HasValue)
                texts.Add(DFK.ToString());
            if (IdDKR.HasValue)
                texts.Add(DKR.ToString());
            if (IdDEK.HasValue)
                texts.Add(DEK.ToString());
            if (IdCodeSubsidy.HasValue)
                texts.Add(CodeSubsidy.ToString());
            if (IdBranchCode.HasValue)
                texts.Add(BranchCode.ToString());

            return string.Join(", ", texts);
        }

	}
}