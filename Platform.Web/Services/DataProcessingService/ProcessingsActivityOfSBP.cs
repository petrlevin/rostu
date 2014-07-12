using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using System.Data.Entity;
using Platform.BusinessLogic;
using Sbor.Document;
using Sbor.DbEnums;
using Sbor.Registry;
using Sbor.Logic;
using ValueType = Sbor.DbEnums.ValueType;
using BaseApp;

namespace Sbor.DataProcessors
{
    class ProcessingsActivityOfSBP
    {
        /// <summary>
        /// Заполнение регистра «Объемы финансовых средств» по данным документа Деятельность ведомства
        /// 
        /// 
        /// 
        /// </summary>
        public void CreationConductingsLimitVolumeAppropriations()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var docs =
                context.ActivityOfSBP.Where(
                    w => w.IdDocStatus == DocStatus.Project || w.IdDocStatus == DocStatus.Changed)
                    .Join(context.ActivityOfSBP_ActivityResourceMaintenance.Where(w => w.IdBudget.HasValue), a => a.Id, b => b.IdOwner, (a, b) => new { a })
                    .ToList();

            foreach (var st in docs)
            {

                ActivityOfSBP doc = st.a;

                // Проверяем не сделал ли документ проводок в регистре «Объемы финансовых средств»
                // если сделал то не обрабатываем документ
                if (context.LimitVolumeAppropriations.Where(w => w.IdRegistrator == doc.Id).Any()) continue;

                var iLimitVolumeAppropriations = new List<LimitVolumeAppropriations>();

                // формируем сторнируешие проводки
                if (doc.IdParent.HasValue)
                {
                    var prevVersions = doc.GetIdAllVersion(context).ToArray(); ;

                    var lva = context.LimitVolumeAppropriations.Where(l => prevVersions.Contains(l.IdRegistrator) &&
                                                                 l.IdRegistratorEntity == doc.EntityId &&
                                                                 l.IdValueType == (byte)ValueType.JustifiedGRBS &&
                                                                 l.EstimatedLine.IdSBP == doc.IdSBP)
                           .ToList();
                    // создаем сторнирующие записи регистра по данным в которые находятся в регистре

                    foreach (var rm in lva)
                    {

                        var newLimitVolumeAppropriations = new LimitVolumeAppropriations()
                        {
                            IdPublicLegalFormation = doc.IdPublicLegalFormation,
                            IdVersion = doc.IdVersion,
                            IdBudget = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id,
                            IdEstimatedLine = rm.IdEstimatedLine,
                            IdTaskCollection = rm.IdTaskCollection,
                            IdHierarchyPeriod = rm.IdHierarchyPeriod,
                            ValueType = DbEnums.ValueType.JustifiedGRBS,
                            Value = -rm.Value,
                            IdRegistrator = doc.Id,
                            DateCreate = DateTime.Now,
                            IdRegistratorEntity = doc.EntityId,
                            HasAdditionalNeed = false
                        };
                        iLimitVolumeAppropriations.Add(newLimitVolumeAppropriations);
                    }
                };
                // формируем обычные записи по значению поля Значение

                var tpResourceMaintenanceV = context.ActivityOfSBP_ActivityResourceMaintenance.Where(w => w.IdOwner == doc.Id && w.IdBudget.HasValue).Join(
                    context.ActivityOfSBP_ActivityResourceMaintenance_Value.Where(r => r.IdOwner == doc.Id && r.Value > 0),
                    a => a.Id, v => v.IdMaster,
                    (a, v) => new { a, v }
                ).ToList();

                //todo: wtf???
                var currentBudgetId = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Id;


                var findParamEstimatedLine = new FindParamEstimatedLine
                {
                    IdBudget = currentBudgetId,
                    IdPublicLegalFormation = doc.IdPublicLegalFormation,
                    IdSbp = doc.IdSBP,
                    IsCreate = true,
                    IsKosgu000 = false,
                    IsRequired = false,
                    TypeLine = ActivityBudgetaryType.Costs
                };

                var SBP = context.SBP.SingleOrDefault(s => s.Id == doc.IdSBP);

                var blank = SBP.IdSBPType == (byte)SBPType.GeneralManager ?
                                                                    context.SBP_Blank.FirstOrDefault(b => b.IdOwner == doc.IdSBP && b.IdBudget == currentBudgetId && b.IdBlankType == (byte)BlankType.FormationGRBS)
                                                                    : context.SBP_Blank.FirstOrDefault(b => b.IdOwner == SBP.IdParent && b.IdBudget == currentBudgetId && b.IdBlankType == (byte)BlankType.FormationGRBS);

                if (blank == null) return;

                var estimatedLines = context.ActivityOfSBP_ActivityResourceMaintenance.Where(w => w.IdOwner == doc.Id && w.IdBudget.HasValue).GetLinesId(context, doc.Id, doc.EntityId, blank, findParamEstimatedLine);

                // создаем записи регистра по данным в ТЧ, которые не находятся в регистре
                foreach (var rm in tpResourceMaintenanceV.Where(w => w.v.Value.HasValue))
                {

                    var estimatedLineId = estimatedLines[rm.a.Id];

                    var newLimitVolumeAppropriations = new LimitVolumeAppropriations()
                    {
                        IdPublicLegalFormation = doc.IdPublicLegalFormation,
                        IdVersion = doc.IdVersion,
                        IdBudget = currentBudgetId,
                        IdEstimatedLine = estimatedLineId,
                        IdTaskCollection = RegisterMethods.FindTaskCollection(context, doc.IdPublicLegalFormation, rm.a.Master.IdActivity, rm.a.Master.IdContingent).Id,
                        IdHierarchyPeriod = rm.v.IdHierarchyPeriod ?? 0,
                        ValueType = DbEnums.ValueType.JustifiedGRBS,
                        Value = rm.v.Value ?? 0,
                        IdRegistrator = doc.Id,
                        DateCreate = DateTime.Now,
                        IdRegistratorEntity = doc.EntityId,
                        HasAdditionalNeed = false
                    };
                    iLimitVolumeAppropriations.Add(newLimitVolumeAppropriations);
                }

                // формируем обычные записи по значению поля Доп. потребность
                // создаем записи регистра по данным в ТЧ, которые не находятся в регистре

                foreach (var rm in tpResourceMaintenanceV.Where(w => w.v.AdditionalValue.HasValue))
                {
                    var estimatedLineId = estimatedLines[rm.a.Id];

                    var newLimitVolumeAppropriations = new LimitVolumeAppropriations()
                    {
                        IdPublicLegalFormation = doc.IdPublicLegalFormation,
                        IdVersion = doc.IdVersion,
                        IdBudget = currentBudgetId,
                        IdEstimatedLine = estimatedLineId,
                        IdTaskCollection = RegisterMethods.FindTaskCollection(context, doc.IdPublicLegalFormation, rm.a.Master.IdActivity, rm.a.Master.IdContingent).Id,
                        IdHierarchyPeriod = rm.v.IdHierarchyPeriod ?? 0,
                        ValueType = DbEnums.ValueType.JustifiedGRBS,
                        Value = rm.v.AdditionalValue ?? 0,
                        IdRegistrator = doc.Id,
                        DateCreate = DateTime.Now,
                        IdRegistratorEntity = doc.EntityId,
                        HasAdditionalNeed = true
                    };
                    iLimitVolumeAppropriations.Add(newLimitVolumeAppropriations);
                }

                iLimitVolumeAppropriations.GroupBy(l => new
                {
                    DateCreate = l.DateCreate,
                    IdRegistratorEntity = l.IdRegistratorEntity,
                    IdPublicLegalFormation = l.IdPublicLegalFormation,
                    IdVersion = l.IdVersion,
                    IdBudget = l.IdBudget,
                    IdEstimatedLine = l.IdEstimatedLine,
                    IdTaskCollection = l.IdTaskCollection,
                    IdHierarchyPeriod = l.IdHierarchyPeriod,
                    ValueType = l.ValueType,
                    HasAdditionalNeed = l.HasAdditionalNeed
                }).Select(g => new
                {
                    IdPublicLegalFormation =
               g.Key.IdPublicLegalFormation,
                    IdVersion = g.Key.IdVersion,
                    IdBudget = g.Key.IdBudget,
                    IdEstimatedLine = g.Key.IdEstimatedLine,
                    IdTaskCollection = g.Key.IdTaskCollection,
                    IdHierarchyPeriod =
                g.Key.IdHierarchyPeriod,
                    ValueType = g.Key.ValueType,
                    HasAdditionalNeed =
                g.Key.HasAdditionalNeed,
                    IdRegistrator = doc.Id,
                    DateCreate = g.Key.DateCreate,
                    IdRegistratorEntity = g.Key.IdRegistratorEntity,
                    value = g.Sum(c => c.Value)
                });


                context.LimitVolumeAppropriations.InsertAsTableValue(iLimitVolumeAppropriations, context);
            }
        }
    }
}
