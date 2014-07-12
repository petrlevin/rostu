using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.DataAccess;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Scopes;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Logic;
using Sbor.Logic.Hierarchy;
using Sbor.Tablepart;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;


namespace Sbor.Services
{
    /// <summary>
    /// Общий для проекта "Sbor" веб-сервис
    /// </summary>
    [AppService]
    class SborCommonService
    {
        /// <summary>
        /// Получение типа документа по-умолчанию для "Тип элемента СЦ"
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborCommonService.getElementTypeSystemGoalDefaultTypeDoc(int idType)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetElementTypeSystemGoalDefaultTypeDoc(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var r = context.ElementTypeSystemGoal_Document.Where(w => w.IdOwner == id && w.IsDefault)
                   .Select(s => new { Id = s.IdDocType, Caption = s.DocType.Caption })
                   .FirstOrDefault();

            return new Dictionary<string, object>()
			{
				{"Id",      r == null ? null : (int?)r.Id },
				{"Caption", r == null ? null : r.Caption  }
            };
        }

        /// <summary>
        /// Проверяет есть ли разрешение для СБП на ввод дополнительной потребности
        /// </summary>
        public bool getPermissionsInputAdditionalRequirements(int? IdSBP)
        {
            if (!IdSBP.HasValue)
            {
                return false;
            }

            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            // находим вышестоящий ГРБС
            var SBP = context.SBP.SingleOrDefault(s=> s.Id == IdSBP);
            var curSBP = SBP;
            while (curSBP != null)
            {

                curSBP = curSBP.IdParent.HasValue ? context.SBP.SingleOrDefault(w => w.Id == curSBP.IdParent) : null;
                if (curSBP != null) SBP = curSBP;
            }
 
            // находим для него разрешения

            var Permissions = context.PermissionsInputAdditionalRequirements.Where(w => w.IdSBP == SBP.Id);

            var error = false;

            if (Permissions.Any())
            {
                foreach (var permission in Permissions)
                {
                    error = permission.EnterAdditionalRequirements;
                }
            }

            return error;

        }

  
        #region Сервисы ЭД Документы СЭР

        /// <summary>
        /// Заполнить по актуальному состоянию справочника «Система целеполагания» ТЧ "Элементы СЦ","Целевые показатели","Значения целевых показателей" ЭД "Документы СЭР"
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborCommonService.fillData_tpDocumentsOfSED_ItemsSystemGoals(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public void fillData_tpDocumentsOfSED_ItemsSystemGoals(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.DocumentsOfSED.Single(s => s.Id == id);

            doc.FillData_ItemsSystemGoals(context);
        }

        /// <summary>
        /// Обновить реквизиты выделенных элементов СЦ
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborCommonService.refreshData_tpDocumentsOfSED_ItemsSystemGoals(id, items)
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public void refreshData_tpDocumentsOfSED_ItemsSystemGoals(CommunicationContext communicationContext, int id, int[] items)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.DocumentsOfSED.Single(s => s.Id == id);

                doc.RefreshData_ItemsSystemGoals(context, items, flag: true);
            }
        }

        /// <summary>
        /// Заполнить (обновить) целевые показатели  выделенных элементов СЦ по актуальному состоянию справочника «Система целеполагания»
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborCommonService.fillData_tpDocumentsOfSED_GoalIndicatorValues(id, items)
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public void fillData_tpDocumentsOfSED_GoalIndicatorValues(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.DocumentsOfSED.Single(s => s.Id == id);

            doc.FillData_GoalIndicatorValues(context, items);
        }

        /// <summary>
        /// Обновить значения выделенных целевых показателей
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborCommonService.refreshData_tpDocumentsOfSED_GoalIndicatorValues(id, items)
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public void refreshData_tpDocumentsOfSED_GoalIndicatorValues(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.DocumentsOfSED.Single(s => s.Id == id);

            doc.RefreshData_GoalIndicatorValues(context, items);
        }

        #endregion

        #region Сервисы ЭД Деятельность ведомства

