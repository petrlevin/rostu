using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Platform.Common.Extensions;
using Sbor.DbEnums;

namespace Sbor.Reports.PrintForms.RegisterActivity
{
    /// <summary>
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=PlanActivity&printFormClassName=PlanActivityPf&docId=25
    ///                http://localhost/platform3/Services/PrintForm.aspx?entityName=PlanActivity&printFormClassName=PlanActivityPf&docId=26
    ///                http://localhost/platform3/Services/PrintForm.aspx?entityName=PlanActivity&printFormClassName=PlanActivityPf&docId=27
    /// </summary>
    [PrintForm(Caption = "Реестр услуг по 210-ФЗ")]
    internal class RegisterActivity210fzPf : PrintFormBase
    {
        private readonly DataContext context;
        private PrimeDataSet header;
        private int idSbp;

        public RegisterActivity210fzPf(int docId)
            : base(docId)
        {
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var registerActivities = context.RegisterActivity.Where(w => w.Id == DocId).FirstOrDefault();
            header = GetHeader(registerActivities);
            idSbp = registerActivities.IdSBP ?? 0;
        }

        //Шапка
        public List<PrimeDataSet> DataSetHeader()
        {
            List<PrimeDataSet> res = new List<PrimeDataSet>();
            res.Add(header);
            return res;
        }

        private static PrimeDataSet GetHeader(Document.RegisterActivity registerActivities)
        {
            PrimeDataSet header = new PrimeDataSet()
                {
                    PPO = registerActivities.PublicLegalFormation.Caption,
                    curDate = DateTime.Now.ToShortDateString() // Дата формирования документа
                };

            if (registerActivities.PublicLegalFormation.BudgetLevel.Caption == "Субъект РФ")
            {
                header.Caption = "РЕЕСТР ГОСУДАРСТВЕННЫХ УСЛУГ";
                header.sn2 = "государственной";
                header.sn3 = "государственных";
                header.sn4 = "государственную";
                header.sn5 = "Государственные";
                header.sn10 = "государственными учреждениями";
                header.sn11 = "государственное";
                header.sn12 = "исполнительными органами государственной власти";
                header.sn13 = "исполнительных органов  государственной власти";
            }
            else
            {
                header.Caption = "РЕЕСТР МУНИЦИПАЛЬНЫХ УСЛУГ";
                header.sn2 = "муниципальной";
                header.sn3 = "муниципальных";
                header.sn4 = "муниципальную";
                header.sn5 = "Муниципальные";
                header.sn10 = "муниципальными учреждениями";
                header.sn11 = "муниципальное";
                header.sn12 = "органами местного самоуправления";
                header.sn13 = "органов местного самоуправления";
            }
            return header;
        }

