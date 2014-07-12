using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Scopes;
using Platform.Common;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Logic;
using Sbor.Tablepart;
using Sbor.Tool;

namespace Sbor.Services
{
    /// <summary>
    /// Сервисы Инструмента Балансировка доходов, расходов и ИФДБ
    /// </summary>
    [AppService]
    public class SborBalancingIFDB
    {
        /// <summary>
        /// Заполнить по регистрам
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborBalancingIFDB.fillData(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public void fillData(CommunicationContext communicationContext, int id)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var doc = context.BalancingIFDB.Single(s => s.Id == id);

                doc.FillData(context);
            }
        }

        /// <summary>
        /// Применить правило
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborBalancingIFDB.applyRule(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public void applyRule(CommunicationContext communicationContext, int id)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                BalancingIFDB.ApplyRule(context, id);
            }
        }

        /// <summary>
        /// Получить список суммовых полей строки ТЧ "Расходы", для которых нет детальных строк
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborBalancingIFDB.disableFields_Expense(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<string> disableFields_Expense(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            return BalancingIFDB.DisableFieldsExpense(context, id);
        }

        /// <summary>
        /// Получить список ссылочный полей строк ТЧ "Расходы", которые нужно скрыть/показать
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborBalancingIFDB.showHideFields_Expense(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public Dictionary<string, IEnumerable<string>> showHideFields_Expense(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.BalancingIFDB.Single(s => s.Id == id);

            return doc.HideShowFieldsExpense(context);
        }

        /// <summary>
        /// Получить тип инструмента по данным ТЧ "Программы/Мероприятия"
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborBalancingIFDB.showHideFields_Expense(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public byte? showHide_Programs(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            int cnt = context.BalancingIFDB_Program.Count(s => s.IdOwner == id);

            return (cnt == 0 ? (byte?)null : (cnt == 1 ? (byte?)BalancingIFDBType.LimitBudgetAllocations : (byte?)BalancingIFDBType.LimitBudgetAllocationsAndActivityOfSBP));
        }

        /// <summary>
        /// Последовательно выполнить над выделенными документами ПОБА операции «Обработать», «Утвердить»
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborBalancingIFDB.processDocs_mlLimitBudgetAllocation(id, ids)
        /// </remarks>
        /// <param name="id">ид иструмента</param>
        /// <param name="ids">ид выделеных строк в мультилинке</param>
        /// <returns></returns>
        public void processDocs_mlLimitBudgetAllocations(CommunicationContext communicationContext, int id, int[] ids)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var tool = context.ApprovalBalancingIFDB.Single(s => s.Id == id);

                tool.ProcessDocs_LimitBudgetAllocations(context, ids);
            }
        }

        /// <summary>
        /// Последовательно выполнить над выделенными документами ДВ операции «Обработать», «Утвердить»
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborBalancingIFDB.processDocs_mlActivityOfSBPs(id, ids)
        /// </remarks>
        /// <param name="communicationContext"></param>
        /// <param name="id">ид иструмента</param>
        /// <param name="ids">ид выделеных строк в мультилинке</param>
        /// <returns></returns>
        public void processDocs_mlActivityOfSBPs(CommunicationContext communicationContext, int id, int[] ids)
        {
            using (new CommunicationContextScope(communicationContext))
            {
                DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

                var tool = context.ApprovalBalancingIFDB.Single(s => s.Id == id);

                tool.ProcessDocs_ActivityOfSBP(context, ids);
            }
        }
    }
}
