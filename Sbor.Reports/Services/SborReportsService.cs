using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.AppServices;
using Platform.Common;
using Sbor.Reports.Tablepart;

namespace Sbor.Reports.Services
{
    /// <summary>
    /// Общий для проекта "SborReports" веб-сервис
    /// </summary>
    [AppService]
    class SborReportsService

    {
        #region Сервисы Отчета Консолидированные расходы

        public class tpPublicLegalFormationsLine
        {
            /// <summary>
            /// ППО
            /// </summary>
            public int idPublicLegalFormation { get; set; }

            /// <summary>
            /// Бюджет
            /// </summary>
            public int idBudget { get; set; }

            /// <summary>
            /// Версия
            /// </summary>
            public int idVersion { get; set; }
        }

        /// <summary>
        /// Получить id года текущего бюджета из иерархии периодов
        /// </summary>
        /// 
        public Dictionary<string, object> GetIdCurrentBudget(int? id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var result = new Dictionary<string, object>();
            var BudgetYear = IoC.Resolve<SysDimensionsState>("CurentDimensions").Budget.Year;

            var HP = context.HierarchyPeriod.SingleOrDefault(s => s.Year == BudgetYear && !s.IdParent.HasValue);

            result.Add("id", HP.Id);
            result.Add("caption", HP.Caption);

            return result;
        }

        /// <summary>
        /// Очищает ТЧ Публично-правовые образования отчета Консолидированные расходы
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborReportsService.clearTables_ConsolidatedExpenditure_PPO(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>

        public void clearTables_ConsolidatedExpenditure_PPO(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var delItem = context.ConsolidatedExpenditure_PPO.Where(r => r.IdOwner == id).ToList();

             foreach (var item in delItem)
            {
                context.ConsolidatedExpenditure_PPO.Remove(item);
            }
            context.SaveChanges();
        }


        /// <summary>
        /// Заполняет ТЧ Публично-правовые образования отчета Консолидированные расходы
        /// </summary>
        /// <remarks>
        /// Пример вызова: SborReportsService.FillData_tpPublicLegalFormations(int id)
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>

        public void fillData_tpPublicLegalFormations(int id)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var report = context.ConsolidatedExpenditure.SingleOrDefault(a => a.Id == id);

            var query = new StringBuilder();

            query.AppendFormat("select" +
                            Environment.NewLine + "ref.PublicLegalFormation.id  as idPublicLegalFormation," +
                            Environment.NewLine + "ref.Budget.id as idBudget," +
                            Environment.NewLine + "ref.Version.id as idVersion" +
                            Environment.NewLine + "from ref.PublicLegalFormation" +
                            Environment.NewLine + "inner join ref.Budget on ref.Budget.idPublicLegalFormation = ref.PublicLegalFormation.id" +
                            Environment.NewLine + "inner join ref.Version on ref.Version.idPublicLegalFormation = ref.PublicLegalFormation.id and ref.Version.idRefStatus = 2" +
                            Environment.NewLine + "where ref.PublicLegalFormation.id in (select id from  dbo.GetChildrens( {0} ,-1879048161,1))",
                            report.IdPublicLegalFormation);

            var result =
                context.Database.SqlQuery<tpPublicLegalFormationsLine>(query.ToString()).ToList();



            foreach (var value in result)
            {
                var newValue = new ConsolidatedExpenditure_PPO()
                {
                    IdOwner = report.Id,
                    IdPublicLegalFormation = value.idPublicLegalFormation,
                    IdBudget = value.idBudget,
                    IdVersion = value.idVersion
                };
                context.ConsolidatedExpenditure_PPO.Add(newValue);
            }

            context.SaveChanges();
        }


        #endregion

    }
}
