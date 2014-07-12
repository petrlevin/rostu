using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Interfaces;
using BaseApp.Reference;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Operations;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Sbor.Document;
using Sbor.Interfaces;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Tablepart;

namespace Sbor.Logic
{
    /// <summary>
    /// Методы для работы с документы системы целепологания
    /// </summary>
    public static class DocSGEMethod
    {
        /// <summary>   
        /// Контроль "Проверка срока реализации документа"
        /// </summary> 
        public static void CommonControl_0101(IDocSGE doc)
        {
            const string msg = "Некорректный срок реализации.<br>«Срок реализации с» должен быть меньше «Срока реализации по».";

            if (doc.DateStart >= doc.DateEnd)
                Controls.Throw(msg);
        }

        /// <summary>   
        /// Контроль "Проверка даты документа"
        /// </summary> 
        public static void CommonControl_0102(IDocSGE doc)
        {
            if (doc.IdParent.HasValue)
            {
                if (doc.ParentDate.Date > doc.Date.Date)
                {
                    var sMsg = "Дата документа не может быть меньше даты предыдущей редакции.<br>" +
                               "Дата текущего документа: {0}<br>" +
                               "Дата предыдущей редакции: {1}";

                    Controls.Throw(string.Format(sMsg, doc.Date.ToShortDateString(), doc.ParentDate.ToShortDateString()));
                }
            }
        }

        /// <summary>
        /// процедура выравнивания-удаления лишних столбцов в табличных частях с значениями
        /// </summary>
        public static void AlignTableOnDates(DataContext context, int idOwner, int entityIdStatic, DateTime vDateStart, DateTime vDateEnd)
        {
            var table = context.Set<ITpWithHierarchyPeriod>(entityIdStatic);

            //todo: add sql method hasEntrance
            foreach (var rcvalue in table.Where(r => r.IdOwner == idOwner)
                                    //.Select(r=>new {r.Id, r.HierarchyPeriod} )
                                    .ToList())
            {
                if (!rcvalue.HierarchyPeriod.HasEntrance(vDateStart, vDateEnd))
                {
                    //var rcrem = table.FirstOrDefault(r => r.Id == rcvalue.Id);
                    table.Remove(rcvalue);
                }
            }
        }

        /// <summary>
        /// процедура удаления записей в регистре по регистратору
        /// </summary>
        public static void RemoveRegRecords(DataContext context, int idEntityRegister, int idRegistrator, int idRegistratorEntity)
        {
            var table = context.Set<IHasCommonRegistrator>(idEntityRegister);

            /*table.Where(r => r.IdRegistrator == idRegistrator && r.IdRegistratorEntity == idRegistratorEntity)
                .ToList()
                .ForEach(e => context.Entry(e).State = EntityState.Deleted);*/

            table.RemoveAll(idEntityRegister, r => r.IdRegistrator == idRegistrator && r.IdRegistratorEntity == idRegistratorEntity);
            
            using (new ControlScope())
            {
                context.SaveChanges();
            }
        }

        /// <summary>
        /// получить неаннулированные записи в регистре ЭлементыСЦ созданные предками данного документа и/или самим документом
        /// </summary>
        public static IQueryable<SystemGoalElement> GetRegDataOfParentDocs(DataContext context, int[] lIdsParent, int entityId, int idself)
        {
            if (idself != 0)
                lIdsParent = lIdsParent.Union(new[] {idself}).ToArray();

            return GetRegDataOfParentDocs(context, lIdsParent, entityId);
        }

        /// <summary>
        /// получить неаннулированные записи в регистре ЭлементыСЦ созданные предками данного документа
        /// </summary>
        public static IQueryable<SystemGoalElement> GetRegDataOfParentDocs(DataContext context, int[] lIdsParent, int entityId)
        {
            // получаем неаннулированные записи в регистре ЭлементыСЦ созданные предками данного документа
            var sgeOfParents =
                context.SystemGoalElement.Where(
                    r => lIdsParent.Contains(r.IdRegistrator) && !r.IdTerminator.HasValue && r.IdRegistratorEntity == entityId);
            return sgeOfParents;
        }


        /// <summary>
        /// проверка пересечения периодов двух документов
        /// </summary>
        public static bool HasIntersection(IHasPeriod doc1, IHasPeriod doc2)
        {
            return
                (doc1.DateStart <= doc2.DateEnd &&
                 doc1.DateStart >= doc2.DateStart) ||
                (doc1.DateEnd <= doc2.DateEnd &&
                 doc1.DateEnd >= doc2.DateStart) ||
                (doc2.DateStart <= doc1.DateEnd &&
                 doc2.DateStart >= doc1.DateStart) ||
                (doc2.DateEnd <= doc1.DateEnd &&
                 doc2.DateEnd >= doc1.DateStart);
        }

