using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.AppServices;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Scopes;
using Platform.Common;
using Platform.Common.Extensions;
using Sbor.DbEnums;
using Sbor.Logic;
using Sbor.Tablepart;

namespace Sbor.Services
{
    /// <summary>
    /// Сервисы ЭД Смета казенного учреждения
    /// </summary>
    [AppService]
    public class SborPublicInstitutionEstimateService
    {
        /// <summary>
        /// Кнопка заполнить на ТЧ "Мероприятия"
        /// </summary>
        /// <param name="id">id текущего документа</param>
        public void fillData_PublicInstitutionEstimate_Activities(CommunicationContext communicationContext, int id)
        {
            new CommunicationContextScope(communicationContext);
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

                doc.FillData_Activities(context);
            }
        }

        /// <summary>
        /// Кнопка заполнить на ТЧ "Мероприятия АУ/БУ"
        /// </summary>
        /// <param name="id">id текущего документа</param>
        public void fillData_PublicInstitutionEstimate_ActivitiesAUBU(CommunicationContext communicationContext, int id)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

                doc.FillData_ActivitiesAUBU(context);
            }
        }

        /// <summary>
        /// Кнопка заполнить на ТЧ "Расходы АУ/БУ"
        /// </summary>
        /// <param name="communicationContext"></param>
        /// <param name="id">id текущего документа</param>
        public void fillData_PublicInstitutionEstimate_ExpenseAloneSubjects(CommunicationContext communicationContext, int id)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

                doc.FillData_ExpenseAloneSubjects(context);
            }
            
        }

        /// <summary>
        /// Кнопка заполнить на ТЧ "Расходы учредителя АУ/БУ"
        /// </summary>
        /// <param name="id">id текущего документа</param>
        /// <param name="rows">Выбранные мероприятия АУБУ</param>
        public void fillData_PublicInstitutionEstimate_FounderAUBUExpenses(int id, int[] rows)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

            doc.FillData_FounderAUBUExpenses(context, rows);
        }

        /// <summary>
        /// Получить по бланку СБП для текущего документа список скрываемых полей в гриде и форме сметной строки
        /// </summary>
        public Dictionary<string, IEnumerable<string>> get_Sbpblank(int idDocument)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PublicInstitutionEstimate.FirstOrDefault(l => l.Id == idDocument);
            if (doc == null)
                return null;

            var sbpBlank = context.SBP_BlankHistory.FirstOrDefault(r => r.Id == doc.IdSBP_BlankActual);
            if (sbpBlank == null)
            {
                return null;
            }

            return sbpBlank.GetBlankCostProperties();
        }

        /// <summary>
        /// Получить по бланку формирования АУБУ для текущего документа список скрываемых полей в гриде и форме сметной строки
        /// </summary>
        public Dictionary<string, IEnumerable<string>> get_SbpblankAuBu(int idDocument)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PublicInstitutionEstimate.FirstOrDefault(l => l.Id == idDocument);
            if (doc == null)
                return null;

            var sbpBlank = context.SBP_BlankHistory.FirstOrDefault(r => r.Id == doc.IdSBP_BlankActualAuBu);
            if (sbpBlank == null)
            {
                return null;
            }

            return sbpBlank.GetBlankCostProperties();
        }

        /// <summary>
        /// Получение контингента и показателя объема
        /// </summary>
        /// <param name="idOwner"></param>
        /// <param name="idActivity"></param>
        /// <param name="idContingent"></param>
        /// <param name="isAuBu"></param>
        /// <returns></returns>
        public Dictionary<string, object> getDefault_Activity(int idOwner, int idActivity, int? idContingent, bool isAuBu)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var result = new Dictionary<string, object>();

            var doc = context.PublicInstitutionEstimate.FirstOrDefault(d => d.Id == idOwner);
            var activity = context.Activity.FirstOrDefault(r => r.Id == idActivity);

            if (activity == null || doc == null) 
                return result;

            var regValues = context.TaskVolume.Where(tv => tv.IdPublicLegalFormation == doc.IdPublicLegalFormation &&
                                                        !tv.IdTerminator.HasValue &&
                                                        tv.IdVersion == doc.IdVersion &&
                                                        tv.IdSBP == doc.IdSBP &&
                                                        tv.HierarchyPeriod.Year >= doc.Budget.Year && 
                                                        tv.HierarchyPeriod.Year <= (doc.Budget.Year + 2) &&
                                                        isAuBu ? (tv.ActivityAUBU.HasValue && tv.ActivityAUBU.Value) : (!tv.ActivityAUBU.HasValue || !tv.ActivityAUBU.Value) &&
                                                        tv.TaskCollection.IdActivity == activity.Id
                                                       );

            var contingents = idContingent.HasValue ? new []{context.Contingent.FirstOrDefault(c=>c.Id == idContingent.Value)} : regValues.Where(r => r.TaskCollection.IdContingent.HasValue).Select(r => r.TaskCollection.Contingent).Distinct().Take(2).ToArray();

            if (contingents.Length == 0)
            {
                result.Add("idcontingent", null);
                result.Add("idcontingent_caption", String.Empty);

                var indicators = regValues.Where(rv => !rv.TaskCollection.IdContingent.HasValue).Select(rv => rv.IndicatorActivity_Volume).Distinct().Take(2).ToArray();
                if (indicators.Length == 1)
                {
                    var indicator = indicators.First();
                    result.Add("idindicator", indicator.Id);
                    result.Add("idindicator_caption", indicator.Caption);

                    result.Add("idindicatoractivitytype", indicator.IdIndicatorActivityType);
                    result.Add("idindicatoractivitytype_caption", indicator.IndicatorActivityType.Caption());
                }
            }else if (contingents.Length == 1)
            {
                    var contingent = contingents.First();
                    result.Add("idcontingent", contingent.Id);
                    result.Add("idcontingent_caption", contingent.Caption);

                    var indicators = regValues.Where(rv => rv.TaskCollection.IdContingent == contingent.Id ).Select(rv=>rv.IndicatorActivity_Volume).Distinct().Take(2).ToArray();
                    if (indicators.Length == 1)
                    {
                        var indicator = indicators.First();
                        result.Add("idindicator", indicator.Id);
                        result.Add("idindicator_caption", indicator.Caption);

                        result.Add("idindicatoractivitytype", indicator.IdIndicatorActivityType);
                        result.Add("idindicatoractivitytype_caption", indicator.IndicatorActivityType.Caption() );
                    }
            }
            
            return result;
        }

        /// <summary>
        /// Распределение косвенных расходов
        /// </summary>
        public void CalculateIndirectExpenses(int id, int[] rows)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

            doc.CalculateIndirectExpenses(context, rows);
        }

        /// <summary>
        /// Кнопка 'Вычислить коды РО' на ТЧ "Мероприятия"
        /// </summary>
        /// <param name="id">id текущего документа</param>
        public void calculateRo_PublicInstitutionEstimate_Activities(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

            doc.calculateRo_Activities(context, items);

        }

        /// <summary>
        /// Кнопка 'Вычислить коды РО' на ТЧ "Расходы"
        /// </summary>
        /// <param name="id">id текущего документа</param>
        public void calculateRo_PublicInstitutionEstimate_Expenses(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

            doc.CalculateRo_Expenses(context, items);
            
        }

        /// <summary>
        /// Кнопка 'Вычислить коды РО' на ТЧ "МероприятияАуБу"
        /// </summary>
        /// <param name="id">id текущего документа</param>
        public void calculateRo_PublicInstitutionEstimate_ActivitiesAUBU(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

            doc.calculateRo_ActivitiesAUBU(context, items);
        }

        /// <summary>
        /// Кнопка 'Вычислить коды РО' на ТЧ "РасходыУчередителяАуБу"
        /// </summary>
        /// <param name="id">id текущего документа</param>
        public void calculateRo_PublicInstitutionEstimate_FounderAUBUExpenses(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PublicInstitutionEstimate.Single(s => s.Id == id);

            doc.calculateRo_FounderAUBUExpenses(context, items);
        } 
    }
}
