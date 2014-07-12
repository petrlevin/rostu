using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Linq.SqlClient;
using System.Data.Objects;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Controls.DispatcherStrategies;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;
using Sbor.DbEnums;
using Sbor.Document;

using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Tablepart;
using TransactionScope = Platform.BusinessLogic.DataAccess.TransactionScope;
using ValueType = Sbor.DbEnums.ValueType;

using Platform.BusinessLogic.DataAccess;

namespace Sbor.DataProcessors
{
/*    class RoundDocs
    {
        #region Общие функции

        protected TransactionScope CreateTransaction()
        {
            var result = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                });

            return result;
        }

        private decimal MyRound(decimal v, int length = -2)
        {
            var p = (decimal)Math.Pow(10, length);
            return Math.Round(v * p) / p;
        }

        private void RoundField(TablePartEntity t, string name, int length = -2)
        {
            var v = (decimal?)t.GetValue(name);
            var nv = ((v ?? 0) == 0 ? v : MyRound((v ?? 0), length));
            if (nv != v) t.SetValue(name, nv);
        }

        private void RoundFields(object o, IEnumerable<string> flds, int length = -2)
        {
            var t = ((TablePartEntity)o);
            foreach (var fld in flds)
            {
                RoundField(t, fld, length);
            }
        }

        #endregion


        #region План финансово-хозяйственной деятельности

        private FinancialAndBusinessActivities CloneFinancialAndBusinessActivities(DataContext context,
                                                                                   FinancialAndBusinessActivities doc)
        {
            Clone cloner = new Clone(doc);
            FinancialAndBusinessActivities newDoc = (FinancialAndBusinessActivities)cloner.GetResult();
            //var newDoc = doc.Clone(doc, context);

            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = doc.Id;
            newDoc.IsRequireClarification = false;
            newDoc.IsApproved = false;
            newDoc.DateCommit = null;
            newDoc.ReasonClarification = null;
            newDoc.ReasonTerminate = null;
            newDoc.ReasonCancel = null;
            newDoc.DateTerminate = null;

            newDoc.IdSBP_BlankActual = doc.IdSBP_BlankActual;

            string[] nn = doc.Number.Split('.');
            if (nn.Count() == 1) nn = (doc.Number + ".0").Split('.');
            int cc = nn.Count();
            nn[cc - 1] = (cc > 1 ? int.Parse(nn[cc - 1]) + 1 : 1).ToString(CultureInfo.InvariantCulture);

            newDoc.Number = string.Join(".", nn);
            newDoc.Caption = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;
            doc.IdDocStatus = DocStatus.Changed;

            //newDoc.FilltpActivity(context, false);
            //context.SaveChanges();

            return newDoc;
        }

        /// <summary>
        /// Округление документа «План финансово-хозяйственной деятельности»
        /// </summary>
        public void RoundFinancialAndBusinessActivities(int topDocs = -1, int idSbp = 0, int length = -2)
        {
            var entity = Objects.ByName<Entity>(typeof(FinancialAndBusinessActivities).Name);
            string docCap = entity.Caption;

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            Debug.WriteLine("RoundDocs[{0}]:  Округление документов «{1}»", DateTime.Now, docCap);

            var docs = context.FinancialAndBusinessActivities.Where(w =>
                w.IdDocStatus != DocStatus.Draft
                && (idSbp == 0 || w.IdSBP == idSbp)
                && !context.FinancialAndBusinessActivities.Any(a => a.IdParent == w.Id)
                && (
                    context.FBA_CostActivities_value.Any(a =>
                        a.IdOwner == w.Id && (
                            (a.Value ?? 0) != Math.Round((a.Value ?? 0), length)
                            || (a.Value2 ?? 0) != Math.Round((a.Value2 ?? 0), length)
                        )
                    )
                    || context.FBA_IndirectCosts_value.Any(a =>
                        a.IdOwner == w.Id && (
                            a.Value != Math.Round(a.Value, length)
                        )
                    )
                )
            ).OrderBy(o => o.Number).ToList();

            if (topDocs >= 1)
            {
                docs = docs.Take(topDocs).ToList();
            }

            var fields1 = new[] { "Value", "Value2" };
            var fields2 = new[] { "Value" };

            Debug.WriteLine("RoundDocs[{0}]: Округляем {2} документов «{1}»", DateTime.Now, docCap, docs.Count());

            using (new ControlScope(new NoControlsStrategy(), false))
            {
                var manager = DataManagerFactory.Create<ToolsDataManager>(entity);
                var idOper = context.EntityOperation.Single(w => w.IdEntity == entity.Id && w.Operation.Name == "Process").Id;

                foreach (var d in docs)
                {
                    using (TransactionScope transaction = CreateTransaction())
                    {
                        Debug.WriteLine("RoundDocs[{0}]: Изменяем {1}", DateTime.Now, d.Caption);

                        var saveStatus = d.IdDocStatus;
                        FinancialAndBusinessActivities nd;

                        try
                        {
                            nd = CloneFinancialAndBusinessActivities(context, d);
                            foreach (var t in nd.CostActivities_values) RoundFields(t, fields1);
                            foreach (var t in nd.IndirectCosts_values) RoundFields(t, fields2);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: Произошла ошибка при клонировании документа {1} ошибка: {2}", DateTime.Now, d.Caption, ex.Message);
                            throw;
                        }

                        Debug.WriteLine("RoundDocs[{0}]: Обрабатываем {1}", DateTime.Now, nd.Caption);

                        try
                        {
                            //nd.ExecuteOperation(e => e.Process(context));
                            //context.SaveChanges();
                            manager.ExecuteOperation(nd.Id, idOper);
                            nd.IdDocStatus = saveStatus;
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: При обработке {1} ошибка : {2}", DateTime.Now, nd.Caption, ex.Message);
                            throw;
                        }

                        transaction.Complete();
                    }
                }
            }

            Debug.WriteLine("RoundDocs[{0}]: Закончено округление документов «{1}»", DateTime.Now, docCap);
        }

        #endregion


        #region Смета казенного учреждения

        private PublicInstitutionEstimate ClonePublicInstitutionEstimate(DataContext context,
                                                                         PublicInstitutionEstimate doc)
        {
            Clone cloner = new Clone(doc);
            PublicInstitutionEstimate newDoc = (PublicInstitutionEstimate)cloner.GetResult();
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = doc.Id;
            newDoc.IsRequireClarification = false;
            newDoc.IsApproved = false;
            newDoc.DateCommit = null;
            //newDoc.ReasonClarification = null;
            newDoc.ReasonTerminate = null;
            //newDoc.ReasonCancel = null;
            newDoc.DateTerminate = null;

            newDoc.IdSBP_BlankActual = doc.IdSBP_BlankActual;
            newDoc.IdSBP_BlankActualAuBu = doc.IdSBP_BlankActualAuBu;

            string[] nn = doc.Number.Split('.');
            if (nn.Count() == 1) nn = (doc.Number + ".0").Split('.');
            int cc = nn.Count();
            nn[cc - 1] = (cc > 1 ? int.Parse(nn[cc - 1]) + 1 : 1).ToString(CultureInfo.InvariantCulture);

            newDoc.Number = string.Join(".", nn);
            newDoc.Caption = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;
            doc.IdDocStatus = DocStatus.Changed;

            return newDoc;
        }

        /// <summary>
        /// Округление документа «Смета казенного учреждения»
        /// </summary>
        public void RoundPublicInstitutionEstimate(int topDocs = -1, int idSbp = 0, int length = -2)
        {
            var entity = Objects.ByName<Entity>(typeof(PublicInstitutionEstimate).Name);
            string docCap = entity.Caption;

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            Debug.WriteLine("RoundDocs[{0}]:  Округление документов «{1}»", DateTime.Now, docCap);

            var docs = context.PublicInstitutionEstimate.Where(w =>
                w.IdDocStatus != DocStatus.Draft
                && (idSbp == 0 || w.IdSBP == idSbp)
                && !context.PublicInstitutionEstimate.Any(a => a.IdParent == w.Id)
                && (w.SBP.IsFounder ||
                    context.PublicInstitutionEstimate_Expense.Any(a =>
                        a.IdOwner == w.Id && (
                            (a.OFG ?? 0) != Math.Round((a.OFG ?? 0), length)
                            || (a.PFG1 ?? 0) != Math.Round((a.PFG1 ?? 0), length)
                            || (a.PFG2 ?? 0) != Math.Round((a.PFG2 ?? 0), length)
                            || (a.AdditionalOFG ?? 0) != Math.Round((a.AdditionalOFG ?? 0), length)
                            || (a.AdditionalPFG1 ?? 0) != Math.Round((a.AdditionalPFG1 ?? 0), length)
                            || (a.AdditionalPFG2 ?? 0) != Math.Round((a.AdditionalPFG2 ?? 0), length)
                        )
                    )
                    || context.PublicInstitutionEstimate_IndirectExpenses.Any(a =>
                        a.IdOwner == w.Id && (
                            (a.OFG ?? 0) != Math.Round((a.OFG ?? 0), length)
                            || (a.PFG1 ?? 0) != Math.Round((a.PFG1 ?? 0), length)
                            || (a.PFG2 ?? 0) != Math.Round((a.PFG2 ?? 0), length)
                        )
                    )
                    || context.PublicInstitutionEstimate_FounderAUBUExpense.Any(a =>
                        a.IdOwner == w.Id && (
                            (a.OFG ?? 0) != Math.Round((a.OFG ?? 0), length)
                            || (a.PFG1 ?? 0) != Math.Round((a.PFG1 ?? 0), length)
                            || (a.PFG2 ?? 0) != Math.Round((a.PFG2 ?? 0), length)
                            || (a.AdditionalOFG ?? 0) != Math.Round((a.AdditionalOFG ?? 0), length)
                            || (a.AdditionalPFG1 ?? 0) != Math.Round((a.AdditionalPFG1 ?? 0), length)
                            || (a.AdditionalPFG2 ?? 0) != Math.Round((a.AdditionalPFG2 ?? 0), length)
                    )
                ))
            ).OrderBy(o => new { o.SBP.IsFounder, o.Number }).ToList();

            if (topDocs >= 1)
            {
                docs = docs.Take(topDocs).ToList();
            }

            var fields1 = new[] { "OFG", "PFG1", "PFG2", "AdditionalOFG", "AdditionalPFG1", "AdditionalPFG2" };
            var fields2 = new[] { "OFG", "PFG1", "PFG2" };

            Debug.WriteLine("RoundDocs[{0}]: Округляем {2} документов «{1}»", DateTime.Now, docCap, docs.Count());

            using (new ControlScope(new NoControlsStrategy(), false))
            {
                var manager = DataManagerFactory.Create<ToolsDataManager>(entity);
                var idOper = context.EntityOperation.Single(w => w.IdEntity == entity.Id && w.Operation.Name == "Process").Id;

                foreach (var d in docs)
                {
                    using (TransactionScope transaction = CreateTransaction())
                    {
                        Debug.WriteLine("RoundDocs[{0}]: Изменяем {1}", DateTime.Now, d.Caption);

                        var saveStatus = d.IdDocStatus;
                        PublicInstitutionEstimate nd;

                        try
                        {
                            nd = ClonePublicInstitutionEstimate(context, d);
                            foreach (var t in nd.Expenses) RoundFields(t, fields1);
                            foreach (var t in nd.IndirectExpenses) RoundFields(t, fields2);
                            foreach (var t in nd.FounderAUBUExpenses) RoundFields(t, fields1);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: Произошла ошибка при клонировании документа {1} ошибка: {2}", DateTime.Now, d.Caption, ex.Message);
                            throw;
                        }

                        if (nd.SBP.IsFounder)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: Перезаполняем в {1} ТЧ {2}", DateTime.Now, nd.Caption,
                                            "Расходы автономных и бюджетных учреждений");
                            try
                            {
                                nd.FillData_ExpenseAloneSubjects(context);
                                context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("RoundDocs[{0}]: При перезаполении {1} ТЧ {2} ошибка : {3}", DateTime.Now, nd.Caption, "Расходы автономных и бюджетных учреждений", ex.Message);
                                throw;
                            }
                        }

                        Debug.WriteLine("RoundDocs[{0}]: Обрабатываем {1}", DateTime.Now, nd.Caption);

                        try
                        {
                            //nd.ExecuteOperation(e => e.Process(context));
                            //context.SaveChanges();
                            manager.ExecuteOperation(nd.Id, idOper);
                            nd.IdDocStatus = saveStatus;
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: При обработке {1} ошибка : {2}", DateTime.Now, nd.Caption, ex.Message);
                            throw;
                        }

                        transaction.Complete();
                    }
                }
            }

            Debug.WriteLine("RoundDocs[{0}]: Закончено округление документов «{1}»", DateTime.Now, docCap);
        }

        #endregion


        #region План деятельности

        private PlanActivity ClonePlanActivity(DataContext context, PlanActivity doc)
        {
            Clone cloner = new Clone(doc);
            PlanActivity newDoc = (PlanActivity)cloner.GetResult();

            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = doc.Id;
            newDoc.IsRequireClarification = false;
            newDoc.DateCommit = null;
            newDoc.ReasonTerminate = null;
            newDoc.DateTerminate = null;
            newDoc.DateLastEdit = null;

            newDoc.IdSBP_BlankActual = doc.IdSBP_BlankActual;
            if (!newDoc.IdSBP_BlankActual.HasValue)
            {
                var idchekblanktype = doc.SBP.SBPType == SBPType.TreasuryEstablishment ? (byte)BlankType.BringingKU : (byte)BlankType.BringingAUBU;

                var newBlank = context.SBP_BlankHistory.Where(r =>
                        r.IdBudget == doc.IdBudget && r.IdOwner == doc.SBP.IdParent && r.IdBlankType == idchekblanktype
                ).OrderByDescending(o => o.DateCreate).FirstOrDefault();

                if (newBlank != null) newDoc.IdSBP_BlankActual = newBlank.Id;
            }

            string[] nn = doc.Number.Split('.');
            if (nn.Count() == 1) nn = (doc.Number + ".0").Split('.');
            int cc = nn.Count();
            nn[cc - 1] = (cc > 1 ? int.Parse(nn[cc - 1]) + 1 : 1).ToString(CultureInfo.InvariantCulture);

            newDoc.Number = string.Join(".", nn);
            newDoc.Caption = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;
            doc.IdDocStatus = DocStatus.Changed;

            return newDoc;
        }

        public class FinProv
        {
            public byte? IdExpenseObligationType { get; set; }
            public int? IdFinanceSource { get; set; }
            public int? IdKFO { get; set; }
            public int? IdKVSR { get; set; }
            public int? IdRZPR { get; set; }
            public int? IdKCSR { get; set; }
            public int? IdKVR { get; set; }
            public int? IdKOSGU { get; set; }
            public int? IdDFK { get; set; }
            public int? IdDKR { get; set; }
            public int? IdDEK { get; set; }
            public int? IdCodeSubsidy { get; set; }
            public int? IdBranchCode { get; set; }

            public int IdHierarchyPeriod { get; set; }
            public bool IsMeansAUBU { get; set; }

            public string key { get; set; }

            public decimal Value { get; set; }
        }

        private void FillFinancialProvisions(DataContext context, PlanActivity d, bool isMandatoryOnly = false)
        {
            string fields = string.Empty;
            string fields2 = string.Empty;
            string fields3 = string.Empty;
            string key = string.Empty;
            foreach (var p in typeof(ISBP_Blank).GetProperties())
            {
                var fn = p.Name.Replace("BlankValueType_", "");
                fields += fn + (fn.Equals("IdExpenseObligationType") ? " tinyint" : " int") + ",";
                if (isMandatoryOnly)
                {
                    fields2 += string.Format("case when isnull(bl.{0}, {2}) == {3} then el.{1} else null end as {1}, ", p.Name, fn, (byte)BlankValueType.Empty, (byte)BlankValueType.Mandatory);
                    fields3 += string.Format("case when isnull(bl.{0}, {2}) == {3} then el.{1} else null end, ", p.Name, fn, (byte)BlankValueType.Empty, (byte)BlankValueType.Mandatory);
                }
                else
                {
                    fields2 += string.Format("case when isnull(bl.{0}, {2}) != {2} then el.{1} else null end as {1}, ", p.Name, fn, (byte)BlankValueType.Empty);
                    fields3 += string.Format("case when isnull(bl.{0}, {2}) != {2} then el.{1} else null end, ", p.Name, fn, (byte)BlankValueType.Empty);
                }
                key += " isnull(cast(" + fn + " as nvarchar(max)),'') +','+ ";
            }

            string queryInsert = string.Format(
                "declare @recs table ([key] nvarchar(max), " + fields + " idHierarchyPeriod int, isMeansAUBU bit, Value numeric(18,2) ) " +

                "insert into @recs " +
                "select " + key + " isnull(cast(isMeansAUBU as nvarchar(max)),'') as [key], * from (" +
                "select " + fields2 + " lva.idHierarchyPeriod, lva.isMeansAUBU, sum(lva.Value) as Value " +
                "from reg.LimitVolumeAppropriations as lva " +
                "   inner join reg.EstimatedLine as el on el.id = lva.idEstimatedLine and el.idSBP = {8} " +
                "   inner join ref.KFO as kfo on kfo.id = el.idKFO and kfo.IsIncludedInBudget = 1 " +
                "   inner join ref.FinanceSource as fs on fs.id = el.idFinanceSource and fs.idFinanceSourceType != {11} " +
                "   inner join ref.SBP as sbp on sbp.id = el.idSBP and sbp.idSBPType in ({3},{4},{5}) " +
                //"   inner join tp.SBP_blank as bl on bl.idOwner = sbp.idParent and bl.idBudget = lva.idBudget " +
                //"       and bl.idBlankType = case " +
                //"       when sbp.idSBPType in ({4},{5}) then {1} " +
                //"       when sbp.idSBPType = {3} then {2} " +
                //"       else null end " +
                "   inner join tp.SBP_BlankHistory as bl on bl.id = {10} " +
                "where lva.idBudget = {9} and lva.idValueType in ({6},{7}) and isnull(lva.HasAdditionalNeed,0) = 0 " +
                "group by " + fields3 + " lva.idHierarchyPeriod, lva.isMeansAUBU " +
                "having sum(lva.Value)!= 0 ) as x " +

                "select * from @recs ",

                DocStatus.Draft,
                (byte)BlankType.BringingAUBU, (byte)BlankType.BringingKU,
                (byte)SBPType.TreasuryEstablishment, (byte)SBPType.BudgetEstablishment, (byte)SBPType.IndependentEstablishment,
                (byte)ValueType.Justified, (byte)ValueType.JustifiedFBA,
                d.IdSBP, d.IdBudget,
                d.IdSBP_BlankActual,
                (byte)FinanceSourceType.Remains
            );
            var result = context.Database.SqlQuery<FinProv>(queryInsert).ToList();

            foreach (var g in result.GroupBy(g => g.key))
            {
                var first = g.First();

                var newKbk = context.PlanActivity_KBKOfFinancialProvision.Create();
                newKbk.Owner = d;
                newKbk.IsMeansAUBU = first.IsMeansAUBU;
                foreach (var p in typeof(ILineCost).GetProperties())
                {
                    newKbk.SetValue(p.Name, first.GetValue(p.Name));
                }
                context.PlanActivity_KBKOfFinancialProvision.Add(newKbk);

                foreach (var s in g)
                {
                    var newPeriod = context.PlanActivity_PeriodsOfFinancialProvision.Create();
                    newPeriod.Owner = d;
                    newPeriod.Master = newKbk;
                    newPeriod.IdHierarchyPeriod = s.IdHierarchyPeriod;
                    newPeriod.Value = s.Value;
                    context.PlanActivity_PeriodsOfFinancialProvision.Add(newPeriod);
                }
            }

        }

        /// <summary>
        /// Округление документа «План деятельности»
        /// </summary>
        public void RoundPlanActivity(int topDocs = -1, int idSbp = 0, int idSession = 1, bool isMandatoryOnly = false, int length = -2)
        {
            var entity = Objects.ByName<Entity>(typeof(PlanActivity).Name);
            string docCap = entity.Caption;

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            Debug.WriteLine("RoundDocs[{0}]:  Округление документов «{1}»", DateTime.Now, docCap);

            // если у нас есть ИД сессии и мы не выбрали конкретный СБП, то получаем "маркер" для пометки обработанных документов
            // это нужно чтобы при запуске повторно не обработывать уже обработанные документы документы
            string session = (idSession != 0) && (idSbp == 0) ? string.Format("RoundDocs-{0}-", idSession) : null;

            // получаем список документов
            var docs = context.PlanActivity.Where(w =>
                w.IdDocStatus != DocStatus.Draft
                && (idSbp == 0 || w.IdSBP == idSbp)
                && !context.PlanActivity.Any(a => a.IdParent == w.Id)
                && (string.IsNullOrEmpty(session) || !w.Description.Contains(session))
                && context.PlanActivity_PeriodsOfFinancialProvision.Any(a => a.IdOwner == w.Id)
            ).OrderBy(o => o.Number).ToList();

            if (topDocs >= 1)
            {
                docs = docs.Take(topDocs).ToList();
            }

            Debug.WriteLine("RoundDocs[{0}]: Округляем {2} документов «{1}»", DateTime.Now, docCap, docs.Count());

            using (new ControlScope(new NoControlsStrategy(), false))
            {
                var manager = DataManagerFactory.Create<ToolsDataManager>(entity);
                var idOper = context.EntityOperation.Single(w => w.IdEntity == entity.Id && w.Operation.Name == "Process").Id;

                foreach (var d in docs)
                {
                    using (TransactionScope transaction = CreateTransaction())
                    {
                        Debug.WriteLine("RoundDocs[{0}]: Изменяем {1}", DateTime.Now, d.Caption);

                        var saveStatus = d.IdDocStatus;
                        PlanActivity nd;

                        try
                        {
                            nd = ClonePlanActivity(context, d);
                            if (!string.IsNullOrEmpty(session)) nd.Description = session + "\r\n" + (nd.Description ?? "");
                            foreach (var t in nd.PeriodsOfFinancialProvisions.ToList()) context.PlanActivity_PeriodsOfFinancialProvision.Remove(t);
                            foreach (var t in nd.KBKOfFinancialProvisions.ToList()) context.PlanActivity_KBKOfFinancialProvision.Remove(t);
                            FillFinancialProvisions(context, nd, isMandatoryOnly);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: Произошла ошибка при клонировании документа {1} ошибка: {2}", DateTime.Now, d.Caption, ex.Message);
                            throw;
                        }

                        Debug.WriteLine("RoundDocs[{0}]: Обрабатываем {1}", DateTime.Now, nd.Caption);

                        try
                        {
                            //nd.ExecuteOperation(e => e.Process(context));
                            //context.SaveChanges();
                            manager.ExecuteOperation(nd.Id, idOper);
                            nd.IdDocStatus = saveStatus;
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: При обработке {1} ошибка : {2}", DateTime.Now, nd.Caption, ex.Message);
                            throw;
                        }

                        transaction.Complete();
                    }
                }
            }

            Debug.WriteLine("RoundDocs[{0}]: Закончено округление документов «{1}»", DateTime.Now, docCap);
        }

        #endregion


        #region Предельные объемы бюджетных ассигнований

        private LimitBudgetAllocations CloneLimitBudgetAllocations(DataContext context, LimitBudgetAllocations doc)
        {
            Clone cloner = new Clone(doc);
            LimitBudgetAllocations newDoc = (LimitBudgetAllocations)cloner.GetResult();

            newDoc.Date = DateTime.Now.Date;

            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.IdParent = doc.Id;
            newDoc.IsRequireClarification = false;
            newDoc.DateCommit = null;
            newDoc.ReasonClarification = null;
            newDoc.DateLastEdit = null;

            newDoc.IdSBP_BlankActual = doc.IdSBP_BlankActual;
            if (!newDoc.IdSBP_BlankActual.HasValue)
            {
                var idchekblanktype = doc.SBP.SBPType == DbEnums.SBPType.GeneralManager ? (byte)DbEnums.BlankType.BringingGRBS : (byte)DbEnums.BlankType.BringingRBS;
                var idcheksbp = doc.SBP.SBPType == DbEnums.SBPType.GeneralManager ? doc.IdSBP : doc.IdParent;

                var newBlank = context.SBP_BlankHistory.Where(r =>
                        r.IdBudget == doc.IdBudget && r.IdOwner == idcheksbp && r.IdBlankType == idchekblanktype
                ).OrderByDescending(o => o.DateCreate).FirstOrDefault();

                if (newBlank != null) newDoc.IdSBP_BlankActual = newBlank.Id;
            }

            string[] nn = doc.Number.Split('.');
            if (nn.Count() == 1) nn = (doc.Number + ".0").Split('.');
            int cc = nn.Count();
            nn[cc - 1] = (cc > 1 ? int.Parse(nn[cc - 1]) + 1 : 1).ToString(CultureInfo.InvariantCulture);

            newDoc.Number = string.Join(".", nn);
            newDoc.Caption = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;
            doc.IdDocStatus = DocStatus.Changed;

            return newDoc;
        }

        public class LimitAlloc
        {
            public byte? IdExpenseObligationType { get; set; }
            public int? IdFinanceSource { get; set; }
            public int? IdKFO { get; set; }
            public int? IdKVSR { get; set; }
            public int? IdRZPR { get; set; }
            public int? IdKCSR { get; set; }
            public int? IdKVR { get; set; }
            public int? IdKOSGU { get; set; }
            public int? IdDFK { get; set; }
            public int? IdDKR { get; set; }
            public int? IdDEK { get; set; }
            public int? IdCodeSubsidy { get; set; }
            public int? IdBranchCode { get; set; }

            public decimal? OFG { get; set; }
            public decimal? PFG1 { get; set; }
            public decimal? PFG2 { get; set; }
        }

        private void FillLimitAllocations(DataContext context, LimitBudgetAllocations d, bool isMandatoryOnly = false)
        {
            string fields = string.Empty;
            string fields2 = string.Empty;
            string fields3 = string.Empty;
            foreach (var p in typeof(ISBP_Blank).GetProperties())
            {
                var fn = p.Name.Replace("BlankValueType_", "");
                fields += fn + (fn.Equals("IdExpenseObligationType") ? " tinyint" : " int") + ",";
                if (isMandatoryOnly)
                {
                    fields2 += string.Format("case when isnull(bl.{0}, {2}) == {3} then el.{1} else null end as {1}, ", p.Name, fn, (byte)BlankValueType.Empty, (byte)BlankValueType.Mandatory);
                    fields3 += string.Format("case when isnull(bl.{0}, {2}) == {3} then el.{1} else null end, ", p.Name, fn, (byte)BlankValueType.Empty, (byte)BlankValueType.Mandatory);
                }
                else
                {
                    fields2 += string.Format("case when isnull(bl.{0}, {2}) != {2} then el.{1} else null end as {1}, ", p.Name, fn, (byte)BlankValueType.Empty);
                    fields3 += string.Format("case when isnull(bl.{0}, {2}) != {2} then el.{1} else null end, ", p.Name, fn, (byte)BlankValueType.Empty);
                }
            }

            string queryInsert = string.Format(
                "declare @SBPs table (idSBP int, idChidrenSBP int) " +

                "insert into @SBPs " +
                "select {0} as idSBP, sbp.id as idChidrenSBP " +
                "from dbo.GetChildrens({0}, -2013265747, 1) as s " +
                "   inner join ref.SBP as sbp on sbp.id = s.id and sbp.idSBPType = {1} " +

                "select " + fields2 + " " +
                "   sum(case when year(hp.DateStart) = b.[Year]+0 then lva.Value else null end) as OFG, " +
                "   sum(case when year(hp.DateStart) = b.[Year]+1 then lva.Value else null end) as PFG1, " +
                "   sum(case when year(hp.DateStart) = b.[Year]+2 then lva.Value else null end) as PFG2 " +
                "from reg.LimitVolumeAppropriations as lva " +
                "   inner join ref.HierarchyPeriod as hp on hp.id = lva.idHierarchyPeriod " +
                "   inner join ref.Budget as b on b.id = lva.idBudget " +
                "   inner join reg.EstimatedLine as el on el.id = lva.idEstimatedLine " +
                "   inner join @SBPs as f on f.idChidrenSBP = el.idSBP " +
                "   inner join tp.SBP_BlankHistory as bl on bl.id = {2} " +
                "where lva.idBudget = {3} and lva.idValueType = {4} and isnull(lva.HasAdditionalNeed,0) = 0 " +
                "group by " + fields3.Substring(0, fields3.LastIndexOf(',')) + " " +
                "having " +
                "   sum(case when year(hp.DateStart) = b.[Year]+0 then lva.Value else 0 end) != 0 " +
                "   or sum(case when year(hp.DateStart) = b.[Year]+1 then lva.Value else 0 end) != 0 " +
                "   or sum(case when year(hp.DateStart) = b.[Year]+2 then lva.Value else 0 end) != 0 ",

                d.IdSBP, (byte)SBPType.TreasuryEstablishment,
                d.IdSBP_BlankActual,
                d.IdBudget, (byte)ValueType.Justified
            );
            var result = context.Database.SqlQuery<LimitAlloc>(queryInsert).ToList();

            foreach (var rec in result)
            {
                var newRec = context.LimitBudgetAllocations_LimitAllocations.Create();
                newRec.Owner = d;
                foreach (var p in typeof(ILineCost).GetProperties())
                {
                    newRec.SetValue(p.Name, rec.GetValue(p.Name));
                }
                newRec.OFG = rec.OFG;
                newRec.PFG1 = rec.PFG1;
                newRec.PFG2 = rec.PFG2;
                context.LimitBudgetAllocations_LimitAllocations.Add(newRec);
            }
        }

        /// <summary>
        /// Округление документа «План деятельности»
        /// </summary>
        public void RoundLimitBudgetAllocations(int topDocs = -1, int idSbp = 0, int idSession = 1, bool isMandatoryOnly = false, int length = -2)
        {
            var entity = Objects.ByName<Entity>(typeof(LimitBudgetAllocations).Name);
            string docCap = entity.Caption;

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            Debug.WriteLine("RoundDocs[{0}]:  Округление документов «{1}»", DateTime.Now, docCap);

            // если у нас есть ИД сессии и мы не выбрали конкретный СБП, то получаем "маркер" для пометки обработанных документов
            // это нужно чтобы при запуске повторно не обработывать уже обработанные документы документы
            string session = (idSession != 0) && (idSbp == 0) ? string.Format("RoundDocs-{0}-", idSession) : null;

            // получаем список документов
            var docs = context.LimitBudgetAllocations.Where(w =>
                w.IdDocStatus != DocStatus.Draft
                && (idSbp == 0 || w.IdSBP == idSbp)
                && !context.LimitBudgetAllocations.Any(a => a.IdParent == w.Id)
                && (string.IsNullOrEmpty(session) || !w.Description.Contains(session))
                && context.LimitBudgetAllocations_LimitAllocations.Any(a => a.IdOwner == w.Id)
            ).OrderBy(o => o.Number).ToList();

            if (topDocs >= 1)
            {
                docs = docs.Take(topDocs).ToList();
            }

            Debug.WriteLine("RoundDocs[{0}]: Округляем {2} документов «{1}»", DateTime.Now, docCap, docs.Count());

            using (new ControlScope(new NoControlsStrategy(), false))
            {
                var manager = DataManagerFactory.Create<ToolsDataManager>(entity);
                var idOper = context.EntityOperation.Single(w => w.IdEntity == entity.Id && w.Operation.Name == "Process").Id;

                foreach (var d in docs)
                {
                    using (TransactionScope transaction = CreateTransaction())
                    {
                        Debug.WriteLine("RoundDocs[{0}]: Изменяем {1}", DateTime.Now, d.Caption);

                        var saveStatus = d.IdDocStatus;
                        LimitBudgetAllocations nd;

                        try
                        {
                            nd = CloneLimitBudgetAllocations(context, d);
                            if (!string.IsNullOrEmpty(session)) nd.Description = session + "\r\n" + (nd.Description ?? "");
                            foreach (var t in nd.LimitAllocations.ToList()) context.LimitBudgetAllocations_LimitAllocations.Remove(t);
                            FillLimitAllocations(context, nd, isMandatoryOnly);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: Произошла ошибка при клонировании документа {1} ошибка: {2}", DateTime.Now, d.Caption, ex.Message);
                            throw;
                        }

                        Debug.WriteLine("RoundDocs[{0}]: Обрабатываем {1}", DateTime.Now, nd.Caption);

                        try
                        {
                            //nd.ExecuteOperation(e => e.Process(context));
                            //context.SaveChanges();
                            manager.ExecuteOperation(nd.Id, idOper);
                            nd.IdDocStatus = saveStatus;
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("RoundDocs[{0}]: При обработке {1} ошибка : {2}", DateTime.Now, nd.Caption, ex.Message);
                            throw;
                        }

                        transaction.Complete();
                    }
                }
            }

            Debug.WriteLine("RoundDocs[{0}]: Закончено округление документов «{1}»", DateTime.Now, docCap);
        }

        #endregion


        #region Деятельность ведомства

        private ActivityOfSBP CloneActivityOfSBP(DataContext context, ActivityOfSBP doc)
        {
            Clone cloner = new Clone(doc);
            ActivityOfSBP newDoc = (ActivityOfSBP)cloner.GetResult();

            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = doc.Id;
            newDoc.DateCommit = null;
            newDoc.DateTerminate = null;
            newDoc.HasAdditionalNeed = doc.HasAdditionalNeed;
            newDoc.DateCommit = null;
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.DocType = doc.DocType;

            var budgets = context.Budget.Where(r => r.IdRefStatus == (byte)RefStatus.Work && r.IdPublicLegalFormation == doc.IdPublicLegalFormation);
            foreach (var bgt in budgets)
            {
                bool isExistsBlank = context.ActivityOfSBP_SBPBlankActual.Any(r => r.IdOwner == doc.Id && r.SBP_BlankHistory.IdBudget == bgt.Id);
                if (!isExistsBlank) // обновляем только если не было бланка !!!
                {
                    var newBlank = context.SBP_BlankHistory.Where(r =>
                        (
                            (doc.SBP.SBPType == DbEnums.SBPType.GeneralManager && r.IdOwner == doc.SBP.Id)
                            || (doc.SBP.SBPType == DbEnums.SBPType.Manager && r.IdOwner == doc.SBP.IdParent)
                        )
                        && r.IdBlankType == (byte)DbEnums.BlankType.FormationGRBS
                        && r.IdBudget == bgt.Id
                    ).OrderByDescending(o => o.DateCreate).FirstOrDefault();

                    if (newBlank != null)
                    {
                        var newBlankActual = context.ActivityOfSBP_SBPBlankActual.Create();
                        newBlankActual.IdOwner = newDoc.Id;
                        newBlankActual.IdSBP_BlankHistory = newBlank.Id;
                        context.ActivityOfSBP_SBPBlankActual.Add(newBlankActual);
                    }
                }
            }

            string[] nn = doc.Number.Split('.');
            if (nn.Count() == 1) nn = (doc.Number + ".0").Split('.');
            int cc = nn.Count();
            nn[cc - 1] = (cc > 1 ? int.Parse(nn[cc - 1]) + 1 : 1).ToString(CultureInfo.InvariantCulture);

            newDoc.Number = string.Join(".", nn);

            newDoc.Header = newDoc.ToString();

            context.Entry(newDoc).State = EntityState.Added;

            doc.IdDocStatus = DocStatus.Changed;

            return newDoc;
        }

        public class ActivityResourceMaintenance
        {
            public int? IdMaster { get; set; }

            public int IdActivity { get; set; }
            public int? IdContingent { get; set; }
            public int IdBudget { get; set; }
            public int IdSBP { get; set; }

            public byte? IdExpenseObligationType { get; set; }
            public int? IdFinanceSource { get; set; }
            public int? IdKFO { get; set; }
            public int? IdKVSR { get; set; }
            public int? IdRZPR { get; set; }
            public int? IdKCSR { get; set; }
            public int? IdKVR { get; set; }
            public int? IdKOSGU { get; set; }
            public int? IdDFK { get; set; }
            public int? IdDKR { get; set; }
            public int? IdDEK { get; set; }
            public int? IdCodeSubsidy { get; set; }
            public int? IdBranchCode { get; set; }

            public int? IdAuthorityOfExpenseObligation { get; set; }
            public int? IdOKATO { get; set; }

            public int IdHierarchyPeriod { get; set; }

            public string key1 { get; set; }
            public string key2 { get; set; }

            public decimal Value { get; set; }
        }

        public void FillActivityResourceMaintenance(DataContext context, ActivityOfSBP d, bool isMandatoryOnly = false)
        {
            string fields = string.Empty;
            string fields2 = string.Empty;
            string fields3 = string.Empty;
            string key1 = string.Empty;
            string key2 = string.Empty;
            foreach (var p in typeof(ISBP_Blank).GetProperties())
            {
                var fn = p.Name.Replace("BlankValueType_", "");
                fields += fn + (fn.Equals("IdExpenseObligationType") ? " tinyint" : " int") + ",";
                if (isMandatoryOnly)
                {
                    fields2 += string.Format("case when isnull(bl.{0}, {2}) == {3} then el.{1} else null end as {1}, ", p.Name, fn, (byte)BlankValueType.Empty, (byte)BlankValueType.Mandatory);
                    fields3 += string.Format("case when isnull(bl.{0}, {2}) == {3} then el.{1} else null end, ", p.Name, fn, (byte)BlankValueType.Empty, (byte)BlankValueType.Mandatory);
                }
                else
                {
                    fields2 += string.Format("case when isnull(bl.{0}, {2}) != {2} then el.{1} else null end as {1}, ", p.Name, fn, (byte)BlankValueType.Empty);
                    fields3 += string.Format("case when isnull(bl.{0}, {2}) != {2} then el.{1} else null end, ", p.Name, fn, (byte)BlankValueType.Empty);
                }
                key2 += " isnull(cast(" + fn + " as nvarchar(max)),'') +','+ ";
            }
            var addkeys2 = new string[] { "IdBudget", "IdAuthorityOfExpenseObligation", "IdOKATO" };
            foreach (var k in addkeys2)
            {
                key2 += " isnull(cast(" + k + " as nvarchar(max)),'') +','+ ";
            }
            var addkeys1 = new string[] { "IdActivity", "IdContingent", "IdSBP" };
            foreach (var k in addkeys1)
            {
                var kkk = " isnull(cast(" + k + " as nvarchar(max)),'') +','+ ";
                key1 += kkk;
                key2 += kkk;
            }

            string queryInsert = string.Format(
                "declare @SBPs table (idSBP int, idChidrenSBP int) " +

                "insert into @SBPs " +
                "select f.id as idSBP, sbp.id as idChidrenSBP " +
                "from ref.SBP as f " +
                "   cross apply dbo.GetChildrens({0}, -2013265747, 1) as s " +
                "   inner join ref.SBP as sbp on sbp.id = s.id and sbp.idSBPType = {1} " +
                "where f.id = {0} or (f.idParent = {0} and f.idSBPType = {4}) " +

                "select " + key1.Substring(0, key1.LastIndexOf("+','+")) + " as key1, " + key2.Substring(0, key2.LastIndexOf("+','+")) + " as key2, * " +
                "from ( select a.id as idMaster, tc.idActivity, tc.idContingent, lva.idBudget, f.idSBP, " + fields2 +
                "   lva.idAuthorityOfExpenseObligation, lva.idOKATO, lva.idHierarchyPeriod, sum(lva.Value) as Value " +
                "from reg.LimitVolumeAppropriations as lva " +
                "   inner join reg.TaskCollection as tc on tc.id = lva.idTaskCollection and tc.idPublicLegalFormation = lva.idPublicLegalFormation " +
                "   inner join reg.EstimatedLine as el on el.id = lva.idEstimatedLine " +
                "   inner join @SBPs as f on f.idChidrenSBP = el.idSBP " +
                "   inner join tp.ActivityOfSBP_Activity as a on a.idOwner = {2} " +
                "       and a.idActivity = tc.idActivity and isnull(a.idContingent,0) = isnull(tc.idContingent,0) and a.idSBP = f.idSBP " +
                "   inner join tp.ActivityOfSBP_SBPBlankActual as ab on ab.IdOwner = {2} " +
                "   inner join tp.SBP_BlankHistory as bl on bl.id = ab.idSBP_BlankHistory and bl.idBudget = lva.idBudget " +
                "where lva.idValueType = {3} and isnull(lva.HasAdditionalNeed,0) = 0 " +
                "group by a.id, tc.idActivity, tc.idContingent, lva.idBudget, f.idSBP, " + fields3 +
                "   lva.idAuthorityOfExpenseObligation, lva.idOKATO, lva.idHierarchyPeriod " +
                "having " +
                "   sum(lva.Value) != 0 ) as x ",

                d.IdSBP, (byte)SBPType.TreasuryEstablishment,
                d.Id, (byte)ValueType.Justified,
                (byte)SBPType.Manager
            );
            var result = context.Database.SqlQuery<ActivityResourceMaintenance>(queryInsert).ToList();

            Debug.WriteLine("RoundDocs[{0}]: Получили из базы для документа «{1}» строки {2}", DateTime.Now, d.Header, result.Count());

            foreach (var g1 in result.GroupBy(g => g.IdMaster)) //GroupBy(g => g.key1))
            {
                //var first1 = g1.First();
                //var activity = d.Activity.SingleOrDefault(a =>
                //    a.IdActivity == first1.IdActivity && (a.IdContingent ?? 0) == (first1.IdContingent ?? 0)
                //    && a.IdSBP == first1.IdSBP
                //);
                //if (activity == null) continue;

                Debug.WriteLine("RoundDocs[{0}]: Обрабатываем для документа «{1}» строку idMaster={2}", DateTime.Now, d.Header, g1.Key);

                foreach (var g2 in g1.GroupBy(g => g.key2))
                {
                    var first = g2.First();

                    var newArm = context.ActivityOfSBP_ActivityResourceMaintenance.Create();
                    newArm.Owner = d;
                    newArm.IdMaster = g1.Key ?? 0; // newArm.Master = activity;
                    newArm.IdBudget = first.IdBudget;
                    newArm.IsDocument = true;
                    foreach (var p in typeof(ILineCost).GetProperties())
                    {
                        newArm.SetValue(p.Name, first.GetValue(p.Name));
                    }
                    newArm.IdAuthorityOfExpenseObligation = first.IdAuthorityOfExpenseObligation;
                    newArm.IdOKATO = first.IdOKATO;
                    context.ActivityOfSBP_ActivityResourceMaintenance.Add(newArm);

                    foreach (var s in g2)
                    {
                        var newPeriod = context.ActivityOfSBP_ActivityResourceMaintenance_Value.Create();
                        newPeriod.Owner = d;
                        newPeriod.Master = newArm;
                        newPeriod.IdHierarchyPeriod = s.IdHierarchyPeriod;
                        newPeriod.Value = s.Value;
                        context.ActivityOfSBP_ActivityResourceMaintenance_Value.Add(newPeriod);
                    }
                }
            }
        }

        /// <summary>
        /// Округление документа «Деятельность ведомства»
        /// </summary>
        public void RoundActivityOfSBP(int topDocs = -1, int idSbp = 0, int idSession = 1, bool isMandatoryOnly = false, int length = -2)
        {
            var entity = Objects.ByName<Entity>(typeof(ActivityOfSBP).Name);
            string docCap = entity.Caption;

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            Debug.WriteLine("RoundDocs[{0}]:  Округление документов «{1}»", DateTime.Now, docCap);

            // если у нас есть ИД сессии и мы не выбрали конкретный СБП, то получаем "маркер" для пометки обработанных документов
            // это нужно чтобы при запуске повторно не обработывать уже обработанные документы документы
            string session = (idSession != 0) && (idSbp == 0) ? string.Format("RoundDocs-{0}-", idSession) : null;

            // получаем список документов
            var docs = context.ActivityOfSBP.Where(w =>
                w.IdDocStatus != DocStatus.Draft
                && (idSbp == 0 || w.IdSBP == idSbp)
                && !context.ActivityOfSBP.Any(a => a.IdParent == w.Id)
                && (string.IsNullOrEmpty(session) || !w.Description.Contains(session))
                && context.ActivityOfSBP_ActivityResourceMaintenance_Value.Any(a => a.IdOwner == w.Id)
            ).OrderBy(o => o.Number).ToList();

            if (topDocs >= 1)
            {
                docs = docs.Take(topDocs).ToList();
            }

            Debug.WriteLine("RoundDocs[{0}]: Округляем {2} документов «{1}»", DateTime.Now, docCap, docs.Count());

            var fields1 = new[] { "Value", "AdditionalValue" };

            using (new ControlScope(new NoControlsStrategy(), false))
            {
                //var manager = DataManagerFactory.Create<ToolsDataManager>(entity);
                //var idOper = context.EntityOperation.Single(w => w.IdEntity == entity.Id && w.Operation.Name == "Process").Id;

                foreach (var d1 in docs)
                {
                    using (DataContext context1 = IoC.Resolve<DbContext>("NewContext").Cast<DataContext>())
                    {
                        var manager = DataManagerFactory.Create<ToolsDataManager>(context.Entity.Single(s => s.Id == entity.Id));
                        var idOper = context1.EntityOperation.Single(w => w.IdEntity == entity.Id && w.Operation.Name == "Process").Id;

                        var d = context1.ActivityOfSBP.Single(s => s.Id == d1.Id);

                        using (TransactionScope transaction = CreateTransaction())
                        {
                            Debug.WriteLine("RoundDocs[{0}]: Изменяем {1}", DateTime.Now, d.Header);

                            var saveStatus = d.IdDocStatus;
                            ActivityOfSBP nd;

                            try
                            {
                                nd = CloneActivityOfSBP(context1, d);

                                Debug.WriteLine("RoundDocs[{0}]: Создали документ «{1}»", DateTime.Now, nd.Header);

                                if (!string.IsNullOrEmpty(session)) nd.Description = session + "\r\n" + (nd.Description ?? "");

                                var q0 = nd.ActivityResourceMaintenance_Value.Where(a => 
                                    (a.Master.IdBudget.HasValue && (a.Master.IsDocument ?? false) == false)
                                    || !a.Master.IdBudget.HasValue
                                );
                                foreach (var t in q0) RoundFields(t, fields1);

                                Debug.WriteLine("RoundDocs[{0}]: Округлили неудаляемые строки документа «{1}»", DateTime.Now, nd.Header);

                                var q1 = nd.ActivityResourceMaintenance_Value.Where(a => a.Master.IdBudget.HasValue && a.Master.IsDocument == true);
                                context1.ActivityOfSBP_ActivityResourceMaintenance_Value.RemoveAll(q1);

                                var q2 = nd.ActivityResourceMaintenance.Where(a => a.IdBudget.HasValue && a.IsDocument == true);
                                context1.ActivityOfSBP_ActivityResourceMaintenance.RemoveAll(q2);

                                Debug.WriteLine("RoundDocs[{0}]: Удалили лишние строки документа «{1}»", DateTime.Now, nd.Header);

                                context1.SaveChanges(); // временно из-за проблем в процедуре FillActivityResourceMaintenance (бланки-то не сохрагнены)

                                Debug.WriteLine("RoundDocs[{0}]: Удалили лишние строки документа «{1}» и сохранили его", DateTime.Now, nd.Header);

                                FillActivityResourceMaintenance(context1, nd, isMandatoryOnly);

                                context1.SaveChanges();

                                Debug.WriteLine("RoundDocs[{0}]: Заполнили строки документа «{1}» и сохранили его", DateTime.Now, nd.Header);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("RoundDocs[{0}]: Произошла ошибка при клонировании документа {1} ошибка: {2}", DateTime.Now, d.Header, ex.Message);
                                throw;
                            }

                            Debug.WriteLine("RoundDocs[{0}]: Обрабатываем {1}", DateTime.Now, nd.Header);

                            try
                            {
                                //nd.ExecuteOperation(e => e.Process(context1));
                                //nd.IdDocStatus = saveStatus;
                                //context1.SaveChanges();
                                manager.ExecuteOperation(nd.Id, idOper);
                                nd.IdDocStatus = saveStatus;
                                context1.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("RoundDocs[{0}]: При обработке {1} ошибка : {2}", DateTime.Now, nd.Header, ex.Message);
                                throw;
                            }

                            transaction.Complete();
                        }

                        var adapter = (IObjectContextAdapter)context;
                        adapter.ObjectContext.Refresh(RefreshMode.StoreWins, d);
                        adapter.ObjectContext.Refresh(RefreshMode.StoreWins, nd);
                    }
                }
            }

            Debug.WriteLine("RoundDocs[{0}]: Закончено округление документов «{1}»", DateTime.Now, docCap);
        }

        #endregion
    }
 */
}
