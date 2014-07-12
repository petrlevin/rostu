using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using System.Globalization;
using Sbor.Reports.Reports.ListProgram;


namespace Sbor.Reports.Reports.ListProgram
{
    [Report]
    public class ListProgram
    {
        public int idBudget { get; set; } //Бюджет
        public int idVersion { get; set; } //Версия
        public bool OutputExecutor { get; set; } //Выводить исполнителей
        public bool OutputCodeProgram { get; set; } //Выводить код программы
        public DateTime DateReport { get; set; } //Дата отчета
        public int idSourcesDataReports { get; set; } //Источник данных для  ресурсного обеспечения
        public string Caption { get; set; } //Наименование
        public bool AdditionalFundingRequirements { get; set; } //Оценка дополнительной потребности в средствах
        public bool RepeatTableHeader { get; set; } //Повторять заголовки  таблиц на каждой странице
        public int idPublicLegalFormation { get; set; } //ППО
        public string idProgram { get; set; } //Государственная программа
        public bool buildReportApprovedData { get; set; } //Строить отчет по утвержденным данным
        public int idListTypeOutputProgram { get; set; } //Тип выводимых программ
        public bool ShapeOnlyBudgetPeriod { get; set; }//Формировать только на период бюджета



        public IEnumerable<DataSetListProgram> ListAllProgram() //данные из регистра
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            context = IoC.Resolve<DbContext>().Cast<DataContext>();

            //основная цель

            string budgetyear;

            if (ShapeOnlyBudgetPeriod)
            {
                budgetyear = context.Budget.Where(s => s.Id == idBudget)
                                    .Select(d => d.Year)
                                    .FirstOrDefault().ToString();
            }
            else
            {
                budgetyear = "NULL";
            }


            var ProgramType = "";
            var query = "";
            var AdditionalNeedQuery = "";

            if (string.IsNullOrEmpty(idProgram))
                idProgram = "NULL";


            switch (AdditionalFundingRequirements)
            {
                case true:
                    AdditionalNeedQuery = ",NULL,0,";
                    break;
                case false:
                    AdditionalNeedQuery = ",0,NULL,";
                    break;
            }

            switch (idListTypeOutputProgram)
            {
                case 1:
                    ProgramType = "-1543503843, NULL, NULL, NULL";
                    break;
                case 2:
                    ProgramType = "-1543503842, NULL, NULL, NULL";
                    break;
                case 3:
                    ProgramType = "-1543503841, NULL, NULL, NULL";
                    break;
                case 4:
                    ProgramType = "-1543503839, NULL, NULL, NULL";
                    break;
                case 5:
                    ProgramType = "-1543503841, -1543503839, NULL, NULL";
                    break;
                case 6:
                    ProgramType = "-1543503843, -1543503842, NULL, NULL";
                    break;
                case 7:
                    ProgramType = "-1543503843, -1543503842, -1543503841, -1543503839";
                    break;
            }

            //[sbor].[ListTargetedProgramsByType]  NULL  ,NULL  ,NULL  ,4  ,NULL  ,2014  ,1  ,1  ,-1543503842  ,-1543503841  ,NULL  ,NULL
            //-1543503843	Государственная программа
            //-1543503842	Подпрограмма ГП
            //-1543503841	Ведомственная целевая программа
            //-1543503839   Основное мероприятие
            if (buildReportApprovedData == false)
            {
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "EXECUTE  [sbor].[ListTargetedProgramsByType]  " + idProgram +
                                AdditionalNeedQuery + "NULL  ,NULL  ,NULL  ,4  ,NULL  , " + budgetyear +
                                "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ", " + ProgramType;
                        break;
                    case 2:
                        query = "EXECUTE  [sbor].[ListTargetedProgramsByTypeActivityOfSBP]  " + idProgram +
                                AdditionalNeedQuery + "NULL  ,NULL  ,NULL  ,9  ,NULL  , " + budgetyear +
                                "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ", " + ProgramType;
                        break;
                    case 3:
                        query = "EXECUTE  [sbor].[ListTargetedProgramsByType]  " + idProgram +
                                AdditionalNeedQuery + "NULL  ,NULL  ,NULL  ,4  ,10  , " + budgetyear +
                                "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ", " + ProgramType;
                        break;
                    case 4:
                        query = string.Format("EXECUTE  [sbor].[ListTargetedProgramsByTypeActivityOfSBP]  {0}{1}NULL  ,NULL  ,NULL  ,9  ,11  , {2},{3},{4}, {5}", idProgram, AdditionalNeedQuery, budgetyear, idVersion.ToString(), idPublicLegalFormation.ToString(), ProgramType);
                        break;
                }
                var res = context.Database.SqlQuery<DataSetListProgram>(query);
                if (OutputExecutor == false)
                    res = SummarizeProgram(res.ToList());

                res = AddYear(res.ToList(), context);

