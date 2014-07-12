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
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=StateProgram&printFormClassName=StateProgramPF&docId=-1275068397
    /// </summary>
    [PrintForm(Caption = "Государственная программа")]
    public class StateProgramPF : PrintFormBase 
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

        public StateProgramPF(int docId) : base(docId)
        {
            //context = IoC.Resolve<DbContext>().Cast<DataContext>();
        }

        


        public IEnumerable<GoalTask_sn8> SubProgram_sn1()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<GoalTask_sn8>("SELECT * FROM [sbor].[StateProgram1_2] (" + DocId.ToString() + ")");//sn_1_2

        }

        
        public IEnumerable<CoExecuter_sn1> CoExecuter_sn3()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<CoExecuter_sn1>("SELECT * FROM [sbor].[StateUnderProgram1_2] (" + DocId.ToString() + ")");//sn_3_4 

        }
        
        public IEnumerable< Program_sn3 > Program_sn5()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Program_sn3>("SELECT * FROM [sbor].[StateProgram5_6] (" + DocId.ToString() + ")");//sn_5_6_PPO_SP_TypeReponsible

        }

        public IEnumerable<Task_sn5> Task_sn7()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Task_sn5>("SELECT * FROM [sbor].[StateUnderProgram5] (" + DocId.ToString() + ",'Государственная программа'," + SPMainGoal().ToString() + ")");//sn7

        }


        public IEnumerable<Task_sn5> SubProgram_sn8()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Task_sn5>("SELECT * FROM [sbor].[StateProgram8_9] (" + DocId.ToString() + ",'Подпрограмма ГП')");//sn8

        }


        public IEnumerable<Task_sn5> SubProgram_sn9()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Task_sn5>("SELECT * FROM [sbor].[StateProgram8_9] (" + DocId.ToString() + ",'Долгосрочная целевая программа')");//sn9

        }


        public IEnumerable<Task_sn5> Task_sn10()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Task_sn5>("SELECT * FROM [sbor].[StateUnderProgram6_7] (" + DocId.ToString() + ",'Основное мероприятие')");//sn10

        }

        public IEnumerable<Task_sn5> Task_sn11()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return context.Database.SqlQuery<Task_sn5>("SELECT * FROM [sbor].[StateUnderProgram6_7] (" + DocId.ToString() + ",'Ведомственная целевая программа')");//sn11

        }


       
        public IEnumerable<GoalTask_sn8> TaskMainGoal()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return AddNull(context.Database.SqlQuery<GoalTask_sn8>("SELECT * FROM [sbor].[StateUnderProgramMainGoal] (" + DocId.ToString() + ")"));//sn12
        }

 
        public IEnumerable<GoalTask_sn8> Indicator_sn12()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return AddNull(context.Database.SqlQuery<GoalTask_sn8>("SELECT * FROM [sbor].[StateUnderProgram8] (" + DocId.ToString() + ",'Государственная программа'," + SPMainGoal() + ")"));//sn12_13_14
        }


        public IEnumerable<GoalTask_sn8> Finance_sn15()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return AddNull(context.Database.SqlQuery<GoalTask_sn8>("SELECT * FROM [sbor].[StateUnderProgram11_12] (" + DocId.ToString() + ")"));//sn15_16
        }


        public IEnumerable<GoalTask_sn8> FinSourceSubP()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return
                AddNull(
                    context.Database.SqlQuery<GoalTask_sn8>(
                        "SELECT * FROM [sbor].[StateProgram17_20] (" + DocId.ToString() + ",'Подпрограмма ГП')"));//sn_17_18_19_20
        }


        public IEnumerable<GoalTask_sn8> FinSourceLongGP()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return
                AddNull(
                    context.Database.SqlQuery<GoalTask_sn8>(
                        "SELECT * FROM [sbor].[StateProgram17_20] (" + DocId.ToString() + ",'Долгосрочная целевая программа')"));//sn_17_18_19_20
        }


        public IEnumerable<GoalTask_sn8> FinSourceOsn()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return
                AddNull(
                    context.Database.SqlQuery<GoalTask_sn8>(
                        "SELECT * FROM [sbor].[StateUnderProgram13_16] (" + DocId.ToString() + ",'Основное мероприятие')"));//sn_21_22_23_24
        }

        public IEnumerable<GoalTask_sn8> FinSourceVed()
        {
            DataContext context;
            context = IoC.Resolve<DbContext>().Cast<DataContext>();
            return
                AddNull(
                    context.Database.SqlQuery<GoalTask_sn8>(
                        "SELECT * FROM [sbor].[StateUnderProgram13_16] (" + DocId.ToString() + ",'Ведомственная целевая программа')"));//sn_21_22_23_24
        }
    }
}
