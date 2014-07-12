using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EntityFramework.Extensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Exceptions;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Common.Extensions;
using Platform.Utils.Common;
using Platform.Utils.Extensions;
using Sbor.DbEnums;
using Sbor.Logic;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;
using Sbor.Interfaces;

// ReSharper disable CheckNamespace
namespace Sbor.Document
// ReSharper restore CheckNamespace
{
    public partial class PublicInstitutionEstimate : IDocStatusTerminate, IDocOfSbpBudget
    {
        private class ActivityValues : KBK
        {
            public decimal? RegValue { get; set; }
            public decimal? AdditionalRegValue { get; set; }

            public int IdHierarchyPeriod { get; set; }

            public int IdActivity { get; set; }
            public int? IdContingent { get; set; }
        }

        private class RegistryAllocations
        {
            public RegistryAllocations Clone(int idHierarchyPeriod, decimal? value, bool hasAdditionalNeed)
            {
                var result = Clone();

                result.IdHierarchyPeriod = idHierarchyPeriod;
                result.Value = value;
                result.HasAdditionalNeed = hasAdditionalNeed;

                return result;
            }

            private RegistryAllocations Clone()
            {
                return new RegistryAllocations
                {
                    IdEstimatedLine = IdEstimatedLine,
                    IsIndirectCosts = IsIndirectCosts,
                    IdAuthorityOfExpenseObligation = IdAuthorityOfExpenseObligation,
                    IdTaskCollection = IdTaskCollection,
                    IdOKATO = IdOKATO
                };
            }

            /// <summary>
            /// Ссылка на сметную строку
            /// </summary>
            public int? IdEstimatedLine { get; set; }

            /// <summary>
            /// Ссылка на период
            /// </summary>
            public int IdHierarchyPeriod { get; set; }

            /// <summary>
            /// значение
            /// </summary>
            public decimal? Value { get; set; }

            /// <summary>
            /// Косвенный расход?
            /// </summary>
            public bool IsIndirectCosts { get; set; }

            /// <summary>
            /// Тип РО
            /// </summary>
            public int? IdAuthorityOfExpenseObligation { get; set; }

            /// <summary>
            /// Доп. потребности
            /// </summary>
            public bool? HasAdditionalNeed { get; set; }

            /// <summary>
            /// Набор задач
            /// </summary>
            public int? IdTaskCollection { get; set; }

            public int? IdOKATO { get; set; }
        }

        #region Common properties
        private readonly int[] _arrRegisters = new []{ LimitVolumeAppropriations.EntityIdStatic };
        
        private int[] _prevVersionIds;

        private SBP _sbpParent;
        private SBP_Blank _blankFormationKU;
        private SBP_Blank _blankFormationAUBU;
        private SBP_Blank _blankBringingKU;

        public SBP CurentSBP
        {
            get { 
                if (SBP == null && IdSBP == 0)
                    throw new PlatformException("У документа не заполненно обязательное поле СБП");

                if (SBP == null)
                {
                    var dc = IoC.Resolve<DbContext>().Cast<DataContext>();
                    SBP = dc.SBP.FirstOrDefault(s => s.Id == IdSBP);
                }
                
                return SBP;
            }
        }

        /// <summary>
        /// Все предки текущего документа
        /// </summary>
        public int[] PrevVersionIds
        {
            get
            {
                if (_prevVersionIds == null)
                {
                    var context = IoC.Resolve<DbContext>().Cast<DataContext>();
                    _prevVersionIds = this.GetIdAllVersion(context).ToArray();
                }
                    
                return _prevVersionIds;
            }
        }

        /// <summary>
        /// Вышестоящий СБП
        /// </summary>
        /// <exception cref="PlatformException"></exception>
        public SBP SBPParent
        {
            get
            {
                if (_sbpParent == null)
                {
                    if (SBP == null)
                        SBP = CurentSBP;

                    if (!SBP.IdParent.HasValue)
                        throw new PlatformException(String.Format("У СБП «{0}» типа 'Казенное учереждение' на установлено вышестоящее учреждение. Проблема с контролями в сущности SBP.", SBP.Caption));
                    
                    _sbpParent = SBP.Parent;
                    if (_sbpParent == null)
                    {
                        var dc = IoC.Resolve<DbContext>().Cast<DataContext>();
                        _sbpParent = dc.SBP.FirstOrDefault(s => s.Id == SBP.IdParent.Value);
                        if (_sbpParent == null)
                            throw new PlatformException(String.Format("Отсутствует СБП с id = {0}", SBP.IdParent.Value));
                    }
                }

                return _sbpParent;
            }
        }

        /// <summary>
        /// Бланк доведения КУ вышестоящего СБП
        /// </summary>
        public SBP_Blank BlankBringingKU
        {
            get
            {
                if (_blankBringingKU == null)
                {
                    _blankBringingKU = SBPParent.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.BlankType == BlankType.BringingKU);

                    if (_blankBringingKU == null)
                        throw new PlatformException("Бланк «Доведение КУ» отсутствует");
                }

                return _blankBringingKU;
            }
        }

        /// <summary>
        /// Бланк формирования КУ вышестоящего СБП
        /// </summary>
        public SBP_Blank BlankFormationKU
        {
            get
            {
                if (_blankFormationKU == null)
                {
                    _blankFormationKU = SBPParent.SBP_Blank.FirstOrDefault(b => b.IdBudget == IdBudget && b.IdBlankType == (byte)BlankType.FormationKU);

                    if (_blankFormationKU == null)
                        throw new PlatformException("Бланк «Формирование КУ» отсутствует");
                }

                return _blankFormationKU;
            }
        }

        /// <summary>
        /// Бланк формирования АУБУ текущего СБП
        /// </summary>
        public SBP_Blank BlankFormationAUBU
        {
            get
            {
                if (_blankFormationAUBU == null)
                {
                    if (SBP == null)
                        SBP = CurentSBP;
                    
                    if (!SBP.IsFounder)
                        return null;

                    _blankFormationAUBU = SBP.SBP_Blank.FirstOrDefault(b => b.IdBlankType == (int)BlankType.FormationAUBU);
                    if (_blankFormationAUBU == null)
                        throw new PlatformException("У учреждения отсутствует бланк 'Формирование АУБУ'. Проблема с контролями в сущности SBP");
                }
                
                return _blankFormationAUBU;
            }

        }

        /// <summary>
        /// ТЧ Расходы учредителя АУБУ
        /// </summary>
        [NotMapped]
        public List<PublicInstitutionEstimate_FounderAUBUExpense> FounderExpensesList
        {
            get
            {
                if (FounderAUBUExpenses == null)
                {
                    var dc = IoC.Resolve<DbContext>().Cast<DataContext>();
                    FounderAUBUExpenses = dc.PublicInstitutionEstimate_FounderAUBUExpense.Where(e => e.IdOwner == Id).ToList();
                }

                return FounderAUBUExpenses.ToList();
            }
        }

        /// <summary>
        /// ТЧ Расходы
        /// </summary>
        [NotMapped]
        public List<PublicInstitutionEstimate_Expense> ExpensesList
        {
            get
            {
                if (Expenses == null)
                {
                    var dc = IoC.Resolve<DbContext>().Cast<DataContext>();
                    Expenses = dc.PublicInstitutionEstimate_Expense.Where(e => e.IdOwner == Id).ToList();
                }

                return Expenses.ToList();
            }
        }


        #endregion

     

      
        #region

        /// <summary>
        /// Заполнить ТЧ мероприятия
        /// </summary>
        /// <param name="context"></param>
        public void FillData_Activities(DataContext context)
        {
            var regActivities = GetActivitiesFromRegister(context).ToList();
            var existed = new List<int>();

            foreach (var regActivity in regActivities)
            {
                var docActivity = Activities.FirstOrDefault(a => a.IdActivity == regActivity.IdActivity &&
                                                                 a.IdContingent == regActivity.IdContingent);
                if (docActivity == null)
                {
                    regActivity.IdOwner = Id;
                    context.PublicInstitutionEstimate_Activity.Add(regActivity);
                }
                else
                    existed.Add(docActivity.Id);
            }

            var deleting = Activities.Where(a => a.Id != 0 && !existed.Contains(a.Id)).ToList();

            if (deleting.Any())
            {
                var deletedIds = deleting.Select(d => d.Id).ToArray();
                if (context.PublicInstitutionEstimate_DistributionActivity.Any(a => deletedIds.Contains(a.IdPublicInstitutionEstimate_Activity)))
                {
                    var indirectActivities = context.PublicInstitutionEstimate_DistributionActivity
                                                    .Where(a => deletedIds.Contains(a.IdPublicInstitutionEstimate_Activity))
                                                    .Select(a => a.IdPublicInstitutionEstimate_Activity)
                                                    .ToArray();

                    var activities = Activities.AsQueryable().Where(a => a.Id != 0 && !existed.Contains(a.Id))
                                .Select(a => new{ id = a.Id, caption = "-" + (a.Activity.Caption + (a.IdContingent.HasValue ? (" - " + a.Contingent.Caption) : ""))})
                                .Distinct()
                                .ToList();

                    var msg = new List<String>();

                    foreach (var m in activities )
                    {
                        if ( indirectActivities.Contains(m.id) )
                            msg.Add("<b>" + m.caption + "</b>");
                        else
                            msg.Add(m.caption);
                    }
                    
                    ExecuteControl(e => e.Control_0728(msg, true));
                }
                else
                {
                    var msg = Activities.AsQueryable().Where(a => a.Id != 0 && !existed.Contains(a.Id))
                        .Select(a => "-" + (a.Activity.Caption + (a.IdContingent.HasValue ? (" - " + a.Contingent.Caption) : "")))
                        .Distinct()
                        .ToList();

                    ExecuteControl(e => e.Control_0728(msg, false));
                }

                
                
                using(new ControlScope())
                    context.PublicInstitutionEstimate_Activity.RemoveAll(deleting);
            }
            
            using (new ControlScope())
                context.SaveChanges();
        }

        /// <summary>
        /// Заполнить ТЧ мероприятия АУ/БУ
        /// </summary>
        /// <param name="context"></param>
        public void FillData_ActivitiesAUBU(DataContext context)
        {
            var regActivities = GetActivitiesAUBUFromRegister(context).ToList();
            var existed = new List<int>();

            foreach (var regActivity in regActivities)
            {
                var docActivity = ActivitiesAUBU.FirstOrDefault(a => a.IdActivity == regActivity.IdActivity &&
                                                                 a.IdContingent == regActivity.IdContingent);
                if (docActivity == null)
                {
                    regActivity.IdOwner = Id;
                    context.PublicInstitutionEstimate_ActivityAUBU.Add(regActivity);
                }
                else
                    existed.Add(docActivity.Id);
            }

            var deleting = ActivitiesAUBU.Where(a => a.Id != 0 && !existed.Contains(a.Id)).ToList();

            if (deleting.Any())
            {
                var msg = ActivitiesAUBU.AsQueryable().Where(a => a.Id != 0 && !existed.Contains(a.Id))
                    .Select(a => "-" + (a.Activity.Caption + (a.IdContingent.HasValue ? (" - " + a.Contingent.Caption) : "")))
                    .Distinct()
                    .ToList();

                ExecuteControl(e => e.Control_0728(msg, false));

                context.PublicInstitutionEstimate_ActivityAUBU.RemoveAll(deleting);
            }
         
            context.SaveChanges();
        }

        /// <summary>
        /// Заполнить ТЧ расходы бюджетных учреждений
        /// </summary>
        /// <param name="context"></param>
        public void FillData_ExpenseAloneSubjects(DataContext context)
        {
            if (SBP.IsFounder)
            {
                var regValues = GetExpensesForAloneSubjects(context);
                var existed = new List<int>();

                var year = Budget.Year;
                var docTaskCollections = ActivitiesAUBU.Select(a => new { a.IdActivity, a.IdContingent, a.Id }).ToList();
                foreach (var value in regValues)
                {
                    var expense = PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense
                                                        .GetQueryByKBK(AloneAndBudgetInstitutionExpenses.AsQueryable(), value)
                                                        .FirstOrDefault(e => e.Master.IdActivity == value.IdActivity &&
                                                                        ((!e.Master.IdContingent.HasValue && !value.IdContingent.HasValue)
                                                                            || (e.Master.IdContingent == value.IdContingent)));

                    if (expense == null)
                    {
                        var activityId = docTaskCollections.Where(c => c.IdActivity == value.IdActivity && c.IdContingent == value.IdContingent)
                                                            .Select(c => c.Id)
                                                            .FirstOrDefault();

                        if (activityId > 0)
                        {
                            expense = new PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense(value)
                            {
                                IdOwner = Id,
                                IdMaster = activityId,

                                OFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : null,
                                PFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : null,
                                PFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : null,
                                AdditionalOFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : null,
                                AdditionalPFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : null,
                                AdditionalPFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : null,
                            };
                            context.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense.Add(expense);
                        }
                    }
                    else
                    {
                        existed.Add(expense.Id);

                        expense.OFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.OFG;
                        expense.PFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.PFG1;
                        expense.PFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.RegValue : expense.PFG2;
                        expense.AdditionalOFG = year.GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalOFG;
                        expense.AdditionalPFG1 = (year + 1).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalPFG1;
                        expense.AdditionalPFG2 = (year + 2).GetIdHierarchyPeriodYear(context) == value.IdHierarchyPeriod ? value.AdditionalRegValue : expense.AdditionalPFG2;
                    }
                }

                var deleting = AloneAndBudgetInstitutionExpenses.Where(a => a.Id != 0 && !existed.Contains(a.Id)).ToList();
                foreach (var expense in deleting)
                    context.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense.Remove(expense);

                context.SaveChanges();

                ExecuteControl(e => e.Control_0715(context, false));
            }
        }

        /// <summary>
        /// Выполнить контроль документа из элемента ТЧ
        /// </summary>
        /// <param name="controlLambda"></param>
        public void InvokeControl(Expression<Action<PublicInstitutionEstimate>> controlLambda)
        {
            ExecuteControl(controlLambda);
        }

        /// <summary>
        /// Заполнить ТЧ расходы учредителя АУ/БУ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activityIds"></param>
        public void FillData_FounderAUBUExpenses(DataContext context, int[] activityIds)
        {
            {
                var founderExpenses = FounderAUBUExpenses.Where(e => activityIds.Contains(e.IdMaster));
                if (founderExpenses.Any())
                {
                    context.PublicInstitutionEstimate_FounderAUBUExpense.RemoveAll(founderExpenses.ToList());
                    context.SaveChanges();
                }
            }

            var kosgu530 = context.KOSGU.FirstOrDefault(c => c.Code == "530");
            if (kosgu530 == null)
                throw new CommonUserFrendlyException("В системе отсутствует КОСГУ #530 ");
            var kosgu241 = context.KOSGU.FirstOrDefault(c => c.Code == "241");
            if (kosgu241 == null)
                throw new CommonUserFrendlyException("В системе отсутствует КОСГУ #241 ");

            var idKosgu530 = kosgu530.Id;
            var idKosgu241 = kosgu241.Id;

            var idKfo6 = context.KFO.Where(c => c.Code == "6").Select(c => c.Id).FirstOrDefault();
            var idKfo1 = context.KFO.Where(c => c.Code == "1").Select(c => c.Id).FirstOrDefault();

            foreach (var activityAUBU in ActivitiesAUBU.Where(a => activityIds.Contains(a.Id)))
            {
                var activityId = activityAUBU.Id;
                var expenses =
                    AloneAndBudgetInstitutionExpenses.Where(e => e.IdMaster == activityId)
                                                     .GroupBy(e => 1)
                                                     .Select(e => new
                                                     {
                                                         kfo6OFG = e.Sum(c => (c.IdKFO == idKfo6) ? c.OFG : null),
                                                         //kfo1OFG = e.Sum(c => (c.IdKFO == idKfo1) ? null : c.OFG),
                                                         kfo1OFG = e.Sum(c => (c.IdKFO == idKfo6) ? null : c.OFG),
                                                         kfo6PFG1 = e.Sum(c => (c.IdKFO == idKfo6) ? c.PFG1 : null),
                                                         //kfo1PFG1 = e.Sum(c => (c.IdKFO == idKfo1) ? null : c.PFG1),
                                                         kfo1PFG1 = e.Sum(c => (c.IdKFO == idKfo6) ? null : c.PFG1),
                                                         kfo6PFG2 = e.Sum(c => (c.IdKFO == idKfo6) ? c.PFG2 : null),
                                                         //kfo1PFG2 = e.Sum(c => (c.IdKFO == idKfo1) ? null : c.PFG2)
                                                         kfo1PFG2 = e.Sum(c => (c.IdKFO == idKfo6) ? null : c.PFG2)
                                                     })
                                                     .ToList();

                foreach (var expense in expenses)
                {
                    if ((expense.kfo1OFG.HasValue && expense.kfo1OFG.Value > 0) ||
                        (expense.kfo1PFG1.HasValue && expense.kfo1PFG1.Value > 0) ||
                        (expense.kfo1PFG2.HasValue && expense.kfo1PFG2.Value > 0))
                    {
                        var founderExpense = new PublicInstitutionEstimate_FounderAUBUExpense
                            {
                                IdOwner = Id,
                                IdKFO = idKfo1,
                                IdKOSGU = idKosgu241,
                                OFG = expense.kfo1OFG,
                                PFG1 = expense.kfo1PFG1,
                                PFG2 = expense.kfo1PFG2,
                                IdMaster = activityAUBU.Id,
                                IsAUBU = true
                            };

                        context.PublicInstitutionEstimate_FounderAUBUExpense.Add(founderExpense);
                    }

                    if ((expense.kfo6OFG.HasValue && expense.kfo6OFG.Value > 0) ||
                        (expense.kfo6PFG1.HasValue && expense.kfo6PFG1.Value > 0) ||
                        (expense.kfo6PFG2.HasValue && expense.kfo6PFG2.Value > 0))
                    {
                        var founderExpense = new PublicInstitutionEstimate_FounderAUBUExpense
                            {
                                IdKFO = idKfo1,
                                IdKOSGU = idKosgu530,
                                OFG = expense.kfo6OFG,
                                PFG1 = expense.kfo6PFG1,
                                PFG2 = expense.kfo6PFG2,
                                IdOwner = Id,
                                IdMaster = activityAUBU.Id,
                                IsAUBU = true
                            };
                        context.PublicInstitutionEstimate_FounderAUBUExpense.Add(founderExpense);
                    }
                }

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Распределение косвенных расходов
        /// </summary>
        /// <param name="context"></param>
        /// <param name="rows">Список методов, для которых делаем расчет</param>
        public void CalculateIndirectExpenses(DataContext context, int[] rows)
        {
            //Чистим все старое
            var indirectCostsMethods = context.PublicInstitutionEstimate_DistributionMethod.Where(m => rows.Contains(m.Id))
                                              .Select(m => m.IdIndirectCostsDistributionMethod)
                                              .ToList();

            var removeable = context.PublicInstitutionEstimate_Expense.Where(e => e.IdOwner == Id && e.IdIndirectCostsDistributionMethod.HasValue &&
                                                                     indirectCostsMethods.Contains(e.IdIndirectCostsDistributionMethod.Value)).ToList();

            context.PublicInstitutionEstimate_Expense.RemoveAll(removeable);

            context.SaveChanges();

            foreach (var method in context.PublicInstitutionEstimate_DistributionMethod.Where(m => m.IdOwner == Id && rows.Contains(m.Id)).ToList())
            {
                switch (method.IndirectCostsDistributionMethod)
                {
                    case IndirectCostsDistributionMethod.M1:
                        AddIndirectExpensesM1(context, method.Id);
                        break;
                    case IndirectCostsDistributionMethod.M2:
                        AddIndirectExpensesM2(context, method.Id);
                        break;
                    case IndirectCostsDistributionMethod.M3:
                        AddIndirectExpensesM3(context, method.Id);
                        break;
                    case IndirectCostsDistributionMethod.M4:
                        AddIndirectExpensesM4(context, method.Id);
                        break;
                    case IndirectCostsDistributionMethod.M5:
                        AddIndirectExpensesM5(context, method.Id);
                        break;
                    default:
                        throw new PlatformException("Не реализованно для метода " + method.IndirectCostsDistributionMethod.Caption());
                }
            }


        }

        private void AddIndirectExpensesM1(DataContext context, int masterId)
        {
            var activities = context.PublicInstitutionEstimate_DistributionActivity.Where(a => a.IdOwner == Id && a.IdMaster == masterId);

            var activitiesCount = activities.Count();
            if (activitiesCount == 0)
                throw new PlatformException("В ТЧ 'Мероприятия для распределения' отсутствуют строки");

            var factor = (decimal)1 / activitiesCount;

            foreach (var activity in activities.ToList())
            {
                foreach (var indirectExpense in context.PublicInstitutionEstimate_IndirectExpenses.Where(e => e.IdMaster == masterId).ToList())
                {
                    var expense = indirectExpense.CloneAsLineCost<PublicInstitutionEstimate_Expense>();
                    expense.OFG = indirectExpense.OFG * factor;
                    expense.PFG1 = indirectExpense.PFG1 * factor;
                    expense.PFG2 = indirectExpense.PFG2 * factor;
                    expense.IdIndirectCostsDistributionMethod = (int)IndirectCostsDistributionMethod.M1;
                    expense.IdMaster = activity.IdPublicInstitutionEstimate_Activity;
                    expense.IsIndirectCosts = true;
                    expense.IdOwner = Id;

                    context.PublicInstitutionEstimate_Expense.Add(expense);
                }

                context.SaveChanges();
            }

        }

        private void AddIndirectExpensesM2(DataContext context, int masterId)
        {
            var activities = context.PublicInstitutionEstimate_DistributionActivity.Where(a => a.IdOwner == Id && a.IdMaster == masterId);

            var commonActivities = activities.Select(a => a.IdPublicInstitutionEstimate_Activity).Distinct().ToList();
            var totalAllocations = context.PublicInstitutionEstimate_Expense.Where(e => e.IdOwner == Id && (!e.IsIndirectCosts.HasValue || !e.IsIndirectCosts.Value) && commonActivities.Contains(e.IdMaster))
                                                                            .GroupBy(e => true)
                                                                            .Select(g => new { tofg = g.Sum(e => e.OFG), tpfg1 = g.Sum(e => e.PFG1), tpfg2 = g.Sum(e => e.PFG2) })
                                                                            .FirstOrDefault();
            if (totalAllocations == null)
            {
                foreach (var activity in activities.ToList())
                {
                    activity.DirectOFG = 0;
                    activity.DirectPFG1 = 0;
                    activity.DirectPFG2 = 0;
                }
                context.SaveChanges();
                return;
            }

            foreach (var activity in activities.ToList())
            {
                var activityAllocation = context.PublicInstitutionEstimate_Expense.Where(e => e.IdOwner == Id && (!e.IsIndirectCosts.HasValue || !e.IsIndirectCosts.Value) && e.IdMaster == activity.IdPublicInstitutionEstimate_Activity)
                                                                                  .GroupBy(e => true)
                                                                                  .Select(g => new { aofg = g.Sum(e => e.OFG), apfg1 = g.Sum(e => e.PFG1), apfg2 = g.Sum(e => e.PFG2) })
                                                                                  .FirstOrDefault() ?? new { aofg = (decimal?)0, apfg1 = (decimal?)0, apfg2 = (decimal?)0 };


                activity.DirectOFG = activityAllocation.aofg ?? 0;
                activity.DirectPFG1 = activityAllocation.apfg1 ?? 0;
                activity.DirectPFG2 = activityAllocation.apfg2 ?? 0;

                foreach (var indirectExpense in context.PublicInstitutionEstimate_IndirectExpenses.Where(e => e.IdMaster == masterId).ToList())
                {
                    var expense = indirectExpense.CloneAsLineCost<PublicInstitutionEstimate_Expense>();
                    expense.OFG = (totalAllocations.tofg.HasValue && totalAllocations.tofg.Value > 0) ? indirectExpense.OFG * activity.DirectOFG / totalAllocations.tofg.Value : null;
                    expense.PFG1 = (totalAllocations.tpfg1.HasValue && totalAllocations.tpfg1.Value > 0) ? indirectExpense.PFG1 * activity.DirectPFG1 / totalAllocations.tpfg1.Value : null;
                    expense.PFG2 = (totalAllocations.tpfg2.HasValue && totalAllocations.tpfg2.Value > 0) ? indirectExpense.PFG2 * activity.DirectPFG2 / totalAllocations.tpfg2.Value : null;

                    if (expense.OFG > 0 || expense.PFG1 > 0 || expense.PFG2 > 0)
                    {
                        expense.IdIndirectCostsDistributionMethod = (int)IndirectCostsDistributionMethod.M2;
                        expense.IdMaster = activity.IdPublicInstitutionEstimate_Activity;
                        expense.IdOwner = Id;
                        expense.IsIndirectCosts = true;

                        context.PublicInstitutionEstimate_Expense.Add(expense);
                    }
                }

                context.SaveChanges();

            }

        }

        private void AddIndirectExpensesM3(DataContext context, int masterId)
        {
            var activities = context.PublicInstitutionEstimate_DistributionActivity.Where(a => a.IdOwner == Id && a.IdMaster == masterId);
            var commonActivities = activities.Select(a => a.PublicInstitutionEstimate_Activity.IdActivity).Distinct().ToList();

            var taskVolumes = context.TaskVolume.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && w.IdBudget == IdBudget
                                                            && w.IdVersion == IdVersion
                                                            && w.IdSBP == IdSBP
                                                            && w.IdValueType == (byte)ValueType.Plan
                                                            && w.IdTerminator == null
                                                            && (!w.ActivityAUBU.HasValue || !w.ActivityAUBU.Value)
                                                            && w.HierarchyPeriod.Year >= Budget.Year
                                                            && w.HierarchyPeriod.Year <= Budget.Year + 2);

            var totalTaskVolumes = taskVolumes.Where(t => commonActivities.Contains(t.TaskCollection.IdActivity))
                                              .GroupBy(g => g.HierarchyPeriod.Year)
                                              .Select(g => new { Year = g.Key, sum = g.Sum(e => e.Value) })
                                              .ToList();

            foreach (var activity in activities.ToList())
            {

                var taskVolume = taskVolumes.Where(w => w.TaskCollection.IdActivity == activity.PublicInstitutionEstimate_Activity.IdActivity
                                                        && w.TaskCollection.IdContingent == activity.PublicInstitutionEstimate_Activity.IdContingent)
                                     .GroupBy(g => g.HierarchyPeriod.Year)
                                     .Select(s => new { year = s.Key, sum = s.Sum(su => su.Value) })
                                     .ToList();

                var ofgTotal = totalTaskVolumes.Where(t => t.Year == Budget.Year).Select(t => t.sum).FirstOrDefault();
                var pfg1Total = totalTaskVolumes.Where(t => t.Year == Budget.Year + 1).Select(t => t.sum).FirstOrDefault();
                var pfg2Total = totalTaskVolumes.Where(t => t.Year == Budget.Year + 2).Select(t => t.sum).FirstOrDefault();

                var ofg = taskVolume.Where(t => t.year == Budget.Year).Select(t => t.sum).FirstOrDefault();
                var pfg1 = taskVolume.Where(t => t.year == Budget.Year + 1).Select(t => t.sum).FirstOrDefault();
                var pfg2 = taskVolume.Where(t => t.year == Budget.Year + 2).Select(t => t.sum).FirstOrDefault();

                activity.VolumeOFG = ofg;
                activity.VolumePFG1 = pfg1;
                activity.VolumePFG2 = pfg2;

                if (ofgTotal != 0 && pfg1Total != 0 && pfg2Total != 0)
                    foreach (var indirectExpense in context.PublicInstitutionEstimate_IndirectExpenses.Where(e => e.IdMaster == masterId).ToList())
                    {
                        var expense = indirectExpense.CloneAsLineCost<PublicInstitutionEstimate_Expense>();
                        expense.OFG = indirectExpense.OFG * ofg / ofgTotal;
                        expense.PFG1 = indirectExpense.PFG1 * pfg1 / pfg1Total;
                        expense.PFG2 = indirectExpense.PFG2 * pfg2 / pfg2Total;
                        expense.IdIndirectCostsDistributionMethod = (int)IndirectCostsDistributionMethod.M3;
                        expense.IdMaster = activity.IdPublicInstitutionEstimate_Activity;
                        expense.IdOwner = Id;
                        expense.IsIndirectCosts = true;

                        context.PublicInstitutionEstimate_Expense.Add(expense);
                    }
                context.SaveChanges();
            }
        }

        private void AddIndirectExpensesM4(DataContext context, int masterId)
        {
            ExecuteControl(e => e.Control_0704(context));

            var activities = context.PublicInstitutionEstimate_DistributionActivity.Where(a => a.IdOwner == Id && a.IdMaster == masterId);

            foreach (var activity in activities.ToList())
            {
                foreach (var indirectExpense in context.PublicInstitutionEstimate_IndirectExpenses.Where(e => e.IdMaster == masterId).ToList())
                {
                    var expense = indirectExpense.CloneAsLineCost<PublicInstitutionEstimate_Expense>();
                    expense.OFG = indirectExpense.OFG * activity.FactorOFG / 100;
                    expense.PFG1 = indirectExpense.PFG1 * activity.FactorPFG1 / 100;
                    expense.PFG2 = indirectExpense.PFG2 * activity.FactorPFG2 / 100;

                    if (expense.OFG > 0 || expense.PFG1 > 0 || expense.PFG2 > 0)
                    {
                        expense.IdIndirectCostsDistributionMethod = (int)IndirectCostsDistributionMethod.M4;
                        expense.IdMaster = activity.IdPublicInstitutionEstimate_Activity;
                        expense.IdOwner = Id;
                        expense.IsIndirectCosts = true;

                        context.PublicInstitutionEstimate_Expense.Add(expense);
                    }
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Пропорционально прямым расходам по указанным КОСГУ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="masterId"></param>
        private void AddIndirectExpensesM5(DataContext context, int masterId)
        {
            var activities = context.PublicInstitutionEstimate_DistributionActivity.Where(a => a.IdOwner == Id && a.IdMaster == masterId);
            var kosguIds = context.PublicInstitutionEstimate_DistributionAdditionalParam.Where(a => a.IdOwner == Id && a.IdMaster == masterId).Select(m => m.IdKOSGU).ToList();

            var commonActivities = activities.Select(a => a.IdPublicInstitutionEstimate_Activity).Distinct().ToList();
            var allocations =
                context.PublicInstitutionEstimate_Expense.Where(e => e.IdOwner == Id &&
                                                                      e.IdKOSGU.HasValue && kosguIds.Contains(e.IdKOSGU.Value) &&
                                                                      (!e.IsIndirectCosts.HasValue || !e.IsIndirectCosts.Value));

            var totalAllocations = allocations.Where(e => commonActivities.Contains(e.IdMaster))
                                              .GroupBy(e => true)
                                              .Select(g => new { tofg = g.Sum(e => e.OFG), tpfg1 = g.Sum(e => e.PFG1), tpfg2 = g.Sum(e => e.PFG2) })
                                              .FirstOrDefault();
            //Если в ТЧ нет записей с КОСГУ
            if (totalAllocations == null)
            {
                foreach (var activity in activities.ToList())
                {
                    activity.DirectOFG = 0;
                    activity.DirectPFG1 = 0;
                    activity.DirectPFG2 = 0;
                }
                context.SaveChanges();

                return;
            }

            foreach (var activity in activities.ToList())
            {
                var activityAllocation = allocations.Where(e => e.IdMaster == activity.IdPublicInstitutionEstimate_Activity)
                                                    .GroupBy(e => true)
                                                    .Select(g => new { aofg = g.Sum(e => e.OFG), apfg1 = g.Sum(e => e.PFG1), apfg2 = g.Sum(e => e.PFG2) })
                                                    .FirstOrDefault() ?? new { aofg = (decimal?)0, apfg1 = (decimal?)0, apfg2 = (decimal?)0 };

                activity.DirectOFG = activityAllocation.aofg ?? 0;
                activity.DirectPFG1 = activityAllocation.apfg1 ?? 0;
                activity.DirectPFG2 = activityAllocation.apfg2 ?? 0;

                foreach (var indirectExpense in context.PublicInstitutionEstimate_IndirectExpenses.Where(e => e.IdMaster == masterId).ToList())
                {
                    var expense = indirectExpense.CloneAsLineCost<PublicInstitutionEstimate_Expense>();
                    expense.OFG = (totalAllocations.tofg.HasValue && totalAllocations.tofg.Value > 0) ? indirectExpense.OFG * activity.DirectOFG / totalAllocations.tofg.Value : null;
                    expense.PFG1 = (totalAllocations.tpfg1.HasValue && totalAllocations.tpfg1.Value > 0) ? indirectExpense.PFG1 * activity.DirectPFG1 / totalAllocations.tpfg1.Value : null;
                    expense.PFG2 = (totalAllocations.tpfg2.HasValue && totalAllocations.tpfg2.Value > 0) ? indirectExpense.PFG2 * activity.DirectPFG2 / totalAllocations.tpfg2.Value : null;

                    if (expense.OFG > 0 || expense.PFG1 > 0 || expense.PFG2 > 0)
                    {
                        expense.IdIndirectCostsDistributionMethod = (int)IndirectCostsDistributionMethod.M5;
                        expense.IdMaster = activity.IdPublicInstitutionEstimate_Activity;
                        expense.IdOwner = Id;
                        expense.IsIndirectCosts = true;

                        context.PublicInstitutionEstimate_Expense.Add(expense);
                    }


                }

                context.SaveChanges();

            }
        }
        #endregion

        protected static void ApproveInReg(DataContext context, int[] docIds, PublicInstitutionEstimate doc)
        {
            context.LimitVolumeAppropriations.Update(
                r => docIds.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityIdStatic && !r.DateCommit.HasValue,
                u => new LimitVolumeAppropriations
                    {
                        DateCommit = doc.Date,
                        IdApproved = doc.Id,
                        IdApprovedEntity = doc.EntityId
                    });


            /*foreach (var reg in context.LimitVolumeAppropriations.Where(r => docIds.Contains(r.IdRegistrator) && r.IdRegistratorEntity == EntityIdStatic && !r.DateCommit.HasValue))
            {
                reg.DateCommit = doc.Date;
                reg.IdApproved = doc.Id;
                reg.IdApprovedEntity = doc.EntityId;
            }
            context.SaveChanges();*/
        }


        #region Клонирование

        private PublicInstitutionEstimate Clone(DataContext context)
        {
            var doc = this;

            var newDoc = new PublicInstitutionEstimate
                {
                    IdDocStatus = doc.IdDocStatus,
                    Date = doc.Date,
                    IdParent = doc.IdParent,
                    IsRequireClarification = doc.IsRequireClarification,
                    IsApproved = doc.IsApproved,
                    DateCommit = doc.Date,
                    ReasonClarification = doc.ReasonClarification,
                    ReasonTerminate = doc.ReasonTerminate,
                    ReasonCancel = doc.ReasonCancel,
                    DateTerminate = doc.DateTerminate,
                    IdBudget = doc.IdBudget,
                    IdPublicLegalFormation = doc.IdPublicLegalFormation,
                    IdSBP = doc.IdSBP,
                    IdVersion = doc.IdVersion,
                    DateLastEdit = doc.DateLastEdit,
                    Description = doc.Description,
                    Number = doc.Number,
                    Caption = doc.Caption,
                    HasAdditionalNeed = doc.HasAdditionalNeed,
                    IdSBP_BlankActual = doc.IdSBP_BlankActual,
                    IdSBP_BlankActualAuBu = doc.IdSBP_BlankActualAuBu,
                };

            context.PublicInstitutionEstimate.Add(newDoc);

            var newActivities = new Dictionary<int, PublicInstitutionEstimate_Activity>();
            #region Мероприятия
            foreach (var activity in doc.Activities.ToList())
            {
                var newActivity = new PublicInstitutionEstimate_Activity
                    {
                        IdActivity = activity.IdActivity,
                        IdContingent = activity.IdContingent,
                        IdIndicatorActivity = activity.IdIndicatorActivity,
                        IdUnitDimension = activity.IdUnitDimension,
                        Owner = newDoc,
                    };

                context.PublicInstitutionEstimate_Activity.Add(newActivity);
                newActivities.Add(activity.Id, newActivity);

                foreach (var expense in activity.PublicInstitutionEstimate_Expense.ToList())
                {
                    var newExpense = new PublicInstitutionEstimate_Expense
                        {
                            AdditionalOFG = expense.AdditionalOFG,
                            AdditionalPFG1 = expense.AdditionalPFG1,
                            AdditionalPFG2 = expense.AdditionalPFG2,

                            OFG = expense.OFG,
                            PFG1 = expense.PFG1,
                            PFG2 = expense.PFG2,

                            IdIndirectCostsDistributionMethod = expense.IdIndirectCostsDistributionMethod,
                            IsIndirectCosts = expense.IsIndirectCosts,
                            IdOKATO = expense.IdOKATO,
                            IdAuthorityOfExpenseObligation = expense.IdAuthorityOfExpenseObligation,

                            Master = newActivity,
                            Owner = newDoc
                        };

                    newExpense.SetLineCostValues(expense);

                    context.PublicInstitutionEstimate_Expense.Add(newExpense);
                }
            }
            #endregion

            #region Доп. методы распределения

            foreach (var method in doc.DistributionMethods.ToList())
            {
                var newMethod = new PublicInstitutionEstimate_DistributionMethod
                    {
                        Owner = newDoc,
                        IdIndirectCostsDistributionMethod = method.IdIndirectCostsDistributionMethod
                    };

                context.PublicInstitutionEstimate_DistributionMethod.Add(newMethod);

                foreach (var activity in method.PublicInstitutionEstimate_DistributionActivity.ToList())
                {
                    var newActivity = new PublicInstitutionEstimate_DistributionActivity
                        {
                            Owner = newDoc,
                            Master = newMethod,
                            PublicInstitutionEstimate_Activity = newActivities.ContainsKey(activity.IdPublicInstitutionEstimate_Activity) ? newActivities[activity.IdPublicInstitutionEstimate_Activity] : null,

                            DirectOFG = activity.DirectOFG,
                            DirectPFG1 = activity.DirectPFG1,
                            DirectPFG2 = activity.DirectPFG2,
                            FactorOFG = activity.FactorOFG,
                            FactorPFG1 = activity.FactorPFG1,
                            FactorPFG2 = activity.FactorPFG2,

                            VolumeOFG = activity.VolumeOFG,
                            VolumePFG1 = activity.VolumePFG1,
                            VolumePFG2 = activity.VolumePFG2
                        };

                    context.PublicInstitutionEstimate_DistributionActivity.Add(newActivity);
                }

                foreach (var additionalParam in method.PublicInstitutionEstimate_DistributionAdditionalParam.ToList())
                {
                    var newParam = new PublicInstitutionEstimate_DistributionAdditionalParam
                        {
                            Master = newMethod,
                            Owner = newDoc,

                            IdKOSGU = additionalParam.IdKOSGU
                        };

                    context.PublicInstitutionEstimate_DistributionAdditionalParam.Add(newParam);
                }

                foreach (var expense in method.PublicInstitutionEstimate_IndirectExpenses.ToList())
                {
                    var newExpense = new PublicInstitutionEstimate_IndirectExpenses
                        {
                            Owner = newDoc,
                            Master = newMethod,

                            OFG = expense.OFG,
                            PFG1 = expense.PFG1,
                            PFG2 = expense.PFG2
                        };
                    newExpense.SetLineCostValues(expense);
                }
            }
            #endregion

            #region Расходы АУБУ

            foreach (var activity in doc.ActivitiesAUBU.ToList())
            {
                var newActivity = new PublicInstitutionEstimate_ActivityAUBU
                    {
                        Owner = newDoc,

                        IdActivity = activity.IdActivity,
                        IdContingent = activity.IdContingent,
                        IdUnitDimension = activity.IdUnitDimension,
                        IdIndicatorActivity = activity.IdIndicatorActivity
                    };

                context.PublicInstitutionEstimate_ActivityAUBU.Add(newActivity);

                foreach (var expense in activity.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense.ToList())
                {
                    var newExpense = new PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense
                        {
                            Owner = newDoc,
                            Master = newActivity,

                            AdditionalOFG = expense.AdditionalOFG,
                            AdditionalPFG1 = expense.AdditionalPFG1,
                            AdditionalPFG2 = expense.AdditionalPFG2,

                            OFG = expense.OFG,
                            PFG1 = expense.PFG1,
                            PFG2 = expense.PFG2,
                        };

                    newExpense.SetLineCostValues(expense);

                    context.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense.Add(newExpense);
                }

                foreach (var expense in activity.PublicInstitutionEstimate_FounderAUBUExpense.ToList())
                {
                    var newExpense = new PublicInstitutionEstimate_FounderAUBUExpense
                    {
                        Owner = newDoc,
                        Master = newActivity,

                        AdditionalOFG = expense.AdditionalOFG,
                        AdditionalPFG1 = expense.AdditionalPFG1,
                        AdditionalPFG2 = expense.AdditionalPFG2,

                        OFG = expense.OFG,
                        PFG1 = expense.PFG1,
                        PFG2 = expense.PFG2,

                        IdAuthorityOfExpenseObligation = expense.IdAuthorityOfExpenseObligation,
                        IsAUBU = expense.IsAUBU
                    };
                    newExpense.SetLineCostValues(expense);

                    context.PublicInstitutionEstimate_FounderAUBUExpense.Add(newExpense);
                }
            }

            #endregion

            return newDoc;
        }

        #endregion

        public void calculateRo_Activities(DataContext context, int[] items)
        {
            var expenses =
                context.PublicInstitutionEstimate_Expense.Where(r => r.IdOwner == Id)
                       .ToList()
                       .Where(r => items == null || items.Contains(r.IdMaster));

            var aeosbyact =
                context.PublicInstitutionEstimate_Activity.Where(r => r.IdOwner == Id).ToList()
                       .Where(r => (items == null || items.Contains(r.Id))).
                    Join(context.Activity_CodeAuthority.ToList(), piea => piea.IdActivity, aca => aca.IdOwner,
                         (piea, aca) => new CRuleAEO()
                             {
                                 Id = piea.Id,
                                 IdActivity = piea.IdActivity,
                                 IsMain = aca.IsMain,
                                 IdAuthorityOfExpenseObligation = aca.IdAuthorityOfExpenseObligation,
                                 hasChildAEO =
                                     context.AuthorityOfExpenseObligation.Any(
                                         e => e.IdParent == aca.IdAuthorityOfExpenseObligation)
                             }).ToList();

            var rulesbykosgu = context.RuleReferExpenceAsRoByKOSGU.ToList();

            foreach (var activity in aeosbyact.Select(s => s.Id))
            {
                var q1 = aeosbyact.Where(r => r.Id == activity);

                CRuleAEO fq;

                if (!q1.Any())
                {
                    continue;
                }
                
                if (q1.Count() != 1)
                {
                    var q2 = q1.Where(r => r.IsMain);
                    if (!q2.Any())
                    {
                        continue;
                    }
                    fq = q2.FirstOrDefault();
                }
                else
                {
                    fq = q1.FirstOrDefault();
                }

                var exps = expenses.Where(r => r.IdMaster == activity);
                if (!fq.hasChildAEO)
                {
                    foreach (var expense in exps)
                    {
                        expense.IdAuthorityOfExpenseObligation = fq.IdAuthorityOfExpenseObligation;
                    }
                }
                else
                {
                    foreach (var expense in exps)
                    {
                        var rule = rulesbykosgu.Where(r =>
                            r.AuthorityOfExpenseObligation.IdParent == fq.IdAuthorityOfExpenseObligation &&
                            r.IdKOSGU == expense.IdKOSGU);

                        if (!rule.Any())
                        {
                            continue;
                        }

                        expense.IdAuthorityOfExpenseObligation = rule.FirstOrDefault().IdAuthorityOfExpenseObligation;
                    }
                }
            }

            context.SaveChanges();
        }

        public class CRuleAEO
        {
            public int Id;
            public int IdActivity ;
            public bool IsMain ;
            public int IdAuthorityOfExpenseObligation ;
            public bool hasChildAEO ;
        }

        public void CalculateRo_Expenses(DataContext context, int[] items)
        {
            var expenses =
                context.PublicInstitutionEstimate_Expense
                       .Where(r => r.IdOwner == Id)
                       .ToList()
                       .Where(r => (!items.Any() || items.Contains(r.Id)));

            var ints = expenses.Select(s => s.IdMaster).ToList();
            var aeosByAct =
                context.PublicInstitutionEstimate_Activity.Where(r => ints.Contains(r.Id)).ToList().
                    Join( 
                        context.Activity_CodeAuthority.ToList(), 
                            piea => piea.IdActivity, 
                            aca => aca.IdOwner,
                            (piea, aca) => new CRuleAEO()
                            {
                                Id = piea.Id,
                                IdActivity = piea.IdActivity,
                                IsMain = aca.IsMain,
                                IdAuthorityOfExpenseObligation = aca.IdAuthorityOfExpenseObligation,
                                hasChildAEO = context.AuthorityOfExpenseObligation.Any(e => e.IdParent == aca.IdAuthorityOfExpenseObligation)
                            }).ToList();

            var rulesByKosgu = context.RuleReferExpenceAsRoByKOSGU.ToList();

            foreach (var activity in aeosByAct.Select(s => s.Id).ToList())
            {
                var q1 = aeosByAct.Where(r => r.Id == activity).ToList();

                CRuleAEO fq;

                if (!q1.Any())
                    continue;
                
                if (q1.Count() != 1)
                {
                    var q2 = q1.Where(r => r.IsMain).ToList();
                    if (!q2.Any())
                        continue;
                    
                    fq = q2.First();
                }
                else
                    fq = q1.First();
                
                var exps = expenses.Where(r => r.IdMaster == activity).ToList();
                if (!fq.hasChildAEO)
                {
                    foreach (var expense in exps)
                    {
                        expense.IdAuthorityOfExpenseObligation = fq.IdAuthorityOfExpenseObligation;
                    }
                }
                else
                {
                    foreach (var expense in exps)
                    {
                        var rule = rulesByKosgu.Where(r =>
                                                        r.AuthorityOfExpenseObligation.IdParent == fq.IdAuthorityOfExpenseObligation &&
                                                        r.IdKOSGU == expense.IdKOSGU)
                                                .ToList();

                        if (!rule.Any())
                        {
                            continue;
                        }

                        expense.IdAuthorityOfExpenseObligation = rule.FirstOrDefault().IdAuthorityOfExpenseObligation;
                    }
                }
            }

            context.SaveChanges();
        }

        public void calculateRo_ActivitiesAUBU(DataContext context, int[] items)
        {
            var expenses = context.PublicInstitutionEstimate_FounderAUBUExpense.Where(r => r.IdOwner == Id)
                       .ToList()
                       .Where(r => items == null || items.Contains(r.IdMaster));

            var aeosbyact =
                context.PublicInstitutionEstimate_ActivityAUBU
                       .Where(r => r.IdOwner == Id)
                       .ToList()
                       .Where(r => (items == null || items.Contains(r.Id))).
                    Join(context.Activity_CodeAuthority.ToList(), piea => piea.IdActivity, aca => aca.IdOwner,
                         (piea, aca) => new CRuleAEO()
                         {
                             Id = piea.Id,
                             IdActivity = piea.IdActivity,
                             IsMain = aca.IsMain,
                             IdAuthorityOfExpenseObligation = aca.IdAuthorityOfExpenseObligation,
                             hasChildAEO =
                                 context.AuthorityOfExpenseObligation.Any(
                                     e => e.IdParent == aca.IdAuthorityOfExpenseObligation)
                         }).ToList();

            var rulesbykosgu = context.RuleReferExpenceAsRoByKOSGU.ToList();

            foreach (var activity in aeosbyact.Select(s => s.Id))
            {
                var q1 = aeosbyact.Where(r => r.Id == activity);

                CRuleAEO fq;

                if (!q1.Any())
                {
                    continue;
                }

                if (q1.Count() != 1)
                {
                    var q2 = q1.Where(r => r.IsMain);
                    if (!q2.Any())
                    {
                        continue;
                    }
                    fq = q2.FirstOrDefault();
                }
                else
                {
                    fq = q1.FirstOrDefault();
                }

                var exps = expenses.Where(r => r.IdMaster == activity);
                if (!fq.hasChildAEO)
                {
                    foreach (var expense in exps)
                    {
                        expense.IdAuthorityOfExpenseObligation = fq.IdAuthorityOfExpenseObligation;
                    }
                }
                else
                {
                    foreach (var expense in exps)
                    {
                        var rule = rulesbykosgu.Where(r =>
                            r.AuthorityOfExpenseObligation.IdParent == fq.IdAuthorityOfExpenseObligation &&
                            r.IdKOSGU == expense.IdKOSGU);

                        if (!rule.Any())
                        {
                            continue;
                        }

                        expense.IdAuthorityOfExpenseObligation = rule.FirstOrDefault().IdAuthorityOfExpenseObligation;
                    }
                }
            }

            context.SaveChanges();
        }

        public void calculateRo_FounderAUBUExpenses(DataContext context, int[] items)
        {
            var expenses =
                context.PublicInstitutionEstimate_FounderAUBUExpense
                       .Where(r => r.IdOwner == Id)
                       .ToList()
                       .Where(r => (!items.Any() || items.Contains(r.Id)));

            var ints = expenses.Select(s => s.IdMaster).ToList();

            var aeosbyact =
                context.PublicInstitutionEstimate_ActivityAUBU.Where(r => ints.Contains(r.Id)).ToList().
                    Join(
                        context.Activity_CodeAuthority.ToList(),
                            piea => piea.IdActivity,
                            aca => aca.IdOwner,
                            (piea, aca) => new CRuleAEO()
                            {
                                Id = piea.Id,
                                IdActivity = piea.IdActivity,
                                IsMain = aca.IsMain,
                                IdAuthorityOfExpenseObligation = aca.IdAuthorityOfExpenseObligation,
                                hasChildAEO = context.AuthorityOfExpenseObligation.Any(e => e.IdParent == aca.IdAuthorityOfExpenseObligation)
                            }).ToList();

            var rulesbykosgu = context.RuleReferExpenceAsRoByKOSGU.ToList();

            foreach (var activity in aeosbyact.Select(s => s.Id))
            {
                var q1 = aeosbyact.Where(r => r.Id == activity);

                CRuleAEO fq;

                if (!q1.Any())
                {
                    continue;
                }

                if (q1.Count() != 1)
                {
                    var q2 = q1.Where(r => r.IsMain);
                    if (!q2.Any())
                    {
                        continue;
                    }
                    fq = q2.FirstOrDefault();
                }
                else
                {
                    fq = q1.FirstOrDefault();
                }

                var exps = expenses.Where(r => r.IdMaster == activity);
                if (!fq.hasChildAEO)
                {
                    foreach (var expense in exps)
                    {
                        expense.IdAuthorityOfExpenseObligation = fq.IdAuthorityOfExpenseObligation;
                    }
                }
                else
                {
                    foreach (var expense in exps)
                    {
                        var rule = rulesbykosgu.Where(r =>
                            r.AuthorityOfExpenseObligation.IdParent == fq.IdAuthorityOfExpenseObligation &&
                            r.IdKOSGU == expense.IdKOSGU);

                        if (!rule.Any())
                        {
                            continue;
                        }

                        expense.IdAuthorityOfExpenseObligation = rule.FirstOrDefault().IdAuthorityOfExpenseObligation;
                    }
                }
            }

            context.SaveChanges();
        }
    }
}

