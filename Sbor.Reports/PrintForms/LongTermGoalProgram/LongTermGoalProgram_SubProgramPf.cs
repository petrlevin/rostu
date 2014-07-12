using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects.SqlClient;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Platform.Utils.Extensions;

namespace Sbor.Reports.PrintForms.LongTermGoalProgram
{
    /// <summary>
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=LongTermGoalProgram&printFormClassName=LongTermGoalProgram_SubProgramPf&docId=-1744830435
    /// </summary>
    [PrintForm(Caption = "Подпрограмма долгосрочной целевой программы")]
    public class LongTermGoalProgram_SubProgramPf : PrintFormBase
    {
        private DataContext context;
        private readonly int dateStart;
        private readonly int dateEnd;

        public LongTermGoalProgram_SubProgramPf(int docId)
            : base(docId)
        {
            context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var yearRange = context.LongTermGoalProgram.Where(w => w.Id == DocId)
                            .Select(s => new { DateStart = s.DateStart.Year, DateEnd = s.DateEnd.Year }).FirstOrDefault();
            if (yearRange != null)
            {
                dateStart = yearRange.DateStart;
                dateEnd = yearRange.DateEnd;
            }
        }

        // Шапка
        public IEnumerable<DataSetHeader> DataSetHeader()
        {
            IEnumerable<DataSetHeader> caption
                = context.LongTermGoalProgram.Where(w => w.Id == DocId)
                         .Select(s =>
                                 new DataSetHeader
                                     {
                                         Id = s.Id,
                                         PublicLegalFormation = s.PublicLegalFormation.Caption, //"Долгосрочная целевая программа.ППО"
                                         Caption = s.Caption,                                   //"Наименование подпрограммы долгосрочной целевой программы",
                                         StateProgram = s.MasterLongTermGoalProgram != null ? s.MasterLongTermGoalProgram.Caption : null, //"Наименование долгосрочной целевой программы
                                         ResponsibleExecutantType = s.ResponsibleExecutantType.Caption, //"Тип ответственного исполнителя"
                                         ResponsibleExecutant = s.SBP.Organization.Caption, //"Ответственный исполнитель подпрограммы ДЦП"
                                         ImplementationPeriod = s.DateStart.Year==s.DateEnd.Year ? 
                                            SqlFunctions.StringConvert( (double) s.DateStart.Year).Trim() + "год" :
                                            SqlFunctions.StringConvert( (double) s.DateStart.Year).Trim() + "-" + SqlFunctions.StringConvert( (decimal) s.DateEnd.Year).Trim() + " годы",
                                         SystemGoal = s.SystemGoalElement.Where(sgW => sgW.IsMainGoal == true).Select(sgS => sgS.SystemGoal.Caption).FirstOrDefault(), //"Цель программы"
                                         CurrentDate = DateTime.Now // Дата формирования документа
                                     }
                    );
            return caption;
        }

        // Шапка - Тип соисполнителей (сноска 2) и соисполнители (сноска 3)
        public IEnumerable<DataSetAccomplice> DataSetAccomplice()
        {
            IEnumerable<DataSetAccomplice> accomplice;
            var longTermGoalProgram = context.LongTermGoalProgram.FirstOrDefault(w => w.Id == DocId);
            if (longTermGoalProgram != null)
            {
                //Из «Соисполнители и ресурсное обеспечение»
                accomplice = longTermGoalProgram.SBPs.Select(s => new DataSetAccomplice()
                    {
                        Type = s.ResponsibleExecutantType.Caption,
                        Name = s.SBP.Caption
                    });
            }
            else accomplice = new List<DataSetAccomplice>();
            return accomplice;
        }

        // Шапка - Задачи программы (сноска 3)
        public IEnumerable<DataSetString> Tasks()
        {
            IEnumerable<DataSetHierarchy> tasks =
                context.LongTermGoalProgram_SystemGoalElement
                       .Where(w => w.IdOwner == DocId
                                   && w.IsMainGoal == false
                                   && w.SystemGoal.DocType_CommitDoc.Caption == "Подпрограмма ДЦП")
                       .Select(s =>
                               new DataSetHierarchy
                                   {
                                       Id = s.SystemGoal.Id,
                                       IdParent = s.SystemGoal.IdParent,
                                       Value = s.SystemGoal.Caption
                                   }).ToList();
            
            tasks.NumerateHierarchy(d => d.Id, p => p.IdParent, ord => ord.Value, (tsk, hier) => tsk.HierarchyNumber = hier); //tasks.NumerateHierarchy();


            return tasks.OrderBy(o => o.HierarchyNumber).Select(s =>
                  new DataSetString {Value =(new String(' ', (s.HierarchyNumber.Length - 2)*2) + s.HierarchyNumber + " " +s.Value)});
        }