                return res;
            }
            else
            {
                switch (idSourcesDataReports)
                {
                    case 0:
                        query = "EXECUTE  [sbor].[ListTargetedProgramsByType_ApprovedData]  " + idProgram +
                                AdditionalNeedQuery + "NULL  ,NULL  ,NULL  ,4  ,NULL  , " + budgetyear +
                                "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ", " + ProgramType + ", '" + DateReport.ToString() + "'";
                        break;
                    case 2:
                        query = "EXECUTE  [sbor].[ListTargetedProgramsByTypeActivityOfSBP_ApprovedData]  " + idProgram +
                                AdditionalNeedQuery + "NULL  ,NULL  ,NULL  ,9  ,NULL  , " + budgetyear +
                                "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ", " + ProgramType + ", '" + DateReport.ToString() + "'";
                        break;
                    case 3:
                        query = "EXECUTE  [sbor].[ListTargetedProgramsByType_ApprovedData]  " + idProgram +
                                AdditionalNeedQuery + "NULL  ,NULL  ,NULL  ,4  ,10  , " + budgetyear +
                                "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ", " + ProgramType + ", '" + DateReport.ToString() + "'";
                        break;
                    case 4:
                        query = "EXECUTE  [sbor].[ListTargetedProgramsByTypeActivityOfSBP_ApprovedData]  " + idProgram +
                                AdditionalNeedQuery + "NULL  ,NULL  ,NULL  ,9  ,11  , " + budgetyear +
                                "," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ", " + ProgramType + ", '" + DateReport.ToString() + "'";
                        break;
                }
                var res = context.Database.SqlQuery<DataSetListProgram>(query);
                if (OutputExecutor == false)
                    res = SummarizeProgram(res.ToList());

                res = AddYear(res.ToList(), context);
                return res;
            }



            //return null;
            //switch (buildReportApprovedData)
            //{
            //    case true:
            //        var res = context.Database.SqlQuery<DataSetVOSP>("SELECT * FROM [sbor].[VCPOMProg1_23_ApprovedData] (" + idProgram.ToString() + ",'" + DateReport.ToString() + "'," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ")").ToList();
            //        res.ForEach(f => f.Goalindicator = f.Goalindicator + " - " + (f.Value.HasValue ? f.Value.Value.ToString("0.#####") : ""));
            //        return res;
            //    default:
            //        var res1 = context.Database.SqlQuery<DataSetVOSP>("SELECT * FROM [sbor].[VCPOMProg1_23] (" + idProgram.ToString() + ",'" + DateReport.ToString() + "'," + idVersion.ToString() + "," + idPublicLegalFormation.ToString() + ")").ToList();
            //        res1.ForEach(f => f.Goalindicator = f.Goalindicator + (f.Value.HasValue ? " - " + f.Value.Value.ToString("0.#####") : ""));
            //        return res1;//return context.Database.SqlQuery<DataSetVOSP>("SELECT * FROM [sbor].[VCPOMProg1_23] (" + idProgram.ToString() + ",'" + DateReport.ToString() + "')");
            //}

        }


        private List<DataSetListProgram> AddYear(List<DataSetListProgram> summation, DataContext context)
        {
            //заполняем всеми годами и убираем пустые столбцы
            //if (summation.Any())
            //{
            int? minDate = summation.Select(s => s.Year).Min();
            int? maxDate = summation.Select(s => s.Year).Max();
            DateTime? minDateStarting = summation.Select(s => s.Datestart).Min();
            DateTime? maxDateEnding = summation.Select(s => s.Dateend).Max();
            //if (minDate == null)
            //{
            //    foreach (var t in summation)
            //        if (t.Year == null)
            //            t.Year = 2014;
            //    //return summation;
            //}
            if (minDate == null)
            {
                minDate = minDateStarting.Value.Year;
                maxDate = minDateStarting.Value.Year;
            }
            else
            {
                if (minDateStarting != null && minDate >= minDateStarting.Value.Year)
                    minDate = minDateStarting.Value.Year;
            }

            //основная цель



            if (maxDateEnding != null && maxDate <= maxDateEnding.Value.Year)
                maxDate = maxDateEnding.Value.Year;

            if (ShapeOnlyBudgetPeriod)
            {
                int? budgetyear = context.Budget.Where(s => s.Id == idBudget).Select(d => d.Year).FirstOrDefault();
                minDate = budgetyear;
                maxDate = budgetyear + 2;
            }

            //pds = summation[0];
            for (int i = minDate.Value; i <= maxDate.Value; i++)
            {
                var pds = new DataSetListProgram
                {
                    CaptionProg = summation[0].CaptionProg,
                    AnalyticalCode = summation[0].AnalyticalCode,
                    Level = summation[0].Level,
                    CaptionSBP = summation[0].CaptionSBP
                };
                pds.Year = i;
                summation.Add(pds);
            }

            foreach (var t in summation)
                if (t.Year == null)
                    t.Year = minDate.Value;
            return summation;
            //}
        }

        private List<DataSetListProgram> SummarizeProgram(List<DataSetListProgram> summation)
        {
            var result = new List<DataSetListProgram>();
            if (summation.Any())
            {
                result = summation.GroupBy(x => new { x.IdProgram, x.CaptionProg, x.sort, x.Level, x.AnalyticalCode, x.Year })
                         .Select(x => new
                                          DataSetListProgram
                         {
                             IdProgram = x.Key.IdProgram,
                             CaptionProg = x.Key.CaptionProg,
                             sort = x.Key.sort,
                             Level = x.Key.Level,
                             AnalyticalCode = x.Key.AnalyticalCode,
                             Year = x.Key.Year,
                             AmountOfCash =
                                 x.Sum(u => u.AmountOfCash)
                         }).ToList();

            }

            return result;
        }
    }
}
