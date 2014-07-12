using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;

namespace Sbor.Reports.PrintForms.StateProgram
{
    /// <summary>
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=StateProgram&printFormClassName=StateUnderProgramPF&docId=-1275068389
    /// </summary>
    [PrintForm(Caption ="Подпрограмма государственной программы")]
    public class StateUnderProgramPF : PrintFormBase 
    {
        
        private string SPMainGoal()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var doc = context.StateProgram_SystemGoalElement.SingleOrDefault(s => s.IdOwner == DocId
                                                                      && s.IsMainGoal == true);
            if(doc == null)
                return "NULL";
// ReSharper disable RedundantIfElseBlock
            else
// ReSharper restore RedundantIfElseBlock
                return doc.Id.ToString();
        }

        private List<int> SPDSDE()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            var docp = context.StateProgram.SingleOrDefault(p => p.Id == DocId);
            List<int> LN = new List<int>();
            LN.Add(docp.DateStart.Year);
            LN.Add(docp.DateEnd.Year);
            return LN;
        }

        private IEnumerable<GoalTask_sn8> AddNull(IEnumerable<GoalTask_sn8> SG)
        {
                DataContext context;
                context = IoC.Resolve<DbContext>().Cast<DataContext>();
                var docp = context.StateProgram.SingleOrDefault(p => p.Id == DocId);

                List<GoalTask_sn8> GT = SG.ToList();
                List<GoalTask_sn8> GTR = new List<GoalTask_sn8>();
                if (GT.Count() == 0)
                    return SG;
                foreach (GoalTask_sn8 gtsk in GT)
                {
                    if (gtsk.NumYear == null)
                    {
                        gtsk.NumYear = docp.DateStart.Year;
                    }
                    GTR.Add(gtsk);
                }
                for (int i = docp.DateStart.Year; i <= docp.DateEnd.Year; i++)
                {
                    GoalTask_sn8 tsn = new GoalTask_sn8();
                    tsn.NumberName = GT[0].NumberName; //goal;
                    tsn.GoalIndicator = GT[0].GoalIndicator; //indicator;
                    tsn.NumYear = i;
                    GTR.Add(tsn);
                }
                return (IEnumerable<GoalTask_sn8>) GTR;
        }


        //private DataContext context;

        public StateUnderProgramPF(int docId) : base(docId)
        {
            //context = IoC.Resolve<DbContext>().Cast<DataContext>();
        }

        public IEnumerable< CoExecuter_sn1> CoExecuter_sn1()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<CoExecuter_sn1>("SELECT * FROM [sbor].[StateUnderProgram1_2] (" + DocId.ToString() + ")");

        }


        
        public IEnumerable< Program_sn3 > Program_sn3()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Program_sn3>("SELECT * FROM [sbor].[StateUnderProgram3_4] (" + DocId.ToString() + ")");

        }

        public IEnumerable<Task_sn5> Task_sn5()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Task_sn5>("SELECT * FROM [sbor].[StateUnderProgram5] (" + DocId.ToString() + ",'Подпрограмма ГП'," + SPMainGoal().ToString() + ")");

        }

        public IEnumerable<Task_sn5> Task_sn6()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Task_sn5>("SELECT * FROM [sbor].[StateUnderProgram6_7] (" + DocId.ToString() + ",'Основное мероприятие')");

        }

        public IEnumerable<Task_sn5> Task_sn7()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Task_sn5>("SELECT * FROM [sbor].[StateUnderProgram6_7] (" + DocId.ToString() + ",'Ведомственная целевая программа')");

        }


       
        public IEnumerable<GoalTask_sn8> TaskMainGoal()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return AddNull(context.Database.SqlQuery<GoalTask_sn8>("SELECT * FROM [sbor].[StateUnderProgramMainGoal] (" + DocId.ToString() + ")"));
            
            //return context.Database.SqlQuery<GoalTask_sn8>("SELECT * FROM [platform3].[dbo].[StateUnderProgram6_7] (" + DocId.ToString() + ",'Ведомственная целевая программа')");
        }

 
        public IEnumerable<GoalTask_sn8> Indicator_sn8()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return AddNull(context.Database.SqlQuery<GoalTask_sn8>("SELECT * FROM [sbor].[StateUnderProgram8] (" + DocId.ToString() + ",'Подпрограмма ГП'," + SPMainGoal() + ")"));
        }


        public IEnumerable<GoalTask_sn8> Finance_sn11()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return AddNull(context.Database.SqlQuery<GoalTask_sn8>("SELECT * FROM [sbor].[StateUnderProgram11_12] (" + DocId.ToString() + ")"));
        }

        public IEnumerable<GoalTask_sn8> FinSourceOsn_sn13()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return
                AddNull(
                    context.Database.SqlQuery<GoalTask_sn8>(
                        "SELECT * FROM [sbor].[StateUnderProgram13_16] (" + DocId.ToString() + ",'Основное мероприятие')"));
        }

        public IEnumerable<GoalTask_sn8> FinSourceVed_sn13()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return
                AddNull(
                    context.Database.SqlQuery<GoalTask_sn8>(
                        "SELECT * FROM [sbor].[StateUnderProgram13_16] (" + DocId.ToString() + ",'Ведомственная целевая программа')"));
        }

        #region trial
        /*
        public List<SPDataSet> SPDataSet()
        {
            return new List<SPDataSet>()
                       {
                           new SPDataSet()
                               {
                                   StateProgram = "Развитие образования",
                                   StateUndProgram = "Общее и дополнительное образование",
                                   TypeCoExecuter = "Соисполнители подпрограммы",
                                   CoExecuter = "Министерство культуры",
                                   Executer = "Министерство образования",
                                   Period = "2015-2020",
                                   MainGoal = "Предоставление качественного общего и дополнительного образования"
                               }
                       };


        }
        */
        #endregion

    }
}