        //Целевые показатели
        public IEnumerable<DataSetGoalIndicator> GoalIndicators()
        {
            var goalindicator = (
                        from sge in
                            context.LongTermGoalProgram_SystemGoalElement.Where(w => w.IdOwner == DocId &&
                                                                                        (w.IsMainGoal == true ||
                                                                                        w.SystemGoal.DocType_CommitDoc.Caption =="Подпрограмма ДЦП"))
                        join jgi in context.LongTermGoalProgram_GoalIndicator.Where(w => w.IdOwner == DocId)
                            on sge.Id equals jgi.IdMaster into tmpgi
                        from gi in tmpgi.DefaultIfEmpty()
                        join jgiv in
                            context.LongTermGoalProgram_GoalIndicator_Value.Where(w => w.IdOwner == DocId)
                            on gi.Id equals jgiv.IdMaster into tmpgiv
                        from giv in tmpgiv.DefaultIfEmpty()
                        orderby sge.IsMainGoal descending
                        select new DataSetGoalIndicator()
                            {
                                Id = sge.Id,
                                IdParent = sge.IdParent,
                                IsMainGoal = sge.IsMainGoal,
                                Goal = sge.SystemGoal.Caption, //Наименование цели/задачи
                                GoalIndicator = gi == null ? null : gi.GoalIndicator.Caption,
                                //Наименование целевого показателя
                                UnitDimension = gi == null ? null : gi.GoalIndicator.UnitDimension.Caption,
                                //Ед.измерения
                                Year = giv == null ? dateStart : giv.HierarchyPeriod.DateStart.Year,
                                //Год <Сноска 9> 
                                Value = giv == null ? (decimal?) null : giv.Value
                                //Значения целевых показателей <Сноска 10>
                            }).ToList();

            if (goalindicator.Any())
            {
                #region Дополнение данных отсутсвующими годами
                //var firstGoalindicator = goalindicator.First();
                //var existingYears = goalindicator.Select(s => s.Year);
                //for (int y = dateStart; y <= dateEnd; y++)
                //{
                //    if (!existingYears.Contains(y))
                //    {
                //        goalindicator.Add(firstGoalindicator.Clone(y, null));
                //    }
                //}

                goalindicator.AddMissingInRange(dateStart, dateEnd, s => s.Year, (obj, y) => (obj.Clone(y, null)));
                #endregion

                #region Нумерация цели/задачи
                var goal = goalindicator.Where(w => w.IsMainGoal == true).ToList();
                var task = goalindicator.Where(w => w.IsMainGoal == false).ToList();

                foreach (var g in goal)
                {
                    g.Goal = "<b>Цель.</b> " + g.Goal;
                    g.HierarchyNumber = "0";
                }

                if (task.Any())
                {
                    task.NumerateHierarchy(d => d.Id, p => p.IdParent, ord => ord.Goal, (tsk, hier) => tsk.HierarchyNumber = hier, "Задача ");
                    goalindicator = goal.Union(task).ToList();    
                }
                
                goalindicator.ForEach(f => f.Goal = (f.HierarchyNumber == "0" ? "" : "<b>" + f.HierarchyNumber + "</b> ") + f.Goal);
                //goalindicator = goalindicator.OrderBy(o => o.HierarchyNumber).ToList();
                #endregion
            }

            return goalindicator;
        }

