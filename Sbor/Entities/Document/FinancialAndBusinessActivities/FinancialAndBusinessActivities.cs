using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.DbEnums;
using Platform.Utils.Extensions;
using Sbor.CommonControls;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;
using EntityFramework.Extensions;
using BaseApp;


// ReSharper disable CheckNamespace
namespace Sbor.Document
// ReSharper restore CheckNamespace
{
    partial class FinancialAndBusinessActivities : IDocStatusTerminate, IColumnFactoryForDenormalizedTablepart, IDocOfSbpBudget
    {
        private SBP _sbpParent;
        private SBP_Blank _blankFormationAUBU;

        public SBP CurentSBP
        {
            get
            {
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
        /// Бланк формирования АУБУ вышестоящего СБП
        /// </summary>
        public SBP_Blank BlankFormationAUBU
        {
            get
            {
                if (_blankFormationAUBU == null)
                {
                    _blankFormationAUBU = SBPParent.SBP_Blank.FirstOrDefault(b => b.IdBlankType == (int)BlankType.FormationAUBU);
                    if (_blankFormationAUBU == null)
                        throw new PlatformException("У учреждения отсутствует бланк 'Формирование АУБУ'. Проблема с контролями в сущности SBP");
                }

                return _blankFormationAUBU;
            }

        }

        #region Операции

        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            DateLastEdit = DateTime.Now;
            if (IdSBP == null)
                Controls.Throw("Не выбрано учреждение");
            ExecuteControl(e => e.Control_0901(context));
            ExecuteControl(e => e.Control_0902(context));

            SetBlankActual(context);

            FilltpActivity(context, false);
            FilltpFBAFinancialIndicatorsInstitutions(context);

            var error = true;
            do
            {
                try
                {
                    var sc =
                    context.FinancialAndBusinessActivities.Where(
                        w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                           .Select(s => s.Number).Distinct().ToList();

                    Number = sc.GetNextCode();
                    
                    context.SaveChanges();
                    error = false;
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("Номер"))
                    {
                        throw;
                    }
                }
            } while (error);
            
        }

        /// <summary>   
        /// Операция «Редактировать»   
        /// </summary>  
        public void BeforeEdit(DataContext context)
        {
            ExecuteControl(e => e.Control_0927(context));

            if (SetBlankActual(context))
            {
                TrimKbkByNewActualBlank(context);
                TrimKbkByNewActualBlankKosv(context);
                context.SaveChanges();
            }

        }

        public void Edit(DataContext context)
        {
            ExecuteControl(e => e.Control_0918(context));
            ExecuteControl(e => e.Control_0919(context));
            RemoveExtraNeed(context);
            DateLastEdit = DateTime.Now;
        }

        /// <summary>   
        /// Операция «Обработать»   
        /// </summary>  
        public void Process(DataContext context)
        {
            ExecuteControl(e => e.Control_0908(context));
            ExecuteControl(e => e.Control_0909(context));
            ExecuteControl(e => e.Control_0913(context));
            ExecuteControl(e => e.Control_0917(context));
            ExecuteControl(e => e.Control_0910(context));
            ExecuteControl(e => e.Control_0911(context));
            ExecuteControl(e => e.Control_0912(context));
            ExecuteControl(e => e.Control_0926(context));
            ExecuteControl(e => e.Control_0914(context));
            ExecuteControl(e => e.Control_0930(context));

	        Dictionary<int, int> taskCollection = GetTaskCollection(context);

			UpdateRegTaskCollection(context, taskCollection);
			
			taskCollection = GetTaskCollection(context);
            WriteToRegistry(context, taskCollection);

            var prevDoc = GetPrevVersionDoc(context, this);
            if (prevDoc != null)
            {
                prevDoc.ExecuteOperation(e => e.Archive(context));
            }
        }

		private Dictionary<int, int> GetTaskCollection(DataContext context)
		{
			return context.Database.SqlQuery<_taskCollection>(
				string.Format(
					"select c.id as [Id], d.id as [IdTaskCollection] from tp.FBA_Activity c inner join doc.FinancialAndBusinessActivities e on e.id=c.idOwner inner join reg.TaskCollection d on d.idPublicLegalFormation=e.idPublicLegalFormation and d.idActivity=c.idActivity and (d.idContingent=c.idContingent or (d.idContingent is null and c.idContingent is null)) where c.idOwner={0}",
					Id)).ToDictionary(a => a.Id, a => a.IdTaskCollection);
		}

        /// <summary>
        /// Операция «Отменить обработку»
        /// </summary>  
        public void UndoProcess(DataContext context)
        {
            UndoProccesAndBackToDraft(context);
        }

        /// <summary>   
        /// Операция «Согласование»
        /// </summary>  
        public void Check(DataContext context)
        {
            ExecuteControl(e => e.Control_0928(context));
            ExecuteControl(e => e.Control_0929(context));

            IdDocStatus = DocStatus.Checking;
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Отменить согласование»
        /// </summary>  
        public void UndoCheck(DataContext context)
        {
            IdDocStatus = DocStatus.Project;
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Утвердить»   
        /// </summary>  
        public void Confirm(DataContext context)
        {
            DateCommit = DateTime.Now.Date;
            ExecuteControl(e => e.Control_0920(context));
            ExecuteControl(e => e.Control_0915(context));
            ExecuteControl(e => e.Control_0916(context));

            var ids = GetIdAllVersionDoc(context);

            if (!IsExtraNeed)
            {
                IsApproved = true;
                SetApprovedInReg(context, Id, Date, EntityId, ids, LimitVolumeAppropriations.EntityIdStatic);

                IdDocStatus = DocStatus.Approved;
            }
            else
            {
                IdDocStatus = DocStatus.Archive;

				Clone clone = new Clone(this);
				FinancialAndBusinessActivities newDoc = (FinancialAndBusinessActivities)clone.GetResult();
				//var newDoc = Clone(this, context);
                newDoc.IdDocStatus = DocStatus.Approved;
                newDoc.IdParent = Id;
                newDoc.IsRequireClarification = false;
                newDoc.DateCommit = null;
                newDoc.ReasonClarification = null;
                newDoc.ReasonTerminate = null;
                newDoc.ReasonCancel = null;
                newDoc.DateTerminate = null;
                newDoc.DateLastEdit = null;

                newDoc.Date = DateTime.Now.Date;

                newDoc.Number = this.GetDocNextNumber(context);

                newDoc.IsExtraNeed = false;
                newDoc.IsApproved = true;

                newDoc.RemoveExtraNeed(context);

                context.Entry(newDoc).State = EntityState.Added;
                context.SaveChanges();

                newDoc.Caption = newDoc.ToString();
                newDoc.EditCloneBug(context);

                RegisterMethods.SetRegsApproved(context, newDoc.Id, Date, EntityId, ids, new[] { LimitVolumeAppropriations.EntityIdStatic }, false);


                var toKill = context.LimitVolumeAppropriations.Where(l => l.IdRegistratorEntity == EntityId
                                                                          && l.HasAdditionalNeed.HasValue
                                                                          && l.HasAdditionalNeed.Value
                                                                          && !l.DateCommit.HasValue
                                                                          && ids.Contains(l.IdRegistrator));

                foreach (var reg in toKill)
                {
                    var sreg = reg.Clone();
                    sreg.Value = -sreg.Value;
                    sreg.IdRegistrator = newDoc.Id;
                    sreg.IdApproved = newDoc.Id;
                    sreg.IdApprovedEntity = EntityId;
                    sreg.DateCommit = newDoc.Date;

                    reg.IdApproved = newDoc.Id;
                    reg.IdApprovedEntity = EntityId;
                    reg.DateCommit = newDoc.Date;

                    context.LimitVolumeAppropriations.Add(sreg);
                }

            }
        }

        /// <summary>
        /// Операция «Утвердить с доп. потребностями»   
        /// </summary>
        /// <param name="context"></param>
        public void ConfirmWithAddNeed(DataContext context)
        {
            DateCommit = DateTime.Now.Date;
            ExecuteControl(e => e.Control_0921(context));
            ExecuteControl(e => e.Control_0922(context));
            ExecuteControl(e => e.Control_0915(context));
            ExecuteControl(e => e.Control_0916(context));
            ExecuteControl(e => e.Control_0911(context));

            IdDocStatus = DocStatus.Archive;
            var ids = GetIdAllVersionDoc(context);

            //var newDoc = Clone(this, context);
			Clone clone = new Clone(this);
			FinancialAndBusinessActivities newDoc = (FinancialAndBusinessActivities)clone.GetResult();

            newDoc.IsExtraNeed = false;
            newDoc.IdParent = Id;
            newDoc.IsApproved = true;

            newDoc.IdDocStatus = DocStatus.Approved;
            newDoc.Number = this.GetDocNextNumber(context);

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();
            newDoc.Caption = newDoc.ToString();
            newDoc.EditCloneBug(context);

            //суммировать поля «Доп. Потребность…» в ТЧ «Расходы», «Расходы учредителя по мероприятиям АУ/БУ» 
            //с полями «Сумма…» тех же строк за соответствующие периоды, затем произвести очистку значений в полях «Доп. потребность».
            newDoc.ReWriteAdditionalsExpenses(context);
            context.SaveChanges();

            newDoc.Process(context);

            //прописать в регистрах даты утверждения
            SetApprovedInReg(context, newDoc.Id, newDoc.Date, newDoc.EntityId, ids,
                             LimitVolumeAppropriations.EntityIdStatic);

            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Отменить утверждение»   
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            DateCommit = null;
            IsApproved = false;

            ClearApprovedInRegister(context, Id, EntityId, LimitVolumeAppropriations.EntityIdStatic);
        }

        /// <summary>   
        /// Операция «Вернуть на черновик»   
        /// </summary>  
        public void BackToDraft(DataContext context)
        {
            UndoProccesAndBackToDraft(context);
        }

        /// <summary>   
        /// Операция «Изменить»   
        /// </summary>  
        public void Change(DataContext context)
        {
			Clone clone = new Clone(this);
			FinancialAndBusinessActivities newDoc = (FinancialAndBusinessActivities)clone.GetResult();

			//var newDoc = Clone(this, context);
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = Id;
            newDoc.IsRequireClarification = false;
            newDoc.IsApproved = false;
            newDoc.DateCommit = null;
            newDoc.ReasonClarification = null;
            newDoc.ReasonTerminate = null;
            newDoc.ReasonCancel = null;
            newDoc.DateTerminate = null;

            newDoc.Number = this.GetDocNextNumber(context);

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();
            newDoc.Caption = newDoc.ToString();

            newDoc.FilltpActivity(context, false);
			context.SaveChanges();
        }

        /// <summary>
        /// При клонировании ТЧ у которой ссылки на две ТЧ документа появляются строки которые ссылаются на ТЧ из старых документов(их я тут и удаляю)
        /// </summary>
        /// <param name="context"></param>
        private void EditCloneBug(DataContext context)
        {
            //var toDeleteAD = ActivitiesDistributions.Where(w => w.FBA_Activity.IdOwner != Id).ToList();

            context.FBA_ActivitiesDistribution.RemoveAll(w => w.FBA_Activity.IdOwner != Id);

            /*var toDeleteCA =
                CostActivitiess.Where(
                    w => w.IdFBA_DistributionMethods != null && w.FBA_DistributionMethods.IdOwner != Id).ToList();*/

            context.FBA_CostActivities.RemoveAll(w => w.IdFBA_DistributionMethods != null && w.FBA_DistributionMethods.IdOwner != Id);

            //Не клонируются строки у которых ссылка на другую ТЧ пустая
            var toInsert = context.FBA_PlannedVolumeIncome.Where(w => w.IdOwner == IdParent && w.IdFBA_Activity == null).ToList();

            foreach (var fPvi in toInsert)
            {
                var exists = PlannedVolumeIncomes.Where(w => w.IdCodeSubsidy == fPvi.IdCodeSubsidy
                                                             && w.IdFinanceSource == fPvi.IdFinanceSource
                                                             && w.IdKFO == fPvi.IdKFO
                                                             && w.IdFBA_Activity == fPvi.IdFBA_Activity).ToList();

                if (!exists.Any())
                {
                    var newFpvi = context.FBA_PlannedVolumeIncome.Create();

                    newFpvi.IdFBA_Activity = null;
                    newFpvi.IdCodeSubsidy = fPvi.IdCodeSubsidy;
                    newFpvi.IdFinanceSource = fPvi.IdFinanceSource;
                    newFpvi.IdKFO = fPvi.IdKFO;
                    newFpvi.IdOwner = Id;
                    context.FBA_PlannedVolumeIncome.Add(newFpvi);

                    foreach (var fPviv in fPvi.FBA_PlannedVolumeIncome_value)
                    {
                        var newFpviv = context.FBA_PlannedVolumeIncome_value.Create();
                        newFpviv.Master = newFpvi;
                        newFpviv.IdHierarchyPeriod = fPviv.IdHierarchyPeriod;
                        newFpviv.Value = fPviv.Value;
                        newFpviv.IdOwner = Id;

                        context.FBA_PlannedVolumeIncome_value.Add(newFpviv);
                    }
                }
            }

            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Отменить изменение»   
        /// </summary>  
        public void UndoChange(DataContext context)
        {
            //var d = context.FinancialAndBusinessActivities.Where(s => s.IdParent == Id).ToList();

            context.FinancialAndBusinessActivities.RemoveAll(s => s.IdParent == Id);

            IdDocStatus = IsApproved ? DocStatus.Approved : ( string.IsNullOrEmpty(ReasonCancel) ? DocStatus.Project : DocStatus.Denied);
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Прекратить»
        /// </summary>  
        public void Terminate(DataContext context)
        {
            ExecuteControl<Control_7001>();
        }

        /// <summary>   
        /// Операция «Отменить прекращение»   
        /// </summary>  
        public void UndoTerminate(DataContext context)
        {
            ReasonTerminate = null;
            DateTerminate = null;
        }

        /// <summary>
        /// Операция «Отказать»
        /// </summary>  
        public void Deny(DataContext context)
        {
            if (string.IsNullOrEmpty(ReasonCancel))
            {
                Controls.Throw("Не заполнено поле 'Причина отказа'");
            }
        }

        /// <summary>   
        /// Операция «Отменить отказ»   
        /// </summary>  
        public void ReturnToProject(DataContext context)
        {
            ReasonCancel = null;
        }

        /// <summary>   
        /// Операция «В архив»   
        /// </summary>  
        public void Archive(DataContext context)
        {

        }

        /// <summary>   
        /// Операция «Вернуть на изменен»   
        /// </summary>  
        public void UndoArchive(DataContext context)
        {

        }

        /// <summary>   
        /// Операция «Согласование МРГ»   
        /// </summary>  
        public void CheckMRG(DataContext context)
        {
            this.IsRequireCheck = false;
        }

        #endregion

        #region Методы

        #region Методы операций

        public void UndoProccesAndBackToDraft(DataContext context)
        {
            ReasonClarification = null;
            IsRequireClarification = false;

            DocSGEMethod.RemoveRegRecords(context, LimitVolumeAppropriations.EntityIdStatic, Id, EntityId);
            using (new ControlScope())
            {
                context.SaveChanges();
            }

            var prevDoc = (FinancialAndBusinessActivities)CommonMethods.GetPrevVersionDoc(context, this, EntityId);
            if (prevDoc != null)
            {
                prevDoc.ExecuteOperation(e => e.UndoArchive(context));
            }
        }

        /// <summary>
        /// 5.	Заполнить ТЧ Мероприятия (вкладка «Основная информация»): 
        /// </summary>
        public void FilltpActivity(DataContext context, bool isRefreshGrid)
        {
            var addOrUpdateActivities = new List<int>();

            var budget = context.Budget.FirstOrDefault(f => f.Id == IdBudget);

            if (budget == null)
                throw new Exception(String.Format("Отсутствует Бюджет с id = {0}", IdBudget));
            var budgetYear = budget.Year;

            var taskVolumes = context.TaskVolume.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation
                                                            && w.IdBudget == IdBudget 
                                                            && w.IdVersion == IdVersion 
                                                            && w.IdSBP == IdSBP 
                                                            && w.IdValueType == (byte)ValueType.Plan 
                                                            && w.IdTerminator == null
                                                            && w.HierarchyPeriod.Year >= budgetYear
                                                            && w.HierarchyPeriod.Year <= budgetYear + 2)
                                     .Select(s => s.IdTaskCollection).Distinct().ToList();

            var taskCollections = context.TaskCollection.Where(w => taskVolumes.Contains(w.Id)).ToList();

            foreach (var taskCollection in taskCollections)
            {
                var existsActivity = Activity.FirstOrDefault(f => f.IdActivity == taskCollection.IdActivity
                                                                  && f.IdContingent == taskCollection.IdContingent);
                if (existsActivity != null)
                {
                    existsActivity.IsOwnActivity = false;
                    addOrUpdateActivities.Add(existsActivity.Id);
                    continue;
                }

                var activity = new FBA_Activity
                    {
                        IdActivity = taskCollection.IdActivity,
                        IdContingent = taskCollection.IdContingent,
                        IsOwnActivity = false,
                        IdOwner = Id
                    };
                
                context.FBA_Activity.Add(activity);
            }

            var toDeleteQuery =
                context.FBA_Activity.Where(w => w.IdOwner == Id && w.Id != 0 && !addOrUpdateActivities.Contains(w.Id)
                                                && !w.IsOwnActivity );

            var toDelete = toDeleteQuery.Select(a => new { id = a.Id, caption = "-" + a.Activity.Caption + (a.IdContingent.HasValue ? (" - " + a.Contingent.Caption) : "")})
                                        .Distinct().ToList();

            if (toDelete.Any())
            {
                if (isRefreshGrid)
                {
                    var activitiesId = toDelete.Select(d => d.id).ToArray();
                    if (context.FBA_ActivitiesDistribution.Any(a => activitiesId.Contains(a.IdFBA_Activity)))
                    {
                        var indirectActivities = context.FBA_ActivitiesDistribution
                                                        .Where(a => activitiesId.Contains(a.IdFBA_Activity))
                                                        .Select(a => a.IdFBA_Activity)
                                                        .ToArray();

                        var msg = new List<String>();

                        foreach (var m in toDelete)
                        {
                            if (indirectActivities.Contains(m.id))
                                msg.Add("<b>" + m.caption + "</b>");
                            else
                                msg.Add(m.caption);
                        }

                        ExecuteControl(e => e.Control_0925(msg, true));
                    }
                    else
                        ExecuteControl(e => e.Control_0925(toDelete.Select(d=>d.caption), false));
                }

                using (new ControlScope())
                {
                    context.FBA_Activity.RemoveAll(toDeleteQuery);
                    context.SaveChanges();
                }
                
            }
            
        }


        /// <summary>
        /// 6.	Заполнить ТЧ «Показатели финансового состояния учреждения» 
        /// </summary>
        /// <param name="context"></param>
        public void FilltpFBAFinancialIndicatorsInstitutions(DataContext context)
        {
            var financialIndicators =
                context.FinancialIndicator.Where(
                    w =>
                    w.IdPublicLegalFormation == IdPublicLegalFormation && w.IdRefStatus == (byte) RefStatus.Work)
                       .ToList();

            foreach (var financialIndicator in financialIndicators)
            {
                if(FinancialIndicatorsInstitutionss.Any(a=> a.IdFinancialIndicator == financialIndicator.Id))
                    continue;

                context.FBA_FinancialIndicatorsInstitutions.Add(new FBA_FinancialIndicatorsInstitutions
                    {
                        IdFinancialIndicator = financialIndicator.Id,
                        IdFinancialIndicatorCaption = financialIndicator.Caption,
                        IdOwner = Id
                    });
            }
        }

        /// <summary>
        /// 2. Проверить в регистре «Набор задач» наличие записей. Если записи отсутствуют, то создать их
        /// </summary>
        /// <param name="context"></param>
        public void UpdateRegTaskCollection(DataContext context, Dictionary<int, int> taskCollection)
        {
	        List<TaskCollection> forInsertTaskCollections = (from fbaActivity in Activity.Where(w => w.IsOwnActivity).ToList()
	                                                         where !taskCollection.ContainsKey(fbaActivity.Id)
	                                                         select new TaskCollection
		                                                         {
			                                                         IdActivity = fbaActivity.IdActivity, IdContingent = fbaActivity.IdContingent, IdPublicLegalFormation = IdPublicLegalFormation
		                                                         }).ToList();
	        context.TaskCollection.InsertAsTableValue(forInsertTaskCollections, context);
            //context.SaveChanges();
        }

        /// <summary>
        /// 3.	Сформировать проводки по регистру «Объемы финансовых средств».
        /// </summary>
        /// <param name="context"></param>
        private void WriteToRegistry(DataContext context, Dictionary<int, int> taskCollection)
        {
			WriteToLimitVolumeAppropriationsPlan(context, taskCollection);
            WriteToLimitVolumeAppropriationsJustified(context, taskCollection);
        }

        /// <summary>
        /// 3.1. Запись проводок по данным ТЧ «Объемы поступлений (скрытая)».
        /// </summary>
        /// <param name="context"></param>
		private void WriteToLimitVolumeAppropriationsPlan(DataContext context, Dictionary<int, int> taskCollection)
        {
			var reversedVolumes = context.LimitVolumeAppropriations.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation
																	&& w.IdBudget == IdBudget
																	&& w.IdVersion == IdVersion
																	&& w.IdValueType == (byte)ValueType.PlanFBA).Join
				(context.EstimatedLine.Where(w => w.IdSBP == IdSBP), l => l.IdEstimatedLine, r => r.Id,
				 (l, r) => new
				 {
					 l.IdTaskCollection,
					 l.IsIndirectCosts,
					 l.IdHierarchyPeriod,
					 l.IdValueType,
					 l.Value,
					 l.IsMeansAUBU,
					 l.IdEstimatedLine,
					 l.HasAdditionalNeed
				 }).ToList().Select(s => new LimitVolumeAppropriations
				 {
					 IdTaskCollection = s.IdTaskCollection,
					 IsIndirectCosts = s.IsIndirectCosts,
					 IdHierarchyPeriod = s.IdHierarchyPeriod,
					 IdValueType = s.IdValueType,
					 Value = -s.Value,
					 IsMeansAUBU = s.IsMeansAUBU,
					 IdEstimatedLine = s.IdEstimatedLine,
				 })
					   .ToList();
			/*
			var reversedVolumes =
                context.LimitVolumeAppropriations.Where(
                    w => w.IdValueType == (byte)ValueType.PlanFBA 
                        && w.EstimatedLine.IdSBP == IdSBP
                        && w.IdPublicLegalFormation == IdPublicLegalFormation
                        && w.IdBudget == IdBudget
                        && w.IdVersion == IdVersion).ToList().Select(s=>
                    new LimitVolumeAppropriations
                        {
                            IdTaskCollection = s.IdTaskCollection,
                            IsIndirectCosts = s.IsIndirectCosts,
                            IdHierarchyPeriod = s.IdHierarchyPeriod,
                            IdValueType = s.IdValueType,
                            Value = - s.Value,
                            IsMeansAUBU = s.IsMeansAUBU,
                            IdEstimatedLine = s.IdEstimatedLine,
                        }).ToList();
			*/
            
			var toWrite = PlannedVolumeIncome_values.ToList().Select(s => new LimitVolumeAppropriations
                {
                    IdTaskCollection = s.Master.IdFBA_Activity != null ? taskCollection[s.Master.IdFBA_Activity.Value] : (int?) null,
                    IsIndirectCosts = false,
                    IdHierarchyPeriod = s.IdHierarchyPeriod,
                    IdValueType = (byte)ValueType.PlanFBA,
                    Value = s.Value,
                    IsMeansAUBU = false,
                    IdEstimatedLine = GetIdEstLine(context, s.Master)
                }).ToList();

			/*
			var toWrite = PlannedVolumeIncome_values.ToList().Select(s => new LimitVolumeAppropriations
                {
                    IdTaskCollection = s.Master.IdFBA_Activity != null ? context.TaskCollection.Single(f=> f.IdActivity == s.Master.FBA_Activity.IdActivity && f.IdContingent == s.Master.FBA_Activity.IdContingent && f.IdPublicLegalFormation == IdPublicLegalFormation).Id : (int?) null,
                    IsIndirectCosts = false,
                    IdHierarchyPeriod = s.IdHierarchyPeriod,
                    IdValueType = (byte)ValueType.PlanFBA,
                    Value = s.Value,
                    IsMeansAUBU = false,
                    IdEstimatedLine = GetIdEstLine(context, s.Master)
				});
			*/
            var result = toWrite.Union(reversedVolumes).GroupBy(g => new
                {
                    g.IdValueType,
                    g.IdEstimatedLine,
                    g.IdHierarchyPeriod,
                    g.IdTaskCollection,
                    g.IsIndirectCosts,
                    g.IsMeansAUBU
                }).Select(s=> new LimitVolumeAppropriations
                    {
                        IdPublicLegalFormation = IdPublicLegalFormation,
                        IdBudget = IdBudget,
                        IdVersion = IdVersion,
                        IdTaskCollection = s.Key.IdTaskCollection,
                        IsIndirectCosts = s.Key.IsIndirectCosts,
                        IdHierarchyPeriod = s.Key.IdHierarchyPeriod,
                        IdValueType = s.Key.IdValueType,
                        IsMeansAUBU = s.Key.IsMeansAUBU,
                        IdRegistrator = Id,
                        IdRegistratorEntity = EntityId,
                        IdEstimatedLine = s.Key.IdEstimatedLine,
                        DateCreate = DateTime.Now,
                        Value = s.Sum(su => su.Value)
                    }).Where(w => w.Value != 0).ToList();

            context.LimitVolumeAppropriations.InsertAsTableValue(result,context);
        }

        public class _taskCollection
        {
			public int Id { get; set; }
			public int IdTaskCollection { get; set; }
        }
		/// <summary>
        /// 3.2. Запись проводок по данным ТЧ «Объемы расходов (скрытая)».
        /// </summary>
        /// <param name="context"></param>
		private void WriteToLimitVolumeAppropriationsJustified(DataContext context, Dictionary<int, int> taskCollection)
        {
            var findParamEstimatedLine = new FindParamEstimatedLine
            {
                IdBudget = IdBudget,
                IdPublicLegalFormation = IdPublicLegalFormation,
                IdSbp = (int)IdSBP,
                IsCreate = true,
                IsKosgu000 = false,
                IsRequired = false,
                TypeLine = ActivityBudgetaryType.Costs
            };
            var blank = context.SBP_Blank.FirstOrDefault(b => b.IdOwner == SBP.IdParent && b.IdBudget == IdBudget && b.IdBlankType == (byte)BlankType.FormationAUBU);

			var reversedVolumes =context.LimitVolumeAppropriations.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation
	                                                                && w.IdBudget == IdBudget
	                                                                && w.IdVersion == IdVersion
	                                                                && w.IdValueType == (byte) ValueType.JustifiedFBA).Join
		        (context.EstimatedLine.Where(w => w.IdSBP == IdSBP), l => l.IdEstimatedLine, r => r.Id,
		         (l, r) => new
			         {
				         l.IdTaskCollection,
				         l.IsIndirectCosts,
				         l.IdHierarchyPeriod,
				         l.IdValueType,
				         l.Value,
				         l.IsMeansAUBU,
				         l.IdEstimatedLine,
				         l.HasAdditionalNeed
					 }).ToList().Select(s => new LimitVolumeAppropriations
					 {
						 IdTaskCollection = s.IdTaskCollection,
						 IsIndirectCosts = s.IsIndirectCosts,
						 IdHierarchyPeriod = s.IdHierarchyPeriod,
						 IdValueType = s.IdValueType,
						 Value = -s.Value,
						 IsMeansAUBU = s.IsMeansAUBU,
						 IdEstimatedLine = s.IdEstimatedLine,
						 HasAdditionalNeed = s.HasAdditionalNeed
					 })
					   .ToList();
                /*
				context.LimitVolumeAppropriations.Where(
                    w => w.IdPublicLegalFormation == IdPublicLegalFormation
                        && w.IdBudget == IdBudget
                        && w.IdVersion == IdVersion
			&& w.IdValueType == (byte) ValueType.JustifiedFBA && w.EstimatedLine.IdSBP == IdSBP).ToList()
                       .Select(s => new LimitVolumeAppropriations
                           {
                               IdTaskCollection = s.IdTaskCollection,
                               IsIndirectCosts = s.IsIndirectCosts,
                               IdHierarchyPeriod = s.IdHierarchyPeriod,
                               IdValueType = s.IdValueType,
                               Value = -s.Value,
                               IsMeansAUBU = s.IsMeansAUBU,
                               IdEstimatedLine = s.IdEstimatedLine,
                               HasAdditionalNeed = s.HasAdditionalNeed
                           })
                       .ToList();
				*/
            var dic = CostActivitiess.ToList()
                                     .GroupBy(g => g as ILineCost)
                                     .ToDictionary(ca => ca.Key,
                                                   ca =>
                                                   ca.Key.GetLineId(context, Id, EntityId, blank, findParamEstimatedLine));


			
			var toWrite = CostActivities_values.Where(w=> w.Value != null).Select(s=> new LimitVolumeAppropriations
                {
                    IdEstimatedLine = (int)dic[s.Master],
                    IdTaskCollection = taskCollection[s.Master.IdMaster],
                    IsIndirectCosts = s.Master.IsIndirectCosts,
                    IdHierarchyPeriod = s.IdHierarchyPeriod,
                    IdValueType = (byte)ValueType.JustifiedFBA,
                    Value = s.Value ?? 0,
                    IsMeansAUBU = false,
                    HasAdditionalNeed = false
                }).ToList();

			var toWrite2 = CostActivities_values.Where(w => w.Value2 != null).Select(s => new LimitVolumeAppropriations
            {
                IdEstimatedLine = (int)dic[s.Master],
				IdTaskCollection = taskCollection[s.Master.IdMaster],
                IsIndirectCosts = s.Master.IsIndirectCosts,
                IdHierarchyPeriod = s.IdHierarchyPeriod,
                IdValueType = (byte)ValueType.JustifiedFBA,
                Value = s.Value2 ?? 0,
                IsMeansAUBU = false,
                HasAdditionalNeed = true
            }).ToList();

            var result = toWrite.Union(reversedVolumes).Union(toWrite2).GroupBy(g => new
            {
                g.IdValueType,
                g.IdEstimatedLine,
                g.IdHierarchyPeriod,
                g.IdTaskCollection,
                g.IsIndirectCosts,
                g.IsMeansAUBU,
                g.HasAdditionalNeed
            }).Select(s => new LimitVolumeAppropriations
            {
                IdPublicLegalFormation = IdPublicLegalFormation,
                IdBudget = IdBudget,
                IdVersion = IdVersion,
                IdTaskCollection = s.Key.IdTaskCollection,
                IsIndirectCosts = s.Key.IsIndirectCosts,
                IdHierarchyPeriod = s.Key.IdHierarchyPeriod,
                IdValueType = s.Key.IdValueType,
                IsMeansAUBU = s.Key.IsMeansAUBU,
                IdRegistrator = Id,
                IdRegistratorEntity = EntityId,
                IdEstimatedLine = s.Key.IdEstimatedLine,
                HasAdditionalNeed = s.Key.HasAdditionalNeed,
                DateCreate = DateTime.Now,
                Value = s.Sum(su => su.Value)
            }).Where(w => w.Value != 0).ToList();

            context.LimitVolumeAppropriations.InsertAsTableValue(result,context);
        }

        private int GetIdEstLine(DataContext context, FBA_PlannedVolumeIncome master)
        {
            var line = context.EstimatedLine.Where(w => w.IdKFO == master.IdKFO
                                                        && w.IdFinanceSource == master.IdFinanceSource
                                                        && w.IdCodeSubsidy == master.IdCodeSubsidy
                                                        && w.IdKOSGU == null
                                                        && w.IdKVR == null
                                                        && w.IdDEK == null
                                                        && w.IdDFK == null
                                                        && w.IdDKR == null
                                                        && w.IdExpenseObligationType == null
                                                        && w.IdKCSR == null
                                                        && w.IdKVSR == null
                                                        && w.IdRZPR == null
                                                        && w.IdBranchCode == null
                                                        && w.IdSBP == IdSBP
                                                        && w.IdPublicLegalFormation == IdPublicLegalFormation
                                                        && w.IdBudget == IdBudget
                                                        && w.IdActivityBudgetaryType == (byte)ActivityBudgetaryType.Costs).ToList();

            if(line.Count > 1)
                Controls.Throw("Ошибка формирования проводок по поступлениям! По данному набору КБК найдено более одной сметной строки");

            if (line.Any())
            {
                return line.First().Id;
            }
            else
            {
                var newEstLine = context.EstimatedLine.Create();

                newEstLine.IdKFO = master.IdKFO;
                newEstLine.IdFinanceSource = master.IdFinanceSource;
                newEstLine.IdCodeSubsidy = master.IdCodeSubsidy;
                newEstLine.IdActivityBudgetaryType = (byte)ActivityBudgetaryType.Costs;
                newEstLine.IdBudget = IdBudget;
                newEstLine.IdSBP = (int)IdSBP;
                newEstLine.IdPublicLegalFormation = IdPublicLegalFormation;
                newEstLine.Caption = Guid.NewGuid().ToString();

                context.EstimatedLine.Add(newEstLine);
                context.SaveChanges();

                newEstLine.Caption = newEstLine.ToString();
                context.SaveChanges();

                return newEstLine.Id;
            }
        }

        /// <summary>
        /// Добавляем доп. расходы к обычным расходам
        /// </summary>
        /// <param name="context"></param>
        public void ReWriteAdditionalsExpenses(DataContext context)
        {
            foreach (var expense in CostActivities_values.ToList())
            {
                expense.Value += expense.Value2;

                expense.Value2 = null;
            }
        }

        #endregion

        #region Общие методы

        private bool SetBlankActual(DataContext context)
        {
            var newBlanks =
                context.SBP_BlankHistory.Where(r =>
                                               r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.IdParent &&
                                               r.IdBlankType == (byte) DbEnums.BlankType.FormationAUBU)
                       .OrderByDescending(o => o.DateCreate);

            if (!newBlanks.Any())
            {
                return false;
            }

            SBP_BlankHistory oldBlankActual = this.SBP_BlankActual;

            this.SBP_BlankActual = newBlanks.FirstOrDefault();

            return this.SBP_BlankActual != null && (oldBlankActual == null || !SBP_BlankHelper.IsEqualBlank(oldBlankActual, this.SBP_BlankActual));
        }


        private void TrimKbkByNewActualBlank(DataContext context)
        {
            var gkbktrim = (from kbk in context.FBA_CostActivities.
                                                Where(r => r.IdOwner == this.Id).
                                                ToList().
                                                Select(s =>
                                                       new
                                                       {
                                                           s.IdOwner,
                                                           s.IdMaster,
                                                           s.IsIndirectCosts,
                                                           s.IdFBA_DistributionMethods,
                                                           IdBranchCode = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_BranchCode) ? null : s.IdBranchCode,
                                                           IdCodeSubsidy = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_CodeSubsidy) ? null : s.IdCodeSubsidy,
                                                           IdDEK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DEK) ? null : s.IdDEK,
                                                           IdDFK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DFK) ? null : s.IdDFK,
                                                           IdDKR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DKR) ? null : s.IdDKR,
                                                           IdExpenseObligationType = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_ExpenseObligationType) ? null : s.IdExpenseObligationType,
                                                           IdFinanceSource = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_FinanceSource) ? null : s.IdFinanceSource,
                                                           IdKCSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KCSR) ? null : s.IdKCSR,
                                                           IdKOSGU = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KOSGU) ? null : s.IdKOSGU,
                                                           IdKFO = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KFO) ? null : s.IdKFO,
                                                           IdKVR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVR) ? null : s.IdKVR,
                                                           IdKVSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVSR) ? null : s.IdKVSR,
                                                           IdRZPR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_RZPR) ? null : s.IdRZPR,
                                                           s.Id
                                                       })
                            join val in context.FBA_CostActivities_value.Where(r => r.IdOwner == this.Id).ToList() on
                                kbk.Id equals val.IdMaster
                            select new { kbk, val }).GroupBy(g =>
                                           new
                                           {
                                               g.kbk.IdOwner,
                                               g.kbk.IdMaster,
                                               g.kbk.IsIndirectCosts,
                                               g.kbk.IdFBA_DistributionMethods,
                                               g.kbk.IdBranchCode,
                                               g.kbk.IdCodeSubsidy,
                                               g.kbk.IdDEK,
                                               g.kbk.IdDFK,
                                               g.kbk.IdDKR,
                                               g.kbk.IdExpenseObligationType,
                                               g.kbk.IdFinanceSource,
                                               g.kbk.IdKCSR,
                                               g.kbk.IdKOSGU,
                                               g.kbk.IdKFO,
                                               g.kbk.IdKVR,
                                               g.kbk.IdKVSR,
                                               g.kbk.IdRZPR,
                                               g.val.IdHierarchyPeriod
                                           }).
                                   Select(s =>
                                          new
                                          {
                                              kbk = s.Key,
                                              Value = s.Sum(ss => ss.val.Value),
                                              AdditionalValue = s.Sum(ss => ss.val.Value2)
                                          }).
                                   ToList();

            context.FBA_CostActivities_value.RemoveAll(context.FBA_CostActivities_value.Where(r => r.IdOwner == this.Id));

            context.FBA_CostActivities.RemoveAll(context.FBA_CostActivities.Where(r => r.IdOwner == this.Id));

            var kbks = gkbktrim.Select(g =>
                                       new
                                       {
                                           g.kbk.IdOwner,
                                           g.kbk.IdMaster,
                                           g.kbk.IsIndirectCosts,
                                           g.kbk.IdFBA_DistributionMethods,
                                           g.kbk.IdBranchCode,
                                           g.kbk.IdCodeSubsidy,
                                           g.kbk.IdDEK,
                                           g.kbk.IdDFK,
                                           g.kbk.IdDKR,
                                           g.kbk.IdExpenseObligationType,
                                           g.kbk.IdFinanceSource,
                                           g.kbk.IdKCSR,
                                           g.kbk.IdKOSGU,
                                           g.kbk.IdKFO,
                                           g.kbk.IdKVR,
                                           g.kbk.IdKVSR,
                                           g.kbk.IdRZPR
                                       });

            foreach (var kbk in kbks.Distinct())
            {
                var newKbk = new FBA_CostActivities()
                {
                    IdOwner = kbk.IdOwner,
                    IdMaster = kbk.IdMaster,
                    IsIndirectCosts = kbk.IsIndirectCosts,
                    IdFBA_DistributionMethods = kbk.IdFBA_DistributionMethods,
                    IdBranchCode = kbk.IdBranchCode,
                    IdCodeSubsidy = kbk.IdCodeSubsidy,
                    IdDEK = kbk.IdDEK,
                    IdDFK = kbk.IdDFK,
                    IdDKR = kbk.IdDKR,
                    IdExpenseObligationType = kbk.IdExpenseObligationType,
                    IdFinanceSource = kbk.IdFinanceSource,
                    IdKCSR = kbk.IdKCSR,
                    IdKOSGU = kbk.IdKOSGU,
                    IdKFO = kbk.IdKFO,
                    IdKVR = kbk.IdKVR,
                    IdKVSR = kbk.IdKVSR,
                    IdRZPR = kbk.IdRZPR
                };

                context.FBA_CostActivities.Add(newKbk);

                var values = gkbktrim.Where(g =>
                                            g.kbk.IdOwner == kbk.IdOwner &&
                                            g.kbk.IdMaster == kbk.IdMaster &&
                                            g.kbk.IsIndirectCosts == kbk.IsIndirectCosts &&
                                            g.kbk.IdFBA_DistributionMethods == kbk.IdFBA_DistributionMethods &&
                                            g.kbk.IdBranchCode == kbk.IdBranchCode &&
                                            g.kbk.IdCodeSubsidy == kbk.IdCodeSubsidy &&
                                            g.kbk.IdDEK == kbk.IdDEK &&
                                            g.kbk.IdDFK == kbk.IdDFK &&
                                            g.kbk.IdDKR == kbk.IdDKR &&
                                            g.kbk.IdExpenseObligationType == kbk.IdExpenseObligationType &&
                                            g.kbk.IdFinanceSource == kbk.IdFinanceSource &&
                                            g.kbk.IdKCSR == kbk.IdKCSR &&
                                            g.kbk.IdKOSGU == kbk.IdKOSGU &&
                                            g.kbk.IdKFO == kbk.IdKFO &&
                                            g.kbk.IdKVR == kbk.IdKVR &&
                                            g.kbk.IdKVSR == kbk.IdKVSR &&
                                            g.kbk.IdRZPR == kbk.IdRZPR);

                foreach (var value in values)
                {
                    var newValue = new FBA_CostActivities_value()
                    {
                        IdOwner = value.kbk.IdOwner,
                        Master = newKbk,
                        IdHierarchyPeriod = value.kbk.IdHierarchyPeriod,
                        Value = value.Value,
                        Value2 = value.AdditionalValue
                    };
                    context.FBA_CostActivities_value.Add(newValue);
                }
            }

            context.SaveChanges();
        }

        private void TrimKbkByNewActualBlankKosv(DataContext context)
        {
            var gkbktrim = (from kbk in context.FBA_IndirectCosts.
                                                Where(r => r.IdOwner == this.Id).
                                                ToList().
                                                Select(s =>
                                                       new
                                                       {
                                                           s.IdOwner,
                                                           s.IdMaster,
                                                           IdBranchCode = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_BranchCode) ? null : s.IdBranchCode,
                                                           IdCodeSubsidy = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_CodeSubsidy) ? null : s.IdCodeSubsidy,
                                                           IdDEK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DEK) ? null : s.IdDEK,
                                                           IdDFK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DFK) ? null : s.IdDFK,
                                                           IdDKR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DKR) ? null : s.IdDKR,
                                                           IdExpenseObligationType = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_ExpenseObligationType) ? null : s.IdExpenseObligationType,
                                                           IdFinanceSource = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_FinanceSource) ? null : s.IdFinanceSource,
                                                           IdKCSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KCSR) ? null : s.IdKCSR,
                                                           IdKOSGU = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KOSGU) ? null : s.IdKOSGU,
                                                           IdKFO = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KFO) ? null : s.IdKFO,
                                                           IdKVR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVR) ? null : s.IdKVR,
                                                           IdKVSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVSR) ? null : s.IdKVSR,
                                                           IdRZPR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_RZPR) ? null : s.IdRZPR,
                                                           s.Id
                                                       })
                            join val in context.FBA_IndirectCosts_value.Where(r => r.IdOwner == this.Id).ToList() on
                                kbk.Id equals val.IdMaster
                            select new { kbk, val }).GroupBy(g =>
                                                           new
                                                           {
                                                               g.kbk.IdOwner,
                                                               g.kbk.IdMaster,
                                                               g.kbk.IdBranchCode,
                                                               g.kbk.IdCodeSubsidy,
                                                               g.kbk.IdDEK,
                                                               g.kbk.IdDFK,
                                                               g.kbk.IdDKR,
                                                               g.kbk.IdExpenseObligationType,
                                                               g.kbk.IdFinanceSource,
                                                               g.kbk.IdKCSR,
                                                               g.kbk.IdKOSGU,
                                                               g.kbk.IdKFO,
                                                               g.kbk.IdKVR,
                                                               g.kbk.IdKVSR,
                                                               g.kbk.IdRZPR,
                                                               g.val.IdHierarchyPeriod
                                                           }).
                                                   Select(s =>
                                                          new
                                                          {
                                                              kbk = s.Key,
                                                              Value = s.Sum(ss => ss.val.Value)
                                                          }).
                                                   ToList();

            context.FBA_IndirectCosts_value.RemoveAll(context.FBA_IndirectCosts_value.Where(r => r.IdOwner == this.Id));
            context.FBA_IndirectCosts.RemoveAll(context.FBA_IndirectCosts.Where(r => r.IdOwner == this.Id));

            var kbks = gkbktrim.Select(g =>
                                       new
                                       {
                                           g.kbk.IdOwner,
                                           g.kbk.IdMaster,
                                           g.kbk.IdBranchCode,
                                           g.kbk.IdCodeSubsidy,
                                           g.kbk.IdDEK,
                                           g.kbk.IdDFK,
                                           g.kbk.IdDKR,
                                           g.kbk.IdExpenseObligationType,
                                           g.kbk.IdFinanceSource,
                                           g.kbk.IdKCSR,
                                           g.kbk.IdKOSGU,
                                           g.kbk.IdKFO,
                                           g.kbk.IdKVR,
                                           g.kbk.IdKVSR,
                                           g.kbk.IdRZPR
                                       });

            foreach (var kbk in kbks.Distinct())
            {
                var newKbk = new FBA_IndirectCosts()
                {
                    IdOwner = kbk.IdOwner,
                    IdMaster = kbk.IdMaster,
                    IdBranchCode = kbk.IdBranchCode,
                    IdCodeSubsidy = kbk.IdCodeSubsidy,
                    IdDEK = kbk.IdDEK,
                    IdDFK = kbk.IdDFK,
                    IdDKR = kbk.IdDKR,
                    IdExpenseObligationType = kbk.IdExpenseObligationType,
                    IdFinanceSource = kbk.IdFinanceSource,
                    IdKCSR = kbk.IdKCSR,
                    IdKOSGU = kbk.IdKOSGU,
                    IdKFO = kbk.IdKFO,
                    IdKVR = kbk.IdKVR,
                    IdKVSR = kbk.IdKVSR,
                    IdRZPR = kbk.IdRZPR
                };

                context.FBA_IndirectCosts.Add(newKbk);

                var values = gkbktrim.Where(g =>
                                            g.kbk.IdOwner == kbk.IdOwner &&
                                            g.kbk.IdMaster == kbk.IdMaster &&
                                            g.kbk.IdBranchCode == kbk.IdBranchCode &&
                                            g.kbk.IdCodeSubsidy == kbk.IdCodeSubsidy &&
                                            g.kbk.IdDEK == kbk.IdDEK &&
                                            g.kbk.IdDFK == kbk.IdDFK &&
                                            g.kbk.IdDKR == kbk.IdDKR &&
                                            g.kbk.IdExpenseObligationType == kbk.IdExpenseObligationType &&
                                            g.kbk.IdFinanceSource == kbk.IdFinanceSource &&
                                            g.kbk.IdKCSR == kbk.IdKCSR &&
                                            g.kbk.IdKOSGU == kbk.IdKOSGU &&
                                            g.kbk.IdKFO == kbk.IdKFO &&
                                            g.kbk.IdKVR == kbk.IdKVR &&
                                            g.kbk.IdKVSR == kbk.IdKVSR &&
                                            g.kbk.IdRZPR == kbk.IdRZPR);

                foreach (var value in values)
                {
                    var newValue = new FBA_IndirectCosts_value()
                    {
                        IdOwner = value.kbk.IdOwner,
                        Master = newKbk,
                        IdHierarchyPeriod = value.kbk.IdHierarchyPeriod,
                        Value = value.Value
                    };
                    context.FBA_IndirectCosts_value.Add(newValue);
                }
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Удаление информации по доп. потребностям
        /// </summary>
        public void RemoveExtraNeed(DataContext context)
        {
            if (IsExtraNeed == false)
            {
                var fbaCostActivitiesValues =
                    context.FBA_CostActivities_value.Where(a => a.IdOwner == Id && a.Value2.HasValue);

                foreach (var fbaCostActivitiesValue in fbaCostActivitiesValues)
                {
                    fbaCostActivitiesValue.Value2 = null;
                }
            }
        }

        #endregion

        #region Распределение косвенных расходов
        private void RelocateIndirect(DataContext context, FBA_DistributionMethods fbaDistributionMethod)
        {
            if (fbaDistributionMethod.FBA_ActivitiesDistribution == null || !fbaDistributionMethod.FBA_ActivitiesDistribution.Any())
                Controls.Throw("В ТЧ 'Мероприятия для распределения' отсутствуют строки");
            if (fbaDistributionMethod.FBA_IndirectCosts == null || !fbaDistributionMethod.FBA_IndirectCosts.Any())
                Controls.Throw("В ТЧ 'Косвенные расходы' отсутствуют строки");


            switch (fbaDistributionMethod.IndirectCostsDistributionMethod)
            {
                case IndirectCostsDistributionMethod.M1:
                    RellocateIndirectM1(context, fbaDistributionMethod);
                    break;
                case IndirectCostsDistributionMethod.M2:
                    RellocateIndirectM2(context, fbaDistributionMethod);
                    break;
                case IndirectCostsDistributionMethod.M3:
                    RellocateIndirectM3(context, fbaDistributionMethod);
                    break;
                case IndirectCostsDistributionMethod.M4:
                    RellocateIndirectM4(context, fbaDistributionMethod);
                    break;
                case IndirectCostsDistributionMethod.M5:
                    RellocateIndirectM5(context, fbaDistributionMethod);
                    break;
                default:
                    throw new Exception("Для данного типа метода не реализованно");
            }
        }

        /// <summary>
        /// Равное распределение между мероприятиями
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fbaDistributionMethod"></param>
        private void RellocateIndirectM1(DataContext context, FBA_DistributionMethods fbaDistributionMethod)
        {
            var koef = 1/((decimal)fbaDistributionMethod.FBA_ActivitiesDistribution.Count);

            foreach (var actDistr in fbaDistributionMethod.FBA_ActivitiesDistribution.ToList())
            {
                foreach (var fbaIndirectCost in fbaDistributionMethod.FBA_IndirectCosts.ToList())
                {
                    CopyCosts(actDistr.IdFBA_Activity, fbaIndirectCost, context, koef);
                }
            }
        }

        /// <summary>
        /// Пропорционально прямым расходам по мероприятиям
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fbaDistributionMethod"></param>
        private void RellocateIndirectM2(DataContext context, FBA_DistributionMethods fbaDistributionMethod)
        {
            decimal allOfg = 0;
            decimal allPfg1 = 0;
            decimal allPfg2 = 0;

            foreach (var fAd in fbaDistributionMethod.FBA_ActivitiesDistribution.ToList())
            {
                fAd.OFG_Direct = 0;
                fAd.PFG1_Direct = 0;
                fAd.PFG2_Direct = 0;

                CalcDirect(context, fAd);

                allOfg += (decimal) fAd.OFG_Direct;
                allPfg1 += (decimal) fAd.PFG1_Direct;
                allPfg2 += (decimal) fAd.PFG2_Direct;
            }

            foreach (var actDistr in fbaDistributionMethod.FBA_ActivitiesDistribution.ToList())
            {
                var koefOgf = allOfg != 0 ? (actDistr.OFG_Direct ?? 0) / allOfg : 0;
                var koefPfg1 = allPfg1 != 0 ? (actDistr.PFG1_Direct ?? 0) / allPfg1 : 0;
                var koefPfg2 = allPfg2 != 0 ? (actDistr.PFG2_Direct ?? 0) / allPfg2 : 0;

                foreach (var fbaIndirectCost in fbaDistributionMethod.FBA_IndirectCosts.ToList())
                {
                    CopyCosts(actDistr.IdFBA_Activity, fbaIndirectCost, context, koefOgf, koefPfg1, koefPfg2);
                }
            }
        }

        /// <summary>
        /// Пропорционально объему предоставляемых мероприятий
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fbaDistributionMethod"></param>
        private void RellocateIndirectM3(DataContext context, FBA_DistributionMethods fbaDistributionMethod)
        {
            decimal allOfg = 0;
            decimal allPfg1 = 0;
            decimal allPfg2 = 0;

            foreach (var actDistr in fbaDistributionMethod.FBA_ActivitiesDistribution.ToList())
            {
                var taskVolume = context.TaskVolume.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation
                                                               && w.IdBudget == IdBudget
                                                               && w.IdVersion == IdVersion
                                                               && w.IdSBP == IdSBP
                                                               && w.IdValueType == (byte) ValueType.Plan
                                                               && w.IdTerminator == null
                                                               && w.TaskCollection.IdActivity == actDistr.FBA_Activity.IdActivity
                                                               && w.TaskCollection.IdContingent == actDistr.FBA_Activity.IdContingent
                                                               && w.HierarchyPeriod.Year >= Budget.Year
                                                               && w.HierarchyPeriod.Year <= Budget.Year + 2)
                                        .GroupBy(g => g.HierarchyPeriod.Year)
                                        .Select(s => new {year = s.Key, sum = s.Sum(su=> su.Value)})
                                        .ToList();

                actDistr.OFG_Activity = taskVolume.SingleOrDefault(w => w.year == Budget.Year) != null ?  taskVolume.SingleOrDefault(w => w.year == Budget.Year).sum : 0;
                actDistr.PFG1_Activity = taskVolume.SingleOrDefault(w => w.year == Budget.Year + 1) != null ? taskVolume.SingleOrDefault(w => w.year == Budget.Year).sum : 0;
                actDistr.PFG2_Activity = taskVolume.SingleOrDefault(w => w.year == Budget.Year + 2) != null ? taskVolume.SingleOrDefault(w => w.year == Budget.Year).sum : 0;

                allOfg += (decimal)actDistr.OFG_Activity;
                allPfg1 += (decimal)actDistr.PFG1_Activity;
                allPfg2 += (decimal)actDistr.PFG2_Activity;
            }

            foreach (var actDistr in fbaDistributionMethod.FBA_ActivitiesDistribution.ToList())
            {
                var koefOgf = allOfg != 0 ? (actDistr.OFG_Activity ?? 0) / allOfg : 0;
                var koefPfg1 = allPfg1 != 0 ? (actDistr.PFG1_Activity ?? 0) / allPfg1 : 0;
                var koefPfg2 = allPfg2 != 0 ? (actDistr.PFG2_Activity ?? 0) / allPfg2 : 0;

                foreach (var fbaIndirectCost in fbaDistributionMethod.FBA_IndirectCosts.ToList())
                {
                    CopyCosts(actDistr.IdFBA_Activity, fbaIndirectCost, context, koefOgf, koefPfg1, koefPfg2);
                }
            }
        }

        /// <summary>
        /// Задаваемый коэффициент распределения
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fbaDistributionMethod"></param>
        private void RellocateIndirectM4(DataContext context, FBA_DistributionMethods fbaDistributionMethod)
        {
            foreach (var actDistr in fbaDistributionMethod.FBA_ActivitiesDistribution.ToList())
            {
                var koefOgf = ((decimal?) actDistr.FactorOFG ?? 0)/100;
                var koefPfg1 = ((decimal?) actDistr.FactorPFG1 ?? 0)/100;
                var koefPfg2 = ((decimal?) actDistr.FactorPFG2 ?? 0)/100;

                foreach (var fbaIndirectCost in fbaDistributionMethod.FBA_IndirectCosts.ToList())
                    CopyCosts(actDistr.IdFBA_Activity, fbaIndirectCost, context, koefOgf, koefPfg1, koefPfg2);
            }
        }

        /// <summary>
        /// Пропорционально прямым расходам по указанным КОСГУ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fbaDistributionMethod"></param>
        private void RellocateIndirectM5(DataContext context, FBA_DistributionMethods fbaDistributionMethod)
        {
            if (!fbaDistributionMethod.FBA_DistributionAdditionalParameter.Any())
                Controls.Throw("В таблице \"Дополнительный параметр распределения\" отсутствуют строки.");

            decimal allOfg = 0;
            decimal allPfg1 = 0;
            decimal allPfg2 = 0;

            var kosguIds = fbaDistributionMethod.FBA_DistributionAdditionalParameter.Select(s => (int?)s.IdKOSGU).ToList();

            foreach (var fAd in fbaDistributionMethod.FBA_ActivitiesDistribution.ToList())
            {
                fAd.OFG_Direct = 0;
                fAd.PFG1_Direct = 0;
                fAd.PFG2_Direct = 0;

                CalcDirect(context, fAd, kosguIds);

                allOfg += (decimal) fAd.OFG_Direct;
                allPfg1 += (decimal) fAd.PFG1_Direct;
                allPfg2 += (decimal) fAd.PFG2_Direct;
            }

            foreach (var actDistr in fbaDistributionMethod.FBA_ActivitiesDistribution.ToList())
            {
                var koefOgf = allOfg != 0 ? (actDistr.OFG_Direct ?? 0) / allOfg : 0;
                var koefPfg1 = allPfg1 != 0 ? (actDistr.PFG1_Direct ?? 0) / allPfg1 : 0;
                var koefPfg2 = allPfg2 != 0 ? (actDistr.PFG2_Direct ?? 0) / allPfg2 : 0;

                foreach (var fbaIndirectCost in fbaDistributionMethod.FBA_IndirectCosts.ToList())
                    CopyCosts(actDistr.IdFBA_Activity, fbaIndirectCost, context, koefOgf, koefPfg1, koefPfg2);
            }

        }

        private void CalcDirect(DataContext context, FBA_ActivitiesDistribution fAd, List<int?> kosguIds = null)
        {
            bool skip = kosguIds == null;
            if(kosguIds == null)
                kosguIds = new List<int?>();

            var fcas = context.FBA_CostActivities.Where(
                w => w.IdMaster == fAd.IdFBA_Activity
                     && !w.IsIndirectCosts
                     && (skip || kosguIds.Contains(w.IdKOSGU)))
                              .ToList();

            foreach (var fCa in fcas)
            {
                if (fCa.FBA_CostActivities_value.Any(w => w.HierarchyPeriod.Year == Budget.Year))
                {
                    fAd.OFG_Direct +=
                        fCa.FBA_CostActivities_value.Where(w => w.HierarchyPeriod.Year == Budget.Year)
                           .Sum(c => c.Value);
                }
                else
                {
                    fAd.OFG_Direct += 0;
                }

                if (fCa.FBA_CostActivities_value.Any(w => w.HierarchyPeriod.Year == Budget.Year + 1))
                {
                    fAd.PFG1_Direct +=
                        fCa.FBA_CostActivities_value.Where(w => w.HierarchyPeriod.Year == Budget.Year + 1)
                           .Sum(c => c.Value);
                }
                else
                {
                    fAd.PFG1_Direct += 0;
                }

                if (fCa.FBA_CostActivities_value.Any(w => w.HierarchyPeriod.Year == Budget.Year + 2))
                {
                    fAd.PFG2_Direct +=
                        fCa.FBA_CostActivities_value.Where(w => w.HierarchyPeriod.Year == Budget.Year + 2)
                           .Sum(c => c.Value);
                }
                else
                {
                    fAd.PFG2_Direct += 0;
                }
            }
        }

        /// <summary>
        /// Копирование косвенных расходов из одной ДТЧ в другую ДТЧ с коэффициентом
        /// </summary>
        /// <param name="idActivity"></param>
        /// <param name="fIc"></param>
        /// <param name="context"></param>
        /// <param name="koefArray"></param>
        private void CopyCosts(int idActivity, FBA_IndirectCosts fIc, DataContext context, params decimal?[] koefArray)
        {
            decimal koefOgf;
            decimal? koefPfg1 = null;
            decimal? koefPfg2 = null;

            if (koefArray.Count() > 1)
            {
                var koef = koefArray[0];
                if (koef == null)
                    throw new Exception("Ошибка коэффициент не определен");
                koefOgf = (decimal) koef;
                koefPfg1 = koefArray[1];
                koefPfg2 = koefArray[2];
            }
            else
            {
                var koef = koefArray[0];
                if (koef == null)
                    throw new Exception("Ошибка коэффициент не определен");
                koefOgf = (decimal) koef;
            }

            var activitiesCost =
                context.FBA_CostActivities.SingleOrDefault(
                    w => w.IdMaster == idActivity
                         && w.IdOwner == Id
                         && w.IsIndirectCosts
                         && w.IdKFO == fIc.IdKFO
                         && w.IdKOSGU == fIc.IdKOSGU
                         && w.IdKVR == fIc.IdKVR
                         && w.IdKVSR == fIc.IdKVSR
                         && w.IdDEK == fIc.IdDEK
                         && w.IdDFK == fIc.IdDFK
                         && w.IdDKR == fIc.IdDKR
                         && w.IdBranchCode == fIc.IdBranchCode
                         && w.IdCodeSubsidy == fIc.IdCodeSubsidy
                         && w.IdExpenseObligationType == fIc.IdExpenseObligationType
                         && w.IdFinanceSource == fIc.IdFinanceSource
                         && w.IdKCSR == fIc.IdKCSR
                         && w.IdRZPR == fIc.IdRZPR);

            if (activitiesCost == null)
            {

                // Проверка КФО по собственной деятельности Control_0905
                var kfo = context.KFO.FirstOrDefault(f => f.Id == fIc.IdKFO);
                var fbaActivity = context.FBA_Activity.FirstOrDefault(f => f.Id == idActivity);

                if (fbaActivity.IsOwnActivity && kfo != null && kfo.IsIncludedInBudget)
                {
                    Controls.Throw(string.Format("Мероприятие {0} не предусмотрено в плане деятельности учреждения. Не допускается вносить расходы за счет средств, отнесенных к бюджетным, с КФО '{1}-{2}'.",fbaActivity.Activity.Caption, kfo.Code, kfo.Caption));
                }

                var newCostAct = context.FBA_CostActivities.Create();

                newCostAct.IdOwner = Id;
                newCostAct.IdMaster = idActivity;
                newCostAct.IsIndirectCosts = true;
                newCostAct.IdKFO = fIc.IdKFO;
                newCostAct.IdKOSGU = fIc.IdKOSGU;
                newCostAct.IdKVSR = fIc.IdKVSR;
                newCostAct.IdDEK = fIc.IdDEK;
                newCostAct.IdDFK = fIc.IdDFK;
                newCostAct.IdDKR = fIc.IdDKR;
                newCostAct.IdBranchCode = fIc.IdBranchCode;
                newCostAct.IdCodeSubsidy = fIc.IdCodeSubsidy;
                newCostAct.IdExpenseObligationType = fIc.IdExpenseObligationType;
                newCostAct.IdFinanceSource = fIc.IdFinanceSource;
                newCostAct.IdKCSR = fIc.IdKCSR;
                newCostAct.IdRZPR = fIc.IdRZPR;
                newCostAct.IdFBA_DistributionMethods = fIc.IdMaster;

                var hasValue = false;
                foreach (var fIcV in fIc.FBA_IndirectCosts_value.ToList())
                {
                    var koef = GetKoef(fIcV.HierarchyPeriod.Year, koefOgf, koefPfg1, koefPfg2);

                    if (fIcV.Value*koef == 0) continue;

                    hasValue = true;
                    var newCostActVal = context.FBA_CostActivities_value.Create();
                    newCostActVal.IdOwner = Id;
                    newCostActVal.Master = newCostAct;
                    newCostActVal.IdHierarchyPeriod = fIcV.IdHierarchyPeriod;
                    newCostActVal.Value = fIcV.Value*koef;
                    context.FBA_CostActivities_value.Add(newCostActVal);
                }

                if (hasValue)
                    context.FBA_CostActivities.Add(newCostAct);
            }
            else
            {
                context.FBA_CostActivities_value.RemoveAll(activitiesCost.FBA_CostActivities_value);

                activitiesCost.IdFBA_DistributionMethods = fIc.IdMaster;
                var hasValue = false;
                foreach (var fIcV in fIc.FBA_IndirectCosts_value.ToList())
                {
                    var koef = GetKoef(fIcV.HierarchyPeriod.Year, koefOgf, koefPfg1, koefPfg2);

                    if (fIcV.Value*koef == 0) continue;
                    hasValue = true;
                    var newCostActVal = context.FBA_CostActivities_value.Create();
                    newCostActVal.IdOwner = Id;
                    newCostActVal.IdMaster = activitiesCost.Id;
                    newCostActVal.IdHierarchyPeriod = fIcV.IdHierarchyPeriod;
                    newCostActVal.Value = fIcV.Value*koef;
                    context.FBA_CostActivities_value.Add(newCostActVal);
                }

                if (!hasValue)
                    context.FBA_CostActivities.Remove(activitiesCost);
            }
        }

        private decimal GetKoef(int year, decimal koefOgf, decimal? koefPfg1, decimal? koefPfg2)
        {
            if (year == Budget.Year)
                return koefOgf;
            if (year == Budget.Year + 1)
                return koefPfg1 ?? koefOgf;
            if (year == Budget.Year + 2)
                return koefPfg2 ?? koefOgf;
            throw new Exception("Ошибка получения коэффициента");
        }

        #endregion

        #endregion

        #region Методы сервисов

        /// <summary>
        /// Действие кнопки "Распределить косвенные"
        /// </summary>
        /// <param name="context"></param>
        /// <param name="rows">Ид строк ТЧ Методы распределения</param>
        public void UpdateIndirect(DataContext context, int[] rows)
        {
            ExecuteControl(e => e.Control_0907(context));
            var fbaDistMethods = context.FBA_DistributionMethods.Where(w => rows.Contains(w.Id)).ToList();

            foreach (var fbaDistributionMethod in fbaDistMethods)
            {
                DeleteActivitiesCosts(context, fbaDistributionMethod);
                RelocateIndirect(context, fbaDistributionMethod);
                context.SaveChanges();
                RestoreLosses(context, fbaDistributionMethod);
            }
        }

        //Восстанавливаем потери после дележки(например, при делении 10 на 3 получается 3.(33) в итоге копейка потеряется)
        private void RestoreLosses(DataContext context, FBA_DistributionMethods fbaDistributionMethod)
        {
            var indirectSums = fbaDistributionMethod.FBA_IndirectCosts.Join(context.FBA_IndirectCosts_value, indirectCosts => indirectCosts.Id, value => value.IdMaster,
                                                                                     (indirectCosts, value) => new
                                                                                         {
                                                                                             indirectCosts.IdBranchCode,
                                                                                             indirectCosts.IdCodeSubsidy,
                                                                                             indirectCosts.IdDEK,
                                                                                             indirectCosts.IdDFK,
                                                                                             indirectCosts.IdDKR,
                                                                                             indirectCosts.IdExpenseObligationType,
                                                                                             indirectCosts.IdFinanceSource,
                                                                                             indirectCosts.IdKCSR,
                                                                                             indirectCosts.IdKFO,
                                                                                             indirectCosts.IdKOSGU,
                                                                                             indirectCosts.IdKVR,
                                                                                             indirectCosts.IdKVSR,
                                                                                             indirectCosts.IdRZPR,
                                                                                             value.IdHierarchyPeriod,
                                                                                             value.Value
                                                                                         }).ToList();

            var costs2 =
                context.FBA_CostActivities_value.Where(w=> w.Master.IdFBA_DistributionMethods == fbaDistributionMethod.Id).Select(s => new
                    {
                        s.Master.IdBranchCode,
                        s.Master.IdCodeSubsidy,
                        s.Master.IdDEK,
                        s.Master.IdDFK,
                        s.Master.IdDKR,
                        s.Master.IdExpenseObligationType,
                        s.Master.IdFinanceSource,
                        s.Master.IdKCSR,
                        s.Master.IdKFO,
                        s.Master.IdKOSGU,
                        s.Master.IdKVR,
                        s.Master.IdKVSR,
                        s.Master.IdRZPR,
                        s.IdHierarchyPeriod,
                        Value = -s.Value ?? 0
                    }).GroupBy(g => new
                        {
                            g.IdBranchCode,
                            g.IdCodeSubsidy,
                            g.IdDEK,
                            g.IdDFK,
                            g.IdDKR,
                            g.IdExpenseObligationType,
                            g.IdFinanceSource,
                            g.IdKCSR,
                            g.IdKFO,
                            g.IdKOSGU,
                            g.IdKVR,
                            g.IdKVSR,
                            g.IdRZPR,
                            g.IdHierarchyPeriod
                        }).Select(s => new
                            {
                                s.Key.IdBranchCode,
                                s.Key.IdCodeSubsidy,
                                s.Key.IdDEK,
                                s.Key.IdDFK,
                                s.Key.IdDKR,
                                s.Key.IdExpenseObligationType,
                                s.Key.IdFinanceSource,
                                s.Key.IdKCSR,
                                s.Key.IdKFO,
                                s.Key.IdKOSGU,
                                s.Key.IdKVR,
                                s.Key.IdKVSR,
                                s.Key.IdRZPR,
                                s.Key.IdHierarchyPeriod,
                                Value = s.Sum(su => su.Value)
                            }).ToList();

            if(!costs2.Any()) return;

            var result = indirectSums.Union(costs2).GroupBy(g => new
                {
                    g.IdBranchCode,
                    g.IdCodeSubsidy,
                    g.IdDEK,
                    g.IdDFK,
                    g.IdDKR,
                    g.IdExpenseObligationType,
                    g.IdFinanceSource,
                    g.IdKCSR,
                    g.IdKFO,
                    g.IdKOSGU,
                    g.IdKVR,
                    g.IdKVSR,
                    g.IdRZPR,
                    g.IdHierarchyPeriod
                }).Select(s => new
                    {
                        s.Key.IdBranchCode,
                        s.Key.IdCodeSubsidy,
                        s.Key.IdDEK,
                        s.Key.IdDFK,
                        s.Key.IdDKR,
                        s.Key.IdExpenseObligationType,
                        s.Key.IdFinanceSource,
                        s.Key.IdKCSR,
                        s.Key.IdKFO,
                        s.Key.IdKOSGU,
                        s.Key.IdKVR,
                        s.Key.IdKVSR,
                        s.Key.IdRZPR,
                        s.Key.IdHierarchyPeriod,
                        Value = s.Sum(su => su.Value)
                    });

            foreach (var res in result.Where(w=> w.Value > 0))
            {
                var toUpdate = CostActivities_values.FirstOrDefault(w => w.IdHierarchyPeriod == res.IdHierarchyPeriod
                                                                && w.Master.IdBranchCode == res.IdBranchCode
                                                                && w.Master.IdCodeSubsidy == res.IdCodeSubsidy
                                                                && w.Master.IdDEK == res.IdDEK
                                                                && w.Master.IdDFK == res.IdDFK
                                                                && w.Master.IdExpenseObligationType == res.IdExpenseObligationType
                                                                && w.Master.IdFinanceSource == res.IdFinanceSource
                                                                && w.Master.IdKCSR == res.IdKCSR
                                                                && w.Master.IdKFO == res.IdKFO
                                                                && w.Master.IdKOSGU == res.IdKOSGU
                                                                && w.Master.IdKVR == res.IdKVR
                                                                && w.Master.IdKVSR == res.IdKVSR
                                                                && w.Master.IdRZPR == res.IdRZPR
                                                                && w.Master.IdFBA_DistributionMethods == fbaDistributionMethod.Id);
                if(toUpdate != null)
                    toUpdate.Value += res.Value;
            }
        }

        private void DeleteActivitiesCosts(DataContext context, FBA_DistributionMethods fbaDistributionMethod)
        {
            var toDelete = fbaDistributionMethod.FBA_CostActivities.ToList();

            context.FBA_CostActivities.RemoveAll(toDelete);

            context.SaveChanges();
        }

        #endregion
        
        #region Implementation of IColumnFactoryForDenormalizedTablepart

        public ColumnsInfo GetColumns(string tablepartEntityName)
        {
            if (tablepartEntityName == typeof(FBA_IndirectCosts).Name ||
                tablepartEntityName == typeof(FBA_PlannedVolumeIncome).Name)
            {
                return GetColumnsForSimpleIndicatorValue();
            }
            if (tablepartEntityName == typeof (FBA_CostActivities).Name)
            {
                return GetColumnsForSimpleIndicatorValue(true);
            }
            return null;
        }

        private ColumnsInfo GetColumnsForSimpleIndicatorValue(bool isAdditionalColumns = false)
        {
            var db = IoC.Resolve<DbContext>().Cast<DataContext>();

            var columns = new List<PeriodIdCaption>();
            var budget = db.Budget.Single(s => s.Id == IdBudget);

            var sbp = SBP;

            if (sbp != null && sbp.IdParent.HasValue)
            {
                var periods =
                    db.SBP_PlanningPeriodsInDocumentsAUBU.SingleOrDefault(w => w.IdOwner == sbp.IdParent && w.IdBudget == budget.Id);

                if (periods != null)
                {
                    var dic = new Dictionary<int, DocAUBUPeriodType>
                        {
                            {budget.Year, (DocAUBUPeriodType) periods.IdDocAUBUPeriodType_OFG},
                            {budget.Year + 1, (DocAUBUPeriodType) periods.IdDocAUBUPeriodType_PFG1},
                            {budget.Year + 2, (DocAUBUPeriodType) periods.IdDocAUBUPeriodType_PFG2}
                        };

                    foreach (var d in dic)
                    {
                        if(d.Value == DocAUBUPeriodType.Empty)
                            continue;

                        var period = db.HierarchyPeriod.Single(
                                p =>
                                !p.IdParent.HasValue && p.DateStart.Month == 1 && p.DateEnd.Month == 12 &&
                                p.DateStart.Year == d.Key);

                        if (d.Value == DocAUBUPeriodType.Year)
                        {
                            AddColumn(columns, period);
                        }
                        if (d.Value == DocAUBUPeriodType.Quarter)
                        {
                            var quaters = period.ChildrenByidParent;
                            foreach (var hierarchyPeriod in quaters)
                            {
                                AddColumn(columns, hierarchyPeriod);
                            }
                        }
                        if (d.Value == DocAUBUPeriodType.Month)
                        {
                            var quaters = period.ChildrenByidParent;
                            
                            foreach (var hierarchyPeriod in quaters)
                            {
                                var mounths = hierarchyPeriod.ChildrenByidParent;
                                foreach (var mounth in mounths)
                                {
                                    AddColumn(columns, mounth);
                                }
                            }
                        }
                    }
                }
            }

            if (isAdditionalColumns && IsExtraNeed)
                return new ColumnsInfo { Periods = columns, Resources = new List<string> { "Value", "Value2" } };
            if (isAdditionalColumns && !IsExtraNeed)
                return new ColumnsInfo { Periods = columns, Resources = new List<string> { "Value" } };

            return new ColumnsInfo { Periods = columns };
        }

        private void AddColumn(List<PeriodIdCaption> list, HierarchyPeriod hp)
        {
            list.Add(new PeriodIdCaption {PeriodId = hp.Id, Caption = hp.Caption});
        }

        #endregion

        #region Функции для работы с версиями

        /// <summary>   
        /// Кэш массива идентификаторов документов всех версий этого документа
        /// </summary>         
        private int[] _ids;

        /// <summary>   
        /// Получение массива идентификаторов документов всех версий этого документа, включая его самого
        /// </summary>         
        public int[] GetIdAllVersionDoc(DataContext context, bool isClearCache = false)
        {
            if (isClearCache || _ids == null)
            {
                var curdoc = this;
                var tmp = new List<int>();
                while (curdoc != null)
                {
                    tmp.Add(curdoc.Id);
                    curdoc = GetPrevVersionDoc(context, curdoc);
                }
                _ids = tmp.ToArray();
            }

            return _ids;
        }

        /// <summary>   
        /// Получение предыдущей версии документа
        /// </summary>         
        public FinancialAndBusinessActivities GetPrevVersionDoc(DataContext context, FinancialAndBusinessActivities curdoc)
        {
            return curdoc.IdParent.HasValue ? context.FinancialAndBusinessActivities.FirstOrDefault(w => w.Id == curdoc.IdParent) : null;
        }

        #endregion


        #region Клонирование

        public FinancialAndBusinessActivities Clone(FinancialAndBusinessActivities doc, DataContext context)
        {
            var newDoc = new FinancialAndBusinessActivities
                {
                    IdDocStatus = doc.IdDocStatus,
                    Date = doc.Date,
                    IdParent = doc.IdParent,
                    IsRequireClarification = doc.IsRequireClarification,
                    IsApproved = doc.IsApproved,
                    DateCommit = doc.DateCommit,
                    ReasonClarification = doc.ReasonClarification,
                    ReasonTerminate = doc.ReasonTerminate,
                    ReasonCancel = doc.ReasonCancel,
                    DateTerminate = doc.DateTerminate,
                    IdBudget = doc.IdBudget,
                    IdPublicLegalFormation = doc.IdPublicLegalFormation,
                    IdSBP = doc.IdSBP,
                    IdVersion = doc.IdVersion,
                    IsExtraNeed = doc.IsExtraNeed,
                    OtherInformation = doc.OtherInformation,
                    DateLastEdit = doc.DateLastEdit,
                    Description = doc.Description,
                    Number = doc.Number,
                    Caption = doc.Caption
                };

            context.FinancialAndBusinessActivities.Add(newDoc);

            var newFii = doc.FinancialIndicatorsInstitutionss.Select(fii => new FBA_FinancialIndicatorsInstitutions
                {
                    IdFinancialIndicator = fii.IdFinancialIndicator,
                    IdFinancialIndicatorCaption = fii.IdFinancialIndicatorCaption,
                    Value = fii.Value,
                    Owner = newDoc
                }).ToList();

            context.FBA_FinancialIndicatorsInstitutions.AddAll(newFii);

            var newFdag =
                doc.DepartmentActivityGoals.Select(
                    fdag => new FBA_DepartmentActivityGoal {DepartmentGoal = fdag.DepartmentGoal, Owner = newDoc});

            context.FBA_DepartmentActivityGoal.AddAll(newFdag);

            var newActivities = new Dictionary<int,FBA_Activity>();
            
            foreach (var act in doc.Activity.ToList())
            {
                var newAct = context.FBA_Activity.Create();
                newAct.IdActivity = act.IdActivity;
                newAct.IdContingent = act.IdContingent;
                newAct.Owner = newDoc;
                newAct.IsOwnActivity = act.IsOwnActivity;

                context.FBA_Activity.Add(newAct);
                newActivities.Add(act.Id, newAct);
            }

            foreach (var pvi in doc.PlannedVolumeIncomes.ToList())
            {
                var newPvi = new FBA_PlannedVolumeIncome
                    {
                        IdCodeSubsidy = pvi.IdCodeSubsidy,
                        IdFinanceSource = pvi.IdFinanceSource,
                        IdKFO = pvi.IdKFO,
                        Owner = newDoc,
                        FBA_Activity = pvi.IdFBA_Activity != null ? newActivities[(int) pvi.IdFBA_Activity] : null
                    };
                context.FBA_PlannedVolumeIncome.Add(newPvi);

                foreach (var pviv in pvi.FBA_PlannedVolumeIncome_value.ToList())
                {
                    var newPviv = new FBA_PlannedVolumeIncome_value
                    {
                        Owner = newDoc,
                        Master = newPvi,
                        IdHierarchyPeriod = pviv.IdHierarchyPeriod,
                        Value = pviv.Value
                    };
                    context.FBA_PlannedVolumeIncome_value.Add(newPviv);
                }
            }

            var newMethods = new Dictionary<int, FBA_DistributionMethods>();
            foreach (var dm in doc.DistributionMethodss.ToList())
            {
                var newDm = new FBA_DistributionMethods
                    {
                        IdIndirectCostsDistributionMethod = dm.IdIndirectCostsDistributionMethod,
                        Owner = newDoc,
                    };
                context.FBA_DistributionMethods.Add(newDm);
                newMethods.Add(dm.Id, newDm);

                var newDap =
                    dm.FBA_DistributionAdditionalParameter.Select(
                        fdag => new FBA_DistributionAdditionalParameter
                            {
                                IdKOSGU = fdag.IdKOSGU,
                                Master = newDm,
                                Owner = newDoc
                            });

                context.FBA_DistributionAdditionalParameter.AddAll(newDap);

                var newAd =
                    dm.FBA_ActivitiesDistribution.Select(
                        fdag => new FBA_ActivitiesDistribution
                            {
                                Master = newDm,
                                Owner = newDoc,
                                FBA_Activity = newActivities[fdag.IdFBA_Activity],
                                FactorOFG = fdag.FactorOFG,
                                FactorPFG1 = fdag.FactorPFG1,
                                FactorPFG2 = fdag.FactorPFG2,
                                OFG_Activity = fdag.OFG_Activity,
                                PFG1_Activity = fdag.PFG1_Activity,
                                PFG1_Direct = fdag.PFG1_Direct,
                                PFG2_Activity = fdag.PFG2_Activity,
                                PFG2_Direct = fdag.PFG2_Direct,
                                OFG_Direct = fdag.OFG_Direct
                            });

                context.FBA_ActivitiesDistribution.AddAll(newAd);

                foreach (var fbaIndirectCost in dm.FBA_IndirectCosts.ToList())
                {
                    var newIndiCost = new FBA_IndirectCosts
                        {
                            Owner = newDoc,
                            Master = newDm,
                            IdBranchCode = fbaIndirectCost.IdBranchCode,
                            IdCodeSubsidy = fbaIndirectCost.IdCodeSubsidy,
                            IdDEK = fbaIndirectCost.IdDEK,
                            IdDFK = fbaIndirectCost.IdDFK,
                            IdDKR = fbaIndirectCost.IdDKR,
                            IdExpenseObligationType = fbaIndirectCost.IdExpenseObligationType,
                            IdFinanceSource = fbaIndirectCost.IdFinanceSource,
                            IdKCSR = fbaIndirectCost.IdKCSR,
                            IdKFO = fbaIndirectCost.IdKFO,
                            IdKOSGU = fbaIndirectCost.IdKOSGU,
                            IdKVR = fbaIndirectCost.IdKVR,
                            IdKVSR = fbaIndirectCost.IdKVSR,
                            IdRZPR = fbaIndirectCost.IdRZPR
                        };
                    context.FBA_IndirectCosts.Add(newIndiCost);

                    foreach (var ficv in fbaIndirectCost.FBA_IndirectCosts_value.ToList())
                    {
                        var newFicv = new FBA_IndirectCosts_value
                            {
                                IdHierarchyPeriod = ficv.IdHierarchyPeriod,
                                Master = newIndiCost,
                                Owner = newDoc,
                                Value = ficv.Value
                            };
                        context.FBA_IndirectCosts_value.Add(newFicv);
                    }
                }
            }


            foreach (var ca in doc.CostActivitiess)
            {
                var newCa = new FBA_CostActivities
                {
                    Owner = newDoc,
                    Master = newActivities[ca.IdMaster],
                    FBA_DistributionMethods = ca.IdFBA_DistributionMethods != null ? newMethods[(int) ca.IdFBA_DistributionMethods] : null,
                    IdBranchCode = ca.IdBranchCode,
                    IdCodeSubsidy = ca.IdCodeSubsidy,
                    IdDEK = ca.IdDEK,
                    IdDFK = ca.IdDFK,
                    IdDKR = ca.IdDKR,
                    IdExpenseObligationType = ca.IdExpenseObligationType,
                    IdFinanceSource = ca.IdFinanceSource,
                    IdKCSR = ca.IdKCSR,
                    IdKFO = ca.IdKFO,
                    IdKOSGU = ca.IdKOSGU,
                    IdKVR = ca.IdKVR,
                    IdKVSR = ca.IdKVSR,
                    IdRZPR = ca.IdRZPR,
                    IsIndirectCosts = ca.IsIndirectCosts
                };
                context.FBA_CostActivities.Add(newCa);

                foreach (var cav in ca.FBA_CostActivities_value)
                {
                    var newCav = new FBA_CostActivities_value
                        {
                            IdHierarchyPeriod = cav.IdHierarchyPeriod,
                            Owner = newDoc,
                            Master = newCa,
                            Value = cav.Value,
                            Value2 = cav.Value2
                        };
                    context.FBA_CostActivities_value.Add(newCav);
                }

            }

            return newDoc;
        }

        #endregion



        /// <summary>
        /// Общий метод не подошел, надо его поправить, времени на это нет
        /// </summary>
        /// <param name="context"></param>
        /// <param name="idApprovedDoc"></param>
        /// <param name="dateCommit"></param>
        /// <param name="idRegistratorEntity"></param>
        /// <param name="ids"></param>
        /// <param name="registerEntityIdStatic"></param>
        public static void SetApprovedInReg(DataContext context, int idApprovedDoc, DateTime dateCommit, int idRegistratorEntity, int[] ids,
                                     int registerEntityIdStatic)
        {
           /* var table = context.Set<LimitVolumeAppropriations>(registerEntityIdStatic);
            var qq = table.Where(w =>
                                 !w.DateCommit.HasValue
                                 && w.IdRegistratorEntity == idRegistratorEntity 
                                 && (ids.Contains(w.IdRegistrator) || w.IdRegistrator == idApprovedDoc)
                );
            foreach (var rec in qq)
            {
                rec.DateCommit = dateCommit;
                rec.IdApprovedEntity = idRegistratorEntity;
                rec.IdApproved = idApprovedDoc;
            }*/

            context.LimitVolumeAppropriations.Update(
                w => !w.DateCommit.HasValue && w.IdRegistratorEntity == idRegistratorEntity
                     && (ids.Contains(w.IdRegistrator) || w.IdRegistrator == idApprovedDoc),
                u => new LimitVolumeAppropriations
                    {
                        DateCommit = dateCommit,
                        IdApprovedEntity = idRegistratorEntity,
                        IdApproved = idApprovedDoc
                    });

        }

        //Общий метод не подошел, надо его поправить, времени на это нет
        public static void ClearApprovedInRegister(DataContext context, int iddoc, int IdApprovedEntity, int registerEntityIdStatic)
        {
           /* var table = context.Set<LimitVolumeAppropriations>(registerEntityIdStatic);
            foreach (var rec in table.Where(w => w.IdApprovedEntity == IdApprovedEntity && w.IdApproved == iddoc))
            {
                rec.DateCommit = null;
                rec.IdApprovedEntity = null;
                rec.IdApproved = null;
            }*/

            context.LimitVolumeAppropriations.Update(
             w => w.IdApprovedEntity == IdApprovedEntity && w.IdApproved == iddoc,
             u => new LimitVolumeAppropriations
             {
                 DateCommit = null,
                 IdApprovedEntity = null,
                 IdApproved = null
             });

        }
    }
}