        public IEnumerable<TableSet> DataSetActivity()
        {
            List<TableSet> tableSet = new List<TableSet>();

            List<Sbor.DbEnums.RegistryKeyActivity> keys = new List<RegistryKeyActivity>();
            keys.Add(RegistryKeyActivity.Key1);
            keys.Add(RegistryKeyActivity.Key3);

            var tpActivity = context.RegisterActivity_Activity.Where(r => r.IdOwner == DocId)
                                    .ToList()
                                    .Select(s => new
                                        {
                                            s,
                                            Activity = s.Activity.Caption,
                                            Contingent = s.IdContingent.HasValue ? s.Contingent.Caption : "",
                                            sn8 =
                                                     s.Activity.IdPaidType.HasValue
                                                         ? s.Activity.PaidType.Value.Caption()
                                                         : "",
                                            IndV = s.IndicatorActivity_Volume.Caption,
                                            UnV = s.IndicatorActivity_Volume.UnitDimension.Caption
                                        })
                                    .ToList();

            var ints = tpActivity.Select(s => s.s.IdActivity).ToList();
            var npas = context.Activity_RegulatoryAct
                              .Where(r => ints.Contains(r.IdOwner))
                              .Select(s => new {s, s.RegulatoryAct})
                              .ToList();

            var tpIndicatorActivity = context.RegisterActivity_IndicatorActivity.Where(r => r.IdOwner == DocId)
                                             .Select(s => new
                                                 {
                                                     s.IdMaster,
                                                     Indicator = s.DicatorActivity.Caption,
                                                     UnitDimension = s.DicatorActivity.UnitDimension.Caption
                                                 })
                                             .ToList();

            var tpPerformers = context.RegisterActivity_Performers.Where(r => r.IdOwner == DocId)
                                      .Select(s => new
                                          {
                                              s.IdMaster,
                                              Org = s.Rformers.Caption
                                          }).ToList();
            TableSet ts;

            foreach (var keyActivity in keys)
            {
                var num = 1;
                var activs = tpActivity.Where(r => r.s.IdRegistryKeyActivity.HasValue && r.s.RegistryKeyActivity == keyActivity);

                ts = new TableSet() {typeline = 0};
                if (keyActivity == RegistryKeyActivity.Key1)
                {
                    ts.Name = string.Format("I.	{0} услуги, предоставляемые {1}", header.sn5, header.sn12);
                }
                else
                {
                    ts.Name = string.Format("II.	{0} услуги, оказываемые {1} и иными организациями, " +
                                            "в которых размещается {2}  задание (заказ), " +
                                            "подлежащие включению в реестр {3}  услуг и предоставляемых в электронной форме", 
                                            header.sn5,
                                            header.sn10,
                                            header.sn11,
                                            header.sn3);
                }
                tableSet.Add(ts);

                foreach (var activity in activs)
                {
                    var fline = true;

                    var indics = tpIndicatorActivity.Where(r => r.IdMaster == activity.s.Id);
                    var perf = tpPerformers.Where(r => r.IdMaster == activity.s.Id)
                                           .Select(s => s.Org);
                    var strperf = "";
                    if (perf.Any())
                    {
                        strperf = perf.Aggregate((a, b) => a + ";/n" + b);
                    }

                    var npa7 = npas.Where(r =>
                                         r.s.IdOwner == activity.s.IdActivity && r.s.IdSBP == idSbp &&
                                         (r.s.IsBasis ?? false))
                                  .Select(s =>
                                          string.Format("{0} от {1} № {2} {3}",
                                                        s.RegulatoryAct.TypeRegulatoryAct.Caption,
                                                        s.RegulatoryAct.Date.ToShortDateString(),
                                                        s.RegulatoryAct.Number,
                                                        s.RegulatoryAct.Caption
                                              ))
                                  .ToList();

                    var strnpa7 = "";
                    if (npa7.Any())
                    {
                        strnpa7 = npa7.Aggregate((a, b) => a + ";/n" + b);
                    }

                    var npa9 = npas.Where(r =>
                                         r.s.IdOwner == activity.s.IdActivity && r.s.IdSBP == idSbp &&
                                         (r.s.IsEstablishQualityStandard ?? false))
                                  .Select(s =>
                                          string.Format("{0} от {1} № {2} {3}",
                                                        s.RegulatoryAct.TypeRegulatoryAct.Caption,
                                                        s.RegulatoryAct.Date.ToShortDateString(),
                                                        s.RegulatoryAct.Number,
                                                        s.RegulatoryAct.Caption
                                              ))
                                  .ToList();

                    var strnpa9 = "";
                    if (npa9.Any())
                    {
                        strnpa9 = npa9.Aggregate((a, b) => a + ";/n" + b);
                    }
                    
                    foreach (var indic in indics)
                    {
                        ts = new TableSet();

                        if (fline)
                        {
                            ts.typeline = 1;
                            ts.num = num.ToString();
                            ts.Name = activity.Activity;
                            ts.Cont = activity.Contingent;
                            ts.IndV = activity.IndV;
                            ts.UnV = activity.UnV;
                            ts.Sbps = strperf;
                            ts.Npa = strnpa9;
                            ts.sn8 = activity.sn8;
                            ts.sn7 = strnpa7;

                            num++;
                            fline = false;
                        }
                        else
                        {
                            ts.typeline = 2;
                        }

                        ts.IndQ = indic.Indicator;
                        ts.UnQ = indic.UnitDimension;

                        tableSet.Add(ts);
                    }
                }
            }

            ts = new TableSet() { typeline = 3 };
            tableSet.Add(ts);
            ts = new TableSet() { typeline = 4};
            tableSet.Add(ts);
            ts = new TableSet() { typeline = 5};
            tableSet.Add(ts);

            var tpActivity2 = context.RegisterActivity_Activity.Where(r => r.IdOwner == DocId)
                                    .ToList()
                                    .Select(s => new
                                        {
                                            s,
                                            Activity = s.Activity.Caption,
                                            Contingent = s.IdContingent.HasValue ? s.Contingent.Caption : "",
                                            sn8 =
                                                     s.Activity.IdPaidType.HasValue
                                                         ? s.Activity.PaidType.Value.Caption()
                                                         : "",
                                            IndV = s.IndicatorActivity_Volume.Caption,
                                            UnV = s.IndicatorActivity_Volume.UnitDimension.Caption
                                        })
                                    .ToList()
                                    .Where(r => r.s.RegistryKeyActivity == RegistryKeyActivity.Key2);

            var num2 = 1;

            foreach (var activity in tpActivity2)
            {
                var perf = tpPerformers.Where(r => r.IdMaster == activity.s.Id)
                                       .Select(s => s.Org);
                var strperf = "";
                if (perf.Any())
                {
                    strperf = perf.Aggregate((a, b) => a + ";/n" + b);
                }


                ts = new TableSet();

                ts.typeline = 6;
                ts.num = num2.ToString();
                ts.Name = activity.Activity;
                ts.Sbps = strperf;

                num2++;

                tableSet.Add(ts);

            }

            return tableSet;
        }

    }
} ;