        /// <summary>
        /// проверка вхождения периодов 
        /// </summary>
        public static bool HasEntrance(IHasPeriod doc, ITpSystemGoalElement sge)
       { 
            return (sge.DateStart <= doc.DateEnd &&
                    sge.DateStart >= doc.DateStart) &&
                   (sge.DateEnd <= doc.DateEnd &&
                    sge.DateEnd >= doc.DateStart);
        }

        /// <summary>
        /// проверка вхождения периодов 
        /// </summary>
        public static bool HasEntrance(ITpSystemGoalElement sge1, ITpSystemGoalElement sge2)
        {
            return (sge1.DateStart <= sge2.DateEnd &&
                    sge1.DateStart >= sge2.DateStart) &&
                   (sge1.DateEnd <= sge2.DateEnd &&
                    sge1.DateEnd >= sge2.DateStart);
        }


        /// <summary>
        /// получает запрос на получение ID последних выполненных операции определенного типа
        /// </summary>
        public static IQueryable<int> GetLastExecutedOperation(DataContext context, int elementId, int entityId, string operationName)
        {
            var result = context.ExecutedOperation.Where( eo => eo.IdRegistrator == elementId 
                                                                && eo.IdRegistratorEntity == entityId 
                                                                && eo.EntityOperation.Operation.Name == operationName)
                                .OrderByDescending(eo => eo.Date)
                                .Select(eo=>eo.Id);

            return result;

            /*return from exo in context.ExecutedOperation.Where(r => r.IdRegistrator == elementId && r.IdRegistratorEntity == entityId)
                   join ento in context.EntityOperation.Where(eo => eo.IdEntity == entityId) on
                       exo.IdEntityOperation equals ento.Id
                   join op in context.Operation.Where(o => o.Name == operationName) on ento.IdOperation equals op.Id
                   orderby exo.Date descending
                   select exo.Id;*/
        }

        /// <summary>
        /// возвращает в цепочке документов последний документ в статусе отличном от Черновик
        /// </summary>
        public static IHierarhy GetLastDoc_NotInStatus(DataContext context, int idEntity, int docId, int status)
        {
            var docs = context.Set<IHierarhy>(idEntity);
            var doc = docs.FirstOrDefault(i => i.Id == docId);

            do
            {
                var child = docs.Where(c => c.IdParent == doc.Id);
                if (child.Any())
                {
                    var fchild = child.FirstOrDefault();
                    if ((int)fchild.GetValue("IdDocStatus") == (int)status)
                    {
                        break;
                    }
                    doc = fchild;
                }
                else
                {
                    break;
                }
            } while (true);
            return doc;
        }

