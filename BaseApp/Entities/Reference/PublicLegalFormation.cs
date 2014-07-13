using System;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common;
using Platform.PrimaryEntities.DbEnums;

namespace BaseApp.Reference
{
	public partial class PublicLegalFormation : ReferenceEntity, IPublicLegalFormation
    {
        //При изменении поля «Уровень» на значение, не равное «Субъект РФ», очищать поле «Код субъекта РФ».
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 20)]
        public void CleanSubjectCode (DataContext context)
        {
            var AttMsg = "Уровень ППО не «Субъект РФ» - поле «Код субъекта РФ» будет очищено";

            if (IdBudgetLevel != BudgetLevel.SubjectRF && Subject != String.Empty)
            {
                Subject = String.Empty;
                context.SaveChanges();
                Controls.Throw(string.Format(AttMsg));
            }
        }


	    /// <summary>
        /// Корректность заполнения кода субъекта РФ
	    /// </summary>
	    /// <param name="context"></param>
        [ControlInitial(InitialCaption = "Корректность заполнения кода субъекта РФ",InitialUNK = "502402")]
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 30)]
        public void Control_502402(DataContext context,PublicLegalFormation old)
	    {
	        var oop = old;
	        if (oop != null)
	        {
	            const string msg =
	                "Для публично-правовых образований уровня Субъект РФ необходимо заполнять поле «Код субъекта РФ»";
	            if ((IdBudgetLevel == BudgetLevel.SubjectRF && IdRefStatus == (decimal) RefStatus.New &&
	                 old.IdRefStatus == (decimal) RefStatus.New) ||
	                (IdBudgetLevel == BudgetLevel.SubjectRF && IdRefStatus == (decimal) RefStatus.Work))
	            {
	                if (string.IsNullOrEmpty(Subject))
	                {
	                    Controls.Throw(msg);
	                }
	            }
	        }
	        else
	        {
                const string msg =
                    "Для публично-правовых образований уровня Субъект РФ необходимо заполнять поле «Код субъекта РФ»";
                if (IdBudgetLevel == BudgetLevel.SubjectRF && IdRefStatus == (decimal)RefStatus.Work)
                {
                    if (string.IsNullOrEmpty(Subject))
                    {
                        Controls.Throw(msg);
                    }
                }
	        }
	        //Controls.Throw(msg
	    }

	    //При сохранении элемента сразу создавать элемент в справочнике «Бюджет»: 
        //Год = 2012, Статус = В работе. 
        //Также создавать элемент в справочнике «Версии»: 
        //наименование = Базовая, статус = В работе. 
        [Control(ControlType.Insert, Sequence.After, ExecutionOrder = 10)]
        public void CreateBudget(DataContext context)
        {
            var exists = context.PublicLegalFormation.FirstOrDefault(a => a.Id == Id);
            Budget b = new Budget();
            b.IdPublicLegalFormation = exists.Id;
            b.Year = 2012;
            b.IdRefStatus = (byte)RefStatus.Work;
            context.Budget.Add(b);
            Version v = new Version();
            v.IdPublicLegalFormation = exists.Id;
            v.Caption = "Базовая";
            v.IdRefStatus = (byte)RefStatus.Work;
            context.Version.Add(v);
            context.SaveChanges();
        }
    }
}
