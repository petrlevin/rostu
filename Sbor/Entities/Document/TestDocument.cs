using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Sbor.CommonControls;
using Sbor.Registry;
using Sbor.Tablepart;

namespace Sbor.Document
{
	/// <summary>
	/// TestDocument
	/// </summary>
	public partial class TestDocument : ITestDocument
	{

	    /// <summary>
	    /// Идентификатор
	    /// </summary>

	    public void Process(DataContext context)
	    {
	        this.Number = this.Number + "^";
	        context.TestReg1.Add(new TestReg1() {IdRegistrator = Id , Text = "Process1"});
            context.TestReg2.Add(new TestReg2() { IdRegistrator = Id ,IdRegistratorEntity = EntityId});
            ExecuteControl(e=>e.ControlNumber1());
	        ExecuteOperation(e => e.Change(context));
	        //ExecuteControl(e => e.ControlNumber2());
	        /*
            ExecuteControl(e => e.ControlNumber3());
             */

	    }

        public void Terminate(DataContext context)
        {
            context.TestReg1.Where(tr=>tr.IdRegistrator == Id).ToList().ForEach(tr =>
                                                                                    {
                                                                                        tr.IdTerminatorEntity =
                                                                                            EntityId;
                                                                                        tr.IdTerminator = Id;
                                                                                    }
                );
        }

        public void Change(DataContext context)
        {
            //ExecuteControl<TestFreeCommonControl>();
            context.TestReg1.Add(new TestReg1() { IdRegistrator = Id , Text = "Change1" });
            ExecuteOperation(e => e.Confirm(context));
            context.SaveChanges();
            context.TestReg1.Add(new TestReg1() { IdRegistrator = Id, Text = "Change2" });
            ExecuteOperation(e => e.Confirm(context));
            context.TestReg1.Add(new TestReg1() { IdRegistrator = Id, Text = "Change3" });
        }

        public void Confirm(DataContext context)
        {
            context.TestReg1.Add(new TestReg1() { IdRegistrator = Id, Text = "Confirm1" });
            context.TestReg1.Add(new TestReg1() { IdRegistrator = Id, Text = "Confirm2" });
            context.TestDocument.Where(d=>d.Number.Contains(Number)&& d.Id!=Id).ToList().ForEach(d=>d.ExecuteOperation(dd=>dd.Terminate(context)));
        }

        

        public void Create(DataContext context)
        {
            Number = "#" + Number;
        }

        [ControlInitial(InitialUNK = "67890qwerty" ,InitialSkippable = true)]
        public void ControlNumber1()
        {
            Controls.Throw("ControlNumber1");
         //   throw new DbUpdateException("ControlNumber1", new SqlException());
        }

        public void ControlNumber2()
        {

        }

        public void ControlNumber3()
        {

        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1500)]
        public void C11ontrolPeriod(DataContext context, ControlType ctType)
        {

        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1500)]
        public void C12ontrolPeriod(DataContext context, ControlType ctType)
        {

        }



	    [Control(ControlType.Update, Sequence.After, ExecutionOrder = 287)]
	    public void AQWEControlOfTestDocument(DataContext dataContext)
	    {
	        
	    }

	    [Control(ControlType.Update, Sequence.After, ExecutionOrder = 2)]
        public void  UpdateControlOfTestDocument(DataContext dataContext)
        {
            using (new ControlScope())
            {
                foreach (var testDocumentTp in this.SomeTablePart)
                {
                    testDocumentTp.Value = "!";
                }
                dataContext.SaveChanges();
            }

            using (new ControlScope(new SpecificControlLauncher.Settings(TestDocumentTP2.EntityIdStatic)))
            {
                foreach (var testDocumentTp in this.SomeTablePart)
                {
                    testDocumentTp.Value = testDocumentTp.Value + "!";
                }
                foreach (var tp in this.OtherTablePart )
                {
                    if (tp.IdValue == dataContext.TestObject.ToList().Last().Id)
                        tp.Value = dataContext.TestObject.First();
                    else if (tp.IdValue == dataContext.TestObject.First().Id)
                        tp.Value = dataContext.TestObject.ToList().Last();

                }
 
                dataContext.SaveChanges();
            }

            Controls.Throw("Вы не заплатили за воду");

            Zumma ++;



        }
	}
}