        //Ресурсное обеспечение
        public IEnumerable<DataSetResourceMaintenance> ResourceMaintenance()
        {
            List<DataSetResourceMaintenance> rm = (
                     from r in context.LongTermGoalProgram_ResourceMaintenance.Where(w => w.IdOwner == DocId)
                     join jrv in context.LongTermGoalProgram_ResourceMaintenance_Value on r.Id equals jrv.IdMaster into tmprv
                     from rv in tmprv.DefaultIfEmpty()
                     select new DataSetResourceMaintenance()
                         {
                             DocType = "",
                             FinanceSource = r.IdFinanceSource == null ? null : r.FinanceSource.Caption,
                             SubProgram = "",
                             Value = rv.Value/1000,
                             Year = rv == null ? dateStart : rv.HierarchyPeriod.DateStart.Year
                         }).ToList();

            if (rm.Count() == 0){
                rm.Add(new DataSetResourceMaintenance() {DocType = "",FinanceSource = "",SubProgram = "",Value = null,Year = dateStart});
            }

            //Дополнение данных отсутсвующими годами
            rm.AddMissingInRange(dateStart, dateEnd, s => s.Year, (obj, y) => (obj.Clone(y, null)));
            return rm;
        }

        //Система мероприятий долгосрочной целевой программы
        public IEnumerable<DataSetActivity> Activity()
        {
            //Объем финансирования
            var actFinanceVolume =
                       from a in context.LongTermGoalProgram_Activity.Where(w => w.IdOwner == DocId &&
                                                                               (w.Master.IsMainGoal == true || w.Master.SystemGoal.DocType_CommitDoc.Entity.Caption == "Долгосрочная целевая программа"))
                       join jrm in context.LongTermGoalProgram_ActivityResourceMaintenance on a.Id equals jrm.IdMaster into tmprm
                       from rm in tmprm.DefaultIfEmpty()
                       join jrmv in context.LongTermGoalProgram_ActivityResourceMaintenance_Value on rm.Id equals jrmv.IdMaster into tmprmv
                       from rmv in tmprmv.DefaultIfEmpty()
                       select new DataSetActivity()
                           {
                               Id = a.Master.Id,
                               IdParent = a.Master.IdParent,
                               HierarchyNumber = "",

                               IsMainGoal = a.Master.IsMainGoal,
                               GoalNumber = "",
                               Goal = a.Master.SystemGoal.Caption,
                               ActivityId = a.Id,
                               ActivityNumber = "",
                               ActivityName = a.Activity.Caption,     //Наименование мероприятия <18>
                               Executor = a.SBP.Caption,          //Наименование исполнителя <19>
                               //==========
                               FeatureName = "Объем финансирования",
                               FinanceSource = rm == null ? null : rm.FinanceSource.Caption,   //Источник финансирования <20>
                               UnitDimension = "тыс.руб",              //Единица измерения
                               Year = rmv == null ? dateStart : rmv.HierarchyPeriod.DateStart.Year,
                               Value = rmv == null ? (decimal?)null : rmv.Value/1000
                           };

            //Показатель объема
            var actVolumeIndicator =
                       from a in context.LongTermGoalProgram_Activity.Where(w => w.IdOwner == DocId &&
                                                                               (w.Master.IsMainGoal == true || w.Master.SystemGoal.DocType_CommitDoc.Entity.Caption == "Долгосрочная целевая программа"))
                       join jav in context.LongTermGoalProgram_Activity_Value on a.Id equals jav.IdMaster into tmpav
                       from av in tmpav.DefaultIfEmpty()
                       select new DataSetActivity()
                       {
                           Id = a.Master.Id,
                           IdParent = a.Master.IdParent,
                           HierarchyNumber = "",

                           IsMainGoal = a.Master.IsMainGoal,
                           GoalNumber = "",
                           Goal = a.Master.SystemGoal.Caption,
                           ActivityId = a.Id,
                           ActivityNumber = "",
                           ActivityName = a.Activity.Caption,     //Наименование мероприятия <18>
                           Executor = a.SBP.Caption,          //Наименование исполнителя <19>
                           //==========
                           FeatureName = "Показатель объема",
                           FinanceSource = a.IndicatorActivity_Volume.Caption,                  //Наименование показателя объема <22>
                           UnitDimension = a.IndicatorActivity_Volume.UnitDimension.Caption,    //Единица измерения <23>
                           Year = av == null ? dateStart : av.HierarchyPeriod.DateStart.Year,
                           Value = av == null ? (decimal?)null : av.Value
                       };
            //Показатель качества
            var actQualityIndicator =

                /* т.к. Показатель качества не отображать если не заполнен, замена left join на inner join
                       from a in context.LongTermGoalProgram_Activity.Where(w => w.IdOwner == DocId &&
                                                                               (w.Master.IsMainGoal == true || w.Master.SystemGoal.DocType_CommitDoc.Entity.Caption == "Долгосрочная целевая программа"))
                       join jia in context.LongTermGoalProgram_IndicatorActivity on a.Id equals jia.IdMaster into tmpia
                       from ia in tmpia.DefaultIfEmpty()
                       join jiav in context.LongTermGoalProgram_IndicatorActivity_Value on ia.Id equals jiav.IdMaster into tmpiav
                       from iav in tmpiav.DefaultIfEmpty()
                 */
                       
                       from a in context.LongTermGoalProgram_Activity.Where(w => w.IdOwner == DocId &&
                                                                               (w.Master.IsMainGoal == true || w.Master.SystemGoal.DocType_CommitDoc.Entity.Caption == "Долгосрочная целевая программа"))
                       join ia in context.LongTermGoalProgram_IndicatorActivity on a.Id equals ia.IdMaster
                       join iav in context.LongTermGoalProgram_IndicatorActivity_Value on ia.Id equals iav.IdMaster

                       select new DataSetActivity()
                       {
                           Id = a.Master.Id,
                           IdParent = a.Master.IdParent,
                           HierarchyNumber = "",

                           IsMainGoal = a.Master.IsMainGoal,
                           GoalNumber = "",
                           Goal = a.Master.SystemGoal.Caption,
                           ActivityId = a.Id,
                           ActivityNumber = "",
                           ActivityName = a.Activity.Caption,     //Наименование мероприятия <18>
                           Executor = a.SBP.Caption,          //Наименование исполнителя <19>
                           //==========
                           FeatureName = "Показатель качества",
                           FinanceSource = ia.IndicatorActivity.Caption, //ia == null ? null : ia,   //Источник финансирования <25>
                           UnitDimension = ia.IndicatorActivity.UnitDimension.Caption,              //Единица измерения <26>
                           Year = iav.HierarchyPeriod.DateStart.Year,
                           Value = iav.Value
                       };
            List<DataSetActivity> act = new List<DataSetActivity>(actFinanceVolume).Concat(actVolumeIndicator).Concat(actQualityIndicator).ToList();
            
            #region Иерархическая нумерация цели/задачи
            var goal = act.Where(w => w.IsMainGoal == true).ToList();
            var task = act.Where(w => w.IsMainGoal == false).ToList();

            goal.ForEach(f => { f.Goal = "<b>Цель.</b> " + f.Goal;
                                f.HierarchyNumber = "0";});

            task.NumerateHierarchy(d => d.Id, p => p.IdParent, ord => ord.Goal, (tsk, hier) => tsk.HierarchyNumber = hier, "Задача "); //task.NumerateHierarchy("Задача ");
            var ret = goal.Union(task).ToList();
            ret.ForEach(f => f.Goal = (f.HierarchyNumber == "0" ? "" : "<b>" + f.HierarchyNumber + "</b> ") + f.Goal);
            #endregion

            // Нумерация внутри группы Id уникальных групп ActivityId
            ret.NumerateInternalGroups(i => i.Id, b => b.ActivityId ?? 0,
                (u, i) => u.ActivityNumber = (u.HierarchyNumber == "0" ? "" : u.HierarchyNumber + ".") + i.ToString());

            if (!ret.Any())
            {
                ret.Add(new DataSetActivity()
                    {
                        Id = -1,
                        IdParent = null,
                        HierarchyNumber = "",
                        IsMainGoal = true,
                        GoalNumber = "",
                        Goal = "",
                        ActivityId = null,
                        ActivityNumber = "",
                        ActivityName = "",
                        Executor = "",
                        FeatureName = "",
                        FinanceSource = "", 
                        UnitDimension = "", 
                        Year = dateStart,
                        Value = null
                    }
                );
            }

            //Добавление отсутствующих годов
            ret.AddMissingInRange(dateStart, dateEnd, s => s.Year, (obj, y) => (obj.Clone(y, null)));

            return ret;
        }
    }
}