        /// <summary>
        /// возвращает в цепочке документов последний документ 
        /// </summary>
        public static IHierarhy GetLeafDoc(DataContext context, int idEntity, int docId)
        {
            var docs = context.Set<IHierarhy>(idEntity);
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

        /// <summary>
        /// возвращает последние версии документов созданные мастер-документом
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityIdStatic">сущность искомых документов</param>
        /// <param name="allVersionDocIds">версии мастер-документа</param>
        /// <param name="thisdoc">мастер-документ</param>
        /// <returns></returns>
        public static IEnumerable<ISubDocSGE> GetSubDoc(DataContext context, int entityIdStatic, int[] allVersionDocIds, IDocSGE thisdoc)
        {
            return context.Set<ISubDocSGE>(entityIdStatic).Where(r => r.IdPublicLegalFormation == thisdoc.IdPublicLegalFormation && r.IdVersion == thisdoc.IdVersion).ToList()
                          .Where(r => allVersionDocIds.Contains(r.IdMasterDoc ?? 0))
                          .Select(s => GetLeafSubDoc(context, entityIdStatic, s.Id));
        }

        /// <summary>
        /// возвращает в цепочке документов последний документ 
        /// </summary>
        public static ISubDocSGE GetLeafSubDoc(DataContext context, int idEntity, int docId)
        {
            var docs = context.Set<ISubDocSGE>(idEntity);
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

        /// <summary>
        /// Устанавливаем признак Требует уточнения у списка документов
        /// </summary>
        public static void SetRequireClarification(DataContext context, IEnumerable<RegCommLink> listDoc, string thiscaption, string mess)
        {
            foreach (var d in listDoc)
            {
                var docs = context.Set<IClarificationDoc>(d.RegistratorEntity);
                SetRequireClarificInDoc(context, d, thiscaption, mess, docs);
            }
            context.SaveChanges();
        }

        /// <summary>
        /// Устанавливаем признак Требует уточнения у списка документов
        /// </summary>
        public static void SetRequireClarificationSimple(DataContext context, IEnumerable<RegCommLink> listDoc, string thiscaption, string mess)
        {
            var docs = context.Set<IClarificationDoc>(listDoc.First().RegistratorEntity);
            foreach (var d in listDoc)
            {
                SetRequireClarificInDoc(context, d, thiscaption, mess, docs);
            }
            context.SaveChanges();
        }

        /// <summary>
        /// Устанавливаем признак Требует уточнения у списка документов
        /// </summary>
        public static void SetRequireClarificationSimple(DataContext context, IEnumerable<RegCommLink> listDoc, string thiscaption, string mess, List<IClarificationDoc> docs)
        {
            foreach (var d in listDoc)
            {
                SetRequireClarificInDoc(context, d, thiscaption, mess, docs);
            }
            context.SaveChanges();
        }

        /// <summary>
        /// Устанавливаем признак Требует уточнения для одного документа
        /// </summary>
        public static void SetRequireClarificInDoc(DataContext context, RegCommLink d, string thiscaption, string mess, IEnumerable<IClarificationDoc> docs)
        {
            var doc = docs.FirstOrDefault(i => i.Id == d.IdRegistrator);

            if (doc != null)
            {
                do
                {
                    var next = docs.FirstOrDefault(i => i.IdParent == doc.Id);
                    if (next != null)
                    {
                        doc = next;
                        continue;
                    }
                    break;
                } while (true);

                doc.IsRequireClarification = true;

                string msg = mess
                    .Replace("{date}", DateTime.Now.ToString("dd.MM.yyyy"))
                    .Replace("{this}", thiscaption);

                doc.ReasonClarification += (string.IsNullOrEmpty(doc.ReasonClarification) ? "" : "\r\n") + msg;

                //context.Entry(doc).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Устанавливаем признак Требует уточнения для одного документа
        /// </summary>
        public static void SetRequireClarificInDoc(DataContext context, RegCommLink d, string thiscaption, string mess, List<IClarificationDoc> docs)
        {
            var doc = docs.FirstOrDefault(i => i.Id == d.IdRegistrator);

            if (doc != null)
            {
                do
                {
                    var next = docs.FirstOrDefault(i => i.IdParent == doc.Id);
                    if (next != null)
                    {
                        doc = next;
                        continue;
                    }
                    break;
                } while (true);

                string msg = mess
                    .Replace("{date}", DateTime.Now.ToString("dd.MM.yyyy"))
                    .Replace("{this}", thiscaption);

                doc.IsRequireClarification = msg != string.Empty;

                doc.ReasonClarification += (string.IsNullOrEmpty(doc.ReasonClarification) ? "" : "\r\n") + msg;

                //context.Entry(doc).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// очистка поля "Доп.потребности" / отказ от данного значения
        /// </summary>
        public static void DeclineAddValueInTp(DataContext context, int EntityIdStatic, int iddoc)
        {
            var table = context.Set<ITpAddValue>(EntityIdStatic);
            foreach (var tpvalue in table.Where(r => r.IdOwner == iddoc && r.AdditionalValue.HasValue).ToList())
            {
                var rec = table.First(r => r.Id == tpvalue.Id);
                rec.AdditionalValue = null;
            }
        }

        /// <summary>
        /// суммирование Значения и "Доп.потребности" / принятие данного значения
        /// </summary>
        public static void AcceptAddValueInTp(DataContext context, int EntityIdStatic, int iddoc)
        {
            var table = context.Set<ITpAddValue>(EntityIdStatic);
            foreach (var tpvalue in table.Where(r => r.IdOwner == iddoc && r.AdditionalValue.HasValue).ToList())
            {
                var rec = table.First(r => r.Id == tpvalue.Id);
                var value = (tpvalue.Value ?? 0) + (tpvalue.AdditionalValue ?? 0);
                rec.Value = value;
                rec.AdditionalValue = null;
            }
        }

        private static string emptykey = "000000000000000";

        public static string GetSKeyReportLine(string prefix = "", string s = "")
        {
            var p = prefix + s;
            var key = p + emptykey.Substring(0, emptykey.Length - s.Length);
            return key;
        }
        /// <summary>
        /// преобразование decimal в строку без лишних нулей справа
        /// </summary>
        public static string ExcludeLastZeros(decimal? sum)
        {
            if (sum == null)
            {
                return "";
            }

            var res0 = sum.Value.ToString("#,#0.00#");
            if (!res0.Contains(","))
            {
                return res0;
            }

            for (; ; )
            {
                var inc = res0.Length - 1;
                if (inc < 2)
                {
                    break;
                }
                if (res0.Substring(inc, 1) == "0")
                {
                    res0 = res0.Substring(0, inc);
                }
                else
                {
                    break;
                }
            }
            if (res0.Substring(res0.Length - 1, 1) == ",")
            {
                res0 = res0.Substring(0, res0.Length - 1);
            }
            return res0;
        }

        /// <summary>
        /// Получить модель Системы Целепологания
        /// </summary>
        /// <param name="context"></param>
        /// <param name="idPublicLegalFormation"></param>
        /// <returns></returns>
        public static IQueryable<SGModel> GetModelSG(this DataContext context, int? idPublicLegalFormation)
        {
            return context.ModelSystemGoal
                       .Where(
                           r =>
                           r.IdRefStatus == (byte)RefStatus.Work &&
                           r.IdPublicLegalFormation == idPublicLegalFormation)
                       .Select(s =>
                               new SGModel()
                               {
                                   ElementType = s.ElementTypeSystemGoal,
                                   ElementParentType =
                                       (s.IdParent.HasValue ? s.Parent.ElementTypeSystemGoal : null)
                               });
        }

        #region Функции для создания подчиненного документа

        /// <summary>
        /// находим подчиненный документ, самый последний в ветке, с указанной основной целью
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="idDocEntity">ид сущности документа</param>
        /// <param name="idTpEntity">ид сущности тч целе</param>
        /// <param name="idCurDoc"></param>
        /// <param name="isFilter">фильтр для нужного нам "подчинения"</param>
        /// <param name="idCurDocTpEntity"></param>
        public static ISubDocSGE GetLeaf_SubDocSGE(DataContext context, int idDocEntity, int idTpEntity, int idCurDocTpEntity, int idCurDoc, Func<ISubDocSGE, bool> isFilter)
        {
            //Документы у которых 
            //в ТЧ «Элементы СЦ» имеется цель, указанная в поле ТекушийДокумент.ТЧ «Перечень подпрограмм».поле «Основная цель», 
            //и у этой цели установлен флажок «Основная цель».
            var mainGoalElementId =
                context.Set<ITpSystemGoalElement>(idCurDocTpEntity)
                       .Where(t => t.IsMainGoal && t.IdOwner == idCurDoc)
                       .Select(s => s.IdSystemGoal).SingleOrDefault();

            return GetLeaf_SubDocSGE(context, idDocEntity, idTpEntity, mainGoalElementId, isFilter);
        }

        /// <summary>
        /// находим подчиненный документ, самый последний в ветке, с указанной основной целью
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="idDocEntity">ид сущности документа</param>
        /// <param name="idTpEntity">ид сущности тч целе</param>
        /// <param name="mainGoalElementId"></param>
        /// <param name="isFilter">фильтр для нужного нам "подчинения"</param>
        public static ISubDocSGE GetLeaf_SubDocSGE(DataContext context, int idDocEntity, int idTpEntity, int mainGoalElementId, Func<ISubDocSGE, bool> isFilter)
        {
            var tps =
                context.Set<ITpSystemGoalElement>(idTpEntity)
                       .Where(t => t.IsMainGoal && t.IdSystemGoal == mainGoalElementId)
                       .Select(s => s.IdOwner)
                       .ToArray();

            var docs =
                context.Set<ISubDocSGE>(idDocEntity)
                       .Where(w => tps.Contains(w.Id))
                       .ToList();//.Where(isFilter);

            var subDocSges = docs.Where(w => docs.All(a => a.IdParent != w.Id));
            ISubDocSGE doc = subDocSges.FirstOrDefault();

            return doc;
        }

        /// <summary>
        /// создаем или обновляем подчиненный документ
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="idDocEntity">ид сущности документа</param>
        /// <param name="idTpEntity">ид сущности тч целей</param>
        /// <param name="srcDoc">исходный документ</param>
        /// <param name="sp">строка ТЧ Список подпрограмм</param>
        /// <param name="idTpSubProgEntity">ид сущности тч подпрограмм</param>
        /// <param name="idTpSrcEntity1"></param>
        /// <param name="idTpSrcEntity2"></param>
        /// <param name="idTpEntity1"></param>
        /// <param name="idTpEntity2"></param>
        private static ISubDocSGE CreateOrUpdate_SubDocSGE(DataContext context, int idDocEntity, int idTpEntity, IDocSGE srcDoc, ITpListSubProgram sp, int idTpSubProgEntity,
            int idTpSrcEntity1, int idTpSrcEntity2, int idTpEntity1, int idTpEntity2)
        {
            var ids = srcDoc.AllVersionDocIds;

            ISubDocSGE doc;
            if (idTpSubProgEntity == StateProgram_DepartmentGoalProgramAndKeyActivity.EntityIdStatic)
            {
                doc = GetLeaf_SubDocSGE(context, idDocEntity, idTpEntity, sp.IdSystemGoal, x =>
                                                                                                      ids.Contains(
                                                                                                          x.IdMasterDoc ??
                                                                                                          0)
                    // нужного типа, созданный одним из цепочки документоа
                    );
            }
            else
            {
                doc = GetLeaf_SubDocSGE(context, idDocEntity, idTpEntity, sp.IdSystemGoal, x =>
                                                                                                      x.IdDocType ==
                                                                                                      sp.IdDocType &&
                                                                                                      ids.Contains(
                                                                                                          x.IdMasterDoc ??
                                                                                                          0)
                    // нужного типа, созданный одним из цепочки документоа
                    );
            }

            //ISubDocSGE doc = GetLeaf_SubDocSGE(context, idDocEntity, idTpEntity, sp.IdSystemGoal, x =>
            //    x.IdDocType == sp.IdDocType && ids.Contains(x.IdMasterDoc ?? 0) // нужного типа, созданный одним из цепочки документоа
            //);

            int? idAnalyticalCode = sp.IdAnalyticalCodeStateProgramValue == 0 ? (int?)null : sp.IdAnalyticalCodeStateProgramValue;

            var dataSetDoc = context.Set<ISubDocSGE>(idDocEntity);
            if (doc == null)
            {
                //Создаем новый документ
                doc = dataSetDoc.Create();
                
                //Рассчитываем номер
                var sc =
                    dataSetDoc.Where(
                        w => w.IdPublicLegalFormation == srcDoc.IdPublicLegalFormation && !w.IdParent.HasValue)
                              .Select(s => s.Number)
                              .Distinct()
                              .ToList();
                doc.Number = sc.GetNextCode();

                doc.Date = srcDoc.Date;
                doc.IdDocStatus = DocStatus.Draft;
                dataSetDoc.Add(doc);
            }
            else
            {
                if (doc.IdDocStatus == DocStatus.Terminated)
                {
                    return null;
                }

                // проверяем - действительно ли есть изменения для ЭД
                var NotChanged =
                    doc.IdVersion == srcDoc.IdVersion &&
                    doc.Caption == sp.Caption &&
                    doc.IdSBP == sp.IdSBP &&
                    doc.IdResponsibleExecutantType == sp.IdResponsibleExecutantType &&
                    (doc.IdAnalyticalCodeStateProgramValue == idAnalyticalCode || doc.IdAnalyticalCodeStateProgramValue == (idAnalyticalCode ?? 0))  &&
                    doc.IdDocType == sp.IdDocType &&
                    doc.DateStart == sp.DateStart.Value &&
                    doc.DateEnd == sp.DateEnd.Value &&
                    doc.HasAdditionalNeed == srcDoc.HasAdditionalNeed;

                if (NotChanged)
                {
                    var dataSetTpSrc =
                        context.Set<ITpResourceMaintenance>(idTpSrcEntity1).Where(r => r.IdOwner == srcDoc.Id && r.IdMaster == sp.Id)
                                     .Join(context.Set<ITpAddValue>(idTpSrcEntity2).Where(r => r.IdOwner == srcDoc.Id), s => s.Id, v => v.IdMaster,
                                           (s, v) => new {s.IdFinanceSource, v.IdHierarchyPeriod, v.Value}).ToList();

                    var dataSetTp =
                        context.Set<ITpResourceMaintenance>(idTpEntity1).Where(r => r.IdOwner == doc.Id)
                                     .Join(context.Set<ITpAddValue>(idTpEntity2).Where(r => r.IdOwner == doc.Id), s => s.Id, v => v.IdMaster,
                                           (s, v) => new {s.IdFinanceSource, v.IdHierarchyPeriod, v.Value}).ToList();

                    NotChanged = true;
                    foreach (var dstp in dataSetTp)
                    {
                        if (!dataSetTpSrc.Any(dstpsrc =>
                                                 Equals(dstpsrc.IdFinanceSource, dstp.IdFinanceSource) &&
                                                 Equals(dstpsrc.IdHierarchyPeriod, dstp.IdHierarchyPeriod) &&
                                                 dstpsrc.Value == dstp.Value))
                        {
                            NotChanged = false;
                            break;
                        }
                    }
                    if (NotChanged)
                    {
                        foreach (var dstpsrc in dataSetTpSrc)
                        {
                            if (!dataSetTp.Any(dstp =>
                                                     Equals(dstpsrc.IdFinanceSource, dstp.IdFinanceSource) &&
                                                     Equals(dstpsrc.IdHierarchyPeriod, dstp.IdHierarchyPeriod) &&
                                                     dstpsrc.Value == dstp.Value))
                            {
                                NotChanged = false;
                                break;
                            }
                        }
                    }

                    if (NotChanged)
                    {
                        sp.IdDocument = doc.Id;
                        sp.IdDocumentEntity = idDocEntity;

                        //doc.IdMasterDoc = srcDoc.Id;

                        context.SaveChanges();

                        return null;
                    }
                }

                if (doc.IdDocStatus == DocStatus.Approved || doc.IdDocStatus == DocStatus.Project)
                {
                    doc.ExecuteOperation(e => e.Change(context));
                    doc = dataSetDoc.Single(s => s.IdParent == doc.Id);
                }
                else if (doc.IdDocStatus != DocStatus.Draft)
                {
                    Controls.Throw(string.Format(
                        "Найденный порожденный документ '{0}' находится на неподходящем статусе '{1}' !",
                        doc.Caption,
                        context.DocStatus.SingleOrDefault(s => s.Id == doc.IdDocStatus)
                    ));
                }
            }

            if (srcDoc.DocType.IdEntity == StateProgram.EntityIdStatic)
            {
                if (!idAnalyticalCode.HasValue)
                    Controls.Throw(string.Format(
                        "Не могу создать документ '{0}' так как поле 'Код' для этого документа обязательное, а в источнике поле не заполнено !",
                        context.DocType.Single(s => s.Id == sp.IdDocType).Caption
                    ));

                if (idDocEntity == LongTermGoalProgram.EntityIdStatic || idDocEntity == ActivityOfSBP.EntityIdStatic)
                {
                    doc.SetValue("HasMasterDoc", true);
                }
            }
            doc.IdMasterDoc = srcDoc.Id;

            doc.IdPublicLegalFormation = srcDoc.IdPublicLegalFormation;
            doc.IdDocType = sp.IdDocType;
            doc.IdVersion = srcDoc.IdVersion;
            doc.DocType = context.DocType.SingleOrDefault(s => s.Id == doc.IdDocType);
            doc.Caption = sp.Caption;
            doc.IdSBP = sp.IdSBP;
            doc.IdResponsibleExecutantType = sp.IdResponsibleExecutantType;
            doc.IdAnalyticalCodeStateProgramValue = idAnalyticalCode;
            doc.DateStart = sp.DateStart.Value;
            doc.DateEnd = sp.DateEnd.Value;

            doc.Header = doc.ToString();

            doc.HasAdditionalNeed = srcDoc.HasAdditionalNeed;

            context.SaveChanges();

            sp.IdDocument = doc.Id;
            sp.IdDocumentEntity = idDocEntity;

            context.SaveChanges();
            
            return doc;
        }

        /// <summary>
        /// создаем или обновляем строки в ТЧ Элементы СЦ
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="idTpEntity">ид сущности тч целей куда вставляем данные</param>
        /// <param name="sp">строка ТЧ Список подпрограмм</param>
        /// <param name="curDoc">документ ПП</param>
        private static void CreateOrUpdate_SubDocSGE_SystemGoalElement(DataContext context, int idTpEntity, ITpListSubProgram sp, ISubDocSGE curDoc)
        {
            var listSge = new List<int>();

            //Основная цель из элемента ТЧ
            var mainSystemGoal = context.SystemGoal.FirstOrDefault(s => s.Id == sp.IdSystemGoal);
            if (mainSystemGoal == null)
                Controls.Throw("В справочнике СЦ отсутствует элемент указанный в табличной части.");

            var dataSetTp = context.Set<ITpSystemGoalElement>(idTpEntity);
            
            //Основная цель в новом документе
            ITpSystemGoalElement mainGoal = dataSetTp.Where(w => w.IdOwner == curDoc.Id && w.IsMainGoal).FirstOrDefault();
            //Родитель основной цели в новом документе
            ITpSystemGoalElement parentGoal = (mainGoal == null || !mainGoal.IdParent.HasValue ? null : dataSetTp.SingleOrDefault(w => w.Id == mainGoal.IdParent));

            if (parentGoal == null && mainGoal != null && !mainGoal.IdParent.HasValue)
            {
                // => будет предпринята попытка создать родительскую запись в ТЧ "Элементы СЦ" для основной цели mainGoal
                // Перед этим найдем родительскую запись в справочнике SystemGoal и проверим нет ли такой записи в ТЧ
                if (!mainGoal.SystemGoal.IdParent.HasValue)
                    throw new PlatformException("Некорректные исходные данные: Цель, указанная в строке ТЧ, помеченной признаком Основная, не имеет родителя в справочнике целей.");

                if (dataSetTp.Any(row => row.IdOwner == curDoc.Id && row.IdSystemGoal == mainGoal.SystemGoal.IdParent))
                    throw new PlatformException("В ТЧ Элементы СЦ присутствует строка с родительской целью, однако сама строка не является родительской по отношению к основной.");
            }
            
            if (!mainSystemGoal.IdParent.HasValue || !mainSystemGoal.IdParent.HasValue)
                Controls.Throw("У основной цели отсутствует родитель.");
            
            //Добавляем родителя основной цели в новый документ
            if (parentGoal == null)
            {
                parentGoal = dataSetTp.Create();
                parentGoal.IdOwner = curDoc.Id;
                dataSetTp.Add(parentGoal);
            }
            parentGoal.IdSystemGoal = mainSystemGoal.IdParent.Value;
            parentGoal.IsMainGoal = false;
            parentGoal.FromAnotherDocumentSE = true;
            context.SaveChanges();

            //Добавляем основную цель в новый документ
            if (mainGoal == null)
            {
                mainGoal = dataSetTp.Create();
                mainGoal.IdOwner = curDoc.Id;
                dataSetTp.Add(mainGoal);
            }
            mainGoal.IdParent = parentGoal.Id;
            mainGoal.IdSystemGoal = mainSystemGoal.Id;
            mainGoal.IsMainGoal = true;
            mainGoal.FromAnotherDocumentSE = false;
            context.SaveChanges();
            

            listSge.Add(parentGoal.Id);
            listSge.Add(mainGoal.Id);
            int[] items = listSge.ToArray();

            //Добавляем детей основной цели
            items = AddUnderlyingSystemGoalElements(context, dataSetTp, mainGoal, curDoc, items);

            curDoc.RefreshData_SystemGoalElement(context, items);
            curDoc.FillData_GoalIndicator_Value(context, items);
        }

        /// <summary>
        /// Добавить нижележащие элементы для основной цели
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tpContext"></param>
        /// <param name="mainGoalElement"></param>
        /// <param name="doc"></param>
        private static int[] AddUnderlyingSystemGoalElements(DataContext context, IQueryableDbSet<ITpSystemGoalElement> tpContext, ITpSystemGoalElement mainGoalElement, ISubDocSGE doc, int[] items)
        {
            var elementsIds = new List<int>(items);

            var systemGoalElements = context.SystemGoal.Where(s => s.IdParent == mainGoalElement.IdSystemGoal && s.IdDocType_CommitDoc == doc.IdDocType).ToList();
            foreach (var systemGoal in systemGoalElements)
            {
                //Проверяем, что элемента нет в ТЧ СЦ
                if ( tpContext.Any(s=>s.IdOwner == doc.Id && s.IdSystemGoal == systemGoal.Id) )
                    continue;
                
                //Добавляем новый элемент
                var element = tpContext.Create();

                element.IdOwner = doc.Id;
                element.IdSystemGoal = systemGoal.Id;
                element.IdParent = mainGoalElement.Id;
                element.IsMainGoal = false;
                element.FromAnotherDocumentSE = false;

                tpContext.Add(element);
                context.SaveChanges();

                elementsIds.Add(element.Id);
            }

            return elementsIds.ToArray();
        }

        /// <summary>
        /// создаем заново строки в ТЧ Ресурсное обеспечение
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="idTpSrcEntity1">ид сущности тч ресурсного обеспечения (источник данных для создаваемого доукмента)</param>
        /// <param name="idTpSrcEntity2">ид сущности тч ресурсного обеспечения - знечиния (источник данных для создаваемого доукмента)</param>
        /// <param name="idTpEntity1">ид сущности тч ресурсного обеспечения (куда вставляем новые данные)</param>
        /// <param name="idTpEntity2">ид сущности тч ресурсного обеспечения - знечиния (куда вставляем новые данные)</param>
        /// <param name="sp">строка ТЧ Список подпрограмм</param>
        /// <param name="curDoc">документ ПП</param>
        private static void ReCreate_SubDocSGE_ResourceMaintenance(DataContext context, int idTpSrcEntity1, int idTpSrcEntity2, int idTpEntity1, int idTpEntity2,
                                                       ITpListSubProgram sp, ISubDocSGE curDoc)
        {
            var dataSetTpSrc1 = context.Set<ITpResourceMaintenance>(idTpSrcEntity1);
            var dataSetTpSrc2 = context.Set<ITpAddValue>(idTpSrcEntity2);

            var dataSetTp1 = context.Set<ITpResourceMaintenance>(idTpEntity1);
            var dataSetTp2 = context.Set<ITpAddValue>(idTpEntity2);

            // удаляем все записи ТЧ Ресурсное обеспечение
            foreach (var rec in dataSetTp1.Where(r => r.IdOwner == curDoc.Id))
            {
                dataSetTp1.Remove(rec);
            }
            context.SaveChanges();

            // заново создаем
            foreach (var rm in dataSetTpSrc1.Where(r => r.IdMaster == sp.Id).ToList())
            {
                var newResourceMaintenance = dataSetTp1.Create();
                newResourceMaintenance.IdOwner = curDoc.Id;
                newResourceMaintenance.IdFinanceSource = rm.IdFinanceSource;
                dataSetTp1.Add(newResourceMaintenance);
                context.SaveChanges();

                foreach (var rmv in dataSetTpSrc2.Where(r => r.IdMaster == rm.Id).ToList())
                {
                    var newResourceMaintenanceValue = dataSetTp2.Create();
                    newResourceMaintenanceValue.IdOwner = curDoc.Id;
                    newResourceMaintenanceValue.IdMaster = newResourceMaintenance.Id;
                    newResourceMaintenanceValue.IdHierarchyPeriod = rmv.IdHierarchyPeriod;
                    newResourceMaintenanceValue.Value = rmv.Value;
                    newResourceMaintenanceValue.AdditionalValue = rmv.AdditionalValue;
                    dataSetTp2.Add(newResourceMaintenanceValue);
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Создаем или обновляем подчиненный документ
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="idType">Тип создаваемого документа, null если любой из тч подпрограмм</param>
        /// <param name="idTpSubProgEntity">ид сущности тч подпрограмм</param>
        /// <param name="idTpSrcEntity0">ид сущности тч целей (источник данных для создаваемого документа)</param>
        /// <param name="idTpSrcEntity1">ид сущности тч ресурсного обеспечения (источник данных для создаваемого документа)</param>
        /// <param name="idTpSrcEntity2">ид сущности тч ресурсного обеспечения - значения (источник данных для создаваемого документа)</param>
        /// <param name="idTpEntity0">ид сущности тч целей (куда вставляем новые данные)</param>
        /// <param name="idTpEntity1">ид сущности тч ресурсного обеспечения (куда вставляем новые данные)</param>
        /// <param name="idTpEntity2">ид сущности тч ресурсного обеспечения - значения (куда вставляем новые данные)</param>
        /// <param name="idMainGoal"></param>
        /// <param name="srcDoc">Исходный документ</param>
        public static void CreateSubDocSGE(DataContext context, int? idType, int idTpSubProgEntity, int idTpSrcEntity0, int idTpSrcEntity1, int idTpSrcEntity2, int idTpEntity0, int idTpEntity1, int idTpEntity2, int idMainGoal, IDocSGE srcDoc)
        {
            var subPrograms = context.Set<ITpListSubProgram>(idTpSubProgEntity).Where(r => r.IdOwner == srcDoc.Id).ToList();
            foreach (var sp in subPrograms.Where(w => !idType.HasValue || w.IdDocType == idType))
            {
                // создаем или обновляем подчиненный документ
                DocType docTyp = context.DocType.Single(w => w.Id == sp.IdDocType);
                ISubDocSGE curdoc = CreateOrUpdate_SubDocSGE(context, docTyp.IdEntity, idTpEntity0, srcDoc, sp, idTpSubProgEntity, idTpSrcEntity1, idTpSrcEntity2, idTpEntity1, idTpEntity2);

                if (curdoc == null) continue;

                var dataSetTp = context.Set<ITpSystemGoalElement>(idTpEntity0);
                var tpSystemGoalElement0s = dataSetTp.Where(w => w.IdOwner == curdoc.Id && w.IsMainGoal);
                if (tpSystemGoalElement0s.Count() > 1)
                {
                    var docheader = context.Set<IHierarhy>(docTyp.IdEntity).FirstOrDefault(r => r.Id == curdoc.Id);

                    if (docheader == null)
                        return;
                    
                    var msg = string.Format("Следующие документы содержат несколько основных целей. " +
                                            "Необходимо в данных документах оставить одну основную цель в таблице «Элементы СЦ»:<br>" +
                                            " - {0} «{1}» {2}<br>",
                                    context.SBP.Where(r => r.Id == sp.IdSBP).Select(r=>r.Caption).FirstOrDefault(),
                                    sp.Caption,
                                    docheader.GetValue("Header")); 
                    Controls.Throw(msg);
                }

                // создаем или обновляем строки в ТЧ Элементы СЦ
                CreateOrUpdate_SubDocSGE_SystemGoalElement(context, idTpEntity0, sp, curdoc);

                // создаем заново строки в ТЧ Ресурсное обеспечение
                ReCreate_SubDocSGE_ResourceMaintenance(context, idTpSrcEntity1, idTpSrcEntity2, idTpEntity1, idTpEntity2, sp, curdoc);
            }

            context.SaveChanges();
        }



        #endregion
    }
}