        /// <summary>
        /// Заполнить по актуальному состоянию справочника «Система целеполагания» ТЧ "Элементы СЦ","Целевые показатели","Значения целевых показателей" ЭД "Документы СЭР"
        /// </summary>
        public void fillData_ActivityOfSBP_SystemGoalElement(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.ActivityOfSBP.Single(s => s.Id == id);

            doc.FillData_ItemsSystemGoals(context);
        }

        /// <summary>
        /// Обновить реквизиты выделенных элементов СЦ
        /// </summary>
        public void refreshData_ActivityOfSBP_SystemGoalElement(CommunicationContext communicationContext, int id, int[] items)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.ActivityOfSBP.Single(s => s.Id == id);

                doc.RefreshData_SystemGoalElement(context, items, flag: true);
            }
            
        }

        /// <summary>
        /// Заполнить (обновить) целевые показатели  выделенных элементов СЦ по актуальному состоянию справочника «Система целеполагания»
        /// </summary>
        public void fillData_ActivityOfSBP_GoalIndicator_Value(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.ActivityOfSBP.Single(s => s.Id == id);

            doc.FillData_GoalIndicator_Value(context, items);
        }

        /// <summary>
        /// Заполнить (обновить) целевые показатели  выделенных элементов СЦ по актуальному состоянию справочника «Система целеполагания»
        /// </summary>
        public void refreshData_ActivityResourceMaintenance_Value(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.ActivityOfSBP.Single(s => s.Id == id);

            doc.FillData_ActivityResourceMaintenance_Value(context, items);
        }

        /// <summary>
        /// Обновить значения выделенных целевых показателей
        /// </summary>
        public void refreshData_ActivityOfSBP_GoalIndicator_Value(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.ActivityOfSBP.Single(s => s.Id == id);

            doc.RefreshData_GoalIndicator_Value(context, items);
        }

        public Dictionary<string, object> getDefaultActivityOfSBP_Activity(int idOwner, int idActivity)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.ActivityOfSBP.Where(r => r.Id == idOwner).FirstOrDefault();

            var result = new Dictionary<string, object>();

            result.Add("idsbp", doc.SBP.Id);
            result.Add("idsbp_caption", doc.SBP.Caption);

            var activity = context.Activity.Where(r => r.Id == idActivity).FirstOrDefault();

            var indicators =
                context.Activity_Indicator.Where(
                    i =>
                    i.IdOwner == activity.Id &&
                    i.IndicatorActivity.IdIndicatorActivityType == (byte)IndicatorActivityType.VolumeIndicator &&
                    i.IdSBP == doc.IdSBP);

            if (indicators.Count() == 1)
            {
                var indicator = indicators.FirstOrDefault().IndicatorActivity;

                result.Add("idindicator", indicator.Id);
                result.Add("idindicator_caption", indicator.Caption);
            }

            if (activity.Activity_Contingent.Count() == 1 && (activity.IdActivityType == (byte)ActivityType.Service || activity.IdActivityType == (byte)ActivityType.PublicLiability))
            {
                result.Add("idcontingent", activity.Activity_Contingent.FirstOrDefault().Id);
                result.Add("idcontingent_caption", activity.Activity_Contingent.FirstOrDefault().Caption);
            }

            return result;
        }

        /// <summary>
        /// Получить по бланку СБП для текущего документа список скрываемых полей в гриде
        /// </summary>
        public Dictionary<string, IEnumerable<string>> getSbpBlank_ActivityOfSBP(int? id, int? idBudget = null, bool Str = false)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            bool isBudget = idBudget != null;

            var doc = context.ActivityOfSBP.SingleOrDefault(s => s.Id == id);
            if (doc == null)
                return null;

            var field_for_budget = new List<string>() {"idexpenseobligationtype", "idkfo", "idkvsr", "idrzpr", "idkcsr", "idkvr", "idkosgu", "iddfk", "iddkr", "iddek", "idcodesubsidy", "idbranchcode", "idokato", "idauthorityofexpenseobligation"};
            if (Str && !isBudget)
            {
                // с не заполненным бюджетом
                // скрываем все колонки КБК
                return new Dictionary<string, IEnumerable<string>>(){
                    {"hidden", field_for_budget},
                    {"required", new List<string>()}
                };

            }

            if (!Str && !doc.ActivityResourceMaintenance.Any(a => a.IdBudget.HasValue))
            {
                // если ТЧ не заполнена или есть только 1 строка с не заполненным бюджетом
                // скрываем все колонки КБК
                return new Dictionary<string, IEnumerable<string>>(){
                    {"hidden", field_for_budget},
                    {"required", new List<string>()}
                };

            }
            
            {
                // если есть строки с заполненным бюджетом 
                // скрываем по бланку
                var sbp = doc.SBP;
                if (sbp == null)
                    return null;

                var budget = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget;

                SBP_BlankHistory sbpBlank;
                var activityOfSbpSbpBlankActuals = context.ActivityOfSBP_SBPBlankActual.Where(a => a.SBP_BlankHistory.IdBudget == budget.Id && a.IdOwner == id);
                if (!activityOfSbpSbpBlankActuals.Any())
                {
                    sbpBlank = null;
                }
                else
                {
                    var sbpBlankHistories = context.SBP_BlankHistory.Where(r => r.Id == activityOfSbpSbpBlankActuals.FirstOrDefault().IdSBP_BlankHistory);
                    if (!sbpBlankHistories.Any())
                    {
                        sbpBlank = null;
                    }
                    else
                    {
                        sbpBlank = sbpBlankHistories.FirstOrDefault();
                    }
                }

                if (sbpBlank == null)
                {
                    return new Dictionary<string, IEnumerable<string>>(){
                        {"hidden", new List<string>(){"idexpenseobligationtype","idkfo","idkvsr","idrzpr","idkcsr","idkvr","idkosgu","iddfk","iddkr","iddek","idcodesubsidy","idbranchcode"}},
                        {"required", new List<string>()}
                    };
                }

                return sbpBlank.GetBlankCostProperties();
            }
        }

        /// <summary>
        /// Получаем  ID периодов 
        /// </summary>
        public Dictionary<string, IEnumerable<string>> getPeriods_ActivityOfSBP(int? IdBudget)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var Budget = context.Budget.SingleOrDefault(s => s.Id == IdBudget);
            var Periods = context.HierarchyPeriod.Where(w => !w.IdParent.HasValue);
            var hidden = new List<string>();
            var required = new List<string>();

            foreach (var Period in Periods)
            {
                if (Budget == null)
                {
                    required.Add(String.Format("value{{" + Period.Id.ToString() + "}}"));
                    continue;
                }

                if (Period.DateEnd.Year == Budget.Year || Period.DateEnd.Year == Budget.Year + 1 ||
                    Period.DateEnd.Year == Budget.Year + 2)
                {
                    required.Add(String.Format("value{{" + Period.Id.ToString() + "}}"));
                }
                else
                {
                    hidden.Add(String.Format("value{{" + Period.Id.ToString() + "}}"));
                }
            }

            return new Dictionary<string, IEnumerable<string>>()
                {
                    {"hidden", hidden},
                    {"required", required}
                };

        }
        #endregion

        #region Сервисы ЭД Долгосрочная целевая программа

        public Dictionary<string, object> getDefaulLongTermGoalProgram_Activity(int idActivity, int IdSBP)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var result = new Dictionary<string, object>();

            var activity = context.Activity.Where(r => r.Id == idActivity).FirstOrDefault();

            var indicators =
                context.Activity_Indicator.Where(
                    i =>
                    i.IdOwner == activity.Id &&
                    i.IndicatorActivity.IdIndicatorActivityType == (byte)IndicatorActivityType.VolumeIndicator &&
                    i.IdSBP == IdSBP);

            if (indicators.Count() == 1)
            {
                var indicator = indicators.FirstOrDefault().IndicatorActivity;

                result.Add("idindicator", indicator.Id);
                result.Add("idindicator_caption", indicator.Caption);
            }

            if (activity.Activity_Contingent.Count() == 1 && (activity.IdActivityType == (byte)ActivityType.Service || activity.IdActivityType == (byte)ActivityType.PublicLiability))
            {
                result.Add("idcontingent", activity.Activity_Contingent.FirstOrDefault().Id);
                result.Add("idcontingent_caption", activity.Activity_Contingent.FirstOrDefault().Caption);
            }

            return result;
        }


        /// <summary>
        /// Заполнить по актуальному состоянию справочника «Система целеполагания» ТЧ "Элементы СЦ","Целевые показатели","Значения целевых показателей" ЭД "Документы СЭР"
        /// </summary>
        public void fillData_LongTermGoalProgram_SystemGoalElement(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.LongTermGoalProgram.Single(s => s.Id == id);

            doc.FillData_ItemsSystemGoals(context);
        }

        /// <summary>
        /// Обновить реквизиты выделенных элементов СЦ
        /// </summary>
        public void refreshData_LongTermGoalProgram_SystemGoalElement(CommunicationContext communicationContext, int id, int[] items)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.LongTermGoalProgram.Single(s => s.Id == id);

                doc.RefreshData_SystemGoalElement(context, items, flag: true);
            }
        }

        /// <summary>
        /// Заполнить (обновить) целевые показатели  выделенных элементов СЦ по актуальному состоянию справочника «Система целеполагания»
        /// </summary>
        public void fillData_LongTermGoalProgram_GoalIndicator_Value(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.LongTermGoalProgram.Single(s => s.Id == id);

            doc.FillData_GoalIndicator_Value(context, items);
        }

        /// <summary>
        /// Обновить значения выделенных целевых показателей
        /// </summary>
        public void refreshData_LongTermGoalProgram_GoalIndicator_Value(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.LongTermGoalProgram.Single(s => s.Id == id);

            doc.RefreshData_GoalIndicator_Value(context, items);
        }

        #endregion

        #region Сервисы ЭД Государственная программа

        /// <summary>
        /// Заполнить по актуальному состоянию справочника «Система целеполагания» ТЧ "Элементы СЦ","Целевые показатели","Значения целевых показателей" ЭД "Документы СЭР"
        /// </summary>
        public void fillData_StateProgram_SystemGoalElement(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.StateProgram.Single(s => s.Id == id);

            doc.FillData_ItemsSystemGoals(context);
        }

        /// <summary>
        /// Обновить реквизиты выделенных элементов СЦ
        /// </summary>
        public void refreshData_StateProgram_SystemGoalElement(CommunicationContext communicationContext, int id, int[] items)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.StateProgram.Single(s => s.Id == id);

                doc.RefreshData_SystemGoalElement(context, items, flag: true);
            }
        }

        /// <summary>
        /// Заполнить (обновить) целевые показатели  выделенных элементов СЦ по актуальному состоянию справочника «Система целеполагания»
        /// </summary>
        public void fillData_StateProgram_GoalIndicator_Value(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.StateProgram.Single(s => s.Id == id);

            doc.FillData_GoalIndicator_Value(context, items);
        }

        /// <summary>
        /// Обновить значения выделенных целевых показателей
        /// </summary>
        public void refreshData_StateProgram_GoalIndicator_Value(int id, int[] items)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.StateProgram.Single(s => s.Id == id);

            doc.RefreshData_GoalIndicator_Value(context, items);
        }

        //http://jira.rostu-comp.ru/browse/SBORIII-312
        public GridResult getListSubProgramExisted(GridParams gridParams)
        {
            //БРОВКИН вот параметр который тебе нужно использовать
            var docId = gridParams.DocId;
            var connection = IoC.Resolve<SqlConnection>("DbConnection");
            var command = connection.CreateCommand();
            command.CommandText = @"
SELECT   [a].[idApproved],
         [a].[idApprovedEntity],
         [a].[idRegistrator],
         [a].[idRegistratorEntity],
         [a].[idTerminator],
         [a].[idTerminatorEntity],
         [a].[id],
         [a].[idPublicLegalFormation],
         [a].[idVersion],
         [a].[DateCommit],
         [a].[DateTerminate],
         [a].[DateCreate],
         [a].[idProgram],
         [a].[idAnalyticalCodeStateProgram],
         [a].[Caption],
         [a].[DateStart],
         [a].[DateEnd],
         [a].[idGoalSystemElement],
         [a].[idParent],
         [a].[idApproved_caption],
         [a].[idApprovedEntity_Caption],
         [a].[idRegistrator_caption],
         [a].[idRegistratorEntity_Caption],
         [a].[idTerminator_caption],
         [a].[idTerminatorEntity_Caption],
         [a].[idPublicLegalFormation_Caption],
         [a].[idVersion_Caption],
         [a].[idProgram_Caption],
         [a].[idAnalyticalCodeStateProgram_Caption],
         [a].[idGoalSystemElement_Caption],
         [a].[idParent_Caption],
         [a].[ProgramType],
         [a].[ProgramSbp],
         [a].[_TotalRow]
FROM     (SELECT [a].[idApproved],
                 [a].[idApprovedEntity],
                 [a].[idRegistrator],
                 [a].[idRegistratorEntity],
                 [a].[idTerminator],
                 [a].[idTerminatorEntity],
                 [a].[id],
                 [a].[idPublicLegalFormation],
                 [a].[idVersion],
                 [a].[DateCommit],
                 [a].[DateTerminate],
                 [a].[DateCreate],
                 [a].[idProgram],
                 [a].[idAnalyticalCodeStateProgram],
                 [a].[Caption],
                 [a].[DateStart],
                 [a].[DateEnd],
                 [a].[idGoalSystemElement],
                 [a].[idParent],
                 [dbo].[GetCaption]([a].[idApprovedEntity], [a].[idApproved]) AS [idApproved_caption],
                 [b].[Caption] AS [idApprovedEntity_Caption],
                 [dbo].[GetCaption]([a].[idRegistratorEntity], [a].[idRegistrator]) AS [idRegistrator_caption],
                 [c].[Caption] AS [idRegistratorEntity_Caption],
                 [dbo].[GetCaption]([a].[idTerminatorEntity], [a].[idTerminator]) AS [idTerminator_caption],
                 [d].[Caption] AS [idTerminatorEntity_Caption],
                 [e].[Caption] AS [idPublicLegalFormation_Caption],
                 [f].[Caption] AS [idVersion_Caption],
                 [g].[id] AS [idProgram_Caption],
                 [h].[AnalyticalCode] AS [idAnalyticalCodeStateProgram_Caption],
                 [i].[id] AS [idGoalSystemElement_Caption],
                 [j].[id] AS [idParent_Caption],
                 [l].[Caption] AS [ProgramType],
                 [n].[Caption] AS [ProgramSbp],
                 ROW_NUMBER() OVER ( ORDER BY [a].[Caption]) AS [RowNumber],
                 COUNT(1) OVER () AS [_TotalRow]
          FROM   [reg].[AttributeOfProgram] AS [a]
                 LEFT OUTER JOIN
                 [ref].[Entity] AS [b]
                 ON [a].[idApprovedEntity] = [b].[id]
                 INNER JOIN
                 [ref].[Entity] AS [c]
                 ON [a].[idRegistratorEntity] = [c].[id]
                 LEFT OUTER JOIN
                 [ref].[Entity] AS [d]
                 ON [a].[idTerminatorEntity] = [d].[id]
                 INNER JOIN
                 [ref].[PublicLegalFormation] AS [e]
                 ON [a].[idPublicLegalFormation] = [e].[id]
                 INNER JOIN
                 [ref].[Version] AS [f]
                 ON [a].[idVersion] = [f].[id]
                 INNER JOIN
                 [reg].[Program] AS [g]
                 ON [a].[idProgram] = [g].[id]
                 LEFT OUTER JOIN
                 [ref].[AnalyticalCodeStateProgram] AS [h]
                 ON [a].[idAnalyticalCodeStateProgram] = [h].[id]
                 INNER JOIN
                 [reg].[SystemGoalElement] AS [i]
                 ON [a].[idGoalSystemElement] = [i].[id]
                 LEFT OUTER JOIN
                 [reg].[Program] AS [j]
                 ON [a].[idParent] = [j].[id]
                 INNER JOIN
                 [reg].[Program] AS [k]
                 ON [a].[idProgram] = [k].[id]
                 INNER JOIN
                 [ref].[DocType] AS [l]
                 ON [k].[idDocType] = [l].[id]
                 INNER JOIN
                 [reg].[Program] AS [m]
                 ON [a].[idProgram] = [m].[id]
                 INNER JOIN
                 [ref].[SBP] AS [n]
                 ON [m].[idSBP] = [n].[id]
                              WHERE  ([a].[idPublicLegalFormation] = @idPublicLegalFormation
                              AND [a].[idVersion] = @idVersion)) AS [a]
            WHERE    [RowNumber] BETWEEN @start AND @end
            ORDER BY [RowNumber]
           
            ";

            var curDim = IoC.Resolve<SysDimensionsState>("CurentDimensions");

            command.Parameters.Add("@start",SqlDbType.Int);
            command.Parameters.Add("@end", SqlDbType.Int);
            command.Parameters.Add("@idPublicLegalFormation", SqlDbType.Int);
            command.Parameters.Add("@idVersion", SqlDbType.Int);

            command.Parameters["@start"].Value = (gridParams.Page - 1)*gridParams.Limit + 1;
            command.Parameters["@end"].Value = (gridParams.Page - 1)*gridParams.Limit + gridParams.Limit;
            command.Parameters["@idPublicLegalFormation"].Value = curDim.PublicLegalFormation.Id;
            command.Parameters["@idVersion"].Value = curDim.Version.Id;

            return  SqlHelper.MakeResult(command);
        }


        #endregion

        #region Сервисы ЭД План деятельности

        public Dictionary<string, object> getDefault_PlanActivity_Activity(int idOwner, int idActivity, int? idContingent, bool searchContingentOtherwiseActivity
            /*true = contingent, false = activity*/)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var result = new Dictionary<string, object>();

            var doc = context.PlanActivity.FirstOrDefault(d => d.Id == idOwner);
            var activity = context.Activity.FirstOrDefault(r => r.Id == idActivity);

            if (activity == null || doc == null) return result;

            if (doc.PublicLegalFormation.UsedGMZ ?? false)
            {
                var indicators = context.Activity_Indicator.Where(i =>
                        i.IdOwner == activity.Id
                        && i.IndicatorActivity.IdIndicatorActivityType == (byte)IndicatorActivityType.VolumeIndicator
                        && i.IdSBP == doc.IdSBP);

                //if ((activity.IdActivityType == (byte)ActivityType.Service ||
                //     activity.IdActivityType == (byte)ActivityType.PublicLiability))
                //{
                if (searchContingentOtherwiseActivity)
                    if (activity.Activity_Contingent.Count() == 1)
                    {
                        var contingent = activity.Activity_Contingent.First();
                        result.Add("idcontingent", contingent.Id);
                        result.Add("idcontingent_caption", contingent.Caption);
                    }
                //}
                else
                    if (indicators.Count() == 1)
                    {
                        var indicator = indicators.First().IndicatorActivity;
                        result.Add("idindicator", indicator.Id);
                        result.Add("idindicator_caption", indicator.Caption);
                    }
            }
            else
            {
                var idSBP_firstAncestor =
                    context.SBP.GetParentsIds(doc.IdSBP, idS => idS.Id, idPS => idPS.IdParent,
                                             stop =>
                                             stop.IdSBPType == (byte) SBPType.GeneralManager ||
                                             stop.IdSBPType == (byte) SBPType.Manager)
                           .OrderByDescending(o => o.Value)
                           .FirstOrDefault()
                           .Key;
                var q =
                    context.TaskVolume.Where(w =>
                        !w.IdTerminator.HasValue
                        && w.IdVersion == doc.IdVersion
                        && w.IdSBP == idSBP_firstAncestor
                        && w.TaskCollection.IdActivity == idActivity
                        && w.HierarchyPeriod.Year >= doc.Budget.Year
                        && w.HierarchyPeriod.Year <= (doc.Budget.Year + 2)
                    );

                //if ((activity.IdActivityType == (byte)ActivityType.Service ||
                //     activity.IdActivityType == (byte)ActivityType.PublicLiability))
                //{
                if (searchContingentOtherwiseActivity)
                {
                    var contingents = q.Where(w => w.TaskCollection.IdContingent.HasValue).Select(s => s.TaskCollection.Contingent).Distinct();
                    if (contingents.Count() == 1)
                    {
                        var contingent = contingents.First();
                        result.Add("idcontingent", contingent.Id);
                        result.Add("idcontingent_caption", contingent.Caption);
                        //idContingent = contingent.Id;
                    }
                }
                //}
                else
                {
                    var indicators = q.Where(w => (w.TaskCollection.IdContingent ?? 0) == (idContingent ?? 0)).Select(s => s.IndicatorActivity_Volume).Distinct();
                    if (indicators.Count() == 1)
                    {
                        var indicator = indicators.First();
                        result.Add("idindicator", indicator.Id);
                        result.Add("idindicator_caption", indicator.Caption);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Получить по бланку СБП для текущего документа список скрываемых полей в гриде и форме сметной строки
        /// </summary>
        public Dictionary<string, IEnumerable<string>> getSbpBlank_PlanActivity(int? id, int? idSbp)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = id.HasValue ? context.PlanActivity.SingleOrDefault(s => s.Id == id.Value) : null;
            var sbp = idSbp.HasValue ? context.SBP.SingleOrDefault(s => s.Id == idSbp.Value) : (doc != null ? doc.SBP : null);
            
            if (sbp == null || doc == null)
                return null;

            var sbpBlank = context.SBP_BlankHistory.FirstOrDefault(r => r.Id == doc.IdSBP_BlankActual);
            if (sbpBlank == null)
            {
                return null;
            }

            var result = sbpBlank.GetBlankCostProperties();

            if (sbp.IdSBPType != (byte) SBPType.TreasuryEstablishment || !sbp.IsFounder)
            {
                var temp = result["hidden"].ToList();
                temp.Add("ismeansaubu"); // скроем для казенного
                result["hidden"] = temp;
            }

            return result;
        }

        /// <summary>
        /// Заполнить (обновить) автоматически мероприятиями и объемами (с периодом Год) из документов «План деятельности» по автономным и бюджетным учреждениям
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborCommonService.fillData_PlanActivity_ActivityAUBU(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public void fillData_PlanActivity_ActivityAUBU(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.PlanActivity.Single(s => s.Id == id);

            doc.FillData_ActivityAUBU(context);
        }

        #endregion

        #region Сервисы ЭД Предельные объемы бюджетных ассигнований

        /// <summary>
        /// Заполнить (обновить) автоматически ТЧ Контрольные соотношения
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborCommonService.tpControlRelation_FillData(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public void tpControlRelation_FillData(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.LimitBudgetAllocations.Single(s => s.Id == id);

            doc.ControlRelation_FillData(context);
        }

        /// <summary>
        /// Заполнить (обновить) автоматически ТЧ Просмотр изменений
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborCommonService.tpShowChanges_FillData(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public void tpShowChanges_FillData(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.LimitBudgetAllocations.Single(s => s.Id == id);

            doc.ShowChanges_FillData(context);
        }
        #endregion


    }
}
