using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.Utils.Common;
using Sbor.CommonControls;


namespace Sbor.Document
{
	
	/// <summary>
	/// Тестовый документ 3
	/// </summary>

    [ControlOrderFor(typeof(TestCommonControl), 1)]
    [ControlInitialFor(typeof(TestCommonControl2),ExcludeFromSetup = false)]
    [ControlInitialFor(typeof(TestFreeCommonControl),InitialUNK = "89787",InitialManaged = true)]
	public partial class TestDocument3 :ITestDocument
	{

        [Control(ControlType.Update, Sequence.After, ExecutionOrder = 2)]
        [ControlInitial(InitialSkippable = true ,InitialUNK ="5600" ,ExcludeFromSetup = true)]
        public void TestDocument3Control1(DataContext dataContext)
        {
            Controls.Throw("Сообщение контрола TestDocument3Control1");
        }

        [Control(ControlType.Update, Sequence.After, ExecutionOrder = 3)]
        [ControlInitial(InitialSkippable = true, InitialUNK = "5600", ExcludeFromSetup = true)]
        public void TestDocument3Control2(DataContext dataContext)
        {
            Controls.Throw("Сообщение контрола TestDocument3Control2");
        }

        public void Change(DataContext context)
        {
            ExecuteControl<TestFreeCommonControl>();
        }

		


	}
}