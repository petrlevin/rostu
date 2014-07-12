using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;
using SBPTyp = Sbor.DbEnums.SBPType;
using BlankTyp = Sbor.DbEnums.BlankType;
using BlankValTyp = Sbor.DbEnums.BlankValueType;
using Platform.Utils.Extensions;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Tablepart
{
    public partial class SBP_Blank : ISBP_Blank
	{
        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.SBP.SingleOrDefault(a => a.Id == IdOwner);

            if (Budget == null)
                Budget = context.Budget.SingleOrDefault(a => a.Id == IdBudget);
        }

        /// <summary>   
        /// Контроль "Проверка последовательности создания бланков"
        /// </summary>
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 60)]
        public void Control_500506(DataContext context)
        {
            InitMaps(context);

            var otherBlanks = 
                context.SBP_Blank.Where(w => w.IdOwner == IdOwner && w.IdBudget == IdBudget && w.Id != Id).ToList();

            string CapBlank = string.Empty;

            if (BlankType == BlankTyp.BringingRBS)
            {
                var bt = BlankTyp.BringingGRBS;
                if (!otherBlanks.Any(a => a.IdBlankType == (byte)bt)) CapBlank = bt.Caption();
            }
            else if (BlankType == BlankTyp.BringingKU && Owner.SBPType == DbEnums.SBPType.GeneralManager)
            {
                var bt = BlankTyp.BringingGRBS;
                if (!otherBlanks.Any(a => a.IdBlankType == (byte)bt)) CapBlank = bt.Caption();
            }
            else if (BlankType == BlankTyp.FormationKU)
            {
                var bt = BlankTyp.BringingKU;
                if (!otherBlanks.Any(a => a.IdBlankType == (byte)bt)) CapBlank = bt.Caption();
            }
            else if (BlankType == BlankTyp.FormationAUBU)
            {
                var bt = BlankTyp.BringingAUBU;
                if (!otherBlanks.Any(a => a.IdBlankType == (byte)bt)) CapBlank = bt.Caption();
            }
            else if (BlankType == BlankTyp.FormationGRBS)
            {
                var bt = BlankTyp.BringingGRBS;
                if (!otherBlanks.Any(a => a.IdBlankType == (byte)bt)) CapBlank = bt.Caption();
            }
            else
            {
                return;
            }

            if (!string.IsNullOrEmpty(CapBlank))
            {
                Controls.Throw(string.Format(
                    "Нарушена последовательность создания бланков. Для сохранения текущего бланка требуется предварительно создать бланк «{0}» с бюджетом «{1}» у СБП «{2}»",
                    CapBlank,
                    Budget.Caption,
                    Owner.Caption
                ));
            }
        }

        /// <summary>   
        /// Контроль "Наличие обязательного поля в бланке"
        /// </summary>
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 70)]
        public void Control_500507(DataContext context)
        {
            // переделать на динамическое определение полей для проверки, см. функцию getListFieldsBlank и свойства listFieldsBlank
            // если вообще не удалить.... контроль бесполезен, так как одно из полей нередактируется и обязательное
            // аналитики пока думают над этим
            if (
                BlankValueType_FinanceSource != BlankValTyp.Mandatory
                && BlankValueType_KFO != BlankValTyp.Mandatory
                && BlankValueType_KVSR != BlankValTyp.Mandatory
                && BlankValueType_RZPR != BlankValTyp.Mandatory
                && BlankValueType_KCSR != BlankValTyp.Mandatory
                && BlankValueType_KVR != BlankValTyp.Mandatory
                && BlankValueType_KOSGU != BlankValTyp.Mandatory
                && BlankValueType_DFK != BlankValTyp.Mandatory
                && BlankValueType_DKR != BlankValTyp.Mandatory
                && BlankValueType_DEK != BlankValTyp.Mandatory
                && BlankValueType_CodeSubsidy != BlankValTyp.Mandatory
                && BlankValueType_ExpenseObligationType != BlankValTyp.Mandatory
            )
                Controls.Throw("Хотя бы одно поле в бланке должно быть обязательным");
        }

        #region Настройка соответсвия бланков

        /*
            Доведение ГРБС  Доведение РБС
            Доведение ГРБС  Доведение КУ у ГРБС
            Доведение ГРБС  Формирование ГРБС
            Доведение РБС (вышестоящий СБП)  Доведение КУ у РБС
            Доведение КУ  Формирование КУ
            Доведение АУ/БУ  Формирование АУ/БУ
            Формирование ГРБС  Формирование КУ
        */

        private class depBlank
        {
            public BlankTyp superiorType;
            public BlankTyp juniorType;
            public SBPTyp?  sbpType;
            public bool     inHierarhySBP;
        };

        List<depBlank> dependingBlanks = new List<depBlank>()
        {
            new depBlank() { superiorType = BlankTyp.BringingGRBS, juniorType = BlankTyp.BringingRBS,   sbpType = null,                  inHierarhySBP = false },
            new depBlank() { superiorType = BlankTyp.BringingGRBS, juniorType = BlankTyp.BringingKU,    sbpType = SBPTyp.GeneralManager, inHierarhySBP = false },
            new depBlank() { superiorType = BlankTyp.BringingGRBS, juniorType = BlankTyp.FormationGRBS, sbpType = null,                  inHierarhySBP = false },
            new depBlank() { superiorType = BlankTyp.BringingRBS,  juniorType = BlankTyp.BringingKU,    sbpType = SBPTyp.Manager,        inHierarhySBP = true  },
            new depBlank() { superiorType = BlankTyp.BringingKU,   juniorType = BlankTyp.FormationKU,   sbpType = null,                  inHierarhySBP = false },
            new depBlank() { superiorType = BlankTyp.BringingAUBU, juniorType = BlankTyp.FormationAUBU, sbpType = null,                  inHierarhySBP = false },
            new depBlank() { superiorType = BlankTyp.FormationGRBS, juniorType = BlankTyp.FormationKU, sbpType = null,                  inHierarhySBP = false }
        };

        /* список полей для проверки */

        Dictionary<string, string> listFieldsBlank = null;

        private Dictionary<string, string> getListFieldsBlank(DataContext context)
        {
            var name = typeof(BlankValTyp).Name;
            return 
                context.EntityField.Where(w => 
                    w.IdEntity == EntityId && context.Entity.Any(a =>  a.Id == w.IdEntityLink && a.Name == name) 
                ).ToList().ToDictionary(a => a.Name.FirstUpper(), b => b.Caption);
        }

        /*
            Обяз (выш)    Обяз (ниж)
            Необяз(выш)   Обяз или Необяз(ниж)
            Пустое (выш)  Пустое или Обяз или Необяз (ниж)
        */

        private class depField
        {
            public BlankValTyp? junior;
            public BlankValTyp? superior;
        };

        List<depField> dependingFields = new List<depField>()
        {
            new depField() { superior = BlankValTyp.Mandatory, junior = BlankValueType.Mandatory },
            new depField() { superior = BlankValTyp.Optional,  junior = BlankValueType.Mandatory },
            new depField() { superior = BlankValTyp.Optional,  junior = BlankValueType.Optional  },
            new depField() { superior = BlankValTyp.Empty,     junior = BlankValueType.Mandatory },
            new depField() { superior = BlankValTyp.Empty,     junior = BlankValueType.Optional  },
            new depField() { superior = BlankValTyp.Empty,     junior = BlankValueType.Empty     }
        };

        #endregion

        /// <summary>   
        /// При сохранении/изменении строки в ТЧ «Бланки доведения и формирования» создавать строку в ТЧ «История создания бланков доведения и формирования».
        /// </summary>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 10)]
        public void CreateHistory(DataContext context)
        {
            SBP_BlankHistory newSBP_BlankHistory = new SBP_BlankHistory()
                {
                    IdOwner = this.IdOwner,
                    DateCreate = DateTime.Now,
                    IdBudget = this.IdBudget,
                    IdBlankType = this.IdBlankType,
                    IdBlankValueType_ExpenseObligationType = this.IdBlankValueType_ExpenseObligationType,
                    IdBlankValueType_FinanceSource = this.IdBlankValueType_FinanceSource,
                    IdBlankValueType_KFO = this.IdBlankValueType_KFO,
                    IdBlankValueType_BranchCode = this.IdBlankValueType_BranchCode,
                    IdBlankValueType_CodeSubsidy = this.IdBlankValueType_CodeSubsidy,
                    IdBlankValueType_KOSGU = this.IdBlankValueType_KOSGU,
                    IdBlankValueType_KCSR = this.IdBlankValueType_KCSR,
                    IdBlankValueType_RZPR = this.IdBlankValueType_RZPR,
                    IdBlankValueType_KVR = this.IdBlankValueType_KVR,
                    IdBlankValueType_KVSR = this.IdBlankValueType_KVSR,
                    IdBlankValueType_DEK = this.IdBlankValueType_DEK,
                    IdBlankValueType_DFK = this.IdBlankValueType_DFK,
                    IdBlankValueType_DKR = this.IdBlankValueType_DKR
                };
            context.SBP_BlankHistory.Add(newSBP_BlankHistory);
            context.SaveChanges();
        }

        /// <summary>   
        /// Контроль "Проверка соответствия бланков при сохранении нижестоящего бланка"
        /// </summary>
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 80)]
        public void Control_500508(DataContext context)
        {
            InitMaps(context);

            if (listFieldsBlank == null)
                listFieldsBlank = getListFieldsBlank(context);

            var deps = dependingBlanks.Where(w => w.juniorType == BlankType);
            foreach (depBlank dep in deps)
            {
                if (dep.sbpType.HasValue && Owner.SBPType != dep.sbpType.Value)
                    continue;

                List<string> listErr = new List<string>();

                SBP_Blank junior = this;
                SBP_Blank superior = context.SBP_Blank.SingleOrDefault(s =>
                    s.IdOwner == (dep.inHierarhySBP ? Owner.IdParent : IdOwner)
                    && s.IdBlankType == (byte)dep.superiorType
                    && s.IdBudget == this.IdBudget
                );

                if (superior == null)
                    continue;

                foreach (KeyValuePair<string, string> f in listFieldsBlank)
                {
                    var p1 = junior.GetValue(f.Key)   ?? BlankValTyp.Empty;
                    var p2 = superior.GetValue(f.Key) ?? BlankValTyp.Empty;

                    List<depField> listDeps = dependingFields.Where(w => w.superior == (BlankValTyp)p2).ToList();

                    if (!listDeps.Any(w => w.junior == (BlankValTyp)p1))
                    {
                        string txt = listDeps.OrderBy(o => (byte)o.junior).Select(k => k.junior.Caption()).Distinct().ToArray().ToString(" или ");
                        listErr.Add(" - " + f.Value + " - " + txt);
                    }
                }

                if (listErr.Any())
                {
                    Controls.Check(listErr, string.Format(
                        "Бланк «{0}» не соответствует бланку «{1}» у СБП «{2}».<br>Должно быть:<br>{{0}}",
                        junior.BlankType.Caption(),
                        superior.BlankType.Caption(),
                        superior.Owner.Caption
                    ));
                }
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия бланков при изменении вышестоящего бланка"
        /// </summary>
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 90)]
        public void Control_500509(DataContext context)
        {
            InitMaps(context);

            if (listFieldsBlank == null)
                listFieldsBlank = getListFieldsBlank(context);

            List<string> listErr = new List<string>();

            var deps = dependingBlanks.Where(w => w.superiorType == BlankType);
            foreach (depBlank dep in deps)
            {
                int[] ids = context.SBP.Where(w => 
                    (dep.inHierarhySBP ? w.IdParent : w.Id) == IdOwner
                    && (!dep.sbpType.HasValue || w.IdSBPType == (byte)dep.sbpType.Value)
                ).Select(s => s.Id).ToArray();

                SBP_Blank superior = this;
                List<SBP_Blank> juniors = context.SBP_Blank.Where(s =>
                    ids.Contains(s.IdOwner) 
                    && s.IdBlankType == (byte)dep.juniorType
                    && s.IdBudget == this.IdBudget
                ).ToList();

                if (!juniors.Any())
                    continue;

                foreach (SBP_Blank junior in juniors)
                {
                    foreach (KeyValuePair<string, string> f in listFieldsBlank)
                    {
                        var p1 = junior.GetValue(f.Key) ?? BlankValTyp.Empty;
                        var p2 = superior.GetValue(f.Key) ?? BlankValTyp.Empty;

                        bool fail =
                            !dependingFields.Any(w => w.junior == (BlankValTyp) p1 && w.superior == (BlankValTyp) p2);
                        if (fail)
                        {
                            listErr.Add(junior.Owner.Caption + " - " + junior.BlankType.Caption());
                            break;
                        }
                    }
                }

            }

            if (listErr.Any())
            {
                Controls.Check(listErr, string.Format(
                    "В результате изменения бланк «{0}» не соответствует нижестоящим бланкам:<br>{{0}}<br><br>Необходимо изменять бланки, начиная с нижестоящих.",
                    BlankType.Caption()
                ));
            }
        }

        /// <summary>   
        /// Контроль "Запрет удаления бланка, который используется документами с проводками"
        /// </summary>
        [ControlInitial(InitialCaption = "Запрет удаления бланка, который используется документами с проводками", InitialUNK = "500510")]
        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 100)]
        public void Control_500510(DataContext context)
        {
            InitMaps(context);

            int pValueType = 0;
            List<int> aSbp = new List<int>();
            string captDoc = string.Empty;

            if (this.BlankType == BlankType.BringingGRBS)
            {
                pValueType = (int)ValueType.Plan;
                aSbp.Add(IdOwner);
                captDoc = "Предельные объемы бюджетных ассигнований";
            }
            else if (this.BlankType == BlankType.BringingRBS)
            {
                pValueType = (int)ValueType.Plan;
                aSbp =
                    context.SBP.Where(r => r.IdParent == IdOwner && r.IdSBPType == (int) SBPType.Manager)
                           .Select(s => s.Id)
                           .ToList();
                captDoc = "Предельные объемы бюджетных ассигнований";
            }
            else if (this.BlankType == BlankType.BringingKU)
            {
                pValueType = (int)ValueType.Plan;
                aSbp =
                    context.SBP.Where(r => r.IdParent == IdOwner && r.IdSBPType == (int) SBPType.TreasuryEstablishment)
                           .Select(s => s.Id)
                           .ToList();
                captDoc = "Предельные объемы бюджетных ассигнований";
            }
            else if (this.BlankType == BlankType.FormationKU)
            {
                pValueType = (int)ValueType.Justified;
                aSbp =
                    context.SBP.Where(r => r.IdParent == IdOwner && r.IdSBPType == (int) SBPType.TreasuryEstablishment)
                           .Select(s => s.Id)
                           .ToList();
                captDoc = "Смета";
            }
            else if (this.BlankType == BlankType.BringingAUBU)
            {
                pValueType = (int)ValueType.Plan;
                aSbp =
                    context.SBP.Where(r => r.IdParent == IdOwner && r.IdSBPType == (int) SBPType.IndependentEstablishment)
                           .Select(s => s.Id)
                           .ToList();
                captDoc = "Финансовое обеспечение АУ/БУ";
            }
            else if (this.BlankType == BlankType.FormationAUBU)
            {
                pValueType = (int)ValueType.JustifiedFBA;
                aSbp =
                    context.SBP.Where(r => r.IdParent == IdOwner && (r.IdSBPType == (int)SBPType.IndependentEstablishment || r.IdSBPType == (int)SBPType.TreasuryEstablishment))
                           .Select(s => s.Id)
                           .ToList();
                captDoc = "План финансово-хозяйственной деятельности";
            }
            else if (this.BlankType == BlankType.FormationGRBS)
            {
                pValueType = (int)ValueType.JustifiedGRBS;
                aSbp =
                    context.SBP.Where(r => r.Id == IdOwner || (r.IdParent == IdOwner && r.IdSBPType == (int)SBPType.Manager))
                           .Select(s => s.Id)
                           .ToList();
                captDoc = "Деятельность ведомства";
            }
            else
            {
                return;
            }

            var ofg =
                context.LimitVolumeAppropriations.Where(r => r.IdValueType == pValueType && aSbp.Contains(r.EstimatedLine.IdSBP) && r.IdBudget == IdBudget);

            if (ofg.Any())
            {
                var listErr = ofg.Select(s => " - " + s.EstimatedLine.SBP.Caption).Distinct().OrderBy(o => o).ToList();

                Controls.Check(listErr, string.Format(
                    "Запрещается удалять бланк «{0}», " +
                    "потому что в бюджете «{1}» имеется обработанный документ «{2}» по СБП:" +
                    "<br>{{0}}",
                    BlankType.Caption(),
                    Budget.Caption,
                    captDoc
                ));
            }

        }

        /// <summary>   
        /// Контроль "Проверка изменения бланка, который используется документами "
        /// </summary>
        [ControlInitial(InitialCaption = "Проверка изменения бланка, который используется документами ", InitialUNK = "500512", InitialSkippable = true)]
        [Control(ControlType.Update, Sequence.Before, ExecutionOrder = 100)]
        public void Control_500512(DataContext context)
        {
            InitMaps(context);

            int pValueType = 0;
            List<int> aSbp = new List<int>();
            string captDoc = string.Empty;

            int idEntityDoc;
            List<IClarificationDoc> docs;
            List<IClarificationDoc> table;

            if (this.BlankType == BlankType.FormationGRBS) //тип бланка = Формирование ГРБС
            {
                pValueType = (int) ValueType.JustifiedGRBS;
                aSbp =
                    context.SBP.Where(
                        r => r.Id == IdOwner || (r.IdParent == IdOwner && r.IdSBPType == (int) SBPType.Manager))
                           .Select(s => s.Id)
                           .ToList();
                captDoc = "Деятельность ведомства";

                idEntityDoc = ActivityOfSBP.EntityIdStatic;
                table = context.Set<IClarificationDoc>(idEntityDoc).ToList();

                var limitVolumeAppropriationses =
                    context.LimitVolumeAppropriations.Where(
                        r =>
                        r.IdBudget == this.IdBudget &&
                        (r.EstimatedLine.IdSBP == this.IdOwner || aSbp.Contains(r.EstimatedLine.IdSBP)) &&
                        r.IdValueType == (byte)ValueType.JustifiedGRBS && r.IdRegistratorEntity == idEntityDoc).ToList();

                if (!limitVolumeAppropriationses.Any())
                {
                    return;
                }

                var docs0 = from reg in limitVolumeAppropriationses
                            join t in table on reg.IdRegistrator equals t.Id
                            select t;
                //docs = docs0.Where(r => !r.IdParent.HasValue).Distinct().ToList();
                docs = docs0.Distinct().ToList();
            }
            else
            {
                if (this.BlankType == BlankType.BringingGRBS) //тип бланка = Доведение ГРБС
                {
                    pValueType = (int) ValueType.Plan;
                    aSbp.Add(IdOwner);
                    captDoc = "Предельные объемы бюджетных ассигнований";
                    idEntityDoc = LimitBudgetAllocations.EntityIdStatic;
                }
                else if (this.BlankType == BlankType.BringingRBS) //тип бланка = Доведение РБС
                {
                    pValueType = (int) ValueType.Plan;
                    aSbp =
                        context.SBP.Where(r => r.IdParent == IdOwner && r.IdSBPType == (int) SBPType.Manager)
                               .Select(s => s.Id)
                               .ToList();
                    captDoc = "Предельные объемы бюджетных ассигнований";
                    idEntityDoc = LimitBudgetAllocations.EntityIdStatic;
                }
                else if (this.BlankType == BlankType.BringingKU) //тип бланка = Доведение КУ
                {
                    pValueType = (int) ValueType.Plan;
                    aSbp =
                        context.SBP.Where(
                            r => r.IdParent == IdOwner && r.IdSBPType == (int) SBPType.TreasuryEstablishment)
                               .Select(s => s.Id)
                               .ToList();
                    captDoc = "Предельные объемы бюджетных ассигнований";
                    idEntityDoc = PlanActivity.EntityIdStatic;
                }
                else if (this.BlankType == BlankType.FormationKU) //тип бланка = Формирование КУ
                {
                    pValueType = (int) ValueType.Justified;
                    aSbp =
                        context.SBP.Where(
                            r => r.IdParent == IdOwner && r.IdSBPType == (int) SBPType.TreasuryEstablishment)
                               .Select(s => s.Id)
                               .ToList();
                    captDoc = "Смета";
                    idEntityDoc = PublicInstitutionEstimate.EntityIdStatic;
                }
                else if (this.BlankType == BlankType.BringingAUBU) //тип бланка = Доведение АУ/БУ
                {
                    pValueType = (int) ValueType.Plan;
                    aSbp =
                        context.SBP.Where(
                            r =>
                            r.IdParent == IdOwner &&
                            (r.IdSBPType == (int)SBPType.IndependentEstablishment ||
                             r.IdSBPType == (int)SBPType.BudgetEstablishment))
                               .Select(s => s.Id)
                               .ToList();
                    captDoc = "Финансовое обеспечение АУ/БУ";
                    idEntityDoc = PlanActivity.EntityIdStatic;
                }
                else if (this.BlankType == BlankType.FormationAUBU) //тип бланка = Формирование АУ/БУ
                {
                    pValueType = (int) ValueType.JustifiedFBA;
                    aSbp =
                        context.SBP.Where(
                            r =>
                            r.IdParent == IdOwner &&
                            (r.IdSBPType == (int) SBPType.IndependentEstablishment ||
                             r.IdSBPType == (int) SBPType.BudgetEstablishment))
                               .Select(s => s.Id)
                               .ToList();
                    captDoc = "План финансово-хозяйственной деятельности";
                    idEntityDoc = FinancialAndBusinessActivities.EntityIdStatic;
                }
                else
                {
                    return;
                }

                table = context.Set<IClarificationDoc>(idEntityDoc).ToList();
                docs = new List<IClarificationDoc>(context.Set<IDocOfSbpBudget>(idEntityDoc).ToList().Where(r => r.IdBudget == this.IdBudget && aSbp.Contains(r.IdSBP) && !r.IdParent.HasValue).ToList());

            }

            if (docs.Any())
            {
                var listErr = docs.Select(s => GetLeafDoc(table, s.Id)).ToList();

                var sdocs = listErr.Select(s => s.ToString()).Aggregate((a, b) => a + "<br>" + b);

                Controls.Throw(string.Format("В бюджете {0} создан документ «{1}», в котором используется текущий бланк:<br>{2}<br>" +
                                             "<br>В данном документе будет установлен признак «Требует уточнения».",
                                             this.Budget.Caption,
                                             captDoc,
                                             sdocs));

                var changetable = context.Set<IClarificationDoc>(idEntityDoc);

                foreach (var d in listErr)
                {
                    var doc = changetable.FirstOrDefault(r => r.Id == d.Id);
                    doc.IsRequireClarification = true;
                    doc.ReasonClarification =
                        string.Format(
                            "{0}. В справочнике «Субъекты бюджетного планирования» у СБП «{1}» был изменен бланк «{2}»",
                            DateTime.Now.ToShortDateString(), 
                            Owner.Caption,
                            this.BlankType.Caption()
                            );
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// возвращает в цепочке документов последний документ 
        /// </summary>
        private IHierarhy GetLeafDoc(List<IClarificationDoc> docs, int docId)
        {
            var doc = docs.FirstOrDefault(i => i.Id == docId);

            do
            {
                var child = docs.Where(c => c.IdParent == doc.Id);
                if (child.Any())
                {
                    doc = child.FirstOrDefault();
                }
                else
                {
                    break;
                }
            } while (true);
            return doc;
        }
	}
}